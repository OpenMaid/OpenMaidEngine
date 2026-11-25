using Ome.Engine.Model;
using Ome.Engine.Natives;

namespace Ome.Engine.Vm;

// The AGERC.DLL code scripts call (docs/engine-re.md, "Comparing engine builds", Verita): Himegari's handlers
// (op_0x14b_load_agerc_module@0x422080, op_0x14c_set_agerc_export@0x422160, op_0x14d_call_agerc_export@0x429ee0) and
// Verita's (0x422c00, 0x422ce0, 0x42aa70) are the same code. The module and the export slots are the shell native's,
// the map the exports keep its AgercMap (ShellServices.Agerc.cs).
public sealed partial class VirtualMachine
{
    // ERROR_PROC_NOT_FOUND: GetProcAddress's error for a name a loaded module does not export.
    private const string ProcNotFound = "127";

    // 0x14b (catalog entry): load the DLL the entry names as the module op 0x14c looks names up in. Only the entry's
    // name is read, which LoadLibraryA finds beside AGE.EXE; the catalog's bytes are not.
    private StepResult OpLoadAgercModule(Instruction ins, int pc)
    {
        long entry = Read(ins.Args[0]);
        string? name = _provider?.EntryName(entry);
        if (name == null)
        {
            HaltReason ??= $"agerc-module-unnamed: {At(ins)}: the port cannot name catalog entry {entry}, whose name AGE "
                           + "loads with LoadLibraryA";
            return StepResult.Halt;
        }
        if (!_shell.LoadAgercModule(name))
        {
            HaltReason ??= $"agerc-module-unsupported: {At(ins)}: entry {entry} names {name}, which AGE loads with "
                           + $"LoadLibraryA; the port's only module is {ShellServices.AgercModuleName}";
            return StepResult.Halt;
        }
        return StepResult.Next;
    }

    // 0x14c (slot, name): bind a slot to an export of the module. A name the module does not export throws with flags
    // 0x10007, 「name のアドレス取得に失敗しました．」 and GetProcAddress's error code.
    private StepResult OpSetAgercExport(Instruction ins, int pc)
    {
        var a = ins.Args;
        string name = ReadStr(a[1]);
        uint slot = unchecked((uint)Read(a[0]));
        switch (_shell.BindAgercExport(slot, name))
        {
            case AgercBindStatus.Bound:
                return StepResult.Next;
            case AgercBindStatus.NoSuchExport:
                return StepResult.Jump(RaiseInstructionScriptError(
                    AgeBoxMessage.Keyed(AgeBoxText.AgercExportMissing, name, ProcNotFound), pc));
            case AgercBindStatus.NoModule:
                HaltReason ??= $"agerc-module-not-loaded: {At(ins)}: no module is loaded for {name} (op 0x14b has not run "
                               + "since the scene reset), where AGE looks the name up in its own program";
                return StepResult.Halt;
            default:
                HaltReason ??= $"index-out-of-range: {At(ins)}: export slot {slot} is outside "
                               + $"0..{ShellServices.AgercExportSlots - 1}, where AGE throws an error whose text it formats "
                               + "from an operand past the instruction";
                return StepResult.Halt;
        }
    }

    // 0x14d (slot, result, cells, count, ref, value): call a slot's export with count integer cells from cells' address,
    // which it may change, and store what it returns. Native copies the cells into a heap block (none for a count of 0
    // or less), calls the export as fn(owner window, block, count, ref's cell address, value), and writes the block back
    // when the count is above 0. No map function reads the window, the count, ref or value.
    private StepResult OpCallAgercExport(Instruction ins, int pc)
    {
        var a = ins.Args;
        int count = unchecked((int)Read(a[3]));
        if (count > MaxScriptRunLength)
        {
            HaltReason ??= $"agerc-call-count-too-large: {At(ins)}: {count} cells";
            return StepResult.Halt;
        }
        VmAddress cells = BaseAddr(a[2]);
        var values = new int[Math.Max(0, count)];
        for (int i = 0; i < values.Length; i++) values[i] = unchecked((int)ReadIntCell(cells.Offset(i)));
        long slot = Read(a[0]);
        if (IndexRefused(ins, "export slot", slot, ShellServices.AgercExportSlots, IndexTest.Unchecked) is { } refused)
            return refused;
        string? export = _shell.AgercExport((int)slot);
        if (export == null)
        {
            HaltReason ??= $"agerc-export-unbound: {At(ins)}: export slot {slot} is bound to nothing, where AGE calls "
                           + "address 0";
            return StepResult.Halt;
        }

        int[] before = (int[])values.Clone();
        AgercCallResult call = _shell.AgercMap.Call(export, values);
        string? refusal = call.Status switch
        {
            AgercCallStatus.Done => null,
            AgercCallStatus.CellsTooShort => $"agerc-call-past-cells: {At(ins)}: {call.Detail}, where AGE's export reads "
                                             + "past its copy of the cells",
            AgercCallStatus.OutsideTable => $"agerc-call-outside-table: {At(ins)}: {call.Detail}, where AGE reads or "
                                            + "writes the statics beside the table",
            AgercCallStatus.DivideFault => $"division-by-zero: {At(ins)}: {call.Detail}, where AGE's division faults "
                                           + "and the process ends",
            _ => $"agerc-shell-export: {At(ins)}: {call.Detail}; the port does not call it for a script",
        };
        if (refusal != null)
        {
            HaltReason ??= refusal;
            return StepResult.Halt;
        }
        // Native writes every cell back. Those the export left hold what they held, so the port writes only the ones it
        // changed: a cell's writer and its watches see the export's writes alone.
        for (int i = 0; i < values.Length; i++)
            if (values[i] != before[i]) WriteIntCell(cells.Offset(i), values[i]);
        Write(a[1], call.Value);
        return StepResult.Next;
    }
}
