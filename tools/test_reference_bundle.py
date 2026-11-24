#!/usr/bin/env python3
"""Tests for tools/reference_bundle.py (game data export spec §8, plan §4.4). The notes, the index and the archives
are synthetic; one test builds the bundle from this repository, as the core gate does."""
from __future__ import annotations

import json
import tempfile
import unittest
import zipfile
from pathlib import Path

import paths
import reference_bundle as rb

COMMIT = "0123456789abcdef0123456789abcdef01234567"


def index_of(**scripts: dict) -> dict:
    return {"format": 1, "game": "test", "scripts": scripts}


INDEX = index_of(**{
    "ITINIT.BIN": {"id": 1, "globals": {"item_price": [0, 4, 0]}},
    "$1$ITINIT.BIN": {"id": 2, "globals": {}},
    "$2$ORPHAN.BIN": {"id": 3, "globals": {}},
    "ADDEXP.BIN": {"id": 4, "globals": {"current_unit_id": [3, 1, 0], "unit_levels": [0, 2, 0],
                                        "unit_bonus": [0, 0, 1]}},
})


class NoteFormTests(unittest.TestCase):
    def test_a_note_reads_with_its_arguments(self) -> None:
        notes, problems = rb.parse_notes(
            '[[script]]\nname = "ADDEXP"\nsummary = " Awards experience. "\nsource = "a doc"\n'
            '[script.arguments]\ncurrent_unit_id = "the unit"\n', "notes")
        self.assertEqual([], problems)
        self.assertEqual([rb.Note("ADDEXP", "Awards experience.", "a doc", {"current_unit_id": "the unit"})], notes)

    def test_refuses_a_malformed_note(self) -> None:
        cases = (
            ('[[script]]\nname = "ADDEXP.BIN"\nsummary = "s"\nsource = "d"\n', "without .BIN"),
            ('[[script]]\nname = "ADDEXP"\nsummary = ""\nsource = "d"\n', "`summary` must be text"),
            ('[[script]]\nname = "ADDEXP"\nsummary = "s"\n', "`source` must be text"),
            ('[[script]]\nname = "ADDEXP"\nsummary = "s"\nsource = "d"\nwhat = 1\n', "unknown key 'what'"),
            ('[[script]]\nname = "ADDEXP"\nsummary = "s"\nsource = "d"\narguments = { x = 1 }\n',
             "`arguments` must map"),
            ('[[script]]\nname = "A"\nsummary = "s"\nsource = "d"\n[[script]]\nname = "a"\nsummary = "s"\n'
             'source = "d"\n', "a second note"),
            ('scripts = 1\n', "unknown top-level key 'scripts'"),
            ('[[script]\n', "notes:"),
        )
        for text, message in cases:
            with self.subTest(message):
                _, problems = rb.parse_notes(text, "notes")
                self.assertTrue(any(message in problem for problem in problems), problems)


class NoteIndexTests(unittest.TestCase):
    def test_a_note_names_an_indexed_script_and_arguments_it_reads(self) -> None:
        notes = [
            rb.Note("addexp", "s", "d", {"current_unit_id": "the unit"}),      # names match without case
            rb.Note("ADDEXP", "s", "d", {"unit_bonus": "a list it reads an entry of"}),
            rb.Note("MISSING", "s", "d"),
            rb.Note("ADDEXP", "s", "d", {"unit_levels": "written, never read"}),
            rb.Note("ITINIT", "s", "d", {"unknown_global": "x"}),
        ]
        problems = rb.note_problems(notes, INDEX, "notes")
        self.assertEqual([
            "notes: MISSING: the committed script index has no such script",
            "notes: ADDEXP: argument unit_levels is never read or indexed by the script",
            "notes: ITINIT: argument unknown_global is not a named global the script uses",
        ], problems)

    def test_the_merge_adds_each_note_and_each_layers_base(self) -> None:
        merged = rb.merge_scripts("test", INDEX, [
            rb.Note("ITINIT", "The items.", "a doc"),
            rb.Note("ADDEXP", "Awards experience.", "a point", {"current_unit_id": "the unit"}),
        ])
        self.assertEqual((rb.SCRIPTS_FORMAT, "test", 1), (merged["format"], merged["game"], merged["index_format"]))
        scripts = merged["scripts"]
        self.assertEqual(["$1$ITINIT.BIN", "$2$ORPHAN.BIN", "ADDEXP.BIN", "ITINIT.BIN"], list(scripts))
        self.assertEqual("ITINIT.BIN", scripts["$1$ITINIT.BIN"]["layer_of"])
        self.assertNotIn("layer_of", scripts["$2$ORPHAN.BIN"])              # its base is not in the index
        self.assertNotIn("note", scripts["$1$ITINIT.BIN"])                  # the site reads the base's
        self.assertEqual({"summary": "The items.", "source": "a doc"}, scripts["ITINIT.BIN"]["note"])
        self.assertEqual({"current_unit_id": "the unit"}, scripts["ADDEXP.BIN"]["note"]["arguments"])
        self.assertEqual(INDEX["scripts"]["ADDEXP.BIN"]["globals"], scripts["ADDEXP.BIN"]["globals"])


class ArchiveTests(unittest.TestCase):
    FILES = {"opcodes.json": b"{}\n", "docs/modding/README.md": b"# Modding\n", "games/a/globals.json": b"{}\n",
             "games/b/scripts.json": b"{}\n"}

    def build(self, directory: Path, version: str = "v1.2.3", dirty: bool = False) -> Path:
        return rb.write_archive(self.FILES, rb.manifest_for(self.FILES, version, COMMIT, dirty), directory)

    def test_round_trip_and_the_same_bytes_each_time(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            first = self.build(Path(temporary) / "one")
            second = self.build(Path(temporary) / "two")
            self.assertEqual("OME-reference-v1.2.3.zip", first.name)
            self.assertEqual(first.read_bytes(), second.read_bytes())
            manifest = rb.verify_archive(first, version="v1.2.3", commit=COMMIT, require_clean=True)
            self.assertEqual(["a", "b"], manifest["games"])
            self.assertEqual({"version": "v1.2.3", "commit": COMMIT, "dirty": False}, manifest["ome"])
            self.assertEqual(rb.digest(b"{}\n"), manifest["files"]["opcodes.json"])
            with zipfile.ZipFile(first) as archive:
                self.assertEqual(rb.MANIFEST, archive.infolist()[0].filename)
                self.assertEqual({rb.ENTRY_TIME}, {info.date_time for info in archive.infolist()})

    def test_refuses_another_version_commit_or_a_dirty_checkout(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            clean = self.build(Path(temporary) / "clean")
            with self.assertRaisesRegex(rb.BundleError, "is version v1.2.3, not v1.2.4"):
                rb.verify_archive(clean, version="v1.2.4")
            with self.assertRaisesRegex(rb.BundleError, "not " + "f" * 40):
                rb.verify_archive(clean, commit="f" * 40)
            dirty = self.build(Path(temporary) / "dirty", dirty=True)
            rb.verify_archive(dirty)
            with self.assertRaisesRegex(rb.BundleError, "a checkout with changes"):
                rb.verify_archive(dirty, require_clean=True)
            renamed = clean.rename(clean.with_name("OME-reference-v9.zip"))
            with self.assertRaisesRegex(rb.BundleError, "is not named for its version v1.2.3"):
                rb.verify_archive(renamed)

    def test_refuses_a_file_that_differs_from_its_hash_or_is_not_listed(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            archive_path = self.build(Path(temporary))
            with zipfile.ZipFile(archive_path) as archive:
                entries = {info.filename: archive.read(info) for info in archive.infolist()}
            for changed, message in (
                    ({**entries, "opcodes.json": b"[]\n"}, "opcodes.json does not match its hash"),
                    ({**entries, "extra.txt": b"x"}, r"unlisted=\['extra.txt'\]"),
                    ({key: value for key, value in entries.items() if key != "opcodes.json"},
                     r"missing=\['opcodes.json'\]"),
                    ({key: value for key, value in entries.items() if key != rb.MANIFEST}, "has no bundle.json"),
                    ({**entries, "../escape": b"x"}, "not plain relative files"),
                    ({**entries, rb.MANIFEST: b"{"}, "not UTF-8 JSON"),
                    ({**entries, rb.MANIFEST: json.dumps({"format": 2}).encode()}, "is not format 1")):
                with self.subTest(message):
                    with zipfile.ZipFile(archive_path, "w") as archive:
                        for name, data in changed.items():
                            archive.writestr(name, data)
                    with self.assertRaisesRegex(rb.BundleError, message):
                        rb.verify_archive(archive_path)
            archive_path.write_bytes(b"not a zip")
            with self.assertRaisesRegex(rb.BundleError, "is not a zip"):
                rb.verify_archive(archive_path)

    def test_a_version_names_a_file(self) -> None:
        self.assertEqual("OME-reference-v0.9.0-16-g5eef8a4a-dirty.zip", rb.archive_name("v0.9.0-16-g5eef8a4a-dirty"))
        for version in ("", "../v1", "v1/2", "v 1", "-v1"):
            with self.subTest(version), self.assertRaises(rb.BundleError):
                rb.archive_name(version)


class RepositoryBundleTests(unittest.TestCase):
    def test_the_repository_builds_a_whole_bundle(self) -> None:
        files = rb.collect()
        games = json.loads(files["games.json"])["games"]
        self.assertEqual(sorted(rb.profile_manifests()), [game["id"] for game in games])
        self.assertIn("docs/modding/README.md", files)
        self.assertIn("opcodes.json", files)
        for game in games:
            prefix = f"games/{game['id']}/"
            with self.subTest(game["id"]):
                for name in ("globals.json", "layout/globals.json", "layout/points.json",
                             "shapes/tables.schema.json"):
                    self.assertIn(prefix + name, files)
                self.assertTrue(any(name.startswith(prefix + "events/") for name in files))
                self.assertEqual(rb.index_path(game["id"]).is_file(), game["script_index"])
                # The launcher's cover, by the publisher's address and its hash, which the site checks its copy by.
                cover = rb.profile_manifests()[game["id"]].get("launcher", {}).get("cover")
                self.assertEqual(cover, game["cover"])
                if cover is not None:
                    self.assertEqual({"url", "sha256"}, set(cover))
                self.assertEqual(game["script_index"], prefix + "scripts.json" in files)
                context = paths.ToolContext.resolve(game["id"], environ={})
                self.assertEqual(
                    {"globals": context.global_reference_doc.relative_to(paths.REPO).as_posix(),
                     "patch_points": context.patch_point_reference_doc.relative_to(paths.REPO).as_posix()},
                    game["references"])
                for doc in game["references"].values():
                    self.assertTrue((paths.REPO / doc).is_file(), doc)
                globals_json = json.loads(files[prefix + "globals.json"])
                registry = paths.ToolContext.resolve(game["id"], environ={}).globals_toml
                self.assertEqual(f"vm-map/{registry.name}", globals_json["meta"]["generated_from"])
                self.assertTrue(globals_json["tables"])
        for game in (game for game in games if game["script_index"]):
            index, notes, problems = rb.load_game_notes(game["id"])
            self.assertEqual([], problems)
            scripts = json.loads(files[f"games/{game['id']}/scripts.json"])["scripts"]
            self.assertEqual(set(index["scripts"]), set(scripts))
            self.assertEqual(len(notes), sum("note" in entry for entry in scripts.values()))


if __name__ == "__main__":
    unittest.main()
