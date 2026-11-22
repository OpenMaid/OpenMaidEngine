using Ome.Engine.Modding;
using Ome.Mods.Data;
using Xunit;

namespace Ome.Engine.Tests;

/// <summary><see cref="DataFileWriter"/> (game data export spec §6.6, plan §4.3): what it writes, read back with
/// <see cref="DataDocumentReader"/>.</summary>
public class DataFileWriterTests
{
    private static DataTable T(params (string Key, DataValue Value)[] entries)
        => new(entries.Select(entry => KeyValuePair.Create(entry.Key, entry.Value)).ToList());

    private static DataInteger I(long value) => new(value);

    private static DataTable ReadBack(string text)
    {
        DataReadResult read = DataDocumentReader.Read(text, "snippet.toml");
        Assert.True(read.Ok, string.Join("\n", read.Problems.Select(problem => problem.Describe("snippet.toml"))));
        return read.Document!.Root;
    }

    [Fact]
    public void ARecordIsATableUnderItsHeaderAndARowIsOneInlineTable()
    {
        DataTable document = T(
            ("item", T(("12", T(("price", T(("0", I(10)), ("sell", I(5)))), ("name", new DataString("剣")))))),
            ("stage", T(("51", T(("enemy", T(("3", T(("unit_id", I(40)), ("min_level", I(-1)))))))))));
        string text = DataFileWriter.Write(document);
        Assert.Equal("""
            [item.12]
            price = {0 = 10, sell = 5}
            name = "剣"

            [stage.51.enemy.3]
            unit_id = 40
            min_level = -1

            """.ReplaceLineEndings("\n"), text);
        Assert.Equal(document.ToText(), ReadBack(text).ToText());
    }

    [Fact]
    public void EveryKindOfValueAndKeyReadsBackTheSame()
    {
        DataTable deep = T(("deeper", T(("leaf", I(9)))));
        DataTable unit = T(("1", T(("stats", T(("max_hp", I(150)))), ("deep", deep))));
        DataTable document = T(
            ("title", new DataString("say \"hi\" \\ then\na tab\there and U+202E: ‮")),
            ("on", new DataBoolean(true)),
            ("ratio", new DataFloat(0.25)),
            ("big", new DataFloat(1e20)),
            ("whole", new DataFloat(3)),
            ("limit", I(DataInteger.Limit)),
            ("list", new DataArray([I(1), new DataString("two"), new DataArray([]), T(("x", I(3)))])),
            ("empty", T()),
            ("a b", T(("日本", I(1)), ("-dash_", I(2)))),
            ("unit", unit));
        Assert.Equal(document.ToText(), ReadBack(DataFileWriter.Write(document)).ToText());
    }

    [Fact]
    public void ATableReadBackHoldsItsValuesBeforeItsSections()
    {
        DataTable record = T(("deep", T(("x", T(("y", I(1)))))), ("hp", I(5)));
        DataTable document = T(("unit", T(("1", record))));
        DataTable back = ReadBack(DataFileWriter.Write(document));
        Assert.Equal("{unit = {1 = {hp = 5, deep = {x = {y = 1}}}}}", back.ToText());
    }

    [Fact]
    public void AnEmptyTableAmongTablesKeepsItsHeader()
    {
        DataTable document = T(("stage", T(("51", T()), ("52", T(("x", I(1)))))));
        string text = DataFileWriter.Write(document);
        Assert.Equal("[stage.51]\n\n[stage.52]\nx = 1\n", text);
        Assert.Equal(document.ToText(), ReadBack(text).ToText());
    }

    [Fact]
    public void AnEmptyDocumentIsAnEmptyFile()
    {
        Assert.Equal("", DataFileWriter.Write(T()));
        Assert.Equal("{}", ReadBack("").ToText());
    }
}
