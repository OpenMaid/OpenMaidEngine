using System.Text.Json;
using System.Text.Json.Serialization;
using Ome.Engine.Diagnostics;
using Ome.Engine.Hosting;
using Ome.Engine.Localization;
using Ome.Engine.Model;
using Ome.Engine.Natives;
using Ome.Engine.Persistence;

namespace Ome.Engine.Vm;

/// <summary>
/// A persistent store carried across scenes. AGE keeps separate integer, float, string, and pointer
/// banks; running scenes in isolation with empty state is why our headless VM
/// diverges from the real game (the bg/sprite geometry drift, the state-gated EMPTY scenes, Lily's
/// form-gated voices are all state divergence — see docs/phase-a-slice-plan.md A2b-Geometry).
///
/// <para>Seam rule: references only <c>Model</c> + <c>Hosting</c> (never <c>Sys4</c>). The
/// <see cref="VirtualMachine"/> is unchanged — its globals are pre-seeded before <c>Run</c> and merged
/// back after, so trace/selftest parity is untouched. Local frames are per-call and correctly do NOT
/// persist (they live in the VM, not here).</para>
/// </summary>
public sealed partial class GameSession
{
    public Dictionary<int, long> Globals { get; } = new();
    public Dictionary<int, long> GlobalFloats { get; } = new();
    public Dictionary<int, string> GlobalStrings { get; } = new();
    public Dictionary<int, int> GlobalPointers { get; } = new();
    public Dictionary<int, int> GlobalStringPointers { get; } = new();
    /// <summary>Each global cell's writer, from the recording runs (game data export spec §6.2): the session carries it
    /// from one to the next, as it carries the banks. Empty after a run that did not record, since that run may have
    /// written any cell.</summary>
    public Dictionary<GlobalCell, string> Writers { get; } = new();
    /// <summary>Each global string cell's origin, from the recording runs, as <see cref="Writers"/>: only records whose
    /// cell still holds the text the stored instruction wrote.</summary>
    public Dictionary<int, TextManager.StringStore> StringStores { get; } = new();
    /// <summary>AGE's selected profile-wide cells plus native shared SAVE.DAT/RT.DAT lifecycle.</summary>
    public SharedProfile SharedProfile { get; }
    /// <summary>Native shared/numbered save directory service used by persistence opcodes.</summary>
    public INativeDatStore? NativeDatStore { get; }
    /// <summary>AGE's profile-lifetime sound:* settings registry, independent of SAVE.DAT.</summary>
    public AudioMixerSettings AudioMixerSettings { get; }
    /// <summary>The live retained ADV backlog shared by every VM run in this session.</summary>
    public AdvTextHistory TextHistory { get; } = new();
    /// <summary>The EngineCtx-lifetime diagnostic accumulator shared across script/scene VMs.</summary>
    public DiagnosticOutputState DiagnosticOutput { get; } = new();
    /// <summary>AGERC.DLL's map, which lives as long as AGE's program: every scene VM of the session calls the same one.
    /// </summary>
    public AgercMap AgercMap { get; } = new();

    public GameSession(SharedProfile? sharedProfile = null, INativeDatStore? nativeDatStore = null,
                       AudioMixerSettings? audioMixerSettings = null)
    {
        SharedProfile = sharedProfile ?? new SharedProfile();
        NativeDatStore = nativeDatStore;
        AudioMixerSettings = audioMixerSettings ?? new AudioMixerSettings();
    }

    public void Seed(int addr, long value) => Globals[addr] = value;
    public void SeedString(int addr, string value) => GlobalStrings[addr] = value;

    /// <summary>Run one scene: seed a fresh VM from session state, execute, merge final state back.</summary>
    /// <param name="scripting">The scripting native of this run (Lua scripting spec §14.1), with its mods' hooks. It
    /// attaches to this run's VM and its script memory is the run's, so each run takes its own.</param>
    /// <param name="textListener">Hears what this run's text native shows (<see cref="ITextListener"/>): the runs with
    /// their ruby and script stack, and the line ends, which <see cref="SceneResult.Emitted"/> does not keep (game data
    /// export spec §5.7).</param>
    /// <param name="presenter">The language the run presents its text in (localization spec §5.2), or null for the
    /// source text. A run with one records the string stores, from the session's, so a drawn string resolves at the
    /// instruction that stored it, as it does in the port (game data export spec §6.4).</param>
    public SceneResult RunScene(Script script, OpcodeTable table, IInputAndWaits waits,
                                VmOptions? options = null, IScriptProvider? provider = null,
                                ITraceSink? sink = null,
                                EngineBuild? build = null, Func<SkipAndAuto, Audio>? audio = null,
                                Scripting? scripting = null, ITextListener? textListener = null,
                                TextPresenter? presenter = null)
    {
        // Each run has its own skip and auto native, and an audio native over it (audio builds one; without it the run
        // plays nothing). The mixer settings are the session's. Its save native reads and writes the session's profile
        // and store, and its shell native appends to the session's diagnostic text and calls the session's AGERC map.
        SkipAndAuto skip = SkipAndAuto.WithSystemClock();
        var shell = ShellServices.Headless(DiagnosticOutput, AgercMap);
        shell.WakeWaitsWith(waits.WakeInputCallbackService);
        var vm = new VirtualMachine(
            script, table, waits, options, provider, sink, TextHistory,
            saves: new SavesAndSettings(SharedProfile, NativeDatStore), shell: shell, build: build, skipAndAuto: skip,
            audio: audio?.Invoke(skip) ?? Audio.Silent(skip, AudioMixerSettings), scripting: scripting);
        if (textListener != null) vm.Text.Listen(textListener);
        if (presenter != null)
        {
            vm.Text.Presenter = presenter;
            vm.RecordsStringStores = true;
        }
        foreach (var kv in Globals) vm.SeedGlobal(kv.Key, kv.Value);
        foreach (var kv in GlobalFloats) vm.GlobalFloats[kv.Key] = kv.Value;
        foreach (var kv in GlobalStrings) vm.GlobalStrings[kv.Key] = kv.Value;
        foreach (var kv in GlobalPointers) vm.GlobalPointers[kv.Key] = kv.Value;
        foreach (var kv in GlobalStringPointers) vm.GlobalStringPointers[kv.Key] = kv.Value;
        bool records = options?.RecordsWrites == true;
        if (records)
            foreach (var kv in Writers) vm.Memory.SeedWriter(kv.Key, kv.Value);
        if (records || presenter != null)
            foreach (var kv in StringStores) vm.Text.SeedStringStore(kv.Key, kv.Value);

        vm.Run();

        // The VM started from every session cell, so its final banks are the whole state: a cell the scene removed
        // (op 0x9 clears the banks) must not come back from the session's copy.
        Replace(Globals, vm.Globals);
        Replace(GlobalFloats, vm.GlobalFloats);
        Replace(GlobalStrings, vm.GlobalStrings);
        Replace(GlobalPointers, vm.GlobalPointers);
        Replace(GlobalStringPointers, vm.GlobalStringPointers);
        // A recording run's records are the session's (game data export spec §6.2), each origin while its cell holds
        // the text; a run that did not record may have written any cell, so no record holds after it.
        Writers.Clear();
        StringStores.Clear();
        if (records)
        {
            foreach (var kv in vm.Memory.Writers) Writers[kv.Key] = kv.Value;
            foreach (var kv in vm.Text.StringStores)
                if (GlobalStrings.TryGetValue(kv.Key, out string? text)
                    && string.Equals(text, kv.Value.Text, StringComparison.Ordinal))
                    StringStores[kv.Key] = kv.Value;
        }

        return new SceneResult(vm.Emitted.ToList(), vm.HaltReason, vm.Steps);
    }

    private static void Replace<T>(Dictionary<int, T> session, IReadOnlyDictionary<int, T> final)
    {
        session.Clear();
        foreach (var kv in final) session[kv.Key] = kv.Value;
    }

    /// <summary>A new session holding this session's banks and records, as <c>FromJson(ToJson())</c> would but without
    /// the text between: the game data export starts each run from its boot so (spec §5.7, §6.3). Like a snapshot, it
    /// leaves out the shared profile, the save store, the mixer settings, History and the diagnostic text; the copy
    /// starts with new ones.</summary>
    public GameSession CopyState()
    {
        var copy = new GameSession();
        Copy(Globals, copy.Globals);
        Copy(GlobalFloats, copy.GlobalFloats);
        Copy(GlobalStrings, copy.GlobalStrings);
        Copy(GlobalPointers, copy.GlobalPointers);
        Copy(GlobalStringPointers, copy.GlobalStringPointers);
        Copy(Writers, copy.Writers);
        Copy(StringStores, copy.StringStores);
        return copy;
    }

    private static void Copy<TKey, T>(Dictionary<TKey, T> from, Dictionary<TKey, T> to) where TKey : notnull
    {
        to.EnsureCapacity(from.Count);
        foreach (var kv in from) to[kv.Key] = kv.Value;
    }

    /// <summary>Serialize the persistent global banks (integer, float, string, pointer and string pointer) to JSON,
    /// keyed by decimal address, with the recording runs' records when there are any (<see cref="Writers"/>,
    /// <see cref="StringStores"/>).
    /// Lets an expensive booted state be snapshotted and reused. Live <see cref="TextHistory"/> is
    /// intentionally excluded until the unified save/profile architecture defines its lifecycle.</summary>
    public string ToJson()
    {
        var snap = new StateSnapshot(
            Keyed(Globals), Keyed(GlobalStrings), Keyed(GlobalFloats), Keyed(GlobalPointers), Keyed(GlobalStringPointers))
        {
            Writers = Writers.Count == 0 ? null : KeyedWriters(Writers),
            Origins = StringStores.Count == 0
                ? null
                : StringStores.ToDictionary(
                    kv => kv.Key.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    kv => new SnapshotOrigin(kv.Value.Origin.Script, kv.Value.Origin.Offset, kv.Value.Origin.IsBaseScript,
                                             kv.Value.Text)),
        };
        return JsonSerializer.Serialize(snap, SnapshotJson.StateSnapshot);
    }

    /// <summary>Rebuild a session from a <see cref="ToJson"/> snapshot.</summary>
    public static GameSession FromJson(string json)
    {
        var s = new GameSession();
        var snap = JsonSerializer.Deserialize(json, SnapshotJson.StateSnapshot) ?? new StateSnapshot(new(), new());
        Unkeyed(snap.Globals, s.Globals);
        Unkeyed(snap.Strings, s.GlobalStrings);
        Unkeyed(snap.Floats, s.GlobalFloats);
        Unkeyed(snap.Pointers, s.GlobalPointers);
        Unkeyed(snap.StringPointers, s.GlobalStringPointers);
        foreach ((string bank, Dictionary<string, string> cells) in snap.Writers ?? [])
        {
            GlobalBank parsed = Enum.Parse<GlobalBank>(bank);
            foreach ((string address, string script) in cells)
                s.Writers[new GlobalCell(parsed, int.Parse(address, System.Globalization.CultureInfo.InvariantCulture))] =
                    script;
        }
        foreach ((string cell, SnapshotOrigin origin) in snap.Origins ?? [])
            s.StringStores[int.Parse(cell, System.Globalization.CultureInfo.InvariantCulture)] = new TextManager.StringStore(
                new Ome.Engine.Localization.TextSite(origin.Script, origin.Offset, origin.IsBaseScript), origin.Text);
        return s;
    }

    /// <summary>The writers by bank name, then by decimal address.</summary>
    private static Dictionary<string, Dictionary<string, string>> KeyedWriters(Dictionary<GlobalCell, string> writers)
    {
        var keyed = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
        foreach ((GlobalCell cell, string script) in writers)
        {
            string bank = cell.Bank.ToString();
            if (!keyed.TryGetValue(bank, out var cells)) keyed[bank] = cells = new(StringComparer.Ordinal);
            cells[cell.Address.ToString(System.Globalization.CultureInfo.InvariantCulture)] = script;
        }
        return keyed;
    }

    private static Dictionary<string, T> Keyed<T>(Dictionary<int, T> bank)
        => bank.ToDictionary(kv => kv.Key.ToString(System.Globalization.CultureInfo.InvariantCulture), kv => kv.Value);

    private static void Unkeyed<T>(Dictionary<string, T>? keyed, Dictionary<int, T> bank)
    {
        if (keyed == null) return;
        foreach (var kv in keyed)
            bank[int.Parse(kv.Key, System.Globalization.CultureInfo.InvariantCulture)] = kv.Value;
    }

    /// <summary>How a snapshot is written: non-ASCII text as it is. The metadata is source-generated, since the engine ships
    /// compiled with NativeAOT, where reflection-based serialization is off.</summary>
    private static readonly SnapshotJsonContext SnapshotJson = new(new JsonSerializerOptions
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    });

    [JsonSerializable(typeof(StateSnapshot))]
    private sealed partial class SnapshotJsonContext : JsonSerializerContext;

    /// <summary>A snapshot from before the float and pointer banks were saved has only the first two. The records are
    /// written only when a recording run left any, so a snapshot without them reads as before.</summary>
    private sealed record StateSnapshot(Dictionary<string, long> Globals, Dictionary<string, string> Strings,
                                        Dictionary<string, long>? Floats = null, Dictionary<string, int>? Pointers = null,
                                        Dictionary<string, int>? StringPointers = null)
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, Dictionary<string, string>>? Writers { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, SnapshotOrigin>? Origins { get; init; }
    }

    /// <summary>A string cell's origin in a snapshot: the storing instruction's site and the text it wrote.</summary>
    private sealed record SnapshotOrigin(string Script, int Offset, bool IsBaseScript, string Text);
}

/// <summary>The observable result of running one scene into a <see cref="GameSession"/>.</summary>
public sealed record SceneResult(IReadOnlyList<(int Offset, string Text, string Script)> Emitted, string? Halt, long Steps);
