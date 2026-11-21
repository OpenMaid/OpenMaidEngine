namespace Ome.Engine.Natives;

/// <summary>The global bank a cell lies in.</summary>
public enum GlobalBank : byte { Integer, Float, String, Pointer, StringPointer }

/// <summary>One global cell: its bank and its address.</summary>
public readonly record struct GlobalCell(GlobalBank Bank, int Address);

/// <summary>
/// The writer record (game data export spec §6.2): for each global cell, the script that last wrote it. A run turns it
/// on (<c>VmOptions.RecordsWrites</c>), and the game data export reads it to refuse a value a scene wrote (spec §5.5).
/// </summary>
/// <remarks>
/// <para>A write is what <see cref="SetInteger"/> and its siblings make: every write of a script, of a native while a
/// script's instruction runs, and of Lua, each named by the script whose frame was running. A seed, a load and a scene
/// reset are no write: a seed leaves the record as it is, and a load and a reset drop it with the banks they replace.
/// </para>
/// <para>The record is scene state, as the banks are. Off, a write costs one test.</para>
/// </remarks>
public sealed partial class ScriptMemory
{
    // The running script's name while the record is on, or null.
    private Func<string>? _runningScript;

    /// <summary>Whether writes are recorded.</summary>
    public bool RecordsWriters => _runningScript != null;

    /// <summary>Records each write from now on with the name <paramref name="runningScript"/> gives, the running
    /// script's; null stops recording and keeps the record.</summary>
    public void RecordWriters(Func<string>? runningScript) => _runningScript = runningScript;

    /// <summary>The script that last wrote each cell the record holds.</summary>
    public IReadOnlyDictionary<GlobalCell, string> Writers => _scene.Writers;

    /// <summary>A cell's writer as a tool brings it back from an earlier run or a snapshot.</summary>
    public void SeedWriter(GlobalCell cell, string script) => _scene.Writers[cell] = script;

    private void Wrote(GlobalBank bank, int address)
    {
        if (_runningScript != null) _scene.Writers[new GlobalCell(bank, address)] = _runningScript();
    }
}
