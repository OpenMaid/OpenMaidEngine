#!/usr/bin/env python3
"""Lint and build a game's entity declarations (game data export spec §5): `vm-map/entities-<game>.toml`, which says
which record tables are kinds the project site shows, how a record of each is named, described and pictured, what of it
is published, which loaders build the tables a game builds on demand, and which script relations to look for.

The declarations are checked against the repository alone: the game's registry (its `[[table]]`, `[[scope]]` and named
globals), its patch points (`vm-map/patch-points-<game>.toml`) and its profile manifest's saved prefix (a field whose
array a save keeps is the player's state, not data, so no kind publishes it). Which scripts write a table is a fact
of the installed game, so that is checked when the exporter runs (export spec §5.5), not here.

  --build   lint, then write build/games/<game>/entities.json, which the exporter reads
  --check   lint only
  --game ID the game (paths.py's selection: OME_PROFILE, else himegari)
  --all     every game with declarations instead

Names: a script is named without the `$n$` prefix of a loose update or append and without its extension, in upper case
(`CGINIT`, not `$1$CGINIT.BIN`); a pattern may hold `*`. A field is named as the record map names it, a column after a
dot by its name (`graphic.face_CA_AA`), or by its number where it has no name."""
from __future__ import annotations

import argparse
import fnmatch
import hashlib
import json
import os
import re
import sys
import tomllib
from pathlib import Path

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import paths  # noqa: E402
import globals_build  # noqa: E402
import base_mod_build  # noqa: E402

IDENTIFIER = re.compile(r"[a-z_][a-z0-9_]*")
SCRIPT = re.compile(r"[A-Z0-9_]+")
SCRIPT_PATTERN = re.compile(r"[A-Z0-9_*]*[A-Z0-9_][A-Z0-9_*]*")
# The site's second path segment under a game is a kind's id, except these (site design §4): the tooltip files and
# the database's search page (site plan §9.2).
RESERVED_KINDS = {"tooltips", "search"}
# A kind's id names its table in the export's database, beside these (game data export spec §7.2); Ome.Export refuses
# them too (GameDataExporter.FixedTables).
EXPORT_TABLES = {"build_info", "kind", "field", "field_group", "group_label", "note", "text_block", "text", "ref",
                 "qualifier", "referenced_as", "mark",
                 "relation", "picture", "cell", "snippet", "script_use", "loaded", "loaded_field", "loaded_part",
                 "label", "value_format", "value_name"}
KEY = "key"
STEP = "step"
EVERY = "*"

EXPORT_KEYS = {"boot", "builders", "excluded_builders", "derived", "text_scripts", "explicit_pictures",
               "text_overrides"}
# A language tag as a language package declares one (`en`, `pt-BR`).
LANGUAGE_TAG = re.compile(r"[a-z]{2,3}(-[A-Za-z0-9]{2,8})*")
KIND_KEYS = {"id", "aliases", "table", "exists", "skip", "key_range", "title", "titles", "text", "fields", "filters",
             "pictures",
             "categories",
             "description", "group", "notes", "labels", "text_blocks", "qualifiers", "referenced_as", "about",
             "joins", "marks"}
# A kind's `about` is the line the site shows on its card and its list: one line, a reader's.
ABOUT_LIMIT = 160
REFERENCED_AS_KEYS = {"heading", "details", "parts"}
QUALIFIER_LIMIT = 3
DETAIL_LIMIT = 4
DESCRIPTION_KEYS = {"name", "script", "seed"}
GROUP_KEYS = {"name", "fields", "labels", "row_label"}
SHEET_KEYS = {"sheet", "first", "columns", "origin", "step", "size"}
# A sheet's optional keys: the field numbering its cell (else the record's id), the colour it is loaded with as its key,
# and its last number (export plan §5).
SHEET_OPTIONAL_KEYS = {"field", "key", "last"}
# A picture sheet's catalog record, with its extension (SO009.AGF); an append's own picture is named with its pack's
# prefix ($1$SO016A.AGF, Himegari's append icons).
SHEET = re.compile(r"(\$[0-9]+\$)?[A-Z0-9_]+\.[A-Z0-9]+")
LOADER_KEYS = {"scope", "keys", "key_range", "steps", "skip", "script", "seed", "read_at", "builders", "tables"}
# A picture another kind's entity has (export plan §5): `from` the kind, `by` its reference field, `picture` its name.
BORROWED_KEYS = {"from", "by", "picture"}
LOADED_TABLE_KEYS = {"table", "exists", "fields", "text", "literals", "notes", "labels", "parts", "where", "joins"}
PART_KEYS = {"name", "field", "global", "same_as", "note"}
# One integer list: a part's global, which the row's field indexes.
LIST_TYPE = re.compile(r"int\[(\d+)\]")
SEED_KEYS = {"global", "index", "value"}
LITERAL_KEYS = {"record", "field", "script", "offset"}
# A note is a line for a reader, shown when they point at a detail's label.
NOTE_LIMIT = 300
# A label is a reader's name for a field or group whose own name reads as the game stores it.
LABEL_LIMIT = 40
RELATION_KEYS = {"id", "negative", "call", "kind", "key", "count", "where", "exclude"}
# The shapes of a relation's globals (export plan §4.5): its key one integer cell or a list, whose first cell it
# reads; its count and the globals of its `where` one integer cell each.
ONE_CELL = re.compile(r"int")
ONE_LIST = re.compile(r"int(\[\d+\])?")


def entities_source(profile_id: str) -> Path:
    return paths.VM_MAP / f"entities-{profile_id}.toml"


def entities_output(profile_id: str) -> Path:
    return paths.SHARED_BUILD / "games" / profile_id / "entities.json"


def script_name(raw: str) -> str:
    """A script's name as the declarations write it: no `$n$` prefix, no extension, upper case."""
    name = re.sub(r"^\$\d+\$", "", raw)
    name = re.sub(r"\.bin$", "", name, flags=re.IGNORECASE)
    return name.upper()


def profiles_with_entities() -> list[str]:
    return [profile_id for profile_id in paths.available_profiles() if entities_source(profile_id).is_file()]


class Game:
    """What the declarations are checked against: the registry's tables, scopes and named globals, the patch points'
    names, and the saved prefix of each bank."""

    def __init__(self, registry: Path, points: Path, manifest: Path):
        curated, _ = globals_build.load_toml(registry)
        self.globals = {e["name"]: e for e in curated.values() if e.get("name")}
        self.tables = {t["name"]: t for t in globals_build.load_tables(registry) if isinstance(t.get("name"), str)}
        self.scopes = {s["name"]: s for s in globals_build.load_scopes(registry) if isinstance(s.get("name"), str)}
        self.points: set[str] = set()
        if points.is_file():
            data = tomllib.loads(points.read_text(encoding="utf-8"))
            self.points = {p["name"] for p in data.get("point", []) if isinstance(p.get("name"), str)}
        self.prefixes, self.prefix_error = base_mod_build.saved_prefixes(manifest)


def bank(type_: str) -> str:
    return "GS" if type_.startswith("string") else "GF" if type_.startswith("float") else "G"


class Field:
    """A field reference resolved against a table: its global, its kind (integer, float, string), whether it names one
    cell of each record (a 1-D field or a column), and whether a save keeps it."""

    def __init__(self, global_name: str, entry: dict, records: int, column: int | None, prefixes: dict | None):
        type_ = str(entry.get("type") or "")
        self.global_name = global_name
        self.column = column
        self.kind = {"GS": "string", "GF": "float", "G": "integer"}[bank(type_)]
        dims = globals_build.record_dims(type_, records) or [records]
        # The global whose values its bits stand for, or whose strings name them (globals_build's value facets).
        self.bits_of = entry.get("bits_of")
        self.single = len(dims) == 1 or column is not None
        width = dims[1] if len(dims) == 2 else 1
        self.width = 1 if self.single else width
        end = globals_build._parse_addr(entry["address"]) + records * width
        prefix = (prefixes or {}).get(bank(type_))
        self.state = prefix is not None and end <= prefix


def resolve_field(game: Game, table: dict, ref, where: str, errors: list[str]) -> Field | None:
    """`field` or `field.column` of `table`, or None (the error appended). A table a kind's view joins (`_joins`,
    see kind_view) gives its fields as `table.field` and `table.field.column`."""
    if not isinstance(ref, str):
        errors.append(f"{where}: {ref!r} is not a field name")
        return None
    field, _, column = ref.partition(".")
    fields = table.get("fields") or {}
    joined = (table.get("_joins") or {}).get(field)
    if field not in fields and joined is not None:
        return resolve_field(game, joined, column, where, errors)
    if field not in fields:
        errors.append(f"{where}: {field!r} is not a field of table {table['name']}")
        return None
    global_name = fields[field]
    entry = game.globals.get(global_name)
    if entry is None:
        errors.append(f"{where}: field {field} names {global_name}, which the registry does not name")
        return None
    dims = globals_build.record_dims(entry.get("type"), table["records"]) or []
    index = None
    if column:
        if len(dims) != 2:
            errors.append(f"{where}: {field} has one cell for each record, so it has no column {column!r}")
            return None
        names = {name: int(i) for i, name in (entry.get("columns") or {}).items()}
        if column in names:
            index = names[column]
        elif column.isdigit() and int(column) < dims[1]:
            index = int(column)
            if str(index) in (entry.get("columns") or {}):
                errors.append(f"{where}: column {index} of {field} is named: write "
                              f"{field}.{entry['columns'][str(index)]}")
                return None
        else:
            errors.append(f"{where}: {field} has no column {column!r}")
            return None
    return Field(global_name, entry, table["records"], index, game.prefixes)


def joinable(table: dict, other: dict) -> bool:
    """Whether a kind of `table` may join `other` (export plan §5): `other`'s record r is the kind's entity r (its
    `records_of`), or `other` owns the kind's records, entity r being of its record r ÷ per (`table`'s `owned_by`)."""
    owner = table.get("owned_by")
    return other.get("records_of") == table.get("name") \
        or isinstance(owner, dict) and owner.get("table") == other.get("name")


def kind_view(game: Game, kind: dict) -> dict | None:
    """A kind's table as its published fields and groups see it: the registry table, with the tables the kind joins
    (`joins`, each a table whose `records_of` is the kind's, or the one that owns the kind's records) under `_joins`,
    so that `table.field` names a field of one. None when the kind names no table of the registry."""
    table = game.tables.get(kind.get("table"))
    if table is None:
        return None
    joins = kind.get("joins") if isinstance(kind.get("joins"), list) else []
    return {**table, "_joins": {name: game.tables[name] for name in joins
                                if isinstance(name, str) and name in game.tables
                                and joinable(table, game.tables[name])}}


def reference_of(table: dict, ref: str) -> tuple[dict, str, object] | None:
    """The registry table a published field reference is of (a joined one's, for `table.field`), the field's name
    there, and its `[table.refs]` entry (a table's name, `picture`, or a two-field reference); None for a field that
    refers to nothing."""
    base, _, rest = ref.partition(".")
    joined = (table.get("_joins") or {}).get(base)
    if base not in (table.get("fields") or {}) and joined is not None:
        return reference_of(joined, rest)
    value = (table.get("refs") or {}).get(base) if isinstance(table.get("refs"), dict) else None
    return (table, base, value) if value is not None else None


def check_joins(game: Game, kind: dict, table: dict, where: str, errors: list[str]) -> None:
    """`joins` (export plan §5): the tables whose record r belongs to the kind's entity r, which the registry says by
    the table's `records_of`, or the table that owns the kind's records, entity r being of its record r ÷ per, which
    the registry says by the kind's table's `owned_by` (a Verita title is character r ÷ 20's). The kind publishes
    their fields as its own, written `table.field`."""
    joins = kind.get("joins", [])
    if not isinstance(joins, list) or not all(isinstance(name, str) for name in joins):
        errors.append(f"{where}: joins must list tables of the registry")
        return
    for name in joins:
        other = game.tables.get(name)
        if other is None:
            errors.append(f"{where}: joins {name!r}, which is not a [[table]] of the registry")
        elif not joinable(table, other):
            errors.append(f"{where}: joins {name}, whose records are not {table.get('name')}'s: the registry's "
                          f"table gives no records_of = {table.get('name')!r}, and {table.get('name')} is not "
                          f"owned_by it")
        if name in (table.get("fields") or {}):
            errors.append(f"{where}: joins {name}, which is also a field of table {table.get('name')}")
        if joins.count(name) > 1:
            errors.append(f"{where}: joins {name} twice")


def is_size(value) -> bool:
    return isinstance(value, int) and not isinstance(value, bool) and value > 0


def check_borrowed_pictures(game: Game, kinds: list, source: str, errors: list[str]) -> None:
    """A picture `{ from, by, picture }` (export plan §5) is picture `picture` of the first entity of kind `from`, by
    id, whose published reference `by` names the entity: a Verita party unit shows its character's joining title's
    chibi, the title being character id ÷ 20's and the character naming its unit. `from` is another declared kind,
    `by` one of its published fields that refers to this kind's table, and `picture` one of its own pictures, which is
    not borrowed itself."""
    by_id = {kind.get("id"): kind for kind in kinds if isinstance(kind, dict)}
    for kind in by_id.values():
        pictures = kind.get("pictures") if isinstance(kind.get("pictures"), dict) else {}
        for name, ref in pictures.items():
            if not (isinstance(ref, dict) and "from" in ref):
                continue
            where = f"{source}: kind {kind.get('id')!r}: picture {name}"
            other = by_id.get(ref.get("from"))
            if other is None or other is kind:
                errors.append(f"{where}: from {ref.get('from')!r} must name another declared kind")
                continue
            view = kind_view(game, other)
            by = ref.get("by")
            if by not in (other.get("fields") or []):
                errors.append(f"{where}: by {by!r} must be one of kind {other.get('id')}'s published fields")
            elif view is not None:
                reference = reference_of(view, by)
                target = globals_build.ref_target(reference[2]) if reference else None
                if target != kind.get("table") or isinstance(reference[2], dict):
                    errors.append(f"{where}: by {by} must be a reference to table {kind.get('table')} named by its "
                                  f"value alone; it refers to {target or 'nothing'}")
            source_pictures = other.get("pictures") if isinstance(other.get("pictures"), dict) else {}
            target_picture = source_pictures.get(ref.get("picture"))
            if target_picture is None:
                errors.append(f"{where}: picture {ref.get('picture')!r} is not one of kind {other.get('id')}'s pictures")
            elif isinstance(target_picture, dict) and "from" in target_picture:
                errors.append(f"{where}: picture {ref.get('picture')!r} of kind {other.get('id')} is borrowed itself")


def unknown_keys(entry: dict, allowed: set[str], where: str, errors: list[str]) -> None:
    for key in sorted(set(entry) - allowed):
        errors.append(f"{where}: unknown key {key!r}")


def refs_of(table: dict) -> dict:
    """A registry table's references, field = the table its values name (or `picture`), a two-field reference's
    included."""
    refs = table.get("refs")
    return globals_build.split_refs(refs)[0] if isinstance(refs, dict) else {}


def check_scripts(value, where: str, errors: list[str], patterns: bool) -> list[str]:
    """A list of script names (or patterns); the errors appended."""
    if not isinstance(value, list) or not all(isinstance(v, str) for v in value):
        errors.append(f"{where} must be a list of script names")
        return []
    form = SCRIPT_PATTERN if patterns else SCRIPT
    for name in value:
        if not form.fullmatch(name):
            errors.append(f"{where}: {name!r} must be a script name in upper case, without its `$n$` prefix or its "
                          f"extension" + (", a `*` matching any run of characters" if patterns else ""))
    return value


def check_picture_patterns(value, where: str, errors: list[str]) -> None:
    """Pictures named by their catalog names (or patterns of them), as [export] explicit_pictures names them."""
    if not isinstance(value, list) or not all(isinstance(v, str) for v in value):
        errors.append(f"{where} must be a list of picture names")
        return
    for name in value:
        if not SCRIPT_PATTERN.fullmatch(name):
            errors.append(f"{where}: {name!r} must be a picture's catalog name in upper case, without its `$n$` prefix "
                          "or its extension, a `*` matching any run of characters")
    for name in sorted({name for name in value if value.count(name) > 1}):
        errors.append(f"{where}: {name} is named twice")


def seed_entries(seed) -> list[dict]:
    """A seed as the list the exporter reads, in order: `{ global = value }` is one cell each, and the array form's
    entries (`{ global, index, value }`) are as written."""
    if isinstance(seed, dict):
        return [{"global": name, "index": [], "value": value} for name, value in seed.items()]
    return [{"global": entry["global"], "index": list(entry.get("index") or []), "value": entry["value"]}
            for entry in seed or []]


def check_seed(game: Game, seed, where: str, errors: list[str], key_range: list[int] | None = None,
               steps: int | None = None, indexed: bool = False) -> None:
    """A seed: a table of global = "key" or a whole number, each an integer cell; or, where `indexed` (a loader's), an
    array of { global, index, value } applied in order, whose index names a cell of an integer array, one term for each
    dimension: a whole number, "key" (the entity's id, which the loader's key_range must keep inside the dimension),
    "step" (the run's step, which its steps must) or "*" (every index of the dimension)."""
    if seed is None:
        return
    if isinstance(seed, list) and indexed:
        if not seed:
            errors.append(f"{where}: seed must list at least one entry")
        for entry in seed:
            if not isinstance(entry, dict) or not {"global", "value"} <= set(entry):
                errors.append(f"{where}: each seed entry must be {{ global = NAME, index = [...], value = ... }}")
                continue
            unknown_keys(entry, SEED_KEYS, f"{where}: seed", errors)
            check_seed_cell(game, entry["global"], entry.get("index") or [], entry["value"], where, errors,
                            key_range, steps)
        return
    if not isinstance(seed, dict) or not seed:
        errors.append(f"{where}: seed must be a table of global = \"{KEY}\" or a whole number"
                      + (", or an array of { global, index, value }" if indexed else ""))
        return
    for name, value in seed.items():
        check_seed_cell(game, name, [], value, where, errors, key_range, steps)


def check_seed_cell(game: Game, name, index, value, where: str, errors: list[str], key_range: list[int] | None,
                    steps: int | None) -> None:
    entry = game.globals.get(name) if isinstance(name, str) else None
    if not (value == KEY or isinstance(value, int) and not isinstance(value, bool)):
        errors.append(f"{where}: seed gives {name} {value!r}, not \"{KEY}\" or a whole number")
    if entry is None:
        errors.append(f"{where}: seed names {name!r}, which the registry does not name")
        return
    type_ = str(entry.get("type") or "int")
    if bank(type_) != "G":
        errors.append(f"{where}: seed names {name}, which is not an integer global")
        return
    dims = [] if re.fullmatch(r"int", type_) else globals_build.dims_of(type_)
    if dims is None:
        errors.append(f"{where}: seed names {name}, whose type {type_} is no fully sized array")
        return
    if not isinstance(index, list) or len(index) != len(dims):
        errors.append(f"{where}: seed {name} is {type_}, so its index must have {len(dims)} term(s)"
                      + (", none for one cell" if not dims else ""))
        return
    for term, size in zip(index, dims):
        if term == KEY:
            if key_range is None or key_range[1] >= size:
                errors.append(f"{where}: seed {name} is indexed by the key, which the loader's key_range must keep "
                              f"below {size}")
        elif term == STEP:
            if steps is None or steps > size:
                errors.append(f"{where}: seed {name} is indexed by the step, which the loader's steps must keep "
                              f"below {size}")
        elif term == EVERY:
            pass
        elif not (isinstance(term, int) and not isinstance(term, bool) and 0 <= term < size):
            errors.append(f"{where}: seed {name}'s index term {term!r} is not \"{KEY}\", \"{STEP}\", \"{EVERY}\" or a "
                          f"whole number below {size}")


def is_key_range(value) -> bool:
    """A `key_range`, a kind's or a loader's: [first, last], whole numbers with 0 <= first <= last."""
    return (isinstance(value, list) and len(value) == 2
            and all(isinstance(v, int) and not isinstance(v, bool) for v in value) and 0 <= value[0] <= value[1])


def check_skip(game: Game, table: dict, skip, where: str, errors: list[str]) -> None:
    if skip is None:
        return
    if not isinstance(skip, dict) or not skip:
        errors.append(f"{where}: skip must be a table of field = a whole number")
        return
    for ref, value in skip.items():
        field = resolve_field(game, table, ref, f"{where}: skip", errors)
        if field is not None and (field.kind != "integer" or not field.single):
            errors.append(f"{where}: skip names {ref}, which is not one integer cell of a record")
        if not isinstance(value, int) or isinstance(value, bool):
            errors.append(f"{where}: skip gives {ref} {value!r}, not a whole number")


def lint(game: Game, data: dict, source: str) -> list[str]:
    errors: list[str] = []
    if game.prefix_error:
        errors.append(f"{source}: {game.prefix_error}")
    unknown_keys(data, {"export", "kind", "loader", "script_relation"}, source, errors)

    export = data.get("export")
    if not isinstance(export, dict):
        errors.append(f"{source}: needs an [export] table")
        export = {}
    unknown_keys(export, EXPORT_KEYS, f"{source}: [export]", errors)
    boot = export.get("boot")
    if not isinstance(boot, dict) or set(boot) != {"script", "read_at"}:
        errors.append(f"{source}: [export] boot must be {{ script = NAME, read_at = PATCH_POINT }}")
    else:
        check_scripts([boot["script"]], f"{source}: [export] boot script", errors, patterns=False)
        if boot["read_at"] not in game.points:
            errors.append(f"{source}: [export] boot reads at {boot['read_at']!r}, which is no patch point of the game")
    check_scripts(export.get("builders"), f"{source}: [export] builders", errors, patterns=True)
    if not export.get("builders"):
        errors.append(f"{source}: [export] builders must name at least one script")
    excluded = set(check_scripts(export.get("excluded_builders", []), f"{source}: [export] excluded_builders",
                                 errors, patterns=False))
    for name in check_scripts(export.get("text_scripts", []), f"{source}: [export] text_scripts", errors,
                              patterns=False):
        if name in excluded:
            errors.append(f"{source}: [export] text script {name} is also an excluded builder")
    # A derived script works out play state from the tables during the boot (Arterial's COUNTSKILL): its writes are
    # withheld without a refusal, so it may be neither a builder nor a text script.
    builders = export.get("builders") if isinstance(export.get("builders"), list) else []
    for name in check_scripts(export.get("derived", []), f"{source}: [export] derived", errors, patterns=False):
        if any(isinstance(b, str) and fnmatch.fnmatchcase(name, b) for b in builders) and name not in excluded:
            errors.append(f"{source}: [export] derived script {name} is a builder")
        if name in (export.get("text_scripts") or []):
            errors.append(f"{source}: [export] derived script {name} is also a text script")
    text_scripts = set(export.get("text_scripts") or [])
    check_picture_patterns(export.get("explicit_pictures", []), f"{source}: [export] explicit_pictures", errors)
    check_text_overrides(export.get("text_overrides", {}), f"{source}: [export] text_overrides", errors)

    kinds = data.get("kind", [])
    if not isinstance(kinds, list):
        errors.append(f"{source}: kind must be an array of tables ([[kind]])")
        kinds = []
    ids: dict[str, str] = {}
    # The tables each kind's loaders publish, which a kind's label may name.
    loaded_names: dict[str, set[str]] = {}
    for loader in data.get("loader", []):
        if isinstance(loader, dict) and isinstance(loader.get("keys"), str) and isinstance(loader.get("tables"), dict):
            loaded_names.setdefault(loader["keys"], set()).update(loader["tables"])
    for kind in kinds:
        lint_kind(game, kind, source, text_scripts, ids, errors,
                  loaded_names.get(kind.get("id"), set()) if isinstance(kind, dict) else set())
    check_borrowed_pictures(game, kinds, source, errors)
    # A reference title takes its target's title, so a kind must show the target's table.
    declared = {kind.get("table") for kind in kinds if isinstance(kind, dict)}
    for kind in kinds:
        table = game.tables.get(kind.get("table")) if isinstance(kind, dict) else None
        title = kind.get("title") if table else None
        for one_title in [title] if isinstance(title, str) else title if isinstance(title, list) else []:
            if not isinstance(one_title, str):
                continue
            target = refs_of(table).get(one_title.partition(".")[0])
            if target and target != globals_build.REF_PICTURE and target not in declared:
                errors.append(f"{source}: kind {kind.get('id')!r}: title {one_title} refers to table {target}, which no "
                              f"kind shows, so it has no title to take")

    relation_ids: set[str] = set()
    kind_tables = {kind.get("id"): kind.get("table") for kind in kinds if isinstance(kind, dict)}
    loaded_by_kind: dict[str, dict] = {}
    for loader in data.get("loader", []):
        lint_loader(game, loader, source, ids, kind_tables, errors, loaded_by_kind)
    # A group's labels and a reference's heading may name a loaded table's fields, so they are checked once every
    # loader has been.
    shown_tables = {kind.get("table") for kind in kinds if isinstance(kind, dict)}
    for kind in kinds:
        if isinstance(kind, dict):
            check_group_labels(game, kind, loaded_by_kind.get(kind.get("id"), {}),
                               f"{source}: kind {kind.get('id')!r}", errors)
            check_referenced_as(game, kind, loaded_by_kind.get(kind.get("id"), {}), shown_tables,
                                f"{source}: kind {kind.get('id')!r}", errors)
    for relation in data.get("script_relation", []):
        lint_relation(game, relation, source, ids, relation_ids, errors)
    return errors


def check_text_overrides(overrides, where: str, errors: list[str]) -> None:
    """[export] text_overrides: a table of language tag = { "game string" = "its text" }, the export's text of a game
    string in that language in place of the language package's (game data export plan §5). Each string and text a
    line that is not empty."""
    if not isinstance(overrides, dict):
        errors.append(f"{where} must be a table of language tag = {{ \"game string\" = \"its text\" }}")
        return
    for tag, texts in overrides.items():
        if not LANGUAGE_TAG.fullmatch(tag):
            errors.append(f"{where}: {tag!r} is not a language tag (en, pt-BR)")
        if not isinstance(texts, dict) or not texts:
            errors.append(f"{where}: {tag} must be a table of \"game string\" = \"its text\"")
            continue
        for text, shown in texts.items():
            if not text.strip() or not isinstance(shown, str) or not shown.strip() or "\n" in text + shown:
                errors.append(f"{where}: {tag} gives {text!r} {shown!r}; a game string and its text are each a line "
                              f"that is not empty")


def lint_kind(game: Game, kind: dict, source: str, text_scripts: set[str], ids: dict[str, str],
              errors: list[str], loaded_names: set[str] = frozenset()) -> None:
    kid = kind.get("id")
    where = f"{source}: kind {kid!r}"
    unknown_keys(kind, KIND_KEYS, where, errors)
    if not isinstance(kid, str) or not IDENTIFIER.fullmatch(kid):
        errors.append(f"{where}: id must be a lowercase identifier")
        return
    if kid in RESERVED_KINDS:
        errors.append(f"{where}: {kid} is a path of the site's database (site design §4), so no kind may take it")
    if kid in EXPORT_TABLES:
        errors.append(f"{where}: {kid} is a table of the export's database (game data export spec §7.2), so no kind "
                      "may take it")
    for name in [kid, *(kind.get("aliases") or [])]:
        if not isinstance(name, str) or not IDENTIFIER.fullmatch(name):
            errors.append(f"{where}: alias {name!r} must be a lowercase identifier")
        elif name in ids:
            errors.append(f"{where}: {name} is also {ids[name]}")
        else:
            ids[name] = f"the id of kind {kid!r}" if name == kid else f"an alias of kind {kid!r}"
    table = game.tables.get(kind.get("table"))
    if table is None:
        errors.append(f"{where}: table {kind.get('table')!r} is not a [[table]] of the registry")
        return
    check_joins(game, kind, table, where, errors)
    # The published fields and the groups may name a joined table's fields; the title, text, pictures and the rest
    # are the kind's own table's.
    view = kind_view(game, kind)

    def one(ref, label: str, want: str | None) -> Field | None:
        field = resolve_field(game, table, ref, f"{where}: {label}", errors)
        if field is None:
            return None
        if not field.single:
            errors.append(f"{where}: {label} {ref} is a row of cells; name one column of it")
        if want and field.kind != want:
            errors.append(f"{where}: {label} {ref} must be a {want} field")
        return field

    def published(ref, label: str, field: Field | None) -> None:
        if field is not None and field.state:
            errors.append(f"{where}: {label} {ref} is saved state ({field.global_name} lies below the saved prefix), "
                          f"the player's progress rather than the game's data, so no kind publishes it")

    one(kind.get("exists"), "exists", None)
    # The title (game data export plan §4.1): a string field, a reference field whose target's title the entity takes
    # (a recipe is named by the item it makes), or none, and the site names an entity by its kind and id. A list of
    # references (plan §5) names it by the first that names a record: an Arterial recipe makes an item or a card.
    if isinstance(kind.get("title"), list):
        titles = kind["title"]
        if len(titles) < 2 or not all(isinstance(t, str) for t in titles):
            errors.append(f"{where}: a title list names two or more references; one is written as a string")
        for name in sorted({t for t in titles if isinstance(t, str) and titles.count(t) > 1}):
            errors.append(f"{where}: title {name} is named twice")
        for title in (t for t in titles if isinstance(t, str)):
            field = one(title, "title", None)
            base = title.partition(".")[0]
            target = refs_of(table).get(base)
            if field is not None and (field.kind != "integer" or target is None
                                      or target == globals_build.REF_PICTURE):
                errors.append(f"{where}: title {title} is no reference; each title of a list is a reference")
            elif field is not None and base not in kind.get("fields", []):
                errors.append(f"{where}: title {title} is a reference, which the kind must publish: list {base} under "
                              f"fields")
            published(title, "title", field)
    elif "title" in kind:
        title = kind["title"]
        field = one(title, "title", None)
        if field is not None and field.kind == "integer":
            base = str(title).partition(".")[0]
            target = refs_of(table).get(base)
            if target is None or target == globals_build.REF_PICTURE:
                errors.append(f"{where}: title {title} is an integer field the registry's [table.refs] does not mark as "
                              f"referring to a table; a title is a string field or a reference")
            elif base not in kind.get("fields", []):
                errors.append(f"{where}: title {title} is a reference, which the kind must publish: list {base} under "
                              f"fields")
        elif field is not None and field.kind != "string":
            errors.append(f"{where}: title {title} must be a string field or a reference")
    # `titles` (export plan §5): OME's word for a record the game names none, its title where its title field is empty
    # (Verita's element 0, which the game draws no name for).
    words = kind.get("titles")
    if words is not None:
        if not isinstance(words, dict) or not words:
            errors.append(f"{where}: titles must be a table of record = OME's word for it")
        else:
            if not isinstance(kind.get("title"), str):
                errors.append(f"{where}: titles name a record the title field leaves empty, so the kind's title is "
                              f"one field")
            key_range = kind.get("key_range")
            for record, word in words.items():
                if not str(record).isdigit() or (len(str(record)) > 1 and str(record).startswith("0")):
                    errors.append(f"{where}: titles: {record!r} must be a record's id, a whole number")
                elif is_key_range(key_range) and not key_range[0] <= int(record) <= key_range[1]:
                    errors.append(f"{where}: titles: record {record} lies outside the kind's key_range {key_range}")
                if not isinstance(word, str) or not word.strip() or "\n" in word or len(word) > LABEL_LIMIT:
                    errors.append(f"{where}: titles: the word for record {record} must be a name of at most "
                                  f"{LABEL_LIMIT} characters, on one line")
        published(title, "title", field)
    check_skip(game, table, kind.get("skip"), where, errors)
    # The records the kind publishes, when not all of them: a table whose record 0 is a name the game gives to none
    # (Kamidori's element 0, 無属, which a reference cannot reach, since a value of 0 is none).
    key_range = kind.get("key_range")
    if key_range is not None:
        if not is_key_range(key_range):
            errors.append(f"{where}: key_range must be [first, last], whole numbers with 0 <= first <= last")
        elif key_range[1] >= table["records"]:
            errors.append(f"{where}: key_range ends at {key_range[1]}, past table {table.get('name')}'s "
                          f"{table['records']} records")

    text = kind.get("text", [])
    fields = kind.get("fields", [])
    for label, refs, want in (("text", text, "string"), ("fields", fields, None)):
        if not isinstance(refs, list):
            errors.append(f"{where}: {label} must be a list of fields")
            continue
        for ref in refs:
            field = resolve_field(game, view if label == "fields" else table, ref, f"{where}: {label}", errors)
            if field is None:
                continue
            # A paired reference (its record a pair of elements, row and slot) is published by the first column of
            # each pair, whose value the record is named by; the pair's second is part of it.
            reference = reference_of(view, ref) if label == "fields" else None
            if reference is not None and isinstance(reference[2], dict) and reference[2].get("pairs") \
                    and (field.column is None or field.column % 2):
                errors.append(f"{where}: fields {ref}: {reference[1]} is a paired reference, a record for each pair of "
                              f"its elements; publish the first column of each pair (column 0, 2 ...)")
            if want == "string" and field.kind != "string":
                errors.append(f"{where}: text {ref} is not a string field; list it under fields")
            if label == "fields" and field.kind == "string":
                errors.append(f"{where}: fields {ref} is a string field; strings are published only under text "
                              f"(export spec §5.5)")
            published(ref, label, field)
    categories = kind.get("categories", {})
    category_names = set(categories) if isinstance(categories, dict) else set()
    for ref in kind.get("filters", []):
        if ref in category_names:
            continue
        if ref not in fields:
            errors.append(f"{where}: filter {ref!r} is not one of the kind's fields or categories")
        else:
            one(ref, "filter", None)

    pictures = kind.get("pictures", {})
    refs = refs_of(table)
    if not isinstance(pictures, dict):
        errors.append(f"{where}: pictures must be a table of name = field")
        pictures = {}
    for name, ref in pictures.items():
        if not IDENTIFIER.fullmatch(name):
            errors.append(f"{where}: picture name {name!r} must be a lowercase identifier")
        if isinstance(ref, dict) and "sheet" in ref:
            check_sheet(ref, lambda field, label=f"picture {name}": one(field, label, "integer"),
                        f"{where}: picture {name}", errors)
            continue
        if isinstance(ref, dict) and "from" in ref:
            # { from, by, picture }: another kind's picture (check_borrowed_pictures, once every kind is read).
            unknown_keys(ref, BORROWED_KEYS, f"{where}: picture {name}", errors)
            continue
        if isinstance(ref, dict):
            # { field, key, frame, blend }: the colour the game loads the picture with as its key, drawn as nothing
            # (export plan §5: Himegari's map sprites, DRAWCH@0x9c), 0xRRGGBB; the picture's first frame, its top left
            # corner as wide and as high as two fields of the record give (a map object's sheet, DRAWOBJ); the first
            # of a strip's `frames` frames side by side, its width ÷ frames and its full height (a Verita title's
            # battle strip, six frames of 120 x 120 or of 320 x 300); and a field
            # of the record holding the mode op 0x203 draws the picture with, whose 1, the additive glow, the export
            # shows as it looks over black (a healing feather). Any may be left out, not all.
            unknown_keys(ref, {"field", "key", "frame", "frames", "blend"}, f"{where}: picture {name}", errors)
            if not {"key", "frame", "frames", "blend"} & set(ref):
                errors.append(f"{where}: picture {name}: a table names a key, a frame, frames or a blend; a field "
                              f"alone is written as its name")
            if "frames" in ref and (not is_size(ref["frames"]) or "frame" in ref):
                errors.append(f"{where}: picture {name}: frames must be a whole number above 0, the strip's frames "
                              f"side by side, and a picture has frame or frames, not both")
            if "blend" in ref:
                one(ref["blend"], f"picture {name}'s blend", "integer")
            key = ref.get("key")
            if "key" in ref and (not isinstance(key, int) or isinstance(key, bool) or not 0 <= key <= 0xFFFFFF):
                errors.append(f"{where}: picture {name}: key must be a colour 0xRRGGBB")
            frame = ref.get("frame")
            if "frame" in ref:
                if not (isinstance(frame, list) and len(frame) == 2 and all(isinstance(f, str) for f in frame)):
                    errors.append(f"{where}: picture {name}: frame must be [width, height], two fields of the record")
                else:
                    for size in frame:
                        one(size, f"picture {name}'s frame", "integer")
            ref = ref.get("field")
        field = one(ref, f"picture {name}", "integer")
        if field is not None and refs.get(str(ref).partition(".")[0]) != globals_build.REF_PICTURE:
            errors.append(f"{where}: picture {name} reads {ref}, which the registry's [table.refs] does not mark as "
                          f"{globals_build.REF_PICTURE}")

    names = set(table.get("fields") or {})
    descriptions = kind.get("description", [])
    if not isinstance(descriptions, list):
        errors.append(f"{where}: description must be an array of tables ([[kind.description]])")
        descriptions = []
    for description in descriptions:
        dname = description.get("name")
        dwhere = f"{where}: description {dname!r}"
        unknown_keys(description, DESCRIPTION_KEYS, dwhere, errors)
        if not isinstance(dname, str) or not IDENTIFIER.fullmatch(dname):
            errors.append(f"{dwhere}: name must be a lowercase identifier")
        elif dname in names:
            errors.append(f"{dwhere}: {dname} is also a field of table {table['name']} or another description")
        else:
            names.add(dname)
        script = description.get("script")
        if not isinstance(script, dict) or len(script) != 1 or not set(script) <= {"field", "name"}:
            errors.append(f"{dwhere}: script must be {{ field = FIELD }}, the field holding the script the game calls "
                          f"for the record, or {{ name = SCRIPT }}")
        elif "field" in script:
            one(script["field"], "description script", "integer")
        elif check_scripts([script["name"]], f"{dwhere}: script", errors, patterns=False) and \
                script["name"] not in text_scripts:
            errors.append(f"{dwhere}: script {script['name']} is not one of [export] text_scripts")
        check_seed(game, description.get("seed"), dwhere, errors)

    # A group (game data export plan §5): published rows of cells, which the site shows as one table, as a recipe's
    # ingredients beside their quantities. The narrowest field's width is the table's row count; a field as wide as it
    # gives a column, and a field some times as wide gives that many, its elements row by row (a condition's nine stat
    # changes at each of its five levels).
    groups = kind.get("group", [])
    if not isinstance(groups, list):
        errors.append(f"{where}: group must be an array of tables ([[kind.group]])")
        groups = []
    published_fields = fields if isinstance(fields, list) else []
    grouped: dict[str, str] = {}
    for group in groups:
        if not isinstance(group, dict):
            errors.append(f"{where}: each group must be a table")
            continue
        gname = group.get("name")
        gwhere = f"{where}: group {gname!r}"
        unknown_keys(group, GROUP_KEYS, gwhere, errors)
        if not isinstance(gname, str) or not IDENTIFIER.fullmatch(gname):
            errors.append(f"{gwhere}: name must be a lowercase identifier")
        elif gname in names:
            errors.append(f"{gwhere}: {gname} is also a field of table {table['name']}, a description or another group")
        else:
            names.add(gname)
        members = group.get("fields")
        if not isinstance(members, list) or len(members) < 2 or not all(isinstance(m, str) for m in members):
            errors.append(f"{gwhere}: fields must list at least two of the kind's fields")
            continue
        widths: dict[str, int] = {}
        for ref in members:
            if ref not in published_fields:
                errors.append(f"{gwhere}: {ref!r} is not one of the kind's fields")
                continue
            if ref in grouped:
                errors.append(f"{gwhere}: {ref} is also in group {grouped[ref]!r}")
                continue
            grouped[ref] = gname
            field = resolve_field(game, view, ref, f"{gwhere}: fields", errors)
            if field is None:
                continue
            if field.single:
                errors.append(f"{gwhere}: {ref} has one cell for each record; a group's fields are rows of cells")
            else:
                widths[ref] = field.width
        if widths and any(width % min(widths.values()) for width in widths.values()):
            shown = ", ".join(f"{ref} {width}" for ref, width in widths.items())
            errors.append(f"{gwhere}: each field's width must be a multiple of the narrowest's ({shown}), which is the "
                          "number of rows")
        if "labels" in group and not isinstance(group["labels"], str):
            errors.append(f"{gwhere}: labels must name a loaded table's text field, as table.field")
        # A row word names row r with its number, "Level 12", where no table's text labels the rows.
        if "row_label" in group:
            check_heading(group["row_label"], f"{gwhere}: row_label", errors)

    check_text_blocks(game, table, kind, where, errors)
    check_categories(game, kind, categories, published_fields, names, one, where, errors)

    # A note (the user, 2026-11-22): a line that explains a detail, keyed by what the infobox shows as one row: a field
    # outside every group, or a group.
    notes = kind.get("notes", {})
    if not isinstance(notes, dict):
        errors.append(f"{where}: notes must be a table of field or group = text")
        notes = {}
    group_names = {group.get("name") for group in groups if isinstance(group, dict)}
    shown_fields = set(published_fields) | set(text if isinstance(text, list) else []) | category_names
    for key, note in notes.items():
        if key in grouped:
            errors.append(f"{where}: note {key}: the field is shown in group {grouped[key]!r}; note the group")
        elif key not in shown_fields and key not in group_names:
            errors.append(f"{where}: note {key!r} names none of the kind's fields, text or groups")
        check_note(note, f"{where}: note {key}", errors)
    # A label (export plan §5): a reader's name for a shown field, grouped or not, a group, or a table the kind's
    # loaders publish, which the page heads with it (a stage's map objects, shown as its dig sites).
    check_labels(kind.get("labels", {}), shown_fields | group_names | set(loaded_names), where, errors)
    # About (site plan §10.3, finding 18): what the kind's entities are, in one line, for its card and its list.
    if "about" in kind:
        about = kind["about"]
        if not isinstance(about, str) or not about.strip() or "\n" in about:
            errors.append(f"{where}: about must be one line of text")
        elif len(about) > ABOUT_LIMIT:
            errors.append(f"{where}: about is {len(about)} characters; it is a line of at most {ABOUT_LIMIT}")
    # Qualifiers (export plan §5): the shown fields, one cell a record, that tell apart entities with one name.
    qualifiers = kind.get("qualifiers", [])
    if not isinstance(qualifiers, list):
        errors.append(f"{where}: qualifiers must list the kind's fields or text")
        qualifiers = []
    elif len(qualifiers) > QUALIFIER_LIMIT:
        errors.append(f"{where}: qualifiers list {len(qualifiers)} fields; a name takes at most {QUALIFIER_LIMIT}")
    for ref in qualifiers:
        if not isinstance(ref, str) or ref not in shown_fields:
            errors.append(f"{where}: qualifier {ref!r} is not one of the kind's fields or text")
        elif qualifiers.count(ref) > 1:
            errors.append(f"{where}: qualifier {ref} is listed twice")
        else:
            one(ref, "qualifier", None)
    # Marks (export plan §5): a word the site shows beside every link to an entity whose field is not none, as a unit
    # that is a boss. The field is a shown value, one cell a record, and no reference: the mark says what it is.
    marks = kind.get("marks", {})
    if not isinstance(marks, dict):
        errors.append(f"{where}: marks must be a table of field = word")
        marks = {}
    for ref, word in marks.items():
        if ref not in published_fields:
            errors.append(f"{where}: mark {ref!r} is not one of the kind's fields")
            continue
        one(ref, "mark", "integer")
        if reference_of(view, ref) is not None:
            errors.append(f"{where}: mark {ref} is a reference; a mark is a value the entity has")
        check_heading(word, f"{where}: mark {ref}", errors)


def check_text_blocks(game: Game, table: dict, kind: dict, where: str, errors: list[str]) -> None:
    """A text field's blocks (game data export plan §5): a row of lines that the game draws as several blocks, each a
    named run of its lines, first to last. The blocks are in the order the game draws them and share no line; a line
    in no block is not shown."""
    blocks = kind.get("text_blocks", {})
    if not isinstance(blocks, dict):
        errors.append(f"{where}: text_blocks must be a table of text field = {{ block = [first, last] }}")
        return
    text = kind.get("text", []) if isinstance(kind.get("text"), list) else []
    for ref, runs in blocks.items():
        bwhere = f"{where}: text_blocks {ref}"
        if ref not in text:
            errors.append(f"{bwhere}: {ref!r} is not one of the kind's text fields")
            continue
        field = resolve_field(game, table, ref, bwhere, errors)
        if field is None:
            continue
        if field.single:
            errors.append(f"{bwhere}: {ref} is one line for each record; blocks divide a row of lines")
            continue
        if not isinstance(runs, dict) or not runs:
            errors.append(f"{bwhere}: must name at least one block, as block = [first, last]")
            continue
        end = -1
        for name, run in runs.items():
            if not IDENTIFIER.fullmatch(name):
                errors.append(f"{bwhere}: block name {name!r} must be a lowercase identifier")
            if not (isinstance(run, list) and len(run) == 2 and all(isinstance(n, int) and not isinstance(n, bool)
                                                                    for n in run)
                    and 0 <= run[0] <= run[1] < field.width):
                errors.append(f"{bwhere}: block {name} must be [first, last], lines of the row's 0 to {field.width - 1}")
                continue
            if run[0] <= end:
                errors.append(f"{bwhere}: block {name} starts at line {run[0]}, within the block before it; the blocks "
                              "are in the order the game draws them and share no line")
            end = max(end, run[1])


def check_categories(game: Game, kind: dict, categories, published_fields: list, names: set, one, where: str,
                     errors: list[str]) -> None:
    """A kind's categories (export plan §5): each a value the export works out for every record from its published
    fields, by rules tried in turn, and publishes as a mask whose bits are named. A rule `{ field, word }` puts a record
    whose field is above 0 and not the field's none in the category `word`, alone; a rule `{ field }` names a field whose
    bits the game names (`bits_of` a list of strings) and puts a record in the category of each bit it sets. The first
    rule that puts a record somewhere decides; a record none does has no category. Arterial's card book: a card with
    ＬＰ is a leader, and any other is listed under each race it has."""
    if not isinstance(categories, dict):
        errors.append(f"{where}: categories must be a table of name = [rules]")
        return
    for name, rules in categories.items():
        cwhere = f"{where}: category {name!r}"
        if not isinstance(name, str) or not IDENTIFIER.fullmatch(name):
            errors.append(f"{cwhere}: name must be a lowercase identifier")
        elif name in names or name in ("id", "content_hash"):
            errors.append(f"{cwhere}: {name} is also a field of table {kind.get('table')}, a description, a group or a "
                          f"column of the kind's table")
        else:
            names.add(name)
        if not isinstance(rules, list) or not rules:
            errors.append(f"{cwhere}: must list its rules, each {{ field, word }} or {{ field }}")
            continue
        bits = 0
        for rule in rules:
            if not isinstance(rule, dict) or not isinstance(rule.get("field"), str):
                errors.append(f"{cwhere}: each rule is {{ field, word }} or {{ field }}")
                continue
            unknown_keys(rule, {"field", "word"}, cwhere, errors)
            ref = rule["field"]
            if ref not in published_fields:
                errors.append(f"{cwhere}: {ref!r} is not one of the kind's fields; a category is worked out from what "
                              f"the page shows")
                continue
            field = one(ref, f"category {name}'s rule", "integer")
            if "word" in rule:
                check_heading(rule["word"], f"{cwhere}: word", errors)
                bits += 1
                continue
            target = game.globals.get(field.bits_of) if field is not None and field.bits_of else None
            type_ = str((target or {}).get("type") or "")
            dims = globals_build.dims_of(type_) if type_.startswith("string[") else None
            count = dims if dims and len(dims) == 1 else None
            if field is not None and not count:
                errors.append(f"{cwhere}: {ref} has no bits the game names (bits_of a list of strings); give the rule "
                              f"a word")
                continue
            bits += count[0] if count else 0
        if bits > 62:
            errors.append(f"{cwhere}: its rules name {bits} categories, more than a mask of 62 bits holds")


def check_labels(labels, shown: set, where: str, errors: list[str]) -> None:
    """`labels`: a reader's name for each shown field (or group) whose own name reads as the game stores it."""
    if not isinstance(labels, dict):
        errors.append(f"{where}: labels must be a table of field = name")
        return
    for key, label in labels.items():
        if key not in shown:
            errors.append(f"{where}: label {key!r} names nothing the page shows")
        if not isinstance(label, str) or not label.strip() or "\n" in label or len(label) > LABEL_LIMIT:
            errors.append(f"{where}: label {key} must be a name of at most {LABEL_LIMIT} characters, on one line")


def check_note(note, where: str, errors: list[str]) -> None:
    if not isinstance(note, str) or not note.strip() or "\n" in note:
        errors.append(f"{where} must be one line of text")
    elif len(note) > NOTE_LIMIT:
        errors.append(f"{where} is {len(note)} characters; a note is a line of at most {NOTE_LIMIT}")


def check_sheet(sheet: dict, check_field, where: str, errors: list[str]) -> None:
    """A picture cut from a sheet: `field` numbers a cell, or the record's own id where it is left out (an element's
    icon), the first being `first`, `columns` to a row from `origin`, `step` apart, each `size` (pixels, x before y). A
    number below `first`, or above `last` where it is given, is no picture: a game whose numbers past a point are
    another sheet's (Himegari's icons from 501, an append's) declares each sheet with its range. `key` is the colour the game loads the sheet with as its key, drawn as nothing."""
    unknown_keys(sheet, SHEET_KEYS | SHEET_OPTIONAL_KEYS, where, errors)
    missing = sorted(SHEET_KEYS - set(sheet))
    if missing:
        errors.append(f"{where}: a sheet picture needs {', '.join(missing)}")
        return
    if "field" in sheet:
        check_field(sheet["field"])
    if not isinstance(sheet["sheet"], str) or not SHEET.fullmatch(sheet["sheet"]):
        errors.append(f"{where}: sheet {sheet['sheet']!r} must be a picture's catalog name in upper case with its "
                      f"extension, and an append's own picture with its `$n$` prefix")
    for key, low in (("first", 0), ("columns", 1)):
        value = sheet[key]
        if not isinstance(value, int) or isinstance(value, bool) or value < low:
            errors.append(f"{where}: {key} must be a whole number of at least {low}")
    for key, low in (("origin", 0), ("step", 1), ("size", 1)):
        pair = sheet[key]
        if not (isinstance(pair, list) and len(pair) == 2
                and all(isinstance(v, int) and not isinstance(v, bool) and v >= low for v in pair)):
            errors.append(f"{where}: {key} must be [x, y], whole numbers of at least {low}")
    if all(isinstance(sheet[k], list) and len(sheet[k]) == 2 for k in ("step", "size")) and \
            any(isinstance(s, int) and isinstance(c, int) and c > s for s, c in zip(sheet["step"], sheet["size"])):
        errors.append(f"{where}: size {sheet['size']} is larger than step {sheet['step']}, so cells would overlap")
    if "key" in sheet and (not isinstance(sheet["key"], int) or isinstance(sheet["key"], bool)
                           or not 0 <= sheet["key"] <= 0xFFFFFF):
        errors.append(f"{where}: key must be a colour 0xRRGGBB")
    if "last" in sheet:
        last, first = sheet["last"], sheet["first"]
        if not isinstance(last, int) or isinstance(last, bool) or (isinstance(first, int) and last < first):
            errors.append(f"{where}: last must be a whole number of at least first")


def check_group_labels(game: Game, kind: dict, loaded: dict[str, dict], where: str, errors: list[str]) -> None:
    """A group's labels: a text field of a table the kind's loaders publish (table.field), one cell a record, whose
    record r labels the group's row r; the table has a record for each of the group's rows."""
    table = game.tables.get(kind.get("table"))
    for group in kind.get("group", []) if isinstance(kind.get("group"), list) else []:
        if not isinstance(group, dict) or not isinstance(group.get("labels"), str) or table is None:
            continue
        gwhere = f"{where}: group {group.get('name')!r}: labels"
        short, _, field_name = group["labels"].partition(".")
        entry = loaded.get(short)
        if entry is None:
            errors.append(f"{gwhere} {group['labels']!r}: {short!r} is no table the kind's loaders publish")
            continue
        loaded_table, declared = entry
        if field_name not in (declared.get("text") or []):
            errors.append(f"{gwhere} {group['labels']!r}: {field_name!r} is no text field of table {short}")
            continue
        field = resolve_field(game, loaded_table, field_name, gwhere, errors)
        if field is not None and not field.single:
            errors.append(f"{gwhere} {group['labels']!r} is a row of cells; a label is one cell of a record")
        widths = [f.width for ref in group.get("fields", [])
                  if (f := resolve_field(game, kind_view(game, kind), ref, gwhere, [])) is not None and not f.single]
        if widths and loaded_table["records"] < min(widths):
            errors.append(f"{gwhere}: table {short} has {loaded_table['records']} records, fewer than the group's "
                          f"{min(widths)} rows")


def check_referenced_as(game: Game, kind: dict, loaded: dict[str, tuple[dict, dict]], shown_tables: set,
                        where: str, errors: list[str]) -> None:
    """How the kind's references read on their targets' pages (export plan §5): `referenced_as` keys a published
    reference field of the kind, or of a table its loaders publish (`table.field`), whose target a kind shows, to its
    heading, or to `{ heading, details, parts }`. A detail is shown beside each referring entity: a field of the kind
    or a text, one cell a record; a field of the reference's group as wide as it (a drop's chance beside the drop); or
    for a loaded table's field, a field or text of the same table, one cell a row. `parts` gives a heading to each of
    the loaded table's parts that its rows read under (a stage's allies). A row of cells of the kind whose columns are
    a table's records (`columns_ref`: a skill's condition levels, by condition) refers to the record of each column whose
    cell is not 0, and may be keyed as a reference is (site plan §13, C2). A field of a table the kind joins is keyed
    `table.field`, and a published column of a paired reference `field.column`, each as the kind publishes it."""
    table = kind_view(game, kind)
    entries = kind.get("referenced_as", {})
    if table is None:
        return
    if not isinstance(entries, dict):
        errors.append(f"{where}: referenced_as must be a table of field = heading or {{ heading, details, parts }}")
        return
    fields = kind.get("fields", []) if isinstance(kind.get("fields"), list) else []
    text = kind.get("text", []) if isinstance(kind.get("text"), list) else []
    groups = {ref: group.get("fields") for group in kind.get("group", []) if isinstance(group, dict)
              for ref in (group.get("fields") or []) if isinstance(group.get("fields"), list)}

    def one_cell(owner: dict, ref: str) -> bool:
        field = resolve_field(game, owner, ref, where, [])
        return field is not None and field.single

    for key, entry in entries.items():
        rwhere = f"{where}: referenced_as {key}"
        short, dot, name = key.partition(".")
        reference = reference_of(table, key) if dot and short not in loaded else None
        if reference is not None:
            # The kind's own published field: a joined table's (table.field) or a paired reference's column.
            if key not in fields:
                errors.append(f"{rwhere}: {key!r} is not a published field")
                continue
            if reference[2] == globals_build.REF_PICTURE:
                errors.append(f"{rwhere}: {key} is not a reference to a table ([table.refs])")
                continue
            target = globals_build.ref_target(reference[2])
            if target not in shown_tables:
                errors.append(f"{rwhere}: {key} refers to table {target}, which no kind shows")
            if isinstance(entry, str):
                entry = {"heading": entry}
            if not isinstance(entry, dict) or "heading" not in entry:
                errors.append(f"{rwhere} must be a heading or {{ heading, details, parts }}")
                continue
            unknown_keys(entry, REFERENCED_AS_KEYS - {"parts"}, rwhere, errors)
            check_heading(entry["heading"], f"{rwhere}: heading", errors)
            details = entry.get("details", [])
            if not isinstance(details, list) or len(details) > DETAIL_LIMIT:
                errors.append(f"{rwhere}: details must list at most {DETAIL_LIMIT} fields")
                details = []
            for detail in details:
                if not isinstance(detail, str) or detail not in fields + text or not one_cell(table, detail):
                    errors.append(f"{rwhere}: detail {detail!r} must be one of the kind's fields or text, one cell a "
                                  f"record")
            continue
        if dot:
            found = loaded.get(short)
            if found is None:
                errors.append(f"{rwhere}: {short!r} is no table the kind's loaders publish")
                continue
            owner, declared = found
            published = declared.get("fields") or []
            own_text = declared.get("text") or []
        else:
            owner, declared, name = table, None, key
            published = fields
            own_text = []
        target = refs_of(owner).get(name)
        if target is None and dot and "." in name:
            # A field of a table the loaded table joins, table.field: its own table's reference.
            joined_reference = reference_of(owner, name)
            target = globals_build.ref_target(joined_reference[2]) if joined_reference else None
        if name not in published:
            errors.append(f"{rwhere}: {key!r} is not a published field" + (f" of table {short}" if dot else ""))
            continue
        if target is None and not dot:
            columns = resolve_field(game, owner, name, rwhere, [])
            if columns is not None and not columns.single:
                target = game.globals.get(columns.global_name, {}).get("columns_ref")
        if target is None or target == globals_build.REF_PICTURE:
            errors.append(f"{rwhere}: {key} is not a reference to a table ([table.refs]) nor a row of cells whose "
                          f"columns are a table's records (columns_ref)")
            continue
        if target not in shown_tables:
            errors.append(f"{rwhere}: {key} refers to table {target}, which no kind shows")
        if isinstance(entry, str):
            entry = {"heading": entry}
        if not isinstance(entry, dict) or "heading" not in entry:
            errors.append(f"{rwhere} must be a heading or {{ heading, details, parts }}")
            continue
        unknown_keys(entry, REFERENCED_AS_KEYS, rwhere, errors)
        check_heading(entry["heading"], f"{rwhere}: heading", errors)
        details = entry.get("details", [])
        if not isinstance(details, list) or len(details) > DETAIL_LIMIT:
            errors.append(f"{rwhere}: details must list at most {DETAIL_LIMIT} fields")
            details = []
        width = resolve_field(game, owner, name, rwhere, [])
        for detail in details:
            dwhere = f"{rwhere}: detail {detail!r}"
            if not isinstance(detail, str) or details.count(detail) > 1 or detail == key:
                errors.append(f"{dwhere} must name another field, once")
                continue
            dshort, ddot, dname = detail.partition(".")
            if ddot and dot and dshort == short:
                # A field or text of the same loaded table, one cell a row.
                if dname not in published and dname not in own_text:
                    errors.append(f"{dwhere} is not a published field or text of table {short}")
                elif not one_cell(owner, dname):
                    errors.append(f"{dwhere} is a row of cells; a detail is one cell of the referring row")
            elif ddot:
                errors.append(f"{dwhere} names another table; a detail is a field of the kind or of the reference's "
                              f"own table")
            elif detail not in fields and detail not in text:
                errors.append(f"{dwhere} is not one of the kind's fields or text")
            elif one_cell(table, detail):
                pass
            elif not dot and groups.get(detail) is not None and groups.get(detail) is groups.get(name):
                field = resolve_field(game, table, detail, dwhere, [])
                if width is not None and field is not None and field.width != width.width:
                    errors.append(f"{dwhere} is {field.width} wide and {key} {width.width}: a detail of the reference's "
                                  f"group is as wide as the reference, element for element")
            else:
                errors.append(f"{dwhere} is a row of cells outside the reference's group; a detail is one cell of the "
                              f"referring entity, or an element of its group beside the reference")
        parts = entry.get("parts", {})
        names = {part.get("name") for part in (declared or {}).get("parts", []) if isinstance(part, dict)}
        if not isinstance(parts, dict):
            errors.append(f"{rwhere}: parts must be a table of part = heading")
            parts = {}
        for part, heading in parts.items():
            if not dot:
                errors.append(f"{rwhere}: part {part!r}: only a loaded table's rows are in parts")
            elif part not in names:
                errors.append(f"{rwhere}: part {part!r} is not a part of table {short}")
            check_heading(heading, f"{rwhere}: part {part}", errors)


def check_heading(heading, where: str, errors: list[str]) -> None:
    if not isinstance(heading, str) or not heading.strip() or "\n" in heading or len(heading) > LABEL_LIMIT:
        errors.append(f"{where} must be a line of at most {LABEL_LIMIT} characters")


def loader_name(loader: dict) -> str:
    """How messages name a loader: by its scope, or by its script when it has none."""
    scope = loader.get("scope")
    return repr(scope) if scope is not None else f"of {loader.get('script')!r}"


def lint_loader(game: Game, loader: dict, source: str, ids: dict[str, str], kind_tables: dict, errors: list[str],
                loaded_by_kind: dict[str, dict] | None = None) -> None:
    """A loader (game data export plan §4.2): its key kind, the scope whose tables it reads (or none, when each table
    names its registry table: one that no data file edits), the range of keys it runs for, how many steps it runs
    each key, its script, seeds and point."""
    where = f"{source}: loader {loader_name(loader)}"
    unknown_keys(loader, LOADER_KEYS, where, errors)
    scope = None
    if "scope" in loader:
        scope = game.scopes.get(loader.get("scope"))
        if scope is None:
            errors.append(f"{where}: scope {loader.get('scope')!r} is not a [[scope]] of the registry")
    keys = loader.get("keys")
    key_table = None
    if not isinstance(keys, str) or not ids.get(keys, "").startswith("the id"):
        errors.append(f"{where}: keys {keys!r} is not the id of a kind")
    else:
        key_table = game.tables.get(kind_tables.get(keys))
    if key_table is not None:
        check_skip(game, key_table, loader.get("skip"), where, errors)
    key_range = loader.get("key_range")
    if key_range is not None and not is_key_range(key_range):
        errors.append(f"{where}: key_range must be [first, last], whole numbers with 0 <= first <= last")
        key_range = None
    steps = loader.get("steps")
    if steps is not None and not (isinstance(steps, int) and not isinstance(steps, bool) and 1 <= steps <= 1000):
        errors.append(f"{where}: steps must be a whole number from 1 to 1000, the runs for each key")
        steps = None
    tables = lint_loaded_tables(game, loader.get("tables", {}), scope, "scope" in loader, key_table, where, errors)
    if loaded_by_kind is not None and isinstance(keys, str):
        loaded_by_kind.setdefault(keys, {}).update(tables)
    check_scripts([loader.get("script")] if isinstance(loader.get("script"), str) else loader.get("script"),
                  f"{where}: script", errors, patterns=False)
    if loader.get("read_at") not in game.points:
        errors.append(f"{where}: reads at {loader.get('read_at')!r}, which is no patch point of the game")
    check_seed(game, loader.get("seed"), where, errors, key_range=key_range, steps=steps, indexed=True)
    seeds = loader.get("seed") if isinstance(loader.get("seed"), list) else []
    if steps is None and any(STEP in (entry.get("index") or []) for entry in seeds if isinstance(entry, dict)):
        errors.append(f"{where}: a seed is indexed by the step, and the loader has no steps")
    if "builders" in loader:
        check_scripts(loader["builders"], f"{where}: builders", errors, patterns=True)


def lint_loaded_tables(game: Game, tables, scope: dict | None, scoped: bool, key_table: dict | None, where: str,
                       errors: list[str]) -> dict[str, tuple[dict, dict]]:
    """A loader's published tables (game data export plan §4.2): each a table of its scope, by its short name, or, for
    a loader with no scope, the registry table its `table` names; with `exists`, `fields` and `text` as a kind has
    them, `literals` (a script's string a record shows in place of its text field's), `notes`, and `parts` (the rows
    the site shows apart, export plan §5). The rows go under the key kind's entities. Returns each table's registry
    table and declaration, by short name."""
    found: dict[str, tuple[dict, dict]] = {}
    part_names: dict[str, str] = {}
    if not isinstance(tables, dict):
        errors.append(f"{where}: tables must be a table of short name = {{ exists, fields }}")
        return found
    shorts = (scope or {}).get("tables") or {}
    key_fields = set((key_table or {}).get("fields") or {})
    for short, entry in tables.items():
        twhere = f"{where}: table {short!r}"
        if not isinstance(entry, dict):
            errors.append(f"{twhere} must be a table of exists and fields")
            continue
        unknown_keys(entry, LOADED_TABLE_KEYS, twhere, errors)
        if not IDENTIFIER.fullmatch(short):
            errors.append(f"{twhere}: a table's short name must be a lowercase identifier")
        if scoped:
            if "table" in entry:
                errors.append(f"{twhere}: a loader with a scope names its tables by the scope's short names, not by "
                              f"table")
            table = game.tables.get(shorts.get(short))
            if table is None:
                if scope is not None:
                    errors.append(f"{twhere} is not a table of scope {scope['name']} (its tables: "
                                  f"{', '.join(sorted(shorts)) or 'none'})")
                continue
        else:
            table = game.tables.get(entry.get("table"))
            if table is None:
                errors.append(f"{twhere}: a loader with no scope names each table's registry table: "
                              f"table {entry.get('table')!r} is not a [[table]] of the registry")
                continue
            owner = next((s["name"] for s in game.scopes.values() if table["name"] in (s.get("tables") or {}).values()),
                         None)
            if owner is not None:
                errors.append(f"{twhere}: table {table['name']} is a table of scope {owner}, which a loader with that "
                              f"scope reads")
        # Joins (export plan §5): `table = field`, a table the row's reference in field reaches; the row publishes the
        # record it names as its own fields, written table.field (a stage's dig sites take their dig table's items).
        base_table = table
        joins = entry.get("joins", {})
        if not isinstance(joins, dict):
            errors.append(f"{twhere}: joins must be a table of table = the field that refers to it")
            joins = {}
        joined: dict[str, dict] = {}
        for jname, through in joins.items():
            jwhere = f"{twhere}: join {jname!r}"
            if jname in (table.get("fields") or {}):
                errors.append(f"{jwhere} has the name of a field of table {table['name']}")
                continue
            if jname not in game.tables:
                errors.append(f"{jwhere} is not a [[table]] of the registry")
                continue
            field = resolve_field(game, table, through, jwhere, errors)
            if field is None:
                continue
            reference = reference_of(table, through)
            if not field.single or "." in through:
                errors.append(f"{jwhere}: {through} must be one cell of the record, which names the joined record")
            elif reference is None or globals_build.ref_target(reference[2]) != jname:
                errors.append(f"{jwhere}: {through} does not refer to table {jname} ([table.refs])")
            else:
                joined[jname] = game.tables[jname]
        if joined:
            table = {**table, "_joins": joined}
        # Where (export plan §5): the values a record's field must hold to be a row (a map object's type, 32 or 33, a
        # dig site's).
        rules = entry.get("where", {})
        if not isinstance(rules, dict):
            errors.append(f"{twhere}: where must be a table of field = the values it may hold")
            rules = {}
        for wref, values in rules.items():
            field = resolve_field(game, base_table, wref, f"{twhere}: where", errors)
            if field is not None and (not field.single or field.kind != "integer"):
                errors.append(f"{twhere}: where {wref} must be one integer cell of the record")
            if not isinstance(values, list) or not values or not all(
                    isinstance(v, int) and not isinstance(v, bool) for v in values):
                errors.append(f"{twhere}: where {wref} must list the whole numbers a row's {wref} may hold")
        found[short] = (table, entry)
        if short in key_fields:
            errors.append(f"{twhere} has the name of a field of the key kind's table {key_table['name']}: the export "
                          f"writes a loaded field as <table>.<field>, which would read as that field's column")
        exists = resolve_field(game, base_table, entry.get("exists"), f"{twhere}: exists", errors)
        if exists is not None and not exists.single:
            errors.append(f"{twhere}: exists {entry.get('exists')} is a row of cells; name one column of it")
        shown: list[str] = []
        for label, want in (("fields", None), ("text", "string")):
            refs = entry.get(label, [])
            if not isinstance(refs, list):
                errors.append(f"{twhere}: {label} must be a list of fields")
                continue
            for ref in refs:
                field = resolve_field(game, table, ref, f"{twhere}: {label}", errors)
                if field is None:
                    continue
                shown.append(ref)
                if want is None and field.kind == "string":
                    errors.append(f"{twhere}: fields {ref} is a string field; list it under text")
                if want == "string" and field.kind != "string":
                    errors.append(f"{twhere}: text {ref} is not a string field; list it under fields")
                if field.state:
                    errors.append(f"{twhere}: {label} {ref} is saved state ({field.global_name} lies below the saved "
                                  f"prefix), so no loader publishes it")
        literals = entry.get("literals", [])
        if not isinstance(literals, list):
            errors.append(f"{twhere}: literals must be an array of {{ record, field, script, offset }}")
            literals = []
        seen: set[tuple] = set()
        for literal in literals:
            lwhere = f"{twhere}: literal"
            if not isinstance(literal, dict) or set(literal) != LITERAL_KEYS:
                errors.append(f"{lwhere} must be {{ record, field, script, offset }}: the record whose text field "
                              f"shows the string of the instruction at script@offset in its place")
                continue
            if literal["field"] not in (entry.get("text") or []):
                errors.append(f"{lwhere}: {literal['field']!r} is not one of the table's text fields")
            else:
                field = resolve_field(game, table, literal["field"], lwhere, [])
                if field is not None and not field.single:
                    errors.append(f"{lwhere}: {literal['field']} is a row of cells; a literal stands for one cell")
            record = literal["record"]
            if not (isinstance(record, int) and not isinstance(record, bool) and 0 <= record < table["records"]):
                errors.append(f"{lwhere}: record {record!r} is not one of table {table['name']}'s "
                              f"{table['records']} records")
            check_scripts([literal["script"]], f"{lwhere}: script", errors, patterns=False)
            offset = literal["offset"]
            if not (isinstance(offset, int) and not isinstance(offset, bool) and offset >= 0):
                errors.append(f"{lwhere}: offset must be the instruction's dword offset, a whole number")
            if (record, literal["field"]) in seen:
                errors.append(f"{lwhere}: record {record}'s {literal['field']} has two literals")
            seen.add((record, literal["field"]))
        notes = entry.get("notes", {})
        if not isinstance(notes, dict):
            errors.append(f"{twhere}: notes must be a table of field = text")
            notes = {}
        for key, note in notes.items():
            if key not in shown:
                errors.append(f"{twhere}: note {key!r} names none of the table's fields or text")
            check_note(note, f"{twhere}: note {key}", errors)
        check_labels(entry.get("labels", {}), set(shown), twhere, errors)
        check_parts(game, table, short, entry, shown, twhere, part_names, errors)
    for name, owner in part_names.items():
        if name in tables:
            errors.append(f"{where}: table {owner!r}'s part {name!r} has the name of one of the loader's tables")
    return found


def check_parts(game: Game, table: dict, short: str, entry: dict, shown: list[str], where: str,
                names: dict[str, str], errors: list[str]) -> None:
    """A loaded table's parts (game data export plan §5): each `{ name, field, global, same_as, note }`, the rows whose
    integer `field` indexes a cell of the integer list `global` that is not 0 and equals its cell `same_as`. The site
    shows each part apart under its name; a data file edits the table whole. `names` collects the loader's part names,
    each by the table it is a part of."""
    parts = entry.get("parts", [])
    if not isinstance(parts, list):
        errors.append(f"{where}: parts must be an array of {{ name, field, global, same_as }}")
        return
    if parts and "part" in shown:
        errors.append(f"{where}: a table with parts publishes no field named part, the column its rows name their "
                      f"part in")
    for part in parts:
        pwhere = f"{where}: part {part.get('name') if isinstance(part, dict) else part!r}"
        if not isinstance(part, dict) or not {"name", "field", "global", "same_as"} <= set(part):
            errors.append(f"{pwhere} must be {{ name, field, global, same_as }}, with an optional note")
            continue
        unknown_keys(part, PART_KEYS, pwhere, errors)
        name = part["name"]
        if not isinstance(name, str) or not IDENTIFIER.fullmatch(name):
            errors.append(f"{pwhere}: a part's name must be a lowercase identifier")
        elif name in names:
            errors.append(f"{pwhere}: the loader has two parts named {name}")
        else:
            names[name] = short
        field = resolve_field(game, table, part["field"], f"{pwhere}: field", errors)
        if field is not None and (field.kind != "integer" or not field.single):
            errors.append(f"{pwhere}: field {part['field']} is not one integer cell of a record")
        global_ = game.globals.get(part["global"]) if isinstance(part["global"], str) else None
        shape = LIST_TYPE.fullmatch(str((global_ or {}).get("type") or ""))
        if global_ is None:
            errors.append(f"{pwhere}: global {part['global']!r} is not a global of the registry")
        elif shape is None:
            errors.append(f"{pwhere}: global {part['global']} is {global_.get('type')}, not a list of integers")
        same_as = part["same_as"]
        if not isinstance(same_as, int) or isinstance(same_as, bool) or same_as < 0 \
                or (shape is not None and same_as >= int(shape.group(1))):
            errors.append(f"{pwhere}: same_as {same_as!r} is not a cell of {part['global']}")
        if "note" in part:
            check_note(part["note"], f"{pwhere}: note", errors)


def lint_relation(game: Game, relation: dict, source: str, ids: dict[str, str], relation_ids: set[str],
                  errors: list[str]) -> None:
    where = f"{source}: script_relation {relation.get('id')!r}"
    unknown_keys(relation, RELATION_KEYS, where, errors)
    for key in ("id", "negative"):
        value = relation.get(key)
        if value is None and key == "negative":
            continue
        if not isinstance(value, str) or not IDENTIFIER.fullmatch(value):
            errors.append(f"{where}: {key} must be a lowercase identifier")
        elif value in relation_ids:
            errors.append(f"{where}: relation {value} is defined twice")
        else:
            relation_ids.add(value)
    call = relation.get("call")
    check_scripts([call] if isinstance(call, str) else call, f"{where}: call", errors, patterns=False)
    kind = relation.get("kind")
    if not isinstance(kind, str) or not ids.get(kind, "").startswith("the id"):
        errors.append(f"{where}: kind {kind!r} is not the id of a kind")
    if relation.get("negative") is not None and relation.get("count") is None:
        errors.append(f"{where}: negative needs a count, whose sign it is")
    conditions = relation.get("where", {})
    if not isinstance(conditions, dict) or not all(isinstance(value, int) and not isinstance(value, bool)
                                                   for value in conditions.values()):
        errors.append(f"{where}: where must be a table of globals and whole numbers")
        conditions = {}
    for key, name, shape in ([("key", relation.get("key"), ONE_LIST), ("count", relation.get("count"), ONE_CELL)]
                             + [("where", name, ONE_CELL) for name in conditions]):
        if name is None and key == "count":
            continue
        entry = game.globals.get(name) if isinstance(name, str) else None
        type_ = str(entry.get("type") or "int") if entry is not None else ""
        if entry is None:
            errors.append(f"{where}: {key} {name!r} is not a global the registry names")
        elif bank(type_) != "G":
            errors.append(f"{where}: {key} {name} is not an integer global")
        elif not shape.fullmatch(type_):
            errors.append(f"{where}: {key} {name} is {type_}, not "
                          + ("an integer or a list of them" if shape is ONE_LIST else "one integer"))
    if "exclude" in relation:
        check_scripts(relation["exclude"], f"{where}: exclude", errors, patterns=True)


def render(profile_id: str, data: dict, source_bytes: bytes, source: str, scopes: dict) -> str:
    """The JSON the exporter reads: the declarations as written, with each optional list or table present, each
    loaded table's registry table beside its short name, and a loader's seed as the list of cells it writes."""
    export = data["export"]

    def loaded(loader: dict, short: str, entry: dict) -> dict:
        table = scopes[loader["scope"]]["tables"][short] if "scope" in loader else entry["table"]
        return {"table": table, "exists": entry["exists"], "fields": entry.get("fields", []),
                "text": entry.get("text", []), "literals": entry.get("literals", []), "notes": entry.get("notes", {}),
                "labels": entry.get("labels", {}), "parts": entry.get("parts", []), "where": entry.get("where", {}),
                "joins": entry.get("joins", {})}

    document = {
        "note": "DO NOT EDIT -- generated by tools/entities_build.py --build",
        "generated_from": source,
        "source_sha256": hashlib.sha256(source_bytes).hexdigest(),
        "profile": profile_id,
        "export": {"boot": export["boot"], "builders": export["builders"],
                   "excluded_builders": export.get("excluded_builders", []),
                   "derived": export.get("derived", []),
                   "text_scripts": export.get("text_scripts", []),
                   "explicit_pictures": export.get("explicit_pictures", []),
                   "text_overrides": export.get("text_overrides", {})},
        "kinds": [{"id": k["id"], "aliases": k.get("aliases", []), "table": k["table"], "exists": k["exists"],
                   "skip": k.get("skip", {}), "key_range": k.get("key_range"), "title": k.get("title"),
                   "titles": k.get("titles", {}), "text": k.get("text", []),
                   "fields": k.get("fields", []), "filters": k.get("filters", []), "pictures": k.get("pictures", {}),
                   "descriptions": k.get("description", []),
                   "groups": [{"name": g["name"], "fields": g["fields"], "labels": g.get("labels"),
                               "row_label": g.get("row_label")}
                              for g in k.get("group", [])],
                   "joins": k.get("joins", []),
                   "notes": k.get("notes", {}), "labels": k.get("labels", {}),
                   "qualifiers": k.get("qualifiers", []), "about": k.get("about"),
                   "marks": [{"field": field, "word": word} for field, word in k.get("marks", {}).items()],
                   "categories": [{"name": name, "rules": rules} for name, rules in k.get("categories", {}).items()],
                   "referenced_as": [
                       {"field": field, "heading": entry if isinstance(entry, str) else entry["heading"],
                        "details": [] if isinstance(entry, str) else entry.get("details", []),
                        "parts": {} if isinstance(entry, str) else entry.get("parts", {})}
                       for field, entry in k.get("referenced_as", {}).items()],
                   "text_blocks": {ref: [{"name": name, "first": run[0], "last": run[1]} for name, run in runs.items()]
                                   for ref, runs in k.get("text_blocks", {}).items()}}
                  for k in data.get("kind", [])],
        "loaders": [{"scope": l.get("scope"), "keys": l["keys"], "key_range": l.get("key_range"),
                     "steps": l.get("steps", 1), "skip": l.get("skip", {}), "script": l["script"],
                     "seed": seed_entries(l.get("seed")), "read_at": l["read_at"],
                     "builders": l.get("builders", []),
                     "tables": {short: loaded(l, short, entry) for short, entry in (l.get("tables") or {}).items()}}
                    for l in data.get("loader", [])],
        "script_relations": [{"id": r["id"], "negative": r.get("negative"), "call": r["call"], "kind": r["kind"],
                              "key": r["key"], "count": r.get("count"), "where": r.get("where", {}),
                              "exclude": r.get("exclude", [])}
                             for r in data.get("script_relation", [])],
    }
    return json.dumps(document, ensure_ascii=False, indent=2) + "\n"


def run(profile_id: str, write: bool, source: Path | None = None, registry: Path | None = None,
        points: Path | None = None, manifest: Path | None = None, output: Path | None = None) -> int:
    source = source or entities_source(profile_id)
    shown = base_mod_build._shown(source)
    raw = source.read_bytes()
    try:
        data = tomllib.loads(raw.decode("utf-8"))
    except (UnicodeDecodeError, tomllib.TOMLDecodeError) as error:
        print(f"error: {shown}: {error}")
        return 1
    game = Game(registry or base_mod_build.globals_source(profile_id),
                points or base_mod_build.points_source(profile_id),
                manifest or base_mod_build.profile_manifest(profile_id))
    errors = lint(game, data, shown)
    for message in errors:
        print(f"error: {message}")
    if errors:
        return 1
    kinds = len(data.get("kind", []))
    if not write:
        print(f"check: {shown}: {kinds} kinds lint clean")
        return 0
    output = output or entities_output(profile_id)
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(render(profile_id, data, raw, shown, game.scopes), encoding="utf-8", newline="\n")
    print(f"build: {shown}: {kinds} kinds -> {base_mod_build._shown(output)}")
    return 0


def main(argv=None) -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    mode = parser.add_mutually_exclusive_group(required=True)
    mode.add_argument("--build", action="store_true", help="lint, then write build/games/<game>/entities.json")
    mode.add_argument("--check", action="store_true", help="lint only")
    parser.add_argument("--all", action="store_true", help="every game with declarations (default: the --game one)")
    args = parser.parse_args(argv)
    games = profiles_with_entities() if args.all else [paths.CONTEXT.profile_id]
    if not args.all and not entities_source(games[0]).is_file():
        print(f"error: {base_mod_build._shown(entities_source(games[0]))} not found (pass --game, or --all)")
        return 1
    failed = False
    for profile_id in games:
        failed |= run(profile_id, write=args.build) != 0
    return 1 if failed else 0


if __name__ == "__main__":
    sys.exit(main())
