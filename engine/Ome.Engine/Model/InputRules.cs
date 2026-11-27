namespace Ome.Engine.Model;

/// <summary>
/// The input rules an engine build gives <see cref="InputBindings"/> (docs/superpowers/specs/2026-11-12-engine-layering-design.md
/// §7.3). The builds that differ are declared in <c>Ome.Engine.Vm/EngineBuilds.cs</c>.
/// </summary>
/// <param name="JoystickPovDirections">Whether the joystick's POV makes the four direction actions. AGE 4.45D's polls
/// (<c>input_poll_joystick_action_bits@0x474450</c> and <c>input_poll_joystick_action_edges@0x474660</c>) read
/// <c>dwPOV</c> when the caps have <c>JOYCAPS_POV4DIR</c>, and while it is pressed take the directions from it in place of
/// the axes; 4.43B's (<c>0x473ed0</c>, <c>0x473fe0</c>) and older read only the axes (docs/engine-re.md, "魔導巧殻 against
/// 天秤のLa DEA。, triaged").</param>
public sealed record InputRules(bool JoystickPovDirections = false)
{
    /// <summary>The rules of every build before AGE 4.45D.</summary>
    public static InputRules AxesOnly { get; } = new();
}
