using System.Text.Json;
using Ome.Engine.Model;
using Ome.Engine.Natives;
using Ome.Engine.Sys4;
using Ome.Export;
using Ome.Script.Abi;
using Ome.Script.Patch;
using Xunit;

namespace Ome.Engine.Tests;

using Script = Ome.Engine.Model.Script;

/// <summary>
/// The script index (game data export spec §8.2, plan §4.3) on synthetic scripts: what it counts of each operand by the
/// operand contracts, its calls and waits, the narrowest named global, and the JSON <c>Ome.Cli script-index</c> writes.
/// The export's <c>script_use</c> from it: GameDataExportTests.
/// </summary>
public sealed class ScriptIndexTests
{
    private static readonly OpcodeTable Table = OpcodeTableJson.Load(Paths.OpcodesJson);
    private static readonly OpcodeContracts Contracts = OpcodeContracts.Load(Paths.OpcodesJson);
    private const int OpExit = 0x2, OpCallScript = 0x3, OpMov = 0x55, OpShowText = 0x6e;
    private static readonly int OpLookupArray = Table.ByLabel("lookup-array")!.Value;
    private static readonly int OpPreload = Table.ByLabel("preload-script-slot")!.Value;

    private static Operand I(long value) => new(0, value);
    private static Operand S(int index) => new(2, index);
    private static Operand G(int cell) => new(3, cell);
    private static Operand GS(int cell) => new(5, cell);
    private static Operand Local(int slot) => new(9, slot);

    private const int Prices = 100, Tax = 105, Flags = 200, Cursor = 300, Line = 400;

    /// <summary>A record array of ten with one cell inside it named apart, flags of no known length, a scalar, and the
    /// string bank's line.</summary>
    private static GlobalNames Names() => new(
    [
        new NamedGlobal("prices", GlobalBank.Integer, Prices, 10),
        new NamedGlobal("tax", GlobalBank.Integer, Tax, 1),
        new NamedGlobal("flags", GlobalBank.Integer, Flags, null),
        new NamedGlobal("cursor", GlobalBank.Integer, Cursor, 1),
        new NamedGlobal("line", GlobalBank.String, Line, 1),
    ]);

    private static IndexedScript Indexed(string name, long id, IReadOnlyList<(int, Operand[])> code, params string[] strings)
        => new(name, id, "sha256:" + new string((char)('a' + id), 64), ScriptAssembler.Assemble(Table, name, code, strings));

    private static ScriptIndex Index() => ScriptIndex.Build(
    [
        Indexed("SHOP.BIN", 1,
        [
            (OpMov, [G(Prices + 2), I(10)]),                    // a write
            (OpMov, [G(Cursor), G(Prices + 2)]),                // a write and a read
            (OpLookupArray, [Local(0), G(Prices), G(Cursor)]),  // an index and a read; the bound local is no global
            (OpMov, [G(Tax), I(5)]),                            // inside prices, but tax is narrower
            (OpMov, [G(Flags + 40), I(1)]),                     // flags reaches to cursor
            (OpMov, [G(Cursor + 1), I(1)]),                     // past cursor: no name
            (OpShowText, [I(0), S(0)]), (OpShowText, [I(0), GS(Line)]),
            (OpCallScript, [I(2)]), (OpCallScript, [I(2)]), (OpCallScript, [I(99)]), (OpCallScript, [G(Cursor)]),
            (OpPreload, [I(2), I(29)]),
            (OpExit, []),
        ], "いらっしゃい"),
        Indexed("PAY.BIN", 2, [(OpMov, [G(Prices + 3), G(Prices + 4)]), (OpExit, [])]),
    ], Table, (opcode, operand) => Contracts.Access(opcode, operand, Table.AbiId));

    [Fact]
    public void EachOperandOfAGlobalBankCountsAsItsContractSays()
    {
        ScriptIndexEntry shop = Index().Entries[0];
        Assert.Equal(new GlobalUse(1, 1, 0), shop.Uses[new GlobalCell(GlobalBank.Integer, Prices + 2)]);
        Assert.Equal(new GlobalUse(0, 0, 1), shop.Uses[new GlobalCell(GlobalBank.Integer, Prices)]);
        Assert.Equal(new GlobalUse(2, 1, 0), shop.Uses[new GlobalCell(GlobalBank.Integer, Cursor)]);   // the computed call
        Assert.Equal(new GlobalUse(1, 0, 0), shop.Uses[new GlobalCell(GlobalBank.String, Line)]);
        Assert.Equal(7, shop.Uses.Count);
        Assert.DoesNotContain(shop.Uses.Keys, cell => cell.Address == 0);   // the local and the immediates
    }

    [Fact]
    public void CallsArePerConstantIdAndAComputedIdIsCounted()
    {
        ScriptIndexEntry shop = Index().Entries[0];
        Assert.Equal(new Dictionary<long, int> { [2] = 2, [99] = 1 }, shop.Calls);
        Assert.Equal(1, shop.ComputedCalls);
        Assert.Equal(new Dictionary<long, int> { [2] = 1 }, shop.Preloads);
        Assert.Equal(new Dictionary<string, int> { ["show-text"] = 2 }, shop.Waits);
    }

    [Fact]
    public void ACellIsNamedForTheNarrowestGlobalThatHoldsIt()
    {
        GlobalNames names = Names();
        string? Name(int address) => names.NameOf(new GlobalCell(GlobalBank.Integer, address));
        Assert.Equal("prices", Name(Prices + 4));
        Assert.Equal("tax", Name(Tax));
        Assert.Equal("flags", Name(Cursor - 1));   // no length: to the next named global
        Assert.Equal("cursor", Name(Cursor));
        Assert.Null(Name(Cursor + 1));
        Assert.Null(Name(Prices - 1));
        Assert.Null(names.NameOf(new GlobalCell(GlobalBank.String, Prices)));   // another bank
    }

    [Theory]
    [InlineData("int", 1)]
    [InlineData("string[2000]", 2000)]
    [InlineData("int[2000][3]", 6000)]
    [InlineData("int[1000x50]", 50000)]
    [InlineData("int[]", null)]
    [InlineData("int[unit][30]", null)]
    [InlineData("pointer", null)]
    public void ARegistryTypeGivesItsLength(string type, int? extent) => Assert.Equal(extent, GlobalNames.ExtentOf(type));

    [Fact]
    public void TheIndexNamesItsCallsCallersGlobalsAndPatchPoints()
    {
        var points = new OmsPatchPoints(
        [
            new OmsPatchPoint("test.shop.open", "SHOP", null,
                              [new OmsPatchPointSite("sha256:" + new string('b', 64), 0x10),
                               new OmsPatchPointSite("sha256:other", 0x20)]),
            new OmsPatchPoint("test.pay.done", "PAY", null, [new OmsPatchPointSite("sha256:other", 0x4)]),
        ]);
        string text = Index().ToJson("testgame", Names(), points, id => id switch { 1 => "SHOP.BIN", 2 => "PAY.BIN", _ => null });
        Assert.EndsWith("}\n", text, StringComparison.Ordinal);
        Assert.DoesNotContain("いらっしゃい", text, StringComparison.Ordinal);   // no text of the game
        using JsonDocument document = JsonDocument.Parse(text);
        JsonElement root = document.RootElement;
        Assert.Equal(ScriptIndex.FormatVersion, root.GetProperty("format").GetInt32());
        JsonElement shop = root.GetProperty("scripts").GetProperty("SHOP.BIN");
        JsonElement pay = root.GetProperty("scripts").GetProperty("PAY.BIN");
        Assert.Equal("{\"#99\":1,\"PAY.BIN\":2}", Compact(shop.GetProperty("calls")));
        Assert.Equal(1, shop.GetProperty("computed_calls").GetInt32());
        Assert.Equal("{\"PAY.BIN\":1}", Compact(shop.GetProperty("preloads")));
        Assert.Equal("[\"SHOP.BIN\"]", Compact(pay.GetProperty("called_by")));
        Assert.False(shop.TryGetProperty("called_by", out _));
        Assert.Equal("{\"cursor\":[2,1,0],\"flags\":[0,1,0],\"line\":[1,0,0],\"prices\":[1,1,1],\"tax\":[0,1,0]}",
                     Compact(shop.GetProperty("globals")));
        Assert.Equal("[0,1,0]", Compact(shop.GetProperty("unnamed")));
        Assert.Equal("{\"prices\":[1,1,0]}", Compact(pay.GetProperty("globals")));
        // A point counts in the revision it lists.
        Assert.Equal("{\"test.shop.open\":\"0x10\"}", Compact(shop.GetProperty("patch_points")));
        Assert.False(pay.TryGetProperty("patch_points", out _));
        Assert.Equal("{\"show-text\":2}", Compact(shop.GetProperty("waits")));
        // A use is one line, so the committed file reads a global per line.
        Assert.Contains("\"cursor\": [2, 1, 0]", text, StringComparison.Ordinal);
    }

    [Fact]
    public void UsesOfRangesSumEachScriptsCells()
    {
        var uses = Index().UsesOf([(GlobalBank.Integer, Prices, Prices + 10)]).ToList();
        Assert.Equal([("SHOP.BIN", new GlobalUse(1, 2, 1)), ("PAY.BIN", new GlobalUse(1, 1, 0))], uses);
        Assert.Empty(Index().UsesOf([(GlobalBank.String, Prices, Prices + 10)]));
    }

    private static string Compact(JsonElement element) => JsonSerializer.Serialize(element);
}
