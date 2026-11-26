using Ome.Engine.Model;

namespace Ome.Engine.Vm;

// AGE 4.43B's model ops and its projection (docs/engine-re.md, "The models"). Each handler reads its operands as native
// does and calls the worker once; the model records live in the graphics native's model map (GfxState.Models.cs).
public sealed partial class VirtualMachine
{
    // 0x330 (handle)(mesh slot): the model draws op 0x329's mesh (op_0x330_set_model_mesh@0x425e50)
    private StepResult OpSetModelMesh(Instruction ins, int pc)
    {
        var a = ins.Args;
        Gfx.SetModelMesh(Read(a[0]), unchecked((int)Read(a[1])));
        return StepResult.Next;
    }

    // 0x332 (handle)(blend mode)(alpha)(rgb): the model's blend mode and colour (op_0x332_set_model_color@0x425ee0)
    private StepResult OpSetModelColor(Instruction ins, int pc)
    {
        var a = ins.Args;
        Gfx.SetModelColor(Read(a[0]), Read(a[1]), unchecked((int)Read(a[2])), unchecked((int)Read(a[3])));
        return StepResult.Next;
    }

    // 0x334 (handle): the model's matrices back to the identity (op_0x334_reset_model_transform@0x426080)
    private StepResult OpResetModelTransform(Instruction ins, int pc)
    {
        Gfx.ResetModelTransform(Read(ins.Args[0]));
        return StepResult.Next;
    }

    // 0x335 (handle)(x)(y)(z): scaling in percent, each float read divided by 100 before it is rounded to a float
    // (op_0x335_set_model_scale@0x4260c0)
    private StepResult OpSetModelScale(Instruction ins, int pc)
    {
        var a = ins.Args;
        Gfx.SetModelScale(Read(a[0]), (float)(ReadFloatExact(a[1]) / 100.0), (float)(ReadFloatExact(a[2]) / 100.0),
                          (float)(ReadFloatExact(a[3]) / 100.0));
        return StepResult.Next;
    }

    // 0x337 (handle)(x)(y)(z): a translation (op_0x337_set_model_translation@0x4261e0)
    private StepResult OpSetModelTranslation(Instruction ins, int pc)
    {
        var a = ins.Args;
        Gfx.SetModelTranslation(Read(a[0]), ReadFloat(a[1]), ReadFloat(a[2]), ReadFloat(a[3]));
        return StepResult.Next;
    }

    // 0x33b (handle)(a)(b)(c): three floats at the model's +0x8 (op_0x33b_set_model_vector_8@0x426430)
    private StepResult OpSetModelVector8(Instruction ins, int pc)
    {
        var a = ins.Args;
        Gfx.SetModelVector8(Read(a[0]), ReadFloat(a[1]), ReadFloat(a[2]), ReadFloat(a[3]));
        return StepResult.Next;
    }

    // 0x33e (handle)(axis x)(axis y)(axis z)(degrees): one more rotation about an axis
    // (op_0x33e_rotate_model_axis@0x426560)
    private StepResult OpRotateModelAxis(Instruction ins, int pc)
    {
        var a = ins.Args;
        Gfx.RotateModelAxis(Read(a[0]), ReadFloat(a[1]), ReadFloat(a[2]), ReadFloat(a[3]), ReadFloat(a[4]));
        return StepResult.Next;
    }

    // 0x33d (field of view in degrees)(near)(far): the projection (op_0x33d_set_projection@0x426510)
    private StepResult OpSetProjection(Instruction ins, int pc)
    {
        var a = ins.Args;
        _graphics.SetProjection(ReadFloat(a[0]), ReadFloat(a[1]), ReadFloat(a[2]));
        return StepResult.Next;
    }

    // 0x244: restart the one-shot clock of every object whose colour fade is armed
    // (op_0x244_clear_fade_state@0x419910)
    private StepResult OpRestartArmedColorFades(Instruction ins, int pc)
    {
        Gfx.RestartArmedColorFades();
        return StepResult.Next;
    }
}
