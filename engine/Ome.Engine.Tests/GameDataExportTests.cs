using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Data.Sqlite;
using Ome.Engine.Hosting;
using Ome.Engine.Localization;
using Ome.Engine.Model;
using Ome.Engine.Natives;
using Ome.Engine.Sys4;
using Ome.Engine.Vm;
using Ome.Export;
using Ome.Mods.Localization;
using Xunit;

namespace Ome.Engine.Tests;

using Script = Ome.Engine.Model.Script;

/// <summary>The game data exporter (game data export spec §5.5, §5.7, §6, §7) on a synthetic game: a boot whose
/// builders fill an item and a skill table, an append layer, a description script, a loader that builds each item's
/// loot table, and one variant per refusal.</summary>
public class GameDataExportTests
{
    private static readonly OpcodeTable Table = OpcodeTableJson.Load(Paths.OpcodesJson);
    private const int OpExit = 0x2, OpCallScript = 0x3, OpMov = 0x55, OpSetString = 0x192, OpShowText = 0x6e,
                      OpFurigana = 0x196, OpEndLine = 0x6f, OpSub = 0x51, OpJmp = 0x8c, OpCall = 0x8f, OpRet = 0x5;
    private static readonly int OpLookupArray = Table.ByLabel("lookup-array")!.Value;
    private static readonly int OpSubstring = Table.ByLabel("cp932-substring")!.Value;

    private static Operand I(long value) => new(0, value);
    private static Operand S(int index) => new(2, index);
    private static Operand G(int cell) => new(3, cell);
    private static Operand GS(int cell) => new(5, cell);
    private static Operand Local(int slot) => new(9, slot);

    // The synthetic registry: item (4 records) and skill (3 records). The skill's names and categories share their
    // addresses, in two banks.
    private const int ItemNames = 100, ItemShorts = 200, SkillNames = 1500, Scratch = 900;
    private const int ItemCategory = 1000, ItemPrices = 1100, ItemIcon = 1200, ItemHandler = 1300, ItemSkill = 1400,
                      SkillCategory = 1500, CurrentItem = 5000, LootSkill = 1600, LootCount = 1700, CurrentCount = 5001,
                      JoinSkills = 5100, JoinCount = 5110, LootSides = 1800;
    // The one-record bonus table (export plan §5) and the rule state its loader seeds: four keys of three rules.
    private const int BonusTitle = 960, BonusLevel = 5200, BonusStats = 5210, RuleState = 5300;
    // The game's names of the item categories (export plan §5), which ITINIT writes: element k names category k + 1.
    private const int CategoryNames = 980;
    // An item's bond (export plan §5): a table whose record r is item r's (records_of), three records, so item 3 has
    // none. Its partners are two skills, each a pair of elements (row × 1 + slot); its tiers two numbers a record.
    private const int BondPartners = 5500, BondTiers = 5520;

    private static RegistryView Registry(bool formats = false, RefKey? skillKey = null, bool bitNames = false,
                                         string? bondsOf = null) => new(
        [
            new TableLayout("item_bond", 3, [("partners", "bond_partners"), ("tier", "bond_tiers")],
                            new Dictionary<string, string> { ["partners"] = "skill" })
            {
                RefKeys = new Dictionary<string, RefKey> { ["partners"] = new RefKey(1, null) }, RecordsOf = bondsOf,
            },
            new TableLayout("item", 4,
                [("name", "item_names"), ("short_description", "item_short_descriptions"), ("category", "item_category"),
                 ("price", "item_prices"), ("icon", "item_icon"), ("handler_script_id", "item_handler"),
                 ("granted_skill", "item_granted_skill")],
                new Dictionary<string, string> { ["granted_skill"] = "skill", ["icon"] = "picture" })
            {
                RefKeys = skillKey is null ? new Dictionary<string, RefKey>() : new Dictionary<string, RefKey> { ["granted_skill"] = skillKey },
            },
            new TableLayout("skill", 3, [("name", "skill_names"), ("category", "skill_category")],
                            new Dictionary<string, string>()),
            // The loot table of the scope `field` (export plan §4.2): a loader builds it for one item at a time.
            new TableLayout("field_loot", 3, [("skill_id", "loot_skill_ids"), ("count", "loot_counts")],
                            new Dictionary<string, string> { ["skill_id"] = "skill" }),
            // A block the game keeps one of (export plan §5): a scalar string, a scalar and a list as its one record.
            new TableLayout("bonus", 1, [("title", "bonus_title"), ("level", "bonus_level"), ("stats", "bonus_stats")],
                            new Dictionary<string, string>()),
        ],
        [
            Array("item_names", GlobalBank.String, ItemNames, 4),
            Array("item_short_descriptions", GlobalBank.String, ItemShorts, 4),
            Array("item_category", GlobalBank.Integer, ItemCategory, 4) with
            {
                Format = formats
                    ? new ValueFormat { NamesFrom = ("category_names", 1), Words = new Dictionary<long, string> { [0] = "None" } }
                    : null,
            },
            Array("category_names", GlobalBank.String, CategoryNames, 3),
            new ArrayLayout("item_prices", GlobalBank.Integer, ItemPrices, [4, 2],
                            new Dictionary<int, string> { [1] = "sell" })    // column 0 unnamed
            {
                Format = formats ? new ValueFormat { Scale = 0.5, Unit = "gold", None = 0 } : null,
                // The column facets: the game's names for its columns, and the table whose records they are.
                ColumnNamesFrom = formats ? ("category_names", 0) : null,
                ColumnsOf = formats ? "skill" : null,
            },
            Array("item_icon", GlobalBank.Integer, ItemIcon, 4),
            Array("item_handler", GlobalBank.Integer, ItemHandler, 4) with
            {
                Format = bitNames ? new ValueFormat { BitsOf = "category_names" } : null,
            },
            Array("item_granted_skill", GlobalBank.Integer, ItemSkill, 4),
            Array("skill_names", GlobalBank.String, SkillNames, 3),
            Array("skill_category", GlobalBank.Integer, SkillCategory, 3),
            new ArrayLayout("current_item_id", GlobalBank.Integer, CurrentItem, [], new Dictionary<int, string>()),
            // The script relations' arguments (export plan §4.5): ADDITEM's count, and JOIN's list and its length.
            new ArrayLayout("current_item_count_change", GlobalBank.Integer, CurrentCount, [], new Dictionary<int, string>()),
            Array("joining_skill_ids", GlobalBank.Integer, JoinSkills, 2),
            new ArrayLayout("joining_skill_count", GlobalBank.Integer, JoinCount, [], new Dictionary<int, string>()),
            Array("loot_skill_ids", GlobalBank.Integer, LootSkill, 3),
            new ArrayLayout("loot_counts", GlobalBank.Integer, LootCount, [3, 2], new Dictionary<int, string>()),
            // The side of each skill a loot record grants, which a part reads (export plan §5): no table's.
            Array("loot_sides", GlobalBank.Integer, LootSides, 3),
            new ArrayLayout("bonus_title", GlobalBank.String, BonusTitle, [], new Dictionary<int, string>()),
            new ArrayLayout("bonus_level", GlobalBank.Integer, BonusLevel, [], new Dictionary<int, string>()),
            new ArrayLayout("bonus_stats", GlobalBank.Integer, BonusStats, [3], new Dictionary<int, string> { [0] = "attack" }),
            new ArrayLayout("rule_state", GlobalBank.Integer, RuleState, [4, 3], new Dictionary<int, string>()),
            new ArrayLayout("bond_partners", GlobalBank.Integer, BondPartners, [3, 4],
                            new Dictionary<int, string> { [0] = "first_row", [1] = "first_slot", [2] = "second_row" }),
            new ArrayLayout("bond_tiers", GlobalBank.Integer, BondTiers, [3, 2], new Dictionary<int, string>()),
        ]);

    private static ArrayLayout Array(string name, GlobalBank bank, int address, int records)
        => new(name, bank, address, [records], new Dictionary<int, string>());

    private const string Declarations = """
        {"profile": "testgame", "source_sha256": "0123abcd",
         "export": {"boot": {"script": "BOOT", "read_at": "testgame.boot.ready"},
                    "builders": ["BOOT", "*INIT"], "excluded_builders": ["CGINIT"], "derived": ["COUNTUP"],
                    "text_scripts": ["ITMES"],
                    "explicit_pictures": ["ICON07*", "NOPE*"]},
         "kinds": [
          {"id": "item", "aliases": [], "table": "item", "exists": "name", "skip": {}, "title": "name",
           "text": ["short_description"], "fields": ["category", "price", "price.sell", "granted_skill"],
           "filters": ["category"], "pictures": {"icon": "icon"},
           "descriptions": [{"name": "description", "script": {"field": "handler_script_id"},
                             "seed": {"current_item_id": "key"}}]},
          {"id": "skill", "aliases": ["ability"], "table": "skill", "exists": "name", "skip": {}, "title": "name",
           "text": [], "fields": ["category"], "filters": [], "pictures": {}, "descriptions": []}],
         "loaders": [], "script_relations": []}
        """;

    // A loader for each item but item 3 (category 4): LOOT, no builder, calls DROPSET, the loader's own builder, and the
    // loot table is read at the point after it.
    private const string Loader = """
        "loaders": [{"scope": "field", "keys": "item", "skip": {"category": 4}, "script": "LOOT",
                     "seed": {"current_item_id": "key"}, "read_at": "testgame.loot.ready", "builders": ["DROPSET"],
                     "tables": {"loot": {"table": "field_loot", "exists": "skill_id", "fields": ["skill_id", "count"]}}}]
        """;

    private static string WithLoader(string loader = Loader)
        => Declarations.Replace("\"loaders\": []", loader.Trim(), StringComparison.Ordinal);

    [Flags]
    private enum Faults
    {
        None = 0,
        SceneWritesTheTable = 1,     // a scene writes item 2's category, and a builder copies the scene's line into it
        ExcludedBuilderWrites = 2,   // CGINIT writes item 3's icon
        StringWithoutOrigin = 4,     // a builder cuts item 1's name into item 3's short description (no origin)
        HandlerIsNoTextScript = 8,   // item 2's handler is OTHER, a scene
        LoaderScriptWrites = 16,     // LOOT itself writes loot record 2's second count, and skill 2's side
        DerivedScriptWrites = 32,    // COUNTUP, a derived script, writes item 1's category
    }

    private sealed class Pictures : IPictureSource
    {
        public ExportPicture? Resolve(long packedId) => packedId is 77 or 79
            ? new ExportPicture($"ICON{packedId:000}", Encoding.ASCII.GetBytes($"png {packedId}"), 32, 16)
            : null;

        // A keyed picture (export plan §5) is named by its key too, as the catalog source names it.
        public ExportPicture? ResolveKeyed(long packedId, int key) => Resolve(packedId) is { } picture
            ? picture with { Name = $"{picture.Name}-key-{key:x6}" }
            : null;

        // A sheet's cells (export plan §5): SHEET.AGF holds one row of cells, so a cell below it is none; a keyed cell
        // is named by its key too.
        public ExportPicture? Cell(string sheet, int x, int y, int width, int height, int? key)
            => sheet == "SHEET.AGF" && y < 18
                ? new ExportPicture($"SHEET-{x}-{y}" + (key is { } k ? $"-key-{k:x6}" : ""),
                                    Encoding.ASCII.GetBytes($"png {x} {y}"), width, height)
                : null;
    }

    private static Script Asm(string name, IReadOnlyList<(int, Operand[])> code, params string[] strings)
        => ScriptAssembler.Assemble(Table, name, code, strings);

    private static ExportInputs Game(Faults faults = Faults.None, bool bonds = false)
    {
        var byId = new Dictionary<long, Script>
        {
            [23] = Asm("BDINIT.BIN",
            [
                (OpMov, [G(BondPartners + 4), I(1)]), (OpMov, [G(BondPartners + 6), I(1)]), (OpMov, [G(BondPartners + 7), I(1)]),
                (OpMov, [G(BondTiers + 2), I(3)]), (OpMov, [G(BondTiers + 3), I(4)]), (OpMov, [G(BondTiers + 4), I(5)]),
                (OpExit, []),
            ]),
            [1] = Asm("ITINIT.BIN",
            [
                (OpSetString, [GS(ItemNames + 1), S(0)]), (OpSetString, [GS(ItemShorts + 1), S(1)]),
                (OpMov, [G(ItemCategory + 1), I(2)]), (OpMov, [G(ItemPrices + 2), I(10)]), (OpMov, [G(ItemPrices + 3), I(5)]),
                (OpMov, [G(ItemIcon + 1), I(77)]), (OpMov, [G(ItemHandler + 1), I(9)]), (OpMov, [G(ItemSkill + 1), I(1)]),
                (OpSetString, [GS(ItemNames + 2), S(2)]), (OpMov, [G(ItemCategory + 2), I(3)]),
                (OpMov, [G(ItemSkill + 2), I(2)]),
                (OpMov, [G(ItemHandler + 2), I(faults.HasFlag(Faults.HandlerIsNoTextScript) ? 10 : 0)]),
                (OpSetString, [GS(CategoryNames + 1), S(3)]), (OpSetString, [GS(CategoryNames + 2), S(4)]),
                (OpExit, []),
            ], "剣", "よく切れる", "盾", "武器", "防具"),
            [2] = Asm("$1$ITINIT.BIN",
                [(OpSetString, [GS(ItemNames + 3), S(0)]), (OpMov, [G(ItemCategory + 3), I(4)]),
                 (OpMov, [G(ItemIcon + 3), I(78)]), (OpMov, [G(ItemSkill + 3), I(-1)]), (OpExit, [])], "槍"),
            [3] = Asm("SKINIT.BIN", [(OpSetString, [GS(SkillNames + 1), S(0)]), (OpMov, [G(SkillCategory + 1), I(1)]),
                                     (OpExit, [])], "斬撃"),
            [4] = Asm("SCENE.BIN", [(OpSetString, [GS(Scratch), S(0)]), (OpMov, [G(ItemCategory + 2), I(99)]), (OpExit, [])],
                      "台詞"),
            [5] = Asm("$2$ITINIT.BIN", [(OpMov, [GS(ItemShorts + 2), GS(Scratch)]), (OpExit, [])]),
            [6] = Asm("CGINIT.BIN", [(OpMov, [G(ItemIcon + 3), I(79)]), (OpExit, [])]),
            [7] = Asm("$3$ITINIT.BIN", [(OpSubstring, [GS(ItemShorts + 3), GS(ItemNames + 1), I(0), I(1)]), (OpExit, [])]),
            [9] = Asm("ITMES.BIN",
            [
                (OpLookupArray, [GS(Scratch + 50), GS(ItemNames), G(CurrentItem)]),
                (OpShowText, [I(0), S(0)]), (OpShowText, [I(0), GS(Scratch + 50)]), (OpShowText, [I(0), S(1)]),
                (OpEndLine, [I(0)]), (OpFurigana, [I(0), S(2), S(3)]), (OpShowText, [I(0), S(4)]), (OpEndLine, [I(0)]),
                (OpCallScript, [I(10)]), (OpExit, []),
            ], "【", "】", "鋭", "するど", "い"),
            [10] = Asm("OTHER.BIN", [(OpShowText, [I(0), S(0)]), (OpExit, [])], "他の台詞"),
            // A derived script: item 1's category, and a count of its own that no global names.
            [22] = Asm("COUNTUP.BIN", [(OpMov, [G(ItemCategory + 1), I(7)]), (OpMov, [G(5400), I(1)]), (OpExit, [])]),
            // The bonus loader's script (export plan §5): the level is the key, the stats key 1's row of the rule state,
            // and the title a string of its own.
            [20] = Asm("RULES.BIN",
            [
                (OpMov, [G(BonusLevel), G(CurrentItem)]), (OpMov, [G(BonusStats), G(RuleState + 3)]),
                (OpMov, [G(BonusStats + 1), G(RuleState + 4)]), (OpMov, [G(BonusStats + 2), G(RuleState + 5)]),
                (OpSetString, [GS(BonusTitle), S(0)]), (OpExit, []),
            ], "称号"),
            // A screen that draws a fixed string, which a literal gives a record's text from.
            [21] = Asm("AIM.BIN", [(OpSetString, [GS(Scratch + 60), S(0)]), (OpExit, [])], "固定"),
            // Loot record 1 grants the skill numbered as the item; record 2 skill 1, five of it. Skill 1 is on side 5 and
            // skill 2 on side 6.
            [11] = Asm("DROPSET.BIN",
            [
                (OpMov, [G(LootSkill + 1), G(CurrentItem)]), (OpMov, [G(LootCount + 2), I(3)]),
                (OpMov, [G(LootCount + 3), I(4)]), (OpMov, [G(LootSkill + 2), I(1)]), (OpMov, [G(LootCount + 4), I(5)]),
                (OpMov, [G(LootSides + 1), I(5)]), (OpMov, [G(LootSides + 2), I(6)]),
                (OpExit, []),
            ]),
        };
        AddRelationScripts(byId);
        var loot = new List<(int, Operand[])> { (OpCallScript, [I(11)]) };
        if (faults.HasFlag(Faults.LoaderScriptWrites))
            loot.AddRange([(OpMov, [G(LootCount + 5), I(6)]), (OpMov, [G(LootSides + 2), I(5)])]);
        // The point: what LOOT writes after it is never read.
        loot.AddRange([(OpMov, [G(LootSkill + 1), I(99)]), (OpExit, [])]);
        byId[12] = Asm("LOOT.BIN", loot);
        var calls = new List<(int, Operand[])> { (OpCallScript, [I(1)]), (OpCallScript, [I(2)]), (OpCallScript, [I(3)]) };
        if (faults.HasFlag(Faults.SceneWritesTheTable)) calls.AddRange([(OpCallScript, [I(4)]), (OpCallScript, [I(5)])]);
        if (faults.HasFlag(Faults.ExcludedBuilderWrites)) calls.Add((OpCallScript, [I(6)]));
        if (faults.HasFlag(Faults.StringWithoutOrigin)) calls.Add((OpCallScript, [I(7)]));
        if (faults.HasFlag(Faults.DerivedScriptWrites)) calls.Add((OpCallScript, [I(22)]));
        if (bonds) calls.Add((OpCallScript, [I(23)]));
        calls.Add((OpExit, []));
        Script boot = Asm("BOOT.BIN", calls);
        var byName = byId.Values.Append(boot).ToDictionary(script => ScriptNames.Normalized(script.Name), StringComparer.Ordinal);
        return new ExportInputs
        {
            GameId = "testgame",
            Table = Table,
            Scripts = new ScriptMap(byId),
            ScriptByName = name => byName.GetValueOrDefault(ScriptNames.Normalized(name)),
            BootStop = new RunStop("BOOT", boot.Instructions[^1].Offset),
            StopOf = point => point == "testgame.loot.ready"
                ? new RunStop("LOOT", byId[12].Instructions[^2].Offset)
                : throw new ExportFailure($"{point} has no site"),
            Declarations = ExportDeclarations.Parse(faults.HasFlag(Faults.LoaderScriptWrites) ? WithLoader() : Declarations),
            Registry = Registry(),
            Pictures = new Pictures(),
            BuildInfo = [("revision", "abcdef012345"), ("title", "テスト")],
        };
    }

    // The script relations' scenes (spec §6.7, §12; plan §4.5), which no run reaches: ADDITEM and JOIN, a scene that
    // gives two items and one that takes one, one with each call the scan leaves unresolved, an excluded debug scene,
    // and one that joins a skill alone and then two.
    private const int AddItem = 13, Join = 18;

    private static void AddRelationScripts(Dictionary<long, Script> byId)
    {
        byId[AddItem] = Asm("ADDITEM.BIN", [(OpExit, [])]);
        byId[14] = Asm("GIFT.BIN",
        [
            (OpMov, [G(CurrentItem), I(1)]), (OpMov, [G(CurrentCount), I(3)]), (OpCallScript, [I(AddItem)]),
            (OpMov, [G(CurrentItem), I(2)]), (OpMov, [G(CurrentCount), I(1)]), (OpCallScript, [I(AddItem)]),
            (OpExit, []),
        ]);
        byId[15] = Asm("$1$TAKE.BIN",
        [
            (OpMov, [G(CurrentItem), I(2)]), (OpSub, [G(CurrentCount), I(0), I(5)]), (OpCallScript, [I(AddItem)]),
            (OpCallScript, [I(AddItem)]),                                     // the first call may have written both
            (OpExit, []),
        ]);
        var odd = new List<(int Op, Operand[] Args)>
        {
            (OpMov, [G(CurrentItem), G(Scratch)]), (OpMov, [G(CurrentCount), I(1)]), (OpCallScript, [I(AddItem)]),
            (OpMov, [G(CurrentItem), I(1)]), (OpMov, [G(CurrentCount), I(0)]), (OpCallScript, [I(AddItem)]),
            (OpMov, [G(CurrentItem), I(9)]), (OpMov, [G(CurrentCount), I(1)]), (OpCallScript, [I(AddItem)]),
            (OpMov, [G(CurrentItem), I(1)]), (OpMov, [G(CurrentCount), I(1)]), (OpCall, [I(0)]), (OpCallScript, [I(AddItem)]),
            (OpMov, [G(CurrentItem), I(1)]), (OpMov, [G(CurrentCount), I(1)]), (OpJmp, [I(0)]), (OpCallScript, [I(AddItem)]),
            (OpMov, [G(CurrentItem), I(1)]), (OpMov, [G(CurrentCount), I(1)]),
            (OpLookupArray, [Local(0), G(CurrentItem), I(0)]), (OpCallScript, [I(AddItem)]),
            (OpExit, []), (OpRet, []),
        };
        int[] at = Offsets(odd);
        odd[11] = (OpCall, [I(at[^1])]);     // the subroutine: the ret
        odd[15] = (OpJmp, [I(at[16])]);      // the next instruction, which the jump makes a block's start
        byId[16] = Asm("ODD.BIN", odd);
        byId[17] = Asm("DEBUGGIVE.BIN",
            [(OpMov, [G(CurrentItem), I(3)]), (OpMov, [G(CurrentCount), I(1)]), (OpCallScript, [I(AddItem)]), (OpExit, [])]);
        byId[Join] = Asm("JOIN.BIN", [(OpExit, [])]);
        byId[19] = Asm("RECRUIT.BIN",
        [
            (OpMov, [G(JoinSkills), I(1)]), (OpMov, [G(JoinCount), I(1)]), (OpCallScript, [I(Join)]),
            (OpMov, [G(JoinSkills), I(2)]), (OpMov, [G(JoinSkills + 1), I(1)]), (OpMov, [G(JoinCount), I(2)]),
            (OpCallScript, [I(Join)]),
            (OpExit, []),
        ]);
    }

    /// <summary>Each instruction's offset: one dword for the opcode and two for each operand.</summary>
    private static int[] Offsets(IReadOnlyList<(int Op, Operand[] Args)> code)
    {
        var offsets = new int[code.Count];
        for (int k = 1; k < code.Count; k++) offsets[k] = offsets[k - 1] + 1 + 2 * code[k - 1].Args.Length;
        return offsets;
    }

    private static ExportEntity Entity(ExportResult result, string kind, int id)
        => result.Kinds.Single(k => k.Declaration.Id == kind).Entities.Single(entity => entity.Id == id);

    private static long?[] Values(ExportEntity entity, string field) => entity.Values.Single(value => value.Field == field).Values;

    private static string? Text(ExportEntity entity, string field, int index = 0, string language = "ja")
        => entity.Texts.SingleOrDefault(text => text.Field == field && text.Index == index && text.Language == language)
            ?.Value;

    // ------------------------------------------------------------------------------------------------- a clean export

    [Fact]
    public void ACleanExportReadsEachKindsEntities()
    {
        ExportResult result = GameDataExporter.Run(Game());

        Assert.Empty(result.Report.Refusals);
        Assert.Equal(0, result.Report.ExitCode);
        Assert.Equal("stop:BOOT@0x" + Game().BootStop.Offset.ToString("x"), result.Report.BootHalt);
        Assert.Equal([1, 2, 3], result.Kinds[0].Entities.Select(entity => entity.Id));
        Assert.Equal([1], result.Kinds[1].Entities.Select(entity => entity.Id));
        ExportEntity slash = Entity(result, "skill", 1);
        Assert.Equal(("斬撃", 1L), (Text(slash, "name"), Values(slash, "category")[0]));

        ExportEntity sword = Entity(result, "item", 1);
        Assert.Equal([2L], Values(sword, "category"));
        Assert.Equal([10L, 5L], Values(sword, "price"));
        Assert.Equal([5L], Values(sword, "price.sell"));
        Assert.Equal("剣", Text(sword, "name"));
        Assert.Equal("よく切れる", Text(sword, "short_description"));
        // The append layer's record is an entity like any other, its writer the layer.
        ExportEntity spear = Entity(result, "item", 3);
        Assert.Equal("槍", Text(spear, "name"));
        Assert.Equal("$1$ITINIT.BIN", spear.Cells.Single(cell => cell.Field == "category").Writer);
    }

    [Fact]
    public void ADescriptionKeepsWhatItsScriptShowsLineByLine()
    {
        ExportResult result = GameDataExporter.Run(Game());

        // ITMES shows the seeded item's name; OTHER's line, shown in the same run, is dropped and counted.
        Assert.Equal("【剣】\n鋭い", Text(Entity(result, "item", 1), "description"));
        Assert.Null(Text(Entity(result, "item", 2), "description"));   // no handler
        Assert.Equal(1, result.Report.DescriptionRuns);
        Assert.Equal(1, result.Report.DroppedPieces);
        Assert.Empty(result.Report.DescriptionHalts);
    }

    [Fact]
    public void ReferencesNameTheirKindAndAMissingTargetIsCounted()
    {
        ExportResult result = GameDataExporter.Run(Game());

        Assert.Equal([new ExportRef("granted_skill", 0, "skill", 1)], Entity(result, "item", 1).Refs);
        Assert.Equal([new ExportRef("granted_skill", 0, "skill", 2)], Entity(result, "item", 2).Refs);
        Assert.Equal(1, result.Report.MissingRefs[("item", "granted_skill", "skill")]);
        // A negative value is none, as 0 is: no record has a negative id (the games write -1 for none too).
        Assert.Equal([-1L], Values(Entity(result, "item", 3), "granted_skill"));
        Assert.Empty(Entity(result, "item", 3).Refs);
    }

    [Fact]
    public void ATitleMayBeAReferenceOrNone()
    {
        // The item is named by its granted skill, as a recipe is by the item it makes; the skill has no title.
        JsonNode declarations = JsonNode.Parse(Declarations)!;
        declarations["kinds"]![0]!["title"] = "granted_skill";
        declarations["kinds"]![1]!["title"] = null;
        ExportInputs game = Game();
        ExportResult result = GameDataExporter.Run(new ExportInputs
        {
            GameId = game.GameId, Table = game.Table, Build = game.Build, Scripts = game.Scripts,
            ScriptByName = game.ScriptByName, BootStop = game.BootStop, Registry = game.Registry,
            Declarations = ExportDeclarations.Parse(declarations.ToJsonString()), Pictures = game.Pictures,
            BuildInfo = game.BuildInfo,
        });

        ExportEntity sword = Entity(result, "item", 1);
        Assert.Empty(result.Report.Refusals);
        Assert.Null(Text(sword, "name"));                           // no longer published as text
        Assert.DoesNotContain(sword.Texts, text => text.Field == "granted_skill");
        Assert.Equal([new ExportRef("granted_skill", 0, "skill", 1)], sword.Refs);
        string root = Path.Combine(Path.GetTempPath(), "ome-export-" + Guid.NewGuid().ToString("N"));
        try
        {
            ExportWriter.Write(result, root, "abcdef012345", Manifest);
            using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
            {
                DataSource = Path.Combine(root, "abcdef012345.sqlite"), Mode = SqliteOpenMode.ReadOnly, Pooling = false,
            }.ToString());
            connection.Open();
            string? Scalar(string sql)
            {
                using SqliteCommand command = connection.CreateCommand();
                command.CommandText = sql;
                return command.ExecuteScalar()?.ToString();
            }
            Assert.Equal("granted_skill", Scalar("SELECT title_field FROM kind WHERE id = 'item'"));
            Assert.Equal("[]", Scalar("SELECT title_fallbacks FROM kind WHERE id = 'item'"));
            Assert.Equal("", Scalar("SELECT ifnull(title_field, '') FROM kind WHERE id = 'skill'"));
            // The reference is a published value, not a text field.
            Assert.Equal("1|0", Scalar("SELECT published || '|' || text FROM field WHERE kind = 'item' AND name = 'granted_skill'"));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ATitleMayBeSeveralReferencesInTurn()
    {
        // A recipe makes an item or a card: the first reference that names a record names it (export plan §5).
        JsonNode declarations = JsonNode.Parse(Declarations)!;
        declarations["kinds"]![0]!["title"] = new JsonArray("granted_skill", "made_card", "made_unit");
        KindDeclaration item = ExportDeclarations.Parse(declarations.ToJsonString()).Kinds[0];

        Assert.Equal("granted_skill", item.Title);
        Assert.Equal(["made_card", "made_unit"], item.TitleFallbacks);
        Assert.Empty(ExportDeclarations.Parse(Declarations).Kinds[0].TitleFallbacks);
    }

    [Fact]
    public void PicturesResolveAndAnUnknownIdIsReported()
    {
        ExportResult result = GameDataExporter.Run(Game());

        var (name, picture) = Assert.Single(Entity(result, "item", 1).Pictures);
        Assert.Equal(("icon", "ICON077"), (name, picture.Name));
        // explicit_pictures marks it, and an entry that matches no picture is reported.
        Assert.True(picture.Explicit);
        Assert.Equal(["NOPE*"], result.Report.UnmatchedExplicitPictures);
        Assert.Empty(Entity(result, "item", 3).Pictures);
        Assert.Equal([("item", 3, "icon", 78L)], result.Report.UnresolvedPictures);
    }

    [Fact]
    public void TheModderCellsShowEveryWrittenCellAndItsWriter()
    {
        ExportResult result = GameDataExporter.Run(Game());
        ExportEntity sword = Entity(result, "item", 1);

        Assert.Contains(new ExportModderCell("handler_script_id", 0, GlobalBank.Integer, ItemHandler + 1, "9", "ITINIT.BIN"),
                        sword.Cells);
        Assert.Contains(new ExportModderCell("price", 1, GlobalBank.Integer, ItemPrices + 3, "5", "ITINIT.BIN"), sword.Cells);
        // A cell no script wrote holds its default and is not listed; one written with 0 is.
        Assert.DoesNotContain(Entity(result, "item", 2).Cells, cell => cell.Field == "icon");
        Assert.Contains(Entity(result, "item", 2).Cells, cell => cell is { Field: "handler_script_id", Raw: "0" });
    }

    // ------------------------------------------------------------------------------------------------- refusals (§5.5)

    [Fact]
    public void ASceneStringCopiedIntoATableAndASceneWriteAreRefused()
    {
        ExportResult result = GameDataExporter.Run(Game(Faults.SceneWritesTheTable));

        ExportEntity shield = Entity(result, "item", 2);
        Assert.Equal([null], Values(shield, "category"));
        Assert.Null(Text(shield, "short_description"));
        Assert.Contains(result.Report.Refusals, refusal => refusal is { Id: 2, Field: "category" }
                                                           && refusal.Reason.Contains("SCENE.BIN", StringComparison.Ordinal));
        Assert.Contains(result.Report.Refusals, refusal => refusal is { Id: 2, Field: "short_description" }
                                                           && refusal.Reason.Contains("SCENE@0x", StringComparison.Ordinal));
        Assert.Equal(1, result.Report.ExitCode);
        Assert.DoesNotContain(shield.Cells, cell => cell.Field == "category" && cell.Raw != null);
    }

    [Fact]
    public void AValueAnExcludedBuilderWroteIsRefusedAndItsPictureLeftOut()
    {
        ExportResult result = GameDataExporter.Run(Game(Faults.ExcludedBuilderWrites));

        Assert.Empty(Entity(result, "item", 3).Pictures);
        Refusal refusal = Assert.Single(result.Report.Refusals);
        Assert.Equal(("item", 3, "icon"), (refusal.Kind, refusal.Id, refusal.Field));
        Assert.Contains("CGINIT.BIN", refusal.Reason, StringComparison.Ordinal);
    }

    [Fact]
    public void ATwoFieldReferenceNamesTheRecordItsTwoValuesMake()
    {
        // The skill is granted_skill × 2 + category (export plan §5): item 1 (skill 1, category 2) names skill 4 and
        // item 2 (skill 2, category 3) skill 7, which no record has; item 3's -1 names none.
        ExportInputs game = Game();
        ExportResult result = GameDataExporter.Run(new ExportInputs
        {
            GameId = game.GameId, Table = game.Table, Build = game.Build, Scripts = game.Scripts,
            ScriptByName = game.ScriptByName, BootStop = game.BootStop, Registry = Registry(skillKey: new RefKey(2, "category")),
            Declarations = game.Declarations, Pictures = game.Pictures, BuildInfo = game.BuildInfo,
        });

        ExportEntity sword = Entity(result, "item", 1);
        Assert.Equal([new ExportRef("granted_skill", 0, "skill", 4)], sword.Refs);
        Assert.Equal([1], Values(sword, "granted_skill"));
        Assert.Equal([new ExportRef("granted_skill", 0, "skill", 7)], Entity(result, "item", 2).Refs);
        Assert.Empty(Entity(result, "item", 3).Refs);
        Assert.Equal(2, result.Report.MissingRefs[("item", "granted_skill", "skill")]);
    }

    [Fact]
    public void ARowReferenceNamesEveryRecordOfItsRow()
    {
        // With any = 2 (export plan §5), a category of 2 names every skill of row granted_skill: item 1 (skill 1,
        // category 2) names row 1, skill 1 alone of it; item 2 (skill 2, category 3) names skill 2 × 1 + 3 = 5, which no
        // record has. An Arterial item that every card of a row may carry has slot 0.
        ExportInputs game = Game();
        ExportResult result = GameDataExporter.Run(new ExportInputs
        {
            GameId = game.GameId, Table = game.Table, Build = game.Build, Scripts = game.Scripts,
            ScriptByName = game.ScriptByName, BootStop = game.BootStop,
            Registry = Registry(skillKey: new RefKey(1, "category") { Any = 2 }),
            Declarations = game.Declarations, Pictures = game.Pictures, BuildInfo = game.BuildInfo,
        });

        ExportEntity sword = Entity(result, "item", 1);
        Assert.Equal([new ExportRef("granted_skill", 0, "skill", 1)], sword.Refs);
        Assert.Equal([0L], sword.KeyedRefs["granted_skill"]);
        Assert.Equal([1L], sword.KeyedRows["granted_skill"]);
        Assert.Equal([new ExportRef("granted_skill", 0, "skill", 5)], Entity(result, "item", 2).Refs);
        Assert.Equal([(long?)null], Entity(result, "item", 2).KeyedRows["granted_skill"]);

        // A row none of whose records is an entity is a missing reference: with times 3, row 1 is skills 3 to 5.
        result = GameDataExporter.Run(new ExportInputs
        {
            GameId = game.GameId, Table = game.Table, Build = game.Build, Scripts = game.Scripts,
            ScriptByName = game.ScriptByName, BootStop = game.BootStop,
            Registry = Registry(skillKey: new RefKey(3, "category") { Any = 2 }),
            Declarations = game.Declarations, Pictures = game.Pictures, BuildInfo = game.BuildInfo,
        });
        Assert.Empty(Entity(result, "item", 1).Refs);
        Assert.Equal(2, result.Report.MissingRefs[("item", "granted_skill", "skill")]);
    }

    // An item kind that joins its bonds (export plan §5): the first partner of each pair, and the tiers beside the
    // prices as a group whose rows are tiers.
    private static ExportInputs Bonds(string? bondsOf = "item")
    {
        ExportInputs game = Game(bonds: true);
        JsonNode declarations = JsonNode.Parse(Declarations)!;
        JsonNode item = declarations["kinds"]![0]!;
        item["joins"] = JsonNode.Parse("""["item_bond"]""");
        item["fields"] = JsonNode.Parse(
            """["category", "price", "price.sell", "granted_skill", "item_bond.partners.first_row", "item_bond.partners.second_row", "item_bond.tier"]""");
        item["groups"] = JsonNode.Parse("""[{"name": "tiers", "fields": ["price", "item_bond.tier"], "row_label": "Tier"}]""");
        return new ExportInputs
        {
            GameId = game.GameId, Table = game.Table, Build = game.Build, Scripts = game.Scripts,
            ScriptByName = game.ScriptByName, BootStop = game.BootStop, StopOf = game.StopOf,
            Registry = Registry(bondsOf: bondsOf), Declarations = ExportDeclarations.Parse(declarations.ToJsonString()),
            Pictures = game.Pictures, BuildInfo = game.BuildInfo,
        };
    }

    [Fact]
    public void AJoinedTablesRecordIsTheEntitysAndAPairOfElementsNamesARecord()
    {
        ExportResult result = GameDataExporter.Run(Bonds());
        ExportEntity sword = Entity(result, "item", 1);
        // Each pair of partners is row × 1 + slot: (1, 0) skill 1 and (1, 1) skill 2, which no record has.
        Assert.Equal([1], Values(sword, "item_bond.partners.first_row"));
        Assert.Equal([3, 4], Values(sword, "item_bond.tier"));
        Assert.Equal([new ExportRef("item_bond.partners.first_row", 0, "skill", 1),
                      new ExportRef("item_bond.partners.second_row", 0, "skill", 2)],
                     sword.Refs.Where(r => r.Field.StartsWith("item_bond.", StringComparison.Ordinal)));
        Assert.Equal(1, result.Report.MissingRefs[("item", "item_bond.partners.second_row", "skill")]);
        // Item 2's bond has no partner, and item 3 is past the bond table's three records: its cells are withheld.
        Assert.Equal([5, 0], Values(Entity(result, "item", 2), "item_bond.tier"));
        Assert.DoesNotContain(Entity(result, "item", 2).Refs, r => r.Field.StartsWith("item_bond.", StringComparison.Ordinal));
        ExportEntity spear = Entity(result, "item", 3);
        Assert.Equal([null, null], Values(spear, "item_bond.tier"));
        Assert.DoesNotContain(spear.Cells, cell => cell.Field.StartsWith("item_bond.", StringComparison.Ordinal));
        Assert.DoesNotContain(result.Report.Refusals, refusal => refusal.Field.StartsWith("item_bond.", StringComparison.Ordinal));
        // The modder cells name the joined table's fields as the kind publishes them, by element.
        Assert.Contains(new ExportModderCell("item_bond.partners", 2, GlobalBank.Integer, BondPartners + 6, "1", "BDINIT.BIN"),
                        sword.Cells);
        // The snippet sets the bond as a record of its own table, each published pair whole (its slot beside its row).
        Assert.EndsWith("""
            granted_skill = 1

            [item_bond.1.partners]
            first_row = 1
            first_slot = 0
            second_row = 1
            3 = 1

            [item_bond.1.tier]
            0 = 3
            1 = 4
            """.ReplaceLineEndings("\n") + "\n", sword.Snippet);
        Assert.DoesNotContain("[item_bond.", spear.Snippet ?? "", StringComparison.Ordinal);
        Assert.Empty(result.Report.SnippetProblems);

        string root = Path.Combine(Path.GetTempPath(), "ome-export-" + Guid.NewGuid().ToString("N"));
        try
        {
            ExportWriter.Write(result, root, "abcdef012345", Manifest);
            using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
            {
                DataSource = Path.Combine(root, "abcdef012345.sqlite"), Mode = SqliteOpenMode.ReadOnly, Pooling = false,
            }.ToString());
            connection.Open();
            string? Scalar(string sql)
            {
                using SqliteCommand command = connection.CreateCommand();
                command.CommandText = sql;
                return command.ExecuteScalar()?.ToString();
            }
            // A joined field is in the kind's field table, with the joined table's reference, and a group's row word.
            Assert.Equal("1|skill|skill", Scalar("SELECT published || '|' || ref_table || '|' || ref_kind FROM field "
                                                 + "WHERE kind = 'item' AND name = 'item_bond.partners.first_row'"));
            Assert.Equal("0", Scalar("SELECT published FROM field WHERE kind = 'item' AND name = 'item_bond.partners'"));
            Assert.Equal("tiers||Tier", Scalar("SELECT name || '|' || ifnull(labels, '') || '|' || row_label FROM group_label "
                                               + "WHERE kind = 'item'"));
            Assert.Equal("[3,4]", Scalar("SELECT \"item_bond.tier\" FROM item WHERE id = 1"));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void AKindJoinsOnlyATableWhoseRecordsAreItsOwn()
    {
        ExportFailure failure = Assert.Throws<ExportFailure>(() => GameDataExporter.Run(Bonds(bondsOf: null)));
        Assert.Contains("joins table item_bond, whose records are not item's", failure.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ADerivedScriptsWriteIsWithheldWithoutARefusal()
    {
        ExportResult result = GameDataExporter.Run(Game(Faults.DerivedScriptWrites));

        ExportEntity sword = Entity(result, "item", 1);
        Assert.Equal([null], Values(sword, "category"));
        Assert.Empty(result.Report.Refusals);
        Assert.Equal(0, result.Report.ExitCode);
        Assert.Contains(new ExportModderCell("category", 0, GlobalBank.Integer, ItemCategory + 1, null, "COUNTUP.BIN"),
                        sword.Cells);
        // Its unnamed count is play state, not an array the registry has yet to name.
        Assert.Empty(result.Report.UnnamedCells);
    }

    [Fact]
    public void AStringWithNoOriginIsRefused()
    {
        ExportResult result = GameDataExporter.Run(Game(Faults.StringWithoutOrigin));

        Refusal refusal = Assert.Single(result.Report.Refusals);
        Assert.Equal(("item", 3, "short_description", "a string with no origin"),
                     (refusal.Kind, refusal.Id, refusal.Field, refusal.Reason));
        Assert.Null(Text(Entity(result, "item", 3), "short_description"));
    }

    [Fact]
    public void ADescriptionScriptThatIsNoTextScriptIsRefused()
    {
        ExportResult result = GameDataExporter.Run(Game(Faults.HandlerIsNoTextScript));

        Refusal refusal = Assert.Single(result.Report.Refusals);
        Assert.Equal(("item", 2, "description"), (refusal.Kind, refusal.Id, refusal.Field));
        Assert.Null(Text(Entity(result, "item", 2), "description"));
        Assert.Equal(1, result.Report.DescriptionRuns);
    }

    [Fact]
    public void ABootThatDoesNotReachItsPointFails()
    {
        ExportInputs game = Game();
        var failure = Assert.Throws<ExportFailure>(() => GameDataExporter.Run(new ExportInputs
        {
            GameId = game.GameId, Table = game.Table, Build = game.Build, Scripts = game.Scripts,
            ScriptByName = game.ScriptByName, BootStop = new RunStop("BOOT", 0x7777), Declarations = game.Declarations,
            Registry = game.Registry,
        }));
        Assert.Contains("ended with exit", failure.Message, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------------------------------------- the format (§7)

    private static readonly ExportManifest Manifest = new(new DateTimeOffset(2026, 11, 21, 0, 0, 0, TimeSpan.Zero),
                                                          "data-export --game testgame", null, "v0.0.0-test", "0000000");

    [Fact]
    public void TwoExportsAreTheSameBytes()
    {
        string root = Path.Combine(Path.GetTempPath(), "ome-export-" + Guid.NewGuid().ToString("N"));
        try
        {
            ExportFiles first = ExportWriter.Write(GameDataExporter.Run(Game()), Path.Combine(root, "a"), "abcdef012345", Manifest);
            ExportFiles second = ExportWriter.Write(GameDataExporter.Run(Game()), Path.Combine(root, "b"), "abcdef012345", Manifest);

            Assert.Equal(first.Files, second.Files);
            Assert.Equal(File.ReadAllBytes(Path.Combine(root, "a", "abcdef012345.sqlite")),
                         File.ReadAllBytes(Path.Combine(root, "b", "abcdef012345.sqlite")));
            Assert.Equal(File.ReadAllText(Path.Combine(root, "a", "manifest.json")),
                         File.ReadAllText(Path.Combine(root, "b", "manifest.json")));
            Assert.Equal("png 77"u8.ToArray(), File.ReadAllBytes(Path.Combine(root, "a", "pictures", "abcdef012345", "ICON077.png")));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void TheDatabaseHoldsAKindsGroupsInTheirOrder()
    {
        // A group (export plan §5). The lint checks that its fields are rows of cells, each width a multiple of the
        // narrowest's; the writer writes the declaration as it is given, so this one need not be.
        JsonNode declarations = JsonNode.Parse(Declarations)!;
        declarations["kinds"]![0]!["groups"] = JsonNode.Parse("""[{"name": "prices", "fields": ["price", "category"]}]""");
        ExportInputs game = Game();
        ExportResult result = GameDataExporter.Run(new ExportInputs
        {
            GameId = game.GameId, Table = game.Table, Build = game.Build, Scripts = game.Scripts,
            ScriptByName = game.ScriptByName, BootStop = game.BootStop, Registry = game.Registry,
            Declarations = ExportDeclarations.Parse(declarations.ToJsonString()), Pictures = game.Pictures,
            BuildInfo = game.BuildInfo,
        });
        Assert.Equal([new GroupDeclaration("prices", ["price", "category"])],
                     result.Kinds[0].Declaration.Groups, new GroupComparer());
        string root = Path.Combine(Path.GetTempPath(), "ome-export-" + Guid.NewGuid().ToString("N"));
        try
        {
            ExportWriter.Write(result, root, "abcdef012345", Manifest);
            using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
            {
                DataSource = Path.Combine(root, "abcdef012345.sqlite"), Mode = SqliteOpenMode.ReadOnly, Pooling = false,
            }.ToString());
            connection.Open();
            string? Scalar(string sql)
            {
                using SqliteCommand command = connection.CreateCommand();
                command.CommandText = sql;
                return command.ExecuteScalar()?.ToString();
            }
            Assert.Equal("prices:0:price,prices:1:category", Scalar(
                "SELECT group_concat(name || ':' || position || ':' || field, ',') FROM "
                + "(SELECT * FROM field_group WHERE kind = 'item' ORDER BY name, position)"));
            Assert.Equal("0", Scalar("SELECT count(*) FROM field_group WHERE kind = 'skill'"));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    private sealed class GroupComparer : IEqualityComparer<GroupDeclaration>
    {
        public bool Equals(GroupDeclaration? x, GroupDeclaration? y)
            => x is not null && y is not null && x.Name == y.Name && x.Fields.SequenceEqual(y.Fields);

        public int GetHashCode(GroupDeclaration group) => group.Name.GetHashCode(StringComparison.Ordinal);
    }

    [Fact]
    public void AMasksBitsTakeTheNamesOfAListOfStrings()
    {
        // bits_of a list of strings (export plan §5): element k names bit k, as Arterial's race names do its races.
        ExportInputs game = Game();
        string declarations = Declarations.Replace("\"fields\": [\"category\", \"price\", \"price.sell\", \"granted_skill\"]",
                                                   "\"fields\": [\"category\", \"handler_script_id\"]", StringComparison.Ordinal);
        ExportResult result = GameDataExporter.Run(new ExportInputs
        {
            GameId = game.GameId, Table = game.Table, Build = game.Build, Scripts = game.Scripts,
            ScriptByName = game.ScriptByName, BootStop = game.BootStop, Registry = Registry(bitNames: true),
            Declarations = ExportDeclarations.Parse(declarations), Pictures = game.Pictures, BuildInfo = game.BuildInfo,
        });
        Assert.Empty(result.Report.Refusals);
        ExportKind item = result.Kinds.Single(kind => kind.Declaration.Id == "item");
        Assert.Equal([new ExportValueName("handler_script_id", 1, "ja", "武器"), new ExportValueName("handler_script_id", 2, "ja", "防具")],
                     item.ValueNames.OrderBy(name => name.Value));
    }

    [Fact]
    public void AShownFieldsValuesHaveTheRegistrysWordsAndTheGamesNamesAndItsNumbersTheirFormat()
    {
        // The value facets (export plan §5): item_category's values are named by the game's table, element k naming
        // value k + 1, and 0 has OME's word; a price is shown halved, in gold, and 0 means none. A label names the
        // category for a reader.
        ExportInputs game = Game();
        // Qualifiers, marks and reference headings (export plan §5) go to the database as the declarations give them.
        string declarations = Declarations.Replace("\"filters\": [\"category\"],",
                                                   "\"filters\": [\"category\"], \"labels\": {\"category\": \"Kind\"}, "
                                                   + "\"about\": \"Things to carry.\", "
                                                   + "\"qualifiers\": [\"category\", \"short_description\"], "
                                                   + "\"marks\": [{\"field\": \"category\", \"word\": \"Gear\"}], "
                                                   + "\"referenced_as\": [{\"field\": \"granted_skill\", "
                                                   + "\"heading\": \"Granted by\", \"details\": [\"price\"], "
                                                   + "\"parts\": {}}],",
                                                   StringComparison.Ordinal);
        ExportResult result = GameDataExporter.Run(new ExportInputs
        {
            GameId = game.GameId, Table = game.Table, Build = game.Build, Scripts = game.Scripts,
            ScriptByName = game.ScriptByName, BootStop = game.BootStop, Registry = Registry(formats: true),
            Declarations = ExportDeclarations.Parse(declarations), Pictures = game.Pictures, BuildInfo = game.BuildInfo,
        });
        Assert.Empty(result.Report.Refusals);
        ExportKind item = result.Kinds.Single(kind => kind.Declaration.Id == "item");
        Assert.Equal([new ExportValueName("category", 0, "", "None"), new ExportValueName("category", 2, "ja", "武器"),
                      new ExportValueName("category", 3, "ja", "防具")],
                     item.ValueNames.OrderBy(name => name.Value));
        Assert.Empty(result.Kinds.Single(kind => kind.Declaration.Id == "skill").ValueNames);
        Assert.Equal([("category", "Kind")], item.Declaration.Labels);
        // A row of cells' columns take the game's names; one column of it is no row, and has none.
        Assert.Equal([new ExportColumnName("price", 1, "ja", "武器"), new ExportColumnName("price", 2, "ja", "防具")],
                     item.ColumnNames.OrderBy(name => name.Column));

        string root = Path.Combine(Path.GetTempPath(), "ome-export-" + Guid.NewGuid().ToString("N"));
        try
        {
            ExportWriter.Write(result, root, "abcdef012345", Manifest);
            using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
            {
                DataSource = Path.Combine(root, "abcdef012345.sqlite"), Mode = SqliteOpenMode.ReadOnly, Pooling = false,
            }.ToString());
            connection.Open();
            string? Scalar(string sql)
            {
                using SqliteCommand command = connection.CreateCommand();
                command.CommandText = sql;
                return command.ExecuteScalar()?.ToString();
            }
            Assert.Equal("0::None,2:ja:武器,3:ja:防具", Scalar(
                "SELECT group_concat(value || ':' || lang || ':' || text, ',') FROM "
                + "(SELECT * FROM value_name WHERE kind = 'item' AND field = 'category' ORDER BY value, lang)"));
            // A published column of a field shares its global's format.
            Assert.Equal("price:0.5:gold:0:0,price.sell:0.5:gold:0:0", Scalar(
                "SELECT group_concat(field || ':' || scale || ':' || unit || ':' || bits || ':' || none, ',') FROM "
                + "(SELECT * FROM value_format WHERE kind = 'item' AND scale IS NOT NULL ORDER BY field)"));
            Assert.Equal("0::", Scalar(
                "SELECT bits || ':' || ifnull(scale, '') || ':' || ifnull(none, '') FROM value_format WHERE field = 'category'"));
            Assert.Equal("Kind", Scalar("SELECT text FROM label WHERE kind = 'item' AND name = 'category'"));
            Assert.Equal("0:category,1:short_description", Scalar(
                "SELECT group_concat(position || ':' || field, ',') FROM (SELECT * FROM qualifier ORDER BY position)"));
            Assert.Equal("item:0:category:Gear", Scalar("SELECT kind || ':' || position || ':' || field || ':' || word FROM mark"));
            Assert.Equal("item:granted_skill::Granted by:[\"price\"]", Scalar(
                "SELECT kind || ':' || field || ':' || part || ':' || heading || ':' || details FROM referenced_as"));
            Assert.Equal("1:ja:武器,2:ja:防具", Scalar(
                "SELECT group_concat(column_index || ':' || lang || ':' || text, ',') FROM "
                + "(SELECT * FROM column_name WHERE kind = 'item' AND field = 'price' ORDER BY column_index)"));
            Assert.Equal("price:skill", Scalar(
                "SELECT group_concat(name || ':' || column_kind, ',') FROM field WHERE column_kind IS NOT NULL"));
            Assert.Equal("Things to carry.:", Scalar(
                "SELECT group_concat(ifnull(about, ''), ':') FROM (SELECT * FROM kind ORDER BY id)"));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void TheDatabaseHoldsTheTablesTheSiteReads()
    {
        string root = Path.Combine(Path.GetTempPath(), "ome-export-" + Guid.NewGuid().ToString("N"));
        try
        {
            ExportWriter.Write(GameDataExporter.Run(Game()), root, "abcdef012345", Manifest);
            string connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = Path.Combine(root, "abcdef012345.sqlite"), Mode = SqliteOpenMode.ReadOnly, Pooling = false,
            }.ToString();
            using var connection = new SqliteConnection(connectionString);
            connection.Open();
            string? Scalar(string sql)
            {
                using SqliteCommand command = connection.CreateCommand();
                command.CommandText = sql;
                return command.ExecuteScalar()?.ToString();
            }

            Assert.Equal("1", Scalar("PRAGMA user_version"));
            Assert.Equal("abcdef012345", Scalar("SELECT value FROM build_info WHERE key = 'revision'"));
            Assert.Equal("テスト", Scalar("SELECT value FROM build_info WHERE key = 'title'"));
            Assert.Equal("3", Scalar("SELECT entities FROM kind WHERE id = 'item'"));
            Assert.Equal("[\"ability\"]", Scalar("SELECT aliases FROM kind WHERE id = 'skill'"));
            Assert.Equal("[10,5]", Scalar("SELECT price FROM item WHERE id = 1"));
            Assert.Equal("5", Scalar("SELECT \"price.sell\" FROM item WHERE id = 1"));
            Assert.Equal("【剣】\n鋭い", Scalar("SELECT value FROM text WHERE kind = 'item' AND id = 1 AND field = 'description'"));
            Assert.Equal("ja", Scalar("SELECT DISTINCT lang FROM text"));
            Assert.Equal("skill", Scalar("SELECT to_kind FROM ref WHERE from_id = 1"));
            Assert.Equal("pictures/abcdef012345/ICON077.png", Scalar("SELECT file FROM picture WHERE id = 1"));
            Assert.Equal("1", Scalar("SELECT explicit FROM picture WHERE id = 1"));
            Assert.Equal("ITINIT.BIN", Scalar("SELECT writer FROM cell WHERE id = 1 AND field = 'handler_script_id'"));
            // The field table: a column's row, a picture field, a reference, a description.
            Assert.Equal("1", Scalar("SELECT column_index FROM field WHERE kind = 'item' AND name = 'price.sell'"));
            // An array field lists its elements' names, null where one has none; a single cell lists none.
            Assert.Equal("[null,\"sell\"]", Scalar("SELECT columns FROM field WHERE kind = 'item' AND name = 'price'"));
            Assert.Equal("", Scalar("SELECT ifnull(columns, '') FROM field WHERE kind = 'item' AND name = 'price.sell'"));
            Assert.Equal("icon", Scalar("SELECT picture FROM field WHERE kind = 'item' AND name = 'icon'"));
            Assert.Equal("skill|skill", Scalar("SELECT ref_table || '|' || ref_kind FROM field WHERE name = 'granted_skill'"));
            Assert.Equal("description", Scalar("SELECT type FROM field WHERE name = 'description'"));
            Assert.Equal("0", Scalar("SELECT count(*) FROM relation"));
            Assert.Equal(64, Scalar("SELECT content_hash FROM item WHERE id = 1")!.Length);
            Assert.StartsWith("[item.1]\nname = \"剣\"", Scalar("SELECT text FROM snippet WHERE kind = 'item' AND id = 1"),
                              StringComparison.Ordinal);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    // ------------------------------------------------------------------------------------------------- the inputs

    [Fact]
    public void BuildersMatchTheBaseScriptAndAnExcludedOneNever()
    {
        var rule = new BuilderRule(["INIT2", "*INIT", "*INIT2", "AUTORUN"], ["CGINIT"]);
        Assert.True(rule.Allows("$3$ITINIT.BIN"));
        Assert.True(rule.Allows("INIT2.BIN"));
        Assert.True(rule.Allows("$1$AUTORUN.BIN"));
        Assert.True(rule.Allows("BTANINIT2.BIN"));
        Assert.False(rule.Allows("CGINIT.BIN"));
        Assert.False(rule.Allows("$2$CGINIT.BIN"));
        Assert.False(rule.Allows("SETSTUDIOORN.BIN"));
        Assert.False(rule.Allows("SC0100.BIN"));
        Assert.Equal("ITINIT", ScriptNames.Base("$3$itinit.bin"));

        var derived = new BuilderRule(["*INIT"], [], ["COUNTSKILL"]);
        Assert.True(derived.Derives("$1$COUNTSKILL.BIN"));
        Assert.False(derived.Derives("ITINIT.BIN"));
        Assert.False(derived.Derives(null));
        Assert.True(derived.With(["DROPSET"]).Derives("COUNTSKILL.BIN"));
        Assert.False(rule.Derives("COUNTSKILL.BIN"));
    }

    [Fact]
    public void TheRegistryViewReadsTablesArraysAndColumnNames()
    {
        string path = Path.Combine(Path.GetTempPath(), "ome-globals-" + Guid.NewGuid().ToString("N") + ".json");
        File.WriteAllText(path, """
            {"cells": {
              "G:0x10": {"address": "0x10", "bank": "G", "name": "unit_graphic", "type": "int[3][4]",
                         "columns": {"0": "face_CA_AA", "2": "sprite"}},
              "GS:0x20": {"address": "0x20", "bank": "GS", "name": "unit_names", "type": "string[3]"},
              "G:0x40": {"address": "0x40", "bank": "G", "name": "current_unit", "type": "int"},
              "G:0x50": {"address": "0x50", "bank": "G", "name": "a_list", "type": "int[4]", "columns": {"1": "second"}}},
             "tables": [{"name": "unit", "records": 3, "fields": {"graphic": "unit_graphic", "name": "unit_names"},
                         "refs": {"graphic": "picture"}},
                        {"name": "block", "records": 1, "fields": {"unit": "current_unit", "list": "a_list"}},
                        {"name": "bad", "records": 3, "fields": {"list": "a_list"}}]}
            """);
        try
        {
            RegistryView registry = RegistryView.Load(path);
            TableLayout unit = registry.Table("unit");
            Assert.Equal("picture", unit.Refs["graphic"]);
            FieldLayout face = registry.Resolve(unit, "graphic.face_CA_AA");
            Assert.Equal((0x10 + 2 * 4 + 0, 1, false), (face.Cell(2, 0), face.Elements, face.IsArray));
            Assert.Equal(1, registry.Resolve(unit, "graphic.1").Column);   // a column with no name, by number
            Assert.Throws<InvalidDataException>(() => registry.Resolve(unit, "graphic.2"));   // named: by its name only
            FieldLayout graphic = registry.Resolve(unit, "graphic");
            Assert.Equal(("int[4]", true), (graphic.TypeName, graphic.IsArray));
            Assert.Equal("string", registry.Resolve(unit, "name").TypeName);
            Assert.Equal(0x40, registry.IntegerGlobal("current_unit"));
            Assert.Throws<InvalidDataException>(() => registry.IntegerGlobal("unit_names"));
            // A one-record table (export plan §5): a scalar is its one cell, a list of n its one row of n.
            TableLayout block = registry.Table("block");
            Assert.Equal((0x40, 1, false), (registry.Resolve(block, "unit").Cell(0, 0), registry.Resolve(block, "unit").Elements,
                                            registry.Resolve(block, "unit").IsArray));
            FieldLayout list = registry.Resolve(block, "list");
            Assert.Equal((0x50 + 2, 4, true, "int[4]"), (list.Cell(0, 2), list.Elements, list.IsArray, list.TypeName));
            Assert.Equal(0x50 + 1, registry.Resolve(block, "list.second").Cell(0, 0));
            Assert.Throws<InvalidDataException>(() => registry.Resolve(registry.Table("bad"), "list"));   // 4 is not 3
            Assert.Equal([3, 4], registry.IntegerArray("unit_graphic").Dimensions);
        }
        finally
        {
            File.Delete(path);
        }
    }

    // ------------------------------------------------------------------------------------------------- script relations (§6.7)

    private const string Relations = """
        "script_relations": [
         {"id": "given_in", "negative": "taken_in", "call": "ADDITEM", "kind": "item", "key": "current_item_id",
          "count": "current_item_count_change", "where": {}, "exclude": ["DEBUG*"]},
         {"id": "joins_in", "negative": null, "call": "JOIN", "kind": "skill", "key": "joining_skill_ids", "count": null,
          "where": {"joining_skill_count": 1}, "exclude": []}]
        """;

    private static ExportResult WithRelations()
    {
        ExportInputs game = Game();
        return GameDataExporter.Run(WithDeclarations(
            game, Declarations.Replace("\"script_relations\": []", Relations.Trim(), StringComparison.Ordinal),
            index: IndexOf(game)));
    }

    private static int CallAt(ExportInputs game, string script, int call)
        => game.ScriptByName(script)!.Instructions.Where(instruction => instruction.Opcode == OpCallScript)
            .ElementAt(call).Offset;

    [Fact]
    public void AGivingSceneAndATakingSceneGiveRowsOnTheirItems()
    {
        ExportInputs game = Game();
        ExportResult result = WithRelations();
        Assert.Equal([new ExportRelation("given_in", "GIFT.BIN", CallAt(game, "GIFT", 0), 3)],
                     Entity(result, "item", 1).Relations);
        // The relation of a negative count is the negative one, and its count stays signed.
        Assert.Equal([new ExportRelation("given_in", "GIFT.BIN", CallAt(game, "GIFT", 1), 1),
                      new ExportRelation("taken_in", "$1$TAKE.BIN", CallAt(game, "$1$TAKE", 0), -5)],
                     Entity(result, "item", 2).Relations);
        Assert.Empty(Entity(result, "item", 3).Relations);                    // DEBUGGIVE's is excluded
        RelationScan scan = result.Report.Relations[0];
        Assert.Equal((11, 4, 3, 1), (scan.Calls, scan.Scripts, scan.Rows, scan.Excluded));
        Assert.Equal([("ODD.BIN", CallAt(game, "ODD", 2), 9L)], scan.Keyless);
    }

    [Fact]
    public void ACallWhoseValuesTheBlockDoesNotSetIsUnresolvedWithItsReason()
    {
        ExportInputs game = Game();
        int[] odd = [.. Enumerable.Range(0, 6).Select(call => CallAt(game, "ODD", call))];
        var unresolved = WithRelations().Report.Relations[0].Unresolved.ToDictionary(call => (call.Script, call.Offset),
                                                                                    call => call.Reason);
        Assert.Equal(6, unresolved.Count);
        int taken = CallAt(game, "$1$TAKE", 0);
        Assert.Equal($"current_item_id: the call at 0x{taken:x} may write it; current_item_count_change: the call at "
                     + $"0x{taken:x} may write it", unresolved[("$1$TAKE.BIN", CallAt(game, "$1$TAKE", 1))]);
        Assert.Equal("current_item_id: computed at 0x0", unresolved[("ODD.BIN", odd[0])]);
        Assert.Equal("current_item_count_change is 0", unresolved[("ODD.BIN", odd[1])]);
        int sub = game.ScriptByName("ODD")!.Instructions.Single(instruction => instruction.Opcode == OpCall).Offset;
        Assert.Equal($"current_item_id: the call at 0x{sub:x} may write it; current_item_count_change: the call at "
                     + $"0x{sub:x} may write it", unresolved[("ODD.BIN", odd[3])]);
        Assert.Equal("current_item_id: no write before the call in its block; current_item_count_change: no write before "
                     + "the call in its block", unresolved[("ODD.BIN", odd[4])]);
        int lookup = game.ScriptByName("ODD")!.Instructions.Single(instruction => instruction.Opcode == OpLookupArray).Offset;
        Assert.Equal($"current_item_id: its address is taken at 0x{lookup:x}", unresolved[("ODD.BIN", odd[5])]);
    }

    [Fact]
    public void AWhereNotMetLeavesACallUnresolvedAndAListKeyIsItsFirstCell()
    {
        ExportInputs game = Game();
        ExportResult result = WithRelations();
        Assert.Equal([new ExportRelation("joins_in", "RECRUIT.BIN", CallAt(game, "RECRUIT", 0), null)],
                     Entity(result, "skill", 1).Relations);
        RelationScan scan = result.Report.Relations[1];
        Assert.Equal([("RECRUIT.BIN", CallAt(game, "RECRUIT", 1), "joining_skill_count is 2, not 1")], scan.Unresolved);
        Assert.Equal((2, 1, 1, 0), (scan.Calls, scan.Scripts, scan.Rows, scan.Excluded));
    }

    [Fact]
    public void RelationsNeedTheScriptIndex()
    {
        string declarations = Declarations.Replace("\"script_relations\": []", Relations.Trim(), StringComparison.Ordinal);
        var error = Assert.Throws<ExportFailure>(() => GameDataExporter.Run(WithDeclarations(Game(), declarations)));
        Assert.Contains("script index", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheDatabaseHoldsTheRelationsAndTheyChangeTheEntitysHash()
    {
        string root = Path.Combine(Path.GetTempPath(), "ome-export-" + Guid.NewGuid().ToString("N"));
        try
        {
            ExportResult result = WithRelations();
            ExportWriter.Write(result, root, "abcdef012345", Manifest);
            string connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = Path.Combine(root, "abcdef012345.sqlite"), Mode = SqliteOpenMode.ReadOnly, Pooling = false,
            }.ToString();
            using var connection = new SqliteConnection(connectionString);
            connection.Open();
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT kind || ' ' || id || ' ' || relation || ' ' || script || ' ' || ifnull(count, 'null') "
                                  + "FROM relation ORDER BY kind, id, relation, script, offset";
            var rows = new List<string>();
            using (SqliteDataReader reader = command.ExecuteReader())
                while (reader.Read()) rows.Add(reader.GetString(0));
            Assert.Equal(["item 1 given_in GIFT.BIN 3", "item 2 given_in GIFT.BIN 1", "item 2 taken_in $1$TAKE.BIN -5",
                          "skill 1 joins_in RECRUIT.BIN null"], rows);
            Assert.NotEqual(ExportWriter.ContentHash(Entity(GameDataExporter.Run(Game()), "item", 2)),
                            ExportWriter.ContentHash(Entity(result, "item", 2)));
            Assert.Equal(ExportWriter.ContentHash(Entity(GameDataExporter.Run(Game()), "item", 3)),
                         ExportWriter.ContentHash(Entity(result, "item", 3)));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    // ------------------------------------------------------------------------------------------------- loaders (§6.3)

    private static ExportInputs WithDeclarations(ExportInputs game, string declarations, Func<string, RunStop>? stopOf = null,
                                                 Ome.Mods.Data.DataShape? shape = null, ScriptIndex? index = null)
        => new()
        {
            GameId = game.GameId, Table = game.Table, Build = game.Build, Scripts = game.Scripts,
            ScriptByName = game.ScriptByName, BootStop = game.BootStop, StopOf = stopOf ?? game.StopOf,
            Declarations = ExportDeclarations.Parse(declarations), Registry = game.Registry, Pictures = game.Pictures,
            BuildInfo = game.BuildInfo, Shape = shape ?? game.Shape, ScriptIndex = index ?? game.ScriptIndex,
        };

    // ------------------------------------------------------------------------------------------------- languages (§6.4)

    private const string ItInitFingerprint = "sha256:0000000000000000000000000000000000000000000000000000000000000000";

    /// <summary>English and French for the synthetic game (plan §4.6), from one package's text root in
    /// <paramref name="root"/>, its site entries bound as the install's ITINIT. English: a site entry at ITINIT's store
    /// of 剣, a global entry for よく切れる, and ITMES's message as one sentence of two lines. French: 剣 at the same site,
    /// and the message's brackets alone, so the message misses and its runs resolve on their own.</summary>
    private static IReadOnlyList<ExportLanguage> Languages(string root, ExportInputs game)
    {
        void Write(string file, string text)
        {
            string path = Path.Combine(root, file);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, text, new UTF8Encoding(false));
        }
        int swordAt = game.ScriptByName("ITINIT")!.Instructions[0].Offset;
        Write("en/ITINIT.toml", $$"""
            script = "ITINIT"
            fingerprint = "{{ItInitFingerprint}}"

            [[text]]
            source = "剣"
            at = {{swordAt}}
            text = "Sword"
            """);
        Write("en/_global.toml", """
            [[text]]
            source = "よく切れる"
            text = "Cuts well"

            [[text]]
            source = "【剣】鋭い"
            text = "[Sword]\nSharp"
            """);
        Write("fr/ITINIT.toml", $$"""
            script = "ITINIT"
            fingerprint = "{{ItInitFingerprint}}"

            [[text]]
            source = "剣"
            at = {{swordAt}}
            text = "Épée"
            """);
        Write("fr/_global.toml", """
            [[text]]
            source = "【"
            text = "«"

            [[text]]
            source = "】"
            text = "»"
            """);
        var source = new TextSource(root, [new LanguageSettings("en"), new LanguageSettings("fr")], "test.languages");
        var binder = new TextSiteBinder(script => script == "ITINIT" ? ItInitFingerprint : null);
        var package = new ExportLanguagePackage("test.languages", "Test languages", "1.2.0", ["A. Translator"]);
        return [.. new[] { "en", "fr" }.Select(tag =>
            new ExportLanguage(tag, TextRootLoader.Load([source], tag, binder, new TextLoadReport()), [package]))];
    }

    private static ExportInputs WithLanguages(ExportInputs game, IReadOnlyList<ExportLanguage> languages)
        => new()
        {
            GameId = game.GameId, Table = game.Table, Build = game.Build, Scripts = game.Scripts,
            ScriptByName = game.ScriptByName, BootStop = game.BootStop, StopOf = game.StopOf,
            Declarations = game.Declarations, Registry = game.Registry, Pictures = game.Pictures,
            BuildInfo = game.BuildInfo, Shape = game.Shape, ScriptIndex = game.ScriptIndex,
            Languages = languages, Messages = new ScriptMessageIndex(Table, Contracts),
        };

    [Fact]
    public void ALanguageLooksUpATableStringAtItsOriginAndADescriptionAsTheEngineDrawsIt()
    {
        string root = Path.Combine(Path.GetTempPath(), "ome-export-" + Guid.NewGuid().ToString("N"));
        try
        {
            ExportInputs game = Game();
            ExportResult result = GameDataExporter.Run(WithLanguages(game, Languages(root, game)));

            ExportEntity sword = Entity(result, "item", 1), shield = Entity(result, "item", 2);
            Assert.Equal("Sword", Text(sword, "name", language: "en"));     // the site entry at ITINIT's store
            Assert.Equal("剣", Text(sword, "name"));                       // the source text stays
            Assert.Equal("Cuts well", Text(sword, "short_description", language: "en"));
            // The message is one sentence, which draws its own line break; its later runs and the line end between them
            // draw nothing.
            Assert.Equal("[Sword]\nSharp", Text(sword, "description", language: "en"));
            // A miss gives no row: the site shows the source text in its place.
            Assert.Null(Text(shield, "name", language: "en"));
            // French has no sentence: it misses, and each run resolves on its own, the item's name at the instruction
            // that stored it, through the boot's string stores.
            Assert.Equal("«Épée»\n鋭い", Text(sword, "description", language: "fr"));
            Assert.Equal("Épée", Text(sword, "name", language: "fr"));
            Assert.Null(Text(sword, "short_description", language: "fr"));

            LanguageReport english = result.Report.Languages[0], french = result.Report.Languages[1];
            Assert.Equal(("en", 5, 2, 1, 1, 0), (english.Language.Tag, english.Strings, english.StringsTranslated,
                                                 english.Descriptions, english.DescriptionsTranslated,
                                                 english.DescriptionsPartly));
            Assert.Equal(("fr", 5, 1, 1, 0, 1), (french.Language.Tag, french.Strings, french.StringsTranslated,
                                                french.Descriptions, french.DescriptionsTranslated,
                                                french.DescriptionsPartly));
            // The misses are the port's, of the exported text: each string at its origin, and French's message once, at
            // its first run; OTHER's line, which the description drops, is none.
            Assert.Equal([("槍", "$1$ITINIT"), ("盾", "ITINIT"), ("斬撃", "SKINIT")],
                         english.Misses.Select(miss => (miss.Source, miss.Script)));
            Script itmes = game.ScriptByName("ITMES")!;
            Assert.Equal([("【剣】鋭い", "ITMES", itmes.Instructions[1].Offset)],
                         french.Misses.Where(miss => miss.Script == "ITMES"));
            Assert.Equal(["槍", "よく切れる", "盾", "斬撃"], french.Misses.Where(miss => miss.Script != "ITMES")
                                                                .Select(miss => miss.Source));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void TheDatabaseHoldsEachLanguagesTextAndItsPackages()
    {
        string root = Path.Combine(Path.GetTempPath(), "ome-export-" + Guid.NewGuid().ToString("N"));
        try
        {
            ExportInputs game = Game();
            ExportResult source = GameDataExporter.Run(game);
            ExportResult result = GameDataExporter.Run(WithLanguages(game, Languages(Path.Combine(root, "text"), game)));
            ExportWriter.Write(result, Path.Combine(root, "export"), "abcdef012345", Manifest);
            using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
            {
                DataSource = Path.Combine(root, "export", "abcdef012345.sqlite"), Mode = SqliteOpenMode.ReadOnly,
                Pooling = false,
            }.ToString());
            connection.Open();
            string? Scalar(string sql)
            {
                using SqliteCommand command = connection.CreateCommand();
                command.CommandText = sql;
                return command.ExecuteScalar()?.ToString();
            }

            Assert.Equal("Sword", Scalar("SELECT value FROM text WHERE kind = 'item' AND id = 1 AND field = 'name' AND lang = 'en'"));
            Assert.Equal("3", Scalar("SELECT count(*) FROM text WHERE lang = 'en'"));
            Assert.Equal("Épée|«Épée»\n鋭い", Scalar("SELECT group_concat(value, '|') FROM (SELECT value FROM text "
                                                    + "WHERE lang = 'fr' ORDER BY field DESC)"));
            Assert.Equal("ja,en,fr", Scalar("SELECT value FROM build_info WHERE key = 'languages'"));
            JsonArray packages = JsonNode.Parse(Scalar("SELECT value FROM build_info WHERE key = 'language_packages'")!)!
                .AsArray();
            Assert.Equal(["en", "fr"], packages.Select(package => package!["language"]!.GetValue<string>()));
            Assert.Equal("""{"language":"en","id":"test.languages","name":"Test languages","version":"1.2.0","authors":["A. Translator"]}""",
                         packages[0]!.ToJsonString());
            // A translated entity's hash covers its text in each language; one no language translates keeps its hash.
            Assert.NotEqual(ExportWriter.ContentHash(Entity(source, "item", 1)), ExportWriter.ContentHash(Entity(result, "item", 1)));
            Assert.Equal(ExportWriter.ContentHash(Entity(source, "item", 2)), ExportWriter.ContentHash(Entity(result, "item", 2)));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>The script index of every script the synthetic game has, run or not, as the CLI builds it from the
    /// catalog.</summary>
    private static ScriptIndex IndexOf(ExportInputs game)
        => ScriptIndex.Build(
            Enumerable.Range(1, 19).Select(id => game.Scripts.GetById(id) is { } script
                                                     ? new IndexedScript(script.Name, id, null, script)
                                                     : null)
                .OfType<IndexedScript>().Append(new IndexedScript("BOOT.BIN", 0, null, game.ScriptByName("BOOT")!)),
            Table, ScriptReading.Of(Contracts, Table.AbiId));

    private static readonly Ome.Script.Abi.OpcodeContracts Contracts = Ome.Script.Abi.OpcodeContracts.Load(Paths.OpcodesJson);

    private static long?[] Loaded(ExportEntity entity, string table, int record, string field)
        => entity.Loaded.Single(row => row.Table == table && row.Record == record).Values.Single(value => value.Field == field).Values;

    [Fact]
    public void ALoaderBuildsATableForEachKeyAndReadsItAtItsPoint()
    {
        ExportResult result = GameDataExporter.Run(WithDeclarations(Game(), WithLoader()));
        Assert.Equal(2, result.Report.LoaderRuns);                                     // item 3 is skipped
        Assert.Empty(result.Report.LoaderHalts);
        Assert.Empty(result.Report.Refusals);                                          // DROPSET is the loader's builder
        ExportEntity sword = Entity(result, "item", 1);
        Assert.Equal([1, 2], sword.Loaded.Select(row => row.Record));                  // record 0 and 3 do not exist
        Assert.Equal([1], Loaded(sword, "loot", 1, "skill_id"));                       // the seed, not LOOT's later 99
        Assert.Equal([3, 4], Loaded(sword, "loot", 1, "count"));
        Assert.Equal([5, 0], Loaded(sword, "loot", 2, "count"));
        Assert.Equal([2], Loaded(Entity(result, "item", 2), "loot", 1, "skill_id"));
        Assert.Empty(Entity(result, "item", 3).Loaded);
        // A loaded reference is the key entity's: <table>.<field>, its index the record times the width.
        Assert.Contains(new ExportRef("loot.skill_id", 1, "skill", 1), sword.Refs);
        Assert.Contains(new ExportRef("loot.skill_id", 2, "skill", 1), sword.Refs);
        Assert.Equal(1, result.Report.MissingRefs[("item", "loot.skill_id", "skill")]);  // item 2's skill 2
        ExportLoadedTable table = result.Kinds.Single(kind => kind.Declaration.Id == "item").Loaded.Single();
        Assert.Equal(("loot", "field_loot", "field"), (table.Declaration.Name, table.Table.Name, table.Scope));
    }

    [Fact]
    public void ALoadedTablesCellsAreTheKeysWithTheirWriters()
    {
        ExportEntity sword = Entity(GameDataExporter.Run(WithDeclarations(Game(), WithLoader())), "item", 1);
        ExportModderCell count = Assert.Single(sword.Cells, cell => cell.Field == "loot.count" && cell.Index == 2 * 2);
        Assert.Equal((LootCount + 2 * 2, "5", "DROPSET.BIN"), (count.Address, count.Raw, count.Writer));
        Assert.Contains(sword.Cells, cell => cell.Field == "loot.skill_id" && cell.Index == 1 && cell.Raw == "1");
        Assert.DoesNotContain(sword.Cells, cell => cell.Field == "loot.skill_id" && cell.Index == 0);   // no record 0
    }

    [Fact]
    public void ALoaderScriptsOwnWriteIsRefused()
    {
        ExportResult result = GameDataExporter.Run(Game(Faults.LoaderScriptWrites));
        var refused = result.Report.Refusals.Where(refusal => refusal.Field == "loot.count").ToList();
        Assert.Equal([1, 2], refused.Select(refusal => refusal.Id));                 // one in each item's run
        Assert.All(refused, refusal => Assert.Equal(("item", 2 * 2 + 1, LootCount + 5),
                                                    (refusal.Kind, refusal.Index, refusal.Address)));
        Assert.Equal([5, null], Loaded(Entity(result, "item", 1), "loot", 2, "count"));
        ExportEntity sword = Entity(result, "item", 1);
        Assert.Contains(sword.Cells, cell => cell.Field == "loot.count" && cell.Index == 5 && cell.Raw == null
                                             && cell.Writer == "LOOT.BIN");
        // The snippet leaves the refused element out.
        Assert.Contains("[field.1.loot.2]\nskill_id = 1\ncount = {0 = 5}\n", sword.Snippet, StringComparison.Ordinal);
    }

    // The loot of skill 1 alone, with the skill's category beside it (export plan §5): a where and a join.
    private static readonly string Joined = Loader.Replace(
        "\"fields\": [\"skill_id\", \"count\"]}",
        "\"fields\": [\"skill_id\", \"count\", \"skill.category\"], \"where\": {\"skill_id\": [1]}, "
        + "\"joins\": {\"skill\": \"skill_id\"}}", StringComparison.Ordinal);

    [Fact]
    public void ALoadedRowKeepsTheRecordsItsWhereNamesAndTheRecordItsJoinReaches()
    {
        ExportResult result = GameDataExporter.Run(WithDeclarations(Game(), WithLoader(Joined)));
        Assert.Empty(result.Report.Refusals);
        ExportEntity sword = Entity(result, "item", 1);
        // Both of item 1's loot records grant skill 1; item 2's record 1 grants skill 2, so its record 2 alone is a row.
        Assert.Equal([1, 2], sword.Loaded.Select(row => row.Record));
        Assert.Equal([2], Entity(result, "item", 2).Loaded.Select(row => row.Record));
        // The row's skill_id names skill 1, whose category is 1; the joined field is written table.field.
        Assert.Equal([1], Loaded(sword, "loot", 1, "skill.category"));
        Assert.Contains(new ExportRef("loot.skill_id", 1, "skill", 1), sword.Refs);   // the join's field still links
        ExportLoadedTable table = result.Kinds[0].Loaded.Single();
        Assert.Equal("skill", table.TableOf(table.Published.Single(field => field.Name == "skill.category")).Name);
        // A data file edits the scope's table: the snippet has no joined field.
        Assert.EndsWith("[field.1.loot.2]\nskill_id = 1\ncount = {0 = 5, 1 = 0}\n", sword.Snippet, StringComparison.Ordinal);

        string root = Path.Combine(Path.GetTempPath(), "ome-export-" + Guid.NewGuid().ToString("N"));
        try
        {
            ExportWriter.Write(result, root, "abcdef012345", Manifest);
            string? Scalar(string folder, string sql)
            {
                using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
                {
                    DataSource = Path.Combine(folder, "abcdef012345.sqlite"), Mode = SqliteOpenMode.ReadOnly, Pooling = false,
                }.ToString());
                connection.Open();
                using SqliteCommand command = connection.CreateCommand();
                command.CommandText = sql;
                return command.ExecuteScalar()?.ToString();
            }
            Assert.Equal("1", Scalar(root, "SELECT \"skill.category\" FROM \"item.loot\" WHERE id = 1 AND record = 1"));
            Assert.Equal("int|1", Scalar(root, "SELECT type || '|' || published FROM loaded_field "
                                                  + "WHERE loaded = 'loot' AND name = 'skill.category'"));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }

        // A join must go through a field that refers to its table.
        string wrong = Joined.Replace("\"joins\": {\"skill\": \"skill_id\"}", "\"joins\": {\"skill\": \"count\"}",
                                      StringComparison.Ordinal);
        ExportFailure failure = Assert.Throws<ExportFailure>(() => GameDataExporter.Run(WithDeclarations(Game(), WithLoader(wrong))));
        Assert.Contains("joins skill through count, which is not one cell that refers to skill", failure.Message,
                        StringComparison.Ordinal);
    }

    [Fact]
    public void AReferenceWithBasesNamesAListedValuesBasePlusItsPlus()
    {
        // With bases {1: 0, 2: 10} (export plan §5): item 1 (skill 1, category 2) names 0 + 2 = skill 2, item 2 (skill
        // 2, category 3) 10 + 3 = skill 13, and item 3's -1, which bases does not list, none. A Kamidori map object of
        // type 32 gathers from dig table 0 plus its payload.
        ExportInputs game = Game();
        ExportResult result = GameDataExporter.Run(new ExportInputs
        {
            GameId = game.GameId, Table = game.Table, Build = game.Build, Scripts = game.Scripts,
            ScriptByName = game.ScriptByName, BootStop = game.BootStop,
            Registry = Registry(skillKey: new RefKey(1, "category") { Bases = new Dictionary<long, long> { [1] = 0, [2] = 10 } }),
            Declarations = game.Declarations, Pictures = game.Pictures, BuildInfo = game.BuildInfo,
        });
        Assert.Equal([new ExportRef("granted_skill", 0, "skill", 2)], Entity(result, "item", 1).Refs);
        Assert.Equal([new ExportRef("granted_skill", 0, "skill", 13)], Entity(result, "item", 2).Refs);
        Assert.Empty(Entity(result, "item", 3).Refs);
        Assert.Equal([0L], Entity(result, "item", 3).KeyedRefs["granted_skill"]);
    }

    // The loot whose skill is on skill 1's side (export plan §5): a part, which the site shows apart.
    private static readonly string Parted = Loader.Replace(
        "\"fields\": [\"skill_id\", \"count\"]}",
        "\"fields\": [\"skill_id\", \"count\"], \"parts\": [{\"name\": \"kin\", \"field\": \"skill_id\", "
        + "\"global\": \"loot_sides\", \"same_as\": 1, \"note\": \"On skill 1's side.\"}]}", StringComparison.Ordinal);

    [Fact]
    public void ARowIsInThePartWhoseGlobalAtItsFieldIsNotZeroAndEqualsTheNamedCell()
    {
        ExportResult result = GameDataExporter.Run(WithDeclarations(Game(), WithLoader(Parted)));
        Assert.Empty(result.Report.Refusals);
        ExportLoadedRow Row(int item, int record)
            => Entity(result, "item", item).Loaded.Single(row => row.Table == "loot" && row.Record == record);
        Assert.Equal(("kin", "kin"), (Row(1, 1).Part, Row(1, 2).Part));              // skill 1, side 5 as cell 1's
        Assert.Equal((null, "kin"), (Row(2, 1).Part, Row(2, 2).Part));               // skill 2 is on side 6
        PartDeclaration part = Assert.Single(result.Kinds[0].Loaded.Single().Declaration.Parts);
        Assert.Equal(("kin", "skill_id", "loot_sides", 1, "On skill 1's side."),
                     (part.Name, part.Field, part.Global, part.SameAs, part.Note));

        // A side is compared as a published value is: LOOT's own write is refused and decides nothing.
        ExportResult faulty = GameDataExporter.Run(WithDeclarations(Game(Faults.LoaderScriptWrites), WithLoader(Parted)));
        Refusal side = Assert.Single(faulty.Report.Refusals, refusal => refusal.Field == "loot.kin:loot_sides");
        Assert.Equal((2, 2, LootSides + 2), (side.Id, side.Index, side.Address));
        Assert.Null(Entity(faulty, "item", 2).Loaded.Single(row => row.Record == 1).Part);

        string root = Path.Combine(Path.GetTempPath(), "ome-export-" + Guid.NewGuid().ToString("N"));
        try
        {
            ExportWriter.Write(result, root, "abcdef012345", Manifest);
            using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
            {
                DataSource = Path.Combine(root, "abcdef012345.sqlite"), Mode = SqliteOpenMode.ReadOnly, Pooling = false,
            }.ToString());
            connection.Open();
            string? Scalar(string sql)
            {
                using SqliteCommand command = connection.CreateCommand();
                command.CommandText = sql;
                return command.ExecuteScalar()?.ToString();
            }
            Assert.Equal("kin|0|skill_id|loot_sides|1|On skill 1's side.", Scalar(
                "SELECT name || '|' || position || '|' || field || '|' || global || '|' || same_as || '|' || note "
                + "FROM loaded_part WHERE kind = 'item' AND loaded = 'loot'"));
            Assert.Equal("1:1:kin,1:2:kin,2:1:-,2:2:kin", Scalar(
                "SELECT group_concat(id || ':' || record || ':' || ifnull(part, '-'), ',') FROM "
                + "(SELECT * FROM \"item.loot\" ORDER BY id, record)"));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    // A loader with no scope (export plan §5): items 1 and 2, three steps each, its seeds the key and the key's row of
    // the rule state, every rule 1 and the step's 2; RULES is its own builder.
    private const string Stepped = """
        "loaders": [{"scope": null, "keys": "item", "key_range": [1, 2], "steps": 3, "skip": {}, "script": "RULES",
                     "seed": [{"global": "current_item_id", "index": [], "value": "key"},
                              {"global": "rule_state", "index": ["key", "*"], "value": 1},
                              {"global": "rule_state", "index": ["key", "step"], "value": 2}],
                     "read_at": "testgame.rules.ready", "builders": ["RULES"],
                     "tables": {"bonus": {"table": "bonus", "exists": "level", "fields": ["level", "stats"],
                                          "text": ["title"], "literals": [], "notes": {"level": "The key."}}}}]
        """;

    private static ExportResult RunStepped(string loader = Stepped, string? declarations = null)
    {
        ExportInputs game = Game();
        Script rules = game.ScriptByName("RULES")!;
        return GameDataExporter.Run(WithDeclarations(game, declarations ?? WithLoader(loader),
            stopOf: point => point == "testgame.rules.ready"
                ? new RunStop("RULES", rules.Instructions[^1].Offset)
                : game.StopOf!(point)));
    }

    [Fact]
    public void ALoaderWithStepsRunsEachKeyOnceAStepWithItsSeedsAndReadsAOneRecordTable()
    {
        ExportResult result = RunStepped();
        Assert.Equal(6, result.Report.LoaderRuns);                                     // items 1 and 2, three steps
        Assert.Empty(result.Report.LoaderHalts);
        Assert.Empty(result.Report.Refusals);
        ExportEntity sword = Entity(result, "item", 1);
        Assert.Equal([0, 1, 2], sword.Loaded.Select(row => row.Record));               // step k is record k
        Assert.Equal([2, 1, 1], Loaded(sword, "bonus", 0, "stats"));                   // "*" then the step's cell
        Assert.Equal([1, 2, 1], Loaded(sword, "bonus", 1, "stats"));
        Assert.Equal([1, 1, 2], Loaded(sword, "bonus", 2, "stats"));
        Assert.Equal([1], Loaded(sword, "bonus", 2, "level"));
        ExportEntity shield = Entity(result, "item", 2);
        Assert.Equal([0, 0, 0], Loaded(shield, "bonus", 0, "stats"));                  // its own row, not key 1's
        Assert.Equal([2], Loaded(shield, "bonus", 0, "level"));
        Assert.Empty(Entity(result, "item", 3).Loaded);                                // outside the key range
        // The text is the entity's, as table.field by row; its cell shows the text; no data file reaches the table.
        Assert.Equal("称号", Text(sword, "bonus.title", 2));
        Assert.Contains(sword.Cells, cell => cell.Field == "bonus.title" && cell.Index == 1 && cell.Raw == "称号");
        Assert.DoesNotContain(".bonus.", sword.Snippet ?? "", StringComparison.Ordinal);
        ExportLoadedTable table = result.Kinds.Single(kind => kind.Declaration.Id == "item").Loaded.Single();
        Assert.Null(table.Scope);
        Assert.Equal(["title"], table.Text.Select(field => field.Name));
    }

    [Fact]
    public void ALiteralGivesARecordsTextFromAScriptsString()
    {
        ExportResult result = RunStepped(Stepped.Replace("\"literals\": []",
            "\"literals\": [{\"record\": 0, \"field\": \"title\", \"script\": \"AIM\", \"offset\": 0}]",
            StringComparison.Ordinal));
        ExportEntity sword = Entity(result, "item", 1);
        Assert.Equal(["固定", "固定", "固定"], Enumerable.Range(0, 3).Select(row => Text(sword, "bonus.title", row)));
        ExportFailure failure = Assert.Throws<ExportFailure>(() => RunStepped(Stepped.Replace("\"literals\": []",
            "\"literals\": [{\"record\": 0, \"field\": \"title\", \"script\": \"AIM\", \"offset\": 1}]",
            StringComparison.Ordinal)));
        Assert.Contains("AIM@0x1 starts no instruction", failure.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ASheetPictureIsTheCellItsFieldNumbers()
    {
        JsonNode declarations = JsonNode.Parse(Declarations)!;
        declarations["kinds"]![0]!["pictures"]!["badge"] = JsonNode.Parse("""
            {"field": "category", "sheet": "SHEET.AGF", "first": 1, "columns": 3, "origin": [0, 10], "step": [8, 8],
             "size": [7, 7]}
            """);
        ExportResult result = GameDataExporter.Run(WithDeclarations(Game(), declarations.ToJsonString()));
        string? Badge(int id) => Entity(result, "item", id).Pictures.SingleOrDefault(p => p.Name == "badge").Picture?.Name;
        Assert.Equal("SHEET-8-10", Badge(1));                                          // category 2: the second cell
        Assert.Equal("SHEET-16-10", Badge(2));                                         // category 3: the third
        Assert.Null(Badge(3));                                                         // category 4: the next row, off it
        Assert.Contains(("item", 3, "badge", 4L), result.Report.UnresolvedPictures);
        Assert.Equal(7, Entity(result, "item", 1).Pictures.Single(p => p.Name == "badge").Picture.Width);
    }

    [Fact]
    public void ASheetWithALastNumberAndAKeyCutsOnlyItsOwnNumbersWithItsKey()
    {
        // Himegari's icons (export plan §5): numbers past a sheet's last are another sheet's, so they are no picture of
        // this one and no unresolved picture; the sheet's key asks for its cells keyed.
        JsonNode declarations = JsonNode.Parse(Declarations)!;
        declarations["kinds"]![0]!["pictures"]!["badge"] = JsonNode.Parse("""
            {"field": "category", "sheet": "SHEET.AGF", "first": 1, "columns": 3, "origin": [0, 10], "step": [8, 8],
             "size": [7, 7], "key": 65280, "last": 2}
            """);
        ExportResult result = GameDataExporter.Run(WithDeclarations(Game(), declarations.ToJsonString()));
        string? Badge(int id) => Entity(result, "item", id).Pictures.SingleOrDefault(p => p.Name == "badge").Picture?.Name;
        Assert.Equal("SHEET-8-10-key-00ff00", Badge(1));                               // category 2
        Assert.Null(Badge(2));                                                         // category 3: past the last
        Assert.DoesNotContain(result.Report.UnresolvedPictures, unresolved => unresolved.Item3 == "badge");
    }

    [Fact]
    public void AKeyedPictureIsAskedForWithItsColourKey()
    {
        // { field, key }: the colour the game draws transparent (Himegari's map sprites); a plain field asks for the
        // picture as it is.
        JsonNode declarations = JsonNode.Parse(Declarations)!;
        declarations["kinds"]![0]!["pictures"]!["icon"] = JsonNode.Parse("""{"field": "icon", "key": 65280}""");
        ExportResult result = GameDataExporter.Run(WithDeclarations(Game(), declarations.ToJsonString()));
        (string name, ExportPicture picture) = Assert.Single(Entity(result, "item", 1).Pictures);
        Assert.Equal(("icon", "ICON077-key-00ff00"), (name, picture.Name));
        Assert.Equal("ICON077", Assert.Single(GameDataExporter.Run(Game()).Kinds[0].Entities[0].Pictures).Picture.Name);
    }

    [Fact]
    public void TheDatabaseHoldsNotesGroupLabelsTextBlocksAndLoadedText()
    {
        JsonNode declarations = JsonNode.Parse(WithLoader(Stepped))!;
        declarations["kinds"]![0]!["groups"] = JsonNode.Parse(
            """[{"name": "prices", "fields": ["price", "category"], "labels": "bonus.title"}]""");
        declarations["kinds"]![0]!["notes"] = JsonNode.Parse("""{"category": "1 a sword.", "prices": "Buy and sell."}""");
        declarations["kinds"]![0]!["text_blocks"] = JsonNode.Parse(
            """{"short_description": [{"name": "first", "first": 0, "last": 0}, {"name": "rest", "first": 1, "last": 3}]}""");
        ExportResult result = RunStepped(declarations: declarations.ToJsonString());
        Assert.Equal("bonus.title", result.Kinds[0].Declaration.Groups.Single().Labels);
        string root = Path.Combine(Path.GetTempPath(), "ome-export-" + Guid.NewGuid().ToString("N"));
        try
        {
            ExportWriter.Write(result, root, "abcdef012345", Manifest);
            using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
            {
                DataSource = Path.Combine(root, "abcdef012345.sqlite"), Mode = SqliteOpenMode.ReadOnly, Pooling = false,
            }.ToString());
            connection.Open();
            string? Scalar(string sql)
            {
                using SqliteCommand command = connection.CreateCommand();
                command.CommandText = sql;
                return command.ExecuteScalar()?.ToString();
            }
            Assert.Equal("prices=bonus.title", Scalar("SELECT name || '=' || labels FROM group_label WHERE kind = 'item'"));
            Assert.Equal("bonus.level:The key.,category:1 a sword.,prices:Buy and sell.", Scalar(
                "SELECT group_concat(name || ':' || text, ',') FROM (SELECT * FROM note WHERE kind = 'item' ORDER BY name)"));
            Assert.Equal("title", Scalar("SELECT name FROM loaded_field WHERE kind = 'item' AND loaded = 'bonus' AND text = 1"));
            Assert.Equal("short_description:0:first:0-0,short_description:1:rest:1-3", Scalar(
                "SELECT group_concat(field || ':' || position || ':' || name || ':' || first || '-' || last, ',') FROM "
                + "(SELECT * FROM text_block WHERE kind = 'item' ORDER BY field, position)"));
            Assert.Equal("1", Scalar("SELECT scope IS NULL FROM loaded WHERE kind = 'item' AND name = 'bonus'"));
            Assert.Equal("3", Scalar("SELECT count(*) FROM text WHERE kind = 'item' AND id = 1 AND field = 'bonus.title'"));
            Assert.Equal("[1,2,1]", Scalar("SELECT stats FROM \"item.bonus\" WHERE id = 1 AND record = 1"));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    // ------------------------------------------------------------------------------------------------- script_use (§4.3)

    [Fact]
    public void ScriptUseCountsEveryScriptOfTheInstallThatTouchesAKindsTables()
    {
        ExportInputs game = Game();
        ExportResult result = GameDataExporter.Run(WithDeclarations(game, WithLoader(), index: IndexOf(game)));
        var item = result.Kinds.Single(kind => kind.Declaration.Id == "item").ScriptUse
            .ToDictionary(use => use.Script, use => use.Use);
        // The builders' writes, the scene's that no run reached, a substring's read and write, ITMES's index of the
        // names, and the loader scripts' writes to the loaded table, which count for the key kind.
        Assert.Equal(new GlobalUse(0, 12, 0), item["ITINIT.BIN"]);
        Assert.Equal(new GlobalUse(0, 4, 0), item["$1$ITINIT.BIN"]);
        Assert.Equal(new GlobalUse(0, 1, 0), item["SCENE.BIN"]);
        Assert.Equal(new GlobalUse(1, 1, 0), item["$3$ITINIT.BIN"]);
        Assert.Equal(new GlobalUse(0, 0, 1), item["ITMES.BIN"]);
        Assert.Equal(new GlobalUse(0, 5, 0), item["DROPSET.BIN"]);
        Assert.Equal(new GlobalUse(0, 1, 0), item["LOOT.BIN"]);
        Assert.DoesNotContain("SKINIT.BIN", item.Keys);
        Assert.Equal(["SKINIT.BIN"], result.Kinds.Single(kind => kind.Declaration.Id == "skill").ScriptUse
                                         .Select(use => use.Script));
        Assert.Empty(GameDataExporter.Run(WithDeclarations(game, WithLoader())).Kinds.SelectMany(kind => kind.ScriptUse));
    }

    [Fact]
    public void AnIntegerCellTheBuildersWroteThatNoGlobalNamesIsReported()
    {
        // The registry's names without item_icon: its two cells the builders wrote are left.
        var names = new GlobalNames(
        [
            new NamedGlobal("item_category", GlobalBank.Integer, ItemCategory, 4),
            new NamedGlobal("item_prices", GlobalBank.Integer, ItemPrices, 8),
            new NamedGlobal("item_handler", GlobalBank.Integer, ItemHandler, 4),
            new NamedGlobal("item_granted_skill", GlobalBank.Integer, ItemSkill, 4),
            new NamedGlobal("skill_category", GlobalBank.Integer, SkillCategory, 3),
        ]);
        ExportInputs game = Game();
        ExportResult result = GameDataExporter.Run(new ExportInputs
        {
            GameId = game.GameId, Table = game.Table, Build = game.Build, Scripts = game.Scripts,
            ScriptByName = game.ScriptByName, BootStop = game.BootStop, StopOf = game.StopOf,
            Declarations = game.Declarations, Registry = game.Registry, Pictures = game.Pictures, Names = names,
        });
        Assert.Equal(["item"], result.Report.UnnamedCells.Keys);
        Assert.Equal([ItemIcon + 1, ItemIcon + 3], result.Report.UnnamedCells["item"]);
        Assert.Equal(0, result.Report.ExitCode);
        Assert.Empty(GameDataExporter.Run(game).Report.UnnamedCells);
    }

    // ------------------------------------------------------------------------------------------------- snippets (§6.6)

    [Fact]
    public void ASnippetSetsWhatTheSiteShowsOfARecordByTheRecordMapsNames()
    {
        ExportResult result = GameDataExporter.Run(Game());
        // The table's order; a row's columns by name where the registry names them, by number otherwise; the title, the
        // text and the picture, not the description's script.
        Assert.Equal("""
            [item.1]
            name = "剣"
            short_description = "よく切れる"
            category = 2
            price = {0 = 10, sell = 5}
            icon = 77
            granted_skill = 1

            """.ReplaceLineEndings("\n"), Entity(result, "item", 1).Snippet);
        Assert.Equal("[skill.1]\nname = \"斬撃\"\ncategory = 1\n", Entity(result, "skill", 1).Snippet);
        Assert.Empty(result.Report.SnippetProblems);
    }

    [Fact]
    public void ASnippetCarriesItsLoadedRowsUnderTheirScope()
    {
        string? snippet = Entity(GameDataExporter.Run(WithDeclarations(Game(), WithLoader())), "item", 1).Snippet;
        Assert.EndsWith("""
            granted_skill = 1

            [field.1.loot.1]
            skill_id = 1
            count = {0 = 3, 1 = 4}

            [field.1.loot.2]
            skill_id = 1
            count = {0 = 5, 1 = 0}

            """.ReplaceLineEndings("\n"), snippet, StringComparison.Ordinal);
    }

    [Fact]
    public void ARefusedValueIsLeftOutOfTheSnippet()
    {
        ExportResult result = GameDataExporter.Run(Game(Faults.ExcludedBuilderWrites));
        Assert.DoesNotContain("icon", Entity(result, "item", 3).Snippet, StringComparison.Ordinal);
    }

    /// <summary>A shape of the synthetic game's data kind with no <c>granted_skill</c>, which every item's snippet sets.
    /// </summary>
    private const string ShapeWithoutGrantedSkill = """
        {
          "$schema": "http://json-schema.org/draft-07/schema#",
          "description": "testgame.tables",
          "type": "object",
          "properties": {
            "item": { "type": "object", "x-ome-decimal-keys": { "minimum": 0, "maximum": 3 },
                      "additionalProperties": { "$ref": "#/definitions/record.item" } },
            "skill": { "type": "object" }
          },
          "additionalProperties": false,
          "definitions": {
            "record.item": {
              "type": "object",
              "properties": {
                "name": { "type": "string" }, "short_description": { "type": "string" },
                "category": { "type": "integer" }, "icon": { "type": "integer" },
                "price": { "type": "object", "properties": { "0": { "type": "integer" }, "sell": { "type": "integer" } },
                           "additionalProperties": false }
              },
              "additionalProperties": false
            }
          }
        }
        """;

    [Fact]
    public void ASnippetTheShapeRefusesIsReportedAndTheExitCodeIsOne()
    {
        Ome.Mods.Data.DataShapeRead shape = Ome.Mods.Data.DataShape.Read(ShapeWithoutGrantedSkill);
        Assert.True(shape.Shape != null, string.Join("\n", shape.Problems));
        ExportResult result = GameDataExporter.Run(WithDeclarations(Game(), Declarations, shape: shape.Shape));
        Assert.Equal([1, 2, 3], result.Report.SnippetProblems.Select(problem => problem.Id));
        Assert.All(result.Report.SnippetProblems, problem =>
        {
            Assert.Equal("item", problem.Kind);
            Assert.Contains("granted_skill", problem.Problem, StringComparison.Ordinal);
            Assert.StartsWith($"item/{problem.Id}.toml:", problem.Problem, StringComparison.Ordinal);
        });
        Assert.Equal(1, result.Report.ExitCode);
    }

    [Fact]
    public void ALoaderThatDoesNotReachItsPointIsReportedAndTheExportGoesOn()
    {
        ExportInputs game = Game();
        ExportResult result = GameDataExporter.Run(WithDeclarations(game, WithLoader(),
                                                                    _ => new RunStop("LOOT", 0x7777)));
        Assert.Equal(2, result.Report.LoaderHalts.Count);
        Assert.All(result.Report.LoaderHalts, halt => Assert.Equal("exit", halt.Halt));
        Assert.Empty(Entity(result, "item", 1).Loaded);
        Assert.Equal("剣", Text(Entity(result, "item", 1), "name"));
    }

    [Fact]
    public void ALoadedTableMayNotTakeTheNameOfAKeyField()
    {
        string clash = WithLoader().Replace("\"loot\": {", "\"category\": {", StringComparison.Ordinal);
        var failure = Assert.Throws<ExportFailure>(() => GameDataExporter.Run(WithDeclarations(Game(), clash)));
        Assert.Contains("category", failure.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheDatabaseHoldsTheLoadedTablesAndTheirRowsChangeTheKeysHash()
    {
        string root = Path.Combine(Path.GetTempPath(), "ome-export-" + Guid.NewGuid().ToString("N"));
        string plain = Path.Combine(Path.GetTempPath(), "ome-export-" + Guid.NewGuid().ToString("N"));
        try
        {
            ExportWriter.Write(GameDataExporter.Run(WithDeclarations(Game(), WithLoader(), index: IndexOf(Game()))), root,
                               "abcdef012345", Manifest);
            ExportWriter.Write(GameDataExporter.Run(Game()), plain, "abcdef012345", Manifest);
            string? Scalar(string folder, string sql)
            {
                using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
                {
                    DataSource = Path.Combine(folder, "abcdef012345.sqlite"), Mode = SqliteOpenMode.ReadOnly, Pooling = false,
                }.ToString());
                connection.Open();
                using SqliteCommand command = connection.CreateCommand();
                command.CommandText = sql;
                return command.ExecuteScalar()?.ToString();
            }

            Assert.Equal("field_loot|field|3|4", Scalar(root, "SELECT table_name || '|' || scope || '|' || records || '|' || rows "
                                                             + "FROM loaded WHERE kind = 'item' AND name = 'loot'"));
            Assert.Equal("skill|skill|1", Scalar(root, "SELECT ref_table || '|' || ref_kind || '|' || published FROM loaded_field "
                                                       + "WHERE kind = 'item' AND loaded = 'loot' AND name = 'skill_id'"));
            Assert.Equal("int[2]", Scalar(root, "SELECT type FROM loaded_field WHERE loaded = 'loot' AND name = 'count'"));
            Assert.Equal("[3,4]", Scalar(root, "SELECT count FROM \"item.loot\" WHERE id = 1 AND record = 1"));
            Assert.Equal("2", Scalar(root, "SELECT skill_id FROM \"item.loot\" WHERE id = 2 AND record = 1"));
            Assert.Equal("2", Scalar(root, "SELECT idx FROM ref WHERE field = 'loot.skill_id' AND from_id = 1 ORDER BY idx DESC"));
            Assert.Equal("1704|DROPSET.BIN|5", Scalar(root, "SELECT address || '|' || writer || '|' || raw FROM cell "
                                                         + "WHERE kind = 'item' AND id = 1 AND field = 'loot.count' AND idx = 4"));
            Assert.Equal("0|5|0", Scalar(root, "SELECT reads || '|' || writes || '|' || addresses FROM script_use "
                                               + "WHERE kind = 'item' AND script = 'DROPSET.BIN'"));
            Assert.Equal("0", Scalar(plain, "SELECT count(*) FROM script_use"));
            Assert.NotEqual(Scalar(plain, "SELECT content_hash FROM item WHERE id = 1"),
                            Scalar(root, "SELECT content_hash FROM item WHERE id = 1"));
            // An entity without loaded rows keeps the hash it has in an export without loaders.
            Assert.Equal(Scalar(plain, "SELECT content_hash FROM item WHERE id = 3"),
                         Scalar(root, "SELECT content_hash FROM item WHERE id = 3"));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            foreach (string folder in new[] { root, plain })
                if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void AKindMayNotTakeTheNameOfAnExportTable()
    {
        ExportInputs game = Game();
        string renamed = Declarations.Replace("\"id\": \"skill\"", "\"id\": \"text\"", StringComparison.Ordinal);
        var failure = Assert.Throws<ExportFailure>(() => GameDataExporter.Run(new ExportInputs
        {
            GameId = game.GameId, Table = game.Table, Build = game.Build, Scripts = game.Scripts,
            ScriptByName = game.ScriptByName, BootStop = game.BootStop, Declarations = ExportDeclarations.Parse(renamed),
            Registry = game.Registry,
        }));
        Assert.Contains("text", failure.Message, StringComparison.Ordinal);
    }
}
