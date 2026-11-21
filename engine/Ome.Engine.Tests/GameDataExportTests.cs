using System.Text;
using Microsoft.Data.Sqlite;
using Ome.Engine.Hosting;
using Ome.Engine.Model;
using Ome.Engine.Natives;
using Ome.Engine.Sys4;
using Ome.Engine.Vm;
using Ome.Export;
using Xunit;

namespace Ome.Engine.Tests;

using Script = Ome.Engine.Model.Script;

/// <summary>The game data exporter (game data export spec §5.5, §5.7, §6, §7) on a synthetic game: a boot whose
/// builders fill an item and a skill table, an append layer, a description script, and one variant per refusal.
/// </summary>
public class GameDataExportTests
{
    private static readonly OpcodeTable Table = OpcodeTableJson.Load(Paths.OpcodesJson);
    private const int OpExit = 0x2, OpCallScript = 0x3, OpMov = 0x55, OpSetString = 0x192, OpShowText = 0x6e,
                      OpFurigana = 0x196, OpEndLine = 0x6f;
    private static readonly int OpLookupArray = Table.ByLabel("lookup-array")!.Value;
    private static readonly int OpSubstring = Table.ByLabel("cp932-substring")!.Value;

    private static Operand I(long value) => new(0, value);
    private static Operand S(int index) => new(2, index);
    private static Operand G(int cell) => new(3, cell);
    private static Operand GS(int cell) => new(5, cell);

    // The synthetic registry: item (4 records) and skill (3 records). The skill's names and categories share their
    // addresses, in two banks.
    private const int ItemNames = 100, ItemShorts = 200, SkillNames = 1500, Scratch = 900;
    private const int ItemCategory = 1000, ItemPrices = 1100, ItemIcon = 1200, ItemHandler = 1300, ItemSkill = 1400,
                      SkillCategory = 1500, CurrentItem = 5000;

    private static RegistryView Registry() => new(
        [
            new TableLayout("item", 4,
                [("name", "item_names"), ("short_description", "item_short_descriptions"), ("category", "item_category"),
                 ("price", "item_prices"), ("icon", "item_icon"), ("handler_script_id", "item_handler"),
                 ("granted_skill", "item_granted_skill")],
                new Dictionary<string, string> { ["granted_skill"] = "skill", ["icon"] = "picture" }),
            new TableLayout("skill", 3, [("name", "skill_names"), ("category", "skill_category")],
                            new Dictionary<string, string>()),
        ],
        [
            Array("item_names", GlobalBank.String, ItemNames, 4),
            Array("item_short_descriptions", GlobalBank.String, ItemShorts, 4),
            Array("item_category", GlobalBank.Integer, ItemCategory, 4),
            new ArrayLayout("item_prices", GlobalBank.Integer, ItemPrices, [4, 2],
                            new Dictionary<int, string> { [0] = "buy", [1] = "sell" }),
            Array("item_icon", GlobalBank.Integer, ItemIcon, 4),
            Array("item_handler", GlobalBank.Integer, ItemHandler, 4),
            Array("item_granted_skill", GlobalBank.Integer, ItemSkill, 4),
            Array("skill_names", GlobalBank.String, SkillNames, 3),
            Array("skill_category", GlobalBank.Integer, SkillCategory, 3),
            new ArrayLayout("current_item_id", GlobalBank.Integer, CurrentItem, [], new Dictionary<int, string>()),
        ]);

    private static ArrayLayout Array(string name, GlobalBank bank, int address, int records)
        => new(name, bank, address, [records], new Dictionary<int, string>());

    private const string Declarations = """
        {"profile": "testgame", "source_sha256": "0123abcd",
         "export": {"boot": {"script": "BOOT", "read_at": "testgame.boot.ready"},
                    "builders": ["BOOT", "*INIT"], "excluded_builders": ["CGINIT"], "text_scripts": ["ITMES"],
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

    [Flags]
    private enum Faults
    {
        None = 0,
        SceneWritesTheTable = 1,     // a scene writes item 2's category, and a builder copies the scene's line into it
        ExcludedBuilderWrites = 2,   // CGINIT writes item 3's icon
        StringWithoutOrigin = 4,     // a builder cuts item 1's name into item 3's short description (no origin)
        HandlerIsNoTextScript = 8,   // item 2's handler is OTHER, a scene
    }

    private sealed class Pictures : IPictureSource
    {
        public ExportPicture? Resolve(long packedId) => packedId is 77 or 79
            ? new ExportPicture($"ICON{packedId:000}", Encoding.ASCII.GetBytes($"png {packedId}"), 32, 16)
            : null;
    }

    private static Script Asm(string name, IReadOnlyList<(int, Operand[])> code, params string[] strings)
        => ScriptAssembler.Assemble(Table, name, code, strings);

    private static ExportInputs Game(Faults faults = Faults.None)
    {
        var byId = new Dictionary<long, Script>
        {
            [1] = Asm("ITINIT.BIN",
            [
                (OpSetString, [GS(ItemNames + 1), S(0)]), (OpSetString, [GS(ItemShorts + 1), S(1)]),
                (OpMov, [G(ItemCategory + 1), I(2)]), (OpMov, [G(ItemPrices + 2), I(10)]), (OpMov, [G(ItemPrices + 3), I(5)]),
                (OpMov, [G(ItemIcon + 1), I(77)]), (OpMov, [G(ItemHandler + 1), I(9)]), (OpMov, [G(ItemSkill + 1), I(1)]),
                (OpSetString, [GS(ItemNames + 2), S(2)]), (OpMov, [G(ItemCategory + 2), I(3)]),
                (OpMov, [G(ItemSkill + 2), I(2)]),
                (OpMov, [G(ItemHandler + 2), I(faults.HasFlag(Faults.HandlerIsNoTextScript) ? 10 : 0)]),
                (OpExit, []),
            ], "剣", "よく切れる", "盾"),
            [2] = Asm("$1$ITINIT.BIN",
                [(OpSetString, [GS(ItemNames + 3), S(0)]), (OpMov, [G(ItemCategory + 3), I(4)]),
                 (OpMov, [G(ItemIcon + 3), I(78)]), (OpExit, [])], "槍"),
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
        };
        var calls = new List<(int, Operand[])> { (OpCallScript, [I(1)]), (OpCallScript, [I(2)]), (OpCallScript, [I(3)]) };
        if (faults.HasFlag(Faults.SceneWritesTheTable)) calls.AddRange([(OpCallScript, [I(4)]), (OpCallScript, [I(5)])]);
        if (faults.HasFlag(Faults.ExcludedBuilderWrites)) calls.Add((OpCallScript, [I(6)]));
        if (faults.HasFlag(Faults.StringWithoutOrigin)) calls.Add((OpCallScript, [I(7)]));
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
            Declarations = ExportDeclarations.Parse(Declarations),
            Registry = Registry(),
            Pictures = new Pictures(),
            BuildInfo = [("revision", "abcdef012345"), ("title", "テスト")],
        };
    }

    private static ExportEntity Entity(ExportResult result, string kind, int id)
        => result.Kinds.Single(k => k.Declaration.Id == kind).Entities.Single(entity => entity.Id == id);

    private static long?[] Values(ExportEntity entity, string field) => entity.Values.Single(value => value.Field == field).Values;

    private static string? Text(ExportEntity entity, string field, int index = 0)
        => entity.Texts.SingleOrDefault(text => text.Field == field && text.Index == index)?.Value;

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
            Assert.Equal("icon", Scalar("SELECT picture FROM field WHERE kind = 'item' AND name = 'icon'"));
            Assert.Equal("skill|skill", Scalar("SELECT ref_table || '|' || ref_kind FROM field WHERE name = 'granted_skill'"));
            Assert.Equal("description", Scalar("SELECT type FROM field WHERE name = 'description'"));
            Assert.Equal("0", Scalar("SELECT count(*) FROM relation"));
            Assert.Equal(64, Scalar("SELECT content_hash FROM item WHERE id = 1")!.Length);
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
              "G:0x40": {"address": "0x40", "bank": "G", "name": "current_unit", "type": "int"}},
             "tables": [{"name": "unit", "records": 3, "fields": {"graphic": "unit_graphic", "name": "unit_names"},
                         "refs": {"graphic": "picture"}}]}
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
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void DeclarationsWithLoadersAreRefusedUntilTheyAreExported()
    {
        string withLoader = Declarations.Replace("\"loaders\": []", "\"loaders\": [{\"scope\": \"stage\"}]", StringComparison.Ordinal);
        var error = Assert.Throws<InvalidDataException>(() => ExportDeclarations.Parse(withLoader));
        Assert.Contains("loaders", error.Message, StringComparison.Ordinal);
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
