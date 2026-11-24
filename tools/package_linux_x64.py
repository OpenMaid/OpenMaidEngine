#!/usr/bin/env python3
"""Verify and package the Linux x64 Godot export, and run its package smoke."""
from __future__ import annotations

import argparse
import contextlib
import gzip
import hashlib
import json
import os
import queue
import re
import shlex
import shutil
import signal
import stat
import struct
import subprocess
import tarfile
import tempfile
import threading
import time
from collections.abc import Iterator, Sequence
from dataclasses import dataclass
from pathlib import Path

import package_notices
import publish_engine
import publish_launcher

HERE = Path(__file__).resolve().parent
REPO = HERE.parent
DATA_DIRECTORY = "data_OME_linuxbsd_x86_64"
PACKAGE_NAME = "OME-linux-x64"
RUNTIME = "linux-x64"
# The launcher beside the engine (launcher spec §10, plan §9.3): its program, natives and notices.
LAUNCHER = publish_launcher.LAYOUTS[RUNTIME]
# The mod tool beside them (modding from a release spec §6.4): its program and its runtime's notices.
TOOL_FILES = publish_engine.tool_files(RUNTIME)
TOOL = publish_engine.TARGETS[RUNTIME].tool
# The FFmpeg shim and the libraries it loads (native/ome_movie_ffmpeg/build-linux-x64.sh), with their notices.
FFMPEG_FILES = (
    "libome_movie_ffmpeg.so",
    "libavformat.so.62",
    "libavcodec.so.62",
    "libavutil.so.60",
    "libswscale.so.9",
    "libswresample.so.6",
    "FFmpeg-LICENSE.txt",
    "FFmpeg-SOURCE.txt",
)
# The data folder Godot loads the engine from, exactly: the engine's NativeAOT library with the notices of the .NET
# runtime it holds (tools/publish_engine.py), and the FFmpeg bundle. A .NET runtime file there would send Godot down its
# hostfxr route (docs/platform-portability.md, "Fewer files in a package").
DATA_FILES = (*publish_engine.publish_files(RUNTIME), *FFMPEG_FILES)
REQUIRED_FILES = (
    "OME",
    "OME.pck",
    *(f"{DATA_DIRECTORY}/{name}" for name in DATA_FILES),
    *LAUNCHER.files(),
    *TOOL_FILES,
)
FORBIDDEN_FILES = (
    publish_launcher.LAYOUTS["win-x64"].program,
    *publish_launcher.LAYOUTS["win-x64"].natives,
    publish_engine.TARGETS["win-x64"].tool,
)
# The engine's NativeAOT library: what Godot calls to start it, and the only libraries it may need, those of glibc
# itself, which every system has.
ENGINE_EXPORT = "godotsharp_game_main_init"
GLIBC_LIBRARIES = frozenset({
    "libc.so.6", "libm.so.6", "libdl.so.2", "librt.so.1", "libpthread.so.0", "ld-linux-x86-64.so.2",
})
ELF_MACHINE_X86_64 = 62
# The FFmpeg bundle's pins, among them the package's glibc floor.
FFMPEG_DEPENDENCY = REPO / "native/ome_movie_ffmpeg/dependency-linux-x64.json"
PROJECT_FILES = ("LICENSE", "README.md", "THIRD_PARTY_NOTICES.md")
SMOKE_MARKER = "PACKAGE SMOKE OK: opcodes=548 ffmpeg-abi=3"
SMOKE_TIMEOUT_S = 120.0
# Godot's notices and the license of the font OME.dll embeds.
PINNED_NOTICES = package_notices.pinned_notices() + package_notices.pinned_notices(package_notices.NOTO_NOTICES)


def _resolved_directory(path: Path, label: str) -> Path:
    resolved = path.resolve()
    if not resolved.is_dir():
        raise ValueError(f"{label} directory was not found: {resolved}")
    return resolved


def minimum_glibc() -> str:
    """The package's glibc floor: the FFmpeg bundle's, which its dependency manifest pins. The engine's library must
    not raise it."""
    with FFMPEG_DEPENDENCY.open(encoding="utf-8") as stream:
        return json.load(stream)["minimum_glibc"]


def _version(text: str) -> tuple[int, ...]:
    return tuple(int(part) for part in text.split("."))


def _readelf(readelf: str, path: Path, *arguments: str) -> str:
    process = subprocess.run((readelf, *arguments, "--wide", str(path)), check=False, capture_output=True, text=True,
                             encoding="utf-8", errors="replace")
    if process.returncode != 0:
        raise RuntimeError(f"readelf failed for {path.name}: {process.stderr.strip()}")
    return process.stdout


def _verify_aot_binary(path: Path, readelf: str, label: str, export: str | None = None) -> dict[str, object]:
    """Check a NativeAOT binary, the engine's library or the tool (`label` names it): an x86-64 ELF file that exports
    `export` when given, needs only glibc's libraries, and no glibc newer than the package's floor."""
    with path.open("rb") as stream:
        header = stream.read(20)
    if (len(header) < 20 or header[:4] != b"\x7fELF" or header[4] != 2
            or struct.unpack_from("<H", header, 18)[0] != ELF_MACHINE_X86_64):
        raise ValueError(f"Linux {label} is not an x86-64 ELF file: {path.name}")
    if export is not None:
        symbols = _readelf(readelf, path, "--dyn-syms")
        if not re.search(rf"\bGLOBAL\s+DEFAULT\s+\d+\s+{export}(?:@\S*)?\s*$", symbols, re.MULTILINE):
            raise ValueError(f"Linux {label} does not export {export}: {path.name}; it is not a NativeAOT build")
    dynamic = _readelf(readelf, path, "--dynamic")
    needed = sorted(set(re.findall(r"\(NEEDED\)\s+Shared library: \[([^\]]+)\]", dynamic)))
    foreign = [name for name in needed if name not in GLIBC_LIBRARIES]
    if foreign:
        raise ValueError(f"Linux {label} needs libraries outside glibc: " + ", ".join(foreign))
    versions = sorted(set(re.findall(r"\bGLIBC_(\d+(?:\.\d+)*)", _readelf(readelf, path, "--version-info"))),
                      key=_version)
    if not versions:
        raise ValueError(f"could not determine the glibc the {label} needs: {path.name}")
    floor = minimum_glibc()
    if _version(versions[-1]) > _version(floor):
        raise ValueError(f"Linux {label} needs glibc {versions[-1]}, newer than the package's floor {floor}; "
                         f"link it on glibc {floor}, as the release workflow's manylinux container does")
    return {"needed": needed, "glibc": versions[-1]}


def verify_engine_library(path: Path, readelf: str = "readelf") -> dict[str, object]:
    """Check the engine's NativeAOT library, which exports Godot's entry point (_verify_aot_binary)."""
    return {"export": ENGINE_EXPORT, **_verify_aot_binary(path, readelf, "engine library", ENGINE_EXPORT)}


def verify_tool(path: Path, readelf: str = "readelf") -> dict[str, object]:
    """Check the mod tool (modding from a release spec §6.4), a NativeAOT program (_verify_aot_binary)."""
    return _verify_aot_binary(path, readelf, "tool")


def verify_export(export_directory: Path, launcher_deps: Path | None = None, engine_deps: Path | None = None,
                  readelf: str = "readelf", tool_deps: Path | None = None) -> Path:
    """Check the export's files, the engine's library and the tool, and that THIRD_PARTY_NOTICES.md names every package
    the engine, the launcher and the tool ship (`engine_deps`, `launcher_deps` and `tool_deps`: the dependency lists
    their publishes kept, by default publish_engine's and publish_launcher's)."""
    export_directory = _resolved_directory(export_directory, "Linux export")
    missing = [name for name in REQUIRED_FILES if not (export_directory / name).is_file()]
    if missing:
        raise ValueError("Linux export is incomplete; missing: " + ", ".join(missing))
    data_directory = export_directory / DATA_DIRECTORY
    extra = sorted(path.relative_to(data_directory).as_posix() for path in data_directory.rglob("*")
                   if path.relative_to(data_directory).as_posix() not in DATA_FILES)
    if extra:
        raise ValueError("Linux export's data folder holds files the NativeAOT engine does not ship: "
                         + ", ".join(extra))
    forbidden = [name for name in FORBIDDEN_FILES if (export_directory / name).exists()]
    if forbidden:
        raise ValueError("Linux export contains Windows-only files: " + ", ".join(forbidden))
    symbols = sorted(path.relative_to(export_directory).as_posix() for path in export_directory.rglob("*.pdb"))
    if symbols:
        raise ValueError("Linux export contains debug symbols: " + ", ".join(symbols))
    package_notices.verify_dependency_notices(
        engine_deps or publish_engine.dependency_list(RUNTIME), REPO / "THIRD_PARTY_NOTICES.md"
    )
    package_notices.verify_launcher_notices(
        launcher_deps or publish_launcher.dependency_list(LAUNCHER.runtime), REPO / "THIRD_PARTY_NOTICES.md"
    )
    package_notices.verify_tool_notices(
        tool_deps or publish_engine.tool_dependency_list(RUNTIME), REPO / "THIRD_PARTY_NOTICES.md"
    )
    for executable in (export_directory / "OME", export_directory / LAUNCHER.program, export_directory / TOOL):
        if os.name != "nt" and not os.access(executable, os.X_OK):
            raise ValueError(f"Linux export executable bit is not set: {executable}")
    verify_engine_library(data_directory / publish_engine.TARGETS[RUNTIME].library, readelf)
    verify_tool(export_directory / TOOL, readelf)
    return export_directory


def _git(*arguments: str) -> str:
    result = subprocess.run(
        ("git", *arguments), cwd=REPO, check=True, capture_output=True, text=True
    )
    return result.stdout.strip()


def source_date_epoch() -> int:
    configured = os.environ.get("SOURCE_DATE_EPOCH")
    value = configured or _git("show", "-s", "--format=%ct", "HEAD")
    try:
        epoch = int(value)
    except ValueError as error:
        raise ValueError(f"invalid SOURCE_DATE_EPOCH: {value!r}") from error
    if epoch < 0:
        raise ValueError("SOURCE_DATE_EPOCH must not be negative")
    return epoch


def build_metadata(epoch: int) -> dict[str, object]:
    with (HERE / "godot-linux-x64.json").open(encoding="utf-8") as stream:
        godot = json.load(stream)
    with FFMPEG_DEPENDENCY.open(encoding="utf-8") as stream:
        ffmpeg = json.load(stream)
    dirty = bool(_git("status", "--porcelain", "--untracked-files=normal"))
    return {
        "schema_version": 1,
        "project": "Open Maid Engine",
        "target": "linux-x64",
        "source_commit": _git("rev-parse", "HEAD"),
        "source_dirty": dirty,
        "source_date_epoch": epoch,
        "godot": {
            "version": godot["godot_version"],
            "editor_archive_sha256": godot["editor"]["sha256"],
            "release_template_sha256": godot["templates"]["members"][0]["sha256"],
        },
        "ffmpeg": {
            "provider": ffmpeg["provider"],
            "version": ffmpeg["ffmpeg_version"],
            "archive_sha256": ffmpeg["sha256"],
            "minimum_glibc": ffmpeg["minimum_glibc"],
        },
    }


def _hash(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def _write_checksums(package_root: Path) -> None:
    checksum_path = package_root / "SHA256SUMS"
    files = sorted(
        path for path in package_root.rglob("*")
        if path.is_file() and path != checksum_path
    )
    lines = [f"{_hash(path)}  {path.relative_to(package_root).as_posix()}" for path in files]
    checksum_path.write_text("\n".join(lines) + "\n", encoding="utf-8", newline="\n")


def _tar_info(info: tarfile.TarInfo, epoch: int) -> tarfile.TarInfo:
    info.uid = 0
    info.gid = 0
    info.uname = "root"
    info.gname = "root"
    info.mtime = epoch
    if info.isdir():
        info.mode = 0o755
    elif info.isfile():
        info.mode = 0o755 if info.mode & stat.S_IXUSR else 0o644
    return info


def create_package(
    export_directory: Path,
    output_directory: Path,
    metadata: dict[str, object],
    epoch: int,
    launcher_deps: Path | None = None,
    engine_deps: Path | None = None,
    tool_deps: Path | None = None,
) -> tuple[Path, Path]:
    export_directory = verify_export(export_directory, launcher_deps, engine_deps, tool_deps=tool_deps)
    output_directory = output_directory.resolve()
    expected_parent = (REPO / "build/package").resolve()
    if output_directory != expected_parent:
        raise ValueError(
            f"refusing to replace unexpected package directory: {output_directory}"
        )
    staging_parent = output_directory / "staging"
    package_root = staging_parent / PACKAGE_NAME
    archive_path = output_directory / f"{PACKAGE_NAME}.tar.gz"
    if staging_parent.exists():
        shutil.rmtree(staging_parent)
    staging_parent.mkdir(parents=True)
    shutil.copytree(export_directory, package_root, copy_function=shutil.copy2)
    for name in PROJECT_FILES:
        source = REPO / name
        if not source.is_file():
            raise ValueError(f"package notice file was not found: {source}")
        shutil.copy2(source, package_root / name)
    package_notices.copy_pinned_notices(PINNED_NOTICES, package_root)
    (package_root / "BUILD-INFO.json").write_text(
        json.dumps(metadata, ensure_ascii=False, indent=2, sort_keys=True) + "\n",
        encoding="utf-8",
        newline="\n",
    )
    _write_checksums(package_root)

    output_directory.mkdir(parents=True, exist_ok=True)
    temporary_archive = archive_path.with_suffix(archive_path.suffix + ".part")
    with temporary_archive.open("wb") as raw:
        with gzip.GzipFile(fileobj=raw, mode="wb", filename="", mtime=epoch) as compressed:
            with tarfile.open(fileobj=compressed, mode="w", format=tarfile.PAX_FORMAT) as archive:
                paths = [package_root, *sorted(package_root.rglob("*"))]
                for path in paths:
                    arcname = path.relative_to(staging_parent).as_posix()
                    archive.add(
                        path,
                        arcname=arcname,
                        recursive=False,
                        filter=lambda info, fixed_epoch=epoch: _tar_info(info, fixed_epoch),
                    )
    os.replace(temporary_archive, archive_path)
    return package_root, archive_path


class SmokeError(RuntimeError):
    pass


def run_smoke(command: list[str], marker: str, log: Path, timeout: float = SMOKE_TIMEOUT_S) -> None:
    """Run a package smoke that holds until its standard input closes (`--package-smoke --hold`). Its marker must reach
    the pipe while it still runs: the launcher captures an engine's output live, and an engine that flushes only at
    exit loses its lines when it is killed (launcher plan §4.12: project.godot's flush_stdout_on_print). Then its
    standard input is closed and it must exit 0. Everything it printed goes to `log`."""
    process = subprocess.Popen(command, stdin=subprocess.PIPE, stdout=subprocess.PIPE, stderr=subprocess.STDOUT)
    lines: queue.Queue[str | None] = queue.Queue()

    def read() -> None:
        assert process.stdout is not None
        for raw in process.stdout:
            lines.put(raw.decode("utf-8", errors="replace").replace("\r\n", "\n"))
        lines.put(None)

    reader = threading.Thread(target=read, daemon=True)
    reader.start()
    output: list[str] = []
    try:
        deadline = time.monotonic() + timeout
        while True:
            try:
                line = lines.get(timeout=max(0.0, deadline - time.monotonic()))
            except queue.Empty:
                raise SmokeError(f"no '{marker}' line reached the pipe within {timeout:.0f} s while the engine ran: "
                                 "its output is not flushed") from None
            if line is None:
                raise SmokeError(f"the engine's output ended ({process.wait()}) before '{marker}'")
            output.append(line)
            if marker in line:
                if process.poll() is not None:
                    raise SmokeError(f"'{marker}' arrived only as the engine exited ({process.returncode})")
                break
        assert process.stdin is not None
        process.stdin.close()
        try:
            returncode = process.wait(timeout=timeout)
        except subprocess.TimeoutExpired:
            raise SmokeError(f"the engine did not exit within {timeout:.0f} s of its standard input closing") from None
        if returncode != 0:
            raise SmokeError(f"the engine exited with {returncode}")
    finally:
        if process.poll() is None:
            process.kill()
            process.wait()
        reader.join(timeout)
        # Whatever reached the pipe goes to the log, on every path.
        while not lines.empty():
            if (line := lines.get_nowait()) is not None:
                output.append(line)
        for stream in (process.stdin, process.stdout):
            if stream is not None and not stream.closed:
                stream.close()
        log.write_text("".join(output), encoding="utf-8", newline="\n")


def smoke_command(package_root: Path) -> list[str]:
    return [str(package_root / "OME"), "--headless", "--", "--package-smoke", "--hold"]


# The smoke's launcher steps (launcher plan §9.4): the launcher's command-line check with no display (gate 11), and the
# engine's hand-over to the launcher beside it (launcher spec §5.6).
LAUNCHER_CHECK_KINDS = ("Install", "Fingerprint", "Root", "Packages", "Link", "Lock")
LAUNCHER_CHECK_MARKER = f"LAUNCHER CHECK OK: checks={len(LAUNCHER_CHECK_KINDS)}"
HAND_OVER_MARKER = "HAND-OVER OK"
# The smoke's tool step (modding from a release spec §12, gates 2, 5 and 7; plan §3.4).
TOOL_MARKER = "TOOL OK: gates=2,5,7"
# Every line an accepted smoke log holds; the release publisher requires them all.
SMOKE_MARKERS = (SMOKE_MARKER, LAUNCHER_CHECK_MARKER, TOOL_MARKER, HAND_OVER_MARKER)
SMOKE_PROFILE = "smoke"
SMOKE_GAME = "himegari"
# One line of `OME-Launcher --check`: the status padded to eight, then `Kind: message`.
CHECK_LINE = re.compile(r"^(passed|warning|FAILED|not run)\s+([A-Za-z]+): ")
# What the engine reports when an argument keeps it and names a folder with no game (GameRootSelection).
NO_GAME_ERROR = "does not contain SYS4INI.BIN"


@dataclass(frozen=True)
class SmokeRun:
    """One program a smoke step ran: what it printed (standard error merged), and its exit code, None on a timeout."""

    command: tuple[str, ...]
    returncode: int | None
    output: str

    def exit(self) -> str:
        return "past its timeout" if self.returncode is None else f"with {self.returncode}"


def _cp932_field(text: str, size: int) -> bytes:
    raw = text.encode("cp932")
    if len(raw) > size:
        raise ValueError(f"{text!r} does not fit in {size} bytes")
    return raw + bytes(size - len(raw))


def scripts_the_modules_run(game: str) -> list[str]:
    """The scripts the game's OMS modules run (`run NAME` in its base mod's oms/*.oms), as catalog names, in order. The
    smoke's install lists them, so the toolchain the engine ships builds against it, as on a real install (modding
    from a release spec §12, gate 2)."""
    found: set[str] = set()
    for path in sorted((REPO / "mods" / f"{game}-base" / "oms").glob("*.oms")):
        found.update(re.findall(r"\brun ([A-Z][A-Z0-9_]*)\b", path.read_text(encoding="utf-8")))
    return [f"{name}.BIN" for name in sorted(found)]


def write_smoke_home(home: Path, game: str = SMOKE_GAME, extra_scripts: Sequence[str] = ()) -> Path:
    """An OME home for `OME-Launcher --check smoke --home HOME`: `profiles/smoke/` in native mode on
    `installs/<game>/`. Its SYS4INI.BIN is the smallest complete catalog:
    - the header of the game's first catalog identity (`engine/Ome.Engine/Profiles/<game>.json`);
    - a directory of one archive, one placeholder entry and an empty entry for each script the game's OMS modules
      run (scripts_the_modules_run) and each of `extra_scripts`, then an empty trailer as the engine reads one (no
      metadata, a zero-length settings block, nothing after it), packed as LZSS literals.
    A header alone is refused, since its expanded size 0 fails (launcher plan §9). Returns the home."""
    identity = json.loads((REPO / f"engine/Ome.Engine/Profiles/{game}.json").read_text(encoding="utf-8"))
    identity = identity["catalogIdentities"][0]
    names = ["@", *scripts_the_modules_run(game), *extra_scripts]
    directory = (struct.pack("<I", 1) + _cp932_field("DATA.ALF", 256) + struct.pack("<I", len(names))
                 + b"".join(_cp932_field(name, 64) + bytes(16) for name in names) + struct.pack("<II", 0, 0))
    packed = bytearray()
    for offset in range(0, len(directory), 8):
        chunk = directory[offset:offset + 8]
        packed.append((1 << len(chunk)) - 1)  # every flag bit set: eight literal bytes follow
        packed += chunk
    header = bytearray(0x138)
    header[0:8] = identity["catalogRevision"].ljust(8).encode("ascii")
    header[8:0x12C] = _cp932_field(identity["title"], 0x12C - 8)
    struct.pack_into("<I", header, 0x12C, len(directory))
    struct.pack_into("<I", header, 0x134, len(packed))
    install = home / "installs" / game
    install.mkdir(parents=True)
    (install / "SYS4INI.BIN").write_bytes(bytes(header) + bytes(packed))
    profile = home / "profiles" / SMOKE_PROFILE
    profile.mkdir(parents=True)
    (profile / "profile.toml").write_text(
        f'ome_profile = 1\ngame = "{game}"\nmode = "native"\n\n[install]\nroot = "../../installs/{game}"\n',
        encoding="utf-8", newline="\n")
    return home


def smoke_environment(scratch: Path) -> dict[str, str]:
    """The environment of the launcher steps: the job's, with no display, and HOME and the XDG folders inside
    `scratch`. So a step neither reads nor writes this machine's own state: the launcher's machine folder and the
    engine's logs live in Godot's user folder, under XDG_DATA_HOME."""
    user = scratch / "user"
    environment = {key: value for key, value in os.environ.items() if key not in ("DISPLAY", "WAYLAND_DISPLAY")}
    environment.update(HOME=str(user), XDG_DATA_HOME=str(user / ".local/share"),
                       XDG_CONFIG_HOME=str(user / ".config"), XDG_CACHE_HOME=str(user / ".cache"))
    for name in ("HOME", "XDG_DATA_HOME", "XDG_CONFIG_HOME", "XDG_CACHE_HOME"):
        Path(environment[name]).mkdir(parents=True, exist_ok=True)
    return environment


def _run_step(command: list[str], cwd: Path, environment: dict[str, str], timeout: float) -> SmokeRun:
    try:
        completed = subprocess.run(command, cwd=cwd, env=environment, stdin=subprocess.DEVNULL,
                                   stdout=subprocess.PIPE, stderr=subprocess.STDOUT, timeout=timeout, check=False)
        returncode, output = completed.returncode, completed.stdout
    except subprocess.TimeoutExpired as error:
        returncode, output = None, error.output or b""
    return SmokeRun(tuple(command), returncode,
                    output.decode("utf-8", errors="replace").replace("\r\n", "\n"))


def _append(log: Path, *parts: str) -> None:
    with log.open("a", encoding="utf-8", newline="\n") as stream:
        for part in parts:
            stream.write(part if part.endswith("\n") or not part else part + "\n")


def launcher_check_problems(run: SmokeRun) -> list[str]:
    """What is wrong with a `--check` run: it must exit 0 with each of the six checks passed, once."""
    problems = []
    if run.returncode != 0:
        problems.append(f"it exited {run.exit()}")
    checks = sorted((match.group(2), match.group(1)) for line in run.output.splitlines()
                    if (match := CHECK_LINE.match(line)))
    if checks != sorted((kind, "passed") for kind in LAUNCHER_CHECK_KINDS):
        found = ", ".join(f"{kind} {status}" for kind, status in checks) or "none"
        problems.append(f"its checks are not {', '.join(LAUNCHER_CHECK_KINDS)}, each passed; it printed: {found}")
    return problems


def run_launcher_check(launcher: list[str], log: Path, timeout: float = SMOKE_TIMEOUT_S) -> None:
    """The smoke's second step (gate 11): `launcher --check smoke --home HOME` on write_smoke_home's home, with no
    display. Appends what it printed to `log`, then LAUNCHER_CHECK_MARKER."""
    with tempfile.TemporaryDirectory(prefix="ome-launcher-check-") as temporary:
        scratch = Path(temporary)
        home = write_smoke_home(scratch / "home")
        run = _run_step([*launcher, "--check", SMOKE_PROFILE, "--home", str(home)], scratch,
                        smoke_environment(scratch), timeout)
    _append(log, f"== the launcher's check, with no display: {' '.join(run.command)}", run.output)
    if problems := launcher_check_problems(run):
        _append(log, "the launcher's check failed: " + "; ".join(problems))
        raise SmokeError("the launcher's check failed: " + "; ".join(problems))
    _append(log, LAUNCHER_CHECK_MARKER)


def write_launcher_stub(path: Path, record: Path) -> None:
    """The hand-over's stand-in for the launcher: it appends a line to `record` each time it runs, its argument count
    and its arguments, tab-separated."""
    path.write_text(
        "#!/bin/sh\n# The package smoke's stand-in for OME-Launcher (launcher plan §9.4): it records how it started.\n"
        f"printf '%s\\t%s\\n' \"$#\" \"$*\" >> {shlex.quote(str(record))}\n", encoding="utf-8", newline="\n")
    path.chmod(0o755)


def _link_or_copy(source: str, destination: str) -> None:
    try:
        os.link(source, destination)
    except OSError:
        shutil.copy2(source, destination)


def hand_over_problems(with_argument: SmokeRun, without: SmokeRun, started_with_argument: list[str],
                       started_without: list[str]) -> list[str]:
    """What is wrong with the hand-over: an argument keeps the engine, which reports its own error and exits 2; with
    none the engine starts the launcher once, with no arguments, and exits 0. The `started_*` lists are the stub's
    records each run added."""
    problems = []
    if with_argument.returncode != 2 or NO_GAME_ERROR not in with_argument.output:
        problems.append(f"with an argument the engine must report its own error and exit with 2; it exited "
                        f"{with_argument.exit()}")
    if started_with_argument:
        problems.append("with an argument the engine started the launcher: an argument must keep the engine")
    if without.returncode != 0:
        problems.append(f"with no argument the engine exited {without.exit()}")
    if not started_without:
        problems.append("with no argument the engine did not start the launcher beside it")
    elif len(started_without) > 1:
        problems.append(f"with no argument the engine started the launcher {len(started_without)} times")
    elif started_without[0].split("\t", 1)[0] != "0":
        problems.append(f"the launcher started with arguments: {started_without[0].split(chr(9), 1)[-1]}")
    return problems


def _launcher_runs(record: Path, deadline: float) -> list[str]:
    """The stub's records, waiting until `deadline` for a first one."""
    while not record.is_file() and time.monotonic() < deadline:
        time.sleep(0.05)
    return record.read_text(encoding="utf-8").splitlines() if record.is_file() else []


@contextlib.contextmanager
def released_copy(package_root: Path) -> Iterator[Path]:
    """The package as a player has it: copied to `OME package`, a folder whose name has a space, in a temporary
    folder outside the repository. A program inside a checkout links with the repository's toolchain
    (`ModLinkBase.ForGame`), so the launcher's steps run on this copy, not where the package was staged. Its files are
    hard links where the filesystem allows; the copy is removed afterwards."""
    package_root = _resolved_directory(package_root, "package")
    with tempfile.TemporaryDirectory(prefix="ome smoke ") as temporary:
        scratch = Path(temporary).resolve()
        if scratch == REPO or REPO in scratch.parents:
            raise SmokeError(f"the temporary folder {scratch} is inside the repository: set TMPDIR to a folder "
                             "outside it")
        copy = scratch / "OME package"
        shutil.copytree(package_root, copy, symlinks=True, copy_function=_link_or_copy)
        yield copy


# The tool step's script, mods and expectations. The script is compiled into the smoke install, which lists it, and the
# fragment patches it at its first instruction, naming one of the game's globals (its base mod's oms/globals.json).
SMOKE_SCRIPT = "SMOKE.BIN"
SMOKE_GLOBAL = "acting_entity_index"
SMOKE_MOD = "smoke.fragment"
BROKEN_MOD = "smoke.broken"
# What the engine prints when it has linked the fragment under --quit-after-link (godot/Main.Mods.cs, Main.cs).
LINKED_FRAGMENT = f"[mods] load order: {SMOKE_MOD} 1.0.0"
QUIT_AFTER_LINK = "[mods] --quit-after-link: the stack linked; quitting"
# What the engine prints when start passes it a game option beside a profile file (RunResolver).
REFUSED_GAME_ROOT = "--game-root is refused with a profile file"
# How long the tool has to end after SIGINT: its grace period (StartCommand.Grace, 10 s) and a margin.
INTERRUPT_LIMIT_S = 30.0
# How long a process of the start's group may take to end once the tool has.
SURVIVOR_LIMIT_S = 5.0


def smoke_script_source(game: str = SMOKE_GAME) -> str:
    return f'oms 1\nscript "{SMOKE_SCRIPT.removesuffix(".BIN")}"\nuse {game}.globals\n\n    {SMOKE_GLOBAL} = 0x0\n    exit\n'


def write_smoke_mods(mods: Path, header: str, game: str = SMOKE_GAME) -> tuple[Path, Path]:
    """Two mod folders in `mods`: SMOKE_MOD, a fragment with the patch header `header` (what `oms-patch
    --fingerprint` printed) that `use`s the game's globals and its ADV module, the base mod's toolchain, and assigns
    SMOKE_GLOBAL, and BROKEN_MOD, which requires
    a mod nobody has, so the Link check fails. Returns both."""
    manifest = ('omp = 1\nid = "{id}"\nname = "{name}"\nversion = "1.0.0"\n\n[game]\nprofile = "' + game
                + '"\ntarget = "native"\n')
    fragment = mods / SMOKE_MOD
    (fragment / "patches").mkdir(parents=True)
    (fragment / "manifest.toml").write_text(manifest.format(id=SMOKE_MOD, name="Smoke: a fragment"),
                                            encoding="utf-8", newline="\n")
    (fragment / "patches/smoke.oms").write_text(
        f"oms 1\n{header}\nuse {game}.globals\nuse {game}.adv\n\nbefore site 0x0 {{\n    {SMOKE_GLOBAL} = 0x1\n}}\n",
        encoding="utf-8", newline="\n")
    broken = mods / BROKEN_MOD
    broken.mkdir(parents=True)
    (broken / "manifest.toml").write_text(
        manifest.format(id=BROKEN_MOD, name="Smoke: a missing requirement")
        + '\n[[requires]]\nid = "smoke.missing"\n', encoding="utf-8", newline="\n")
    return fragment, broken


def tool_version_problems(run: SmokeRun, commit: str) -> list[str]:
    """Gate 5: `OME-Tool --version` exits 0 with one line naming the package's commit."""
    lines = run.output.strip().splitlines()
    pattern = re.compile(rf"^OME-Tool \S+ \(commit {re.escape(commit[:12])}\)$")
    if run.returncode != 0 or len(lines) != 1 or not pattern.match(lines[0]):
        return [f"--version exited {run.exit()} and printed {run.output.strip()!r}, not one line naming commit "
                f"{commit[:12]}"]
    return []


def tool_refusal_problems(run: SmokeRun) -> list[str]:
    """Gate 5: a command the tool lacks exits 2 with the tool's own list."""
    if run.returncode != 2 or "OME-Tool has no command 'trace'; its commands: " not in run.output:
        return [f"a command the tool lacks exited {run.exit()}, not 2 with the tool's commands"]
    return []


def tool_check_problems(run: SmokeRun, game: str = SMOKE_GAME) -> list[str]:
    """Gate 2: `omp check` links the fragment on the loose script, with the base mod's toolchain and its patch
    points."""
    problems = []
    if run.returncode != 0:
        problems.append(f"omp check of the fragment exited {run.exit()}")
    toolchain = next((line for line in run.output.splitlines() if line.startswith(f"toolchain: {game}-base ")), None)
    if toolchain is None:
        problems.append(f"omp check named no toolchain of {game}-base")
    elif any(part not in toolchain for part in ("oms/globals.json", "oms/adv.oms", "layout/points.json")):
        problems.append(f"the toolchain line lacks the globals, the ADV module or the patch points: {toolchain}")
    if f"{SMOKE_SCRIPT}: compiled patches/smoke.oms" not in run.output:
        problems.append(f"omp check did not compile the fragment on {SMOKE_SCRIPT}")
    return problems


def _last_line(text: str) -> str:
    lines = text.rstrip("\n").splitlines()
    return lines[-1] if lines else ""


def start_problems(run: SmokeRun, engine_log: str, code: int, line: str, label: str) -> list[str]:
    """Gate 7: `start` relays the engine's lines and its exit code `code`, and the profile's engine.log keeps them:
    `line`, one the engine prints, is in both, and the log's last line is the launcher's."""
    problems = []
    if run.returncode != code:
        problems.append(f"{label}: start exited {run.exit()}, not the engine's {code}")
    if f"OME-Tool: starting {SMOKE_PROFILE} with " not in run.output:
        problems.append(f"{label}: start did not say it started the engine")
    if line not in run.output:
        problems.append(f"{label}: start did not relay the engine's line {line!r}")
    if line not in engine_log:
        problems.append(f"{label}: engine.log lacks the engine's line {line!r}")
    if _last_line(engine_log) != f"[launcher] the engine exited with {code}":
        problems.append(f"{label}: engine.log ends {_last_line(engine_log)!r}")
    return problems


def refused_start_problems(run: SmokeRun, log_before: str, log_after: str) -> list[str]:
    """Gate 7: a `--mod` that fails the Link check stops the start (exit 1) before the engine runs, so the engine
    prints nothing and engine.log is not touched."""
    problems = []
    if run.returncode != 1:
        problems.append(f"a start with a broken mod exited {run.exit()}, not 1")
    if f"{SMOKE_PROFILE} is not started: a check failed: Link" not in run.output:
        problems.append("a start with a broken mod did not name the failed Link check")
    if SMOKE_MARKER in run.output or log_after != log_before:
        problems.append("a start with a broken mod ran the engine")
    return problems


def interrupt_problems(run: SmokeRun, engine_log: str, survivors: Sequence[int]) -> list[str]:
    """Ctrl+C's path in the tool (plan §3.3), after SIGINT to the tool alone: it says it closes the engine, ends within
    INTERRUPT_LIMIT_S, engine.log says the engine was stopped, and no process of the start is left."""
    problems = group_interrupt_problems(run, survivors)
    if "OME-Tool: closing the engine" not in run.output:
        problems.append("after SIGINT the tool did not say it closes the engine")
    if not _last_line(engine_log).startswith("[launcher] the engine was stopped (Ctrl+C"):
        problems.append(f"after SIGINT engine.log ends {_last_line(engine_log)!r}")
    return problems


def group_interrupt_problems(run: SmokeRun, survivors: Sequence[int]) -> list[str]:
    """Ctrl+C in a terminal, which sends SIGINT to the tool and the engine alike: whichever ends first, the tool ends
    within INTERRUPT_LIMIT_S and leaves no process of the start running. Which lines it prints depends on which process
    the signal reaches first, so they are not checked."""
    problems = []
    if run.returncode is None:
        problems.append(f"after SIGINT the tool did not end within {INTERRUPT_LIMIT_S:.0f} s")
    if survivors:
        problems.append(f"after SIGINT the start left processes running: {', '.join(map(str, survivors))}")
    return problems


def _group_members(group: int) -> list[int]:
    """The processes of process group `group` that still run (a zombie has ended, whether or not it is reaped)."""
    members = []
    for entry in Path("/proc").iterdir():
        if not entry.name.isdigit():
            continue
        try:
            fields = (entry / "stat").read_text(encoding="ascii", errors="replace").rsplit(")", 1)[1].split()
        except (OSError, IndexError):
            continue
        # After the command's closing parenthesis: state, ppid, pgrp.
        if len(fields) > 2 and fields[2] == str(group) and fields[0] != "Z":
            members.append(int(entry.name))
    return sorted(members)


def _read(path: Path) -> str:
    return path.read_text(encoding="utf-8", errors="replace") if path.is_file() else ""


def _interrupt_start(command: list[str], cwd: Path, environment: dict[str, str], group: bool,
                     timeout: float = SMOKE_TIMEOUT_S) -> tuple[SmokeRun, float | None, list[int]]:
    """Run `start` as a terminal runs it, in a process group of its own that its engine joins, and once the engine has
    printed SMOKE_MARKER send SIGINT to the tool alone, or with `group` to the whole group, as Ctrl+C in a terminal
    does. The engine holds (`--hold`) until its standard input, the tool's, closes, which happens only afterwards.
    Returns the run (returncode None when the tool outlived INTERRUPT_LIMIT_S), the seconds from the signal to its exit,
    and the processes of the group still running SURVIVOR_LIMIT_S after it, which are then killed."""
    process = subprocess.Popen(command, cwd=cwd, env=environment, stdin=subprocess.PIPE, stdout=subprocess.PIPE,
                               stderr=subprocess.STDOUT, start_new_session=True)
    lines: list[str] = []
    printed = threading.Event()

    def read() -> None:
        for raw in process.stdout:
            text = raw.decode("utf-8", errors="replace").rstrip("\r\n")
            lines.append(text)
            if SMOKE_MARKER in text:
                printed.set()

    reader = threading.Thread(target=read, daemon=True)
    reader.start()
    elapsed: float | None = None
    survivors: list[int] = []
    try:
        if printed.wait(timeout):
            sent = time.monotonic()
            if group:
                os.killpg(process.pid, signal.SIGINT)
            else:
                process.send_signal(signal.SIGINT)
            try:
                process.wait(INTERRUPT_LIMIT_S)
                elapsed = time.monotonic() - sent
            except subprocess.TimeoutExpired:
                pass
        else:
            lines.append(f"(the engine did not print {SMOKE_MARKER} within {timeout:.0f} s)")
    finally:
        if process.poll() is None:
            os.killpg(process.pid, signal.SIGKILL)
            process.wait()
        # A process the tool left behind has SURVIVOR_LIMIT_S to end, then is killed.
        deadline = time.monotonic() + SURVIVOR_LIMIT_S
        while (survivors := _group_members(process.pid)) and time.monotonic() < deadline:
            time.sleep(0.1)
        if survivors:
            with contextlib.suppress(ProcessLookupError):
                os.killpg(process.pid, signal.SIGKILL)
        process.stdin.close()
        reader.join(5)
    return (SmokeRun(tuple(command), process.returncode if elapsed is not None else None, "\n".join(lines) + "\n"),
            elapsed, survivors)


def run_tool(folder: Path, log: Path, timeout: float = SMOKE_TIMEOUT_S) -> None:
    """The smoke's tool step (modding from a release spec §12; plan §3.4), on a released_copy, in a scratch OME home
    from write_smoke_home whose install also lists SMOKE_SCRIPT:
    - gate 5: `--version` names the package's commit (BUILD-INFO.json), and a command the tool lacks exits 2;
    - gate 2: `oms-compile` puts SMOKE_SCRIPT in the install, `oms-patch --fingerprint` gives its header, and
      `omp check` links a fragment on it that uses the game's globals and its ADV module, the base mod's toolchain;
    - gates 2 and 7: `start --mod FRAGMENT --engine-arg --headless -- --quit-after-link` has the engine link the
      fragment itself and quit, and relays its lines and its exit 0, which engine.log keeps; with `-- --game-root DIR`
      the engine refuses and start relays its exit 2; a `--mod` that fails the Link check stops the start before the
      engine runs;
    - Ctrl+C (plan §3.3): SIGINT to the tool alone takes its path (it asks the engine to close, and ends it after its
      grace: the smoke runs before the control channel listens), and SIGINT to the process group, as a terminal sends
      it, ends the start within INTERRUPT_LIMIT_S; neither leaves an engine running.
    Appends every run to `log`, then TOOL_MARKER."""
    folder = _resolved_directory(folder, "package copy")
    tool = str(folder / TOOL)
    commit = json.loads((folder / "BUILD-INFO.json").read_text(encoding="utf-8"))["source_commit"]
    problems: list[str] = []
    with tempfile.TemporaryDirectory(prefix="ome tool ") as temporary:
        scratch = Path(temporary)
        environment = smoke_environment(scratch)
        home = write_smoke_home(scratch / "home", extra_scripts=(SMOKE_SCRIPT,))
        install = home / "installs" / SMOKE_GAME
        engine_log = home / "profiles" / SMOKE_PROFILE / "diagnostics" / "engine.log"
        game = ("--game", SMOKE_GAME, "--game-root", str(install))
        profile = ("--profile", SMOKE_PROFILE, "--home", str(home))

        def step(label: str, *arguments: str) -> SmokeRun:
            run = _run_step([tool, *arguments], scratch, environment, timeout)
            _append(log, f"== the tool, {label}: {' '.join(run.command)} (exited {run.exit()})", run.output)
            return run

        problems += tool_version_problems(step("its version", "--version"), commit)
        problems += tool_refusal_problems(step("a command it lacks", "trace"))

        source = scratch / "smoke.oms"
        source.write_text(smoke_script_source(), encoding="utf-8", newline="\n")
        compiled = step("compiling a script into the install", "oms-compile", "--source", str(source),
                        "--output", str(install / SMOKE_SCRIPT), *game)
        if compiled.returncode != 0:
            problems.append(f"oms-compile exited {compiled.exit()}")
        fingerprint = step("the script's patch header", "oms-patch", "--fingerprint", SMOKE_SCRIPT, *game)
        header = next((line for line in fingerprint.output.splitlines()
                       if line.startswith(f'patch "{SMOKE_SCRIPT}" fingerprint "sha256:')), None)
        if header is None:
            problems.append(f"oms-patch --fingerprint printed no header for {SMOKE_SCRIPT}")
        else:
            fragment, broken = write_smoke_mods(scratch / "mods", header)
            problems += tool_check_problems(step("checking the fragment", "omp", "check", "--mod", str(fragment),
                                                 *profile))
            ran = step("start, the engine linking the fragment", "start", *profile, "--mod", str(fragment),
                       "--engine-arg", "--headless", "--", "--quit-after-link")
            problems += start_problems(ran, _read(engine_log), 0, LINKED_FRAGMENT, "the engine's link")
            if QUIT_AFTER_LINK not in ran.output:
                problems.append("the engine's link: the engine did not quit after linking")
            refused = step("start, the engine refusing", "start", *profile, "--engine-arg", "--headless", "--",
                           "--game-root", str(install))
            problems += start_problems(refused, _read(engine_log), 2, REFUSED_GAME_ROOT, "the engine's refusal")
            before = _read(engine_log)
            stopped = step("start, a mod that fails the Link check", "start", *profile, "--mod", str(broken),
                           "--engine-arg", "--headless", "--", "--package-smoke")
            problems += refused_start_problems(stopped, before, _read(engine_log))
            command = [tool, "start", *profile, "--engine-arg", "--headless", "--", "--package-smoke", "--hold"]
            for group in (False, True):
                interrupted, elapsed, survivors = _interrupt_start(command, scratch, environment, group, timeout)
                label = "SIGINT to the process group" if group else "SIGINT to the tool"
                _append(log, f"== the tool, start and {label}: {' '.join(command)} (exited {interrupted.exit()}"
                        + (f", {elapsed:.1f} s after SIGINT" if elapsed is not None else "") + ")",
                        interrupted.output, "engine.log ends: " + _last_line(_read(engine_log)),
                        "left running: " + (", ".join(map(str, survivors)) or "nothing"))
                problems += (group_interrupt_problems(interrupted, survivors) if group
                             else interrupt_problems(interrupted, _read(engine_log), survivors))
    if problems:
        _append(log, "the tool failed: " + "; ".join(problems))
        raise SmokeError("the tool failed: " + "; ".join(problems))
    _append(log, TOOL_MARKER)


def run_hand_over(folder: Path, log: Path, timeout: float = SMOKE_TIMEOUT_S) -> None:
    """The smoke's third step (launcher spec §5.6), on a released_copy, whose launcher it replaces with
    write_launcher_stub's stub; both runs start from a folder with no game.
    - `OME --headless -- --game-root <that folder>` must keep the engine: it reports its error and exits 2.
    - `OME --headless` must start the launcher once, with no arguments, and exit 0.
    Appends both runs and the stub's records to `log`, then HAND_OVER_MARKER."""
    folder = _resolved_directory(folder, "package copy")
    launcher = folder / LAUNCHER.program
    launcher.unlink(missing_ok=True)  # a hard link to the package's own launcher, never written through
    with tempfile.TemporaryDirectory(prefix="ome hand-over ") as temporary:
        scratch = Path(temporary)
        record = scratch / "launcher-runs.txt"
        write_launcher_stub(launcher, record)
        elsewhere = scratch / "no game here"
        elsewhere.mkdir()
        # Godot can change the directory before the engine starts; the engine reads the caller's from PWD.
        environment = dict(smoke_environment(scratch), PWD=str(elsewhere))
        engine = str(folder / "OME")
        with_argument = _run_step([engine, "--headless", "--", "--game-root", str(elsewhere)], elsewhere,
                                  environment, timeout)
        # The engine starts the launcher and exits without waiting, so the run with none waits for its record.
        started_with_argument = record.read_text(encoding="utf-8").splitlines() if record.is_file() else []
        without = _run_step([engine, "--headless"], elsewhere, environment, timeout)
        started_without = _launcher_runs(record, time.monotonic() + timeout)[len(started_with_argument):]
    _append(log, f"== the hand-over, with an argument: {' '.join(with_argument.command)} (exited "
            f"{with_argument.exit()})", with_argument.output,
            f"== the hand-over, with none: {' '.join(without.command)} (exited {without.exit()})", without.output,
            "the launcher's stand-in started: " + "; ".join(
                f"{len(runs)} times {label}" + "".join(f" (argc {run})" for run in runs)
                for label, runs in (("with an argument", started_with_argument), ("with none", started_without))))
    if problems := hand_over_problems(with_argument, without, started_with_argument, started_without):
        _append(log, "the hand-over failed: " + "; ".join(problems))
        raise SmokeError("the hand-over failed: " + "; ".join(problems))
    _append(log, HAND_OVER_MARKER)


def main() -> int:
    parser = argparse.ArgumentParser()
    subparsers = parser.add_subparsers(dest="command", required=True)
    verify = subparsers.add_parser("verify")
    verify.add_argument("export_directory", type=Path)
    verify.add_argument("--launcher-deps", type=Path, help="the launcher's kept dependency list")
    verify.add_argument("--engine-deps", type=Path, help="the engine's kept dependency list")
    verify.add_argument("--tool-deps", type=Path, help="the tool's kept dependency list")
    package = subparsers.add_parser("package")
    package.add_argument("export_directory", type=Path)
    package.add_argument("--launcher-deps", type=Path, help="the launcher's kept dependency list")
    package.add_argument("--engine-deps", type=Path, help="the engine's kept dependency list")
    package.add_argument("--tool-deps", type=Path, help="the tool's kept dependency list")
    package.add_argument("--output-directory", type=Path, default=REPO / "build/package")
    smoke = subparsers.add_parser("smoke")
    smoke.add_argument("package_root", type=Path)
    smoke.add_argument("--log", type=Path, required=True)
    args = parser.parse_args()

    if args.command == "verify":
        print(verify_export(args.export_directory, args.launcher_deps, args.engine_deps, tool_deps=args.tool_deps))
        return 0
    if args.command == "smoke":
        # Four steps, each adding to the log (launcher plan §9.4, modding from a release plan §3.4): the engine, the
        # launcher's check, the tool, the hand-over, which replaces the copy's launcher and so comes last.
        package_root = args.package_root.resolve()  # the launcher's steps run from scratch folders
        try:
            run_smoke(smoke_command(package_root), SMOKE_MARKER, args.log)
            # The launcher's and the tool's steps run on the package as a player has it: outside the repository.
            with released_copy(package_root) as copy:
                run_launcher_check([str(copy / LAUNCHER.program)], args.log)
                run_tool(copy, args.log)
                run_hand_over(copy, args.log)
        except SmokeError as error:
            print(f"package smoke: {error}", flush=True)
            return 1
        return 0
    epoch = source_date_epoch()
    package_root, archive_path = create_package(
        args.export_directory, args.output_directory, build_metadata(epoch), epoch, args.launcher_deps,
        args.engine_deps, args.tool_deps,
    )
    print(package_root)
    print(archive_path)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
