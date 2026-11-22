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
                 "qualifier", "referenced_as",
                 "relation", "picture", "cell", "snippet", "script_use", "loaded", "loaded_field", "loaded_part",
                 "label", "value_format", "value_name"}
KEY = "key"
STEP = "step"
EVERY = "*"

EXPORT_KEYS = {"boot", "builders", "excluded_builders", "text_scripts", "explicit_pictures"}
KIND_KEYS = {"id", "aliases", "table", "exists", "skip", "title", "text", "fields", "filters", "pictures",
             "description", "group", "notes", "labels", "text_blocks", "qualifiers", "referenced_as", "about"}
# A kind's `about` is the line the site shows on its card and its list: one line, a reader's.
ABOUT_LIMIT = 160
REFERENCED_AS_KEYS = {"heading", "details", "parts"}
QUALIFIER_LIMIT = 3
DETAIL_LIMIT = 4
DESCRIPTION_KEYS = {"name", "script", "seed"}
GROUP_KEYS = {"name", "fields", "labels"}
SHEET_KEYS = {"field", "sheet", "first", "columns", "origin", "step", "size"}
# A picture sheet's catalog record, with its extension (SO009.AGF).
SHEET = re.compile(r"[A-Z0-9_]+\.[A-Z0-9]+")
LOADER_KEYS = {"scope", "keys", "key_range", "steps", "skip", "script", "seed", "read_at", "builders", "tables"}
LOADED_TABLE_KEYS = {"table", "exists", "fields", "text", "literals", "notes", "labels", "parts"}
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
        self.kind = {"GS": "string", "GF": "float", "G": "integer"}[bank(type_)]
        dims = globals_build.record_dims(type_, records) or [records]
        self.single = len(dims) == 1 or column is not None
        width = dims[1] if len(dims) == 2 else 1
        self.width = 1 if self.single else width
        end = globals_build._parse_addr(entry["address"]) + records * width
        prefix = (prefixes or {}).get(bank(type_))
        self.state = prefix is not None and end <= prefix


def resolve_field(game: Game, table: dict, ref, where: str, errors: list[str]) -> Field | None:
    """`field` or `field.column` of `table`, or None (the error appended)."""
    if not isinstance(ref, str):
        errors.append(f"{where}: {ref!r} is not a field name")
        return None
    field, _, column = ref.partition(".")
    fields = table.get("fields") or {}
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


def unknown_keys(entry: dict, allowed: set[str], where: str, errors: list[str]) -> None:
    for key in sorted(set(entry) - allowed):
        errors.append(f"{where}: unknown key {key!r}")


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
    text_scripts = set(export.get("text_scripts") or [])
    check_picture_patterns(export.get("explicit_pictures", []), f"{source}: [export] explicit_pictures", errors)

    kinds = data.get("kind", [])
    if not isinstance(kinds, list):
        errors.append(f"{source}: kind must be an array of tables ([[kind]])")
        kinds = []
    ids: dict[str, str] = {}
    for kind in kinds:
        lint_kind(game, kind, source, text_scripts, ids, errors)
    # A reference title takes its target's title, so a kind must show the target's table.
    declared = {kind.get("table") for kind in kinds if isinstance(kind, dict)}
    for kind in kinds:
        table = game.tables.get(kind.get("table")) if isinstance(kind, dict) else None
        title = kind.get("title") if table else None
        target = (table.get("refs") or {}).get(title.partition(".")[0]) if isinstance(title, str) else None
        if target and target != globals_build.REF_PICTURE and target not in declared:
            errors.append(f"{source}: kind {kind.get('id')!r}: title {title} refers to table {target}, which no kind "
                          f"shows, so it has no title to take")

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


def lint_kind(game: Game, kind: dict, source: str, text_scripts: set[str], ids: dict[str, str],
              errors: list[str]) -> None:
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
    # (a recipe is named by the item it makes), or none, and the site names an entity by its kind and id.
    if "title" in kind:
        title = kind["title"]
        field = one(title, "title", None)
        if field is not None and field.kind == "integer":
            base = str(title).partition(".")[0]
            target = (table.get("refs") or {}).get(base)
            if target is None or target == globals_build.REF_PICTURE:
                errors.append(f"{where}: title {title} is an integer field the registry's [table.refs] does not mark as "
                              f"referring to a table; a title is a string field or a reference")
            elif base not in kind.get("fields", []):
                errors.append(f"{where}: title {title} is a reference, which the kind must publish: list {base} under "
                              f"fields")
        elif field is not None and field.kind != "string":
            errors.append(f"{where}: title {title} must be a string field or a reference")
        published(title, "title", field)
    check_skip(game, table, kind.get("skip"), where, errors)

    text = kind.get("text", [])
    fields = kind.get("fields", [])
    for label, refs, want in (("text", text, "string"), ("fields", fields, None)):
        if not isinstance(refs, list):
            errors.append(f"{where}: {label} must be a list of fields")
            continue
        for ref in refs:
            field = resolve_field(game, table, ref, f"{where}: {label}", errors)
            if field is None:
                continue
            if want == "string" and field.kind != "string":
                errors.append(f"{where}: text {ref} is not a string field; list it under fields")
            if label == "fields" and field.kind == "string":
                errors.append(f"{where}: fields {ref} is a string field; strings are published only under text "
                              f"(export spec §5.5)")
            published(ref, label, field)
    for ref in kind.get("filters", []):
        if ref not in fields:
            errors.append(f"{where}: filter {ref!r} is not one of the kind's fields")
        else:
            one(ref, "filter", None)

    pictures = kind.get("pictures", {})
    refs = table.get("refs") or {}
    if not isinstance(pictures, dict):
        errors.append(f"{where}: pictures must be a table of name = field")
        pictures = {}
    for name, ref in pictures.items():
        if not IDENTIFIER.fullmatch(name):
            errors.append(f"{where}: picture name {name!r} must be a lowercase identifier")
        if isinstance(ref, dict):
            check_sheet(ref, lambda field, label=f"picture {name}": one(field, label, "integer"),
                        f"{where}: picture {name}", errors)
            continue
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
            field = resolve_field(game, table, ref, f"{gwhere}: fields", errors)
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

    check_text_blocks(game, table, kind, where, errors)

    # A note (the user, 2026-11-22): a line that explains a detail, keyed by what the infobox shows as one row: a field
    # outside every group, or a group.
    notes = kind.get("notes", {})
    if not isinstance(notes, dict):
        errors.append(f"{where}: notes must be a table of field or group = text")
        notes = {}
    group_names = {group.get("name") for group in groups if isinstance(group, dict)}
    shown_fields = set(published_fields) | set(text if isinstance(text, list) else [])
    for key, note in notes.items():
        if key in grouped:
            errors.append(f"{where}: note {key}: the field is shown in group {grouped[key]!r}; note the group")
        elif key not in shown_fields and key not in group_names:
            errors.append(f"{where}: note {key!r} names none of the kind's fields, text or groups")
        check_note(note, f"{where}: note {key}", errors)
    # A label (export plan §5): a reader's name for a shown field, grouped or not, or a group.
    check_labels(kind.get("labels", {}), shown_fields | group_names, where, errors)
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
    """A picture cut from a sheet: `field` numbers a cell, the first being `first`, `columns` to a row from `origin`,
    `step` apart, each `size` (pixels, x before y). A number below `first` is no picture."""
    unknown_keys(sheet, SHEET_KEYS, where, errors)
    missing = sorted(SHEET_KEYS - set(sheet))
    if missing:
        errors.append(f"{where}: a sheet picture needs {', '.join(missing)}")
        return
    check_field(sheet["field"])
    if not isinstance(sheet["sheet"], str) or not SHEET.fullmatch(sheet["sheet"]):
        errors.append(f"{where}: sheet {sheet['sheet']!r} must be a picture's catalog name in upper case with its "
                      f"extension, without its `$n$` prefix")
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
                  if (f := resolve_field(game, table, ref, gwhere, [])) is not None and not f.single]
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
    the loaded table's parts that its rows read under (a stage's allies)."""
    table = game.tables.get(kind.get("table"))
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
        target = (owner.get("refs") or {}).get(name)
        if name not in published:
            errors.append(f"{rwhere}: {key!r} is not a published field" + (f" of table {short}" if dot else ""))
            continue
        if target is None or target == globals_build.REF_PICTURE:
            errors.append(f"{rwhere}: {key} is not a reference to a table ([table.refs])")
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
    if key_range is not None and not (isinstance(key_range, list) and len(key_range) == 2
                                      and all(isinstance(v, int) and not isinstance(v, bool) for v in key_range)
                                      and 0 <= key_range[0] <= key_range[1]):
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
        found[short] = (table, entry)
        if short in key_fields:
            errors.append(f"{twhere} has the name of a field of the key kind's table {key_table['name']}: the export "
                          f"writes a loaded field as <table>.<field>, which would read as that field's column")
        exists = resolve_field(game, table, entry.get("exists"), f"{twhere}: exists", errors)
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
                "labels": entry.get("labels", {}), "parts": entry.get("parts", [])}

    document = {
        "note": "DO NOT EDIT -- generated by tools/entities_build.py --build",
        "generated_from": source,
        "source_sha256": hashlib.sha256(source_bytes).hexdigest(),
        "profile": profile_id,
        "export": {"boot": export["boot"], "builders": export["builders"],
                   "excluded_builders": export.get("excluded_builders", []),
                   "text_scripts": export.get("text_scripts", []),
                   "explicit_pictures": export.get("explicit_pictures", [])},
        "kinds": [{"id": k["id"], "aliases": k.get("aliases", []), "table": k["table"], "exists": k["exists"],
                   "skip": k.get("skip", {}), "title": k.get("title"), "text": k.get("text", []),
                   "fields": k.get("fields", []), "filters": k.get("filters", []), "pictures": k.get("pictures", {}),
                   "descriptions": k.get("description", []),
                   "groups": [{"name": g["name"], "fields": g["fields"], "labels": g.get("labels")}
                              for g in k.get("group", [])],
                   "notes": k.get("notes", {}), "labels": k.get("labels", {}),
                   "qualifiers": k.get("qualifiers", []), "about": k.get("about"),
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
