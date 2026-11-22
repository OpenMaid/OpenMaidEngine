using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Ome.Engine.Natives;

namespace Ome.Export;

/// <summary>What a manifest records beside the database (game data export spec §7.1): what changes from run to run
/// and machine to machine, so the database does not.</summary>
public sealed record ExportManifest(DateTimeOffset ExportTime, string CommandLine, string? GameVersion,
                                    string OmeVersion, string OmeCommit);

/// <summary>The files an export wrote, relative to its output folder, with their SHA-256.</summary>
public sealed record ExportFiles(string Database, IReadOnlyList<(string File, string Sha256)> Files);

/// <summary>
/// Writes an export (spec §7): <c>&lt;revision&gt;.sqlite</c>, <c>pictures/&lt;revision&gt;/&lt;catalog name&gt;.png</c>
/// and <c>manifest.json</c>. The database is written in key order with a fixed page size and vacuumed, so the same
/// export gives the same bytes (§7.3).
/// </summary>
public static class ExportWriter
{
    /// <summary>The database's format, in <c>build_info</c> and its <c>user_version</c>.</summary>
    public const int FormatVersion = 1;

    /// <summary>The source text's language (localization spec §4: <c>LanguageTag.Source</c>).</summary>
    public const string SourceLanguage = "ja";

    public static ExportFiles Write(ExportResult result, string outputDirectory, string revision, ExportManifest manifest)
    {
        Directory.CreateDirectory(outputDirectory);
        string databaseName = revision + ".sqlite";
        string database = Path.Combine(outputDirectory, databaseName);
        var files = new List<(string File, string Sha256)>();

        // Each distinct picture once (§6.5); a picture folder from an earlier export of this revision goes first.
        string pictureFolder = Path.Combine(outputDirectory, "pictures", revision);
        if (Directory.Exists(pictureFolder)) Directory.Delete(pictureFolder, recursive: true);
        var pictures = new SortedDictionary<string, ExportPicture>(StringComparer.Ordinal);
        foreach (ExportKind kind in result.Kinds)
            foreach (ExportEntity entity in kind.Entities)
                foreach ((_, ExportPicture picture) in entity.Pictures)
                    pictures.TryAdd(picture.Name, picture);
        if (pictures.Count > 0) Directory.CreateDirectory(pictureFolder);
        foreach (ExportPicture picture in pictures.Values)
            File.WriteAllBytes(Path.Combine(pictureFolder, picture.Name + ".png"), picture.Png);

        string temporary = database + ".tmp";
        File.Delete(temporary);
        WriteDatabase(result, temporary, revision);
        File.Move(temporary, database, overwrite: true);

        files.Add((databaseName, Sha256Of(File.ReadAllBytes(database))));
        foreach (ExportPicture picture in pictures.Values)
            files.Add(($"pictures/{revision}/{picture.Name}.png", picture.Hash));
        WriteManifest(Path.Combine(outputDirectory, "manifest.json"), result, revision, manifest, files);
        return new ExportFiles(databaseName, files);
    }

    private static string Sha256Of(byte[] bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));

    // ------------------------------------------------------------------------------------------------- the database

    private static void WriteDatabase(ExportResult result, string path, string revision)
    {
        string connectionString = new SqliteConnectionStringBuilder { DataSource = path, Pooling = false }.ToString();
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            Execute(connection, "PRAGMA page_size = 4096");
            Execute(connection, "PRAGMA journal_mode = OFF");
            Execute(connection, $"PRAGMA user_version = {FormatVersion}");
            using (SqliteTransaction transaction = connection.BeginTransaction())
            {
                CreateTables(connection, result);
                WriteBuildInfo(connection, result, revision);
                foreach (ExportKind kind in result.Kinds) WriteKind(connection, kind, revision, result.Kinds);
                transaction.Commit();
            }
            Execute(connection, "VACUUM");
        }
    }

    private static void Execute(SqliteConnection connection, string sql)
    {
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private static string Quote(string identifier) => "\"" + identifier.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";

    private static void CreateTables(SqliteConnection connection, ExportResult result)
    {
        string[] fixedTables =
        [
            "CREATE TABLE build_info (key TEXT PRIMARY KEY, value TEXT NOT NULL)",
            "CREATE TABLE kind (id TEXT PRIMARY KEY, table_name TEXT NOT NULL, title_field TEXT, aliases TEXT NOT NULL, entities INTEGER NOT NULL)",
            "CREATE TABLE field (kind TEXT NOT NULL, name TEXT NOT NULL, type TEXT NOT NULL, published INTEGER NOT NULL, "
            + "text INTEGER NOT NULL, filter INTEGER NOT NULL, ref_table TEXT, ref_kind TEXT, picture TEXT, bank TEXT, "
            + "address INTEGER, width INTEGER, column_index INTEGER, columns TEXT, PRIMARY KEY (kind, name))",
            // A kind's groups (export plan §5): its fields shown as one table, a row for each element, in order.
            "CREATE TABLE field_group (kind TEXT NOT NULL, name TEXT NOT NULL, position INTEGER NOT NULL, "
            + "field TEXT NOT NULL, PRIMARY KEY (kind, name, position))",
            // A group's row labels (export plan §5): a loaded table's text field, table.field, whose record r labels
            // row r.
            "CREATE TABLE group_label (kind TEXT NOT NULL, name TEXT NOT NULL, labels TEXT NOT NULL, "
            + "PRIMARY KEY (kind, name))",
            // A line that explains a detail to a reader (export plan §5): a field's, a group's, or a loaded table's
            // field's, written table.field.
            "CREATE TABLE note (kind TEXT NOT NULL, name TEXT NOT NULL, text TEXT NOT NULL, PRIMARY KEY (kind, name))",
            // The blocks a text field's lines are drawn in (export plan §5): lines first to last, in the game's order.
            "CREATE TABLE text_block (kind TEXT NOT NULL, field TEXT NOT NULL, position INTEGER NOT NULL, "
            + "name TEXT NOT NULL, first INTEGER NOT NULL, last INTEGER NOT NULL, PRIMARY KEY (kind, field, position))",
            "CREATE TABLE text (kind TEXT NOT NULL, id INTEGER NOT NULL, field TEXT NOT NULL, idx INTEGER NOT NULL, "
            + "lang TEXT NOT NULL, value TEXT NOT NULL, PRIMARY KEY (kind, id, field, idx, lang))",
            "CREATE TABLE ref (from_kind TEXT NOT NULL, from_id INTEGER NOT NULL, field TEXT NOT NULL, idx INTEGER NOT NULL, "
            + "to_kind TEXT NOT NULL, to_id INTEGER NOT NULL, PRIMARY KEY (from_kind, from_id, field, idx))",
            // The calls the script relations resolved (spec §6.7, export plan §4.5): the script as the index names it,
            // the call's offset, and the count as the script sets it, signed (null for a relation with none).
            "CREATE TABLE relation (kind TEXT NOT NULL, id INTEGER NOT NULL, relation TEXT NOT NULL, script TEXT NOT NULL, "
            + "offset INTEGER NOT NULL, count INTEGER, PRIMARY KEY (kind, id, relation, script, offset))",
            "CREATE TABLE picture (kind TEXT NOT NULL, id INTEGER NOT NULL, name TEXT NOT NULL, file TEXT NOT NULL, "
            + "explicit INTEGER NOT NULL, "
            + "width INTEGER NOT NULL, height INTEGER NOT NULL, hash TEXT NOT NULL, PRIMARY KEY (kind, id, name))",
            "CREATE TABLE cell (kind TEXT NOT NULL, id INTEGER NOT NULL, field TEXT NOT NULL, idx INTEGER NOT NULL, "
            + "bank TEXT NOT NULL, address INTEGER NOT NULL, raw TEXT, writer TEXT, PRIMARY KEY (kind, id, field, idx))",
            "CREATE TABLE snippet (kind TEXT NOT NULL, id INTEGER NOT NULL, text TEXT NOT NULL, PRIMARY KEY (kind, id))",
            // The scripts whose operands read, write or index (take as an array's base) a cell of the kind's table or
            // of its loaded tables (export plan §4.3), from the install's script index.
            "CREATE TABLE script_use (kind TEXT NOT NULL, script TEXT NOT NULL, reads INTEGER NOT NULL, writes INTEGER NOT NULL, "
            + "addresses INTEGER NOT NULL, PRIMARY KEY (kind, script))",
            // A loader's published tables (export plan §4.2): each listed, its fields as `field` lists a kind's, and its
            // rows in a table named <kind>.<name>.
            // Its scope is null for a table no data file edits (export plan §5), whose loader has none. A loaded text
            // field's values are the entity's text, written table.field.
            "CREATE TABLE loaded (kind TEXT NOT NULL, name TEXT NOT NULL, table_name TEXT NOT NULL, scope TEXT, "
            + "records INTEGER NOT NULL, rows INTEGER NOT NULL, PRIMARY KEY (kind, name))",
            "CREATE TABLE loaded_field (kind TEXT NOT NULL, loaded TEXT NOT NULL, name TEXT NOT NULL, type TEXT NOT NULL, "
            + "published INTEGER NOT NULL, text INTEGER NOT NULL, ref_table TEXT, ref_kind TEXT, bank TEXT, "
            + "address INTEGER, width INTEGER, column_index INTEGER, columns TEXT, PRIMARY KEY (kind, loaded, name))",
            // A loaded table's parts (export plan §5), which the site shows apart, in order: a row is in the part when
            // global[field] is not 0 and equals global[same_as]. A parted table's rows name theirs in a part column,
            // null for the table's own.
            "CREATE TABLE loaded_part (kind TEXT NOT NULL, loaded TEXT NOT NULL, name TEXT NOT NULL, "
            + "position INTEGER NOT NULL, field TEXT NOT NULL, global TEXT NOT NULL, same_as INTEGER NOT NULL, note TEXT, "
            + "PRIMARY KEY (kind, loaded, name))",
            // A reader's name for a field or group (export plan §5), a loaded table's field's written table.field.
            "CREATE TABLE label (kind TEXT NOT NULL, name TEXT NOT NULL, text TEXT NOT NULL, PRIMARY KEY (kind, name))",
            // How a shown field's number reads (export plan §5): the stored value times scale, plus offset, in unit;
            // bits is 1 for a mask, whose value names are its bits'. A loaded table's field is written table.field.
            "CREATE TABLE value_format (kind TEXT NOT NULL, field TEXT NOT NULL, scale REAL, offset INTEGER, unit TEXT, "
            + "bits INTEGER NOT NULL, PRIMARY KEY (kind, field))",
            // What a shown field's value means (export plan §5): OME's word, lang '', or the game's name in a language.
            "CREATE TABLE value_name (kind TEXT NOT NULL, field TEXT NOT NULL, value INTEGER NOT NULL, lang TEXT NOT NULL, "
            + "text TEXT NOT NULL, PRIMARY KEY (kind, field, value, lang))",
        ];
        foreach (string sql in fixedTables) Execute(connection, sql);
        foreach (ExportKind kind in result.Kinds)
        {
            var columns = new List<string> { "id INTEGER PRIMARY KEY" };
            columns.AddRange(kind.Published.Select(field => Quote(field.Name) + (field.IsArray ? " TEXT" : " INTEGER")));
            columns.Add("content_hash TEXT NOT NULL");
            Execute(connection, $"CREATE TABLE {Quote(kind.Declaration.Id)} ({string.Join(", ", columns)})");
            foreach (ExportLoadedTable table in kind.Loaded)
            {
                var loaded = new List<string> { "id INTEGER NOT NULL", "record INTEGER NOT NULL" };
                loaded.AddRange(table.Published.Select(field => Quote(field.Name) + (field.IsArray ? " TEXT" : " INTEGER")));
                if (table.Parts.Count > 0) loaded.Add("part TEXT");
                loaded.Add("PRIMARY KEY (id, record)");
                Execute(connection, $"CREATE TABLE {Quote(LoadedTableName(kind, table))} ({string.Join(", ", loaded)})");
            }
        }
    }

    /// <summary>The SQL table a loaded table's rows go in: <c>stage.enemy</c>. A kind id has no dot, so no kind's table
    /// takes the name.</summary>
    public static string LoadedTableName(ExportKind kind, ExportLoadedTable table)
        => $"{kind.Declaration.Id}.{table.Declaration.Name}";

    /// <summary>Inserts rows through one prepared command.</summary>
    private sealed class Inserter : IDisposable
    {
        private readonly SqliteCommand _command;
        private readonly SqliteParameter[] _parameters;

        public Inserter(SqliteConnection connection, string table, params string[] columns)
        {
            _command = connection.CreateCommand();
            _command.CommandText = $"INSERT INTO {Quote(table)} ({string.Join(", ", columns.Select(Quote))}) VALUES "
                                   + $"({string.Join(", ", columns.Select((_, k) => "$p" + k.ToString(CultureInfo.InvariantCulture)))})";
            _parameters = columns.Select((_, k) => _command.Parameters.Add(new SqliteParameter("$p" + k.ToString(CultureInfo.InvariantCulture), null)))
                .ToArray();
        }

        public void Row(params object?[] values)
        {
            for (int k = 0; k < values.Length; k++) _parameters[k].Value = values[k] ?? DBNull.Value;
            _command.ExecuteNonQuery();
        }

        public void Dispose() => _command.Dispose();
    }

    private static void WriteBuildInfo(SqliteConnection connection, ExportResult result, string revision)
    {
        var rows = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["format_version"] = FormatVersion.ToString(CultureInfo.InvariantCulture),
            ["game"] = result.Declarations.Profile,
            ["revision"] = revision,
            ["declarations_sha256"] = result.Declarations.SourceSha256,
            ["languages"] = string.Join(",", result.Languages.Select(language => language.Tag).Prepend(SourceLanguage)),
            ["source_language"] = SourceLanguage,
        };
        // Each translated language's packages (spec §6.4), for the site to credit.
        if (result.Languages.Count > 0) rows["language_packages"] = LanguagePackages(result.Languages);
        foreach ((string key, string value) in result.BuildInfo) rows[key] = value;
        using var insert = new Inserter(connection, "build_info", "key", "value");
        foreach ((string key, string value) in rows) insert.Row(key, value);
    }

    /// <summary>A JSON array of each language's packages: its language, id, name, version and authors.</summary>
    private static string LanguagePackages(IReadOnlyList<ExportLanguage> languages)
    {
        var buffer = new MemoryStream();
        using (var json = new Utf8JsonWriter(buffer, new JsonWriterOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }))
        {
            json.WriteStartArray();
            foreach (ExportLanguage language in languages)
                foreach (ExportLanguagePackage package in language.Packages)
                {
                    json.WriteStartObject();
                    json.WriteString("language", language.Tag);
                    json.WriteString("id", package.Id);
                    json.WriteString("name", package.Name);
                    json.WriteString("version", package.Version);
                    json.WriteStartArray("authors");
                    foreach (string author in package.Authors) json.WriteStringValue(author);
                    json.WriteEndArray();
                    json.WriteEndObject();
                }
            json.WriteEndArray();
        }
        return Encoding.UTF8.GetString(buffer.ToArray());
    }

    private static void WriteKind(SqliteConnection connection, ExportKind kind, string revision,
                                  IReadOnlyList<ExportKind> kinds)
    {
        KindDeclaration declaration = kind.Declaration;
        string id = declaration.Id;
        using (var insert = new Inserter(connection, "kind", "id", "table_name", "title_field", "aliases", "entities"))
            insert.Row(id, kind.Table.Name, declaration.Title, JsonSerializer.Serialize(declaration.Aliases), kind.Entities.Count);
        WriteFields(connection, kind, kinds);

        string[] columns = ["id", .. kind.Published.Select(field => field.Name), "content_hash"];
        using var values = new Inserter(connection, id, columns);
        using var texts = new Inserter(connection, "text", "kind", "id", "field", "idx", "lang", "value");
        using var refs = new Inserter(connection, "ref", "from_kind", "from_id", "field", "idx", "to_kind", "to_id");
        using var pictures = new Inserter(connection, "picture", "kind", "id", "name", "file", "explicit", "width", "height",
                                          "hash");
        using var cells = new Inserter(connection, "cell", "kind", "id", "field", "idx", "bank", "address", "raw", "writer");
        using var relations = new Inserter(connection, "relation", "kind", "id", "relation", "script", "offset", "count");
        foreach (ExportEntity entity in kind.Entities)
        {
            var row = new List<object?> { entity.Id };
            foreach ((FieldLayout field, (string _, long?[] elements)) in kind.Published.Zip(entity.Values))
                row.Add(field.IsArray ? JsonSerializer.Serialize(elements) : elements[0]);
            row.Add(ContentHash(entity));
            values.Row([.. row]);
            foreach (ExportText text in entity.Texts)
                texts.Row(id, entity.Id, text.Field, text.Index, text.Language, text.Value);
            foreach (ExportRef reference in entity.Refs)
                refs.Row(id, entity.Id, reference.Field, reference.Index, reference.ToKind, reference.ToId);
            foreach ((string name, ExportPicture picture) in entity.Pictures)
                pictures.Row(id, entity.Id, name, $"pictures/{revision}/{picture.Name}.png", picture.Explicit ? 1 : 0,
                             picture.Width, picture.Height, picture.Hash);
            foreach (ExportModderCell cell in entity.Cells)
                cells.Row(id, entity.Id, cell.Field, cell.Index, BankName(cell.Bank), cell.Address, cell.Raw, cell.Writer);
            foreach (ExportRelation relation in entity.Relations)
                relations.Row(id, entity.Id, relation.Relation, relation.Script, relation.Offset, relation.Count);
        }
        using (var snippets = new Inserter(connection, "snippet", "kind", "id", "text"))
            foreach (ExportEntity entity in kind.Entities)
                if (entity.Snippet != null) snippets.Row(id, entity.Id, entity.Snippet);
        using (var uses = new Inserter(connection, "script_use", "kind", "script", "reads", "writes", "addresses"))
            foreach ((string script, GlobalUse use) in kind.ScriptUse)
                uses.Row(id, script, use.Reads, use.Writes, use.Addresses);
        WriteLoaded(connection, kind, kinds);
    }

    /// <summary>A kind's loaded tables (export plan §4.2): one <c>loaded</c> row each, a <c>loaded_field</c> row per field
    /// of its registry table, and its rows by key entity and record.</summary>
    private static void WriteLoaded(SqliteConnection connection, ExportKind kind, IReadOnlyList<ExportKind> kinds)
    {
        string id = kind.Declaration.Id;
        foreach (ExportLoadedTable table in kind.Loaded)
        {
            string name = table.Declaration.Name;
            var rows = kind.Entities.SelectMany(entity => entity.Loaded.Where(row => row.Table == name)
                                                    .Select(row => (entity.Id, Row: row))).ToList();
            using (var insert = new Inserter(connection, "loaded", "kind", "name", "table_name", "scope", "records", "rows"))
                insert.Row(id, name, table.Table.Name, table.Scope, table.Table.Records, rows.Count);
            using (var insert = new Inserter(connection, "loaded_field", "kind", "loaded", "name", "type", "published",
                                             "text", "ref_table", "ref_kind", "bank", "address", "width", "column_index",
                                             "columns"))
                foreach (FieldLayout field in table.TableFields)
                {
                    string? refTable = table.Table.Refs.GetValueOrDefault(field.Field);
                    string? refKind = refTable == null || refTable == "picture"
                        ? null
                        : kinds.FirstOrDefault(other => other.Table.Name == refTable)?.Declaration.Id;
                    insert.Row(id, name, field.Name, field.TypeName,
                               table.Published.Any(published => published.Name == field.Name) ? 1 : 0,
                               table.Text.Any(text => text.Name == field.Name) ? 1 : 0, refTable, refKind,
                               BankName(field.Array.Bank), field.Array.Address, field.Array.Width, field.Column,
                               ColumnNames(field));
                }
            using (var insert = new Inserter(connection, "loaded_part", "kind", "loaded", "name", "position", "field",
                                             "global", "same_as", "note"))
                foreach (var ((part, _, _), position) in table.Parts.Select((part, position) => (part, position)))
                    insert.Row(id, name, part.Name, position, part.Field, part.Global, part.SameAs, part.Note);
            string[] columns = ["id", "record", .. table.Published.Select(field => field.Name),
                                .. table.Parts.Count > 0 ? ["part"] : Array.Empty<string>()];
            using var values = new Inserter(connection, LoadedTableName(kind, table), columns);
            foreach ((int entity, ExportLoadedRow row) in rows)
            {
                var cells = new List<object?> { entity, row.Record };
                foreach ((FieldLayout field, (string _, long?[] elements)) in table.Published.Zip(row.Values))
                    cells.Add(field.IsArray ? JsonSerializer.Serialize(elements) : elements[0]);
                if (table.Parts.Count > 0) cells.Add(row.Part);
                values.Row([.. cells]);
            }
        }
    }

    /// <summary>A record array's element names, as a JSON array with null for an element the registry does not name
    /// (<c>[null,"sell"]</c>); null when the field is one cell or names none.</summary>
    private static string? ColumnNames(FieldLayout field)
    {
        if (!field.IsArray || field.Array.Columns.Count == 0) return null;
        string?[] names = Enumerable.Range(0, field.Elements).Select(index => field.Array.Columns.GetValueOrDefault(index))
            .ToArray();
        return JsonSerializer.Serialize(names);
    }

    private static string BankName(GlobalBank bank) => bank switch
    {
        GlobalBank.String => "string",
        GlobalBank.Float => "float",
        _ => "int",
    };

    /// <summary>One row per named field of the kind's table, per column a declaration names, and per description, so
    /// the site's templates know what each is (spec §7.2).</summary>
    private static void WriteFields(SqliteConnection connection, ExportKind kind, IReadOnlyList<ExportKind> kinds)
    {
        KindDeclaration declaration = kind.Declaration;
        var named = kind.TableFields.Concat(kind.Declared.Where(field => field.Column is not null))
            .DistinctBy(field => field.Name).ToList();
        using var insert = new Inserter(connection, "field", "kind", "name", "type", "published", "text", "filter",
                                        "ref_table", "ref_kind", "picture", "bank", "address", "width", "column_index",
                                        "columns");
        foreach (FieldLayout field in named)
        {
            string? refTable = kind.Table.Refs.GetValueOrDefault(field.Field);
            string? refKind = refTable == null || refTable == "picture"
                ? null
                : kinds.FirstOrDefault(other => other.Table.Name == refTable)?.Declaration.Id;
            string[] pictureNames = declaration.Pictures.Where(picture => picture.Field == field.Name)
                .Select(picture => picture.Name)
                .Concat(declaration.Sheets.Where(sheet => sheet.Field == field.Name).Select(sheet => sheet.Name))
                .ToArray();
            // A title that is a reference takes its target's title (game data export plan §4.1); it is no text field.
            bool text = field.IsString && (field.Name == declaration.Title || declaration.Text.Contains(field.Name));
            insert.Row(declaration.Id, field.Name, field.TypeName, declaration.Fields.Contains(field.Name) ? 1 : 0, text ? 1 : 0,
                       declaration.Filters.Contains(field.Name) ? 1 : 0, refTable, refKind,
                       pictureNames.Length == 0 ? null : string.Join(",", pictureNames), BankName(field.Array.Bank),
                       field.Array.Address, field.Array.Width, field.Column, ColumnNames(field));
        }
        foreach (DescriptionDeclaration description in declaration.Descriptions)
            insert.Row(declaration.Id, description.Name, "description", 0, 1, 0, null, null, null, null, null, null, null,
                       null);
        using (var groups = new Inserter(connection, "field_group", "kind", "name", "position", "field"))
            foreach (GroupDeclaration group in declaration.Groups)
                for (int position = 0; position < group.Fields.Count; position++)
                    groups.Row(declaration.Id, group.Name, position, group.Fields[position]);
        using (var labels = new Inserter(connection, "group_label", "kind", "name", "labels"))
            foreach (GroupDeclaration group in declaration.Groups)
                if (group.Labels != null) labels.Row(declaration.Id, group.Name, group.Labels);
        using (var blocks = new Inserter(connection, "text_block", "kind", "field", "position", "name", "first", "last"))
            foreach ((string field, IReadOnlyList<TextBlockDeclaration> list) in declaration.TextBlocks)
                for (int position = 0; position < list.Count; position++)
                    blocks.Row(declaration.Id, field, position, list[position].Name, list[position].First, list[position].Last);
        using (var notes = new Inserter(connection, "note", "kind", "name", "text"))
        {
            foreach ((string name, string note) in declaration.Notes) notes.Row(declaration.Id, name, note);
            foreach (ExportLoadedTable table in kind.Loaded)
                foreach ((string name, string note) in table.Declaration.Notes)
                    notes.Row(declaration.Id, $"{table.Declaration.Name}.{name}", note);
        }
        using (var labels = new Inserter(connection, "label", "kind", "name", "text"))
        {
            foreach ((string name, string label) in declaration.Labels) labels.Row(declaration.Id, name, label);
            foreach (ExportLoadedTable table in kind.Loaded)
                foreach ((string name, string label) in table.Declaration.Labels)
                    labels.Row(declaration.Id, $"{table.Declaration.Name}.{name}", label);
        }
        using (var formats = new Inserter(connection, "value_format", "kind", "field", "scale", "offset", "unit", "bits"))
        {
            var shown = kind.Published.Select(field => (field.Name, field.Array.Format))
                .Concat(kind.Loaded.SelectMany(table => table.Published.Select(
                    field => ($"{table.Declaration.Name}.{field.Name}", field.Array.Format))));
            foreach ((string name, ValueFormat? format) in shown)
                if (format != null)
                    formats.Row(declaration.Id, name, format.Scale, format.Offset, format.Unit, format.BitsOf != null ? 1 : 0);
        }
        using var names = new Inserter(connection, "value_name", "kind", "field", "value", "lang", "text");
        foreach (ExportValueName name in kind.ValueNames)
            names.Row(declaration.Id, name.Field, name.Value, name.Language, name.Text);
    }

    /// <summary>The SHA-256 of an entity's canonical form (spec §7.3): its values, its text in each language, references,
    /// its pictures' hashes and explicit marks, the calls its script relations found, and the rows its loaders built
    /// (written only when it has some), in the export's order.
    /// </summary>
    public static string ContentHash(ExportEntity entity)
    {
        var buffer = new MemoryStream();
        using (var json = new Utf8JsonWriter(buffer, new JsonWriterOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }))
        {
            json.WriteStartObject();
            json.WriteNumber("id", entity.Id);
            json.WriteStartArray("values");
            foreach ((string field, long?[] elements) in entity.Values)
            {
                json.WriteStartArray();
                json.WriteStringValue(field);
                foreach (long? element in elements)
                    if (element is { } number) json.WriteNumberValue(number);
                    else json.WriteNullValue();
                json.WriteEndArray();
            }
            json.WriteEndArray();
            json.WriteStartArray("text");
            foreach (ExportText text in entity.Texts)
            {
                json.WriteStartArray();
                json.WriteStringValue(text.Field);
                json.WriteNumberValue(text.Index);
                json.WriteStringValue(text.Language);
                json.WriteStringValue(text.Value);
                json.WriteEndArray();
            }
            json.WriteEndArray();
            json.WriteStartArray("refs");
            foreach (ExportRef reference in entity.Refs)
            {
                json.WriteStartArray();
                json.WriteStringValue(reference.Field);
                json.WriteNumberValue(reference.Index);
                json.WriteStringValue(reference.ToKind);
                json.WriteNumberValue(reference.ToId);
                json.WriteEndArray();
            }
            json.WriteEndArray();
            json.WriteStartArray("pictures");
            foreach ((string name, ExportPicture picture) in entity.Pictures)
            {
                json.WriteStartArray();
                json.WriteStringValue(name);
                json.WriteStringValue(picture.Hash);
                json.WriteBooleanValue(picture.Explicit);
                json.WriteEndArray();
            }
            json.WriteEndArray();
            json.WriteStartArray("relations");
            foreach (ExportRelation relation in entity.Relations)
            {
                json.WriteStartArray();
                json.WriteStringValue(relation.Relation);
                json.WriteStringValue(relation.Script);
                json.WriteNumberValue(relation.Offset);
                if (relation.Count is { } count) json.WriteNumberValue(count);
                else json.WriteNullValue();
                json.WriteEndArray();
            }
            json.WriteEndArray();
            if (entity.Loaded.Count > 0)
            {
                json.WriteStartArray("loaded");
                foreach (ExportLoadedRow row in entity.Loaded)
                {
                    json.WriteStartArray();
                    json.WriteStringValue(row.Table);
                    json.WriteNumberValue(row.Record);
                    foreach ((string field, long?[] elements) in row.Values)
                    {
                        json.WriteStartArray();
                        json.WriteStringValue(field);
                        foreach (long? element in elements)
                            if (element is { } number) json.WriteNumberValue(number);
                            else json.WriteNullValue();
                        json.WriteEndArray();
                    }
                    if (row.Part is { } part)
                    {
                        json.WriteStartArray();
                        json.WriteStringValue("#part");
                        json.WriteStringValue(part);
                        json.WriteEndArray();
                    }
                    json.WriteEndArray();
                }
                json.WriteEndArray();
            }
            json.WriteEndObject();
        }
        return Sha256Of(buffer.ToArray());
    }

    // ------------------------------------------------------------------------------------------------- the manifest

    private static void WriteManifest(string path, ExportResult result, string revision, ExportManifest manifest,
                                      IReadOnlyList<(string File, string Sha256)> files)
    {
        var buffer = new MemoryStream();
        using (var json = new Utf8JsonWriter(buffer, new JsonWriterOptions
               {
                   Indented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
               }))
        {
            json.WriteStartObject();
            json.WriteNumber("format_version", FormatVersion);
            json.WriteString("game", result.Declarations.Profile);
            json.WriteString("revision", revision);
            json.WriteString("export_time", manifest.ExportTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));
            json.WriteString("command_line", manifest.CommandLine);
            if (manifest.GameVersion is { } version) json.WriteString("game_version", version);
            else json.WriteNull("game_version");
            json.WriteString("ome_version", manifest.OmeVersion);
            json.WriteString("ome_commit", manifest.OmeCommit);
            json.WriteStartObject("files");
            foreach ((string file, string sha256) in files) json.WriteString(file, sha256);
            json.WriteEndObject();
            json.WriteEndObject();
        }
        File.WriteAllText(path, Encoding.UTF8.GetString(buffer.ToArray()) + "\n", new UTF8Encoding(false));
    }
}
