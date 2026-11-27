using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Ome.Engine.Diagnostics;

// Frontend-side trace consumer. Runs on the VM background thread, so it just queues the dispatched
// call-script ids; the main thread drains them (Godot drops GD.Print from background threads). This
// replaced a host callback that reported each dispatch: subroutine visibility is now an
// engine fact delivered over the trace seam.
//
// A step is the event of every instruction, so recording one takes no lock and allocates nothing: the engine thread,
// the only thread that emits, writes it into a ring and counts it, and a reader on another thread copies what it needs
// of the ring and keeps only the steps the engine thread cannot have rewritten meanwhile (RecentStepsCopy). Frames
// enter and leave rarely, so the script stack and the halt snapshot keep a lock.
public sealed class GodotTraceSink : ITraceSink
{
    private const int RecentStepCapacity = 128;
    // The ring holds twice the steps a snapshot keeps, so a step the engine thread writes while another thread copies
    // the newest RecentStepCapacity overwrites only an older one (RecentStepsCopy). A power of two: a step's slot is its
    // number masked.
    private const int RingSize = RecentStepCapacity * 2;
    private const string UnknownScript = "<unknown>";
    private readonly TimelineLog? _timeline;
    private readonly PageLocatorState _locator;
    private readonly object _snapshotLock = new();
    private readonly Stack<string> _scripts = new();
    private readonly GodotTraceStepSnapshot[] _ring = new GodotTraceStepSnapshot[RingSize];
    private long _steps;                     // the steps written to the ring: the engine thread publishes it, Volatile
    private string _script = UnknownScript;  // the top frame's script, for its steps: the engine thread's own
    private GodotTraceSnapshot? _haltSnapshot;
    public GodotTraceSink(PageLocatorState locator, TimelineLog? timeline = null)
    { _locator = locator; _timeline = timeline; }
    // The page locator needs the exact script/offset even when the heavier timeline log is disabled.
    public bool TracingSteps => true;
    public readonly ConcurrentQueue<long> CallScripts = new();
    public void Emit(in TraceEvent e)
    {
        if (e.Kind == TraceEventKind.Step)
        {
            if (e.Ins != null) Step(e.Ins.Offset, e.Opcode, e.Depth);
            return;
        }
        if (e.Kind == TraceEventKind.CallScript)
        {
            CallScripts.Enqueue(e.Id);
            _timeline?.Event("call-script", new()
            {
                ["id"] = $"0x{e.Id:x}", ["resolved_name"] = e.Name,
            });
        }
        if (e.Kind == TraceEventKind.FrameEnter && e.Name != null)
        {
            string[] callStack;
            lock (_snapshotLock)
            {
                _scripts.Push(e.Name);
                _script = e.Name;
                callStack = CurrentCallStackLocked();
            }
            _locator.CallStack(callStack);
            _timeline?.Event("frame-enter", new()
            {
                ["name"] = e.Name, ["depth"] = e.Depth,
                ["cause"] = e.Cause.ToString(), ["call_id"] = $"0x{e.Id:x}",
            });
        }
        else if (e.Kind == TraceEventKind.FrameExit && _scripts.Count > 0)
        {
            _timeline?.Event("frame-exit", new()
            {
                ["name"] = e.Name, ["depth"] = e.Depth, ["outcome"] = e.Text,
            });
            string[] callStack;
            lock (_snapshotLock)
            {
                if (e.Text == "Halted" && _haltSnapshot == null)
                    _haltSnapshot = SnapshotLocked();
                if (_scripts.Count > 0) _scripts.Pop();
                _script = _scripts.Count > 0 ? _scripts.Peek() : UnknownScript;
                callStack = CurrentCallStackLocked();
            }
            _locator.CallStack(callStack);
        }
        else if (e.Kind == TraceEventKind.Stub)
            _timeline?.Event("stub", new()
            {
                ["stub_opcode"] = $"0x{e.Opcode:x}", ["pc_index"] = e.Pc,
            });
        else if (e.Kind == TraceEventKind.UnsupportedOpcode
                 && e.CompatibilityDiagnostic is { } diagnostic)
            _timeline?.Event("unsupported-opcode", new()
            {
                ["profile_id"] = diagnostic.ProfileId,
                ["engine_abi_id"] = diagnostic.EngineAbiId,
                ["script_revision"] = diagnostic.ScriptRevision,
                ["script_name"] = diagnostic.ScriptName,
                ["packed_script_id"] = $"0x{diagnostic.PackedScriptId:x}",
                ["offset"] = $"0x{diagnostic.Offset:x}",
                ["opcode"] = $"0x{diagnostic.Opcode:x}",
                ["canonical_label"] = diagnostic.CanonicalLabel,
                ["operands"] = diagnostic.Operands.Select(value => $"{value.Type}:{value.Value}").ToArray(),
            });
        else if (e.Kind == TraceEventKind.Halt)
            _timeline?.State("halted", new() { ["reason"] = e.Text, ["steps"] = e.Steps });
    }

    public GodotTraceSnapshot Snapshot()
    {
        lock (_snapshotLock) return SnapshotLocked();
    }

    /// <summary>The deepest still-active script stack captured before a halted frame unwinds.</summary>
    public GodotTraceSnapshot? HaltSnapshot
    {
        get { lock (_snapshotLock) return _haltSnapshot; }
    }

    /// <summary>The newest step, from any thread, null before the first: an allocation-free coordinate for
    /// once-per-frame diagnostics.</summary>
    public GodotTraceStepSnapshot? LatestStep
    {
        get
        {
            while (true)
            {
                long steps = Volatile.Read(ref _steps);
                if (steps == 0) return null;
                GodotTraceStepSnapshot step = _ring[(steps - 1) & (RingSize - 1)];
                Interlocked.MemoryBarrier();
                // Kept by the rule of RecentStepsCopy; read again in the (never seen) case the engine thread ran on so
                // far that it may have overwritten the slot.
                if (Volatile.Read(ref _steps) - steps < RingSize - RecentStepCapacity) return step;
            }
        }
    }

    // The engine thread's step. Its slot is written before the count that publishes it (a release write), so a reader
    // that has read the count sees the step.
    private void Step(int offset, int opcode, int depth)
    {
        long step = _steps;
        string script = _script;
        _ring[step & (RingSize - 1)] = new GodotTraceStepSnapshot(script, offset, opcode, depth);
        Volatile.Write(ref _steps, step + 1);
        _locator.Step(script, offset);
        _timeline?.Step(script, offset, opcode, depth);
    }

    /// <summary>The newest steps, oldest first, at most <see cref="RecentStepCapacity"/>, from any thread. The engine
    /// thread may go on stepping while another thread copies them, and a step it writes overwrites the one
    /// <see cref="RingSize"/> before it. So the copy keeps the steps among the newest RecentStepCapacity counted once
    /// it is done: overwriting one of those would take the engine thread RecentStepCapacity steps past the count it
    /// had published, more than any processor's reordering of its stores. On the engine thread it keeps them all.
    /// </summary>
    private GodotTraceStepSnapshot[] RecentStepsCopy()
    {
        long end = Volatile.Read(ref _steps);
        long start = System.Math.Max(0, end - RecentStepCapacity);
        var steps = new GodotTraceStepSnapshot[end - start];
        for (long step = start; step < end; step++) steps[step - start] = _ring[step & (RingSize - 1)];
        Interlocked.MemoryBarrier();
        long intact = Volatile.Read(ref _steps) - RecentStepCapacity;
        return intact <= start ? steps : steps[(int)System.Math.Min(intact - start, steps.Length)..];
    }

    private string[] CurrentCallStackLocked()
    {
        var stack = _scripts.ToArray();
        System.Array.Reverse(stack);
        return stack;
    }

    private GodotTraceSnapshot SnapshotLocked()
    {
        GodotTraceStepSnapshot[] recent = RecentStepsCopy();
        GodotTraceStepSnapshot? current = recent.Length > 0 ? recent[^1] : null;
        return new GodotTraceSnapshot(
            current?.Script ?? (_scripts.Count > 0 ? _scripts.Peek() : UnknownScript),
            current?.Offset ?? -1,
            current?.Opcode ?? -1,
            current?.Depth ?? System.Math.Max(0, _scripts.Count - 1),
            CurrentCallStackLocked(),
            recent);
    }
}

/// <summary>One step of the trace: a value, so the sink's ring and <see cref="GodotTraceSink.LatestStep"/> allocate
/// nothing.</summary>
public readonly record struct GodotTraceStepSnapshot(string Script, int Offset, int Opcode, int Depth);

public sealed record GodotTraceSnapshot(string CurrentScript, int CurrentOffset, int CurrentOpcode,
                                        int CurrentDepth, IReadOnlyList<string> CallStack,
                                        IReadOnlyList<GodotTraceStepSnapshot> RecentSteps);
