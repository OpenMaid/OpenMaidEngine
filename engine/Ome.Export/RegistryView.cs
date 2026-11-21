using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Ome.Engine.Natives;

namespace Ome.Export;

/// <summary>A named global array (or one cell): its bank, address, dimensions and named columns.</summary>
public sealed record ArrayLayout(string Name, GlobalBank Bank, int Address, IReadOnlyList<int> Dimensions,
                                 IReadOnlyDictionary<int, string> Columns)
{
    /// <summary>The cells of one record: a two-dimensional table's row width, else 1.</summary>
    public int Width => Dimensions.Count == 2 ? Dimensions[1] : 1;
}

/// <summary>A registry table (data patches spec §5): its record count, its fields (field → array) and their
/// references (game data export spec §5.3).</summary>
public sealed record TableLayout(string Name, int Records, IReadOnlyList<(string Field, string Array)> Fields,
                                 IReadOnlyDictionary<string, string> Refs);

/// <summary>A field as a declaration names it (<c>stats</c>, or one column, <c>graphic.face_CA_AA</c>): the cells it
/// covers in each record.</summary>
public sealed record FieldLayout(string Name, string Field, ArrayLayout Array, int? Column)
{
    /// <summary>How many cells it has in a record.</summary>
    public int Elements => Column is null ? Array.Width : 1;

    /// <summary>Whether it is published as an array (a row), not one value.</summary>
    public bool IsArray => Column is null && Array.Dimensions.Count == 2;

    public bool IsString => Array.Bank == GlobalBank.String;

    /// <summary>The address of element <paramref name="element"/> of record <paramref name="record"/>.</summary>
    public int Cell(int record, int element) => Array.Address + record * Array.Width + (Column ?? element);

    /// <summary>Its type as the export's field table writes it: <c>int</c>, <c>string</c>, <c>int[14]</c>.</summary>
    public string TypeName => (IsString ? "string" : "int") + (IsArray ? $"[{Elements}]" : "");
}

/// <summary>
/// What the exporter reads of a game's registry view, <c>build/games/&lt;game&gt;/globals.json</c>: its tables, the
/// arrays their fields name, and the named single cells a seed writes. It reads the view itself, not through
/// <c>Ome.Script</c>'s module, whose names are OMS identifiers: a column name such as <c>face_CA_AA</c> is the record
/// map's (data patches spec §6.2).
/// </summary>
public sealed class RegistryView
{
    private static readonly Regex TypeShape = new(@"^(int|string|float)((?:\[\d*\])*)$", RegexOptions.CultureInvariant);

    private readonly Dictionary<string, TableLayout> _tables = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ArrayLayout> _arrays = new(StringComparer.Ordinal);

    public RegistryView(IEnumerable<TableLayout> tables, IEnumerable<ArrayLayout> arrays)
    {
        foreach (ArrayLayout array in arrays) _arrays.TryAdd(array.Name, array);
        foreach (TableLayout table in tables) _tables[table.Name] = table;
    }

    public static RegistryView Load(string globalsJson)
    {
        using FileStream stream = File.OpenRead(globalsJson);
        using JsonDocument document = JsonDocument.Parse(stream);
        JsonElement root = document.RootElement;
        var arrays = new List<ArrayLayout>();
        foreach (JsonProperty cell in root.GetProperty("cells").EnumerateObject())
        {
            JsonElement entry = cell.Value;
            if (entry.GetProperty("name").GetString() is not { Length: > 0 } name) continue;
            Match shape = TypeShape.Match(entry.GetProperty("type").GetString() ?? "");
            if (!shape.Success) continue;
            GlobalBank? bank = entry.GetProperty("bank").GetString() switch
            {
                "G" => GlobalBank.Integer,
                "GS" => GlobalBank.String,
                "GF" => GlobalBank.Float,
                _ => null,
            };
            if (bank is null) continue;
            int[] dimensions = Regex.Matches(shape.Groups[2].Value, @"\[(\d*)\]")
                .Select(match => match.Groups[1].Value.Length == 0 ? 0 : int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture))
                .ToArray();
            var columns = new Dictionary<int, string>();
            if (entry.TryGetProperty("columns", out JsonElement map))
                foreach (JsonProperty column in map.EnumerateObject())
                    if (int.TryParse(column.Name, NumberStyles.None, CultureInfo.InvariantCulture, out int index)
                        && column.Value.GetString() is { Length: > 0 } columnName)
                        columns[index] = columnName;
            int address = Convert.ToInt32(entry.GetProperty("address").GetString(), 16);
            arrays.Add(new ArrayLayout(name, bank.Value, address, dimensions, columns));
        }
        var tables = new List<TableLayout>();
        if (root.TryGetProperty("tables", out JsonElement list))
            foreach (JsonElement entry in list.EnumerateArray())
            {
                var fields = entry.GetProperty("fields").EnumerateObject()
                    .Select(field => (field.Name, field.Value.GetString()!)).ToList();
                var refs = entry.TryGetProperty("refs", out JsonElement refMap)
                    ? refMap.EnumerateObject().ToDictionary(pair => pair.Name, pair => pair.Value.GetString()!, StringComparer.Ordinal)
                    : new Dictionary<string, string>(StringComparer.Ordinal);
                tables.Add(new TableLayout(entry.GetProperty("name").GetString()!, entry.GetProperty("records").GetInt32(),
                                           fields, refs));
            }
        return new RegistryView(tables, arrays);
    }

    public TableLayout Table(string name)
        => _tables.TryGetValue(name, out TableLayout? table)
            ? table
            : throw new InvalidDataException($"the registry has no table {name}");

    /// <summary>Every field of <paramref name="table"/>, each whole, in the registry's order.</summary>
    public IReadOnlyList<FieldLayout> FieldsOf(TableLayout table)
        => table.Fields.Select(field => Resolve(table, field.Field)).ToList();

    /// <summary>A field of <paramref name="table"/> as a declaration names it: <c>field</c>, or <c>field.column</c>
    /// with the column's name (its number where it has none).</summary>
    public FieldLayout Resolve(TableLayout table, string reference)
    {
        int dot = reference.IndexOf('.', StringComparison.Ordinal);
        string field = dot < 0 ? reference : reference[..dot];
        string? arrayName = table.Fields.FirstOrDefault(pair => pair.Field == field).Array;
        if (arrayName is null || !_arrays.TryGetValue(arrayName, out ArrayLayout? array))
            throw new InvalidDataException($"table {table.Name} has no field {field}");
        if (array.Dimensions.Count == 0 || array.Dimensions[0] != table.Records)
            throw new InvalidDataException($"{table.Name}.{field}'s array {arrayName} is not {table.Records} records long");
        if (dot < 0) return new FieldLayout(reference, field, array, null);
        string columnName = reference[(dot + 1)..];
        int column = array.Columns.FirstOrDefault(pair => pair.Value == columnName) is { Value: not null } named
            ? named.Key
            : int.TryParse(columnName, NumberStyles.None, CultureInfo.InvariantCulture, out int number)
              && number < array.Width && !array.Columns.ContainsKey(number)
                ? number
                : throw new InvalidDataException($"{table.Name}.{field} has no column {columnName}");
        if (array.Dimensions.Count != 2)
            throw new InvalidDataException($"{table.Name}.{field} has no columns");
        return new FieldLayout(reference, field, array, column);
    }

    /// <summary>The address of a named integer cell, as a seed writes it.</summary>
    public int IntegerGlobal(string name)
        => _arrays.TryGetValue(name, out ArrayLayout? cell) && cell.Bank == GlobalBank.Integer && cell.Dimensions.Count == 0
            ? cell.Address
            : throw new InvalidDataException($"the registry has no integer global {name}");
}
