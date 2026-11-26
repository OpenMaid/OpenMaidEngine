using Ome.Engine.Model;

namespace Ome.Engine.Vm;

/// <summary>
/// AGE's screen mode, as far as the interpreter sees it (docs/engine-re.md, "Screen mode"). The shell-services native
/// owns the mode and decides every switch (Ome.Engine/Natives/ShellServices.ScreenMode.cs); the front end shows it. A
/// switch queues CALLBACK_WINDOW.BIN at the native, which the VM runs as it runs CALLBACK_SETTING.BIN: at the next
/// instruction boundary outside a statement block, or from a text wait's input service.
/// </summary>
public sealed partial class VirtualMachine
{
    /// <summary>Op 0x2cf (op_0x2cf_handler@0x425370): operand 0 asks for fullscreen and 1 for a window; any other
    /// value does nothing. The front end switches at its next frame.</summary>
    private StepResult OpRequestScreenMode(Instruction ins, int pc)
    {
        long mode = Read(ins.Args[0]);
        if (mode is 0 or 1) _shell.RequestScreenMode(windowed: mode == 1);
        return StepResult.Next;
    }

    /// <summary>Op 0x2ce (op_0x2ce_get_windowed@0x42ece0 in 4.43B; 4.33G's and 4.38D's the same): 1 while the game runs
    /// in a window, display_windowed (4.43B ctx+0xa3c14, which engine_initialize_graphics_from_settings sets from
    /// display:ScreenMode and a switch changes). 魔導巧殻's CONFIG1 compares it with the player's choice before op
    /// 0x2cf.</summary>
    private StepResult OpGetWindowed(Instruction ins, int pc)
    {
        Write(ins.Args[0], _shell.Windowed ? 1 : 0);
        return StepResult.Next;
    }
}
