using System.Globalization;
using System.Text;
using Ome.Engine.Hosting;
using Ome.Engine.Localization;
using Ome.Engine.Model;
using Ome.Engine.Modding;
using Ome.Engine.Natives;
using Ome.Engine.Vm;
using Ome.Mods.Data;

namespace Ome.Export;

using Script = Ome.Engine.Model.Script;

/// <summary>
/// Reads a game's data through the engine (game data export spec §6): the boot with its writes recorded, each kind's
/// records, the run-time check of what may be published (§5.5), the descriptions (§5.7), the loaders (§6.3), the
/// references (§5.3), the pictures (§6.5), the script relations (§6.7) and the text in each language (§6.4).
/// <see cref="ExportWriter"/> writes the result.
/// </summary>
public static class GameDataExporter
{
    /// <summary>The export's own tables (spec §7.2), which a kind's table may not share a name with.</summary>
    public static readonly IReadOnlySet<string> FixedTables = new HashSet<string>(StringComparer.Ordinal)
    {
        "build_info", "kind", "field", "field_group", "group_label", "note", "text_block", "text", "ref", "relation",
        "picture", "cell", "snippet", "script_use", "loaded", "loaded_field", "loaded_part", "label", "value_format", "qualifier", "mark", "referenced_as",
        "value_name", "column_name",
    };

    public static ExportResult Run(ExportInputs inputs)
    {
        ExportDeclarations declarations = inputs.Declarations;
        var rule = new BuilderRule(declarations.Builders, declarations.ExcludedBuilders, declarations.Derived);
        var report = new ExportReport();
        var explicitPictures = new ExplicitPictures(declarations.ExplicitPictures);
        GameSession boot = Boot(inputs, report);
        List<Translation> languages = [.. inputs.Languages.Select(language => new Translation(
            language, inputs.Messages, declarations.TextOverrides.GetValueOrDefault(language.Tag)))];
        report.Languages.AddRange(languages.Select(language => language.Report));

        var kinds = new List<ExportKind>();
        foreach (KindDeclaration kind in declarations.Kinds)
        {
            if (FixedTables.Contains(kind.Id))
                throw new ExportFailure($"kind {kind.Id} has the name of one of the export's own tables");
            kinds.Add(ReadKind(inputs, boot, kind, rule, explicitPictures, languages, report));
        }
        foreach (LoaderDeclaration loader in declarations.Loaders)
            Load(inputs, boot, loader, kinds, rule, languages, report);
        foreach (ExportKind kind in kinds) NameValues(inputs, boot, kind, rule, languages, report);
        LinkReferences(kinds, report);
        BorrowPictures(kinds);
        if (declarations.Relations.Count > 0) FindRelations(inputs, kinds, report);
        foreach (ExportKind kind in kinds) WriteSnippets(inputs, kind, report);
        if (inputs.ScriptIndex is { } index)
            foreach (ExportKind kind in kinds)
                kind.ScriptUse.AddRange(index.UsesOf(
                    [.. kind.TableFields.Concat(kind.JoinedFields).Concat(kind.Loaded.SelectMany(loaded => loaded.TableFields))
                        .Select(field => field.Array).Distinct()
                        .Select(array => (array.Bank, array.Address, array.Address + array.Dimensions[0] * array.Width))])
                    .OrderBy(use => use.Script, StringComparer.Ordinal));
        if (inputs.Names is { } names)
            foreach (ExportKind kind in kinds) CountUnnamed(names, boot, kind, rule, report);
        report.UnmatchedExplicitPictures.AddRange(explicitPictures.Unmatched);
        return new ExportResult(declarations, kinds, report, inputs.BuildInfo) { Languages = inputs.Languages };
    }

    /// <summary>The words and names of the values a kind shows (plan §5): for each published field, its own and its
    /// loaded tables', whose global has value facets, OME's words and the game's names for its values, or for a mask's
    /// bits those of the global its bits stand for; and the game's names for a shown row of cells' columns
    /// (<c>columns_from</c>).</summary>
    private static void NameValues(ExportInputs inputs, GameSession boot, ExportKind kind, BuilderRule rule,
                                   IReadOnlyList<Translation> languages, ExportReport report)
    {
        string id = kind.Declaration.Id;
        var fields = kind.Published.Select(field => (field.Name, Field: field))
            .Concat(kind.Loaded.SelectMany(table => table.Published.Select(
                field => ($"{table.Declaration.Name}.{field.Name}", Field: field))));
        foreach ((string name, FieldLayout field) in fields)
        {
            ArrayLayout array = field.Array;
            if (field.IsArray && array.ColumnNamesFrom is { } columns)
                foreach ((int element, string language, string text) in GameNames(inputs, boot, rule, languages, report, id,
                                                                              name, columns.Global))
                    kind.ColumnNames.Add(new ExportColumnName(name, columns.First + element, language, text));
            if (array.Format is not { Names: true } format) continue;
            ValueFormat named = format;
            if (format.BitsOf is { } bits)
            {
                ArrayLayout? target = inputs.Registry.Find(bits);
                if (target is { Bank: GlobalBank.String, Dimensions.Count: 1 })
                {
                    // The game's names of the bits (export plan §5): element k names bit k, Arterial's races.
                    foreach ((int element, string language, string text) in GameNames(inputs, boot, rule, languages, report,
                                                                                  id, name, bits))
                        kind.ValueNames.Add(new ExportValueName(name, element, language, text));
                    continue;
                }
                named = target?.Format
                        ?? throw new ExportFailure($"{id}.{name}'s bits stand for the values of {bits}, which have no words");
            }
            foreach ((long value, string word) in named.Words) kind.ValueNames.Add(new ExportValueName(name, value, "", word));
            if (named.NamesFrom is not { } from) continue;
            foreach ((int element, string language, string text) in GameNames(inputs, boot, rule, languages, report, id,
                                                                          name, from.Global))
                kind.ValueNames.Add(new ExportValueName(name, from.First + element, language, text));
        }
        // A category's bits (export plan §5): a rule's word names its one bit, and the game's names of a field's bits
        // name the rule's bits from its first.
        foreach (ExportCategory category in kind.Categories)
            foreach (ExportCategoryRule categoryRule in category.Rules)
            {
                if (categoryRule.Word is { } word)
                {
                    kind.ValueNames.Add(new ExportValueName(category.Name, categoryRule.Offset, "", word));
                    continue;
                }
                foreach ((int element, string language, string text) in GameNames(inputs, boot, rule, languages, report, id,
                                                                              category.Name, categoryRule.BitsOf!))
                    kind.ValueNames.Add(new ExportValueName(category.Name, categoryRule.Offset + element, language, text));
            }
    }

    /// <summary>The strings of one of the game's tables of names, by element: each a string the boot left there,
    /// checked as a published text is, in the source language and each translation's. <paramref name="field"/> names
    /// the field they name in a refusal.</summary>
    private static IEnumerable<(int Element, string Language, string Text)> GameNames(
        ExportInputs inputs, GameSession boot, BuilderRule rule, IReadOnlyList<Translation> languages, ExportReport report,
        string kind, string field, string global)
    {
        ArrayLayout names = inputs.Registry.Find(global) is { Bank: GlobalBank.String, Dimensions.Count: 1 } list
            ? list
            : throw new ExportFailure($"{kind}.{field} is named by {global}, which is no list of strings");
        for (int element = 0; element < names.Dimensions[0]; element++)
        {
            int address = names.Address + element;
            string text = boot.GlobalStrings.GetValueOrDefault(address, "");
            if (text.Length == 0) continue;
            string? writer = boot.Writers.TryGetValue(new GlobalCell(GlobalBank.String, address), out string? script)
                ? script
                : null;
            if (RefusalOf(boot, new ExportCell(names.Name, element, GlobalBank.String, address, 0, text, writer), rule)
                is { } reason)
            {
                report.Refusals.Add(new Refusal(kind, 0, $"{field}:{names.Name}", element, GlobalBank.String, address,
                                                reason));
                continue;
            }
            yield return (element, ExportWriter.SourceLanguage, text);
            if (!boot.StringStores.TryGetValue(address, out TextManager.StringStore store)) continue;
            foreach (Translation language in languages)
                if (language.String(text, store.Origin) is { } translated)
                    yield return (element, language.Tag, translated);
        }
    }

    /// <summary>The integer cells a kind's builders (the writers of its cells) wrote that no global of the registry
    /// names (spec §6.6, plan §4.3): an array the registry has yet to name. Reported, not published.</summary>
    private static void CountUnnamed(GlobalNames names, GameSession boot, ExportKind kind, BuilderRule rule,
                                     ExportReport report)
    {
        // A derived script wrote some of the kind's cells, and is no builder: what else it wrote is play state.
        var builders = kind.Entities.SelectMany(entity => entity.Cells).Select(cell => cell.Writer).OfType<string>()
            .Where(writer => !rule.Derives(writer)).ToHashSet(StringComparer.Ordinal);
        int[] cells = boot.Writers.Where(pair => pair.Key.Bank == GlobalBank.Integer && builders.Contains(pair.Value)
                                                 && names.NameOf(pair.Key) == null)
            .Select(pair => pair.Key.Address).Order().ToArray();
        if (cells.Length > 0) report.UnnamedCells[kind.Declaration.Id] = cells;
    }

    /// <summary>Each script relation's calls in every script of the install (spec §6.7, plan §4.5), on the entities
    /// their keys name; a key no entity has is reported.</summary>
    private static void FindRelations(ExportInputs inputs, List<ExportKind> kinds, ExportReport report)
    {
        ScriptIndex index = inputs.ScriptIndex
                            ?? throw new ExportFailure("script relations are declared, and the export has no script index "
                                                       + "to find them in");
        foreach (RelationDeclaration relation in inputs.Declarations.Relations)
        {
            ExportKind kind = kinds.SingleOrDefault(kind => kind.Declaration.Id == relation.Kind)
                              ?? throw new ExportFailure($"script relation {relation.Id} names kind {relation.Kind}, which "
                                                         + "is not declared");
            var entities = kind.Entities.ToDictionary(entity => (long)entity.Id);
            (RelationScan scan, List<(long Key, ExportRelation Relation)> found) =
                ScriptRelations.Find(relation, index, inputs.Table, inputs.Registry,
                                     id => inputs.Scripts.GetById(id)?.Name);
            foreach ((long key, ExportRelation row) in found)
                if (entities.TryGetValue(key, out ExportEntity? entity)) entity.Relations.Add(row);
                else scan.Keyless.Add((row.Script, row.Offset, key));
            scan.Rows = found.Count - scan.Keyless.Count;
            report.Relations.Add(scan);
        }
        foreach (ExportEntity entity in kinds.SelectMany(kind => kind.Entities))
        {
            List<ExportRelation> sorted = [.. entity.Relations.OrderBy(row => row.Relation, StringComparer.Ordinal)
                                                   .ThenBy(row => row.Script, StringComparer.Ordinal)
                                                   .ThenBy(row => row.Offset)];
            entity.Relations.Clear();
            entity.Relations.AddRange(sorted);
        }
    }

    // ------------------------------------------------------------------------------------------------- the boot (§6.2)

    private static GameSession Boot(ExportInputs inputs, ExportReport report)
    {
        ExportDeclarations declarations = inputs.Declarations;
        if (ScriptNames.Normalized(declarations.BootScript) != inputs.BootStop.Script)
            throw new ExportFailure($"{declarations.ReadAt} lies in {inputs.BootStop.Script}, not in the boot script {declarations.BootScript}");
        Script script = inputs.ScriptByName(declarations.BootScript)
                        ?? throw new ExportFailure($"the install has no boot script {declarations.BootScript}");
        var session = new GameSession();
        var options = new VmOptions(MaxSteps: inputs.BootMaxSteps)
        {
            ProfileId = inputs.GameId, RecordsWrites = true, StopAt = inputs.BootStop,
        };
        SceneResult result = session.RunScene(script, inputs.Table, new HeadlessWaits(), options, inputs.Scripts,
                                              build: inputs.Build);
        report.BootHalt = result.Halt ?? "";
        if (result.Halt != inputs.BootStop.HaltReason)
            throw new ExportFailure($"the boot ended with {result.Halt ?? "no halt"}, before {declarations.ReadAt} "
                                    + $"({inputs.BootStop.Script}@0x{inputs.BootStop.Offset:x})");
        return session;
    }

    // ------------------------------------------------------------------------------------------------- one kind

    private static ExportKind ReadKind(ExportInputs inputs, GameSession boot, KindDeclaration kind, BuilderRule rule,
                                       ExplicitPictures explicitPictures, IReadOnlyList<Translation> languages,
                                       ExportReport report)
    {
        RegistryView registry = inputs.Registry;
        TableLayout table = registry.Table(kind.Table);
        IReadOnlyList<FieldLayout> tableFields = registry.FieldsOf(table);
        FieldLayout exists = registry.Resolve(table, kind.Exists);
        var skips = kind.Skip.Select(skip => (Field: registry.Resolve(table, skip.Field), skip.Value)).ToList();
        // The tables the kind joins (export plan §5): record r of each is entity r (the registry's records_of), or
        // record r ÷ per of the table that owns the kind's records (its owned_by: a Verita title is character r ÷ 20's),
        // and the kind publishes their fields as its own, written table.field. An entity past a joined table's records
        // has none of its cells.
        var joins = new Dictionary<string, TableLayout>(StringComparer.Ordinal);
        var perJoined = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (string name in kind.Joins)
        {
            TableLayout joined = registry.Table(name);
            if (joined.RecordsOf == table.Name) perJoined[name] = 1;
            else if (table.OwnedBy is { } owner && owner.Table == name) perJoined[name] = owner.Per;
            else
                throw new ExportFailure($"{kind.Id} joins table {name}, whose records are not {table.Name}'s (its "
                                        + $"records_of), and {table.Name} is not owned_by it");
            joins[name] = joined;
        }
        IReadOnlyList<FieldLayout> joinedFields = [.. joins.SelectMany(pair => registry.FieldsOf(pair.Value).Select(
            field => field with { Name = $"{pair.Key}.{field.Name}", Joined = pair.Key }))];
        FieldLayout Resolve(string reference)
        {
            int dot = reference.IndexOf('.', StringComparison.Ordinal);
            return dot > 0 && joins.TryGetValue(reference[..dot], out TableLayout? joined)
                ? registry.Resolve(joined, reference[(dot + 1)..]) with { Name = reference, Joined = reference[..dot] }
                : registry.Resolve(table, reference);
        }
        IReadOnlyList<FieldLayout> published = kind.Fields.Select(Resolve).ToList();
        IReadOnlyList<ExportCategory> categories = Categories(inputs, kind, published);
        var texts = new List<string>();
        // A title is a string field, a reference whose target's title the entity takes, or none (plan §4.1).
        if (kind.Title is { } title && registry.Resolve(table, title).IsString) texts.Add(title);
        texts.AddRange(kind.Text.Where(text => text != kind.Title));
        IReadOnlyList<FieldLayout> textFields = texts.Select(text => registry.Resolve(table, text)).ToList();
        var textArrays = new HashSet<string>(textFields.Select(field => field.Field), StringComparer.Ordinal);
        // A frame's size is two fields of the record, and its blend a third (export plan §5): a map object's sheet, its
        // first frame, and the mode DRAWOBJ draws it with.
        var pictures = kind.Pictures.Select(picture => (picture.Name, Field: registry.Resolve(table, picture.Field), picture.Key,
                                                        Frame: picture.Frame is { } frame
                                                            ? (registry.Resolve(table, frame.Width), registry.Resolve(table, frame.Height))
                                                            : ((FieldLayout Width, FieldLayout Height)?)null,
                                                        picture.Frames,
                                                        Blend: picture.Blend is { } blend ? registry.Resolve(table, blend) : null,
                                                        Where: picture.Where.Select(rule => (Field: registry.Resolve(table, rule.Field), rule.Values)).ToList()))
            .ToList();
        // A sheet without a field is numbered by the record's own id.
        var sheets = kind.Sheets
            .Select(sheet => (Sheet: sheet, Field: sheet.Field is null ? null : registry.Resolve(table, sheet.Field))).ToList();
        IReadOnlyList<FieldLayout> sheetFields = [.. sheets.Select(pair => pair.Field).OfType<FieldLayout>()];
        IReadOnlyList<(string Name, FieldLayout Field)> pictureParts =
        [
            .. pictures.Where(picture => picture.Frame is not null)
                .SelectMany(picture => new[] { (picture.Name, picture.Frame!.Value.Width), (picture.Name, picture.Frame!.Value.Height) }),
            .. pictures.Where(picture => picture.Blend is not null).Select(picture => (picture.Name, picture.Blend!)),
            .. pictures.SelectMany(picture => picture.Where.Select(rule => (picture.Name, rule.Field))),
        ];
        IReadOnlyList<FieldLayout> frameFields = [.. pictureParts.Select(part => part.Field)];
        foreach ((string name, FieldLayout field) in pictures.Select(picture => (picture.Name, picture.Field))
                     .Concat(sheets.Where(pair => pair.Field is not null).Select(pair => (pair.Sheet.Name, pair.Field!)))
                     .Concat(pictureParts))
            if (field.Elements != 1 || field.IsString)
                throw new ExportFailure($"{kind.Id}'s picture {name} ({field.Name}) is not one integer cell of a record");

        // The snippet sets what the site shows of a record (plan §4.3): its published fields, its text and its pictures,
        // and the second cell of each two-field reference, without which a modder could not change the record it names
        // (a card's slot beside its row).
        IReadOnlyList<FieldLayout> snippetFields = [.. published, .. KeyPartners(published, tableFields, joinedFields, table, joins),
                                                    .. textFields, .. pictures.Select(picture => picture.Field),
                                                    .. sheetFields, .. frameFields];
        var entities = new List<ExportEntity>();
        (int first, int last) = kind.KeyRange ?? (0, table.Records - 1);
        for (int record = first; record <= Math.Min(last, table.Records - 1); record++)
        {
            if (Read(boot, exists, record, 0).IsDefault) continue;
            if (skips.Any(skip => Read(boot, skip.Field, record, 0).Number == skip.Value)) continue;
            var entity = new ExportEntity(record);

            // Every named field's cells, each checked once (§5.5, §6.6): an integer's value is shown, so a value no
            // builder wrote is refused; a string is shown only when the kind publishes it as text; and a derived
            // script's cell is withheld, its writer shown (export plan §5).
            // Keyed by bank and address: a string array and an integer array may share addresses in their banks.
            var cells = new Dictionary<GlobalCell, (ExportCell Cell, bool Refused)>();
            // The record of the field's table that holds the entity's cells: a joined table's is r ÷ its per.
            int RecordOf(FieldLayout field) => field.Joined is { } joined ? record / perJoined[joined] : record;
            bool Joins(FieldLayout field) => field.Joined is not { } joined || RecordOf(field) < joins[joined].Records;
            foreach (FieldLayout field in tableFields.Concat(joinedFields.Where(Joins)))
            {
                bool publishedText = field.Joined == null && textArrays.Contains(field.Field);
                for (int element = 0; element < field.Elements; element++)
                {
                    ExportCell cell = Read(boot, field, RecordOf(field), element);
                    bool derived = rule.Derives(cell.Writer);
                    bool shown = (!field.IsString || publishedText) && !derived;
                    string? reason = shown ? RefusalOf(boot, cell, rule) : null;
                    if (reason != null)
                        report.Refusals.Add(new Refusal(kind.Id, record, field.Name, element, cell.Bank, cell.Address, reason));
                    cells[new GlobalCell(cell.Bank, cell.Address)] = (cell, reason != null || derived);
                    if (cell.Writer != null || !cell.IsDefault)
                        entity.Cells.Add(new ExportModderCell(field.Name, element, cell.Bank, cell.Address,
                            reason != null || !shown ? null : cell.Text ?? cell.Number.ToString(CultureInfo.InvariantCulture),
                            cell.Writer));
                }
            }
            // A joined field of a record its table does not have is withheld, as a refused cell is.
            (ExportCell Cell, bool Refused) At(FieldLayout field, int element)
                => Joins(field)
                    ? cells[new GlobalCell(field.Array.Bank, field.Cell(RecordOf(field), element))]
                    : (new ExportCell(field.Name, element, field.Array.Bank, 0, 0, null, null), true);

            foreach (FieldLayout field in published)
            {
                var values = new long?[field.Elements];
                for (int element = 0; element < field.Elements; element++)
                {
                    var (cell, refused) = At(field, element);
                    values[element] = refused ? null : cell.Number;
                }
                entity.Values.Add((field.Name, values));
                TableLayout owner = field.Joined is { } joined ? joins[joined] : table;
                if (owner.RefKeys.TryGetValue(field.Field, out RefKey? key))
                {
                    var (ids, rows) = KeyedIds(field, field.Joined == null ? tableFields
                                                   : [.. joinedFields.Where(other => other.Joined == field.Joined)],
                                               key, At);
                    entity.KeyedRefs[field.Name] = ids;
                    if (rows != null) entity.KeyedRows[field.Name] = rows;
                }
            }
            // Its categories (export plan §5), after its published fields, as the writer reads them.
            foreach (ExportCategory category in categories) entity.Values.Add((category.Name, [CategoryOf(category, At)]));
            // A string is translated at its origin, which stands for the site the game draws it at (plan §4.6).
            foreach (FieldLayout field in textFields)
                for (int element = 0; element < field.Elements; element++)
                {
                    var (cell, refused) = At(field, element);
                    if (refused || cell.Text is not { Length: > 0 } text) continue;
                    entity.Texts.Add(new ExportText(field.Name, element, text));
                    if (!boot.StringStores.TryGetValue(cell.Address, out TextManager.StringStore store)) continue;
                    foreach (Translation language in languages)
                        if (language.String(text, store.Origin) is { } translated)
                            entity.Texts.Add(new ExportText(field.Name, element, translated, language.Tag));
                }
            foreach ((string name, FieldLayout field, int? key, (FieldLayout Width, FieldLayout Height)? frame,
                      int? frames, FieldLayout? blend, List<(FieldLayout Field, IReadOnlyList<long> Values)> where) in pictures)
            {
                var (cell, refused) = At(field, 0);
                if (refused || cell.Number == 0 || inputs.Pictures == null) continue;
                // A record has the picture only where each field of its where holds one of the values; a withheld one
                // leaves it out.
                if (where.Any(rule => At(rule.Field, 0) is var (value, withheld) && (withheld || !rule.Values.Contains(value.Number))))
                    continue;
                // Op 0x203's mode 1, the additive glow, as the record's blend gives it; a withheld blend leaves the
                // picture out.
                (ExportCell Cell, bool Refused)? mode = blend is null ? null : At(blend, 0);
                if (mode is { Refused: true }) continue;
                bool additive = mode?.Cell.Number == 1;
                ExportPicture? picture;
                if (frame is { } sizes)
                {
                    // The first frame of the record's sheet, as wide and as high as its fields give; a size a builder
                    // did not write, or of 0, is no frame.
                    var (width, widthRefused) = At(sizes.Width, 0);
                    var (height, heightRefused) = At(sizes.Height, 0);
                    if (widthRefused || heightRefused) continue;
                    picture = width.Number is > 0 and <= int.MaxValue && height.Number is > 0 and <= int.MaxValue
                        ? inputs.Pictures.Frame(cell.Number, ((int)width.Number, (int)height.Number), key, additive)
                        : null;
                }
                // The first of a strip's frames side by side (a Verita title's chibi): its width ÷ frames, its height.
                else if (frames is { } count)
                    picture = (key is { } colour ? inputs.Pictures.ResolveKeyed(cell.Number, colour) : inputs.Pictures.Resolve(cell.Number))
                        is { } strip
                        ? inputs.Pictures.Frame(cell.Number, (strip.Width / count, strip.Height), key, additive)
                        : null;
                else if (additive) picture = inputs.Pictures.Frame(cell.Number, null, key, additive);
                else picture = key is { } colour ? inputs.Pictures.ResolveKeyed(cell.Number, colour) : inputs.Pictures.Resolve(cell.Number);
                if (additive && picture is not null) picture = picture with { Additive = true };
                if (picture is not null) entity.Pictures.Add((name, explicitPictures.Mark(picture)));
                else report.UnresolvedPictures.Add((kind.Id, record, name, cell.Number));
            }
            // A cell of a sheet (plan §5): a number below the sheet's first, or above its last, is no picture.
            foreach ((SheetPictureDeclaration sheet, FieldLayout? field) in sheets)
            {
                (ExportCell? cell, bool refused) = field is null ? (null, false) : At(field, 0);
                long number = cell?.Number ?? record;
                if (refused || inputs.Pictures == null || sheet.Cell(number) is not { } at) continue;
                if (inputs.Pictures.Cell(sheet.Sheet, at.X, at.Y, sheet.Size.Width, sheet.Size.Height, sheet.Key)
                    is { } picture)
                    entity.Pictures.Add((sheet.Name, explicitPictures.Mark(picture)));
                else report.UnresolvedPictures.Add((kind.Id, record, sheet.Name, number));
            }
            foreach (DescriptionDeclaration description in kind.Descriptions)
                Describe(inputs, boot, kind, entity, description, At, table, languages, report);
            DataValue? SnippetValue(FieldLayout field, int column)
            {
                var (cell, refused) = At(field, column);
                return refused ? null : field.IsString ? new DataString(cell.Text ?? "") : new DataInteger(cell.Number);
            }
            entity.SnippetRecord.AddRange(SnippetEntries(tableFields, snippetFields, SnippetValue));
            // A joined table's record is a record of its own table in a data file.
            foreach ((string name, TableLayout joined) in joins)
            {
                int joinedRecord = record / perJoined[name];
                if (joinedRecord >= joined.Records) continue;
                List<KeyValuePair<string, DataValue>> fields = SnippetEntries(
                    [.. joinedFields.Where(field => field.Joined == name)],
                    [.. snippetFields.Where(field => field.Joined == name)], SnippetValue);
                if (fields.Count > 0) entity.SnippetJoined.Add((name, joinedRecord, fields));
            }
            entities.Add(entity);
        }
        var declared = published.Concat(textFields).Concat(pictures.Select(picture => picture.Field))
            .Concat(sheetFields).Concat(skips.Select(skip => skip.Field)).Append(exists)
            .DistinctBy(field => field.Name).ToList();
        return new ExportKind(kind, table, tableFields, published, declared, entities)
        {
            Joins = joins, JoinedFields = joinedFields, Categories = categories,
        };
    }

    /// <summary>A kind's categories (export plan §5), each rule's categories being the mask's next bits: a word's one,
    /// and a field's as many as the game has names for its bits.</summary>
    private static IReadOnlyList<ExportCategory> Categories(ExportInputs inputs, KindDeclaration kind,
                                                            IReadOnlyList<FieldLayout> published)
    {
        var categories = new List<ExportCategory>();
        foreach (CategoryDeclaration category in kind.Categories)
        {
            var rules = new List<ExportCategoryRule>();
            int offset = 0;
            foreach (CategoryRule rule in category.Rules)
            {
                FieldLayout field = published.FirstOrDefault(other => other.Name == rule.Field)
                                    ?? throw new ExportFailure($"{kind.Id}'s category {category.Name} reads {rule.Field}, "
                                                               + "which the kind does not publish");
                if (field.Elements != 1 || field.IsString)
                    throw new ExportFailure($"{kind.Id}'s category {category.Name} reads {rule.Field}, which is not one "
                                            + "integer cell of a record");
                if (rule.Word is { } word)
                {
                    rules.Add(new ExportCategoryRule(field, word, offset, 1, null));
                    offset += 1;
                    continue;
                }
                string bits = field.Array.Format?.BitsOf
                              ?? throw new ExportFailure($"{kind.Id}'s category {category.Name}: {rule.Field}'s bits have "
                                                         + "no names, so its rule needs a word");
                int count = inputs.Registry.Find(bits) is { Bank: GlobalBank.String, Dimensions.Count: 1 } names
                    ? names.Dimensions[0]
                    : throw new ExportFailure($"{kind.Id}'s category {category.Name}: {rule.Field}'s bits stand for the "
                                              + $"values of {bits}, which is no list of the game's strings");
                rules.Add(new ExportCategoryRule(field, null, offset, count, bits));
                offset += count;
            }
            if (offset > 62)
                throw new ExportFailure($"{kind.Id}'s category {category.Name} has {offset} categories, more than a mask "
                                        + "of 62 bits holds");
            categories.Add(new ExportCategory(category.Name, rules));
        }
        return categories;
    }

    /// <summary>An entity's mask in a category (export plan §5): the categories of the first rule that places it, 0 for
    /// none, or null where a rule reads a withheld cell before one places it.</summary>
    private static long? CategoryOf(ExportCategory category, Func<FieldLayout, int, (ExportCell Cell, bool Refused)> at)
    {
        foreach (ExportCategoryRule rule in category.Rules)
        {
            var (cell, refused) = at(rule.Field, 0);
            if (refused) return null;
            long value = cell.Number;
            if (rule.Word is not null)
            {
                if (value > 0 && value != rule.Field.Array.Format?.None) return 1L << rule.Offset;
                continue;
            }
            long set = value & ((1L << rule.Bits) - 1);
            if (set != 0) return set << rule.Offset;
        }
        return 0;
    }

    private static ExportCell Read(GameSession session, FieldLayout field, int record, int element)
    {
        int address = field.Cell(record, element);
        GlobalBank bank = field.Array.Bank;
        string? writer = session.Writers.TryGetValue(new GlobalCell(bank, address), out string? script) ? script : null;
        return bank switch
        {
            GlobalBank.String => new ExportCell(field.Field, element, bank, address, 0,
                                                session.GlobalStrings.GetValueOrDefault(address, ""), writer),
            GlobalBank.Float => new ExportCell(field.Field, element, bank, address,
                                               session.GlobalFloats.GetValueOrDefault(address), null, writer),
            _ => new ExportCell(field.Field, element, bank, address, session.Globals.GetValueOrDefault(address), null, writer),
        };
    }

    /// <summary>Why a shown cell may not be published (spec §5.5), or null: its writer is no builder, or its string's
    /// origin is in none or missing. A cell no script wrote holds its default and is published as such.</summary>
    private static string? RefusalOf(GameSession session, ExportCell cell, BuilderRule rule)
    {
        if (cell.Writer == null) return cell.IsDefault ? null : "no script wrote it";
        if (!rule.Allows(cell.Writer)) return $"written by {cell.Writer}, which is no builder";
        if (cell.Text is not { Length: > 0 }) return null;
        if (!session.StringStores.TryGetValue(cell.Address, out TextManager.StringStore store))
            return "a string with no origin";
        return rule.Allows(store.Origin.Script) ? null : $"a string stored at {store.Origin}, which is in no builder";
    }

    // ------------------------------------------------------------------------------------------------- loaders (§6.3)

    /// <summary>Runs <paramref name="loader"/> for each entity of its key kind in its key range (export plan §4.2, §5):
    /// from a copy of the booted session, once for each step, with its seeds, to its point; then reads its published
    /// tables' records that exist, each shown cell checked as a kind's are (§5.5), with the loader's own builders beside
    /// the export's. Run k's records are numbered k times the table's records plus the record. A published text is
    /// translated at its origin, as a kind's (plan §4.6), and a literal gives a record's text from a script.</summary>
    private static void Load(ExportInputs inputs, GameSession boot, LoaderDeclaration loader, IReadOnlyList<ExportKind> kinds,
                             BuilderRule exportRule, IReadOnlyList<Translation> languages, ExportReport report)
    {
        string where = $"loader {loader.Name}";
        ExportKind keys = kinds.FirstOrDefault(kind => kind.Declaration.Id == loader.Keys)
                          ?? throw new ExportFailure($"{where}: its keys {loader.Keys} is no kind");
        if (inputs.StopOf is null) throw new ExportFailure($"{where} reads at {loader.ReadAt}, and no patch point is resolved");
        RunStop stop = inputs.StopOf(loader.ReadAt);
        Script script = inputs.ScriptByName(loader.Script)
                        ?? throw new ExportFailure($"{where} runs {loader.Script}, which the install lacks");
        if (ScriptNames.Normalized(script.Name) != stop.Script)
            throw new ExportFailure($"{where} reads at {loader.ReadAt}, which lies in {stop.Script}, not in {loader.Script}");
        if (loader.Steps < 1) throw new ExportFailure($"{where} runs {loader.Steps} steps");
        RegistryView registry = inputs.Registry;
        BuilderRule rule = exportRule.With(loader.Builders);
        var skips = loader.Skip.Select(skip => (Field: registry.Resolve(keys.Table, skip.Field), skip.Value)).ToList();
        var seeds = loader.Seed.Select(seed => (Seed: seed, Array: registry.IntegerArray(seed.Global))).ToList();
        var tables = new List<ExportLoadedTable>();
        var literals = new List<(ExportLoadedTable Table, LiteralDeclaration Literal, List<ExportText> Texts)>();
        foreach (LoadedTableDeclaration declared in loader.Tables)
        {
            if (keys.Table.Fields.Any(field => field.Field == declared.Name))
                throw new ExportFailure($"{where}: table {declared.Name} has the name of a field of {keys.Table.Name}");
            TableLayout table = registry.Table(declared.Table);
            var parts = new List<(PartDeclaration, FieldLayout, ArrayLayout)>();
            foreach (PartDeclaration part in declared.Parts)
            {
                FieldLayout field = registry.Resolve(table, part.Field);
                ArrayLayout global = registry.IntegerArray(part.Global);
                if (field.Elements != 1 || field.IsString)
                    throw new ExportFailure($"{where}: table {declared.Name}'s part {part.Name} reads {part.Field}, which "
                                            + "is not one integer cell of a record");
                if (global.Dimensions.Count != 1 || part.SameAs < 0 || part.SameAs >= global.Dimensions[0])
                    throw new ExportFailure($"{where}: table {declared.Name}'s part {part.Name} compares with cell "
                                            + $"{part.SameAs} of {part.Global}, which is no cell of a list");
                parts.Add((part, field, global));
            }
            // The tables a row's reference reaches (plan §5): the record it names publishes its fields as the row's,
            // written table.field (a dig site's dig table's items).
            var joins = new Dictionary<string, (TableLayout Table, FieldLayout Through)>(StringComparer.Ordinal);
            foreach ((string joinedName, string through) in declared.Joins)
            {
                FieldLayout field = registry.Resolve(table, through);
                if (field.Elements != 1 || table.Refs.GetValueOrDefault(field.Field) != joinedName)
                    throw new ExportFailure($"{where}: table {declared.Name} joins {joinedName} through {through}, which "
                                            + $"is not one cell that refers to {joinedName}");
                joins[joinedName] = (registry.Table(joinedName), field);
            }
            FieldLayout Resolve(string reference)
            {
                int dot = reference.IndexOf('.', StringComparison.Ordinal);
                return dot > 0 && joins.TryGetValue(reference[..dot], out var joined)
                    ? registry.Resolve(joined.Table, reference[(dot + 1)..]) with { Name = reference, Joined = reference[..dot] }
                    : registry.Resolve(table, reference);
            }
            var loaded = new ExportLoadedTable(declared, loader.Scope, table, registry.FieldsOf(table),
                                               declared.Fields.Select(Resolve).ToList(),
                                               registry.Resolve(table, declared.Exists))
            {
                Text = declared.Text.Select(field => registry.Resolve(table, field)).ToList(),
                Parts = parts,
                Where = declared.Where.Select(rule => (registry.Resolve(table, rule.Field),
                                                       (IReadOnlySet<long>)rule.Values.ToHashSet())).ToList(),
                Joins = joins,
                JoinedFields = [.. joins.SelectMany(pair => registry.FieldsOf(pair.Value.Table).Select(
                    field => field with { Name = $"{pair.Key}.{field.Name}", Joined = pair.Key }))],
            };
            if (loaded.Where.Any(rule => rule.Field.Elements != 1 || rule.Field.IsString))
                throw new ExportFailure($"{where}: table {declared.Name}'s where reads a field that is not one integer "
                                        + "cell of a record");
            if (loaded.Published.Any(field => field.IsString))
                throw new ExportFailure($"{where}: table {declared.Name} publishes a string among its fields; text is "
                                        + "published as text");
            if (loaded.Text.Any(field => !field.IsString))
                throw new ExportFailure($"{where}: table {declared.Name}'s text names a field that holds no string");
            tables.Add(loaded);
            foreach (LiteralDeclaration literal in declared.Literals)
                literals.Add((loaded, literal, LiteralTexts(inputs, literal, languages, where)));
        }
        keys.Loaded.AddRange(tables);

        foreach (ExportEntity entity in keys.Entities)
        {
            if (loader.KeyRange is { } range && (entity.Id < range.First || entity.Id > range.Last)) continue;
            if (skips.Any(skip => Read(boot, skip.Field, entity.Id, 0).Number == skip.Value)) continue;
            for (int step = 0; step < loader.Steps; step++)
            {
                GameSession run = boot.CopyState();
                foreach ((SeedCell seed, ArrayLayout array) in seeds)
                    foreach (int address in SeedAddresses(array, seed.Index, entity.Id, step, where))
                        run.Seed(address, seed.Value.For(entity.Id));
                var options = new VmOptions(MaxSteps: inputs.LoaderMaxSteps)
                {
                    ProfileId = inputs.GameId, RecordsWrites = true, StopAt = stop,
                };
                SceneResult result = run.RunScene(script, inputs.Table, new HeadlessWaits(), options, inputs.Scripts,
                                                  build: inputs.Build);
                report.LoaderRuns++;
                if (result.Halt != stop.HaltReason)
                {
                    report.LoaderHalts.Add((loader.Name, keys.Declaration.Id, entity.Id, result.Halt ?? ""));
                    continue;
                }
                foreach (ExportLoadedTable table in tables)
                    ReadLoadedTable(run, table, step, keys, entity, rule, languages, report);
                // A literal stands for a record's text whatever the run left there (plan §5).
                foreach ((ExportLoadedTable table, LiteralDeclaration literal, List<ExportText> texts) in literals)
                {
                    string field = $"{table.Declaration.Name}.{literal.Field}";
                    int index = step * table.Table.Records + literal.Record;
                    entity.Texts.RemoveAll(text => text.Field == field && text.Index == index);
                    entity.Texts.AddRange(texts.Select(text => text with { Field = field, Index = index }));
                }
            }
        }
    }

    /// <summary>One run's records of <paramref name="table"/> that exist, under the key entity: its fields' values, its
    /// text in each language, its cells for the modder data, and its snippet rows when a data file reaches it (a scope's
    /// table).</summary>
    private static void ReadLoadedTable(GameSession run, ExportLoadedTable table, int step, ExportKind keys,
                                        ExportEntity entity, BuilderRule rule, IReadOnlyList<Translation> languages,
                                        ExportReport report)
    {
        string name = table.Declaration.Name;
        var textArrays = new HashSet<string>(table.Text.Select(field => field.Field), StringComparer.Ordinal);
        for (int record = 0; record < table.Table.Records; record++)
        {
            if (Read(run, table.Exists, record, 0).IsDefault) continue;
            // A record whose field holds none of its where's values is no row (plan §5): a map object that is no dig site.
            if (table.Where.Any(rule => !rule.Values.Contains(Read(run, rule.Field, record, 0).Number))) continue;
            int row = step * table.Table.Records + record;
            // Every field's cells, as a kind's (§6.6): an integer's value is shown and checked, and so is a published
            // text; another string, and a derived script's cell, shows its address and writer only. Its cells are the
            // key entity's, the field written <table>.<field> and the index the row times the width plus the element.
            var cells = new Dictionary<GlobalCell, (ExportCell Cell, bool Refused)>();
            foreach (FieldLayout field in table.TableFields)
            {
                bool text = !field.IsString || textArrays.Contains(field.Field);
                for (int element = 0; element < field.Elements; element++)
                {
                    ExportCell cell = Read(run, field, record, element);
                    bool derived = rule.Derives(cell.Writer);
                    bool shown = text && !derived;
                    string? reason = shown ? RefusalOf(run, cell, rule) : null;
                    int index = row * field.Elements + element;
                    if (reason != null)
                        report.Refusals.Add(new Refusal(keys.Declaration.Id, entity.Id, $"{name}.{field.Field}", index,
                                                        cell.Bank, cell.Address, reason));
                    cells[new GlobalCell(cell.Bank, cell.Address)] = (cell, reason != null || derived);
                    if (cell.Writer != null || !cell.IsDefault)
                        entity.Cells.Add(new ExportModderCell($"{name}.{field.Field}", index, cell.Bank, cell.Address,
                            reason != null || !shown ? null : cell.Text ?? cell.Number.ToString(CultureInfo.InvariantCulture),
                            cell.Writer));
                }
            }
            // The record each joined table's reference names (plan §5), or null for none: a joined field of none, or of
            // a record past its table, is withheld, as a refused cell is.
            var joinedRecords = new Dictionary<string, int?>(StringComparer.Ordinal);
            foreach ((string joinedName, (TableLayout joinedTable, FieldLayout through)) in table.Joins)
            {
                long? id = table.Table.RefKeys.TryGetValue(through.Field, out RefKey? throughKey)
                    ? KeyedIds(through, table.TableFields, throughKey, OwnAt).Ids[0]
                    : OwnAt(through, 0) is var (cell, refused) && !refused ? cell.Number : (long?)null;
                joinedRecords[joinedName] = id is { } r && r > 0 && r < joinedTable.Records ? (int)r : null;
            }
            (ExportCell Cell, bool Refused) OwnAt(FieldLayout field, int element)
                => cells[new GlobalCell(field.Array.Bank, field.Cell(record, element))];
            (ExportCell Cell, bool Refused) At(FieldLayout field, int element)
            {
                if (field.Joined is not { } joined) return OwnAt(field, element);
                if (joinedRecords[joined] is not { } at)
                    return (new ExportCell(field.Name, element, field.Array.Bank, 0, 0, null, null), true);
                ExportCell cell = Read(run, field, at, element);
                string? reason = RefusalOf(run, cell, rule);
                if (reason != null)
                    report.Refusals.Add(new Refusal(keys.Declaration.Id, entity.Id, $"{name}.{field.Name}",
                                                    row * field.Elements + element, cell.Bank, cell.Address, reason));
                return (cell, reason != null || rule.Derives(cell.Writer));
            }

            var values = new List<(string Field, long?[] Values)>();
            var keyed = new Dictionary<string, long?[]>(StringComparer.Ordinal);
            var keyedRows = new Dictionary<string, long?[]>(StringComparer.Ordinal);
            foreach (FieldLayout field in table.Published)
            {
                var elements = new long?[field.Elements];
                for (int element = 0; element < field.Elements; element++)
                {
                    var (cell, refused) = At(field, element);
                    elements[element] = refused ? null : cell.Number;
                }
                values.Add((field.Name, elements));
                if (table.TableOf(field).RefKeys.TryGetValue(field.Field, out RefKey? key))
                {
                    var (ids, rows) = KeyedIds(field, field.Joined == null ? table.TableFields
                                                   : [.. table.JoinedFields.Where(other => other.Joined == field.Joined)],
                                               key, At);
                    keyed[field.Name] = ids;
                    if (rows != null) keyedRows[field.Name] = rows;
                }
            }
            entity.Loaded.Add(new ExportLoadedRow(name, row, values)
            {
                Part = PartOf(run, table, At, keys, entity, rule, report), KeyedRefs = keyed, KeyedRows = keyedRows,
            });
            foreach (FieldLayout field in table.Text)
                for (int element = 0; element < field.Elements; element++)
                {
                    var (cell, refused) = At(field, element);
                    if (refused || cell.Text is not { Length: > 0 } text) continue;
                    string textField = $"{name}.{field.Name}";
                    int index = row * field.Elements + element;
                    entity.Texts.Add(new ExportText(textField, index, text));
                    if (!run.StringStores.TryGetValue(cell.Address, out TextManager.StringStore store)) continue;
                    foreach (Translation language in languages)
                        if (language.String(text, store.Origin) is { } translated)
                            entity.Texts.Add(new ExportText(textField, index, translated, language.Tag));
                }
            // A data file reaches a scope's table only (plan §5): a table with none has no snippet rows.
            if (table.Scope is not { } scope) continue;
            entity.SnippetRows.Add((scope, name, row,
                SnippetEntries(table.TableFields, [.. table.Published, .. KeyPartners(table.Published, table.TableFields,
                                                       table.JoinedFields, table.Table,
                                                       table.Joins.ToDictionary(pair => pair.Key, pair => pair.Value.Table)),
                                                   .. table.Text],
                               (field, column) =>
                {
                    var (cell, refused) = At(field, column);
                    return refused ? null : field.IsString ? new DataString(cell.Text ?? "") : new DataInteger(cell.Number);
                })));
        }
    }

    /// <summary>The part of its table a loaded record is in (export plan §5): the first part whose global, at the
    /// index the record's field holds, is not 0 and equals the global's cell the part names; null for none. The cells
    /// it compares are checked as a published value is: a refused one is reported and decides nothing.</summary>
    private static string? PartOf(GameSession run, ExportLoadedTable table,
                                  Func<FieldLayout, int, (ExportCell Cell, bool Refused)> at, ExportKind keys,
                                  ExportEntity entity, BuilderRule rule, ExportReport report)
    {
        foreach ((PartDeclaration part, FieldLayout field, ArrayLayout global) in table.Parts)
        {
            var (index, refused) = at(field, 0);
            if (refused || index.Number < 0 || index.Number >= global.Dimensions[0]) continue;
            long? Checked(int element)
            {
                int address = global.Address + element;
                string? writer = run.Writers.TryGetValue(new GlobalCell(global.Bank, address), out string? script) ? script : null;
                var cell = new ExportCell(global.Name, element, global.Bank, address, run.Globals.GetValueOrDefault(address),
                                          null, writer);
                if (RefusalOf(run, cell, rule) is not { } reason) return cell.Number;
                report.Refusals.Add(new Refusal(keys.Declaration.Id, entity.Id,
                                                $"{table.Declaration.Name}.{part.Name}:{global.Name}", element,
                                                cell.Bank, cell.Address, reason));
                return null;
            }
            long? group = Checked((int)index.Number), same = Checked(part.SameAs);
            if (group is { } value && value != 0 && value == same) return part.Name;
        }
        return null;
    }

    /// <summary>The cells a seed writes for key <paramref name="key"/> at step <paramref name="step"/>: one for a single
    /// cell, and for an array each cell its index terms name, "*" naming every index of its dimension.</summary>
    private static IEnumerable<int> SeedAddresses(ArrayLayout array, IReadOnlyList<SeedIndex> index, int key, int step,
                                                  string where)
    {
        if (index.Count != array.Dimensions.Count)
            throw new ExportFailure($"{where}: a seed of {array.Name} has {index.Count} index terms for its "
                                    + $"{array.Dimensions.Count} dimensions");
        IEnumerable<int> offsets = [0];
        for (int dimension = 0; dimension < index.Count; dimension++)
        {
            int size = array.Dimensions[dimension];
            int stride = array.Dimensions.Skip(dimension + 1).Aggregate(1, (product, next) => product * next);
            SeedIndex term = index[dimension];
            int[] values = term.Kind switch
            {
                SeedIndexKind.Key => [key],
                SeedIndexKind.Step => [step],
                SeedIndexKind.Every => [.. Enumerable.Range(0, size)],
                _ => [term.Value],
            };
            if (values.Any(value => value < 0 || value >= size))
                throw new ExportFailure($"{where}: a seed of {array.Name} indexes {string.Join(", ", values)} in a "
                                        + $"dimension of {size}");
            offsets = offsets.SelectMany(offset => values.Select(value => offset + value * stride)).ToList();
        }
        return offsets.Select(offset => array.Address + offset);
    }

    /// <summary>A literal's text (plan §5): the string operand of the instruction at its script and offset, and its
    /// translations at that site.</summary>
    private static List<ExportText> LiteralTexts(ExportInputs inputs, LiteralDeclaration literal,
                                                 IReadOnlyList<Translation> languages, string where)
    {
        Script script = inputs.ScriptByName(literal.Script)
                        ?? throw new ExportFailure($"{where}: a literal is in {literal.Script}, which the install lacks");
        if (!script.IndexByOffset.TryGetValue(literal.Offset, out int at))
            throw new ExportFailure($"{where}: {literal.Script}@0x{literal.Offset:x} starts no instruction");
        string? text = script.Instructions[at].Args
            .Where(operand => operand.Type == TextOpcodes.LiteralOperandType)
            .Select(operand => script.Strings.TryGetValue((int)operand.Value, out string? value) ? value : null)
            .FirstOrDefault(value => value != null);
        if (text is not { Length: > 0 })
            throw new ExportFailure($"{where}: {literal.Script}@0x{literal.Offset:x} has no string operand");
        var site = new TextSite(script.Name, literal.Offset);
        var texts = new List<ExportText> { new(literal.Field, 0, text) };
        foreach (Translation language in languages)
            if (language.String(text, site) is { } translated)
                texts.Add(new ExportText(literal.Field, 0, translated, language.Tag));
        return texts;
    }

    // ------------------------------------------------------------------------------------------------- snippets (§6.6)

    /// <summary>A record's entries in a snippet (plan §4.3): each field of <paramref name="tableFields"/> that
    /// <paramref name="shown"/> covers, once, in the table's order, at its value. A row's columns are written by name
    /// where the registry names them and by number otherwise, as a data file must; a refused element is left out.
    /// </summary>
    /// <summary>The second cell of each published two-field reference (export plan §5): its <c>plus</c> field whole, or
    /// for a paired one the column after the published one, and a reference's zero field (the field that says where a
    /// 0 names record 0). An offset reference has no second cell.</summary>
    private static IEnumerable<FieldLayout> KeyPartners(IReadOnlyList<FieldLayout> published, IReadOnlyList<FieldLayout> tableFields,
                                                        IReadOnlyList<FieldLayout> joinedFields, TableLayout table,
                                                        IReadOnlyDictionary<string, TableLayout> joins)
    {
        foreach (FieldLayout field in published)
        {
            TableLayout owner = field.Joined is { } joined ? joins[joined] : table;
            if (!owner.RefKeys.TryGetValue(field.Field, out RefKey? key)) continue;
            // The field that says where a 0 names record 0 (a Kamidori item's category), which a modder changes too.
            if (key.Zero?.Field is { } on)
                yield return (field.Joined == null ? tableFields : joinedFields)
                    .First(other => other.Field == on && other.Joined == field.Joined && other.Column == null);
            if (key.Offset != null || key.Zero != null || key.Last != null) continue;
            if (key.Pairs)
            {
                if (field.Column is { } column) yield return field with { Column = column + 1 };
                continue;
            }
            yield return (field.Joined == null ? tableFields : joinedFields)
                .First(other => other.Field == key.Plus && other.Joined == field.Joined);
        }
    }

    private static List<KeyValuePair<string, DataValue>> SnippetEntries(IReadOnlyList<FieldLayout> tableFields,
        IReadOnlyList<FieldLayout> shown, Func<FieldLayout, int, DataValue?> valueOf)
    {
        var entries = new List<KeyValuePair<string, DataValue>>();
        foreach (FieldLayout whole in tableFields)
        {
            var columns = new SortedSet<int>();
            foreach (FieldLayout field in shown.Where(field => field.Field == whole.Field && field.Joined == whole.Joined))
                if (field.Column is { } column) columns.Add(column);
                else columns.UnionWith(Enumerable.Range(0, whole.Elements));
            if (columns.Count == 0) continue;
            if (whole.Array.Dimensions.Count != 2)
            {
                if (valueOf(whole, 0) is { } value) entries.Add(KeyValuePair.Create(whole.Field, value));
                continue;
            }
            var row = new List<KeyValuePair<string, DataValue>>();
            foreach (int column in columns)
                if (valueOf(whole, column) is { } value)
                    row.Add(KeyValuePair.Create(whole.Array.Columns.GetValueOrDefault(column)
                                                ?? column.ToString(CultureInfo.InvariantCulture), value));
            if (row.Count > 0) entries.Add(KeyValuePair.Create(whole.Field, (DataValue)new DataTable(row)));
        }
        return entries;
    }

    /// <summary>Each entity's snippet (spec §6.6, plan §4.3): its record under <c>[&lt;table&gt;.&lt;id&gt;]</c> and its
    /// loaded rows under <c>[&lt;scope&gt;.&lt;id&gt;.&lt;table&gt;.&lt;record&gt;]</c>, written as a data file, read back and
    /// checked against the base mod's shape.</summary>
    private static void WriteSnippets(ExportInputs inputs, ExportKind kind, ExportReport report)
    {
        foreach (ExportEntity entity in kind.Entities)
        {
            string id = entity.Id.ToString(CultureInfo.InvariantCulture);
            var top = new List<KeyValuePair<string, DataValue>>
            {
                KeyValuePair.Create(kind.Table.Name, (DataValue)new DataTable([KeyValuePair.Create(id, (DataValue)new DataTable(entity.SnippetRecord))])),
            };
            foreach ((string table, int record, List<KeyValuePair<string, DataValue>> fields) in entity.SnippetJoined)
                top.Add(KeyValuePair.Create(table, (DataValue)new DataTable(
                    [KeyValuePair.Create(record.ToString(CultureInfo.InvariantCulture), (DataValue)new DataTable(fields))])));
            foreach (IGrouping<string, (string Scope, string Table, int Record, List<KeyValuePair<string, DataValue>> Fields)> scope
                     in entity.SnippetRows.GroupBy(row => row.Scope))
            {
                var tables = scope.GroupBy(row => row.Table).Select(rows => KeyValuePair.Create(rows.Key, (DataValue)new DataTable(
                    rows.Select(row => KeyValuePair.Create(row.Record.ToString(CultureInfo.InvariantCulture),
                                                           (DataValue)new DataTable(row.Fields))).ToList()))).ToList();
                top.Add(KeyValuePair.Create(scope.Key, (DataValue)new DataTable(
                    [KeyValuePair.Create(id, (DataValue)new DataTable(tables))])));
            }
            string text = DataFileWriter.Write(new DataTable(top));
            string origin = $"{kind.Declaration.Id}/{id}.toml";
            DataReadResult read = DataDocumentReader.Read(text, origin);
            IReadOnlyList<DataProblem> problems = !read.Ok ? read.Problems
                : inputs.Shape is { } shape ? DataShapeCheck.Check(shape, read).Problems : [];
            foreach (DataProblem problem in problems)
                report.SnippetProblems.Add((kind.Declaration.Id, entity.Id, problem.Describe(origin)));
            entity.Snippet = text;
        }
    }

    // ------------------------------------------------------------------------------------------------- descriptions (§5.7)

    private static void Describe(ExportInputs inputs, GameSession boot, KindDeclaration kind, ExportEntity entity,
                                 DescriptionDeclaration description, Func<FieldLayout, int, (ExportCell Cell, bool Refused)> at,
                                 TableLayout table, IReadOnlyList<Translation> languages, ExportReport report)
    {
        Script? script;
        if (description.ScriptField is { } scriptField)
        {
            var (cell, refused) = at(inputs.Registry.Resolve(table, scriptField), 0);
            if (refused || cell.Number == 0) return;
            script = inputs.Scripts.GetById(cell.Number);
            if (script == null)
            {
                report.MissingScripts.Add((kind.Id, entity.Id, description.Name, cell.Number));
                return;
            }
        }
        else
            script = inputs.ScriptByName(description.ScriptName!)
                     ?? throw new ExportFailure($"{kind.Id}'s description {description.Name} names no script {description.ScriptName}");
        if (!inputs.Declarations.TextScripts.Contains(ScriptNames.Base(script.Name), StringComparer.Ordinal))
        {
            report.Refusals.Add(new Refusal(kind.Id, entity.Id, description.Name, 0, GlobalBank.Integer, 0,
                                            $"its script {ScriptNames.Normalized(script.Name)} is no text script"));
            return;
        }

        // Each run starts from the boot with the description's seeds; a language's run presents its text (plan §4.6).
        (SceneResult Result, DescriptionText Shown) RunFromBoot(TextPresenter? presenter)
        {
            GameSession run = boot.CopyState();
            foreach ((string global, SeedValue value) in description.Seed)
                run.Seed(inputs.Registry.IntegerGlobal(global), value.For(entity.Id));
            var shown = new DescriptionText(script.Name);
            SceneResult result = run.RunScene(script, inputs.Table, new HeadlessWaits(),
                                              new VmOptions(MaxSteps: inputs.DescriptionMaxSteps) { ProfileId = inputs.GameId },
                                              inputs.Scripts, build: inputs.Build, textListener: shown, presenter: presenter);
            return (result, shown);
        }

        var (result, shown) = RunFromBoot(null);
        report.DescriptionRuns++;
        report.DroppedPieces += shown.Dropped;
        if (result.Halt != "exit") report.DescriptionHalts.Add((kind.Id, entity.Id, description.Name, result.Halt ?? ""));
        if (shown.Text() is not { Length: > 0 } text)
        {
            report.EmptyDescriptions++;
            return;
        }
        entity.Texts.Add(new ExportText(description.Name, 0, text));
        foreach (Translation language in languages)
        {
            // Each run logs its own misses, so the report keeps those of the description's own script.
            var misses = new TextMissLog();
            var (again, presented) = RunFromBoot(language.PresenterFor(misses));
            if (again.Halt != "exit")
                language.Report.DescriptionHalts.Add((kind.Id, entity.Id, description.Name, again.Halt ?? ""));
            string own = TextCatalog.NormalizeScriptName(script.Name);
            foreach ((string missed, string where, int offset, _) in misses.Misses())
                if (where == own) language.Report.Missed(missed, where, offset);
            if (language.Description(presented) is { } translated)
                entity.Texts.Add(new ExportText(description.Name, 0, translated, language.Tag));
        }
    }

    /// <summary>One language of the export (spec §6.4, plan §4.6): presenters as the port's, with the same catalogs
    /// and message tables, the declarations' overrides of its table strings (<c>[export] text_overrides</c>), and its
    /// report.</summary>
    private sealed class Translation(ExportLanguage language, ITextMessageIndex? messages,
                                     IReadOnlyDictionary<string, string>? overrides)
    {
        private readonly TextPresenter _strings = new(language.Text, messages: messages);

        public LanguageReport Report { get; } = new(language, overrides?.Keys ?? []);
        public string Tag => language.Tag;

        /// <summary>A presenter that logs to <paramref name="misses"/>, for one description run.</summary>
        public TextPresenter PresenterFor(TextMissLog misses) => new(language.Text, misses: misses, messages: messages);

        /// <summary>A table string's text in the language, looked up at its origin; null when no entry has it, which a
        /// string that is only a number never needs.</summary>
        public string? String(string text, TextSite origin)
        {
            Report.Strings++;
            // An override is the declarations' text, not the package's: it stands wherever the string does.
            if (overrides?.GetValueOrDefault(text) is { } overridden)
            {
                Report.Overridden(text);
                return overridden;
            }
            if (_strings.Present(text, origin, origin) is { Entry: not null } presented)
            {
                Report.StringsTranslated++;
                return presented.Text;
            }
            if (!TextPatternMatcher.IsNumber(text)) Report.Missed(text, origin.Script, origin.Offset);
            return null;
        }

        /// <summary>A description's text in the language, when any of its runs was presented; null when none was.</summary>
        public string? Description(DescriptionText shown)
        {
            Report.Descriptions++;
            if (shown.Presented == 0) return null;
            if (shown.Missed == 0) Report.DescriptionsTranslated++;
            else Report.DescriptionsPartly++;
            return shown.Text();
        }
    }

    /// <summary>The text one script shows in a description run (spec §5.7), line by line; another script's runs are
    /// dropped and counted. In a run that presents its text (plan §4.6), a run shows its presented text, or its source
    /// text when it has no entry; a presented message's later runs show nothing, and the line ends before them lie
    /// inside its sentence, which draws its own breaks (localization spec §6.2).</summary>
    private sealed class DescriptionText(string script) : ITextListener
    {
        private readonly List<StringBuilder> _lines = [new()];
        private int _breaks;

        public int Dropped { get; private set; }
        /// <summary>The script's runs an entry presented, and those that missed one (a number misses none).</summary>
        public int Presented { get; private set; }
        public int Missed { get; private set; }

        public void RunShown(AdvLiveTextRun run, int glyphDelayMilliseconds)
        {
            if (!Mine(run)) return;
            Break();
            if (run.Presented is { Entry: not null }) Presented++;
            else if (run.Text.Length > 0 && !TextPatternMatcher.IsNumber(run.Text)) Missed++;
            _lines[^1].Append(run.Presented?.Text ?? run.Text);
        }

        public void HiddenRunShown(AdvLiveTextRun run)
        {
            if (Mine(run)) _breaks = 0;
        }

        // A line end takes effect at the next run, which is hidden when the line end lies inside a presented message.
        public void LineEnded(int layoutSlot) => _breaks++;

        private bool Mine(AdvLiveTextRun run)
        {
            if (run.ScriptStack.Count > 0 && string.Equals(run.ScriptStack[^1], script, StringComparison.Ordinal))
                return true;
            Dropped++;
            return false;
        }

        private void Break()
        {
            for (; _breaks > 0; _breaks--) _lines.Add(new StringBuilder());
        }

        /// <summary>The lines joined by <c>\n</c>, without the empty lines at the end.</summary>
        public string Text()
        {
            Break();
            var lines = _lines.Select(line => line.ToString()).ToList();
            while (lines.Count > 0 && lines[^1].Length == 0) lines.RemoveAt(lines.Count - 1);
            return string.Join('\n', lines);
        }
    }

    /// <summary>The declarations' explicit pictures: a picture is marked when its catalog name, without an append's
    /// <c>$n$</c> prefix, matches one of them; the ones that match nothing are reported.</summary>
    private sealed class ExplicitPictures(IReadOnlyList<string> patterns)
    {
        private readonly HashSet<string> _matched = new(StringComparer.Ordinal);

        public IEnumerable<string> Unmatched => patterns.Where(pattern => !_matched.Contains(pattern));

        public ExportPicture Mark(ExportPicture picture)
        {
            string name = ScriptNames.Base(picture.Name);
            bool isExplicit = false;
            foreach (string pattern in patterns)
                if (ScriptNames.Matches(pattern, name))
                {
                    _matched.Add(pattern);
                    isExplicit = true;
                }
            return isExplicit == picture.Explicit ? picture : picture with { Explicit = isExplicit };
        }
    }

    // ------------------------------------------------------------------------------------------------- references (§5.3)

    /// <summary>The records a two-field reference names (<c>[table.refs]</c> <c>{ table, times, plus }</c>, export plan
    /// §5): each element's value times <c>times</c>, plus the <c>plus</c> field's value at the same element; for a
    /// paired one (<c>{ table, times, pairs = true }</c>) element 2k's value times <c>times</c>, plus element 2k + 1's,
    /// and an element 2k + 1, the second of a pair, names none. A value of 0 or below stays as it is, none; an element
    /// either of whose cells is withheld is null. With <c>any</c> (<see cref="RefKey.Any"/>), an element whose plus is
    /// that value names its value's row instead, every record of it: its id is 0 and its row is the value. The rows are
    /// null for a reference without <c>any</c>. With <c>bases</c> (<see cref="RefKey.Bases"/>), a value names its base
    /// plus the plus field's value, and another value none. With an offset (<c>{ table, offset }</c>,
    /// <see cref="RefKey.Offset"/>), a value above 0 names itself plus the offset, and one that names no record above 0
    /// none. With <c>zero</c> (<see cref="RefKey.Zero"/>), on a plain or an offset reference, a 0 so named is record 0
    /// where the zero's field holds one of its values (everywhere for <c>zero = true</c>), and none is -1: a value of 0
    /// or below before the offset or below 0 after it, and a 0 elsewhere. The zero's field withheld withholds the 0.
    /// With <c>last</c> (<see cref="RefKey.Last"/>), a value above it names none, -1.
    /// </summary>
    private static (long?[] Ids, long?[]? Rows) KeyedIds(FieldLayout field, IReadOnlyList<FieldLayout> tableFields,
                                                       RefKey key, Func<FieldLayout, int, (ExportCell Cell, bool Refused)> at)
    {
        if (key.Offset is not null || key.Zero is not null || key.Last is not null)
        {
            long offset = key.Offset ?? 0;
            // Whether a 0 names record 0 on this record: true, false, or null where the zero's field is withheld.
            bool? zeroNames = null;
            if (key.Zero is not { Field: { } on } zero) zeroNames = key.Zero is not null;
            else if (at(tableFields.First(other => other.Field == on && other.Column == null), 0) is var (onCell, onRefused)
                     && !onRefused)
                zeroNames = zero.Values.Contains(onCell.Number);
            var named = new long?[field.Elements];
            for (int element = 0; element < field.Elements; element++)
            {
                var (value, refused) = at(field, element);
                long? id = refused ? null
                    : key.Offset is null ? value.Number
                    : value.Number <= 0 ? (key.Zero is null ? value.Number : -1)
                    : Math.Max(value.Number + offset, key.Zero is null ? 0 : -1);
                if (key.Zero is not null && id == 0) id = zeroNames switch { true => 0, false => -1, null => null };
                if (key.Last is { } last && id > last) id = -1;
                named[element] = id;
            }
            return (named, null);
        }
        FieldLayout plus = tableFields.First(other => other.Field == (key.Plus ?? field.Field) && other.Column == null);
        var ids = new long?[field.Elements];
        long?[]? rows = key.Any is null ? null : new long?[field.Elements];
        for (int element = 0; element < field.Elements; element++)
        {
            int column = field.Column ?? element;
            if (key.Pairs && column % 2 == 1) continue;
            var (value, refused) = at(field, element);
            var (part, partRefused) = at(plus, key.Pairs ? column + 1 : column);
            if (!refused && !partRefused && value.Number > 0 && rows != null && part.Number == key.Any)
            {
                ids[element] = 0;
                rows[element] = value.Number;
                continue;
            }
            // With bases, a value names its base plus the plus field's value, and a value that is not one of them none.
            if (key.Bases is { } bases)
            {
                ids[element] = refused || partRefused ? null
                    : bases.TryGetValue(value.Number, out long first) ? first + part.Number : 0;
                continue;
            }
            ids[element] = refused || partRefused ? null
                : value.Number <= 0 ? value.Number
                : value.Number * key.Times + part.Number;
        }
        return (ids, rows);
    }

    /// <summary>The pictures each kind borrows (export plan §5): an entity takes picture P of the first entity of the
    /// other kind, by id, that has P and whose published reference names it. A Verita party unit shows its character's
    /// joining title's chibi, title 20 × character being the first of the character's titles, all of which name the
    /// unit.</summary>
    private static void BorrowPictures(IReadOnlyList<ExportKind> kinds)
    {
        foreach (ExportKind kind in kinds)
            foreach (BorrowedPictureDeclaration borrowed in kind.Declaration.BorrowedPictures)
            {
                ExportKind from = kinds.SingleOrDefault(other => other.Declaration.Id == borrowed.From)
                                  ?? throw new ExportFailure($"{kind.Declaration.Id}'s picture {borrowed.Name} is borrowed "
                                                             + $"from kind {borrowed.From}, which is not declared");
                var entities = kind.Entities.ToDictionary(entity => (long)entity.Id);
                var given = new HashSet<long>();
                foreach (ExportEntity source in from.Entities.OrderBy(entity => entity.Id))
                {
                    if (source.Pictures.FirstOrDefault(pair => pair.Name == borrowed.Picture).Picture is not { } picture)
                        continue;
                    foreach (ExportRef reference in source.Refs)
                        if (reference.Field == borrowed.By && reference.ToKind == kind.Declaration.Id
                            && entities.TryGetValue(reference.ToId, out ExportEntity? entity) && given.Add(reference.ToId))
                            entity.Pictures.Add((borrowed.Name, picture));
                }
            }
    }

    private static void LinkReferences(IReadOnlyList<ExportKind> kinds, ExportReport report)
    {
        var kindOfTable = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (ExportKind kind in kinds) kindOfTable.TryAdd(kind.Table.Name, kind.Declaration.Id);
        var ids = kinds.ToDictionary(kind => kind.Declaration.Id, kind => kind.Entities.Select(entity => entity.Id).ToHashSet(),
                                     StringComparer.Ordinal);
        void Link(ExportKind kind, ExportEntity entity, string table, string name, int first, long?[] values,
                  long?[]? rows = null, int times = 0, bool zero = false)
        {
            // A row reference's element names every record of its row that is an entity (export plan §5): an item that
            // every card of a row may carry refers to each of them, under the one element.
            for (int element = 0; rows != null && element < rows.Length; element++)
            {
                if (rows[element] is not { } row) continue;
                if (!kindOfTable.TryGetValue(table, out string? toKind))
                {
                    var unpublished = (kind.Declaration.Id, name, table);
                    report.UnpublishedRefs[unpublished] = report.UnpublishedRefs.GetValueOrDefault(unpublished) + 1;
                    continue;
                }
                int named = 0;
                for (long id = row * times; id < (row + 1) * times; id++)
                    if (id is >= 0 and <= int.MaxValue && ids[toKind].Contains((int)id))
                    {
                        entity.Refs.Add(new ExportRef(name, first + element, toKind, id));
                        named++;
                    }
                if (named == 0)
                {
                    var missing = (kind.Declaration.Id, name, toKind);
                    report.MissingRefs[missing] = report.MissingRefs.GetValueOrDefault(missing) + 1;
                }
            }
            for (int element = 0; element < values.Length; element++)
            {
                // 0 is none, and so is a negative value: no record has a negative id, and the games write -1 for none
                // too (Kamidori's stage_required_party_unit_ids; export plan §5). A reference with zero has named its
                // nones -1 (KeyedIds), so its 0 is record 0: a Kamidori weapon's attack element 0, 無属.
                if (values[element] is not { } id || id < 0 || id == 0 && !zero) continue;
                if (!kindOfTable.TryGetValue(table, out string? toKind))
                {
                    var unpublished = (kind.Declaration.Id, name, table);
                    report.UnpublishedRefs[unpublished] = report.UnpublishedRefs.GetValueOrDefault(unpublished) + 1;
                    continue;
                }
                entity.Refs.Add(new ExportRef(name, first + element, toKind, id));
                if (id is < int.MinValue or > int.MaxValue || !ids[toKind].Contains((int)id))
                {
                    var missing = (kind.Declaration.Id, name, toKind);
                    report.MissingRefs[missing] = report.MissingRefs.GetValueOrDefault(missing) + 1;
                }
            }
        }
        foreach (ExportKind kind in kinds)
        {
            foreach (FieldLayout field in kind.Published)
            {
                if (!kind.TableOf(field).Refs.TryGetValue(field.Field, out string? target) || target == "picture") continue;
                RefKey? key = kind.TableOf(field).RefKeys.GetValueOrDefault(field.Field);
                foreach (ExportEntity entity in kind.Entities)
                    Link(kind, entity, target, field.Name, 0, entity.KeyedRefs.GetValueOrDefault(field.Name)
                                                              ?? entity.Values.First(value => value.Field == field.Name).Values,
                         entity.KeyedRows.GetValueOrDefault(field.Name), key?.Times ?? 0, key?.Zero is not null);
            }
            // A loaded table's references are the key entity's (export plan §4.2): its field written <table>.<field>,
            // its index the record times the field's width plus the element.
            foreach (ExportLoadedTable table in kind.Loaded)
                foreach (FieldLayout field in table.Published)
                {
                    if (!table.TableOf(field).Refs.TryGetValue(field.Field, out string? target) || target == "picture")
                        continue;
                    // A field a join goes through names the record whose fields the row publishes (a dig site's type
                    // and its dig table): to a table no kind shows, the join is all of its reference, so no table is
                    // missing. To a kind's table it links as any reference does.
                    if (field.Joined is null && !kindOfTable.ContainsKey(target)
                        && table.Joins.Values.Any(join => join.Through.Name == field.Name))
                        continue;
                    string name = $"{table.Declaration.Name}.{field.Name}";
                    RefKey? key = table.TableOf(field).RefKeys.GetValueOrDefault(field.Field);
                    foreach (ExportEntity entity in kind.Entities)
                        foreach (ExportLoadedRow row in entity.Loaded.Where(row => row.Table == table.Declaration.Name))
                            Link(kind, entity, target, name, row.Record * field.Elements,
                                 row.KeyedRefs.GetValueOrDefault(field.Name)
                                 ?? row.Values.First(value => value.Field == field.Name).Values,
                                 row.KeyedRows.GetValueOrDefault(field.Name), key?.Times ?? 0, key?.Zero is not null);
                }
        }
    }
}
