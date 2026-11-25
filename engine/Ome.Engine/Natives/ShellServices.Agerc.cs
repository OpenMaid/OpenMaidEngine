namespace Ome.Engine.Natives;

/// <summary>What op 0x14c's binding came to (<see cref="ShellServices.BindAgercExport"/>).</summary>
public enum AgercBindStatus
{
    /// <summary>The slot holds the export.</summary>
    Bound,
    /// <summary>No module is loaded: op 0x14b has not run since the scene reset, and AGE looks the name up in its own
    /// program, which the port does not follow.</summary>
    NoModule,
    /// <summary>AGERC.DLL has no export of that name: AGE throws 「name のアドレス取得に失敗しました」.</summary>
    NoSuchExport,
    /// <summary>The slot is 100 or more, as an unsigned number: AGE throws an error whose text it reads past the
    /// instruction.</summary>
    SlotOutOfRange,
}

/// <summary>
/// The shell native's part of AGERC.DLL that scripts call (docs/engine-re.md, "Comparing engine builds", Verita): op
/// 0x14b loads the module, op 0x14c binds an export of it to one of 100 slots, and op 0x14d calls a slot's export with
/// the script's cells. AGE loads the DLL with LoadLibraryA and looks the names up with GetProcAddress; the port's module
/// is its own AGERC, the map functions in <see cref="AgercMap"/> and the menus and dialogs this native shows. The module
/// and the slots are the interpreter's (scene_context_init_reset clears them); the map is the DLL's and outlives them.
/// </summary>
public sealed partial class ShellServices
{
    /// <summary>The module the port stands for: the AGERC.DLL beside AGE.EXE, which LoadLibraryA finds for that name.
    /// </summary>
    public const string AgercModuleName = "AGERC.DLL";

    /// <summary>The export slots op 0x14c binds.</summary>
    public const int AgercExportSlots = 100;

    /// <summary>AGERC.DLL's map: its statics and the functions that set and query them.</summary>
    public AgercMap AgercMap { get; }

    /// <summary>Op 0x14b: load the DLL a catalog entry names as the module op 0x14c looks names up in. AGE frees the module
    /// it held, which never unloads the DLL its menus hold too, and loads the name again. False for a name other than
    /// AGERC.DLL, which the port has no module for (native loads whatever DLL the name finds, or throws).</summary>
    public bool LoadAgercModule(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        // LoadLibraryA's names are the file system's, which ignores case.
        if (!string.Equals(name, AgercModuleName, StringComparison.OrdinalIgnoreCase)) return false;
        _scene.AgercModuleLoaded = true;
        return true;
    }

    /// <summary>Op 0x14c: bind <paramref name="slot"/> to the export <paramref name="name"/>. AGE looks the name up
    /// before it tests the slot, so a missing name is the error even when the slot is bad too.</summary>
    public AgercBindStatus BindAgercExport(uint slot, string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (!_scene.AgercModuleLoaded) return AgercBindStatus.NoModule;
        if (!AgercMap.HasExport(name)) return AgercBindStatus.NoSuchExport;
        if (slot >= AgercExportSlots) return AgercBindStatus.SlotOutOfRange;
        _scene.AgercExports[slot] = name;
        return AgercBindStatus.Bound;
    }

    /// <summary>The export op 0x14c bound to a slot of 0 to 99, or null when none is (AGE's slot holds 0).</summary>
    public string? AgercExport(int slot) => _scene.AgercExports[slot];
}
