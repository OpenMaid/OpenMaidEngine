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
        "picture", "cell", "snippet", "script_use", "loaded", "loaded_field", "loaded_part", "label", "value_format", "qualifier", "referenced_as",
        "value_name",
    };

    public static ExportResult Run(ExportInputs inputs)
    {
        ExportDeclarations declarations = inputs.Declarations;
        var rule = new BuilderRule(declarations.Builders, declarations.ExcludedBuilders);
        var report = new ExportReport();
        var explicitPictures = new ExplicitPictures(declarations.ExplicitPictures);
        GameSession boot = Boot(inputs, report);
        List<Translation> languages = [.. inputs.Languages.Select(language => new Translation(language, inputs.Messages))];
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
        if (declarations.Relations.Count > 0) FindRelations(inputs, kinds, report);
        foreach (ExportKind kind in kinds) WriteSnippets(inputs, kind, report);
        if (inputs.ScriptIndex is { } index)
            foreach (ExportKind kind in kinds)
                kind.ScriptUse.AddRange(index.UsesOf(
                    [.. kind.TableFields.Concat(kind.Loaded.SelectMany(loaded => loaded.TableFields))
                        .Select(field => field.Array).Distinct()
                        .Select(array => (array.Bank, array.Address, array.Address + array.Dimensions[0] * array.Width))])
                    .OrderBy(use => use.Script, StringComparer.Ordinal));
        if (inputs.Names is { } names)
            foreach (ExportKind kind in kinds) CountUnnamed(names, boot, kind, report);
        report.UnmatchedExplicitPictures.AddRange(explicitPictures.Unmatched);
        return new ExportResult(declarations, kinds, report, inputs.BuildInfo) { Languages = inputs.Languages };
    }

    /// <summary>The words and names of the values a kind shows (plan §5): for each published field, its own and its
    /// loaded tables', whose global has value facets, OME's words and the game's names for its values, or for a mask's
    /// bits those of the global its bits stand for. A name is a string the boot left in the game's table, checked as a
    /// published text is, in the source language and each translation's.</summary>
    private static void NameValues(ExportInputs inputs, GameSession boot, ExportKind kind, BuilderRule rule,
                                   IReadOnlyList<Translation> languages, ExportReport report)
    {
        string id = kind.Declaration.Id;
        var fields = kind.Published.Select(field => (field.Name, field.Array))
            .Concat(kind.Loaded.SelectMany(table => table.Published.Select(
                field => ($"{table.Declaration.Name}.{field.Name}", field.Array))));
        foreach ((string name, ArrayLayout array) in fields)
        {
            if (array.Format is not { Names: true } format) continue;
            ValueFormat named = format;
            if (format.BitsOf is { } bits)
                named = inputs.Registry.Find(bits)?.Format
                        ?? throw new ExportFailure($"{id}.{name}'s bits stand for the values of {bits}, which have no words");
            foreach ((long value, string word) in named.Words) kind.ValueNames.Add(new ExportValueName(name, value, "", word));
            if (named.NamesFrom is not { } from) continue;
            ArrayLayout names = inputs.Registry.Find(from.Global) is { Bank: GlobalBank.String, Dimensions.Count: 1 } list
                ? list
                : throw new ExportFailure($"{id}.{name}'s values are named by {from.Global}, which is no list of strings");
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
                    report.Refusals.Add(new Refusal(id, 0, $"{name}:{names.Name}", element, GlobalBank.String, address,
                                                    reason));
                    continue;
                }
                long value = from.First + element;
                kind.ValueNames.Add(new ExportValueName(name, value, ExportWriter.SourceLanguage, text));
                if (!boot.StringStores.TryGetValue(address, out TextManager.StringStore store)) continue;
                foreach (Translation language in languages)
                    if (language.String(text, store.Origin) is { } translated)
                        kind.ValueNames.Add(new ExportValueName(name, value, language.Tag, translated));
            }
        }
    }

    /// <summary>The integer cells a kind's builders (the writers of its cells) wrote that no global of the registry
    /// names (spec §6.6, plan §4.3): an array the registry has yet to name. Reported, not published.</summary>
    private static void CountUnnamed(GlobalNames names, GameSession boot, ExportKind kind, ExportReport report)
    {
        var builders = kind.Entities.SelectMany(entity => entity.Cells).Select(cell => cell.Writer).OfType<string>()
            .ToHashSet(StringComparer.Ordinal);
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
        IReadOnlyList<FieldLayout> published = kind.Fields.Select(field => registry.Resolve(table, field)).ToList();
        var texts = new List<string>();
        // A title is a string field, a reference whose target's title the entity takes, or none (plan §4.1).
        if (kind.Title is { } title && registry.Resolve(table, title).IsString) texts.Add(title);
        texts.AddRange(kind.Text.Where(text => text != kind.Title));
        IReadOnlyList<FieldLayout> textFields = texts.Select(text => registry.Resolve(table, text)).ToList();
        var textArrays = new HashSet<string>(textFields.Select(field => field.Field), StringComparer.Ordinal);
        var pictures = kind.Pictures.Select(picture => (picture.Name, Field: registry.Resolve(table, picture.Field))).ToList();
        var sheets = kind.Sheets.Select(sheet => (Sheet: sheet, Field: registry.Resolve(table, sheet.Field))).ToList();
        foreach ((string name, FieldLayout field) in pictures.Concat(sheets.Select(pair => (pair.Sheet.Name, pair.Field))))
            if (field.Elements != 1 || field.IsString)
                throw new ExportFailure($"{kind.Id}'s picture {name} ({field.Name}) is not one integer cell of a record");

        // The snippet sets what the site shows of a record (plan §4.3): its published fields, its text and its pictures.
        IReadOnlyList<FieldLayout> snippetFields = [.. published, .. textFields, .. pictures.Select(picture => picture.Field),
                                                    .. sheets.Select(sheet => sheet.Field)];
        var entities = new List<ExportEntity>();
        for (int record = 0; record < table.Records; record++)
        {
            if (Read(boot, exists, record, 0).IsDefault) continue;
            if (skips.Any(skip => Read(boot, skip.Field, record, 0).Number == skip.Value)) continue;
            var entity = new ExportEntity(record);

            // Every named field's cells, each checked once (§5.5, §6.6): an integer's value is shown, so a value no
            // builder wrote is refused; a string is shown only when the kind publishes it as text.
            // Keyed by bank and address: a string array and an integer array may share addresses in their banks.
            var cells = new Dictionary<GlobalCell, (ExportCell Cell, bool Refused)>();
            foreach (FieldLayout field in tableFields)
            {
                bool publishedText = textArrays.Contains(field.Field);
                for (int element = 0; element < field.Elements; element++)
                {
                    ExportCell cell = Read(boot, field, record, element);
                    bool shown = !field.IsString || publishedText;
                    string? reason = shown ? RefusalOf(boot, cell, rule) : null;
                    if (reason != null)
                        report.Refusals.Add(new Refusal(kind.Id, record, field.Field, element, cell.Bank, cell.Address, reason));
                    cells[new GlobalCell(cell.Bank, cell.Address)] = (cell, reason != null);
                    if (cell.Writer != null || !cell.IsDefault)
                        entity.Cells.Add(new ExportModderCell(field.Field, element, cell.Bank, cell.Address,
                            reason != null || !shown ? null : cell.Text ?? cell.Number.ToString(CultureInfo.InvariantCulture),
                            cell.Writer));
                }
            }
            (ExportCell Cell, bool Refused) At(FieldLayout field, int element)
                => cells[new GlobalCell(field.Array.Bank, field.Cell(record, element))];

            foreach (FieldLayout field in published)
            {
                var values = new long?[field.Elements];
                for (int element = 0; element < field.Elements; element++)
                {
                    var (cell, refused) = At(field, element);
                    values[element] = refused ? null : cell.Number;
                }
                entity.Values.Add((field.Name, values));
            }
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
            foreach ((string name, FieldLayout field) in pictures)
            {
                var (cell, refused) = At(field, 0);
                if (refused || cell.Number == 0 || inputs.Pictures == null) continue;
                if (inputs.Pictures.Resolve(cell.Number) is { } picture)
                    entity.Pictures.Add((name, explicitPictures.Mark(picture)));
                else report.UnresolvedPictures.Add((kind.Id, record, name, cell.Number));
            }
            // A cell of a sheet (plan §5): a number below the sheet's first is no picture.
            foreach ((SheetPictureDeclaration sheet, FieldLayout field) in sheets)
            {
                var (cell, refused) = At(field, 0);
                if (refused || inputs.Pictures == null || sheet.Cell(cell.Number) is not { } at) continue;
                if (inputs.Pictures.Cell(sheet.Sheet, at.X, at.Y, sheet.Size.Width, sheet.Size.Height) is { } picture)
                    entity.Pictures.Add((sheet.Name, explicitPictures.Mark(picture)));
                else report.UnresolvedPictures.Add((kind.Id, record, sheet.Name, cell.Number));
            }
            foreach (DescriptionDeclaration description in kind.Descriptions)
                Describe(inputs, boot, kind, entity, description, At, table, languages, report);
            entity.SnippetRecord.AddRange(SnippetEntries(tableFields, snippetFields, (field, column) =>
            {
                var (cell, refused) = At(field, column);
                return refused ? null : field.IsString ? new DataString(cell.Text ?? "") : new DataInteger(cell.Number);
            }));
            entities.Add(entity);
        }
        var declared = published.Concat(textFields).Concat(pictures.Select(picture => picture.Field))
            .Concat(sheets.Select(sheet => sheet.Field)).Concat(skips.Select(skip => skip.Field)).Append(exists)
            .DistinctBy(field => field.Name).ToList();
        return new ExportKind(kind, table, tableFields, published, declared, entities);
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
            var loaded = new ExportLoadedTable(declared, loader.Scope, table, registry.FieldsOf(table),
                                               declared.Fields.Select(field => registry.Resolve(table, field)).ToList(),
                                               registry.Resolve(table, declared.Exists))
            {
                Text = declared.Text.Select(field => registry.Resolve(table, field)).ToList(),
                Parts = parts,
            };
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
            int row = step * table.Table.Records + record;
            // Every field's cells, as a kind's (§6.6): an integer's value is shown and checked, and so is a published
            // text; another string shows its address and writer only. Its cells are the key entity's, the field written
            // <table>.<field> and the index the row times the width plus the element.
            var cells = new Dictionary<GlobalCell, (ExportCell Cell, bool Refused)>();
            foreach (FieldLayout field in table.TableFields)
            {
                bool shown = !field.IsString || textArrays.Contains(field.Field);
                for (int element = 0; element < field.Elements; element++)
                {
                    ExportCell cell = Read(run, field, record, element);
                    string? reason = shown ? RefusalOf(run, cell, rule) : null;
                    int index = row * field.Elements + element;
                    if (reason != null)
                        report.Refusals.Add(new Refusal(keys.Declaration.Id, entity.Id, $"{name}.{field.Field}", index,
                                                        cell.Bank, cell.Address, reason));
                    cells[new GlobalCell(cell.Bank, cell.Address)] = (cell, reason != null);
                    if (cell.Writer != null || !cell.IsDefault)
                        entity.Cells.Add(new ExportModderCell($"{name}.{field.Field}", index, cell.Bank, cell.Address,
                            reason != null || !shown ? null : cell.Text ?? cell.Number.ToString(CultureInfo.InvariantCulture),
                            cell.Writer));
                }
            }
            (ExportCell Cell, bool Refused) At(FieldLayout field, int element)
                => cells[new GlobalCell(field.Array.Bank, field.Cell(record, element))];

            var values = new List<(string Field, long?[] Values)>();
            foreach (FieldLayout field in table.Published)
            {
                var elements = new long?[field.Elements];
                for (int element = 0; element < field.Elements; element++)
                {
                    var (cell, refused) = At(field, element);
                    elements[element] = refused ? null : cell.Number;
                }
                values.Add((field.Name, elements));
            }
            entity.Loaded.Add(new ExportLoadedRow(name, row, values) { Part = PartOf(run, table, At, keys, entity, rule, report) });
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
                SnippetEntries(table.TableFields, [.. table.Published, .. table.Text], (field, column) =>
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
    private static List<KeyValuePair<string, DataValue>> SnippetEntries(IReadOnlyList<FieldLayout> tableFields,
        IReadOnlyList<FieldLayout> shown, Func<FieldLayout, int, DataValue?> valueOf)
    {
        var entries = new List<KeyValuePair<string, DataValue>>();
        foreach (FieldLayout whole in tableFields)
        {
            var columns = new SortedSet<int>();
            foreach (FieldLayout field in shown.Where(field => field.Field == whole.Field))
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
    /// and message tables, and its report.</summary>
    private sealed class Translation(ExportLanguage language, ITextMessageIndex? messages)
    {
        private readonly TextPresenter _strings = new(language.Text, messages: messages);

        public LanguageReport Report { get; } = new(language);
        public string Tag => language.Tag;

        /// <summary>A presenter that logs to <paramref name="misses"/>, for one description run.</summary>
        public TextPresenter PresenterFor(TextMissLog misses) => new(language.Text, misses: misses, messages: messages);

        /// <summary>A table string's text in the language, looked up at its origin; null when no entry has it, which a
        /// string that is only a number never needs.</summary>
        public string? String(string text, TextSite origin)
        {
            Report.Strings++;
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

    private static void LinkReferences(IReadOnlyList<ExportKind> kinds, ExportReport report)
    {
        var kindOfTable = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (ExportKind kind in kinds) kindOfTable.TryAdd(kind.Table.Name, kind.Declaration.Id);
        var ids = kinds.ToDictionary(kind => kind.Declaration.Id, kind => kind.Entities.Select(entity => entity.Id).ToHashSet(),
                                     StringComparer.Ordinal);
        void Link(ExportKind kind, ExportEntity entity, string table, string name, int first, long?[] values)
        {
            for (int element = 0; element < values.Length; element++)
            {
                // 0 is none, and so is a negative value: no record has a negative id, and the games write -1 for none
                // too (Kamidori's stage_required_party_unit_ids; export plan §5).
                if (values[element] is not { } id || id <= 0) continue;
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
                if (!kind.Table.Refs.TryGetValue(field.Field, out string? target) || target == "picture") continue;
                foreach (ExportEntity entity in kind.Entities)
                    Link(kind, entity, target, field.Name, 0, entity.Values.First(value => value.Field == field.Name).Values);
            }
            // A loaded table's references are the key entity's (export plan §4.2): its field written <table>.<field>,
            // its index the record times the field's width plus the element.
            foreach (ExportLoadedTable table in kind.Loaded)
                foreach (FieldLayout field in table.Published)
                {
                    if (!table.Table.Refs.TryGetValue(field.Field, out string? target) || target == "picture") continue;
                    string name = $"{table.Declaration.Name}.{field.Name}";
                    foreach (ExportEntity entity in kind.Entities)
                        foreach (ExportLoadedRow row in entity.Loaded.Where(row => row.Table == table.Declaration.Name))
                            Link(kind, entity, target, name, row.Record * field.Elements,
                                 row.Values.First(value => value.Field == field.Name).Values);
                }
        }
    }
}
