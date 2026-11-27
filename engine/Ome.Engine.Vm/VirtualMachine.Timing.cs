using Ome.Engine.Model;

namespace Ome.Engine.Vm;

public sealed partial class VirtualMachine
{
    private readonly record struct TimedCallback(long DeadlineMs, int PrimaryOffset, int CatchUpOffset);


    private void ResetTimedCallbackSequence()
    {
        _scene.TimedCallbacks.Clear();
        _scene.TimedCallbackCursor = 0;
        _scene.TimedCallbackStartedAtMs = 0;
        _scene.TimedCallbackAbortOffset = -1;
        _scene.TimedCallbackGuard = null;
    }

    private StepResult OpGetMonotonicTimeMs(Instruction ins, int pc)
    {
        var a = ins.Args;
        Write(a[0], unchecked((int)_waits.InputClockMilliseconds)); return StepResult.Next;
    }

    // 0xc8 (duration) — the waits sleep duration ms; headless stand-ins return at once (parity). Frame pacing.
    private StepResult OpSleep(Instruction ins, int pc)
    {
        var a = ins.Args;
        // sleep_op_0xc8@0x420ec0 arms nothing while ADV fast-forward (run-state 0x08000000) is active.
        if (AdvFastForwardActive) return StepResult.Next;
        _waits.Sleep(Read(a[0])); return StepResult.Next;
    }

    // Op 0xce's own interval timer (sleep_timer_arm on AGE 4.45D's ctx+0x68e00: start +0x14, interval +0x18), apart
    // from op 0xc8's (+0x68dc8) and op 0xcc's (+0x68de4).
    private int _timedCatchUpIntervalMs;
    private long _timedCatchUpArmedAtMs;

    // 0xce (interval ms, on-time target, catch-up target): register the engine's one timed catch-up callback, owned by
    // the running script, and start its timer (op_0xce_register_timed_catchup_callback; AGE 4.45D 0x421090, the same
    // code as 4.24J's 0x420fd0). sleep_timer_arm takes an interval of 0 as 1. A registration from any frame replaces
    // the earlier one.
    private StepResult OpRegisterTimedCatchUpCallback(Instruction ins, int pc)
    {
        var a = ins.Args;
        _scene.TimedCatchUpOnTimeTarget = unchecked((int)Read(a[1]));
        _scene.TimedCatchUpLateTarget = unchecked((int)Read(a[2]));
        _scene.TimedCatchUpOwner = _cur.Script;
        int interval = unchecked((int)Read(a[0]));
        _timedCatchUpIntervalMs = interval == 0 ? 1 : interval;
        _timedCatchUpArmedAtMs = _waits.InputClockMilliseconds;
        return StepResult.Next;
    }

    /// <summary>The most catch-up runs op 0xcf queues after the first: its loop stops at a count of 128.</summary>
    private const int MaxQueuedCatchUps = 127;

    // 0xcf (op_0xcf_dispatch_timed_catchup_callback; AGE 4.45D 0x41a830, the same code as 4.24J's 0x417f80). Once the
    // interval has passed (at once under fast-forward) it restarts the timer and pushes the following pc, as a call
    // does. Within two intervals it runs the on-time target; a -1 target pops the pc again. Later, it runs the catch-up
    // target instead, once for each further whole interval (at most 128 runs), then the on-time target, then returns
    // here: it pushes the on-time target and the further catch-ups as returns. A -1 catch-up target drops those and
    // runs nothing, but leaves the first pc pushed, so the frame's next ret comes back here. A target registered by
    // another script raises Depth with the pc still pushed: with the raw script ids on time (「Depth が不正です %d !=
    // %d」), with their names late.
    private StepResult OpDispatchTimedCatchUpCallback(Instruction ins, int pc)
    {
        long now = _waits.InputClockMilliseconds;
        int elapsed = unchecked((int)(now - _timedCatchUpArmedAtMs));
        int interval = _timedCatchUpIntervalMs;
        if (elapsed < interval && !AdvFastForwardActive) return StepResult.Next;
        _timedCatchUpArmedAtMs = now;
        // Native's reset leaves the owner -1, which is no frame's, and keeps the targets; the port's scene starts again
        // with no registration, so until op 0xce runs only the timer restarts.
        if (_scene.TimedCatchUpOwner is not { } owner) return StepResult.Next;
        _cur.CallStack.Add(pc + 1);
        if (elapsed < unchecked(interval * 2))
        {
            int onTime = _scene.TimedCatchUpOnTimeTarget;
            if (onTime == -1)
            {
                _cur.CallStack.RemoveAt(_cur.CallStack.Count - 1);
                return StepResult.Next;
            }
            if (!IsSameScript(owner, _cur.Script))
                return StepResult.Jump(RaiseInstructionScriptError(DepthIdError(owner), pc));
            return TryTimedCatchUpTarget(onTime, out int target) ? StepResult.Jump(target) : StepResult.Halt;
        }
        if (!IsSameScript(owner, _cur.Script))
            return StepResult.Jump(RaiseInstructionScriptError(DepthError(owner), pc));
        int queued = 0;
        for (int remaining = unchecked(elapsed - interval); interval < remaining && queued < MaxQueuedCatchUps; queued++)
            remaining = unchecked(remaining - interval);
        int late = _scene.TimedCatchUpLateTarget;
        if (late == -1) return StepResult.Next;
        if (!TryTimedCatchUpTarget(_scene.TimedCatchUpOnTimeTarget, out int onTimeReturn)
            || !TryTimedCatchUpTarget(late, out int catchUp))
            return StepResult.Halt;
        _cur.CallStack.Add(onTimeReturn);
        for (int run = 0; run < queued; run++) _cur.CallStack.Add(catchUp);
        return StepResult.Jump(catchUp);
    }

    // Native moves the frame's pc to the target's dword offset unchecked; the port stops at one that is no instruction's.
    private bool TryTimedCatchUpTarget(int offset, out int target)
    {
        if (_cur.Script.IndexByOffset.TryGetValue(offset, out target)) return true;
        HaltReason ??= $"timed-catchup-target-not-an-instruction:{offset:#x}";
        return false;
    }

    // 0xd3: clear the engine-wide relative schedule
    private StepResult OpBeginTimedCallbackSequence(Instruction ins, int pc)
    {
        ResetTimedCallbackSequence();
        return StepResult.Next;
    }

    // 0xd4: (interval ms, count, on-time PC, catch-up PC)
    private StepResult OpAppendRelativeTimedCallbacks(Instruction ins, int pc)
    {
        var a = ins.Args;
        long intervalMs = Read(a[0]);
        int count = System.Math.Max(0, unchecked((int)Read(a[1])));
        if (count > MaxScriptRunLength - _scene.TimedCallbacks.Count)
        {
            HaltReason ??= $"timed-callback-count-too-large:{count}";
            return StepResult.Halt;
        }
        int primaryOffset = unchecked((int)Read(a[2]));
        int catchUpOffset = unchecked((int)Read(a[3]));
        long deadlineMs = _scene.TimedCallbacks.Count == 0 ? 0 : _scene.TimedCallbacks[^1].DeadlineMs;
        for (int i = 0; i < count; i++)
        {
            deadlineMs += intervalMs;
            _scene.TimedCallbacks.Add(new TimedCallback(deadlineMs, primaryOffset, catchUpOffset));
        }
        return StepResult.Next;
    }

    // 0xd5: dispatch each scheduled local callback, resuming here after ret
    private StepResult OpRunTimedCallbackSequence(Instruction ins, int pc)
    {
        var a = ins.Args;
        // op_0xd5_handler@0x421090 starts the sequence when its cursor is 0: it records the abort pc and the
        // running script as the guard (+0x5f6b8) and starts the clock.
        if (_scene.TimedCallbackCursor == 0)
        {
            FlushActionTriggers();
            _scene.TimedCallbackStartedAtMs = _waits.InputClockMilliseconds;
            _scene.TimedCallbackAbortOffset = unchecked((int)Read(a[0]));
            _scene.TimedCallbackGuard = _cur.Script;
        }
        // Native op 0xd5 stops at last_index rather than count. The final entry is a
        // look-ahead sentinel: it supplies the next deadline for the preceding event but
        // is not itself dispatched.
        if (_scene.TimedCallbackCursor >= _scene.TimedCallbacks.Count - 1) return StepResult.Next;
        if (TryAbortTimedCallbackSequence(out int abortTarget)) return StepResult.Jump(abortTarget);

        var callback = _scene.TimedCallbacks[_scene.TimedCallbackCursor];
        long elapsedMs = _waits.InputClockMilliseconds - _scene.TimedCallbackStartedAtMs;
        if (elapsedMs < callback.DeadlineMs)
        {
            _waits.WaitForTimedCallbackDeadline(callback.DeadlineMs - elapsedMs);
            elapsedMs = _waits.InputClockMilliseconds - _scene.TimedCallbackStartedAtMs;
            if (TryAbortTimedCallbackSequence(out abortTarget)) return StepResult.Jump(abortTarget);
        }

        bool fellBehind = _scene.TimedCallbackCursor + 1 < _scene.TimedCallbacks.Count
            && _scene.TimedCallbacks[_scene.TimedCallbackCursor + 1].DeadlineMs < elapsedMs;
        int targetOffset = fellBehind ? callback.CatchUpOffset : callback.PrimaryOffset;
        // timed_callback_sequence_tick@0x408170: the schedule is engine-wide, so a frame that runs op 0xd5
        // while another script's sequence is under way dispatches that script's entry. When the guard is
        // another script, AGE raises Depth before the cursor moves; op 0xd5 recorded length 0, so 再試行 and
        // 無視 both run it again and the error repeats until 中止.
        if (targetOffset != -1 && _scene.TimedCallbackGuard is { } guard && !IsSameScript(guard, _cur.Script))
            return StepResult.Jump(RaiseInstructionScriptError(DepthError(guard), pc, ignoreSkipsInstruction: false));
        _scene.TimedCallbackCursor++;
        if (targetOffset < 0 || !_cur.Script.IndexByOffset.TryGetValue(targetOffset, out int target))
            return StepResult.Jump(pc);
        _cur.CallStack.Add(pc);
        return StepResult.Jump(target);
    }

    /// <summary>timed_callback_sequence_tick@0x408170 polls the input latch on every tick of run-state 0x40, which op
    /// 0xd5 sets until its last entry. Any trigger moves the frame to the abort pc (op 0xd5's operand, recorded at
    /// cursor 0) and clears 0x40; with no abort pc (-1: 16 of Himegari's 17 sequences, 14 of Kamidori's 15) the trigger
    /// is dropped. The cursor stays. The port polls at each op 0xd5 step, before and after the wait for the next entry
    /// (the literal entry intervals in both games' scripts are 33 ms at most).</summary>
    private bool TryAbortTimedCallbackSequence(out int target)
    {
        target = -1;
        if (TakeHeldTriggers() == 0 || _scene.TimedCallbackAbortOffset == -1) return false;
        return _cur.Script.IndexByOffset.TryGetValue(_scene.TimedCallbackAbortOffset, out target);
    }
}
