namespace Ome.Engine.Vm;

/// <summary>
/// Native AGE's input latch (docs/engine-re.md, "Effect skipping and the input latch"). A mapped click
/// (input_latch_mouse_click@0x485e20, ctx+0x13e0) or key stays latched until a service polls it through
/// input_poll_triggered_action_mask@0x460880, which takes every latched trigger at once. The port hands a trigger to
/// its consumer as it arrives wherever a native service would poll it at once (a text wait, a reveal, a movie, an
/// effect under system:EffectSkipOnClick). This latch keeps the triggers that arrive while nothing polls, which are:
/// <list type="bullet">
/// <item>an effect service that does not poll. That is any effect unless EffectSkipOnClick is 1, and a run-state-0x400
/// effect while service-flags bit 0 is set;</item>
/// <item>a script sleep (op 0xc8), the ops between two waits, and a hotspot callback: anywhere but a polling text wait
/// (<see cref="IsAdvTextWaitPolling"/>) or a running reveal. The frontend holds these and the effect triggers with
/// <see cref="HoldTriggers"/>;</item>
/// <item>an ADV coroutine yield (handlers A and B and the hidden-window wait), through
/// <see cref="TryLatchAdvCoroutineYieldTrigger"/>.</item>
/// </list>
/// The next native poll takes the latch:
/// <list type="bullet">
/// <item>the hidden-window wait: any trigger ends it;</item>
/// <item>a text reveal's first tick, unless ctx+0x55108 is set: action 4 finishes it;</item>
/// <item>a modal movie's first tick: action 4 cancels it;</item>
/// <item>an op-0xd5 timed sequence: any trigger moves the frame to the abort pc, or is dropped when that is -1;</item>
/// <item>a text wait resumed by op 0x7c: the triggers count as fresh input.</item>
/// </list>
/// These discard it:
/// <list type="bullet">
/// <item>op 0x72's entry, op 0xfa, and a hotspot callback's return to its wait (the return re-runs op 0x72), unless
/// fast-forward is on;</item>
/// <item>always: op 0x101, input_flush_action_triggers@0x405380 (legacy fade starts, op 0xd5, hotspot dispatch, the
/// skip-click cancel) and a scene reset.</item>
/// </list>
/// The flush and the scene reset also arm the click suppressor while a mouse button is down
/// (<see cref="FlushActionTriggers"/>).
/// </summary>
public sealed partial class VirtualMachine
{
    /// <inheritdoc cref="Ome.Engine.Model.InputBindings.AdvanceAction"/>
    public const int AdvanceAction = Ome.Engine.Model.InputBindings.AdvanceAction;

    /// <summary>Logical action 5, Space in both games: at a polling text wait it enters the frame's handler A, which
    /// hides the window (adv_input_service_poll@0x411230).</summary>
    public const int AdvWaitCoroutineYieldAction = 5;

    private const int AdvanceTrigger = 1 << AdvanceAction;

    private int _heldTriggers;   // logical action bits; bit 31 is the native menu's hide-window request
    private int _mouseButtonState;   // GetAsyncKeyState's buttons, left 0x1 and right 0x2: device state a scene keeps
    private bool _clickSuppressorArmed;   // ctx+0x1c30

    /// <summary>The triggers the latch holds (logical action bits).</summary>
    public int HeldTriggers => _heldTriggers;

    /// <summary>Hold triggered logical actions (bit = 1 &lt;&lt; action) in the latch while nothing polls it. The
    /// frontend calls this for a trigger that arrives during an effect service that does not poll.</summary>
    public void HoldTriggers(int actionMask)
    {
        if (actionMask == 0) return;
        _heldTriggers |= actionMask;
        _waits.WakeInputCallbackService();
    }

    private int TakeHeldTriggers() => Take(ref _heldTriggers);

    /// <summary>input_flush_action_triggers (4.33G 0x405380, 4.24N 0x405430), which scene_context_init_reset and the
    /// debug reload repeat: take the latch and drop it, then arm the one-shot click suppressor (ctx+0x1c30) if the
    /// left or right button is down (input_poll_raw_mouse_buttons, GetAsyncKeyState). The click that button latches
    /// next, its release under CLICKONUP, then reaches nothing (<see cref="TryConsumeClickSuppressor"/>). Verita's
    /// field walks while the button is held and starts an event through op 0xd5 (FIELD@0x3172), so letting go does
    /// not advance the event's first page.</summary>
    private void FlushActionTriggers()
    {
        TakeHeldTriggers();
        if ((_mouseButtonState & 0x3) != 0) _clickSuppressorArmed = true;
    }

    /// <summary>The legacy transitions flush as they start, except under fast-forward.</summary>
    private void FlushActionTriggersUnlessFastForward()
    {
        if (!AdvFastForwardActive) FlushActionTriggers();
    }

    /// <summary>input_latch_mouse_click (4.33G 0x485e20, 4.24N 0x487690): with the suppressor armed, a click clears it
    /// and latches nothing. True means this click is the one it takes.</summary>
    public bool TryConsumeClickSuppressor()
    {
        if (!_clickSuppressorArmed) return false;
        _clickSuppressorArmed = false;
        return true;
    }

    /// <summary>Op 0x72's entry and op 0xfa poll the latch and discard it unless run-state 0x8000000
    /// (fast-forward) is on.</summary>
    private void DiscardHeldTriggersUnlessFastForward()
    {
        if (!AdvFastForwardActive) TakeHeldTriggers();
    }

    /// <summary>adv_text_reveal_service_tick@0x406fa0 polls the latch on every tick, so its first tick takes what the
    /// latch holds. A held action 4 finishes the reveal and sets ctx+0x55108, so the page's later reveals publish at
    /// once, without polling, until op 0x72. Under fast-forward, or with no glyph delay, no reveal runs, so nothing
    /// polls.</summary>
    private void OfferHeldTriggersToReveal()
    {
        if (_text.MessageGlyphDelayMilliseconds <= 0 || AdvFastForwardActive || _text.TextRevealsFinishedUntilWait)
            return;
        if ((TakeHeldTriggers() & AdvanceTrigger) != 0) _text.FinishTextRevealsUntilWait();
    }

    /// <summary>The modal movie service (run-state 0x2000) polls on its first tick while a service movie plays
    /// (<see cref="Ome.Engine.Model.MovieFlagBits.IsServiceMovie"/>): a held action 4 stops the movie. It does not read EffectSkipOnClick.
    /// For any other movie nothing polls, and the latch keeps what it holds.</summary>
    private void OfferHeldTriggersToModalMovie()
    {
        if ((TakeHeldTriggers() & AdvanceTrigger) != 0) _waits.CancelNextModalMovie();
    }

    /// <summary>adv_input_service_poll@0x411230 at a text wait that resumed without re-running op 0x72. Op 0x7c
    /// returns to the wait that Space parked, and the wait takes what handler B's effects held. The poll handles the
    /// held triggers as fresh input. An op-0x97 bound action runs its record first. Then action 5 enters handler A
    /// again. Then action 4 activates the current hit or, with none, advances the page. Activations and the yield are
    /// queued for the caller's dispatch.</summary>
    private void RouteHeldTriggersAtTextWait()
    {
        if (AdvFastForwardActive) return;   // op 0x72 leaves the wait unarmed (no 0x80000000): nothing polls
        int held = TakeHeldTriggers() & int.MaxValue;
        if (held == 0) return;
        if (TryActivateInputActions(held)) return;
        if ((held & (1 << AdvWaitCoroutineYieldAction)) != 0 && TryRequestAdvWaitCoroutineYield()) return;
        if ((held & (1 << Ome.Engine.Model.InputBindings.FastForwardAction)) != 0)
        {
            if (!_skip.StartPhysicalFastForwardFromLatchedTrigger(InputBindings)) _waits.EndInputWait();
            return;
        }
        if ((held & AdvanceTrigger) == 0) return;
        bool activated;
        activated = _scene.InteractiveFrame?.Hotspots.Activate() == true;
        if (!activated) _waits.EndInputWait();
    }
}
