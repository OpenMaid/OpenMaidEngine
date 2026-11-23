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
    g("0x350", "hero_weapon_levels", "int[4][2]"),
    g("0x360", "hero_trait_ids", "int[4][4]"),
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
    g("0x460", "hero_rule_state", "int[4][5]"),
    g("0x60", "bonus_title", "string"),
    g("0x480", "bonus_level", "int"),
    g("0x490", "bonus_stats", "int[3]", {"0": "attack", "1": "defense", "2": "speed"}),
    g("0x4a0", "bonus_skills", "int[2][3]"),
    g("0x4c0", "foe_side_groups", "int[4]"),
]) + '''
[[table]]
name = "hero"
records = 4
built_by = "boot"
[table.fields]
level = "hero_levels"
class = "hero_classes"
weapon = "hero_weapon_ids"
weapon_level = "hero_weapon_levels"
trait = "hero_trait_ids"
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

# A block the game keeps one of (registry one-record tables): a scalar is its one cell, a 1-D array its one row.
[[table]]
name = "bonus"
records = 1
built_by = "bonus"
[table.fields]
title = "bonus_title"
level = "bonus_level"
stats = "bonus_stats"

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
fields = ["class", "weapon", "weapon_level"]
filters = ["class"]
pictures = { face = "art.face", other = "art.1" }

[[kind.group]]
name = "arms"
fields = ["weapon", "weapon_level"]

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
                                                               "fields": ["hero", "levels"], "text": [],
                                                               "literals": [], "notes": {}, "labels": {},
                                                               "parts": [], "where": {}, "joins": {}}},
          "a loader's published table names its registry table beside its short name")
    check(data.get("loaders", [{}])[0].get("seed") == [{"global": "current_hero_id", "index": [], "value": "key"}],
          "a loader's seed table is written as the list of cells it seeds")
    check(data.get("loaders", [{}])[0].get("steps") == 1 and data["loaders"][0].get("key_range") is None,
          "a loader without steps runs once for each key, every key")
    check(data.get("script_relations", [{}])[0].get("count") is None, "a relation without a count has count null")
    check(data.get("script_relations", [{}])[0].get("where") == {}, "a relation without a where has an empty one")
    check(data.get("script_relations", [{}, {}])[1].get("where") == {"joining_hero_count": 1},
          "a relation keeps its where")
    check(len(data.get("source_sha256", "")) == 64, "the JSON records the source's hash")
    check(data.get("export", {}).get("explicit_pictures") == ["HR*B", "HR001A"], "the JSON keeps the explicit pictures")
    check(hero.get("groups") == [{"name": "arms", "fields": ["weapon", "weapon_level"], "labels": None,
                                  "row_label": None}],
          "the JSON keeps a kind's groups")
    check(hero.get("joins") == [], "a kind that joins no table has an empty list")
    check(hero.get("notes") == {} and hero.get("text_blocks") == {}, "a kind without notes or text blocks has empty tables")
    check(weapon.get("groups") == [], "a kind without groups has an empty list")
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
    output = errors_of('[export]\nboot = { script = "INIT2", read_at = "demo.boot.data_ready" }\nbuilders = ["*INIT"]\n'
                       'text_scripts = ["HEROMES"]\nderived = ["SKILLINIT", "HEROMES", "count*"]\n')
    check("derived script SKILLINIT is a builder" in output, "a derived script may not be a builder")
    check("derived script HEROMES is also a text script" in output, "a derived script may not be a text script")
    check("derived: 'count*' must be a script name" in output, "a derived script is a plain name")
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

[[kind]]
id = "search"
table = "hero"
''')
    for needle in (
            "tooltips is a path of the site's database", "search is a path of the site's database",
            "picture is a table of the export's database",
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


def test_group_errors():
    """A group (export plan §5): two or more published rows of cells, each width a multiple of the narrowest's."""
    kind = GOOD.split("[[kind.group]]")[0].split("[[kind]]")[1]
    output = errors_of(EXPORT + "[[kind]]" + kind.replace('fields = ["class", "weapon", "weapon_level"]',
                                                         'fields = ["class", "weapon", "weapon_level", "art", "art.map"]') + '''
[[kind.group]]
name = "arms"
fields = ["weapon", "art"]
size = 2

[[kind.group]]
name = "class"
fields = ["weapon_level", "level"]

[[kind.group]]
name = "arms"
fields = ["class", "art.map"]

[[kind.group]]
name = "Lone"
fields = ["weapon"]
''')
    for needle in (
            "group 'arms': each field's width must be a multiple of the narrowest's (weapon 2, art 3)",
            "group 'arms': unknown key 'size'",
            "group 'class': class is also a field of table hero", "group 'class': 'level' is not one of the kind's fields",
            "group 'arms': arms is also a field of table hero, a description or another group",
            "group 'arms': class has one cell for each record", "group 'arms': art.map has one cell for each record",
            "group 'Lone': name must be a lowercase identifier",
            "group 'Lone': fields must list at least two of the kind's fields"):
        check(needle in output, f"the lint flags {needle!r}")
    output = errors_of(EXPORT + "[[kind]]" + kind + '''
[[kind.group]]
name = "first"
fields = ["weapon", "weapon_level"]

[[kind.group]]
name = "second"
fields = ["weapon_level", "weapon"]
''')
    check("group 'second': weapon_level is also in group 'first'" in output, "a field is in one group only")
    # Two traits for each weapon: a field twice as wide as the narrowest gives two columns.
    wide = kind.replace('fields = ["class", "weapon", "weapon_level"]', 'fields = ["class", "weapon", "weapon_level", "trait"]')
    code, output, written = run(EXPORT + "[[kind]]" + wide + '''
[[kind.group]]
name = "arms"
fields = ["weapon", "trait"]
''', write=True)
    check(code == 0 and written is not None
          and json.loads(written)["kinds"][0]["groups"] == [{"name": "arms", "fields": ["weapon", "trait"],
                                                             "labels": None, "row_label": None}],
          f"a field a multiple of the narrowest's width may join a group: {output}")


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
    # A list of references names an entity by the first that names a record (export plan §5).
    by_weapons = GOOD.replace('title = "name"\ntext = ["bio"]', 'title = ["weapon.0", "weapon.1"]\ntext = ["bio"]')
    code, output, written = run(by_weapons, write=True)
    check(code == 0, f"a list of references may be a title ({output.strip()})")
    kinds = json.loads(written or "{}").get("kinds", [])
    check(bool(kinds) and kinds[0].get("title") == ["weapon.0", "weapon.1"], "a title list is in the JSON as written")
    output = errors_of(EXPORT + '''
[[kind]]
id = "hero"
table = "hero"
exists = "name"
title = ["weapon.0"]
fields = ["weapon", "class", "art"]

[[kind]]
id = "hero_twice"
table = "hero"
exists = "name"
title = ["weapon.0", "weapon.0", "class", "art.face"]
fields = ["weapon", "class", "art"]
''')
    for needle in ("kind 'hero': a title list names two or more references",
                   "kind 'hero_twice': title weapon.0 is named twice",
                   "title class is no reference; each title of a list is a reference",
                   "title art.face is no reference; each title of a list is a reference"):
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


# The design pass's additions (export plan §5): notes, a picture cut from a sheet, a group labelled by a loaded table's
# text, a literal, and a loader with no scope that runs each key in steps with indexed seeds over a one-record table.
FEATURES = EXPORT + '''
[[kind]]
id = "hero"
table = "hero"
exists = "name"
title = "name"
text = ["bio"]
fields = ["class", "weapon", "weapon_level"]
qualifiers = ["class"]
marks = { class = "Classed" }
about = "The heroes who join the party."

[[kind.group]]
name = "arms"
fields = ["weapon", "weapon_level"]
labels = "foe.name"

[kind.pictures.badge]
field = "handler"
sheet = "BADGES.AGF"
first = 1
columns = 4
origin = [0, 100]
step = [16, 16]
size = [15, 15]

[kind.notes]
class = "1 knight, 2 archer."
arms = "Each weapon slot and its level."

[kind.labels]
weapon_level = "Weapon rank"
arms = "Arms"

[kind.text_blocks.bio]
summary = [0, 0]
detail = [1, 1]

[kind.referenced_as]
weapon = { heading = "Wielded by", details = ["weapon_level", "class"] }
"foe.hero" = { heading = "Fought on", details = ["class", "foe.name"], parts = { friend = "Fought beside" } }

[[kind]]
id = "weapon"
table = "weapon"
exists = "name"
title = "name"

[[loader]]
scope = "stage"
keys = "hero"
script = "FIELD"
read_at = "demo.stage.loaded"

[loader.tables.foe]
exists = "hero"
fields = ["hero"]
text = ["name"]
literals = [{ record = 0, field = "name", script = "AIM", offset = 0x10 }]
notes = { hero = "Who it is." }
labels = { hero = "Foe" }
parts = [{ name = "friend", field = "hero", global = "foe_side_groups", same_as = 1, note = "On the hero's side." }]

[[loader]]
keys = "hero"
key_range = [0, 3]
steps = 5
script = "FIELD"
read_at = "demo.stage.loaded"
seed = [
    { global = "current_hero_id", value = "key" },
    { global = "hero_rule_state", index = ["key", "*"], value = 1 },
    { global = "hero_rule_state", index = ["key", "step"], value = 2 },
]

[loader.tables.bonus]
table = "bonus"
exists = "level"
text = ["title"]
fields = ["level", "stats"]
'''


def test_column_references():
    """A row of cells whose columns are a table's records (`columns_ref`) may be headed as a reference is."""
    global REGISTRY
    saved = REGISTRY
    trait_global = 'name = "hero_trait_ids"\ncategory = "data-table"\ntype = "int[4][4]"\n'
    published = FEATURES.replace('fields = ["class", "weapon", "weapon_level"]\n',
                                 'fields = ["class", "weapon", "weapon_level", "trait"]\n', 1)
    headed = published.replace('[kind.referenced_as]\n', '[kind.referenced_as]\ntrait = "Trait of"\n')
    try:
        REGISTRY = REGISTRY.replace(trait_global, trait_global + 'columns_ref = "weapon"\n')
        code, output, written = run(headed, write=True)
        check(code == 0, f"a row of cells whose columns are records takes a heading ({output.strip()})")
        entries = json.loads(written or "{}").get("kinds", [{}])[0].get("referenced_as", [])
        check({"field": "trait", "heading": "Trait of", "details": [], "parts": {}} in entries,
              "the JSON keeps a column reference's heading")
    finally:
        REGISTRY = saved
    output = errors_of(headed)
    check("referenced_as trait: trait is not a reference to a table ([table.refs]) nor a row of cells whose columns "
          "are a table's records (columns_ref)" in output, "the lint flags a heading on a row of plain cells")


# A hero's bond (export plan §5): record r is hero r's (records_of), its partners two weapons, each a pair of
# elements (row x 1 + slot), and its ranks two numbers a record. The last global lies below the saved prefix.
BONDS = "".join([
    g("0x500", "bond_partners", "int[4][4]", {"0": "first_row", "1": "first_slot", "2": "second_row"}),
    g("0x520", "bond_ranks", "int[4][2]"),
    g("0x180", "bond_flags", "int[4]"),
]) + '''
[[table]]
name = "hero_bond"
records = 4
built_by = "boot"
records_of = "hero"
[table.fields]
partners = "bond_partners"
rank = "bond_ranks"
flags = "bond_flags"
[table.refs]
partners = { table = "weapon", times = 1, pairs = true }
'''


# A stage's camps, each led by a hero (export plan §5): a loaded table whose rows are the camps of kind 1 or 2, joined to
# the hero its chief names.
CAMPS = "".join([g("0x4d0", "camp_chief_ids", "int[5]"), g("0x4e0", "camp_kinds", "int[5]")]) + '''
[[table]]
name = "camp"
records = 5
built_by = "stage"
[table.fields]
chief = "camp_chief_ids"
kind = "camp_kinds"
[table.refs]
chief = "hero"
'''

CAMP_LOADER = '''
[[loader]]
keys = "hero"
script = "FIELD"
read_at = "demo.stage.loaded"

[loader.tables.camp]
table = "camp"
exists = "kind"
where = { kind = [1, 2] }
joins = { hero = "chief" }
fields = ["kind", "chief", "hero.class", "hero.weapon"]
labels = { "hero.class" = "Chief's class" }
'''


def test_keyed_picture():
    """A picture may name the colour the game draws transparent, { field, key }; the JSON keeps it."""
    keyed = GOOD.replace('other = "art.1" }', 'other = { field = "art.1", key = 0x00ff00 } }', 1)
    code, output, written = run(keyed, write=True)
    check(code == 0, f"a keyed picture lints clean ({output.strip()[-300:]})")
    hero = json.loads(written or "{}").get("kinds", [{}])[0]
    check(hero.get("pictures", {}).get("other") == {"field": "art.1", "key": 0x00ff00},
          "the JSON keeps a picture's key")
    code, output, _ = run(keyed.replace("key = 0x00ff00", "key = 0x1000000"))
    check(code != 0 and "picture other: key must be a colour 0xRRGGBB" in output, "a key is a colour 0xRRGGBB")
    code, output, _ = run(keyed.replace("key = 0x00ff00", "key = 0x00ff00, alpha = 1"))
    check(code != 0 and "picture other: unknown key 'alpha'" in output, "a keyed picture has a field and a key")


def test_loaded_where_and_joins():
    """A loaded table keeps the records whose field holds a value its where lists, publishes the fields of the record a
    row's reference reaches as table.field, and its kind may label it; a joined reference is keyed table.field.field."""
    global REGISTRY
    saved = REGISTRY
    camps = GOOD.replace('fields = ["class", "weapon", "weapon_level"]\n',
                         'fields = ["class", "weapon", "weapon_level"]\n'
                         'labels = { camp = "Camp" }\n'
                         'referenced_as = { "camp.hero.weapon" = "Wielded in camps" }\n', 1) + CAMP_LOADER
    try:
        REGISTRY = saved + CAMPS
        code, output, written = run(camps, write=True)
        check(code == 0, f"a loaded table's where, joins and label lint clean ({output.strip()[-400:]})")
        loaders = json.loads(written or "{}").get("loaders", [])
        camp = loaders[-1].get("tables", {}).get("camp", {}) if loaders else {}
        check(camp.get("where") == {"kind": [1, 2]} and camp.get("joins") == {"hero": "chief"},
              "the JSON keeps a loaded table's where and joins")

        def flags(text: str, needle: str, message: str) -> None:
            code, output, _ = run(text)
            check(code != 0 and needle in output, f"{message} ({output.strip()[-300:]})")

        flags(camps.replace('joins = { hero = "chief" }', 'joins = { hero = "kind" }'),
              "join 'hero': kind does not refer to table hero", "a join goes through a reference to its table")
        flags(camps.replace('joins = { hero = "chief" }', 'joins = { tavern = "chief" }'),
              "join 'tavern' is not a [[table]] of the registry", "a join names a registry table")
        flags(camps.replace('joins = { hero = "chief" }', 'joins = { kind = "chief" }'),
              "join 'kind' has the name of a field of table camp", "a join's name is no field of the table")
        flags(camps.replace('where = { kind = [1, 2] }', 'where = { kind = [], hero = [1] }'),
              "where kind must list the whole numbers", "a where lists values")
        flags(camps.replace('labels = { camp = "Camp" }', 'labels = { tent = "Tent" }'),
              "label 'tent' names nothing the page shows", "a kind's label names a loaded table it has")
    finally:
        REGISTRY = saved


def test_joins_and_pairs():
    """A kind publishes a joined table's fields as table.field, a paired reference by the first column of each pair,
    and a group's rows by a word and their number."""
    global REGISTRY
    saved = REGISTRY
    joined = GOOD.replace('fields = ["class", "weapon", "weapon_level"]\n',
                          'joins = ["hero_bond"]\n'
                          'fields = ["class", "weapon", "weapon_level", "hero_bond.partners.first_row", '
                          '"hero_bond.partners.second_row", "hero_bond.rank"]\n', 1)
    joined = joined.replace('[[kind.group]]\nname = "arms"\nfields = ["weapon", "weapon_level"]\n',
                            '[[kind.group]]\nname = "arms"\nfields = ["weapon", "weapon_level", "hero_bond.rank"]\n'
                            'row_label = "Slot"\n\n'
                            '[kind.labels]\n"hero_bond.rank" = "Rank"\n\n'
                            '[kind.referenced_as]\n"hero_bond.partners.first_row" = "First partner of"\n', 1)
    try:
        REGISTRY = saved + BONDS
        code, output, written = run(joined, write=True)
        check(code == 0, f"a joined table's fields and a paired reference's columns lint clean ({output.strip()})")
        hero = json.loads(written or "{}").get("kinds", [{}])[0]
        check(hero.get("joins") == ["hero_bond"], "the JSON keeps a kind's joins")
        check(hero.get("groups", [{}])[0].get("row_label") == "Slot", "the JSON keeps a group's row word")
        check({"field": "hero_bond.partners.first_row", "heading": "First partner of", "details": [], "parts": {}}
              in hero.get("referenced_as", []), "the JSON keeps a joined reference's heading")

        def flags(text: str, needle: str, message: str) -> None:
            code, output, _ = run(text)
            check(code != 0 and needle in output, f"{message} ({output.strip()[-300:]})")

        flags(joined.replace('"hero_bond.partners.second_row"', '"hero_bond.partners.first_slot"'),
              "fields hero_bond.partners.first_slot: partners is a paired reference",
              "the lint refuses a pair's second column")
        flags(joined.replace('"hero_bond.partners.second_row"', '"hero_bond.partners"'),
              "fields hero_bond.partners: partners is a paired reference", "the lint refuses a paired field whole")
        flags(joined.replace('"hero_bond.rank"]\n', '"hero_bond.rank", "hero_bond.flags"]\n', 1),
              "fields hero_bond.flags is saved state", "a joined field below the saved prefix is refused")
        flags(joined.replace('joins = ["hero_bond"]', 'joins = ["weapon"]'),
              "joins weapon, whose records are not hero's", "the lint refuses a join without records_of")
        flags(joined.replace('row_label = "Slot"', 'row_label = ""'),
              "group 'arms': row_label must be a line", "the lint refuses an empty row word")
        flags(joined.replace('title = "name"\ntext = ["bio"]', 'title = "hero_bond.rank"\ntext = ["bio"]', 1),
              "'hero_bond' is not a field of table hero", "only fields and groups name a joined table's fields")
    finally:
        REGISTRY = saved


def test_design_pass_features():
    code, output, written = run(FEATURES, write=True)
    check(code == 0, f"notes, a sheet picture, group labels, a literal and a stepped loader lint clean ({output.strip()})")
    data = json.loads(written or "{}")
    hero = data.get("kinds", [{}])[0]
    check(hero.get("groups", [{}])[0].get("labels") == "foe.name", "the JSON keeps a group's labels")
    check(hero.get("notes") == {"class": "1 knight, 2 archer.", "arms": "Each weapon slot and its level."},
          "the JSON keeps a kind's notes")
    check(hero.get("pictures", {}).get("badge", {}).get("sheet") == "BADGES.AGF", "the JSON keeps a sheet picture")
    check(hero.get("text_blocks") == {"bio": [{"name": "summary", "first": 0, "last": 0},
                                              {"name": "detail", "first": 1, "last": 1}]},
          "the JSON keeps a text field's blocks in their order")
    loaders = data.get("loaders", [{}, {}])
    foe = loaders[0].get("tables", {}).get("foe", {})
    check(foe.get("text") == ["name"] and foe.get("literals") == [{"record": 0, "field": "name", "script": "AIM",
                                                                  "offset": 0x10}]
          and foe.get("notes") == {"hero": "Who it is."}, "a loaded table keeps its text, literals and notes")
    check(hero.get("labels") == {"weapon_level": "Weapon rank", "arms": "Arms"} and foe.get("labels") == {"hero": "Foe"},
          "the JSON keeps a kind's and a loaded table's labels")
    check(foe.get("parts") == [{"name": "friend", "field": "hero", "global": "foe_side_groups", "same_as": 1,
                                "note": "On the hero's side."}], "a loaded table keeps its parts")
    check(hero.get("qualifiers") == ["class"], "the JSON keeps a kind's qualifiers")
    check(hero.get("marks") == [{"field": "class", "word": "Classed"}], "the JSON keeps a kind's marks")
    check(hero.get("about") == "The heroes who join the party.", "the JSON keeps a kind's about line")
    check(hero.get("referenced_as") == [
        {"field": "weapon", "heading": "Wielded by", "details": ["weapon_level", "class"], "parts": {}},
        {"field": "foe.hero", "heading": "Fought on", "details": ["class", "foe.name"],
         "parts": {"friend": "Fought beside"}}], "the JSON keeps each reference's heading, details and parts")
    stepped = loaders[1] if len(loaders) > 1 else {}
    check(stepped.get("scope") is None and stepped.get("steps") == 5 and stepped.get("key_range") == [0, 3],
          "a loader with no scope keeps its steps and key range")
    check(stepped.get("seed", [{}])[2:] == [{"global": "hero_rule_state", "index": ["key", "step"], "value": 2}],
          "an indexed seed keeps its terms, in order")
    check(stepped.get("tables", {}).get("bonus", {}).get("table") == "bonus",
          "a loader with no scope names each table's registry table")

    output = errors_of(FEATURES.replace('class = "1 knight, 2 archer."',
                                        'weapon = "a slot"\nnobody = "x"\nbio = "two\\nlines"')
                       .replace('labels = "foe.name"', 'labels = "foe.hero"')
                       .replace('sheet = "BADGES.AGF"', 'sheet = "badges"\nshade = 1')
                       .replace('summary = [0, 0]\ndetail = [1, 1]', 'Summary = [1, 1]\ndetail = [0, 2]')
                       .replace('size = [15, 15]', 'size = [17, 15]')
                       .replace('key_range = [0, 3]', 'key_range = [3, 0]')
                       .replace('steps = 5', 'steps = 0')
                       .replace('literals = [{ record = 0, field = "name", script = "AIM", offset = 0x10 }]',
                                'literals = [{ record = 7, field = "hero", script = "aim", offset = -1 }, '
                                '{ record = 7, field = "hero", script = "AIM", offset = 1 }]')
                       .replace('notes = { hero = "Who it is." }', 'notes = { speed = "fast" }')
                       .replace('weapon_level = "Weapon rank"', 'trait = "Trait"\nweapon = "two\\nlines"')
                       .replace('labels = { hero = "Foe" }', 'labels = { levels = "Levels" }')
                       .replace('{ global = "current_hero_id", value = "key" },',
                                '{ global = "current_hero_id", index = [0], value = "key" },')
                       .replace('[loader.tables.bonus]\ntable = "bonus"', '[loader.tables.bonus]\ntable = "foe"'))
    for needle in ("note weapon: the field is shown in group 'arms'; note the group",
                   "note 'nobody' names none of the kind's fields, text or groups",
                   "note bio must be one line of text",
                   "labels 'foe.hero': 'hero' is no text field of table foe",
                   "picture badge: unknown key 'shade'", "sheet 'badges' must be a picture's catalog name",
                   "size [17, 15] is larger than step [16, 16]",
                   "key_range must be [first, last]", "steps must be a whole number from 1 to 1000",
                   "literal: 'hero' is not one of the table's text fields", "literal: record 7 is not one of table foe",
                   "literal: script: 'aim' must be", "literal: offset must be the instruction's dword offset",
                   "literal: record 7's hero has two literals", "note 'speed' names none of the table's fields or text",
                   "label 'trait' names nothing the page shows", "label weapon must be a name of at most 40 characters",
                   "table 'foe': label 'levels' names nothing the page shows",
                   "seed current_hero_id is int, so its index must have 0 term(s)",
                   "text_blocks bio: block name 'Summary' must be a lowercase identifier",
                   "text_blocks bio: block detail must be [first, last], lines of the row's 0 to 1",
                   "table foe is a table of scope stage, which a loader with that scope reads"):
        check(needle in output, f"the lint flags {needle!r}")

    output = errors_of(FEATURES.replace('about = "The heroes who join the party."', 'about = "' + "x" * 161 + '"'))
    check("about is 161 characters; it is a line of at most 160" in output, "the lint flags a long about line")
    output = errors_of(FEATURES.replace('marks = { class = "Classed" }',
                                        'marks = { nobody = "x", weapon = "Armed", class = "' + "z" * 41 + '" }'))
    for needle in ("mark 'nobody' is not one of the kind's fields",
                   "mark weapon is a row of cells; name one column of it",
                   "mark weapon is a reference; a mark is a value the entity has",
                   "mark class must be a line of at most 40 characters"):
        check(needle in output, f"the lint flags {needle!r}")
    output = errors_of(FEATURES.replace('qualifiers = ["class"]', 'qualifiers = ["class", "nobody", "weapon", "class"]')
                       .replace('weapon = { heading = "Wielded by", details = ["weapon_level", "class"] }',
                                'weapon = { heading = "Wielded by", details = ["bio", "level"], extra = 1, '
                                'parts = { friend = "x" } }\n'
                                'class = "Classed"\nhandler = "' + "x" * 41 + '"')
                       .replace('"foe.hero" = { heading = "Fought on", details = ["class", "foe.name"], '
                                'parts = { friend = "Fought beside" } }',
                                '"foe.hero" = { heading = "Fought on", details = ["trap.damage", "weapon", '
                                '"foe.levels"], parts = { enemy = "Against" } }\n"foe.levels" = "Levels"\n'
                                '"loot.hero" = "Looted"'))
    for needle in ("qualifiers list 4 fields; a name takes at most 3",
                   "qualifier 'nobody' is not one of the kind's fields or text",
                   "qualifier weapon is a row of cells; name one column of it",
                   "qualifier class is listed twice",
                   "referenced_as weapon: unknown key 'extra'",
                   "referenced_as weapon: part 'friend': only a loaded table's rows are in parts",
                   "referenced_as weapon: detail 'bio' is a row of cells outside the reference's group",
                   "referenced_as weapon: detail 'level' is not one of the kind's fields or text",
                   "referenced_as class: class is not a reference to a table ([table.refs]) nor a row of cells",
                   "referenced_as handler: 'handler' is not a published field",
                   "referenced_as foe.hero: detail 'trap.damage' names another table",
                   "referenced_as foe.hero: detail 'weapon' is a row of cells outside the reference's group",
                   "referenced_as foe.hero: detail 'foe.levels' is not a published field or text of table foe",
                   "referenced_as foe.hero: part 'enemy' is not a part of table foe",
                   "referenced_as foe.levels: 'foe.levels' is not a published field of table foe",
                   "referenced_as loot.hero: 'loot' is no table the kind's loaders publish"):
        check(needle in output, f"the lint flags {needle!r}")
    output = errors_of(FEATURES.replace('"foe.hero" = { heading = "Fought on"', '"foe.hero" = { heading = "' + "y" * 41 + '"')
                       .replace('[[kind]]\nid = "weapon"\ntable = "weapon"\nexists = "name"\ntitle = "name"\n', ''))
    for needle in ("referenced_as foe.hero: heading must be a line of at most 40 characters",
                   "referenced_as weapon: weapon refers to table weapon, which no kind shows"):
        check(needle in output, f"the lint flags {needle!r}")

    output = errors_of(FEATURES.replace('text = ["bio"]\nfields = ["class"', 'text = ["bio", "name"]\nfields = ["class"')
                       .replace('[kind.text_blocks.bio]\nsummary = [0, 0]\ndetail = [1, 1]',
                                        '[kind.text_blocks.bio]\nsummary = [1, 1]\ndetail = [0, 1]\n'
                                        '[kind.text_blocks.name]\nall = [0, 0]\n'
                                        '[kind.text_blocks.class]\nall = [0, 0]'))
    for needle in ("text_blocks bio: block detail starts at line 0, within the block before it",
                   "text_blocks name: name is one line for each record; blocks divide a row of lines",
                   "text_blocks class: 'class' is not one of the kind's text fields"):
        check(needle in output, f"the lint flags {needle!r}")

    output = errors_of(FEATURES.replace(
        'parts = [{ name = "friend", field = "hero", global = "foe_side_groups", same_as = 1, note = "On the hero\'s side." }]',
        'parts = [{ name = "Friend", field = "levels", global = "foe_levels", same_as = 1, side = 1 }, '
        '{ name = "bonus", field = "hero", global = "foe_side_groups", same_as = 9 }, '
        '{ name = "bonus", field = "hero", global = "nowhere", same_as = 0 }]'))
    for needle in ("part 'Friend': a part's name must be a lowercase identifier", "part 'Friend': unknown key 'side'",
                   "part 'Friend': field levels is not one integer cell of a record",
                   "part 'Friend': global foe_levels is int[5][2], not a list of integers",
                   "part 'bonus': same_as 9 is not a cell of foe_side_groups",
                   "part 'bonus': the loader has two parts named bonus",
                   "part 'bonus': global 'nowhere' is not a global of the registry"):
        check(needle in output, f"the lint flags {needle!r}")

    output = errors_of(FEATURES.replace('fields = ["hero"]\ntext = ["name"]', 'fields = ["hero"]\ntext = ["name"]')
                       .replace('name = "friend", field', 'name = "foe", field'))
    check("table 'foe''s part 'foe' has the name of one of the loader's tables" in output,
          "the lint flags a part named as one of its loader's tables")

    output = errors_of(FEATURES.replace('labels = "foe.name"', 'labels = "boss.name"')
                       .replace('key_range = [0, 3]\n', '').replace('steps = 5\n', '')
                       .replace('[loader.tables.bonus]\ntable = "bonus"', '[loader.tables.bonus]')
                       .replace('[loader.tables.foe]\n', '[loader.tables.foe]\ntable = "foe"\n'))
    for needle in ("labels 'boss.name': 'boss' is no table the kind's loaders publish",
                   "seed hero_rule_state is indexed by the key, which the loader's key_range must keep below 4",
                   "seed hero_rule_state is indexed by the step, which the loader's steps must keep below 5",
                   "a seed is indexed by the step, and the loader has no steps",
                   "a loader with no scope names each table's registry table: table None",
                   "a loader with a scope names its tables by the scope's short names, not by table"):
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
    test_group_errors()
    test_titles()
    test_loader_and_relation_errors()
    test_design_pass_features()
    test_column_references()
    test_joins_and_pairs()
    test_loaded_where_and_joins()
    test_keyed_picture()
    test_the_repository_declarations()
    print("FAILURES:", len(FAILS))
    return 1 if FAILS else 0


if __name__ == "__main__":
    sys.exit(main())
