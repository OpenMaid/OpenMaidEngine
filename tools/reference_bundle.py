#!/usr/bin/env python3
"""Build OME's reference bundle (game data export spec §8, plan §4.4): `OME-reference-<version>.zip`, what the project
site's reference section reads, built from the repository alone.

  --output DIR    build the zip into DIR (its name carries the version)
  --version V     the OME version it names (default: `git describe --tags --always --dirty`; a release passes its tag)
  --check         check the script notes against the committed script indexes and build the bundle in memory (every
                  input must read and lint); writes nothing. A core gate.
  --verify ZIP    check a built zip: its bundle.json and every file's hash

The zip holds, at its root:
  bundle.json                        format, OME version, commit and dirty flag, the games, every other file's hash
  games.json                         each game: its titles, engine build, support levels, store site, cover, base mod,
                                     and the paths of its generated references, which the guides link
  opcodes.json                       vm-map/opcodes.toml as tools/opcodes_build.py emits it
  docs/modding/**                    the modding guides, as git tracks them
  games/<game>/globals.json          the registry's curated globals, tables, references and scopes (no auto map)
  games/<game>/layout/globals.json   the base mod's layout of named cells
  games/<game>/layout/points.json    the base mod's copy of the patch points
  games/<game>/events/*.toml         the base mod's events
  games/<game>/shapes/tables.schema.json   the shape a data file of the game's record tables is checked against
  games/<game>/scripts.json          the committed script index (docs/script-index-<game>.json) merged with the
                                     hand-written notes (vm-map/scripts-<game>.toml), for a game that has an index

The globals the auto map infers from the game files are not in it: they have no names, and CI has no game files."""
from __future__ import annotations

import argparse
import contextlib
import hashlib
import io
import json
import re
import subprocess
import sys
import tomllib
import zipfile
from dataclasses import dataclass, field
from pathlib import Path

import paths

FORMAT_VERSION = 1
SCRIPTS_FORMAT = 1
GAMES_FORMAT = 1
MANIFEST = "bundle.json"
ARCHIVE_PREFIX = "OME-reference-"
VERSION_PATTERN = re.compile(r"[0-9A-Za-z][0-9A-Za-z.+_-]*\Z")
COMMIT_PATTERN = re.compile(r"[0-9a-f]{40}\Z")
DIGEST_PATTERN = re.compile(r"sha256:[0-9a-f]{64}\Z")
LAYER_PATTERN = re.compile(r"\$(\d+)\$(.+)\Z")
NOTE_KEYS = frozenset({"name", "summary", "source", "arguments"})
# Every entry carries this time, so one commit's bundle holds the same entries wherever it is built.
ENTRY_TIME = (1980, 1, 1, 0, 0, 0)
MAX_MANIFEST_SIZE = 4 * 1024 * 1024


class BundleError(Exception):
    """A bundle that cannot be built, or a zip that is not one."""


def archive_name(version: str) -> str:
    if not VERSION_PATTERN.fullmatch(version):
        raise BundleError(f"the version must be letters, digits and . + _ -, not {version!r}")
    return f"{ARCHIVE_PREFIX}{version}.zip"


def digest(data: bytes) -> str:
    return "sha256:" + hashlib.sha256(data).hexdigest()


def _json_bytes(document: object) -> bytes:
    return (json.dumps(document, ensure_ascii=False, indent=2) + "\n").encode("utf-8")


# ---------------------------------------------------------------------------------------------- script notes

@dataclass(frozen=True)
class Note:
    name: str
    summary: str
    source: str
    arguments: dict[str, str] = field(default_factory=dict)


def notes_path(game: str) -> Path:
    return paths.VM_MAP / f"scripts-{game}.toml"


def index_path(game: str) -> Path:
    return paths.REPO / "docs" / f"script-index-{game}.json"


def parse_notes(text: str, label: str) -> tuple[list[Note], list[str]]:
    """The notes of a vm-map/scripts-<game>.toml, and the problems with its form."""
    try:
        document = tomllib.loads(text)
    except tomllib.TOMLDecodeError as error:
        return [], [f"{label}: {error}"]
    problems = [f"{label}: unknown top-level key '{key}'" for key in document if key != "script"]
    entries = document.get("script", [])
    if not isinstance(entries, list):
        return [], problems + [f"{label}: `script` must be an array of tables ([[script]])"]
    notes, seen = [], set()
    for position, entry in enumerate(entries, 1):
        where = f"{label}: script {position}"
        if not isinstance(entry, dict):
            problems.append(f"{where} is not a table")
            continue
        problems += [f"{where}: unknown key '{key}'" for key in entry if key not in NOTE_KEYS]
        name = entry.get("name")
        if not isinstance(name, str) or not name or name.upper().endswith(".BIN"):
            problems.append(f"{where}: `name` must name a script without .BIN")
            continue
        where = f"{label}: {name}"
        if name.upper() in seen:
            problems.append(f"{where}: a second note")
        seen.add(name.upper())
        texts = {}
        for key in ("summary", "source"):
            value = entry.get(key)
            if not isinstance(value, str) or not value.strip():
                problems.append(f"{where}: `{key}` must be text")
            texts[key] = value.strip() if isinstance(value, str) else ""
        arguments = entry.get("arguments", {})
        if not isinstance(arguments, dict) or not all(
                isinstance(value, str) and value.strip() for value in arguments.values()):
            problems.append(f"{where}: `arguments` must map each global's name to what it means")
            arguments = {}
        notes.append(Note(name, texts["summary"], texts["source"],
                          {key: value.strip() for key, value in arguments.items()}))
    return notes, problems


def _keys_by_name(scripts: dict) -> dict[str, str]:
    """An index's script keys by their upper-case name without .BIN."""
    return {key.upper().removesuffix(".BIN"): key for key in scripts}


def note_problems(notes: list[Note], index: dict, label: str) -> list[str]:
    """A note must name a script the index has, and each of its arguments a named global the script reads, itself or
    through an index (a list it reads an entry of)."""
    scripts = index.get("scripts", {})
    keys = _keys_by_name(scripts)
    problems = []
    for note in notes:
        key = keys.get(note.name.upper())
        if key is None:
            problems.append(f"{label}: {note.name}: the committed script index has no such script")
            continue
        uses = scripts[key].get("globals", {})
        for name in note.arguments:
            counts = uses.get(name)
            if not counts:
                problems.append(f"{label}: {note.name}: argument {name} is not a named global the script uses")
            elif not counts[0] and not counts[2]:
                problems.append(f"{label}: {note.name}: argument {name} is never read or indexed by the script")
    return problems


def layer_of(key: str, keys: dict[str, str]) -> str | None:
    """The base script a layer's script ($n$NAME) writes over, when the index has it."""
    match = LAYER_PATTERN.fullmatch(key)
    return keys.get(match.group(2).upper().removesuffix(".BIN")) if match else None


def merge_scripts(game: str, index: dict, notes: list[Note]) -> dict:
    """The bundle's games/<game>/scripts.json: each indexed script with its `layer_of` and `note` where it has one."""
    scripts = index["scripts"]
    keys = _keys_by_name(scripts)
    by_key = {keys[note.name.upper()]: note for note in notes if note.name.upper() in keys}
    merged = {}
    for key in sorted(scripts):
        entry = dict(scripts[key])
        base = layer_of(key, keys)
        if base is not None:
            entry["layer_of"] = base
        note = by_key.get(key)
        if note is not None:
            entry["note"] = {"summary": note.summary, "source": note.source}
            if note.arguments:
                entry["note"]["arguments"] = dict(note.arguments)
        merged[key] = entry
    return {
        "generated": (f"by tools/reference_bundle.py from docs/script-index-{game}.json and "
                      f"vm-map/scripts-{game}.toml; do not edit (game data export spec §8.2)"),
        "format": SCRIPTS_FORMAT,
        "game": game,
        "index_format": index.get("format"),
        "scripts": merged,
    }


def load_game_notes(game: str) -> tuple[dict | None, list[Note], list[str]]:
    """A game's committed index (None when it has none), its notes, and their problems."""
    index_file, notes_file = index_path(game), notes_path(game)
    index = json.loads(index_file.read_text(encoding="utf-8")) if index_file.is_file() else None
    if not notes_file.is_file():
        return index, [], []
    label = notes_file.relative_to(paths.REPO).as_posix()
    notes, problems = parse_notes(notes_file.read_text(encoding="utf-8"), label)
    if index is None:
        return None, notes, problems + [f"{label}: the game has no committed script index "
                                        f"(docs/script-index-{game}.json) to name its scripts"]
    return index, notes, problems + note_problems(notes, index, label)


# ---------------------------------------------------------------------------------------------- the bundle

def profile_manifests() -> dict[str, dict]:
    manifests = {}
    for path in sorted(paths.PROFILE_MANIFEST_DIR.glob("*.json")):
        manifest = json.loads(path.read_text(encoding="utf-8"))
        manifests[manifest["id"]] = manifest
    return manifests


def bundled_manifests() -> dict[str, dict]:
    """The games the bundle carries: those at modding level M3, which have the base mod and registry its files come
    from. A game still being added has neither yet; the generated games page in docs/modding/ lists it."""
    return {game: manifest for game, manifest in profile_manifests().items()
            if manifest["support"]["modding"] == "M3"}


def _quiet(build, label: str):
    """Run a builder that prints its lint; show what it printed only when it fails."""
    printed = io.StringIO()
    with contextlib.redirect_stdout(printed):
        result = build()
    if result is None:
        raise BundleError(f"{label} does not lint:\n{printed.getvalue().rstrip()}")
    return result


def _opcodes_json() -> bytes:
    import opcodes_build
    import opcodes_model

    model = opcodes_model.load(opcodes_build.TOML_DEFAULT)
    errors, _ = opcodes_model.lint(model)
    errors += opcodes_build.profile_contract_errors(model)
    if errors:
        raise BundleError("vm-map/opcodes.toml does not lint:\n" + "\n".join(errors))
    return opcodes_build.emit_json(model).encode("utf-8")


def _git(*arguments: str) -> str:
    try:
        process = subprocess.run(["git", "-C", str(paths.REPO), *arguments], capture_output=True, text=True,
                                 encoding="utf-8", check=False)
    except OSError as error:
        raise BundleError(f"git is needed for the bundle: {error}") from error
    if process.returncode != 0:
        raise BundleError(f"git {' '.join(arguments)} failed: {process.stderr.strip()}")
    return process.stdout


def _modding_docs() -> dict[str, bytes]:
    tracked = [name for name in _git("ls-files", "-z", "--", "docs/modding").split("\0") if name]
    if not tracked:
        raise BundleError("git tracks no file under docs/modding")
    return {name: (paths.REPO / name).read_bytes() for name in sorted(tracked)}


def _game_files(game: str, manifest: dict) -> tuple[dict[str, bytes], dict]:
    """A game's files in the bundle, and its entry in games.json."""
    import globals_build

    prefix = f"games/{game}/"
    registry = paths.ToolContext.resolve(game, environ={}).globals_toml
    files = {prefix + "globals.json": _quiet(lambda: globals_build.curated_json(registry),
                                             registry.relative_to(paths.REPO).as_posix()).encode("utf-8")}
    base_mod = paths.REPO / "mods" / f"{game}-base"
    for relative in ("layout/globals.json", "layout/points.json", "shapes/tables.schema.json"):
        source = base_mod / relative
        if not source.is_file():
            raise BundleError(f"the base mod has no {source.relative_to(paths.REPO).as_posix()}")
        files[prefix + relative] = source.read_bytes()
    for event in sorted((base_mod / "events").glob("*.toml")):
        files[prefix + "events/" + event.name] = event.read_bytes()
    index, notes, problems = load_game_notes(game)
    if problems:
        raise BundleError("\n".join(problems))
    if index is not None:
        files[prefix + "scripts.json"] = _json_bytes(merge_scripts(game, index, notes))
    base = tomllib.loads((base_mod / "manifest.toml").read_text(encoding="utf-8"))
    context = paths.ToolContext.resolve(game, environ={})
    references = {}
    for kind, doc in (("globals", context.global_reference_doc), ("patch_points", context.patch_point_reference_doc)):
        if not doc.is_file():
            raise BundleError(f"{game} has no generated reference {doc.relative_to(paths.REPO).as_posix()}")
        references[kind] = doc.relative_to(paths.REPO).as_posix()
    entry = {
        "id": game,
        "title": manifest["displayTitle"],
        "titles": [identity["title"] for identity in manifest.get("catalogIdentities", [])],
        "engine_build": manifest["engineBuild"],
        "script_revisions": list(manifest.get("scriptRevisions", [])),
        "support": dict(manifest["support"]),
        "site": manifest.get("launcher", {}).get("site"),
        # The cover the launcher shows (launcher spec §8.7), the publisher's address and the picture's SHA-256, which the
        # project site's list of databases shows from a copy it checks against the hash.
        "cover": manifest.get("launcher", {}).get("cover"),
        "base_mod": {"id": base["id"], "version": base["version"]},
        "script_index": index is not None,
        "references": references,
    }
    return files, entry


def collect() -> dict[str, bytes]:
    """Every file of the bundle but bundle.json, by its path in the zip."""
    files = {"opcodes.json": _opcodes_json(), **_modding_docs()}
    games = []
    for game, manifest in bundled_manifests().items():
        game_files, entry = _game_files(game, manifest)
        files.update(game_files)
        games.append(entry)
    files["games.json"] = _json_bytes({"format": GAMES_FORMAT, "games": games})
    return files


def manifest_for(files: dict[str, bytes], version: str, commit: str, dirty: bool) -> dict:
    games = sorted({name.split("/")[1] for name in files if name.startswith("games/")})
    return {
        "generated": "by tools/reference_bundle.py from the repository (game data export spec §8)",
        "format": FORMAT_VERSION,
        "ome": {"version": version, "commit": commit, "dirty": dirty},
        "games": games,
        "files": {name: digest(files[name]) for name in sorted(files)},
    }


def write_archive(files: dict[str, bytes], manifest: dict, output: Path) -> Path:
    """The zip at output/OME-reference-<version>.zip: bundle.json first, then every file by name."""
    output.mkdir(parents=True, exist_ok=True)
    target = output / archive_name(manifest["ome"]["version"])
    entries = {MANIFEST: _json_bytes(manifest), **{name: files[name] for name in sorted(files)}}
    with zipfile.ZipFile(target, "w") as archive:
        for name, data in entries.items():
            info = zipfile.ZipInfo(name, ENTRY_TIME)
            info.compress_type = zipfile.ZIP_DEFLATED
            info.create_system = 3
            info.external_attr = 0o100644 << 16
            archive.writestr(info, data)
    return target


def verify_archive(archive_path: Path, *, version: str | None = None, commit: str | None = None,
                   require_clean: bool = False) -> dict:
    """bundle.json, read back: its format and fields, a file in the zip for each of its hashes and none besides, and
    (when given) the version and commit it must name. Returns the manifest."""
    try:
        with zipfile.ZipFile(archive_path) as archive:
            infos = archive.infolist()
            names = [info.filename for info in infos]
            if len(set(names)) != len(names):
                raise BundleError("the zip holds an entry twice")
            unsafe = [name for name in names if name.startswith("/") or "\\" in name or ".." in name.split("/")
                      or name.endswith("/")]
            if unsafe:
                raise BundleError(f"the zip holds entries that are not plain relative files: {unsafe[:5]}")
            if MANIFEST not in names:
                raise BundleError(f"the zip has no {MANIFEST}")
            if archive.getinfo(MANIFEST).file_size > MAX_MANIFEST_SIZE:
                raise BundleError(f"{MANIFEST} is too large")
            try:
                manifest = json.loads(archive.read(MANIFEST).decode("utf-8"))
            except (UnicodeDecodeError, json.JSONDecodeError) as error:
                raise BundleError(f"{MANIFEST} is not UTF-8 JSON: {error}") from error
            if not isinstance(manifest, dict) or manifest.get("format") != FORMAT_VERSION:
                raise BundleError(f"{MANIFEST} is not format {FORMAT_VERSION}")
            ome = manifest.get("ome")
            if (not isinstance(ome, dict) or not isinstance(ome.get("version"), str)
                    or not VERSION_PATTERN.fullmatch(ome["version"])
                    or not isinstance(ome.get("commit"), str) or not COMMIT_PATTERN.fullmatch(ome["commit"])
                    or not isinstance(ome.get("dirty"), bool)):
                raise BundleError(f"{MANIFEST} has no valid ome version, commit and dirty flag")
            hashes = manifest.get("files")
            if not isinstance(hashes, dict) or not all(
                    isinstance(value, str) and DIGEST_PATTERN.fullmatch(value) for value in hashes.values()):
                raise BundleError(f"{MANIFEST} has no valid file hashes")
            listed, present = set(hashes), set(names) - {MANIFEST}
            if listed != present:
                raise BundleError(f"{MANIFEST} and the zip differ: missing={sorted(listed - present)[:5]}, "
                                  f"unlisted={sorted(present - listed)[:5]}")
            for name in sorted(listed):
                if digest(archive.read(name)) != hashes[name]:
                    raise BundleError(f"{name} does not match its hash in {MANIFEST}")
    except zipfile.BadZipFile as error:
        raise BundleError(f"{archive_path.name} is not a zip: {error}") from error
    if archive_path.name != archive_name(ome["version"]):
        raise BundleError(f"{archive_path.name} is not named for its version {ome['version']}")
    if version is not None and ome["version"] != version:
        raise BundleError(f"the bundle is version {ome['version']}, not {version}")
    if commit is not None and ome["commit"] != commit:
        raise BundleError(f"the bundle was built from {ome['commit']}, not {commit}")
    if require_clean and ome["dirty"]:
        raise BundleError("the bundle was built from a checkout with changes")
    return manifest


def check() -> int:
    problems = []
    for game in profile_manifests():
        problems += load_game_notes(game)[2]
    for problem in problems:
        print("error:", problem)
    if problems:
        return 1
    try:
        files = collect()
    except BundleError as error:
        print("error:", error)
        return 1
    notes = sum(len(load_game_notes(game)[1]) for game in profile_manifests())
    print(f"check: the reference bundle builds ({len(files)} files; {notes} script notes name indexed scripts)")
    return 0


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__.split("\n\n")[0])
    action = parser.add_mutually_exclusive_group(required=True)
    action.add_argument("--output", type=Path, help="build the zip into this folder")
    action.add_argument("--check", action="store_true", help="check the notes and build the bundle in memory")
    action.add_argument("--verify", type=Path, help="check a built zip")
    parser.add_argument("--version", help="the OME version the bundle names (default: git describe)")
    args = parser.parse_args(argv)
    try:
        if args.check:
            return check()
        if args.verify:
            manifest = verify_archive(args.verify)
            print(f"verify: {args.verify.name} is version {manifest['ome']['version']} of "
                  f"{manifest['ome']['commit'][:12]}, {len(manifest['files'])} files, games {manifest['games']}")
            return 0
        version = args.version or _git("describe", "--tags", "--always", "--dirty").strip()
        archive_name(version)
        commit = _git("rev-parse", "HEAD").strip()
        dirty = bool(_git("status", "--porcelain", "--untracked-files=no").strip())
        files = collect()
        target = write_archive(files, manifest_for(files, version, commit, dirty), args.output)
        verify_archive(target, version=version, commit=commit)
        shown = target.relative_to(paths.REPO).as_posix() if target.is_relative_to(paths.REPO) else str(target)
        print(f"build: {shown} ({len(files) + 1} files{', from a checkout with changes' if dirty else ''})")
        return 0
    except BundleError as error:
        print("error:", error, file=sys.stderr)
        return 1


if __name__ == "__main__":
    sys.exit(main())
