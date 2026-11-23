#!/usr/bin/env python3
"""Generator + linter for the living per-game global-variable registries.
Each profile owns its own registry because global addresses are per game: Himegari uses
vm-map/globals.toml -> docs/global-reference.md; any other profile uses
vm-map/globals-<game>.toml -> docs/global-reference-<game>.md.
  --build   merge curated over build/games/<game>/global-var-map.json -> build/games/<game>/globals.json
            + the profile's generated reference doc
  --build-json  lint and merge, then write only build/games/<game>/globals.json (the core level's step; a checkout
            has no auto map, so its JSON holds the curated globals, which the OMS modules name)
  --lint    run the linter, print errors/warnings, exit nonzero on errors
  --check   lint, then exit nonzero if the reference doc differs from what --build would write (needs the auto map)
A registry may also declare `[[table]]` entries: Open Maid Script tables (record views over parallel
arrays, design spec §5.3), emitted to the JSON's `tables` list and the reference doc. A global's `aliases` keep the
names it had before a rename, which a base mod still exports (tools/base_mod_build.py; Lua scripting spec §9.1). So do
a table's `aliases` and its `[table.field_aliases]` (old name = current field), which a data file may still write
(data patches plan §9.1). A `[[scope]]` entry keys tables by a scope of the game, such as a stage (data patches spec
§6.2): its ids' range and, in `[scope.tables]`, each table's short name under it; the JSON lists them as `scopes`.
A table's `[table.refs]` says which of its integer fields hold the ids of another table's records, or packed resource
ids (`picture`), every element of the field, 0 for none (game data export spec §5.3); the JSON's `tables` and the
reference doc carry them. A reference whose record is made of two fields is `{ table, times, plus }`: the record is the
field's value times `times`, plus field `plus`'s value at the same element (Arterial's cards, row × 30 + slot), and
with `any` a plus of that value names every record of the row, value × times to value × times + times - 1 (an Arterial
item that any card of a row may carry: `{ table, times, plus, any = 0 }`); one whose value picks where its records
start is `{ table, bases, plus }`: a value that `bases` lists names its base plus the plus field's value, and another
value names none (a Kamidori map object of type 32 or 33 uses dig table 0 or 100 plus its payload); one
whose two cells are a pair of elements of one field is `{ table, times, pairs = true }`: element 2k's value times
`times`, plus element 2k + 1's, names a record (a battle's leader cards, a row and a slot for each side). The JSON
writes its table under `refs` and the rest under `ref_keys`. A table's `records_of` names the table whose record r its
record r belongs to (Arterial's `party_member`, kept for each character by its character id); the JSON and the reference
doc carry it.
A global's value facets say what its values mean to a reader (game data export plan §5): `values` gives OME's word for
some values, `values_from` names the game's own table of names (`{ global, first }`: the string global's element k names
value first + k), `bits_of` makes it a mask whose bit k stands for value k of another global (or for element k of a
list of strings, the game's names of the bits: Arterial's races), and `scale`, `offset` and
`unit` say how a number is shown (the stored value times scale, plus offset, in unit), and `none` the stored value
that means none (a quest's deadline of 0: it has none), which a reader is not shown as a number. The JSON and the
reference doc carry them; the game data export publishes them.
An array's column facets name its columns for a reader where `columns` gives OME's words: `columns_from` names the
game's own table of names (`{ global, first }`: the string global's element k names column first + k, as an element's
names its resistances), and `columns_ref` the table whose record k column k is (an item's condition levels, by
condition id). The JSON and the reference doc carry them; the game data export publishes them.
See docs/superpowers/specs/2026-07-07-globals-registry-and-story-flags-design.md."""
from __future__ import annotations
import os, re, sys, json, math, argparse, collections, tomllib
from pathlib import Path
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import paths

TOML_DEFAULT = paths.CONTEXT.globals_toml
REFERENCE_DOC = paths.CONTEXT.global_reference_doc
AUTO_MAP     = paths.BUILD / "global-var-map.json"

CATEGORIES = {"story-flag", "index-pointer", "data-table", "string-table",
              "ui-toggle", "choice-output", "counter", "unknown"}
SOURCES    = {"investigation", "harness", "inference", "auto-shape"}
CONFIDENCE = {"low": 1, "med": 2, "high": 3}


def _parse_addr(v) -> int:
    return int(v, 16) if isinstance(v, str) else int(v)


def bank_of(type_: str | None) -> str:
    """The VM global bank an entry lives in. Integer and string globals are separate banks with
    separate address spaces, so `G[0x0]` and `GS[0x0]` are different cells; the type implies the bank."""
    return "GS" if (type_ or "").startswith("string") else "G"


def cell_key(bank: str, addr: int) -> str:
    return f"{bank}:0x{addr:x}"


class Registry(dict):
    """Entries keyed by (bank, address). A bare int address looks up the integer bank first and then the
    string bank, which keeps address-keyed callers working where banks do not collide."""

    def _resolve(self, key):
        if isinstance(key, int):
            for bank in ("G", "GS"):
                if dict.__contains__(self, (bank, key)):
                    return (bank, key)
        return key

    def __getitem__(self, key):
        return dict.__getitem__(self, self._resolve(key))

    def __contains__(self, key):
        return dict.__contains__(self, self._resolve(key))

    def get(self, key, default=None):
        return dict.get(self, self._resolve(key), default)

    def addresses(self) -> set[int]:
        return {addr for _, addr in self}


def load_toml(path) -> tuple[Registry, dict]:
    """Return ({(bank, addr) -> entry-dict (with _addr/_bank set)}, meta-dict). A repeated (bank, addr)
    keeps the last entry and records `_duplicates` for the linter."""
    data = tomllib.loads(Path(path).read_text(encoding="utf-8"))
    out = Registry()
    for e in data.get("global", []):
        addr = _parse_addr(e["address"])
        e = dict(e)
        e["_addr"] = addr
        e["_bank"] = bank_of(e.get("type"))
        key = (e["_bank"], addr)
        if key in out:
            e["_duplicates"] = out[key].get("_duplicates", 1) + 1
        out[key] = e
    return out, data.get("meta", {})


def lint(entries: dict[int, dict], all_addrs: set[int]) -> tuple[list[str], list[str]]:
    """Errors: bad vocabulary, auto-shape@high, dangling depends_on. Warnings: overlapping arrays."""
    errors: list[str] = []
    warnings: list[str] = []
    all_addrs = {key[1] if isinstance(key, tuple) else key for key in all_addrs}
    for key, e in sorted(entries.items(), key=lambda item: str(item[0])):
        bank, addr = key if isinstance(key, tuple) else (bank_of(e.get("type")), key)
        tag = f"0x{addr:x}" if bank == "G" else cell_key(bank, addr)
        if e.get("_duplicates"):
            errors.append(f"{tag}: defined {e['_duplicates']} times")
        cat, src, conf = e.get("category"), e.get("source"), e.get("confidence")
        if cat not in CATEGORIES:
            errors.append(f"{tag}: bad category {cat!r}")
        if src not in SOURCES:
            errors.append(f"{tag}: bad source {src!r}")
        if conf not in CONFIDENCE:
            errors.append(f"{tag}: bad confidence {conf!r}")
        if src == "auto-shape" and conf == "high":
            errors.append(f"{tag}: auto-shape source may not claim high confidence")
        columns = e.get("columns", {})
        if not isinstance(columns, dict):
            errors.append(f"{tag}: columns must be a table")
        else:
            for column, name in columns.items():
                try:
                    column_index = int(column)
                except (TypeError, ValueError):
                    errors.append(f"{tag}: bad column index {column!r}")
                    continue
                if column_index < 0:
                    errors.append(f"{tag}: negative column index {column_index}")
                if not isinstance(name, str) or not name:
                    errors.append(f"{tag}: column {column_index} has no semantic name")
        for dep in e.get("depends_on", []):
            if _parse_addr(dep) not in all_addrs:
                errors.append(f"{tag}: depends_on missing address {dep}")
    warnings.extend(overlap_warnings(entries))
    return errors, warnings


def dims_of(type_: str | None) -> list[int] | None:
    """The dimensions of a fully sized array (`int[1000][14]` -> [1000, 14]); None for a scalar, an open
    leading dimension (`int[][27]`) or a shape the OMS module loader would not read."""
    m = re.fullmatch(r"(?:int|string|float)((?:\[\d*\])+)", type_ or "")
    if not m:
        return None
    dims = re.findall(r"\[(\d*)\]", m.group(1))
    if "" in dims:
        return None
    return [int(d) for d in dims]


def record_dims(type_: str | None, records: int) -> list[int] | None:
    """A table field's global as `records` records: [records] (a cell each) or [records, n] (a row of n each). A
    table of one record may also name a scalar, its one cell, or a 1-D array of n, its one row of n: a block the game
    keeps one of, such as what a class change gives. None for any other shape."""
    if records == 1 and re.fullmatch(r"(?:int|string|float)", type_ or ""):
        return [1]
    dims = dims_of(type_)
    if dims is None:
        return None
    if len(dims) in (1, 2) and dims[0] == records:
        return dims
    if records == 1 and len(dims) == 1:
        return [1, dims[0]]
    return None


def extent_of(type_: str | None) -> int | None:
    """Cells a fully sized array covers (`int[1000][14]` -> 14000), or None (see dims_of)."""
    dims = dims_of(type_)
    return None if dims is None else math.prod(dims)


IDENTIFIER = re.compile(r"[a-z_][a-z0-9_]*")


def load_tables(path) -> list[dict]:
    """The registry's `[[table]]` entries, in order: Open Maid Script tables (design spec §5.3). Each names
    the parallel arrays that hold one kind of record; a field maps to a curated global by name."""
    data = tomllib.loads(Path(path).read_text(encoding="utf-8"))
    return [dict(t) for t in data.get("table", [])]


def load_scopes(path) -> list[dict]:
    """The registry's `[[scope]]` entries, in order (data patches spec §6.2): a scope of the game that keys some of
    its record tables, such as a stage, with its ids' range and its tables' short names."""
    data = tomllib.loads(Path(path).read_text(encoding="utf-8"))
    return [dict(s) for s in data.get("scope", [])]


def lint_scopes(scopes: list[dict], tables: list[dict], entries: dict) -> list[str]:
    """Errors: a scope name that is not a lowercase identifier, a repeated scope, and a scope named like a table, a
    table's alias or a global (a data file's top level names either); `first` and `last` that are not whole numbers
    with 0 <= first <= last; a `[scope.tables]` map that is missing or empty, a short name that is not a lowercase
    identifier, one that names no table of the registry, and a table under two scopes."""
    errors: list[str] = []
    globals_by_name = {e["name"] for e in entries.values() if e.get("name")}
    table_names = {t.get("name") for t in tables if isinstance(t.get("name"), str)}
    aliases = {alias for t in tables for alias in (t.get("aliases") or []) if isinstance(alias, str)}
    seen: set[str] = set()
    owner: dict[str, str] = {}
    for s in scopes:
        name = s.get("name")
        tag = f"scope {name!r}"
        if not isinstance(name, str) or not IDENTIFIER.fullmatch(name):
            errors.append(f"{tag}: name must be a lowercase identifier")
            continue
        if name in seen:
            errors.append(f"{tag}: defined twice")
        seen.add(name)
        if name in table_names:
            errors.append(f"{tag}: also names a table")
        elif name in aliases:
            errors.append(f"{tag}: also an alias of a table")
        if name in globals_by_name:
            errors.append(f"{tag}: also names a global")
        first, last = s.get("first"), s.get("last")
        if not all(isinstance(v, int) and not isinstance(v, bool) for v in (first, last)) or not 0 <= first <= last:
            errors.append(f"{tag}: first and last must be whole numbers with 0 <= first <= last, the range of its ids")
        short_names = s.get("tables")
        if not isinstance(short_names, dict) or not short_names:
            errors.append(f"{tag}: needs a [scope.tables] map, each short name to a table")
            continue
        for short, table in short_names.items():
            if not IDENTIFIER.fullmatch(short):
                errors.append(f"{tag}: short name {short!r} must be a lowercase identifier")
            if not isinstance(table, str) or table not in table_names:
                errors.append(f"{tag}: {short} names {table!r}, which is no table of the registry")
            elif table in owner:
                errors.append(f"{tag}: table {table} is named by scope {owner[table]!r} already; a table has one "
                              f"scope and one short name")
            else:
                owner[table] = name
    return errors


def lint_tables(tables: list[dict], entries: dict) -> list[str]:
    """Errors: a table or field name that is not a lowercase identifier, a repeated table, a table named like
    a global, a `built_by` label that is missing or not a lowercase identifier (which part of the game writes the
    table, data patches spec §6.3), a field whose global is missing or is not a fully sized 1-D or 2-D array of
    `records` entries or rows, the old names' errors (`lint_table_aliases`) and the references' (`lint_table_refs`)."""
    errors: list[str] = []
    by_name: dict[str, dict] = {}
    for e in entries.values():
        if e.get("name"):
            by_name.setdefault(e["name"], e)
    seen: set[str] = set()
    for t in tables:
        name = t.get("name")
        tag = f"table {name!r}"
        if not isinstance(name, str) or not IDENTIFIER.fullmatch(name):
            errors.append(f"{tag}: name must be a lowercase identifier")
            continue
        if name in seen:
            errors.append(f"{tag}: defined twice")
        seen.add(name)
        if name in by_name:
            errors.append(f"{tag}: also names a global")
        built_by = t.get("built_by")
        if not isinstance(built_by, str) or not IDENTIFIER.fullmatch(built_by):
            errors.append(f"{tag}: built_by must be a lowercase identifier, the part of the game that writes the "
                          f"table (boot, stage)")
        records = t.get("records")
        if not isinstance(records, int) or isinstance(records, bool) or records <= 0:
            errors.append(f"{tag}: records must be a positive integer")
            continue
        fields = t.get("fields")
        if not isinstance(fields, dict) or not fields:
            errors.append(f"{tag}: needs a [table.fields] map")
            continue
        for field, global_name in fields.items():
            if not IDENTIFIER.fullmatch(field):
                errors.append(f"{tag}: field {field!r} must be a lowercase identifier")
            e = by_name.get(global_name) if isinstance(global_name, str) else None
            if e is None:
                errors.append(f"{tag}: field {field} names unknown global {global_name!r}")
                continue
            if record_dims(e.get("type"), records) is None:
                errors.append(f"{tag}: field {field} needs a fully sized [{records}] or [{records}][n] array"
                              + (", a scalar or a 1-D array" if records == 1 else "")
                              + f"; {global_name} is {e.get('type')!r}")
    return (errors + lint_table_aliases(tables, by_name) + lint_table_refs(tables, by_name)
            + lint_table_records_of(tables))


def lint_table_records_of(tables: list[dict]) -> list[str]:
    """`records_of`: the table whose record r this table's record r belongs to, as the game keeps a character's party
    state by its character id (Arterial's `party_member`). Errors: a value that names no other table of the registry, and
    a table with more records than the one it names."""
    errors: list[str] = []
    by_name = {t.get("name"): t for t in tables if isinstance(t.get("name"), str)}
    for t in tables:
        if "records_of" not in t or not isinstance(t.get("name"), str):
            continue
        tag, other = f"table {t['name']!r}", t["records_of"]
        target = by_name.get(other) if isinstance(other, str) else None
        if target is None or other == t["name"]:
            errors.append(f"{tag}: records_of {other!r} must name another table of the registry")
        elif isinstance(t.get("records"), int) and isinstance(target.get("records"), int) \
                and t["records"] > target["records"]:
            errors.append(f"{tag}: has {t['records']} records, more than {other}'s {target['records']}, so not every "
                          f"record is one of {other}'s")
    return errors


REF_PICTURE = "picture"


def ref_target(value) -> str | None:
    """A reference's target table: the value itself, or a two-field reference's `table`."""
    return value.get("table") if isinstance(value, dict) else value


def split_refs(refs: dict) -> tuple[dict, dict]:
    """A table's references as the JSON writes them: field = target table under `refs`, and a two-field reference's
    `{ times, plus }` under `ref_keys`, and an offset reference's `{ offset }`."""
    targets = {field: ref_target(value) for field, value in refs.items()}
    keys = {field: ({"offset": value.get("offset")} if "offset" in value
                    else {"times": value.get("times"), "pairs": True} if value.get("pairs")
                    else {"bases": value.get("bases"), "plus": value.get("plus")} if "bases" in value
                    else {"times": value.get("times"), "plus": value.get("plus")}
                    | ({"any": value["any"]} if "any" in value else {}))
            for field, value in refs.items() if isinstance(value, dict)}
    return targets, keys


def lint_table_refs(tables: list[dict], globals_by_name: dict) -> list[str]:
    """The references (game data export spec §5.3). Errors: `refs` that is not a table of field = target, a field that
    is not a field of the table (an old name included: a reference names the current one), a target that is neither
    a table of the registry nor `picture`, and a field whose global is not an integer array, since an id is an
    integer. A two-field reference is `{ table, times, plus }`, its table a table of the registry, `times` a whole
    number above 0 and `plus` another integer field of the table, as wide as the field; a paired one is
    `{ table, times, pairs = true }`, its field a row of cells of an even width, each pair of elements one record. An
    offset reference is `{ table, offset }`, `offset` a whole number other than 0 that a value above 0 adds to name
    its record (a Himegari unit's summon flag is its stone's item id plus 1901)."""
    errors: list[str] = []
    names = {t.get("name") for t in tables if isinstance(t.get("name"), str)}
    for t in tables:
        name, refs = t.get("name"), t.get("refs")
        if refs is None or not isinstance(name, str):
            continue
        tag = f"table {name!r}"
        if not isinstance(refs, dict) or not all(isinstance(v, (str, dict)) for v in refs.values()):
            errors.append(f"{tag}: refs must be a table of field = the table its values name, {REF_PICTURE}, "
                          f"{{ table, times, plus }}, {{ table, times, pairs = true }} or {{ table, offset }}")
            continue
        fields = t.get("fields") if isinstance(t.get("fields"), dict) else {}

        def type_of(field: str) -> str:
            e = globals_by_name.get(fields[field])
            return str(e.get("type") or "") if e is not None else ""

        for field, value in refs.items():
            if field not in fields:
                errors.append(f"{tag}: refs names {field!r}, which is not a field of the table")
                continue
            target = ref_target(value)
            # `offset`: a value above 0 plus the offset names the record (a Himegari unit's summon flag less 1901 is its
            # summoning stone's item id, SALLY@0x189c).
            if isinstance(value, dict) and set(value) == {"table", "offset"}:
                offset = value["offset"]
                if not isinstance(offset, int) or isinstance(offset, bool) or offset == 0:
                    errors.append(f"{tag}: refs gives {field} offset {offset!r}, which must be a whole number other "
                                  f"than 0, what a value adds to name its record")
                if target == REF_PICTURE:
                    errors.append(f"{tag}: refs gives {field} an offset reference to {REF_PICTURE}, whose value is "
                                  f"the picture's own id")
            elif isinstance(value, dict):
                paired = set(value) == {"table", "times", "pairs"}
                based = set(value) == {"table", "bases", "plus"}
                if set(value) - {"any"} != {"table", "times", "plus"} and not paired and not based:
                    errors.append(f"{tag}: refs gives {field} a two-field reference, which must be "
                                  f"{{ table, times, plus }}, {{ table, times, plus, any }}, {{ table, bases, plus }}, "
                                  f"{{ table, times, pairs = true }} or {{ table, offset }}")
                    continue
                # `bases`: the values that name a record, each with the record its plus counts from (a Kamidori map
                # object's type 32 or 33 picks dig table 0 or 100, which its payload adds to).
                if based:
                    bases = value["bases"]
                    if not isinstance(bases, dict) or not bases or not all(
                            key.lstrip("-").isdigit() and isinstance(base, int) and not isinstance(base, bool)
                            and base >= 0 for key, base in bases.items()):
                        errors.append(f"{tag}: refs gives {field} bases {bases!r}, which must be a table of value = "
                                      f"the record a value's plus counts from, whole numbers")
                    value = {**value, "times": 1}
                # `any`: the plus value that names every record of the row (an item any card of a row may carry).
                if "any" in value and (not isinstance(value["any"], int) or isinstance(value["any"], bool)):
                    errors.append(f"{tag}: refs gives {field} any {value['any']!r}, which must be the whole number a "
                                  f"plus has when the value names every record of its row")
                if target == REF_PICTURE:
                    errors.append(f"{tag}: refs gives {field} a two-field reference to {REF_PICTURE}, which is one "
                                  f"cell")
                times = value["times"]
                if not isinstance(times, int) or isinstance(times, bool) or times < 1:
                    errors.append(f"{tag}: refs gives {field} times {times!r}, which must be a whole number above 0")
                plus = value.get("plus")
                if paired:
                    # Each pair of elements is one record: element 2k times `times`, plus element 2k + 1.
                    dims = record_dims(type_of(field), t.get("records")) if isinstance(t.get("records"), int) else None
                    if value["pairs"] is not True:
                        errors.append(f"{tag}: refs gives {field} pairs {value['pairs']!r}; a paired reference is "
                                      f"pairs = true")
                    elif dims is None or len(dims) != 2 or dims[1] % 2:
                        errors.append(f"{tag}: refs pairs {field}'s elements, so its global {fields[field]} must be a "
                                      f"row of cells of an even width ({type_of(field) or 'no type'})")
                elif not isinstance(plus, str) or plus not in fields or plus == field:
                    errors.append(f"{tag}: refs gives {field} plus {plus!r}, which must be another field of the table")
                else:
                    plus_type, field_type = type_of(plus), type_of(field)
                    if bank_of(plus_type) != "G" or plus_type.startswith("float"):
                        errors.append(f"{tag}: refs gives {field} plus {plus}, whose global {fields[plus]} is not an "
                                      f"integer array")
                    elif plus_type.partition("[")[2] != field_type.partition("[")[2]:
                        errors.append(f"{tag}: refs gives {field} plus {plus}, whose global {fields[plus]} is "
                                      f"{plus_type}, not as wide as {fields[field]} ({field_type})")
            if target != REF_PICTURE and target not in names:
                errors.append(f"{tag}: refs gives {field} the target {target!r}, which is neither a table of the "
                              f"registry nor {REF_PICTURE}")
            e = globals_by_name.get(fields[field])
            type_ = str(e.get("type") or "") if e is not None else ""
            if e is not None and (bank_of(type_) != "G" or type_.startswith("float")):
                errors.append(f"{tag}: refs names {field}, whose global {fields[field]} is not an integer array; an "
                              f"id is an integer")
    return errors


def lint_table_aliases(tables: list[dict], globals_by_name: dict) -> list[str]:
    """The old names of tables and fields (data patches plan §9.1). Errors: `aliases` that is not a list of lowercase
    identifiers, or an alias that is a table's name, a global's name or another table's alias; `field_aliases` that is
    not a table of old name = current field, an old name that is not a lowercase identifier or is a field of the table,
    and one that names no field of the table. A data file names a table or a field by either, so each names one."""
    errors: list[str] = []
    names = {t.get("name") for t in tables if isinstance(t.get("name"), str)}
    owners: dict[str, str] = {}
    for t in tables:
        name = t.get("name")
        if not isinstance(name, str):
            continue
        tag = f"table {name!r}"
        aliases = t.get("aliases")
        if aliases is not None:
            if not isinstance(aliases, list) or not all(isinstance(alias, str) for alias in aliases):
                errors.append(f"{tag}: aliases must be a list of names")
            else:
                for alias in aliases:
                    if not IDENTIFIER.fullmatch(alias):
                        errors.append(f"{tag}: alias {alias!r} must be a lowercase identifier")
                    elif alias in names:
                        errors.append(f"{tag}: alias {alias} is also the name of a table")
                    elif alias in globals_by_name:
                        errors.append(f"{tag}: alias {alias} is also the name of a global")
                    elif alias in owners:
                        errors.append(f"{tag}: alias {alias} is also an alias of table {owners[alias]!r}")
                    else:
                        owners[alias] = name
        field_aliases = t.get("field_aliases")
        if field_aliases is None:
            continue
        fields = t.get("fields") if isinstance(t.get("fields"), dict) else {}
        if not isinstance(field_aliases, dict) or not all(isinstance(v, str) for v in field_aliases.values()):
            errors.append(f"{tag}: field_aliases must be a table of old name = current field")
            continue
        for old, current in field_aliases.items():
            if not IDENTIFIER.fullmatch(old):
                errors.append(f"{tag}: old field name {old!r} must be a lowercase identifier")
            elif old in fields:
                errors.append(f"{tag}: old field name {old} is also the name of a field")
            elif current not in fields:
                errors.append(f"{tag}: old field name {old} names {current!r}, which is not a field of the table")
    return errors


# A global's value facets (game data export plan §5), as the JSON carries them.
VALUE_FACETS = ("values", "values_from", "bits_of", "scale", "offset", "unit", "none")
# An array's column facets, as the JSON carries them.
COLUMN_FACETS = ("columns_from", "columns_ref")
# A word for a value or a unit is a reader's: one line, short.
VALUE_WORD_LIMIT = 60
UNIT_LIMIT = 12


def lint_values(entries: dict) -> list[str]:
    """The value facets: `values` keys whole numbers and words one short line; `values_from` a one-dimensional string
    global and a whole first value; `bits_of` an integer global's mask over a global whose values have words or names,
    or over a list of strings whose element k names bit k;
    `scale` a positive number other than 1, `offset` a whole number other than 0, `unit` a short word, `none` a whole
    number that `values` gives no word to. Facets are for integer globals; a mask's bits name values, so it has no
    words, names, scale, offset or none of its own."""
    errors: list[str] = []
    by_name = {e["name"]: e for e in entries.values() if e.get("name")}
    for key, e in sorted(entries.items(), key=lambda item: str(item[0])):
        present = [facet for facet in VALUE_FACETS if facet in e]
        if not present:
            continue
        tag = e.get("name") or f"0x{e['_addr']:x}"
        if bank_of(e.get("type")) != "G" or (e.get("type") or "").startswith("float"):
            errors.append(f"{tag}: {', '.join(present)}: value facets are for integer globals")
            continue
        values = e.get("values")
        if values is not None:
            if not isinstance(values, dict) or not values:
                errors.append(f"{tag}: values must be a table of value = word")
            else:
                for value, word in values.items():
                    try:
                        int(value, 10)
                    except (TypeError, ValueError):
                        errors.append(f"{tag}: values: {value!r} is not a whole number")
                    if not isinstance(word, str) or not word.strip() or "\n" in word or len(word) > VALUE_WORD_LIMIT:
                        errors.append(f"{tag}: values: the word for {value} must be one line of at most "
                                      f"{VALUE_WORD_LIMIT} characters")
        source = e.get("values_from")
        if source is not None:
            names = by_name.get(source.get("global")) if isinstance(source, dict) else None
            if not isinstance(source, dict) or set(source) != {"global", "first"}:
                errors.append(f"{tag}: values_from must be {{ global, first }}: the string global whose element k "
                              f"names value first + k")
            elif names is None:
                errors.append(f"{tag}: values_from names {source['global']!r}, which the registry does not name")
            elif not re.fullmatch(r"string\[\d+\]", names.get("type") or ""):
                errors.append(f"{tag}: values_from {source['global']} is {names.get('type')}, not a list of strings")
            if isinstance(source, dict) and (not isinstance(source.get("first"), int)
                                             or isinstance(source.get("first"), bool)):
                errors.append(f"{tag}: values_from: first must be a whole number, the value element 0 names")
        bits = e.get("bits_of")
        if bits is not None:
            target = by_name.get(bits) if isinstance(bits, str) else None
            if target is None:
                errors.append(f"{tag}: bits_of names {bits!r}, which the registry does not name")
            elif re.fullmatch(r"string\[\d+\]", target.get("type") or ""):
                pass                                    # the game's names of the bits, element k naming bit k
            elif "values" not in target and "values_from" not in target:
                errors.append(f"{tag}: bits_of {bits}, whose values have no words or names, and which is no list "
                              f"of strings")
            clash = [facet for facet in ("values", "values_from", "scale", "offset", "none") if facet in e]
            if clash:
                errors.append(f"{tag}: a mask's bits name values, so it takes no {', '.join(clash)}")
        scale = e.get("scale")
        if scale is not None and (isinstance(scale, bool) or not isinstance(scale, (int, float)) or scale <= 0
                                  or scale == 1):
            errors.append(f"{tag}: scale must be a positive number other than 1, what the stored value is multiplied by")
        offset = e.get("offset")
        if offset is not None and (isinstance(offset, bool) or not isinstance(offset, int) or offset == 0):
            errors.append(f"{tag}: offset must be a whole number other than 0, what is added to the stored value")
        unit = e.get("unit")
        if unit is not None and (not isinstance(unit, str) or not unit.strip() or len(unit) > UNIT_LIMIT):
            errors.append(f"{tag}: unit must be a word of at most {UNIT_LIMIT} characters")
        none = e.get("none")
        if none is not None:
            if isinstance(none, bool) or not isinstance(none, int):
                errors.append(f"{tag}: none must be a whole number, the stored value that means none")
            elif isinstance(values, dict) and str(none) in values:
                errors.append(f"{tag}: values gives a word to {none}, which none says means none; give one")
    return errors


def lint_columns(entries: dict, tables: list[dict]) -> list[str]:
    """The column facets: `columns_from` a one-dimensional string global and a whole first column, `columns_ref` a
    table of the registry, and one of them at most; both are for integer arrays, whose columns they name."""
    errors: list[str] = []
    by_name = {e["name"]: e for e in entries.values() if e.get("name")}
    table_names = {t.get("name") for t in tables}
    for key, e in sorted(entries.items(), key=lambda item: str(item[0])):
        present = [facet for facet in COLUMN_FACETS if facet in e]
        if not present:
            continue
        tag = e.get("name") or f"0x{e['_addr']:x}"
        if not re.fullmatch(r"int(\[[^\]]*\])+", e.get("type") or ""):
            errors.append(f"{tag}: {', '.join(present)}: column facets are for integer arrays")
            continue
        if len(present) > 1:
            errors.append(f"{tag}: columns_from and columns_ref both name its columns; give one")
        source = e.get("columns_from")
        if source is not None:
            names = by_name.get(source.get("global")) if isinstance(source, dict) else None
            if not isinstance(source, dict) or set(source) != {"global", "first"}:
                errors.append(f"{tag}: columns_from must be {{ global, first }}: the string global whose element k "
                              f"names column first + k")
            elif names is None:
                errors.append(f"{tag}: columns_from names {source['global']!r}, which the registry does not name")
            elif not re.fullmatch(r"string\[\d+\]", names.get("type") or ""):
                errors.append(f"{tag}: columns_from {source['global']} is {names.get('type')}, not a list of strings")
            if isinstance(source, dict) and (not isinstance(source.get("first"), int)
                                             or isinstance(source.get("first"), bool)):
                errors.append(f"{tag}: columns_from: first must be a whole number, the column element 0 names")
        ref = e.get("columns_ref")
        if ref is not None and (not isinstance(ref, str) or ref not in table_names):
            errors.append(f"{tag}: columns_ref names {ref!r}, which is no table of the registry")
    return errors


def column_facets_line(e: dict) -> str:
    """The reference doc's line for an array's column facets, or ""."""
    parts = []
    if source := e.get("columns_from"):
        parts.append(f"Column names: `{source['global']}`, element 0 naming column {source['first']}.")
    if ref := e.get("columns_ref"):
        parts.append(f"Column k is record k of `{ref}`.")
    return " ".join(parts)


def value_facets_line(e: dict) -> str:
    """The reference doc's line for a global's value facets, or ""."""
    parts = []
    if values := e.get("values"):
        parts.append("Values: " + ", ".join(f"{value} {word}" for value, word in
                                            sorted(values.items(), key=lambda item: int(item[0]))) + ".")
    if source := e.get("values_from"):
        parts.append(f"Names: `{source['global']}`, element 0 naming {source['first']}.")
    if bits := e.get("bits_of"):
        parts.append(f"Bits: bit k stands for `{bits}` value k.")
    if "scale" in e or "offset" in e or "unit" in e:
        shown = "the value" + (f" × {e['scale']:g}" if "scale" in e else "") \
            + (f" {'+' if e['offset'] > 0 else '−'} {abs(e['offset'])}" if "offset" in e else "")
        parts.append(f"Shown as {shown}" + (f", in {e['unit']}" if "unit" in e else "") + ".")
    if "none" in e:
        parts.append(f"{e['none']} means none.")
    return " ".join(parts)


def lint_aliases(entries: dict) -> list[str]:
    """Errors: an `aliases` value that is not a list of lowercase identifiers, and an alias that is also a global's
    name or another global's alias. A base mod exports a global's aliases beside its name (Lua scripting spec §9.1),
    so each must name one cell."""
    errors: list[str] = []
    names = {e["name"]: e for e in entries.values() if e.get("name")}
    owners: dict[str, str] = {}
    for e in entries.values():
        aliases = e.get("aliases")
        if aliases is None:
            continue
        tag = e.get("name") or f"0x{e['_addr']:x}"
        if not e.get("name"):
            errors.append(f"{tag}: aliases need the global's own name")
            continue
        if not isinstance(aliases, list) or not all(isinstance(alias, str) for alias in aliases):
            errors.append(f"{tag}: aliases must be a list of names")
            continue
        for alias in aliases:
            if not IDENTIFIER.fullmatch(alias):
                errors.append(f"{tag}: alias {alias!r} must be a lowercase identifier")
            elif alias in names:
                errors.append(f"{tag}: alias {alias} is also the name of a global")
            elif alias in owners:
                errors.append(f"{tag}: alias {alias} is also an alias of {owners[alias]}")
            else:
                owners[alias] = tag
    return errors


def overlap_warnings(entries: dict) -> list[str]:
    """Fully sized arrays of one bank whose cells overlap. The OMS decompiler names a cell by the smallest
    array containing it (spec §11.3), so a size error here misnames cells, though it never changes bytes."""
    spans = collections.defaultdict(list)
    for key, e in entries.items():
        bank, addr = key if isinstance(key, tuple) else (bank_of(e.get("type")), key)
        if (extent := extent_of(e.get("type"))) is not None:
            spans[bank].append((addr, addr + extent, e.get("name") or "?"))
    warnings = []
    for bank, arrays in sorted(spans.items()):
        arrays.sort()
        for i, (start, end, name) in enumerate(arrays):
            for other_start, other_end, other in arrays[i + 1:]:
                if other_start >= end:
                    break
                warnings.append(f"{cell_key(bank, start)}: array {name} [0x{start:x}, 0x{end:x}) overlaps "
                                f"{other} [0x{other_start:x}, 0x{other_end:x})")
    return warnings


GEN_JSON_META = {"generated_from": f"vm-map/{TOML_DEFAULT.name}",
                 "note": "DO NOT EDIT -- generated by tools/globals_build.py --build"}


def load_auto(path) -> dict:
    try:
        return json.loads(Path(path).read_text(encoding="utf-8"))
    except Exception:
        return {}


def _auto_category(e: dict) -> str:
    """Best-effort category for an auto shape-map entry (unreliable; curation overrides)."""
    kind = e.get("kind") or ""
    if kind == "record-table":
        return "data-table"
    if kind == "array-1d":
        return "data-table"
    lbl = e.get("label") or ""
    if lbl.startswith("string-table"):
        return "string-table"
    return "unknown"


def merge(curated: dict, auto: dict) -> Registry:
    out = Registry()
    for addr_s, e in auto.get("globals", {}).items():
        if not e.get("label"):
            continue
        addr = int(addr_s, 16)
        bank = bank_of(e.get("type"))
        out[(bank, addr)] = {"address": f"0x{addr:x}", "bank": bank, "name": None,
                             "category": _auto_category(e),
                             "type": e.get("type"), "value_domain": None, "usage": e.get("label"),
                             "source": "auto-shape", "confidence": e.get("confidence") or "low",
                             "depends_on": [], "provenance": "auto"}
    for key, e in curated.items():
        bank, addr = key if isinstance(key, tuple) else (bank_of(e.get("type")), key)
        out[(bank, addr)] = {"address": f"0x{addr:x}", "bank": bank, "name": e.get("name"),
                     "category": e.get("category", "unknown"), "type": e.get("type"),
                     "value_domain": e.get("value_domain"), "usage": e.get("usage", ""),
                     "source": e.get("source", "inference"), "confidence": e.get("confidence", "low"),
                     "depends_on": [f"0x{_parse_addr(d):x}" for d in e.get("depends_on", [])],
                     "provenance": "curated"}
        if e.get("aliases"):
            out[(bank, addr)]["aliases"] = list(e["aliases"])
        if e.get("columns"):
            out[(bank, addr)]["columns"] = {
                str(column): name for column, name in e["columns"].items()
            }
        for facet in VALUE_FACETS + COLUMN_FACETS:
            if facet in e:
                out[(bank, addr)][facet] = e[facet]
    return out


def emit_json(merged: Registry, meta: dict, tables: list[dict] = (), scopes: list[dict] = ()) -> str:
    """`globals` is the legacy address-keyed view (the integer bank wins where a string global shares its
    address); `cells` keys every entry by bank and address (`G:0x0`, `GS:0x0`); `tables` lists the OMS
    tables in declaration order, and `scopes` the scopes that key some of them."""
    ordered = sorted(merged.items(), key=lambda item: (item[0][1], item[0][0] != "G"))
    legacy: dict[str, dict] = {}
    for _, e in ordered:
        legacy.setdefault(e["address"], e)
    out = {"meta": {**GEN_JSON_META, **meta},
           "globals": legacy,
           "cells": {cell_key(bank, addr): e for (bank, addr), e in sorted(merged.items())},
           "tables": [table_json(t) for t in tables],
           "scopes": [scope_json(s) for s in scopes]}
    return json.dumps(out, ensure_ascii=False, indent=2) + "\n"


def scope_json(s: dict) -> dict:
    return {"name": s["name"], "first": s["first"], "last": s["last"], "usage": s.get("usage", ""),
            "tables": dict(s["tables"])}


def table_json(t: dict) -> dict:
    """A table as the JSON lists it: its old names only when it has some."""
    out = {"name": t["name"], "records": t["records"], "built_by": t["built_by"], "usage": t.get("usage", ""),
           "fields": dict(t["fields"])}
    if t.get("aliases"):
        out["aliases"] = sorted(t["aliases"])
    if t.get("field_aliases"):
        out["field_aliases"] = dict(sorted(t["field_aliases"].items()))
    if t.get("records_of"):
        out["records_of"] = t["records_of"]
    if t.get("refs"):
        targets, keys = split_refs(t["refs"])
        out["refs"] = dict(sorted(targets.items()))
        if keys:
            out["ref_keys"] = dict(sorted(keys.items()))
    return out


def emit_reference_md(merged: Registry, tables: list[dict] = (), scopes: list[dict] = ()) -> str:
    source = f"vm-map/{TOML_DEFAULT.name}"
    L = [f"<!-- DO NOT EDIT -- generated from {source} by tools/globals_build.py --build -->",
         f"# Global Variable Reference — {paths.CONTEXT.profile_id} (generated)", "",
         f"{len(merged)} globals ({sum(1 for e in merged.values() if e['provenance']=='curated')} curated, "
         f"{sum(1 for e in merged.values() if e['provenance']=='auto')} auto shape-inferred). "
         f"Source of truth: `{source}`.", ""]
    if tables:
        by_name = {e["name"]: e for e in merged.values() if e.get("name")}
        L += ["## Tables", "",
              "Open Maid Script tables (design spec §5.3): `table[record].field` is the field's array element.", ""]
        for t in tables:
            L += [f"### {t['name']}[{t['records']}]", "", f"Built by: `{t['built_by']}`.", ""]
            if t.get("aliases"):
                L += ["Formerly " + ", ".join(f"`{alias}`" for alias in sorted(t["aliases"])) + ".", ""]
            if t.get("usage"):
                L += [t["usage"], ""]
            if t.get("records_of"):
                L += [f"Record r is record r of `{t['records_of']}`.", ""]
            refs = t.get("refs") or {}
            L += (["| field | global | address | type | refers to |", "|---|---|---|---|---|"] if refs
                  else ["| field | global | address | type |", "|---|---|---|---|"])
            old_fields: dict[str, list[str]] = collections.defaultdict(list)
            for old, current in sorted((t.get("field_aliases") or {}).items()):
                old_fields[current].append(old)
            for field, global_name in t["fields"].items():
                e = by_name[global_name]
                address = e["address"] if e["bank"] == "G" else f"{e['bank']} {e['address']}"
                shown = field + (f" (formerly {', '.join(old_fields[field])})" if old_fields.get(field) else "")
                row = f"| {shown} | {global_name} | `{address}` | `{e['type']}` |"
                if refs:
                    value = refs.get(field)
                    target = ref_target(value)
                    key = ("" if not isinstance(value, dict)
                           else f" (value {'+' if value['offset'] > 0 else '−'} {abs(value['offset'])})"
                           if "offset" in value
                           else f" (element 2k × {value['times']} + element 2k + 1)" if value.get("pairs")
                           else " (" + ", ".join(f"{v} from {base}" for v, base in value["bases"].items())
                                + f", + {value['plus']}; another value names none)" if "bases" in value
                           else f" (× {value['times']} + {value['plus']}; a {value['plus']} of {value['any']} names "
                                f"every record from × {value['times']} to × {value['times']} + {value['times'] - 1})"
                           if "any" in value
                           else f" (× {value['times']} + {value['plus']})")
                    row += " pictures |" if target == REF_PICTURE else f" `{target}`{key} |" if target else " |"
                L.append(row)
            L.append("")
        for s in scopes:
            L += [f"### Scope {s['name']}", "",
                  f"Ids {s['first']} to {s['last']}. A data file reaches each table below as "
                  f"`{s['name']}.<id>.<name>.<record>` and not by its own name (data patches spec §6.2).", ""]
            if s.get("usage"):
                L += [s["usage"], ""]
            L += ["| name | table |", "|---|---|"]
            for short, table in s["tables"].items():
                L.append(f"| {short} | {table} |")
            L.append("")
    by_cat = collections.defaultdict(list)
    for key, e in merged.items():
        by_cat[e["category"]].append(key)
    for cat in sorted(by_cat):
        L += [f"## {cat}", "", "| address | name | conf | source | usage |", "|---|---|---|---|---|"]
        # curated first, then by address
        for key in sorted(by_cat[cat], key=lambda k: (merged[k]["provenance"] != "curated", k[1], k[0])):
            e = merged[key]
            name = e["name"] or "—"
            if aliases := e.get("aliases"):
                name += " (formerly " + ", ".join(aliases) + ")"
            usage = (e["usage"] or "").replace("|", "\\|").replace("\n", " ")
            if columns := e.get("columns"):
                mapping = ", ".join(
                    f"{column}={column_name}"
                    for column, column_name in sorted(
                        columns.items(), key=lambda item: int(item[0])
                    )
                )
                usage += f" Columns: {mapping}."
            if facets := column_facets_line(e):
                usage += " " + facets.replace("|", "\\|")
            if facets := value_facets_line(e):
                usage += " " + facets.replace("|", "\\|")
            address = e["address"] if e["bank"] == "G" else f"{e['bank']} {e['address']}"
            L.append(f"| `{address}` | {name} | {e['confidence']} | {e['source']} | {usage} |")
        L.append("")
    return "\n".join(L).rstrip("\n") + "\n"


def _linted_merge(tp: Path, auto_map: Path | None) -> tuple[Registry, dict, list[dict], list[dict]] | None:
    """Lint the registry against itself and the auto map (None: none), then merge: the globals, the meta, the tables
    and the scopes. None (errors printed) when it does not lint."""
    curated, meta = load_toml(tp)
    auto = load_auto(auto_map) if auto_map is not None else {}
    all_addrs = curated.addresses() | {int(a, 16) for a in auto.get("globals", {})}
    errors, warnings = lint(curated, all_addrs)
    tables = load_tables(tp)
    scopes = load_scopes(tp)
    errors += lint_tables(tables, curated)
    errors += lint_scopes(scopes, tables, curated)
    errors += lint_aliases(curated)
    errors += lint_values(curated)
    errors += lint_columns(curated, tables)
    for m in warnings: print("warn:", m)
    if errors:
        for m in errors: print("error:", m)
        return None
    return merge(curated, auto), meta, tables, scopes


def _shown(path: Path) -> str:
    """A path as messages show it: repository-relative when it lies in the repository."""
    return path.relative_to(paths.REPO).as_posix() if path.is_relative_to(paths.REPO) else str(path)


def check(tp: Path, auto_map: Path = AUTO_MAP, reference_doc: Path = REFERENCE_DOC) -> int:
    """Lint, then exit nonzero if the tracked reference doc differs from what --build would write. The doc merges
    the auto map, a workspace-derived input (global_map.py), so a missing map is an error, not an empty merge."""
    if not Path(auto_map).is_file():
        print(f"error: auto map {auto_map} not found; build it with global_map.py (the reference doc merges it)")
        return 1
    prepared = _linted_merge(tp, auto_map)
    if prepared is None:
        return 1
    merged, _, tables, scopes = prepared
    expected = emit_reference_md(merged, tables, scopes)
    current = reference_doc.read_text(encoding="utf-8") if reference_doc.is_file() else None
    if current != expected:
        print(f"stale: {_shown(reference_doc)}; run globals_build.py "
              f"--game {paths.CONTEXT.profile_id} --build")
        return 1
    print(f"check: {_shown(reference_doc)} is current ({len(merged)} globals)")
    return 0


def build_json(tp: Path, auto_map: Path = AUTO_MAP, output: Path | None = None) -> int:
    """Lint and merge, then write only the profile's globals.json, which Open Maid Script loads as `<game>.globals`.
    The tracked reference doc is left alone. The core level runs this for every profile. A checkout has no auto map
    (a workspace input), so its JSON holds the curated globals, the only ones the OMS modules name; --build, which also
    writes the doc, would shorten the doc there."""
    prepared = _linted_merge(tp, auto_map)
    if prepared is None:
        return 1
    merged, meta, tables, scopes = prepared
    output = output or paths.BUILD / "globals.json"
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(emit_json(merged, meta, tables, scopes), encoding="utf-8", newline="\n")
    without_map = "" if Path(auto_map).is_file() else f" (no auto map at {_shown(Path(auto_map))}: curated globals only)"
    print(f"build-json: {len(merged)} globals, {len(tables)} tables -> {_shown(output)}{without_map}")
    return 0


def curated_json(tp: Path) -> str | None:
    """The registry's globals.json without the auto map: its curated globals, tables, references and scopes. It is
    the reference bundle's games/<game>/globals.json (tools/reference_bundle.py; game data export spec §8.1): the
    inferred globals have no names, and CI has no game files. It names the registry it was given, whatever game the
    process was started for. None (errors printed) when the registry does not lint."""
    prepared = _linted_merge(tp, None)
    if prepared is None:
        return None
    merged, meta, tables, scopes = prepared
    meta = {**meta, "generated_from": f"vm-map/{tp.name}",
            "note": "DO NOT EDIT -- generated by tools/reference_bundle.py from the registry alone (no auto map)"}
    return emit_json(merged, meta, tables, scopes)


def build(tp: Path, auto_map: Path = AUTO_MAP) -> int:
    prepared = _linted_merge(tp, auto_map)
    if prepared is None:
        return 1
    merged, meta, tables, scopes = prepared
    paths.BUILD.mkdir(parents=True, exist_ok=True)
    (paths.BUILD / "globals.json").write_text(emit_json(merged, meta, tables, scopes), encoding="utf-8",
                                               newline="\n")
    REFERENCE_DOC.write_text(emit_reference_md(merged, tables, scopes), encoding="utf-8", newline="\n")
    print(f"build: {len(merged)} globals, {len(tables)} tables -> "
          f"{(paths.BUILD / 'globals.json').relative_to(paths.REPO)}, {REFERENCE_DOC.relative_to(paths.REPO)}")
    return 0


def main(argv=None):
    ap = argparse.ArgumentParser()
    ap.add_argument("--build", action="store_true")
    ap.add_argument("--build-json", action="store_true",
                    help="lint, then write only build/games/<game>/globals.json (no reference doc; works without the "
                         "auto map)")
    ap.add_argument("--lint", action="store_true")
    ap.add_argument("--check", action="store_true",
                    help="lint, then fail if the profile's reference doc differs from what --build would write")
    ap.add_argument("--toml", default=str(TOML_DEFAULT))
    ap.add_argument("--auto-map", default=str(AUTO_MAP),
                    help="profile-specific global-var-map.json input")
    args = ap.parse_args(argv)
    tp = Path(args.toml)
    if args.lint:
        entries, _ = load_toml(tp)
        errors, warnings = lint(entries, entries.addresses())
        errors += lint_tables(load_tables(tp), entries)
        errors += lint_scopes(load_scopes(tp), load_tables(tp), entries)
        errors += lint_aliases(entries)
        for m in warnings: print("warn:", m)
        for m in errors: print("error:", m)
        print(f"lint: {len(errors)} errors, {len(warnings)} warnings")
        return 1 if errors else 0
    if args.check:
        return check(tp, Path(args.auto_map))
    if args.build:
        return build(tp, Path(args.auto_map))
    if args.build_json:
        return build_json(tp, Path(args.auto_map))
    ap.error("no action (expected --build/--build-json/--check/--lint)")


if __name__ == "__main__":
    sys.exit(main())
