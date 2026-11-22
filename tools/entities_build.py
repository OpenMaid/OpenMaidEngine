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
# The site's second path segment under a game is a kind's id, except these (site design §4).
RESERVED_KINDS = {"tooltips"}
# A kind's id names its table in the export's database, beside these (game data export spec §7.2); Ome.Export refuses
# them too (GameDataExporter.FixedTables).
EXPORT_TABLES = {"build_info", "kind", "field", "field_group", "text", "ref", "relation", "picture", "cell", "snippet",
                 "script_use", "loaded", "loaded_field"}
KEY = "key"

EXPORT_KEYS = {"boot", "builders", "excluded_builders", "text_scripts", "explicit_pictures"}
KIND_KEYS = {"id", "aliases", "table", "exists", "skip", "title", "text", "fields", "filters", "pictures",
             "description", "group"}
DESCRIPTION_KEYS = {"name", "script", "seed"}
GROUP_KEYS = {"name", "fields"}
LOADER_KEYS = {"scope", "keys", "skip", "script", "seed", "read_at", "builders", "tables"}
LOADED_TABLE_KEYS = {"exists", "fields"}
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
        dims = globals_build.dims_of(type_) or [records]
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
    dims = globals_build.dims_of(entry.get("type")) or []
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


def check_seed(game: Game, seed, where: str, errors: list[str]) -> None:
    if seed is None:
        return
    if not isinstance(seed, dict) or not seed:
        errors.append(f"{where}: seed must be a table of global = \"{KEY}\" or a whole number")
        return
    for name, value in seed.items():
        entry = game.globals.get(name)
        if entry is None:
            errors.append(f"{where}: seed names {name!r}, which the registry does not name")
        elif bank(str(entry.get("type") or "int")) != "G":
            errors.append(f"{where}: seed names {name}, which is not an integer global")
        if not (value == KEY or isinstance(value, int) and not isinstance(value, bool)):
            errors.append(f"{where}: seed gives {name} {value!r}, not \"{KEY}\" or a whole number")


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
    for loader in data.get("loader", []):
        lint_loader(game, loader, source, ids, kind_tables, errors)
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


def lint_loader(game: Game, loader: dict, source: str, ids: dict[str, str], kind_tables: dict, errors: list[str]) -> None:
    where = f"{source}: loader {loader.get('scope')!r}"
    unknown_keys(loader, LOADER_KEYS, where, errors)
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
    lint_loaded_tables(game, loader.get("tables", {}), scope, key_table, where, errors)
    check_scripts([loader.get("script")] if isinstance(loader.get("script"), str) else loader.get("script"),
                  f"{where}: script", errors, patterns=False)
    if loader.get("read_at") not in game.points:
        errors.append(f"{where}: reads at {loader.get('read_at')!r}, which is no patch point of the game")
    check_seed(game, loader.get("seed"), where, errors)
    if "builders" in loader:
        check_scripts(loader["builders"], f"{where}: builders", errors, patterns=True)


def lint_loaded_tables(game: Game, tables, scope: dict | None, key_table: dict | None, where: str,
                       errors: list[str]) -> None:
    """A loader's published tables (game data export plan §4.2): each a table of its scope, by its short name, with
    `exists` and `fields` as a kind has them; the rows go under the key kind's entities."""
    if not isinstance(tables, dict):
        errors.append(f"{where}: tables must be a table of short name = {{ exists, fields }}")
        return
    shorts = (scope or {}).get("tables") or {}
    key_fields = set((key_table or {}).get("fields") or {})
    for short, entry in tables.items():
        twhere = f"{where}: table {short!r}"
        if not isinstance(entry, dict):
            errors.append(f"{twhere} must be a table of exists and fields")
            continue
        unknown_keys(entry, LOADED_TABLE_KEYS, twhere, errors)
        table = game.tables.get(shorts.get(short))
        if table is None:
            if scope is not None:
                errors.append(f"{twhere} is not a table of scope {scope['name']} (its tables: "
                              f"{', '.join(sorted(shorts)) or 'none'})")
            continue
        if short in key_fields:
            errors.append(f"{twhere} has the name of a field of the key kind's table {key_table['name']}: the export "
                          f"writes a loaded field as <table>.<field>, which would read as that field's column")
        exists = resolve_field(game, table, entry.get("exists"), f"{twhere}: exists", errors)
        if exists is not None and not exists.single:
            errors.append(f"{twhere}: exists {entry.get('exists')} is a row of cells; name one column of it")
        fields = entry.get("fields", [])
        if not isinstance(fields, list):
            errors.append(f"{twhere}: fields must be a list of fields")
            continue
        for ref in fields:
            field = resolve_field(game, table, ref, f"{twhere}: fields", errors)
            if field is None:
                continue
            if field.kind == "string":
                errors.append(f"{twhere}: fields {ref} is a string field; a loaded table's text is not published")
            if field.state:
                errors.append(f"{twhere}: fields {ref} is saved state ({field.global_name} lies below the saved "
                              f"prefix), so no loader publishes it")


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
    """The JSON the exporter reads: the declarations as written, with each optional list or table present, and each
    loaded table's registry table beside its short name."""
    export = data["export"]
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
                   "groups": [{"name": g["name"], "fields": g["fields"]} for g in k.get("group", [])]}
                  for k in data.get("kind", [])],
        "loaders": [{"scope": l["scope"], "keys": l["keys"], "skip": l.get("skip", {}), "script": l["script"],
                     "seed": l.get("seed", {}), "read_at": l["read_at"], "builders": l.get("builders", []),
                     "tables": {short: {"table": scopes[l["scope"]]["tables"][short], "exists": entry["exists"],
                                        "fields": entry.get("fields", [])}
                                for short, entry in (l.get("tables") or {}).items()}}
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
