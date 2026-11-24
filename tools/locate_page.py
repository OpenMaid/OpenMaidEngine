"""Resolve a runtime ADV page number to authoritative script offsets and disassembly.

Usage:
  py -3.11 -X utf8 tools/locate_page.py SC0000 14
  py -3.11 -X utf8 tools/locate_page.py --game kamidori SC0000 14
  py -3.11 -X utf8 tools/locate_page.py SC0000 14 --map path/to/page-map.jsonl --context 10
  py -3.11 -X utf8 tools/locate_page.py --listing SC0000 [--no-names]
  py -3.11 -X utf8 tools/locate_page.py --digests [--no-names]

The Godot editor runtime writes build/games/<game>/page-map-<SCENE>.jsonl at each wait-for-input.
Page numbers are run-relative conveniences; wait_script + wait_offset is the stable bytecode coordinate.

``OME-Tool locate`` and ``Ome.Cli locate`` print the same report (modding from a release spec §7). ``--no-names``
leaves out the registry's names (globals.json, callscript-names.json), which those commands take from the base mod's
layout and the catalog instead. ``--listing`` prints a whole script's instruction lines, and ``--digests`` one JSON line
per script of the corpus with the SHA-256 of its bytes and of its listing: the engine's workspace test compares them
with the C# listing.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import re
from pathlib import Path

import paths
import sys4load


def load_records(path: Path) -> list[dict]:
    records = []
    with path.open(encoding="utf-8") as f:
        for number, line in enumerate(f, 1):
            line = line.strip()
            if not line:
                continue
            try:
                records.append(json.loads(line))
            except json.JSONDecodeError as exc:
                raise ValueError(f"{path}:{number}: invalid JSON: {exc}") from exc
    return records


def default_map_path(scene: str, context: paths.ToolContext | None = None) -> Path:
    """Where a Godot editor run writes the scene's map: the profile's build directory."""
    return (context or paths.CONTEXT).game_build / f"page-map-{scene.upper().removesuffix('.BIN')}.jsonl"


def find_page(records: list[dict], scene: str, page: int) -> dict | None:
    scene = scene.upper().removesuffix(".BIN")
    return next((r for r in records
                 if str(r.get("root_scene", "")).upper().removesuffix(".BIN") == scene
                 and r.get("page") == page), None)


def context_window(offsets: list[int], target: int, radius: int) -> tuple[int, int]:
    try:
        index = offsets.index(target)
    except ValueError as exc:
        raise ValueError(f"offset 0x{target:x} is not an instruction boundary") from exc
    return max(0, index - radius), min(len(offsets), index + radius + 1)


def load_script(script: str) -> sys4load.Sys4Script:
    key = script.upper()
    if not key.endswith(".BIN"):
        key += ".BIN"
    scripts = paths.scripts()
    if key not in scripts:
        raise ValueError(f"script {key} is not present in the override-aware corpus")
    scr = sys4load.load(scripts[key])
    sys4load.decode_code(scr)
    return scr


def instruction_lines(scr: sys4load.Sys4Script) -> list[tuple[int, str]]:
    """Each instruction's offset and its text, as the listing renders it."""
    line_by_offset = {}
    for line in sys4load.render_listing(scr).splitlines():
        match = re.match(r"\s*0x([0-9a-fA-F]+):\s*(.*)", line)
        if match:
            line_by_offset[int(match.group(1), 16)] = match.group(2).rstrip()
    return [(ins.offset, line_by_offset.get(ins.offset, "?")) for ins in scr.instructions]


def disasm_context(script: str, target: int, radius: int) -> list[tuple[int, str, bool]]:
    lines = instruction_lines(load_script(script))
    begin, end = context_window([offset for offset, _ in lines], target, radius)
    return [(off, text, off == target) for off, text in lines[begin:end]]


def listing_text(lines: list[tuple[int, str]]) -> str:
    return "\n".join(f"0x{offset:05x}: {text}" for offset, text in lines)


def digests() -> None:
    """One JSON line per script of the corpus: its name, the SHA-256 of its bytes and of its listing text."""
    for name, path in sorted(paths.scripts().items()):
        try:
            scr = sys4load.load(path)
        except sys4load.Sys4Error:
            continue                                # SYS4INI.BIN and the other catalogs share the extension
        sys4load.decode_code(scr)
        lines = instruction_lines(scr)
        print(json.dumps({"script": name,
                          "bytes": hashlib.sha256(path.read_bytes()).hexdigest(),
                          "listing": hashlib.sha256(listing_text(lines).encode("utf-8")).hexdigest(),
                          "instructions": len(lines)}, ensure_ascii=False))


def parse_hex(value: object, field: str) -> int:
    if not isinstance(value, str):
        raise ValueError(f"page record has no {field}")
    return int(value, 0)


def report(record: dict, radius: int) -> str:
    scene, page = record["root_scene"], record["page"]
    wait_script = record["wait_script"]
    wait_offset = parse_hex(record.get("wait_offset"), "wait_offset")
    lines = [f"{scene} P{page:03d}",
             f"page start: {record.get('page_start_script') or '?'}@{record.get('page_start_offset') or '?'}",
             f"last text:  {record.get('text_script') or '?'}@{record.get('text_offset') or '?'}",
             f"wait:       {wait_script}@0x{wait_offset:x}"]
    stack = record.get("call_stack") or []
    if stack:
        lines.append("call stack: " + " > ".join(stack))
    if record.get("text"):
        lines.append("text: " + str(record["text"]).replace("\r", " ").replace("\n", " "))
    lines += ["", f"disassembly around {wait_script}@0x{wait_offset:x}:"]
    for offset, instruction, selected in disasm_context(wait_script, wait_offset, radius):
        lines.append(f"{'>>>' if selected else '   '} 0x{offset:05x}: {instruction}")
    return "\n".join(lines)


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("scene", nargs="?", help="root scene, e.g. SC0000")
    parser.add_argument("page", nargs="?", type=int, help="one-based runtime page number")
    parser.add_argument("--map", type=Path, dest="map_path", help="page-map JSONL (default: build/games/PROFILE/page-map-SCENE.jsonl)")
    parser.add_argument("--context", type=int, default=8, help="instructions on either side of the wait (default: 8)")
    parser.add_argument("--no-names", action="store_true",
                        help="leave out the registry's global and call-script names")
    parser.add_argument("--listing", metavar="SCRIPT", help="print every instruction line of SCRIPT")
    parser.add_argument("--digests", action="store_true",
                        help="print the SHA-256 of each script's bytes and listing, one JSON line per script")
    args = parser.parse_args()
    if args.no_names:
        sys4load.GLOBAL_LABELS.clear()
        sys4load.CALLSCRIPT_NAMES.clear()
    if args.digests:
        digests()
        return 0
    if args.listing:
        try:
            print(listing_text(instruction_lines(load_script(args.listing))))
        except (OSError, ValueError) as exc:
            parser.error(str(exc))
        return 0
    if args.scene is None or args.page is None:
        parser.error("give a scene and a page, --listing SCRIPT or --digests")
    scene = args.scene.upper().removesuffix(".BIN")
    map_path = args.map_path or default_map_path(scene)
    if not map_path.is_file():
        parser.error(f"page map not found: {map_path} (run the Godot scene to the desired page first)")
    if args.context < 0:
        parser.error("--context must be non-negative")
    try:
        record = find_page(load_records(map_path), scene, args.page)
        if record is None:
            raise ValueError(f"{scene} page {args.page} is not present in {map_path}")
        print(report(record, args.context))
    except (OSError, ValueError, KeyError) as exc:
        parser.error(str(exc))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
