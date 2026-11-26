#!/usr/bin/env python3
"""Generate each game's base mod layout (Lua scripting spec §9.1): mods/<game>-base/layout/, and the compiler's view of
the same registry, mods/<game>-base/oms/globals.json (modding from a release spec §5.1).

A base mod is the per-game package that names the game's cells for Lua mods and for OMS. Its generated files come
from the repository alone, so no game content is read:

  vm-map/globals[-<game>].toml      every named global: its bank, address, shape, columns and aliases; the
                                    record tables ([[table]]), with what builds each; and the scopes that key
                                    some of them ([[scope]])
  vm-map/patch-points-<game>.toml   the named patch points, copied so an exported build carries them
  mods/<game>-base/manifest.toml    the base mod's version; its major version is the version of its API
  engine/Ome.Engine/Profiles/<game>.json   each bank's saved prefix (persistence.bankDimensions), which says which
                                    fields of a record table a save holds

For each profile with a base mod (a hand-written mods/<game>-base/manifest.toml) it writes:

  layout/globals.json   the layout the engine builds a mod's accessors from (game.g, game.table)
  layout/points.json    the patch points, in the form of build/games/<game>/patch-points.json
  oms/globals.json      the module <game>.globals that a fragment or script `use`s (GameModule.ParseGlobals): each
                        named cell with its old names, its type in the form the compiler reads (`int[1000x100]` as
                        `int[1000][100]`, a record table's name as a dimension as its record count) and its columns,
                        and each record table with its old names and its fields' old names. Beside it, the base mod's
                        hand-kept modules (`oms/*.oms`, such as adv.oms) make up the game's toolchain
  layout/exports.txt    the names the base mod exports: its API, checked in, so a review sees a name change: the
                        globals, then the record tables (`table NAME`) and their fields (`field TABLE.FIELD`), each
                        old name as `OLD = NAME`, then the scopes (`scope NAME`) and their tables' short names
                        (`scope NAME.SHORT`), then the names its OMS modules declare (`module MODULE.NAME`; modding
                        from a release spec §5.2): what a source that uses the module may name, as the compiler
                        imports them
  lua/generated/tables.lua   the record map (data patches spec §6.3): each record table's record count and what
                        builds it, and for each field its global, kind, row width, named columns and whether a save
                        holds it, for the base mod's tables.lua; the old names of tables and fields; and each scope's
                        ids and its tables by short name (spec §6.2)
  shapes/tables.schema.json   the shape of the kind <game>.tables (data patches spec §7, plan §6.2), from the same
                        record map: the link checks each data file of the kind against it before any code runs, and
                        an editor reads it as JSON Schema; a state field, a scoped table's own name and a named
                        column's number are marked as not offered, with why

The last two belong to the data kind: they are written only when the manifest's [[accepts]] names the shape, since
the package loader refuses a shape no [[accepts]] names. A base mod that does not accept the kind yet (a new game
before its tables' handlers) has neither. A base mod whose game has no registry yet (a new game at M1, which carries
its toolchain before its data step) gets a layout and an oms/globals.json that name nothing.

The rename rule (spec §9.1): a name the checked-in exports.txt lists stays exported, as a global's name or as one of
its `aliases`, until the base mod's major version is raised. The layout and oms/globals.json name the same globals
and tables from the same registry, with the same old names, so the rule covers both. A rename that does neither is refused. A record table's
name and its fields' names follow the same rule, kept as a table's `aliases` or in its `[table.field_aliases]`
(data patches plan §9.1). A scope and its tables' short names have no old-name form: one that leaves needs a raised
major version, and so does a name an OMS module declares (oms/*.oms, hand-kept: its constants, enums and their
members, templates, cells, tables, and subs with their labels).

  --build   write the layouts; refuses a change the rename rule forbids
  --check   exit nonzero if a layout is stale or the rename rule is broken (the core validation gate)
"""
from __future__ import annotations

import argparse
import json
import os
import re
import sys
import tomllib
from pathlib import Path

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import paths  # noqa: E402
import globals_build  # noqa: E402
import patch_points_build  # noqa: E402

MODS = paths.REPO / "mods"
LAYOUT_FORMAT = 1
OMS_GLOBALS_FORMAT = 1
IDENTIFIER = re.compile(r"[a-z_][a-z0-9_]*")
COLUMN = re.compile(r"[A-Za-z_][A-Za-z0-9_]*")   # a column is reached as a Lua name; registries use capitals there
SHAPE = re.compile(r"(int|string|float)((?:\[[^\]]*\])*)")
BANKS = {"int": "G", "float": "GF", "string": "GS"}
KINDS = {"G": "integer", "GF": "float", "GS": "string"}
PREFIX_KEYS = {"G": "integerGlobals", "GF": "floatGlobals", "GS": "stringGlobals"}
RECORD_MAP_FORMAT = 1
LUA_NAME = re.compile(r"[A-Za-z_][A-Za-z0-9_]*")
LUA_KEYWORDS = {"and", "break", "do", "else", "elseif", "end", "false", "for", "function", "goto", "if", "in",
                "local", "nil", "not", "or", "repeat", "return", "then", "true", "until", "while"}
VERSION = re.compile(r"(\d+)\.(\d+)\.(\d+)")
OMS_NAME = r"[^\W\d]\w*"
OMS_MODULE = re.compile(r"module\s+(" + OMS_NAME + r"(?:\." + OMS_NAME + r")*)\s*$")
# A declaration at the top of a module (OmsParser's statement keywords): its name, and whether it opens a block.
OMS_DECLARATION = re.compile(r"(?:@guard\s+)?(sub|coroutine)\s+(" + OMS_NAME + r")"
                             r"|(?:@dialogue\s+)?(inline)\s+(" + OMS_NAME + r")"
                             r"|(enum|const|table)\s+(" + OMS_NAME + r")"
                             r"|(global)\s+\w+\s+(" + OMS_NAME + r")")
OMS_LABEL = re.compile(r"\s*(" + OMS_NAME + r"):\s*$")
OMS_MEMBER = re.compile(r"(" + OMS_NAME + r")\s*=")


def base_mod_id(profile_id: str) -> str:
    return f"{profile_id}-base"


def base_mod_dir(profile_id: str, mods: Path = MODS) -> Path:
    return mods / base_mod_id(profile_id)


def globals_source(profile_id: str) -> Path:
    return paths.VM_MAP / ("globals.toml" if profile_id == "himegari" else f"globals-{profile_id}.toml")


def points_source(profile_id: str) -> Path:
    return paths.VM_MAP / f"patch-points-{profile_id}.toml"


def profile_manifest(profile_id: str) -> Path:
    return paths.PROFILE_MANIFEST_DIR / f"{profile_id}.json"


def dimensions(type_: str) -> tuple[str, list[int | None]] | None:
    """A registry type as (element type, dimensions): `int[100]` is ("int", [100]); `int[1000][14]` and the
    Kamidori registry's `int[1000x14]` are ("int", [1000, 14]); an open or symbolic dimension (`int[][27]`,
    `int[unit][30]`) is None. None for a type that is no shape."""
    match = SHAPE.fullmatch(type_ or "")
    if not match:
        return None
    dims: list[int | None] = []
    for inner in re.findall(r"\[([^\]]*)\]", match.group(2)):
        if re.fullmatch(r"\d+", inner):
            dims.append(int(inner))
        elif re.fullmatch(r"\d+x\d+", inner):
            dims.extend(int(part) for part in inner.split("x"))
        elif inner == "" or IDENTIFIER.fullmatch(inner):
            dims.append(None)
        else:
            return None
    return match.group(1), dims


def layout_globals(entries: list[dict], source: str) -> tuple[list[dict], list[str]]:
    """The layout's globals, by name, and the errors: a type that is no shape, a dimension of zero, an open dimension
    after the first, a column map on a scalar or past two dimensions, a column out of range or named twice. A column
    map names the last dimension: a row's columns in a 2-D array, the elements of a 1-D one."""
    out: list[dict] = []
    errors: list[str] = []
    for entry in entries:
        name = entry.get("name")
        if not name:
            continue
        where = f"{source}: {name}"
        shape = dimensions(entry.get("type", ""))
        if shape is None:
            errors.append(f"{where}: type {entry.get('type')!r} is not a shape the layout maps")
            continue
        element, dims = shape
        if any(d == 0 for d in dims):
            errors.append(f"{where}: type {entry['type']!r} has a dimension of 0")
            continue
        if None in dims[1:]:
            errors.append(f"{where}: type {entry['type']!r}: only the first dimension may be open")
            continue
        value = {"name": name, "bank": BANKS[element], "address": f"0x{globals_build._parse_addr(entry['address']):x}",
                 "dims": dims}
        columns = entry.get("columns") or {}
        if columns:
            if len(dims) not in (1, 2):
                errors.append(f"{where}: columns name a 1-D array's elements or a 2-D array's columns; the type is "
                              f"{entry['type']!r}")
                continue
            by_name: dict[str, int] = {}
            for index, column in sorted(columns.items(), key=lambda item: int(item[0])):
                if not COLUMN.fullmatch(column):
                    errors.append(f"{where}: column {index} is named {column!r}, which is not a Lua name")
                elif not 0 <= int(index) < (dims[-1] if dims[-1] is not None else 1 << 31):
                    errors.append(f"{where}: column {index} ({column}) is outside its {dims[-1]}")
                elif column in by_name:
                    errors.append(f"{where}: column name {column} is given to columns {by_name[column]} and {index}")
                else:
                    by_name[column] = int(index)
            value["columns"] = by_name
        if entry.get("aliases"):
            value["aliases"] = sorted(entry["aliases"])
        out.append(value)
    out.sort(key=lambda value: value["name"])
    return out, errors


def render_globals(profile_id: str, values: list[dict], source: str) -> str:
    """The layout document, one global per line, so a registry change is a one-line diff."""
    def text(value) -> str:
        return json.dumps(value, ensure_ascii=False)
    lines = ["{",
             f' "note": {text(f"DO NOT EDIT -- generated from {source} by tools/base_mod_build.py --build")},',
             f' "layout": {LAYOUT_FORMAT},',
             f' "profile": {text(profile_id)},',
             ' "globals": [']
    lines += [f"  {text(value)}" + ("," if k + 1 < len(values) else "") for k, value in enumerate(values)]
    lines += [" ]", "}"]
    return "\n".join(lines) + "\n"


def oms_type(type_: str, records: dict[str, int]) -> str | None:
    """A registry type in the form the compiler's loader reads (GameModule.ParseGlobals): `int[1000x100]` is
    `int[1000][100]`, and a record table's name as a dimension (`int[unit][30]`) is that table's record count, given
    by `records`. An open first dimension stays open (`int[][27]`). None for a type it cannot write."""
    match = SHAPE.fullmatch(type_ or "")
    if not match:
        return None
    dims: list[str] = []
    for k, inner in enumerate(re.findall(r"\[([^\]]*)\]", match.group(2))):
        if re.fullmatch(r"\d+", inner):
            dims.append(inner)
        elif re.fullmatch(r"\d+x\d+", inner):
            dims.extend(inner.split("x"))
        elif inner == "" and k == 0:
            dims.append("")
        elif inner in records:
            dims.append(str(records[inner]))
        else:
            return None
    return match.group(1) + "".join(f"[{d}]" for d in dims)


def oms_globals(curated: dict, tables: list[dict], source: str) -> tuple[list[tuple[str, dict]], list[dict], list[str]]:
    """The cells and tables of oms/globals.json, and the errors: a named cell whose type the compiler cannot read. The
    cells keep the order of build/games/<game>/globals.json (by bank, then address), and the tables the registry's."""
    records = {t["name"]: t["records"] for t in tables if isinstance(t.get("records"), int)}
    cells: list[tuple[str, dict]] = []
    errors: list[str] = []
    for (bank, address), entry in sorted(curated.items()):
        name = entry.get("name")
        if not name:
            continue
        type_ = oms_type(entry.get("type", ""), records)
        if type_ is None:
            errors.append(f"{source}: {name}: type {entry.get('type')!r} is not a shape oms/globals.json can write "
                          f"(a dimension is a number, RxC, a record table's name, or open first)")
            continue
        cell: dict = {"name": name}
        if entry.get("aliases"):
            cell["aliases"] = sorted(entry["aliases"])
        cell["type"] = type_
        cell["address"] = f"0x{globals_build._parse_addr(entry['address']):x}"
        if entry.get("columns"):
            cell["columns"] = {index: column for index, column in
                               sorted(entry["columns"].items(), key=lambda item: int(item[0]))}
        cells.append((globals_build.cell_key(bank, address), cell))
    out: list[dict] = []
    for table in tables:
        value: dict = {"name": table["name"]}
        if table.get("aliases"):
            value["aliases"] = sorted(table["aliases"])
        value["records"] = table["records"]
        value["fields"] = dict(table["fields"])
        if table.get("field_aliases"):
            value["field_aliases"] = dict(sorted(table["field_aliases"].items()))
        out.append(value)
    return cells, out, errors


def render_oms_globals(profile_id: str, cells: list[tuple[str, dict]], tables: list[dict], source: str) -> str:
    """oms/globals.json, one cell or table per line, so a registry change is a one-line diff."""
    def text(value) -> str:
        return json.dumps(value, ensure_ascii=False)
    lines = ["{",
             f' "note": {text(f"DO NOT EDIT -- generated from {source} by tools/base_mod_build.py --build")},',
             f' "format": {OMS_GLOBALS_FORMAT},',
             f' "profile": {text(profile_id)},',
             ' "cells": {']
    lines += [f"  {text(key)}: {text(cell)}" + ("," if k + 1 < len(cells) else "") for k, (key, cell) in enumerate(cells)]
    lines += [" },", ' "tables": [']
    lines += [f"  {text(table)}" + ("," if k + 1 < len(tables) else "") for k, table in enumerate(tables)]
    lines += [" ]", "}"]
    return "\n".join(lines) + "\n"


def render_points(profile_id: str, registry: Path) -> tuple[str, list[str]]:
    """The patch points as patch_points_build.py writes them, validated without the install."""
    source = f"vm-map/{registry.name}"
    points: list[dict] = []
    errors: list[str] = []
    if registry.is_file():
        points, errors, _ = patch_points_build.validate(patch_points_build.load_registry(registry), None,
                                                        profile=profile_id)
        errors = [f"{source}: {error}" for error in errors]
    document = {"generated_from": source if registry.is_file() else None,
                "note": "DO NOT EDIT -- generated by tools/base_mod_build.py --build",
                "profile": profile_id, "points": points}
    return json.dumps(document, ensure_ascii=False, indent=2) + "\n", errors


def saved_prefixes(manifest: Path) -> tuple[dict[str, int] | None, str | None]:
    """Each bank's saved prefix from a profile manifest's persistence.bankDimensions: a cell below it is kept by a
    save, a cell at or above it is rebuilt at boot (docs/sys4-format-notes.md, "Saved prefix"). Or an error."""
    try:
        data = json.loads(manifest.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError) as error:
        return None, f"{manifest.name}: {error}"
    dims = (data.get("persistence") or {}).get("bankDimensions")
    if not isinstance(dims, dict):
        return None, (f"{manifest.name} has no persistence.bankDimensions, so which fields of a record table a save "
                      f"holds is not known")
    prefixes: dict[str, int] = {}
    for bank, key in PREFIX_KEYS.items():
        value = dims.get(key)
        if not isinstance(value, int) or isinstance(value, bool) or value < 0:
            return None, f"{manifest.name}: persistence.bankDimensions.{key} is {value!r}, not a count of cells"
        prefixes[bank] = value
    return prefixes, None


def record_tables(tables: list[dict], values: list[dict], prefixes: dict[str, int] | None,
                  source: str) -> tuple[list[dict], list[str]]:
    """The record map's tables (data patches spec §6.3), and the errors: a field whose global the layout does not map,
    a 1-D field with column names (they would name records), and a field whose array straddles its bank's saved
    prefix. A field is state when its array ends at or below the prefix, so that a load brings its value back."""
    by_name = {value["name"]: value for value in values}
    out: list[dict] = []
    errors: list[str] = []
    for table in tables:
        fields: dict[str, dict] = {}
        for field, global_name in table["fields"].items():
            where = f"{source}: table {table['name']}: field {field}"
            value = by_name.get(global_name)
            if value is None:
                errors.append(f"{where} names {global_name}, which the layout does not map")
                continue
            dims = value["dims"]
            # A table of one record may name a scalar (its one cell) or a 1-D array (its one row), as
            # globals_build.record_dims allows.
            if table["records"] == 1 and len(dims) == 0:
                dims = [1]
            elif table["records"] == 1 and len(dims) == 1 and dims[0] != 1:
                dims = [1, dims[0]]
            if not dims or dims[0] != table["records"] or len(dims) not in (1, 2):
                errors.append(f"{where}: {global_name} is not {table['records']} records of one cell or one row")
                continue
            entry: dict = {"global": global_name, "kind": KINDS[value["bank"]]}
            if len(dims) == 2:
                entry["width"] = dims[1]
                if value.get("columns"):
                    entry["columns"] = value["columns"]
            elif value.get("columns"):
                errors.append(f"{where}: {global_name} has one cell for each record, so its column names would name "
                              f"records")
                continue
            start = int(value["address"], 16)
            end = start + table["records"] * (dims[1] if len(dims) == 2 else 1)
            prefix = prefixes[value["bank"]] if prefixes else None
            if prefix is not None and end <= prefix:
                entry["state"] = True
            elif prefix is not None and start < prefix:
                errors.append(f"{where}: {global_name} ({value['bank']} {value['address']} to 0x{end:x}) straddles "
                              f"the saved prefix 0x{prefix:x}, so a save would keep part of it")
                continue
            fields[field] = entry
        record_table = {"name": table["name"], "records": table["records"], "built_by": table["built_by"],
                        "fields": fields}
        # The old names (data patches plan §9.1), of the fields the map has.
        if table.get("aliases"):
            record_table["aliases"] = sorted(table["aliases"])
        old_fields = {old: current for old, current in (table.get("field_aliases") or {}).items() if current in fields}
        if old_fields:
            record_table["old_fields"] = old_fields
        out.append(record_table)
    out.sort(key=lambda table: table["name"])
    return out, errors


def scope_label_errors(tables: list[dict], scopes: list[dict], source: str) -> list[str]:
    """Errors: a table that no scope reaches whose `built_by` is a label that builds a scoped table. ome-records'
    applier writes a label's tables either from the document's top level (no scope given) or through the scopes the
    apply names, so a base mod applies a scoped label with its scope and the other table would be checked and never
    written (data patches plan §12.6)."""
    scoped = {table for scope in scopes for table in (scope.get("tables") or {}).values()}
    scoped_labels: dict[str, str] = {}
    for table in sorted(tables, key=lambda table: table["name"]):
        if table["name"] in scoped:
            scoped_labels.setdefault(table["built_by"], table["name"])
    errors: list[str] = []
    for table in sorted(tables, key=lambda table: table["name"]):
        label = table["built_by"]
        if table["name"] not in scoped and label in scoped_labels:
            errors.append(f"{source}: table {table['name']} is built by {label}, as the scoped table "
                          f"{scoped_labels[label]} is, but no scope reaches it: ome-records writes a {label} table only "
                          f"through its scope, so name it in a [scope.tables] map, or give it its own built_by")
    return errors


def _lua_key(name: str) -> str:
    return name if LUA_NAME.fullmatch(name) and name not in LUA_KEYWORDS else f"[{json.dumps(name)}]"


def render_record_map(profile_id: str, tables: list[dict], source: str, manifest: str,
                      scopes: list[dict] = ()) -> str:
    """The record map as a Lua module, one field to a line, so a registry change is a one-line diff. `scopes` follows
    `tables` when the registry has a scope: each scope's ids and its tables by short name."""
    def names_line(indent: str, key: str, names: dict[str, str]) -> str:
        return (f"{indent}{key} = {{ " + ", ".join(f"{_lua_key(old)} = {json.dumps(names[old])}" for old in sorted(names))
                + " },")

    def field_line(name: str, entry: dict) -> str:
        parts = [f"global = {json.dumps(entry['global'])}", f"kind = {json.dumps(entry['kind'])}"]
        if "width" in entry:
            parts.append(f"width = {entry['width']}")
        if entry.get("columns"):
            columns = sorted(entry["columns"].items(), key=lambda item: item[1])
            parts.append("columns = { " + ", ".join(f"{_lua_key(column)} = {index}" for column, index in columns)
                         + " }")
        if entry.get("state"):
            parts.append("state = true")
        return f"                {_lua_key(name)} = {{ " + ", ".join(parts) + " },"
    lines = [f"-- DO NOT EDIT -- generated from {source} and {manifest} by tools/base_mod_build.py --build",
             f"-- The record tables of {base_mod_id(profile_id)} (data patches spec §6.3), for its tables.lua: each "
             f"table's",
             "-- record count and the part of the game that builds it (the registry's built_by), and for each field its",
             "-- global, its kind, a row's width and named columns (from 0), and state = true where the field's array",
             "-- lies in its bank's saved prefix, so that a load brings the saved value back. old_fields and old_tables",
             "-- give each old name of a field or a table its current one. scopes gives each scope's ids (first to",
             "-- last) and its tables by short name, which a document reaches as scope.<id>.<short name>.",
             "return {",
             f"    format = {RECORD_MAP_FORMAT},",
             "    tables = {"]
    for table in tables:
        lines += [f"        {_lua_key(table['name'])} = {{",
                  f"            records = {table['records']},",
                  f"            built_by = {json.dumps(table['built_by'])},",
                  "            fields = {"]
        lines += [field_line(name, table["fields"][name]) for name in sorted(table["fields"])]
        lines += ["            },"]
        if table.get("old_fields"):
            lines.append(names_line("            ", "old_fields", table["old_fields"]))
        lines += ["        },"]
    lines += ["    },"]
    if scopes:
        lines.append("    scopes = {")
        for scope in sorted(scopes, key=lambda scope: scope["name"]):
            short_names = ", ".join(f"{_lua_key(short)} = {json.dumps(scope['tables'][short])}"
                                    for short in sorted(scope["tables"]))
            lines.append(f"        {_lua_key(scope['name'])} = {{ first = {scope['first']}, last = {scope['last']}, "
                         f"tables = {{ {short_names} }} }},")
        lines.append("    },")
    old_tables = {alias: table["name"] for table in tables for alias in table.get("aliases", [])}
    if old_tables:
        lines.append(names_line("    ", "old_tables", old_tables))
    lines += ["}"]
    return "\n".join(lines) + "\n"


SHAPE_FILE = "tables.schema.json"
SHAPE_DRAFT = "http://json-schema.org/draft-07/schema#"
DECIMAL_KEYS = "x-ome-decimal-keys"
INTEGER_RANGE = (-2**31, 2**32 - 1)   # an integer cell's values, as ome-records' applier takes them


def _not_offered(why: str) -> dict:
    """The subset's mark for a key a file does not set (plan §6.2): no value passes, and the error is `why`."""
    return {"description": why, "type": "null"}


def _cell(kind: str, description: str) -> dict:
    if kind == "integer":
        return {"description": f"{description}, a whole number", "type": "integer",
                "minimum": INTEGER_RANGE[0], "maximum": INTEGER_RANGE[1]}
    if kind == "float":
        return {"description": f"{description}, a number", "type": "number"}
    return {"description": f"{description}, text the save's code page (932) can encode", "type": "string"}


def _saved_state(name: str) -> dict:
    return _not_offered(f"{name} is saved state: a load brings the saved value back, so a file does not set it")


def _field_shape(table: str, field: str, entry: dict) -> dict:
    """A field's schema: a value, or a row by its column numbers, or a row by its column names and the numbers of
    the columns that have none."""
    name, global_name = f"{table}.{field}", entry["global"]
    if entry.get("state"):
        return _saved_state(name)
    if "width" not in entry:
        return _cell(entry["kind"], f"{name}: {global_name}")
    width = entry["width"]
    columns = entry.get("columns") or {}
    # A row's description says what to write: the checker quotes it when a file gives the row another kind of value
    # (a whole row as an array, a single number), and an editor shows it on hover.
    if not columns:
        return {"description": f"{name}: {global_name}, a row of {width} columns by number, 0 to {width - 1}; "
                               f"write {field}.<number> = ...",
                "type": "object", DECIMAL_KEYS: {"minimum": 0, "maximum": width - 1},
                "additionalProperties": _cell(entry["kind"], f"{name}.<column>: a column of {global_name}")}
    by_index = {index: column for column, index in columns.items()}
    properties: dict[str, dict] = {}
    for index in range(width):
        column = by_index.get(index)
        if column is None:
            properties[str(index)] = _cell(entry["kind"], f"{name}.{index}: column {index} of {global_name}")
        else:
            properties[column] = _cell(entry["kind"], f"{name}.{column}: column {index} of {global_name}")
            properties[str(index)] = _not_offered(f"column {index} is named: write {column}")
    spelling = ("by name" if len(by_index) == width
                else "by name where a column has one and by number where it has none")
    return {"description": f"{name}: {global_name}, a row of {width} columns {spelling}; write {field}.<column> = ...",
            "type": "object", "properties": dict(sorted(properties.items())), "additionalProperties": False}


LABELS_LINE = re.compile(r"^\s*labels\s*=\s*\{([^}]*)\}", re.MULTILINE)


def format_labels(root: Path) -> tuple[list[str] | None, str | None]:
    """The `built_by` labels the base mod's `<game>.tables` handlers apply: the `labels = { ... }` line of its
    hand-kept `lua/tables.lua`, which hands them to ome-records' applier. None when the base mod has no such file."""
    handlers = root / "lua" / "tables.lua"
    if not handlers.is_file():
        return None, None
    match = LABELS_LINE.search(handlers.read_text(encoding="utf-8"))
    labels = re.findall(r'"([a-z_][a-z0-9_]*)"', match.group(1)) if match else []
    if not labels:
        return None, (f"{handlers.relative_to(root.parent.parent).as_posix()}: no `labels = {{ \"...\" }}` line, which "
                      "names the tables the format applies")
    return labels, None


def render_shape(profile_id: str, tables: list[dict], source: str, manifest: str, scopes: list[dict] = (),
                 labels: list[str] | None = None) -> str:
    """The shape of the kind <game>.tables (data patches spec §7, plan §6.2), as JSON in the shape subset: what
    ome-records' applier takes, in the terms a shape can say. A table built by a label the format's handlers do not
    apply (`labels`; all of them when None) takes no value, as the applier refuses it. Each schema's keywords come in
    one order, with its description first; names in `properties` and `definitions` are sorted, so a registry change
    is a small diff."""
    scoped = {table_name: (scope["name"], short) for scope in scopes for short, table_name in scope["tables"].items()}
    definitions: dict[str, dict] = {}
    top: dict[str, dict] = {}
    for table in tables:
        name, last = table["name"], table["records"] - 1
        if labels is not None and table["built_by"] not in labels:
            for spelling in [name, *table.get("aliases", [])]:
                top[spelling] = _not_offered(f"{name} is built by {table['built_by']}, which {profile_id}.tables "
                                             f"does not edit")
            continue
        fields = {field: _field_shape(name, field, entry) for field, entry in table["fields"].items()}
        for old, current in (table.get("old_fields") or {}).items():
            if table["fields"][current].get("state"):
                fields[old] = _saved_state(f"{name}.{old}")
            else:
                fields[old] = {**fields[current], "description": f"{name}.{old}: an old name of {name}.{current}; "
                                                                 f"write {current}"}
        # The path a file writes a record's field at, which the checker quotes when a record or the table is given
        # another kind of value: a scoped table's records are under their scope's id.
        written = f"{scoped[name][0]}.<id>.{scoped[name][1]}" if name in scoped else name
        definitions[f"record.{name}"] = {"description": f"{name}: one record's fields, by name; write "
                                                        f"{written}.<id>.<field> = ...", "type": "object",
                                         "properties": dict(sorted(fields.items())), "additionalProperties": False}
        definitions[f"records.{name}"] = {"description": f"{name}: records by id, 0 to {last}; write "
                                                         f"{written}.<id>.<field> = ...", "type": "object",
                                          DECIMAL_KEYS: {"minimum": 0, "maximum": last},
                                          "additionalProperties": {"$ref": f"#/definitions/record.{name}"}}
        if name in scoped:
            scope, short = scoped[name]
            for spelling in [name, *table.get("aliases", [])]:
                top[spelling] = _not_offered(f"{name} is reached through its scope: write {scope}.<id>.{short}")
        else:
            top[name] = {"$ref": f"#/definitions/records.{name}"}
            for alias in table.get("aliases", []):
                top[alias] = {"description": f"{alias}: an old name of the table {name}; write {name}",
                              "$ref": f"#/definitions/records.{name}"}
    refused = {table["name"]: table["built_by"] for table in tables
               if labels is not None and table["built_by"] not in labels}
    for scope in scopes:
        scope_name, first, last = scope["name"], scope["first"], scope["last"]
        parts = {short: (_not_offered(f"{table_name} is built by {refused[table_name]}, which {profile_id}.tables does "
                                      f"not edit") if table_name in refused
                         else {"$ref": f"#/definitions/records.{table_name}"})
                 for short, table_name in scope["tables"].items()}
        write = f"write {scope_name}.<id>.{' or '.join(sorted(parts))}.<id>.<field> = ..."
        definitions[f"scope.{scope_name}"] = {
            "description": f"{scope_name}: each {scope_name}'s tables by its id, {first} to {last}; {write}",
            "type": "object", DECIMAL_KEYS: {"minimum": first, "maximum": last},
            "additionalProperties": {"description": f"one {scope_name}'s tables, by short name; {write}",
                                     "type": "object", "properties": dict(sorted(parts.items())),
                                     "additionalProperties": False}}
        top[scope_name] = {"$ref": f"#/definitions/scope.{scope_name}"}
    shape = {
        "$schema": SHAPE_DRAFT,
        "description": (f"DO NOT EDIT -- generated from {source} and {manifest} by tools/base_mod_build.py --build. "
                        f"The shape of {profile_id}.tables, the kind {base_mod_id(profile_id)} accepts (data patches "
                        f"spec §7): a file's record tables by name, each record by its id in decimal, each field by "
                        f"name, and each scope's tables by its id and their short names. A field the save holds, a "
                        f"scoped table's own name, a table the format does not edit and a named column's number take "
                        f"no value, and say why. The base "
                        f"mod still checks, at a file's first delivery, what a shape cannot say: text the save's code "
                        f"page cannot encode, one cell named twice through an old name, and a part at an event this "
                        f"install does not raise."),
        "type": "object",
        "properties": dict(sorted(top.items())),
        "additionalProperties": False,
        "definitions": dict(sorted(definitions.items())),
    }
    return json.dumps(shape, indent=2, ensure_ascii=False) + "\n"


TABLE_PREFIX, FIELD_PREFIX, SCOPE_PREFIX, MODULE_PREFIX = "table ", "field ", "scope ", "module "


def _without_comment(line: str) -> str:
    """A line of OMS source without its `//` comment (one outside a string literal)."""
    quoted = False
    for index, char in enumerate(line):
        if char == '"' and (index == 0 or line[index - 1] != "\\"):
            quoted = not quoted
        elif not quoted and line.startswith("//", index):
            return line[:index].rstrip()
    return line.rstrip()


def module_names(root: Path) -> tuple[dict[str, str], list[str]]:
    """The names the base mod's OMS modules declare (oms/*.oms), as `MODULE.NAME` to the file that declares it, and
    the errors. They are what a source that uses the module may name, as OmsCompiler.Use imports them: constants,
    enums and their members, templates, global cells, tables, and subs with the labels in their bodies. Read by the
    modules' layout: a declaration starts at column 0, and a `}` at column 0 closes its block. BaseModTests checks the
    list against the parser for each base mod the engine ships."""
    names: dict[str, str] = {}
    errors: list[str] = []
    for path in sorted((root / "oms").glob("*.oms")):
        shown = f"oms/{path.name}"
        module = None
        block = None   # the kind of the open top-level block
        members = ""   # an open enum's text
        for number, raw in enumerate(path.read_text(encoding="utf-8").splitlines(), 1):
            line = _without_comment(raw)
            if block is not None:
                if line == "}":
                    if block == "enum":
                        for member in OMS_MEMBER.findall(members):
                            names.setdefault(f"{module}.{member}", shown)
                    block = None
                elif block == "enum":
                    members += line + "\n"
                elif block in ("sub", "coroutine") and (label := OMS_LABEL.fullmatch(line)):
                    names.setdefault(f"{module}.{label.group(1)}", shown)
                continue
            if not line or line[0].isspace():
                continue
            if module is None:
                if found := OMS_MODULE.fullmatch(line):
                    module = found.group(1)
                continue
            found = OMS_DECLARATION.match(line)
            if not found:
                continue
            groups = [group for group in found.groups() if group is not None]
            kind, name = groups[0], groups[1]
            if f"{module}.{name}" in names:
                errors.append(f"{shown}:{number}: '{name}' is declared twice in {module}")
            names[f"{module}.{name}"] = shown
            opens, closes = line.count("{"), line.count("}")
            if kind == "enum" and opens:
                members = line[line.index("{") + 1:] + "\n"
                if closes:
                    for member in OMS_MEMBER.findall(members.split("}", 1)[0]):
                        names.setdefault(f"{module}.{member}", shown)
                else:
                    block = "enum"
            elif opens > closes:
                block = "template" if kind == "inline" else kind
        if module is None:
            errors.append(f"{shown}: no `module` line")
    return names, errors


def exported_names(values: list[dict], tables: list[dict] = (), scopes: list[dict] = (),
                   modules: dict[str, str] | None = None) -> dict[str, str | None]:
    """Each exported name: None for a global's own name, the global's name for an alias. A record table is
    `table NAME` and its field `field TABLE.FIELD` (data patches plan §9.1), an old one with its current name. A scope
    is `scope NAME`, and each table's short name under it `scope NAME.SHORT` (data patches spec §6.2). A name an OMS
    module declares is `module MODULE.NAME` (module_names)."""
    names: dict[str, str | None] = {}
    for value in values:
        names[value["name"]] = None
        for alias in value.get("aliases", []):
            names[alias] = value["name"]
    for table in tables:
        name = table["name"]
        names[TABLE_PREFIX + name] = None
        for alias in table.get("aliases") or []:
            names[TABLE_PREFIX + alias] = name
        for field in table["fields"]:
            names[f"{FIELD_PREFIX}{name}.{field}"] = None
        for old, current in (table.get("field_aliases") or {}).items():
            names[f"{FIELD_PREFIX}{name}.{old}"] = f"{name}.{current}"
    for scope in scopes:
        names[SCOPE_PREFIX + scope["name"]] = None
        for short in scope["tables"]:
            names[f"{SCOPE_PREFIX}{scope['name']}.{short}"] = None
    for name in modules or {}:
        names[MODULE_PREFIX + name] = None
    return names


def render_exports(profile_id: str, major: int, names: dict[str, str | None], source: str) -> str:
    lines = [f"# DO NOT EDIT -- generated from {source} by tools/base_mod_build.py --build",
             f"# The names {base_mod_id(profile_id)} {major} exports (Lua scripting spec §9.1): its globals, then its",
             "# record tables and their fields (data patches plan §9.1), then its scopes and their tables' short names",
             "# (data patches spec §6.2), then the names its OMS modules declare, oms/*.oms (modding from a release",
             "# spec §5.2). Each stays until the major version changes: a renamed global, table or field keeps its old",
             "# name, listed as OLD = NAME; a scope and a module's names have no old-name form.",
             f"major = {major}"]
    def group(name: str) -> int:
        return (1 if name.startswith(TABLE_PREFIX) else 2 if name.startswith(FIELD_PREFIX)
                else 3 if name.startswith(SCOPE_PREFIX) else 4 if name.startswith(MODULE_PREFIX) else 0)
    for name in sorted(names, key=lambda name: (group(name), name)):
        lines.append(name if names[name] is None else f"{name} = {names[name]}")
    return "\n".join(lines) + "\n"


def parse_exports(text: str) -> tuple[int | None, set[str]]:
    """The major version and the names of an exports.txt."""
    major = None
    names: set[str] = set()
    for raw in text.splitlines():
        line = raw.strip()
        if not line or line.startswith("#"):
            continue
        key, _, value = (part.strip() for part in line.partition("="))
        if key == "major":
            major = int(value)
        else:
            names.add(key)
    return major, names


def accepts_tables(manifest: Path) -> bool:
    """Whether the base mod's manifest accepts its data kind: an [[accepts]] names the shape SHAPE_FILE."""
    try:
        data = tomllib.loads(manifest.read_text(encoding="utf-8"))
    except (OSError, tomllib.TOMLDecodeError):
        return False
    return any(isinstance(entry, dict) and entry.get("shape") == SHAPE_FILE for entry in data.get("accepts", []))


def manifest_major(manifest: Path) -> tuple[int | None, str | None]:
    """The base mod's major version, or an error."""
    try:
        data = tomllib.loads(manifest.read_text(encoding="utf-8"))
    except (OSError, tomllib.TOMLDecodeError) as error:
        return None, f"{manifest.name}: {error}"
    match = VERSION.match(str(data.get("version", "")))
    if not match:
        return None, f"{manifest.name}: version {data.get('version')!r} is not MAJOR.MINOR.PATCH"
    return int(match.group(1)), None


def rename_errors(profile_id: str, released: tuple[int | None, set[str]] | None, major: int,
                  names: dict[str, str | None], modules_read: bool = True) -> list[str]:
    """The rename rule against the checked-in exports (spec §9.1)."""
    if released is None or released[0] is None:
        return []
    released_major, released_names = released
    if major < released_major:
        return [f"{base_mod_id(profile_id)}: the major version {major} is lower than the released {released_major}"]
    if major > released_major:
        return []
    # A field of a table's old name is the field of its current name.
    current_table = {key[len(TABLE_PREFIX):]: target for key, target in names.items()
                     if key.startswith(TABLE_PREFIX) and target is not None}
    dropped = released_names - names.keys()
    errors = []
    for name in sorted(dropped):
        if name.startswith(MODULE_PREFIX):
            if not modules_read:
                continue  # The modules' own errors stand for them.
            module, _, symbol = name[len(MODULE_PREFIX):].rpartition(".")
            errors.append(f"{base_mod_id(profile_id)} {major} exports '{symbol}' of the module {module}, which oms/ no "
                          f"longer declares: a module's names have no old-name form, so keep the name (a sub can stay "
                          f"as one that calls its new name) or raise the base mod's major version")
        elif name.startswith(SCOPE_PREFIX):
            scope, _, short = name[len(SCOPE_PREFIX):].partition(".")
            if short and SCOPE_PREFIX + scope in dropped:
                continue  # A dropped scope's error stands for its short names.
            what = f"the short name '{scope}.{short}'" if short else f"the scope '{scope}'"
            errors.append(f"{base_mod_id(profile_id)} {major} exports {what}, which the registry no longer has: a "
                          f"scope and its short names have no old-name form, so raise the base mod's major version")
        elif name.startswith(FIELD_PREFIX):
            table, _, field = name[len(FIELD_PREFIX):].partition(".")
            # A dropped table's error stands for its fields.
            if (f"{FIELD_PREFIX}{current_table.get(table, table)}.{field}" in names
                    or TABLE_PREFIX + table in dropped):
                continue
            errors.append(f"{base_mod_id(profile_id)} {major} exports the field '{table}.{field}', which the registry no "
                          f"longer has: keep it as an old name of the field's new name ([table.field_aliases] "
                          f"{field} = \"NEW\"), or raise the base mod's major version")
        elif name.startswith(TABLE_PREFIX):
            table = name[len(TABLE_PREFIX):]
            errors.append(f"{base_mod_id(profile_id)} {major} exports the table '{table}', which the registry no longer "
                          f"has: keep it as an alias of the table's new name (aliases = [\"{table}\"] in its [[table]]), "
                          f"or raise the base mod's major version")
        else:
            errors.append(f"{base_mod_id(profile_id)} {major} exports '{name}', which the registry no longer has: keep "
                          f"it as an alias of the global's new name (aliases = [\"{name}\"]), or raise the base mod's "
                          f"major version")
    return errors


def generate(profile_id: str, mods: Path = MODS, registry: Path | None = None,
             points_registry: Path | None = None, manifest: Path | None = None) -> tuple[dict[Path, str], list[str]]:
    """The files a base mod's layout, record map, shape and oms/globals.json should hold, and the errors that block
    them."""
    root = base_mod_dir(profile_id, mods)
    registry = registry or globals_source(profile_id)
    points_registry = points_registry or points_source(profile_id)
    manifest = manifest or profile_manifest(profile_id)
    # A game's base mod starts with its toolchain (adding-a-game.md, step 10, M1), which may come before its registry
    # (step 9, M2): it then names no cell, no table and no scope.
    has_registry = registry.is_file()
    source = f"vm-map/{registry.name}" if has_registry else f"no registry (vm-map/{registry.name} is not written yet)"
    errors: list[str] = []
    major, error = manifest_major(root / "manifest.toml")
    if error:
        return {}, [f"{base_mod_id(profile_id)}: {error}"]
    curated = globals_build.load_toml(registry)[0] if has_registry else globals_build.Registry()
    lint_errors, _ = globals_build.lint(curated, curated.addresses())
    lint_errors += globals_build.lint_aliases(curated)
    errors += [f"{source}: {message}" for message in lint_errors]
    values, shape_errors = layout_globals(list(curated.values()), source)
    errors += shape_errors
    tables = globals_build.load_tables(registry) if has_registry else []
    scopes = globals_build.load_scopes(registry) if has_registry else []
    table_errors = globals_build.lint_tables(tables, curated) + globals_build.lint_scopes(scopes, tables, curated)
    errors += [f"{source}: {message}" for message in table_errors]
    if not table_errors:
        errors += scope_label_errors(tables, scopes, source)
    prefixes, prefix_error = saved_prefixes(manifest) if tables else (None, None)
    if prefix_error:
        errors.append(f"{base_mod_id(profile_id)}: {prefix_error}")
    oms_cells, oms_tables, oms_errors = oms_globals(curated, tables if not table_errors else [], source)
    errors += oms_errors
    record_map: list[dict] = []
    if not table_errors and not prefix_error:
        record_map, map_errors = record_tables(tables, values, prefixes, source)
        errors += map_errors
    points, point_errors = render_points(profile_id, points_registry)
    errors += point_errors
    labels, label_error = format_labels(root)
    if label_error:
        errors.append(f"{base_mod_id(profile_id)}: {label_error}")
    modules, module_errors = module_names(root)
    errors += [f"{base_mod_id(profile_id)}: {message}" for message in module_errors]
    names = exported_names(values, tables if not table_errors else [], scopes if not table_errors else [], modules)
    exports = root / "layout" / "exports.txt"
    released = parse_exports(exports.read_text(encoding="utf-8")) if exports.is_file() else None
    errors += rename_errors(profile_id, released, major, names, modules_read=not module_errors)
    files = {root / "layout" / "globals.json": render_globals(profile_id, values, source),
             root / "layout" / "points.json": points,
             root / "oms" / "globals.json": render_oms_globals(profile_id, oms_cells, oms_tables, source),
             exports: render_exports(profile_id, major, names, source)}
    if accepts_tables(root / "manifest.toml"):
        files[root / "lua" / "generated" / "tables.lua"] = render_record_map(
            profile_id, record_map, source, _shown(manifest), scopes if not table_errors else [])
        files[root / "shapes" / SHAPE_FILE] = render_shape(profile_id, record_map, source, _shown(manifest),
                                                           scopes if not table_errors else [], labels)
    else:
        for stale in (root / "lua" / "generated" / "tables.lua", root / "shapes" / SHAPE_FILE):
            if stale.is_file():
                errors.append(f"{base_mod_id(profile_id)}: {stale.relative_to(root).as_posix()} belongs to the data kind, which the "
                              f"manifest does not accept (no [[accepts]] names {SHAPE_FILE}); remove it")
    return files, errors


def profiles_with_base_mods(mods: Path = MODS) -> list[str]:
    return [profile_id for profile_id in paths.available_profiles()
            if (base_mod_dir(profile_id, mods) / "manifest.toml").is_file()]


def _shown(path: Path) -> str:
    return path.relative_to(paths.REPO).as_posix() if path.is_relative_to(paths.REPO) else str(path)


def main(argv=None) -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    mode = parser.add_mutually_exclusive_group(required=True)
    mode.add_argument("--build", action="store_true",
                      help="write every base mod's layout, record map, shape and oms/globals.json")
    mode.add_argument("--check", action="store_true", help="fail if a layout is stale or a rename breaks the rule")
    args = parser.parse_args(argv)
    failed = False
    profiles = profiles_with_base_mods()
    for profile_id in profiles:
        files, errors = generate(profile_id)
        for error in errors:
            print(f"error: {error}")
        if errors:
            failed = True
            continue
        for path, text in files.items():
            current = path.read_text(encoding="utf-8") if path.is_file() else None
            if current == text:
                continue
            if args.check:
                print(f"stale: {_shown(path)}; run base_mod_build.py --build")
                failed = True
            else:
                path.parent.mkdir(parents=True, exist_ok=True)
                path.write_text(text, encoding="utf-8", newline="\n")
                print(f"wrote {_shown(path)}")
    if not failed:
        print(f"{'check' if args.check else 'build'}: {len(profiles)} base mods current ({', '.join(profiles)})")
    return 1 if failed else 0


if __name__ == "__main__":
    sys.exit(main())
