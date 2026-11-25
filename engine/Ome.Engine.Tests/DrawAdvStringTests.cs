using System;
using System.Collections.Generic;
using Ome.Engine.Model;
using Ome.Engine.Sys4;
using Ome.Engine.Vm;
using Xunit;

/// <summary>
/// Op 0x89 (docs/engine-re.md, "Op 0x89, a string drawn as the ADV text is"): draw-string's draw with AntiFont on, a
/// start at the effect offsets with it off, and nothing drawn where its rasterizers find no surface.
/// </summary>
public sealed class DrawAdvStringTests
{
    private const int Surface = 0x83;
    private static readonly OpcodeTable Table = OpcodeTableJson.Load(Paths.OpcodesJson);

    private static Operand I(long value) => new(0, value);

    private static int Op(string label) => Table.ByLabel(label)!.Value;

    private static (int, Operand[]) Create(int slot) => (Op("create-texture"), [I(slot), I(256), I(32), I(0)]);

    private static (int, Operand[]) Draw(int slot, int x, int y, string text, string[] strings)
        => (Op("draw-adv-string"), [I(slot), I(x), I(y), new Operand(2, Array.IndexOf(strings, text))]);

    private static (VirtualMachine Vm, RecordingWaits Host) Run(bool antiFont, string[] strings,
                                                                 params (int, Operand[])[] code)
    {
        var body = new List<(int, Operand[])>(code) { (0x2, []) };
        Script script = ScriptAssembler.Assemble(Table, "ADVSTR", body, strings);
        var host = new RecordingWaits();
        var vm = new VirtualMachine(script, Table, host);
        vm.Text.SetTextAntialias(antiFont);
        vm.Run();
        return (vm, host);
    }

    [Fact]
    public void WithAntiFontOnItDrawsAsDrawStringDoes()
    {
        string[] strings = ["#######"];
        var (vm, host) = Run(true, strings,
            (Op("set-text-render-mode"), [I(3)]), (Op("set-text-effect-offset"), [I(2), I(3)]),
            Create(Surface), Draw(Surface, 0x1ab, 40, "#######", strings));

        Assert.Equal("exit", vm.HaltReason);
        Assert.Equal((Surface, 0x1ab, 40, "#######"), Assert.Single(host.SurfaceStrings));
    }

    [Fact]
    public void WithAntiFontOffAStringOfAnEffectModeStartsAtTheEffectOffsets()
    {
        string[] strings = ["価格"];
        var (_, shifted) = Run(false, strings,
            (Op("set-text-render-mode"), [I(3)]), (Op("set-text-effect-offset"), [I(2), I(3)]),
            Create(Surface), Draw(Surface, 10, 20, "価格", strings));
        Assert.Equal((Surface, 12, 23, "価格"), Assert.Single(shifted.SurfaceStrings));

        var (_, plain) = Run(false, strings,
            (Op("set-text-render-mode"), [I(0)]), (Op("set-text-effect-offset"), [I(2), I(3)]),
            Create(Surface), Draw(Surface, 10, 20, "価格", strings));
        Assert.Equal((Surface, 10, 20, "価格"), Assert.Single(plain.SurfaceStrings));
    }

    [Fact]
    public void AnEmptySlotOrAnEmptyStringDrawsNothing()
    {
        string[] strings = ["", "x"];
        var (vm, host) = Run(true, strings,
            Draw(Surface, 0, 0, "x", strings),
            Create(Surface), Draw(Surface, 0, 0, "", strings));

        Assert.Equal("exit", vm.HaltReason);
        Assert.Empty(host.SurfaceStrings);
        Assert.Empty(host.Warnings);
    }
}
