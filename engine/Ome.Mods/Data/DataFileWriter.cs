using System.Text;
using Ome.Engine.Modding;

namespace Ome.Mods.Data;

/// <summary>
/// Writes a data file's document as TOML (game data export spec §6.6, plan §4.3), which <see cref="DataDocumentReader"/>
/// reads back to the same values:
/// <list type="bullet">
/// <item>a table that holds values other than tables gets a header (<c>[item.12]</c>; the root none), and under it each
/// of those values on a line of its own (<c>name = "剣"</c>), with each of its tables that hold no table written inline
/// (<c>price = {0 = 10, sell = 5}</c>);</item>
/// <item>a table that holds only tables gets no header, since its sections' headers make it: each of its tables is a
/// section of its own (<c>[stage.51.enemy.3]</c>, not <c>3 = {…}</c> under <c>[stage.51.enemy]</c>);</item>
/// <item>the other tables follow as sections.</item>
/// </list>
/// Keys and values are written as <see cref="DataValue"/> prints them. A table read back holds its lines before its
/// sections, which is the only order it may lose.
/// </summary>
public static class DataFileWriter
{
    public static string Write(DataTable root)
    {
        var text = new StringBuilder();
        WriteTable(root, [], text);
        return text.ToString();
    }

    private static void WriteTable(DataTable table, List<string> path, StringBuilder text)
    {
        bool lines = table.Entries.Any(entry => entry.Value is not DataTable);
        bool OnALine(DataValue value) => value is not DataTable inner || lines && inner.Entries.All(e => e.Value is not DataTable);

        bool header = path.Count > 0;
        void Header()
        {
            if (text.Length > 0) text.Append('\n');
            text.Append('[').Append(string.Join('.', path.Select(DataValue.KeyText))).Append("]\n");
            header = false;
        }
        foreach (var (key, value) in table.Entries)
        {
            if (!OnALine(value)) continue;
            if (header) Header();
            text.Append(DataValue.KeyText(key)).Append(" = ").Append(value.ToText()).Append('\n');
        }
        if (header && table.Count == 0) Header();   // an empty table has only its header to say it is there
        foreach (var (key, value) in table.Entries)
            if (value is DataTable inner && !OnALine(inner))
            {
                path.Add(key);
                WriteTable(inner, path, text);
                path.RemoveAt(path.Count - 1);
            }
    }
}
