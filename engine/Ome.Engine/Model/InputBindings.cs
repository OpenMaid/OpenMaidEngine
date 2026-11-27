namespace Ome.Engine.Model;

/// <summary>
/// AGE's process-owned physical-to-logical input map. Scripts configure the logical action count and
/// keyboard/mouse/joystick bindings; op 0xff polls this state and op 0x100 dispatches the resulting mask.
/// Win32 virtual-key values are retained as the keyboard ABI because native op 0x10c translates DIK scan
/// codes through that table before installing a mapping.
/// </summary>
public sealed class InputBindings
{
    public const int MaximumActions = 32;
    public const int DefaultActionCount = 7;
    public const double JoystickAxisThreshold = 0.5;

    /// <summary>The joystick axes AGE 4.45D's op 0x2f9 reads: X, Y, Z, R, U and V (<c>JOYINFOEX</c>'s six positions).
    /// The port's are the front end's axes 0 to 5; the directions use 0 and 1.</summary>
    public const int JoystickAxisCount = 6;

    /// <summary><c>JOY_POVCENTERED</c>: the POV is not pressed.</summary>
    public const int JoystickPovCentered = 0xffff;

    /// <summary>WinMM's axis range, 0 to 65535, which the port takes for every axis: native divides a position by its
    /// caps' max - min.</summary>
    public const int JoystickAxisRange = 65535;

    /// <summary>Each joystick reader polls <c>joyGetPosEx</c> again only once more than this many milliseconds have
    /// passed since its last poll.</summary>
    public const int JoystickPollIntervalMs = 20;

    /// <summary>Logical action 4, the left click in both games: it advances a page, finishes a reveal, skips an effect
    /// (EffectSkipOnClick 1) and cancels a modal movie.</summary>
    public const int AdvanceAction = 4;

    /// <summary>Logical action 6, Ctrl, C and Backspace in both games: held, it fast-forwards ADV text (run-state
    /// 0x08000000; <c>Ome.Engine.Natives.SkipAndAuto</c>).</summary>
    public const int FastForwardAction = 6;

    private readonly int[] _keyboardActions = Enumerable.Repeat(-1, 256).ToArray();
    private readonly int[] _mouseButtonSlots = new int[2];
    private readonly int[] _joystickButtons = new int[MaximumActions];
    private readonly HashSet<int> _heldVirtualKeys = new();
    /// <summary>The keys pressed since op 0x10e last asked for them: GetAsyncKeyState's bit 0.</summary>
    private readonly HashSet<int> _pressedSinceKeyState = new();
    private uint _heldMouseButtons;
    private uint _heldJoystickButtons;
    private readonly double[] _joystickAxes = new double[JoystickAxisCount];
    private int _joystickPov = JoystickPovCentered;
    private int _actionCount = DefaultActionCount;
    // Op 0x2f9's and op 0x2fb's own polls (input_read_joystick_axes@0x474920, input_read_joystick_buttons@0x4745e0):
    // each keeps its last poll's time, from 0 as native's module globals do, and the axes reader its last positions.
    private long _axesPolledAtMs;
    private readonly int[] _polledAxes = new int[JoystickAxisCount];
    private long _buttonsPolledAtMs;

    public InputBindings() : this(InputRules.AxesOnly)
    {
    }

    public InputBindings(InputRules rules)
    {
        ArgumentNullException.ThrowIfNull(rules);
        Rules = rules;
        // input_manager_initialize_defaults@0x460630: seven default actions before SYSTEM4 extends
        // the table to ten. Several physical keys may map to the same logical action.
        MapKeyboardScanCode(0, 0xc8); // Up
        MapKeyboardScanCode(1, 0xcd); // Right
        MapKeyboardScanCode(2, 0xd0); // Down
        MapKeyboardScanCode(3, 0xcb); // Left
        MapKeyboardScanCode(4, 0x1c); // Enter
        MapKeyboardScanCode(5, 0x39); // Space
        MapKeyboardScanCode(6, 0x0e); // Backspace
    }

    public InputRules Rules { get; }

    /// <summary>Op 0x2fa's value (AGE 4.45D: the input manager's <c>+0x1a74</c>). With bit 0 set the axes make no
    /// directions; the POV still does.</summary>
    public int JoystickAxesFlags { get; set; }

    public int ActionCount
    {
        get { return _actionCount; }
    }

    public bool SetActionCount(int count)
    {
        if ((uint)count >= MaximumActions) return false;
        _actionCount = count;
        return true;
    }

    public bool MapKeyboardScanCode(int action, int dikScanCode)
    {
        if ((uint)action >= MaximumActions) return false;
        int virtualKey = DikToVirtualKey(dikScanCode);
        if (virtualKey == 0) return true; // Native writes VK[0]; GetAsyncKeyState(0) never contributes.
        _keyboardActions[virtualKey] = action;
        return true;
    }

    public void MapJoystickButton(int buttonSlot, int physicalButton)
    {
        if ((uint)buttonSlot >= MaximumActions) return;
        _joystickButtons[buttonSlot] = physicalButton;
    }

    /// <summary>Op 0x10b. Native's table holds 32 physical buttons and polling reads 0 and 1, so a mapping of another
    /// button changes nothing.</summary>
    public void MapMouseButton(int buttonSlot, int physicalButton)
    {
        if ((uint)buttonSlot >= MaximumActions || (uint)physicalButton >= _mouseButtonSlots.Length) return;
        _mouseButtonSlots[physicalButton] = buttonSlot;
    }

    public int KeyboardAction(int virtualKey)
    {
        if ((uint)virtualKey >= _keyboardActions.Length) return -1;
        return _keyboardActions[virtualKey];
    }

    public int MouseAction(int physicalButton)
    {
        if ((uint)physicalButton >= _mouseButtonSlots.Length) return -1;
        return (_mouseButtonSlots[physicalButton] + 4) & 31;
    }

    public int JoystickButtonActionMask(int physicalButton)
    {
        if ((uint)physicalButton >= 32) return 0;
        return JoystickButtonActions(physicalButton);
    }

    public void UpdateKeyboardVirtualKey(int virtualKey, bool pressed)
    {
        if ((uint)virtualKey >= _keyboardActions.Length) return;
        if (pressed)
        {
            _heldVirtualKeys.Add(virtualKey);
            _pressedSinceKeyState.Add(virtualKey);
        }
        else _heldVirtualKeys.Remove(virtualKey);
    }

    public void UpdateMouseButton(int physicalButton, bool pressed)
    {
        if ((uint)physicalButton >= 32) return;
        UpdateBit(ref _heldMouseButtons, physicalButton, pressed);
    }

    public void UpdateJoystickButton(int physicalButton, bool pressed)
    {
        if ((uint)physicalButton >= 32) return;
        UpdateBit(ref _heldJoystickButtons, physicalButton, pressed);
    }

    public void UpdateJoystickAxes(double x, double y)
    {
        UpdateJoystickAxis(0, x);
        UpdateJoystickAxis(1, y);
    }

    /// <summary>Axis <paramref name="axis"/> (0 to 5) at <paramref name="value"/>, from -1 to 1.</summary>
    public void UpdateJoystickAxis(int axis, double value)
    {
        if ((uint)axis < JoystickAxisCount) _joystickAxes[axis] = Math.Clamp(value, -1.0, 1.0);
    }

    /// <summary>The POV as <c>joyGetPosEx</c> reports it: hundredths of a degree clockwise from up (0 to 35999), or
    /// <see cref="JoystickPovCentered"/>.</summary>
    public void UpdateJoystickPov(int hundredthsOfDegree)
        => _joystickPov = hundredthsOfDegree is >= 0 and < 36000 ? hundredthsOfDegree : JoystickPovCentered;

    /// <summary>
    /// Op 0x2f9: the six axes' positions over WinMM's range (0 at one end, 1 at the other, about 0.5 at rest). Like
    /// <c>input_read_joystick_axes@0x474920</c> it polls again only once more than
    /// <see cref="JoystickPollIntervalMs"/> have passed since its last poll, and otherwise gives that poll's positions.
    /// </summary>
    public void ReadJoystickAxes(long nowMs, Span<float> values)
    {
        if (unchecked((uint)(nowMs - _axesPolledAtMs)) > JoystickPollIntervalMs)
        {
            _axesPolledAtMs = nowMs;
            for (int axis = 0; axis < JoystickAxisCount; axis++)
                _polledAxes[axis] = (int)Math.Round((_joystickAxes[axis] + 1.0) / 2.0 * JoystickAxisRange);
        }
        for (int axis = 0; axis < JoystickAxisCount && axis < values.Length; axis++)
            values[axis] = (float)_polledAxes[axis] / JoystickAxisRange;
    }

    /// <summary>
    /// Op 0x2fb: the held physical buttons, bit N for button N (<c>JOYINFOEX.dwButtons</c>). Like
    /// <c>input_read_joystick_buttons@0x4745e0</c> it gives them only from a fresh poll, so 0 when it last polled
    /// within <see cref="JoystickPollIntervalMs"/>.
    /// </summary>
    public uint ReadJoystickButtons(long nowMs)
    {
        if (unchecked((uint)(nowMs - _buttonsPolledAtMs)) <= JoystickPollIntervalMs) return 0;
        _buttonsPolledAtMs = nowMs;
        return _heldJoystickButtons;
    }

    /// <summary>The entries of native's DIK-to-VK table, which op 0x10e indexes.</summary>
    public const int ScanCodeCount = 256;

    /// <summary>
    /// Op 0x10e: GetAsyncKeyState for the virtual key of DirectInput scan code <paramref name="dikScanCode"/>, as
    /// the signed 16-bit value AGE widens: bit 15 while the key is down, and bit 0 when it was pressed since the last
    /// call for that key, which the call clears (so -32767, -32768, 1 or 0). Windows keeps that bit for every
    /// process and any of them can take it; the port keeps it for the game's own calls. A scan code with no virtual
    /// key asks for key 0, which is never down.
    /// </summary>
    public int KeyState(int dikScanCode)
    {
        int virtualKey = DikToVirtualKey(dikScanCode);
        if (virtualKey == 0) return 0;
        int state = _heldVirtualKeys.Contains(virtualKey) ? short.MinValue : 0;
        return _pressedSinceKeyState.Remove(virtualKey) ? state | 1 : state;
    }

    public int PollActionMask()
    {
        int mask = 0;
        foreach (int virtualKey in _heldVirtualKeys)
        {
            int action = _keyboardActions[virtualKey];
            if ((uint)action < MaximumActions) mask |= 1 << action;
        }

        // Native polls only VK_LBUTTON/VK_RBUTTON. Their zero-initialized slots both produce
        // action 4 until scripts remap one of them.
        for (int button = 0; button < _mouseButtonSlots.Length; button++)
            if ((_heldMouseButtons & (1u << button)) != 0)
                mask |= 1 << ((_mouseButtonSlots[button] + 4) & 31);

        mask |= JoystickDirections();

        for (int physicalButton = 0; physicalButton < 32; physicalButton++)
            if ((_heldJoystickButtons & (1u << physicalButton)) != 0)
                mask |= JoystickButtonActions(physicalButton);
        return mask;
    }

    // input_poll_joystick_action_bits: up 0, right 1, down 2, left 3. Under AGE 4.45D's rule a pressed POV makes them
    // (0x474450: up past 27000 or below 9000, right 1 to 17999, down 9001 to 26999, left past 18000) and the axes are
    // not read; otherwise the axes do, unless op 0x2fa's bit 0 is set.
    private int JoystickDirections()
    {
        if (Rules.JoystickPovDirections && _joystickPov != JoystickPovCentered)
        {
            uint pov = (uint)_joystickPov;
            int directions = 0;
            if (pov > 27000 || pov < 9000) directions |= 1 << 0;
            if (unchecked(pov - 1) < 17999) directions |= 1 << 1;
            if (unchecked(pov - 9001) < 17999) directions |= 1 << 2;
            if (pov > 18000) directions |= 1 << 3;
            return directions;
        }
        if ((JoystickAxesFlags & 1) != 0) return 0;
        int mask = 0;
        double x = _joystickAxes[0], y = _joystickAxes[1];
        if (y < -JoystickAxisThreshold) mask |= 1 << 0;
        else if (y > JoystickAxisThreshold) mask |= 1 << 2;
        if (x > JoystickAxisThreshold) mask |= 1 << 1;
        else if (x < -JoystickAxisThreshold) mask |= 1 << 3;
        return mask;
    }

    private int JoystickButtonActions(int physicalButton)
    {
        int mask = 0;
        // Native loops input_action_count slots and uses x86's masked shift for slot+4.
        for (int slot = 0; slot < _actionCount; slot++)
            if (_joystickButtons[slot] == physicalButton) mask |= 1 << ((slot + 4) & 31);
        return mask;
    }

    private static void UpdateBit(ref uint field, int bit, bool set)
    {
        uint mask = 1u << bit;
        field = set ? field | mask : field & ~mask;
    }

    /// <summary>The DIK-to-VK entries populated by input_initialize_dik_to_vk_table@0x45fc60.</summary>
    public static int DikToVirtualKey(int dik) => dik switch
    {
        0x01 => 0x1b, 0x02 => 0x31, 0x03 => 0x32, 0x04 => 0x33, 0x05 => 0x34,
        0x06 => 0x35, 0x07 => 0x36, 0x08 => 0x37, 0x09 => 0x38, 0x0a => 0x39,
        0x0b => 0x30, 0x0c => 0x6d, 0x0e => 0x08, 0x0f => 0x09,
        0x10 => 0x51, 0x11 => 0x57, 0x12 => 0x45, 0x13 => 0x52, 0x14 => 0x54,
        0x15 => 0x59, 0x16 => 0x55, 0x17 => 0x49, 0x18 => 0x4f, 0x19 => 0x50,
        0x1c => 0x0d, 0x1d => 0x11, 0x1e => 0x41, 0x1f => 0x53, 0x20 => 0x44,
        0x21 => 0x46, 0x22 => 0x47, 0x23 => 0x48, 0x24 => 0x4a, 0x25 => 0x4b,
        0x26 => 0x4c, 0x2a => 0x10, 0x2c => 0x5a, 0x2d => 0x58, 0x2e => 0x43,
        0x2f => 0x56, 0x30 => 0x42, 0x31 => 0x4e, 0x32 => 0x4d, 0x36 => 0x10,
        0x38 => 0x12, 0x39 => 0x20,
        0x3b => 0x70, 0x3c => 0x71, 0x3d => 0x72, 0x3e => 0x73, 0x3f => 0x74,
        0x40 => 0x75, 0x41 => 0x76, 0x42 => 0x77, 0x43 => 0x78, 0x44 => 0x79,
        0x45 => 0x90, 0x46 => 0x91, 0x47 => 0x67, 0x48 => 0x68, 0x49 => 0x69,
        0x4a => 0x6d, 0x4b => 0x64, 0x4c => 0x65, 0x4d => 0x66, 0x4e => 0x6b,
        0x4f => 0x61, 0x50 => 0x62, 0x51 => 0x63, 0x52 => 0x60, 0x53 => 0x6e,
        0x57 => 0x7a, 0x58 => 0x7b, 0x70 => 0x15, 0x79 => 0x1c, 0x7b => 0x1d,
        0x94 => 0x19, 0x9c => 0x0d, 0x9d => 0x11, 0xb5 => 0x6f, 0xb8 => 0x12,
        0xc7 => 0x12, 0xc8 => 0x26, 0xc9 => 0x21, 0xcb => 0x25,
        0xcd => 0x27, 0xcf => 0x23, 0xd0 => 0x28, 0xd1 => 0x22, 0xd2 => 0x2d,
        0xdb => 0x5b, 0xdc => 0x5c, 0xdd => 0x5d,
        _ => 0,
    };
}
