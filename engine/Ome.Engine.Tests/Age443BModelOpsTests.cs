using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Ome.Engine.Hosting;
using Ome.Engine.Model;
using Ome.Engine.Natives;
using Ome.Engine.Sys4;
using Ome.Engine.Vm;
using Xunit;

/// <summary>
/// AGE 4.43B's models, its projection and op 0x244 (docs/engine-re.md, "The models"): each op as its handler and worker
/// in /koukaku/4.43B leave the record.
/// </summary>
public class Age443BModelOpsTests
{
    private const int Imm = 0, Float = 1;
    private const int Exit = 0x2, EraseObjects = 0x1f7, ClearObjects = 0x1f6;
    private const int SetMesh = 0x330, SetColor = 0x332, ResetTransform = 0x334, SetScale = 0x335,
                      SetTranslation = 0x337, SetVector8 = 0x33b, SetProjection = 0x33d, RotateAxis = 0x33e,
                      RestartFades = 0x244;

    private static Operand I(long value) => new(Imm, value);
    private static Operand F(float value) => new(Float, unchecked((uint)BitConverter.SingleToInt32Bits(value)));

    private static VirtualMachine Run(params (int, Operand[])[] ops)
    {
        OpcodeTable table = OpcodeTableJson.Load(Paths.OpcodesJson, "SYS4443");
        var all = ops.Append((Exit, Array.Empty<Operand>())).ToList();
        var vm = new VirtualMachine(ScriptAssembler.Assemble(table, "MODELS.BIN", all, []), table, new RecordingWaits());
        vm.Run();
        Assert.Equal("exit", vm.HaltReason);
        return vm;
    }

    private static void Near(Matrix4x4 expected, Matrix4x4 actual)
    {
        for (int row = 0; row < 4; row++)
            for (int column = 0; column < 4; column++)
                Assert.Equal(expected[row, column], actual[row, column], 5);
    }

    [Fact]
    public void TheModelOpsFillTheHandlesRecord()
    {
        VirtualMachine vm = Run(
            (SetMesh, [I(7), I(3)]),
            (SetColor, [I(7), I(2), I(0x1ff), I(0x7123456)]),     // alpha capped at 0xff; RGB's high byte dropped
            (SetScale, [I(7), F(200), F(50), I(100)]),
            (SetTranslation, [I(7), F(1.5f), F(-2), F(30)]),
            (SetVector8, [I(7), F(0.25f), F(0.5f), F(0.75f)]),
            (RotateAxis, [I(7), F(0), F(2), F(0), F(30)]),          // the axis is normalized
            (RotateAxis, [I(7), F(0), F(1), F(0), F(60)]));         // multiplied on the right

        GfxState.GfxModel model = Assert.IsType<GfxState.GfxModel>(vm.Gfx.TryGetModel(7));
        Assert.True(model.Made);
        Assert.Equal(3, model.MeshSlot);
        Assert.Equal(2, model.BlendMode);
        Assert.Equal(0xff123456u, model.Argb);
        Assert.True(model.TransformSet);
        Near(Matrix4x4.CreateScale(2f, 0.5f, 1f), model.Scale);
        Near(Matrix4x4.CreateTranslation(1.5f, -2f, 30f), model.Translation);
        Assert.Equal(new Vector3(0.25f, 0.5f, 0.75f), model.Vector8);
        Near(Matrix4x4.CreateRotationY(MathF.PI / 2), model.AxisRotation);
        Assert.Null(vm.Gfx.TryGet(7));                              // a model is no retained object

        vm = Run((SetScale, [I(7), F(200), F(50), I(100)]), (RotateAxis, [I(7), F(1), F(0), F(0), F(45)]),
                 (ResetTransform, [I(7)]));
        model = vm.Gfx.TryGetModel(7)!;
        Assert.False(model.Made);                                   // any model op makes the record
        Assert.False(model.TransformSet);
        Assert.Equal(Matrix4x4.Identity, model.Scale);
        Assert.Equal(Matrix4x4.Identity, model.AxisRotation);
    }

    [Fact]
    public void ANegativeAlphaOrRgbTakesTheObjectsColourAndWhiteWithoutOne()
    {
        var state = new GfxState();
        state.SetObjectColor(4, 0x80abcdef);
        state.SetModelColor(4, 0, -1, 0x123456);
        Assert.Equal(0x80123456u, state.TryGetModel(4)!.Argb);
        state.SetModelColor(4, 0, 0x40, -1);
        Assert.Equal(0x40abcdefu, state.TryGetModel(4)!.Argb);

        state.SetModelColor(9, 0, -1, -1);
        Assert.Equal(0xffffffffu, state.TryGetModel(9)!.Argb);
        Assert.Null(state.TryGet(9));                               // gfx_object_get_color creates no object
    }

    [Fact]
    public void AnEraseTakesTheModelWithTheObjectAndAClearTakesEveryModel()
    {
        VirtualMachine vm = Run((SetMesh, [I(1), I(0)]), (SetMesh, [I(2), I(0)]), (SetMesh, [I(3), I(0)]),
                                (SetMesh, [I(10), I(0)]),
                                (EraseObjects, [I(1), I(0)]),       // one handle
                                (EraseObjects, [I(2), I(2)]));      // the range 2 and 3
        Assert.Equal(1, vm.Gfx.ModelCount);
        Assert.NotNull(vm.Gfx.TryGetModel(10));

        vm = Run((SetMesh, [I(1), I(0)]), (ClearObjects, []));
        Assert.Equal(0, vm.Gfx.ModelCount);
    }

    [Fact]
    public void ANumberedLoadsRestoreLeavesTheModels()
    {
        // save_data_deserialize_and_begin_restore@0x40ffe0 clears the polygons but never the model map.
        var state = new GfxState();
        state.SetModelMesh(5, 1);
        state.RestorePersistenceSnapshot(state.CapturePersistenceSnapshot());
        Assert.NotNull(state.TryGetModel(5));
    }

    [Fact]
    public void Op0x244RestartsTheOneShotClockOfEveryObjectWhoseColourFadeIsArmed()
    {
        var state = new GfxState();
        state.BindDraw(1, 0, 0, 0, 1, 1, 0, 0);
        state.SetAnimatedObjectColorResolved(1, 0, 500, 0, 0);
        state.TryGet(1)!.OneShotStartMs = 400;
        state.SetScaleChannel(2, 0, 500, (200, 200, 100));
        state.TryGet(2)!.OneShotStartMs = 400;                     // a scale alone: bit 1 is not set

        state.RestartArmedColorFades();

        Assert.Equal(-1, state.TryGet(1)!.OneShotStartMs);
        Assert.Equal(400, state.TryGet(2)!.OneShotStartMs);
    }

    [Fact]
    public void Op0x244RunsAsAnOpcode()
    {
        VirtualMachine vm = Run((RestartFades, []));
        Assert.Equal(0, vm.Gfx.ModelCount);
    }

    [Fact]
    public void TheProjectionIsTheConstructorsUntilOp0x33dAndTheEffectsTakeItAtOnce()
    {
        VirtualMachine vm = Run();
        Assert.Equal(new Scene3DProjection(45, 1, 1000), vm.Graphics.Scene3D.Projection);
        // 45° is π/4 exactly: the fixed projection of 4.38D.
        Matrix4x4 fixedProjection = Graphics.Projection(1024, 576, Scene3DProjection.Default);
        Assert.Equal(1f / MathF.Tan(MathF.PI / 8f), fixedProjection.M22);
        Assert.Equal(1000f / 999f, fixedProjection.M33);

        vm = Run((SetProjection, [F(60), F(2), I(2000)]));
        Assert.Equal(new Scene3DProjection(60, 2, 2000), vm.Graphics.Scene3D.Projection);
        Assert.Equal(vm.Graphics.Scene3D.Projection, vm.Graphics.Scene3D.EffectProjection);
        Matrix4x4 projection = Graphics.Projection(1024, 576, vm.Graphics.Scene3D.Projection);
        Assert.Equal(1f / MathF.Tan((float)(60 * 0.01745329238474369) / 2f), projection.M22);
        Assert.Equal(-2f * 2000f / 1998f, projection.M43, 3);
    }

    [Fact]
    public void FourFortyThreeBsCameraHandsTheEffectsTheViewAtOnceAndOlderBuildsAtTheNextDeviceSetup()
    {
        static Graphics NewGraphics(bool rule) => new(null, new FrameClock(), new Sys4LogicalCanvas(1024, 576))
        {
            CameraHandsEffectsTheView = rule,
        };
        Graphics older = NewGraphics(false), newer = NewGraphics(EngineBuilds.Age443B.Rules.Gfx.CameraHandsEffectsTheView);
        foreach (Graphics graphics in new[] { older, newer })
            graphics.SetCameraLookAt((0, 0, -10), (0, 0, 0));

        Assert.Equal(Matrix4x4.Identity, older.Scene3D.EffectView);
        Assert.NotEqual(Matrix4x4.Identity, newer.Scene3D.EffectView);
        older.SetUpDeviceTransforms();
        Assert.Equal(newer.Scene3D.EffectView, older.Scene3D.EffectView);
        Assert.False(EngineBuilds.Age438D.Rules.Gfx.CameraHandsEffectsTheView);
    }
}
