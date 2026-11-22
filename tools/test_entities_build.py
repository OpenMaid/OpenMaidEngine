"""Plain tests for entities_build.py: synthesized registries, patch points, profile manifests and declarations, no game
content; and Kamidori's declarations against the repository's registry."""
import json
import sys
import tempfile
from pathlib import Path

import entities_build as build

FAILS = []


def check(condition, message):
    (FAILS.append(message) or print("FAIL:", message)) if not condition else print("ok:", message)


def g(address, name, type_, columns=None):
    text = (f'[[global]]\naddress = "{address}"\nname = "{name}"\ncategory = "data-table"\ntype = "{type_}"\n'
            f'source = "investigation"\nconfidence = "high"\n')
    if columns:
        text += "columns = { " + ", ".join(f'"{i}" = "{n}"' for i, n in columns.items()) + " }\n"
    return text + "\n"


REGISTRY = "".join([
    g("0x10", "current_hero_id", "int"),
    g("0x20", "current_weapon_id", "int"),
    g("0x100", "hero_levels", "int[4]"),             # saved state: below the prefix 0x200
    g("0x300", "hero_classes", "int[4]"),
    g("0x310", "hero_weapon_ids", "int[4][2]"),
    g("0x320", "hero_art_ids", "int[4][3]", {"0": "face", "2": "map"}),
    g("0x340", "hero_handler_ids", "int[4]"),
    g("0x10", "hero_names", "string[4]"),
    g("0x20", "hero_bios", "string[4][2]"),
    g("0x400", "weapon_prices", "int[3]"),
    g("0x40", "weapon_names", "string[3]"),
    g("0x410", "foe_hero_ids", "int[5]"),
    g("0x420", "foe_levels", "int[5][2]"),
    g("0x50", "foe_names", "string[5]"),
    g("0x440", "trap_damage", "int[5]"),
    g("0x30", "joining_hero_ids", "int[2]"),
    g("0x32", "joining_hero_count", "int"),
]) + '''
[[table]]
name = "hero"
records = 4
built_by = "boot"
[table.fields]
level = "hero_levels"
class = "hero_classes"
weapon = "hero_weapon_ids"
art = "hero_art_ids"
handler = "hero_handler_ids"
name = "hero_names"
bio = "hero_bios"
[table.refs]
weapon = "weapon"
art = "picture"

[[table]]
name = "weapon"
records = 3
built_by = "boot"
[table.fields]
price = "weapon_prices"
name = "weapon_names"

[[table]]
name = "foe"
records = 5
built_by = "stage"
[table.fields]
hero = "foe_hero_ids"
levels = "foe_levels"
name = "foe_names"
[table.refs]
hero = "hero"

[[table]]
name = "trap"
records = 5
built_by = "stage"
[table.fields]
damage = "trap_damage"

[[scope]]
name = "stage"
first = 1
last = 9
[scope.tables]
foe = "foe"
art = "trap"
'''

POINTS = '''
[[point]]
name = "demo.boot.data_ready"
script = "INIT2.BIN"
description = "the exit"
[[point.at]]
fingerprint = "sha256:00"
site = 0x10

[[point]]
name = "demo.stage.loaded"
script = "FIELD.BIN"
description = "a stage's tables"
[[point.at]]
fingerprint = "sha256:01"
site = 0x20
'''

MANIFEST = {"persistence": {"bankDimensions": {"integerGlobals": 0x200, "floatGlobals": 0, "stringGlobals": 0}}}

GOOD = '''
[export]
boot = { script = "INIT2", read_at = "demo.boot.data_ready" }
builders = ["INIT2", "*INIT"]
excluded_builders = ["CGINIT"]
text_scripts = ["HEROMES"]
explicit_pictures = ["HR*B", "HR001A"]

[[kind]]
id = "hero"
aliases = ["heroes"]
table = "hero"
exists = "name"
title = "name"
text = ["bio"]
fields = ["class", "weapon"]
filters = ["class"]
pictures = { face = "art.face", other = "art.1" }

[[kind.description]]
name = "story"
script = { field = "handler" }
seed = { current_hero_id = "key", current_weapon_id = 0 }

[[kind.description]]
name = "motto"
script = { name = "HEROMES" }

[[kind]]
id = "weapon"
table = "weapon"
exists = "price"
title = "name"

[[loader]]
scope = "stage"
keys = "hero"
script = "FIELD"
seed = { current_hero_id = "key" }
read_at = "demo.stage.loaded"
builders = ["FOEINIT"]
skip = { class = 9 }

[loader.tables.foe]
exists = "hero"
fields = ["hero", "levels"]

[[script_relation]]
id = "given_in"
call = "ADDWEAPON"
kind = "weapon"
key = "current_weapon_id"
exclude = ["DEBUG*"]

[[script_relation]]
id = "joins_in"
call = "JOINHERO"
kind = "hero"
key = "joining_hero_ids"
where = { joining_hero_count = 1 }
'''


def run(declarations: str, write: bool = False):
    """Lint (and build) the declarations against the synthetic game; the exit code, the output and the JSON."""
    import contextlib, io
    with tempfile.TemporaryDirectory() as tmp:
        root = Path(tmp)
        (root / "globals.toml").write_text(REGISTRY, encoding="utf-8")
        (root / "points.toml").write_text(POINTS, encoding="utf-8")
        (root / "demo.json").write_text(json.dumps(MANIFEST), encoding="utf-8")
        (root / "entities.toml").write_text(declarations, encoding="utf-8")
        out = io.StringIO()
        with contextlib.redirect_stdout(out):
            code = build.run("demo", write, source=root / "entities.toml", registry=root / "globals.toml",
                             points=root / "points.toml", manifest=root / "demo.json", output=root / "entities.json")
        written = (root / "entities.json").read_text(encoding="utf-8") if (root / "entities.json").is_file() else None
        return code, out.getvalue(), written


def test_script_names():
    check(build.script_name("$1$CGINIT.BIN") == "CGINIT", "a script name drops the $n$ prefix and the extension")
    check(build.script_name("init2.bin") == "INIT2", "a script name is upper case")


def test_good_declarations():
    code, output, written = run(GOOD, write=True)
    check(code == 0, f"good declarations lint clean ({output.strip()})")
    data = json.loads(written or "{}")
    check([k["id"] for k in data.get("kinds", [])] == ["hero", "weapon"], "the JSON lists the kinds in order")
    hero = data["kinds"][0] if data.get("kinds") else {}
    check(hero.get("descriptions", [{}])[0].get("seed") == {"current_hero_id": "key", "current_weapon_id": 0},
          "a description keeps its seed")
    weapon = data["kinds"][1] if len(data.get("kinds", [])) > 1 else {}
    check(weapon.get("text") == [] and weapon.get("pictures") == {} and weapon.get("descriptions") == [],
          "an optional list or table is present and empty")
    check(data.get("loaders", [{}])[0].get("builders") == ["FOEINIT"], "a loader keeps its own builders")
    check(data.get("loaders", [{}])[0].get("tables") == {"foe": {"table": "foe", "exists": "hero",
                                                               "fields": ["hero", "levels"]}},
          "a loader's published table names its registry table beside its short name")
    check(data.get("script_relations", [{}])[0].get("count") is None, "a relation without a count has count null")
    check(data.get("script_relations", [{}])[0].get("where") == {}, "a relation without a where has an empty one")
    check(data.get("script_relations", [{}, {}])[1].get("where") == {"joining_hero_count": 1},
          "a relation keeps its where")
    check(len(data.get("source_sha256", "")) == 64, "the JSON records the source's hash")
    check(data.get("export", {}).get("explicit_pictures") == ["HR*B", "HR001A"], "the JSON keeps the explicit pictures")
    code, _, written_again = run(GOOD, write=True)
    same = {k: v for k, v in json.loads(written_again or "{}").items() if k != "generated_from"}
    check(same == {k: v for k, v in data.items() if k != "generated_from"},
          "building twice writes the same JSON (its source's path aside: each run has its own folder)")


def errors_of(declarations: str) -> str:
    code, output, written = run(declarations, write=True)
    check(code == 1 and written is None, "bad declarations fail and write nothing")
    return output


def test_export_errors():
    output = errors_of('''
[export]
boot = { script = "init2", read_at = "demo.nowhere" }
builders = ["init*"]
excluded_builders = ["CG*"]
text_scripts = ["CG*", "HEROMES"]
explicit_pictures = ["hr*b", "HR1.PNG", "HR2", "HR2"]
extra = 1
''')
    for needle in ("boot script: 'init2' must be a script name", "reads at 'demo.nowhere', which is no patch point",
                   "builders: 'init*' must be", "excluded_builders: 'CG*' must be", "unknown key 'extra'",
                   "explicit_pictures: 'hr*b' must be a picture's catalog name",
                   "explicit_pictures: 'HR1.PNG' must be", "explicit_pictures: HR2 is named twice"):
        check(needle in output, f"the lint flags {needle!r}")
    output = errors_of('[export]\nboot = { script = "INIT2", read_at = "demo.boot.data_ready" }\nbuilders = []\n'
                       'excluded_builders = ["HEROMES"]\ntext_scripts = ["HEROMES"]\n')
    check("builders must name at least one script" in output, "the lint wants a builder")
    check("text script HEROMES is also an excluded builder" in output, "a text script may not be an excluded builder")
    check("needs an [export] table" in errors_of('[[kind]]\nid = "x"\n'), "the lint wants an [export] table")


EXPORT = GOOD.split("[[kind]]")[0]


def test_kind_errors():
    output = errors_of(EXPORT + '''
[[kind]]
id = "tooltips"
table = "hero"
exists = "bio"
title = "class"
text = ["class", "nope"]
fields = ["name", "level", "art.face", "weapon"]
filters = ["weapon", "bio"]
pictures = { icon = "class", face = "art.0", far = "art.7", Bad = "art.1" }
colour = "red"

[[kind]]
id = "heroes"
aliases = ["tooltips"]
table = "monster"

[[kind]]
id = "picture"
table = "hero"
''')
    for needle in (
            "tooltips is a path of the site's database", "picture is a table of the export's database",
            "unknown key 'colour'",
            "exists bio is a row of cells",
            "title class is an integer field the registry's [table.refs] does not mark as referring to a table",
            "text class is not a string field", "'nope' is not a field of table hero",
            "fields name is a string field", "fields level is saved state",
            "filter 'bio' is not one of the kind's fields", "filter weapon is a row of cells",
            "picture icon reads class, which the registry's [table.refs] does not mark as picture",
            "column 0 of art is named: write art.face", "art has no column '7'",
            "picture name 'Bad' must be a lowercase identifier",
            "kind 'heroes': tooltips is also the id of kind 'tooltips'",
            "table 'monster' is not a [[table]] of the registry"):
        check(needle in output, f"the lint flags {needle!r}")


def test_titles():
    """A title is a string field, a reference whose target's title the entity takes, or none (export plan §4.1)."""
    no_title = GOOD.replace('exists = "price"\ntitle = "name"\n', 'exists = "price"\n')
    code, output, written = run(no_title, write=True)
    check(code == 0, f"a kind may have no title ({output.strip()})")
    kinds = json.loads(written or "{}").get("kinds", [])
    check(len(kinds) > 1 and kinds[1].get("title") is None, "a kind without a title has title null in the JSON")
    by_weapon = GOOD.replace('title = "name"\ntext = ["bio"]', 'title = "weapon.0"\ntext = ["bio"]')
    code, output, _ = run(by_weapon, write=True)
    check(code == 0, f"a reference to a shown kind's table may be a title ({output.strip()})")
    output = errors_of(EXPORT + '''
[[kind]]
id = "hero"
table = "hero"
exists = "name"
title = "weapon.1"

[[kind]]
id = "hero_by_art"
table = "hero"
exists = "name"
title = "art.face"
fields = ["art"]
''')
    for needle in ("title weapon.1 is a reference, which the kind must publish: list weapon under fields",
                   "title art.face is an integer field the registry's [table.refs] does not mark as referring to a table",
                   "kind 'hero': title weapon.1 refers to table weapon, which no kind shows"):
        check(needle in output, f"the lint flags {needle!r}")


def test_description_errors():
    output = errors_of(EXPORT + '''
[[kind]]
id = "hero"
table = "hero"
exists = "name"
title = "name"

[[kind.description]]
name = "class"
script = { name = "SC0100" }
seed = { nobody = "key", hero_names = 1, current_hero_id = "value" }

[[kind.description]]
name = "other"
script = { field = "bio", name = "HEROMES" }
''')
    for needle in ("description 'class': class is also a field of table hero",
                   "script SC0100 is not one of [export] text_scripts", "seed names 'nobody'",
                   "seed names hero_names, which is not an integer global",
                   "seed gives current_hero_id 'value'", "script must be { field = FIELD }"):
        check(needle in output, f"the lint flags {needle!r}")


def test_loader_and_relation_errors():
    output = errors_of(EXPORT + '''
[[kind]]
id = "hero"
table = "hero"
exists = "name"
title = "name"

[[loader]]
scope = "dungeon"
keys = "monster"
script = "field"
read_at = "demo.stage.unloaded"

[[loader]]
scope = "stage"
keys = "hero"
script = "FIELD"
read_at = "demo.stage.loaded"
skip = { kind = 1 }
[loader.tables.foe]
exists = "levels"
fields = ["hero", "name", "speed"]
colour = 1
[loader.tables.art]
exists = "damage"
[loader.tables.boss]
exists = "hero"

[[script_relation]]
id = "given_in"
call = "ADDWEAPON"
kind = "weapon"
key = "current_monster_id"

[[script_relation]]
id = "given_in"
negative = "Taken"
call = "ADDHERO"
kind = "hero"
key = "hero_names"
count = "current_hero_id"

[[script_relation]]
id = "found_in"
negative = "lost_in"
call = "ADDHERO"
kind = "hero"
key = "hero_weapon_ids"
where = { hero_classes = 1 }

[[script_relation]]
id = "met_in"
call = "ADDHERO"
kind = "hero"
key = "current_hero_id"
count = "joining_hero_ids"
where = { joining_hero_count = "one" }
''')
    for needle in ("scope 'dungeon' is not a [[scope]]", "keys 'monster' is not the id of a kind",
                   "script: 'field' must be", "reads at 'demo.stage.unloaded'",
                   "kind 'weapon' is not the id of a kind", "key 'current_monster_id' is not a global",
                   "relation given_in is defined twice", "negative must be a lowercase identifier",
                   "key hero_names is not an integer global",
                   "script_relation 'found_in': negative needs a count", "where must be a table of globals and whole",
                   "key hero_weapon_ids is int[4][2], not an integer or a list of them",
                   "count joining_hero_ids is int[2], not one integer",
                   "where hero_classes is int[4], not one integer",
                   "skip: 'kind' is not a field of table hero", "table 'foe': unknown key 'colour'",
                   "exists levels is a row of cells", "fields name is a string field",
                   "fields: 'speed' is not a field of table foe",
                   "table 'art' has the name of a field of the key kind's table hero",
                   "table 'boss' is not a table of scope stage (its tables: art, foe)"):
        check(needle in output, f"the lint flags {needle!r}")


def test_the_repository_declarations():
    for profile_id in build.profiles_with_entities():
        import contextlib, io
        out = io.StringIO()
        with contextlib.redirect_stdout(out):
            code = build.run(profile_id, write=False)
        check(code == 0, f"{profile_id}'s declarations lint clean ({out.getvalue().strip()})")
    check("kamidori" in build.profiles_with_entities(), "Kamidori has declarations")


def main():
    test_script_names()
    test_good_declarations()
    test_export_errors()
    test_kind_errors()
    test_description_errors()
    test_loader_and_relation_errors()
    test_the_repository_declarations()
    print("FAILURES:", len(FAILS))
    return 1 if FAILS else 0


if __name__ == "__main__":
    sys.exit(main())
