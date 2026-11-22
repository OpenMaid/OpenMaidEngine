using System.Globalization;
using System.Text;
using Ome.Engine.Hosting;
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
/// references (§5.3) and the pictures (§6.5). <see cref="ExportWriter"/> writes the result.
/// </summary>
public static class GameDataExporter
{
    /// <summary>The export's own tables (spec §7.2), which a kind's table may not share a name with.</summary>
    public static readonly IReadOnlySet<string> FixedTables = new HashSet<string>(StringComparer.Ordinal)
    {
        "build_info", "kind", "field", "text", "ref", "relation", "picture", "cell", "snippet", "script_use", "loaded",
        "loaded_field",
    };

    public static ExportResult Run(ExportInputs inputs)
    {
        ExportDeclarations declarations = inputs.Declarations;
        var rule = new BuilderRule(declarations.Builders, declarations.ExcludedBuilders);
        var report = new ExportReport();
        var explicitPictures = new ExplicitPictures(declarations.ExplicitPictures);
        GameSession boot = Boot(inputs, report);

        var kinds = new List<ExportKind>();
        foreach (KindDeclaration kind in declarations.Kinds)
        {
            if (FixedTables.Contains(kind.Id))
                throw new ExportFailure($"kind {kind.Id} has the name of one of the export's own tables");
            kinds.Add(ReadKind(inputs, boot, kind, rule, explicitPictures, report));
        }
        foreach (LoaderDeclaration loader in declarations.Loaders) Load(inputs, boot, loader, kinds, rule, report);
        LinkReferences(kinds, report);
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
        return new ExportResult(declarations, kinds, report, inputs.BuildInfo);
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
                                       ExplicitPictures explicitPictures, ExportReport report)
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
        foreach ((string name, FieldLayout field) in pictures)
            if (field.Elements != 1 || field.IsString)
                throw new ExportFailure($"{kind.Id}'s picture {name} ({field.Name}) is not one integer cell of a record");

        // The snippet sets what the site shows of a record (plan §4.3): its published fields, its text and its pictures.
        IReadOnlyList<FieldLayout> snippetFields = [.. published, .. textFields, .. pictures.Select(picture => picture.Field)];
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
            foreach (FieldLayout field in textFields)
                for (int element = 0; element < field.Elements; element++)
                {
                    var (cell, refused) = At(field, element);
                    if (!refused && cell.Text is { Length: > 0 } text) entity.Texts.Add(new ExportText(field.Name, element, text));
                }
            foreach ((string name, FieldLayout field) in pictures)
            {
                var (cell, refused) = At(field, 0);
                if (refused || cell.Number == 0 || inputs.Pictures == null) continue;
                if (inputs.Pictures.Resolve(cell.Number) is { } picture)
                    entity.Pictures.Add((name, explicitPictures.Mark(picture)));
                else report.UnresolvedPictures.Add((kind.Id, record, name, cell.Number));
            }
            foreach (DescriptionDeclaration description in kind.Descriptions)
                Describe(inputs, boot, kind, entity, description, At, table, report);
            entity.SnippetRecord.AddRange(SnippetEntries(tableFields, snippetFields, (field, column) =>
            {
                var (cell, refused) = At(field, column);
                return refused ? null : field.IsString ? new DataString(cell.Text ?? "") : new DataInteger(cell.Number);
            }));
            entities.Add(entity);
        }
        var declared = published.Concat(textFields).Concat(pictures.Select(picture => picture.Field))
            .Concat(skips.Select(skip => skip.Field)).Append(exists)
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

    /// <summary>Runs <paramref name="loader"/> once for each entity of its key kind (export plan §4.2): from a copy of
    /// the booted session, with its seeds, to its point; then reads its published tables' records that exist, each
    /// shown cell checked as a kind's are (§5.5), with the loader's own builders beside the export's.</summary>
    private static void Load(ExportInputs inputs, GameSession boot, LoaderDeclaration loader, IReadOnlyList<ExportKind> kinds,
                             BuilderRule exportRule, ExportReport report)
    {
        string where = $"loader {loader.Scope}";
        ExportKind keys = kinds.FirstOrDefault(kind => kind.Declaration.Id == loader.Keys)
                          ?? throw new ExportFailure($"{where}: its keys {loader.Keys} is no kind");
        if (inputs.StopOf is null) throw new ExportFailure($"{where} reads at {loader.ReadAt}, and no patch point is resolved");
        RunStop stop = inputs.StopOf(loader.ReadAt);
        Script script = inputs.ScriptByName(loader.Script)
                        ?? throw new ExportFailure($"{where} runs {loader.Script}, which the install lacks");
        if (ScriptNames.Normalized(script.Name) != stop.Script)
            throw new ExportFailure($"{where} reads at {loader.ReadAt}, which lies in {stop.Script}, not in {loader.Script}");
        RegistryView registry = inputs.Registry;
        BuilderRule rule = exportRule.With(loader.Builders);
        var skips = loader.Skip.Select(skip => (Field: registry.Resolve(keys.Table, skip.Field), skip.Value)).ToList();
        var tables = new List<ExportLoadedTable>();
        foreach (LoadedTableDeclaration declared in loader.Tables)
        {
            if (keys.Table.Fields.Any(field => field.Field == declared.Name))
                throw new ExportFailure($"{where}: table {declared.Name} has the name of a field of {keys.Table.Name}");
            TableLayout table = registry.Table(declared.Table);
            var loaded = new ExportLoadedTable(declared, loader.Scope, table, registry.FieldsOf(table),
                                               declared.Fields.Select(field => registry.Resolve(table, field)).ToList(),
                                               registry.Resolve(table, declared.Exists));
            if (loaded.Published.Any(field => field.IsString))
                throw new ExportFailure($"{where}: table {declared.Name} publishes a string, which a loaded table does not");
            tables.Add(loaded);
        }
        keys.Loaded.AddRange(tables);

        foreach (ExportEntity entity in keys.Entities)
        {
            if (skips.Any(skip => Read(boot, skip.Field, entity.Id, 0).Number == skip.Value)) continue;
            GameSession run = boot.CopyState();
            foreach ((string global, SeedValue value) in loader.Seed)
                run.Seed(registry.IntegerGlobal(global), value.For(entity.Id));
            var options = new VmOptions(MaxSteps: inputs.LoaderMaxSteps)
            {
                ProfileId = inputs.GameId, RecordsWrites = true, StopAt = stop,
            };
            SceneResult result = run.RunScene(script, inputs.Table, new HeadlessWaits(), options, inputs.Scripts,
                                              build: inputs.Build);
            report.LoaderRuns++;
            if (result.Halt != stop.HaltReason)
            {
                report.LoaderHalts.Add((loader.Scope, keys.Declaration.Id, entity.Id, result.Halt ?? ""));
                continue;
            }
            foreach (ExportLoadedTable table in tables)
                for (int record = 0; record < table.Table.Records; record++)
                {
                    if (Read(run, table.Exists, record, 0).IsDefault) continue;
                    string name = table.Declaration.Name;
                    // Every field's cells, as a kind's (§6.6): an integer's value is shown and checked; a string, which a
                    // loaded table does not publish, shows its address and writer only. Its cells are the key entity's,
                    // the field written <table>.<field> and the index the record times the width plus the element.
                    var cells = new Dictionary<GlobalCell, (ExportCell Cell, bool Refused)>();
                    foreach (FieldLayout field in table.TableFields)
                        for (int element = 0; element < field.Elements; element++)
                        {
                            ExportCell cell = Read(run, field, record, element);
                            string? reason = field.IsString ? null : RefusalOf(run, cell, rule);
                            int index = record * field.Elements + element;
                            if (reason != null)
                                report.Refusals.Add(new Refusal(keys.Declaration.Id, entity.Id, $"{name}.{field.Field}", index,
                                                                cell.Bank, cell.Address, reason));
                            cells[new GlobalCell(cell.Bank, cell.Address)] = (cell, reason != null);
                            if (cell.Writer != null || !cell.IsDefault)
                                entity.Cells.Add(new ExportModderCell($"{name}.{field.Field}", index, cell.Bank, cell.Address,
                                    reason != null || field.IsString ? null : cell.Number.ToString(CultureInfo.InvariantCulture),
                                    cell.Writer));
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
                    entity.Loaded.Add(new ExportLoadedRow(name, record, values));
                    entity.SnippetRows.Add((loader.Scope, name, record,
                        SnippetEntries(table.TableFields, table.Published, (field, column) =>
                        {
                            var (cell, refused) = At(field, column);
                            return refused ? null : new DataInteger(cell.Number);
                        })));
                }
        }
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
                                 TableLayout table, ExportReport report)
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

        GameSession run = boot.CopyState();
        foreach ((string global, SeedValue value) in description.Seed)
            run.Seed(inputs.Registry.IntegerGlobal(global), value.For(entity.Id));
        var shown = new DescriptionText(script.Name);
        SceneResult result = run.RunScene(script, inputs.Table, new HeadlessWaits(),
                                          new VmOptions(MaxSteps: inputs.DescriptionMaxSteps) { ProfileId = inputs.GameId },
                                          inputs.Scripts, build: inputs.Build, textListener: shown);
        report.DescriptionRuns++;
        report.DroppedPieces += shown.Dropped;
        if (result.Halt != "exit") report.DescriptionHalts.Add((kind.Id, entity.Id, description.Name, result.Halt ?? ""));
        if (shown.Text() is { Length: > 0 } text) entity.Texts.Add(new ExportText(description.Name, 0, text));
        else report.EmptyDescriptions++;
    }

    /// <summary>The text one script shows in a description run (spec §5.7), line by line; another script's runs are
    /// dropped and counted.</summary>
    private sealed class DescriptionText(string script) : ITextListener
    {
        private readonly List<StringBuilder> _lines = [new()];

        public int Dropped { get; private set; }

        public void RunShown(AdvLiveTextRun run, int glyphDelayMilliseconds) => Shown(run);

        public void HiddenRunShown(AdvLiveTextRun run) => Shown(run);

        public void LineEnded(int layoutSlot) => _lines.Add(new StringBuilder());

        private void Shown(AdvLiveTextRun run)
        {
            if (run.ScriptStack.Count > 0 && string.Equals(run.ScriptStack[^1], script, StringComparison.Ordinal))
                _lines[^1].Append(run.Text);
            else Dropped++;
        }

        /// <summary>The lines joined by <c>\n</c>, without the empty lines at the end.</summary>
        public string Text()
        {
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
