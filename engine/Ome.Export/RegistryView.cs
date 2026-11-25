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

    /// <summary>What its values mean to a reader (export plan §5), or null when the registry says nothing.</summary>
    public ValueFormat? Format { get; init; }

    /// <summary>The game's table of names for its columns (<c>columns_from</c>): element k of the string global names
    /// column first + k; null when the registry gives none.</summary>
    public (string Global, int First)? ColumnNamesFrom { get; init; }

    /// <summary>The registry table whose record k its column k is (<c>columns_ref</c>), or null.</summary>
    public string? ColumnsOf { get; init; }
}

/// <summary>A global's value facets (export plan §5; <c>tools/globals_build.py</c>): OME's words for some values, the
/// game's table of names (element k of the string global names value first + k), the global whose values a mask's
/// bits stand for, and how a number is shown: the stored value times the scale, plus the offset, in the unit; and the
/// stored value that means none, which a reader is not shown as a number (a quest's deadline of 0).</summary>
public sealed record ValueFormat
{
    public IReadOnlyDictionary<long, string> Words { get; init; } = new Dictionary<long, string>();
    public (string Global, int First)? NamesFrom { get; init; }
    public string? BitsOf { get; init; }
    public double? Scale { get; init; }
    public long? Offset { get; init; }
    public string? Unit { get; init; }
    public long? None { get; init; }

    /// <summary>Whether a value may have a word or a name: its own, or its bits'.</summary>
    public bool Names => Words.Count > 0 || NamesFrom is not null || BitsOf is not null;

    internal static ValueFormat? Parse(JsonElement entry)
    {
        var format = new ValueFormat
        {
            Words = entry.TryGetProperty("values", out JsonElement values) && values.ValueKind == JsonValueKind.Object
                ? values.EnumerateObject().ToDictionary(pair => long.Parse(pair.Name, CultureInfo.InvariantCulture),
                                                         pair => pair.Value.GetString()!)
                : new Dictionary<long, string>(),
            NamesFrom = entry.TryGetProperty("values_from", out JsonElement from) && from.ValueKind == JsonValueKind.Object
                ? (from.GetProperty("global").GetString()!, from.GetProperty("first").GetInt32())
                : null,
            BitsOf = entry.TryGetProperty("bits_of", out JsonElement bits) && bits.ValueKind == JsonValueKind.String
                ? bits.GetString()
                : null,
            Scale = entry.TryGetProperty("scale", out JsonElement scale) && scale.ValueKind == JsonValueKind.Number
                ? scale.GetDouble()
                : null,
            Offset = entry.TryGetProperty("offset", out JsonElement offset) && offset.ValueKind == JsonValueKind.Number
                ? offset.GetInt64()
                : null,
            Unit = entry.TryGetProperty("unit", out JsonElement unit) && unit.ValueKind == JsonValueKind.String
                ? unit.GetString()
                : null,
            None = entry.TryGetProperty("none", out JsonElement none) && none.ValueKind == JsonValueKind.Number
                ? none.GetInt64()
                : null,
        };
        return format.Names || format.Scale is not null || format.Offset is not null || format.Unit is not null
               || format.None is not null
            ? format
            : null;
    }
}

/// <summary>A registry table (data patches spec §5): its record count, its fields (field → array) and their
/// references (game data export spec §5.3).</summary>
public sealed record TableLayout(string Name, int Records, IReadOnlyList<(string Field, string Array)> Fields,
                                 IReadOnlyDictionary<string, string> Refs)
{
    /// <summary>Its two-field references' keys, by field (<c>ref_keys</c>): the record a value names is the value times
    /// <see cref="RefKey.Times"/> plus the <see cref="RefKey.Plus"/> field's value, or for a paired reference the next
    /// element's.</summary>
    public IReadOnlyDictionary<string, RefKey> RefKeys { get; init; } = new Dictionary<string, RefKey>();

    /// <summary>The table whose record r its record r belongs to (<c>records_of</c>), or null: Arterial's party_member,
    /// kept for each character by its character id.</summary>
    public string? RecordsOf { get; init; }

    /// <summary>The table whose record r ÷ <c>Per</c> its record r belongs to (<c>owned_by</c>), or null: a Verita title,
    /// keyed 20 × character + k, is character r ÷ 20's.</summary>
    public (string Table, int Per)? OwnedBy { get; init; }
}

/// <summary>How a two-field reference names its record (export plan §5): the field's value times
/// <paramref name="Times"/>, plus field <paramref name="Plus"/>'s value at the same element. Arterial names a card by
/// its row and slot, row × 30 + slot. A paired reference (<paramref name="Plus"/> null) takes both from its own field:
/// element 2k times <paramref name="Times"/>, plus element 2k + 1 (a battle's leader card for each side). An offset
/// reference (<see cref="Offset"/>) names the value plus the offset.</summary>
public sealed record RefKey(int Times, string? Plus)
{
    public bool Pairs => Plus is null && Offset is null && Zero is null;

    /// <summary>Where a 0 names record 0 instead of none (<c>zero</c>), or null for nowhere: a plain or offset reference
    /// may have one. A Kamidori skill's attack element 1, less 1, is element 0, 無属, and so is a weapon's 0.</summary>
    public RefZero? Zero { get; init; }

    /// <summary>What a value above 0 adds to name its record (<c>offset</c>), or null: a Himegari unit's summon flag
    /// less 1901 is its summoning stone's item id.</summary>
    public long? Offset { get; init; }

    /// <summary>The plus value that names every record of the value's row, value × <see cref="Times"/> to value ×
    /// <see cref="Times"/> + <see cref="Times"/> - 1 (<c>any</c>), or null: an Arterial item that every card of a row
    /// may carry has slot 0.</summary>
    public long? Any { get; init; }

    /// <summary>The values that name a record, each with the record its plus counts from (<c>bases</c>), or null: a
    /// value that is not one of them names none. A Kamidori map object of type 32 gathers from dig table 0 plus its
    /// payload, one of type 33 from table 100 plus it, and another type from none.</summary>
    public IReadOnlyDictionary<long, long>? Bases { get; init; }
}

/// <summary>Where a reference's 0 names record 0 (<c>zero</c>): on every record (<paramref name="Field"/> null,
/// <c>zero = true</c>), or on those whose one-cell field <paramref name="Field"/> holds one of
/// <paramref name="Values"/> (a Kamidori item of a category that attacks).</summary>
public sealed record RefZero(string? Field, IReadOnlySet<long> Values);

/// <summary>A field as a declaration names it (<c>stats</c>, or one column, <c>graphic.face_CA_AA</c>): the cells it
/// covers in each record. A field of a table the kind joins (<see cref="Joined"/>) is named <c>table.field</c>.</summary>
public sealed record FieldLayout(string Name, string Field, ArrayLayout Array, int? Column)
{
    /// <summary>The table the kind joins that the field is of (export plan §5, <c>joins</c>), or null for the kind's own.
    /// </summary>
    public string? Joined { get; init; }

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
            arrays.Add(new ArrayLayout(name, bank.Value, address, dimensions, columns)
            {
                Format = ValueFormat.Parse(entry),
                ColumnNamesFrom = entry.TryGetProperty("columns_from", out JsonElement from)
                                  && from.ValueKind == JsonValueKind.Object
                    ? (from.GetProperty("global").GetString()!, from.GetProperty("first").GetInt32())
                    : null,
                ColumnsOf = entry.TryGetProperty("columns_ref", out JsonElement of) && of.ValueKind == JsonValueKind.String
                    ? of.GetString()
                    : null,
            });
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
                var keys = entry.TryGetProperty("ref_keys", out JsonElement keyMap)
                    ? keyMap.EnumerateObject().ToDictionary(
                        pair => pair.Name,
                        pair => new RefKey(pair.Value.TryGetProperty("times", out JsonElement times) ? times.GetInt32() : 1,
                                           pair.Value.TryGetProperty("plus", out JsonElement plus) ? plus.GetString() : null)
                        {
                            Any = pair.Value.TryGetProperty("any", out JsonElement any) ? any.GetInt64() : null,
                            Offset = pair.Value.TryGetProperty("offset", out JsonElement offset) ? offset.GetInt64() : null,
                            Bases = pair.Value.TryGetProperty("bases", out JsonElement bases)
                                ? bases.EnumerateObject().ToDictionary(
                                    basePair => long.Parse(basePair.Name, CultureInfo.InvariantCulture),
                                    basePair => basePair.Value.GetInt64())
                                : null,
                            Zero = !pair.Value.TryGetProperty("zero", out JsonElement zero) ? null
                                : zero.ValueKind == JsonValueKind.True ? new RefZero(null, new HashSet<long>())
                                : new RefZero(zero.GetProperty("field").GetString(),
                                              zero.GetProperty("values").EnumerateArray().Select(v => v.GetInt64()).ToHashSet()),
                        },
                        StringComparer.Ordinal)
                    : new Dictionary<string, RefKey>(StringComparer.Ordinal);
                tables.Add(new TableLayout(entry.GetProperty("name").GetString()!, entry.GetProperty("records").GetInt32(),
                                           fields, refs)
                {
                    RefKeys = keys,
                    RecordsOf = entry.TryGetProperty("records_of", out JsonElement of) ? of.GetString() : null,
                    OwnedBy = entry.TryGetProperty("owned_by", out JsonElement owner)
                        ? (owner.GetProperty("table").GetString()!, owner.GetProperty("per").GetInt32())
                        : null,
                });
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
        if (arrayName is null || !_arrays.TryGetValue(arrayName, out ArrayLayout? global))
            throw new InvalidDataException($"table {table.Name} has no field {field}");
        ArrayLayout array = AsRecords(global, table.Records)
            ?? throw new InvalidDataException($"{table.Name}.{field}'s array {arrayName} is not {table.Records} records long");
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

    /// <summary>A global as <paramref name="records"/> records: its own dimensions when they lead with the count, and for
    /// a table of one record a scalar as <c>[1]</c> and a list of n as <c>[1][n]</c>, its one row (the registry's
    /// one-record tables, <c>globals_build.record_dims</c>); null for any other shape.</summary>
    private static ArrayLayout? AsRecords(ArrayLayout global, int records)
    {
        IReadOnlyList<int> dimensions = global.Dimensions;
        if (dimensions.Count is 1 or 2 && dimensions[0] == records) return global;
        if (records != 1 || dimensions.Count > 1) return null;
        return global with { Dimensions = dimensions.Count == 0 ? [1] : [1, dimensions[0]] };
    }

    /// <summary>A named global, or null.</summary>
    public ArrayLayout? Find(string name) => _arrays.GetValueOrDefault(name);

    /// <summary>The address of a named integer cell, as a seed writes it.</summary>
    public int IntegerGlobal(string name)
        => _arrays.TryGetValue(name, out ArrayLayout? cell) && cell.Bank == GlobalBank.Integer && cell.Dimensions.Count == 0
            ? cell.Address
            : throw new InvalidDataException($"the registry has no integer global {name}");

    /// <summary>A named integer global, one cell or an array, as a loader's seed writes it (export plan §5).</summary>
    public ArrayLayout IntegerArray(string name)
        => _arrays.TryGetValue(name, out ArrayLayout? array) && array.Bank == GlobalBank.Integer
           && array.Dimensions.All(size => size > 0)
            ? array
            : throw new InvalidDataException($"the registry has no integer global {name} of a fixed size");

    /// <summary>The address of a named integer global, or of the first cell of a named list of them (a one-dimensional
    /// array), as a script relation's key reads it (export plan §4.5).</summary>
    public int IntegerCell(string name)
        => _arrays.TryGetValue(name, out ArrayLayout? cell) && cell.Bank == GlobalBank.Integer && cell.Dimensions.Count <= 1
            ? cell.Address
            : throw new InvalidDataException($"the registry has no integer global or list of them {name}");
}
