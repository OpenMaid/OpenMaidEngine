"""Unit tests for profile-neutral engine-dump provenance helpers (Frida not required)."""
from __future__ import annotations

import hashlib
import sys
import tempfile
from pathlib import Path

import struct

from dump_engine import (
    PROFILE_LANDMARK_RVAS,
    build_manifest,
    pe_base_relocations,
    read_catalog_identity,
    sha256_file,
    undo_relocations,
)

FAILS: list[str] = []


def check(condition: bool, message: str) -> None:
    if condition:
        print("ok:", message)
    else:
        FAILS.append(message)
        print("FAIL:", message)


def test_hash_and_catalog_identity() -> None:
    with tempfile.TemporaryDirectory() as directory:
        root = Path(directory)
        payload = root / "range.bin"
        payload.write_bytes(b"unpacked-age-image")
        check(
            sha256_file(payload) == hashlib.sha256(payload.read_bytes()).hexdigest(),
            "range hash is lowercase SHA-256 of exact bytes",
        )

        catalog = root / "SYS4INI.BIN"
        revision = b"S4IC433 "
        title = "神採りアルケミーマイスター".encode("cp932") + b"\0"
        catalog.write_bytes(revision + title.ljust(256, b"\0"))
        identity = read_catalog_identity(catalog)
        check(
            identity == {
                "catalog_revision": "S4IC433",
                "title": "神採りアルケミーマイスター",
            },
            "catalog identity is read from the fixed header without loading game content",
        )


def test_manifest_keeps_profile_runtime_and_range_provenance_separate() -> None:
    profile = {
        "profile_id": "kamidori",
        "engine_abi_id": "SYS4433",
        "packed_executable": {"size": 12, "sha256": "a" * 64},
    }
    runtime = {
        "age_path": "S:/Kamidori/age.exe",
        "age_base": "0x400000",
        "age_size": 0x280000,
        "landmark_rva": None,
        "landmark_bytes": None,
        "rx": [{"base": "0x400000", "size": 0x280000}],
        "rw": [],
    }
    ranges = [{
        "base": "0x400000", "tag": "age-module", "file": "range_00400000.bin",
        "declared_size": 0x280000, "captured_bytes": 0x280000, "gaps": [],
        "file_size": 0x280000, "sha256": "b" * 64,
    }]
    manifest = build_manifest(profile, runtime, ranges, "1234", "2026-08-19T12:00:00Z")
    check(
        manifest["schema_version"] == 2
        and manifest["profile"]["profile_id"] == "kamidori"
        and manifest["runtime_module"]["image_base"] == "0x400000"
        and manifest["dumped_ranges"][0]["sha256"] == "b" * 64,
        "manifest carries distinct packed-image, runtime-module, and dumped-range identities",
    )


def test_new_profiles_have_no_borrowed_landmark() -> None:
    check(
        PROFILE_LANDMARK_RVAS.get("himegari") == 0x74F1F
        and "kamidori" not in PROFILE_LANDMARK_RVAS,
        "Kamidori capture does not inherit Himegari's decoder landmark",
    )


def synthetic_pe(image_base: int, relocations: list[int]) -> bytes:
    """A PE32 header with one section at RVA 0x1000 (file 0x400) and, when given, a .reloc block for page 0x1000."""
    pe = bytearray(0x800)
    struct.pack_into("<I", pe, 0x3C, 0x80)
    pe[0x80:0x84] = b"PE\0\0"
    struct.pack_into("<H", pe, 0x86, 2)                  # two sections
    struct.pack_into("<H", pe, 0x94, 0xE0)               # optional header size
    optional = 0x98
    struct.pack_into("<H", pe, optional, 0x10B)
    struct.pack_into("<I", pe, optional + 28, image_base)
    struct.pack_into("<I", pe, optional + 92, 16)
    table = optional + 0xE0
    struct.pack_into("<8sIIII", pe, table, b".text", 0x1000, 0x1000, 0x200, 0x400)
    struct.pack_into("<8sIIII", pe, table + 40, b".reloc", 0x1000, 0x2000, 0x200, 0x600)
    if relocations:
        entries = [0x3000 | rva - 0x1000 for rva in relocations] + [0]      # a pad entry, type 0
        block = 8 + 2 * len(entries)
        struct.pack_into("<II", pe, 0x600, 0x1000, block)
        for index, entry in enumerate(entries):
            struct.pack_into("<H", pe, 0x608 + 2 * index, entry)
        struct.pack_into("<II", pe, optional + 96 + 5 * 8, 0x2000, block)
    return bytes(pe)


def test_a_relocated_module_is_taken_back_to_its_preferred_base() -> None:
    base, sites = pe_base_relocations(synthetic_pe(0x400000, [0x1004, 0x1010]))
    check(base == 0x400000 and sites == [0x1004, 0x1010], "the reader finds the base and the HIGHLOW sites")
    check(pe_base_relocations(synthetic_pe(0x400000, []))[1] == [], "a PE without relocations, as a packed AGE, has none")
    image = bytearray(0x2000)
    struct.pack_into("<I", image, 0x1004, 0x5A1234)      # 0x401234 as the loader relocated it to 0x5a0000
    struct.pack_into("<I", image, 0x1010, 0x6D0000)
    struct.pack_into("<I", image, 0x1008, 0x5A9999)      # not a relocation site: stays
    undone = undo_relocations(image, sites, 0x5A0000 - 0x400000)
    check(undone == 2 and struct.unpack_from("<I", image, 0x1004)[0] == 0x401234
          and struct.unpack_from("<I", image, 0x1010)[0] == 0x530000
          and struct.unpack_from("<I", image, 0x1008)[0] == 0x5A9999,
          "the loader's delta comes off the relocation sites alone")


def main() -> int:
    test_hash_and_catalog_identity()
    test_a_relocated_module_is_taken_back_to_its_preferred_base()
    test_manifest_keeps_profile_runtime_and_range_provenance_separate()
    test_new_profiles_have_no_borrowed_landmark()
    print("FAILURES:", len(FAILS))
    return 1 if FAILS else 0


if __name__ == "__main__":
    raise SystemExit(main())
