# Engine extension through mods: notes

**Date:** 2026-11-25
**Status:** notes from a discussion with the user (2026-11-24 and 2026-11-25). This is not a design: nothing here is
accepted, and nothing is built. The user set the topic aside for game compatibility work and expects to come back to
it. A design spec would start from these notes and settle the questions each part lists.
**Checked against:** the working tree of `feature/verita` at `1ff22810`. Each claim about code names the file it was
read in. Two claims were not run, and say so where they stand (§4, §6). Paths are relative to `age-reimpl/`.
**The question:** `docs/modding/README.md` says "Mods cannot yet extend the engine". What would that look like as
part of the mod system, instead of source changes on a branch?

## 1. The short answer

Mods never add opcodes or native code. Extending the engine through the mod system comes to three parts, and a
fourth that is already built:

1. **Functions a script can call** (§4). A mod declares a function whose body is Lua. The engine adds one opcode
   that calls it.
2. **Engine services** (§5). First-party C#, as today, but each one lands through an extension registry that
   generates its Open Maid Script and Lua forms.
3. **Port cells** (§6). Global cells that the port supplies above the game's own banks, allocated by the linker.
   This part is the user's idea (2026-11-25).
4. **Mods extending mods** (§7), which the data kinds, the declared events and the read-only exports already give.

## 2. What bounds it

These decisions already stand, and the three parts stay inside them.

- **A package carries no code but Lua source.**
  - The user ruled out opcode sets as downloaded DLLs: every build ships in the one binary (engine layering spec §2,
    decision 5, 2026-11-11).
  - Lua has no .NET access, and packages are untrusted (Lua scripting spec §15; mod loader spec §11).
- **A release cannot load a C# plugin.** The engine is compiled with NativeAOT (`docs/platform-portability.md`, "Fewer
  files in a package"), so a release has no JIT and nothing that runs IL.
  - `Assembly.LoadFrom` fails in a release, and would work in the engine tests, which run under the JIT.
  - Reflection over the engine's own code still works, but only over what the trimmer kept.
  - This is the weaker reason. A loaded assembly has the process's full rights, and .NET has no sandbox for it, so
    the first bullet would rule plugins out under the JIT too.
  - A C# plugin would take one of three things: the JIT runtime shipped again (the data folder back to about 200
    files); native plugins over a flat C interface, built per operating system, with no objects crossing; or
    compiling the plugin into the engine, which is a source change.
- **The engine knows no game** (Lua scripting spec §3, decision 1).
- **A Lua handler runs to completion** (the same, decision 4). Anything the player can save in the middle of is
  bytecode.
- **`extended` content runs in the port only.** `omp export` refuses it, and a profile in Compatibility mode
  refuses it (launcher spec §6).

## 3. Where `extended` stands

The language design gives `extended` three things (Open Maid Script spec §3.3): extension opcodes (§9.4), virtual
script identities, and engine-owned mod state (§9.2). None is built. A manifest's `target = "extended"` only marks a
package the port alone runs (`docs/backlog.md`, "The `extended` compile mode").

Lua has since taken most of the purpose: mod state is `maid.state`, and logic beyond the AGE ABI is a handler. What
is left:

- **Bytecode that calls Lua or the engine.** §4 and §5.
- **Mod state that Open Maid Script can reach.** §6.
- **Virtual script identities.** They would only lift the cap on selectors, 1 to `0x7f` (mod loader spec §6.2). They
  can wait until a stack meets the cap.

## 4. Part 1: functions a script can call

**The gap.** Lua can run a script (`maid.vm.call`). A script cannot run Lua: Lua runs only where it has attached
itself, at a hook, a watch or an event.

**The nearest thing today** is a watch (read from `docs/modding/lua.md`; not run). The script writes a request cell,
a handler watching that cell runs after the instruction, and it leaves its answer in another cell. It costs:

- the `vm` capability, since the watch names a cell by address;
- free global cells for the request, the arguments and the result (§6);
- no compiler check.

A script hook on the mod's own script is no route: the link refuses a script hook on a script the game does not have.

**The sketch.** Open Maid Script already calls an opcode like a function and names it an intrinsic (its spec §6.3):
`get_cursor_virtual(out x, out y)`, or `x = f(args)` when the opcode writes one operand. A mod function is an
intrinsic whose body is Lua.

```toml
[[functions]]
name    = "roll_drop"              # scripts call example.loot:roll_drop
params  = ["int", "int"]
returns = "int"
handler = "lua/loot.lua:roll_drop"
```

```
use example.loot
item = roll_drop(enemy_id, luck)
```

1. **Declare.** The manifest lists the function, as it lists hooks. The link checks it before any mod code runs.
2. **Compile.** A call becomes one instruction: the engine's call-function opcode, at `0x10000` or above. Its
   operands are a reference to the function by name, the arguments, and the cell that takes the result. The
   arguments are checked against the declared types, as an opcode's operands are (spec §8.1).
3. **Link.** The reference resolves as `run "mod.id:NAME"` does, through a symbolic operand like the plan format's
   `Resource` (mod loader spec §6.2). A missing mod is a link error.
4. **Run.** The handler reads the operands, the scripting native calls the Lua function with plain values, and the
   handler writes the return value. Budgets, errors and the trace work as for a hook.

**Its rules.**

- A function that only computes needs no capability. One that writes the game's names lists them, as a Lua mod does
  (`[lua] writes`).
- It cannot wait, so its opcode's `waits` fact is `never`.
- Mods get one opcode between them, so there are no numbers to allocate and one dispatch path. The Open Maid Script
  spec's "engine-owned opcodes" (§9.4) still holds: the opcode is the engine's, and the function is the mod's.

**What it buys.** A mod's own scripts get tables, strings and loops on demand. A second mod can call the first
one's functions, so mods get an API between them at the script level.

**To settle.**

- **The operand count.** The decoder frames an instruction by its opcode's `argc`. Either each function fixes its
  count at the link, or the instruction carries a count operand.
- **Types.** Strings and floats as parameters and results, and more than one result through `out`.
- **Restores.** A load runs the saved scripts' prologues again (Lua scripting spec §11), so a function can be called
  during a restore, as a hook is.
- **Dispatch.** The handlers are bound into an array indexed by opcode (engine layering spec §4), so opcodes at
  `0x10000` need a second table or a branch.
- **Decompiling** an `extended` script, and how a dependent names another mod's function (`[[requires]]` and `use`).

## 5. Part 2: engine services

Two gaps hide under "engine services".

**a. Lua cannot reach what opcodes reach.** A script plays a sound with `sfx-load` and `sfx-start`. Their handlers
(`engine/Ome.Engine.Vm/VirtualMachine.Audio.cs`, `OpSfxLoad` and `OpSfxStart`) are a few lines that end in
`Audio.LoadSoundEffect` and `Audio.StartSoundEffect` (`engine/Ome.Engine/Natives/Audio.SoundEffects.cs`). Lua has no
route to those natives. A `maid.audio` would be the same call with Lua arguments in place of operands. The engine
code exists, so this is binding work, and Open Maid Script needs nothing: it has the opcodes as intrinsics.

**b. Things AGE never had.** Examples: a settings page for each mod in the options panel, state kept per profile
instead of per save, an overlay layer, gamepad input. Each needs a new native in C#, because it owns state, may
write a save record and resets with the scene (engine layering spec §5.1). A mod cannot supply one (§2).

**How a service lands today.** By hand. `maid.text_fits` is in `engine/Ome.Engine/Modding/ScriptAccess.cs`,
`engine/Ome.Scripting.Lua/ModEnvironment.cs` and `engine/Ome.Mods/Tools/LuaStubs.cs`, and in the guide, and it has no
Open Maid Script form.

**The sketch.** One extension registry in `vm-map/opcodes.toml`'s schema, as the Open Maid Script spec already says
(§9.4). One row generates the intrinsic in the `abi.ome` module, the Lua binding with its editor stub, and the
reference page. The row below is a sketch, not a format:

```toml
[[extension]]
name       = "mod-setting-get"   # OMS: mod_setting_get(out value, key); Lua: maid.settings.get(key)
operands   = [{ access = "write" }, { access = "read-string" }]
waits      = "never"
capability = "settings"
```

- **The rule it sets:** a source change adds a general service, and what one mod does with it stays in its package.
  The layering spec's test already says where the code goes: "Could a Lua mod or the options panel want the same
  action? Then it is a native" (§3.1).
- **A service that takes time** offers a start call and a wait the engine owns, as native's fade and effect opcodes
  do. The wait is a member of `IInputAndWaits`, so the test that lists the waits covers it. The script does the
  waiting. This avoids Lua that can be suspended, which a save cannot hold (Lua scripting spec §16, "Lua that
  waits").

**An example: mod settings.** The engine gets, once, a settings native that holds each mod's values, stores them in
the profile and gives the options panel a page. A mod declares its settings as data in its manifest and reads them
from either language.

**To settle.**

- **Which service comes first.** A real mod's need should pick it.
- **How the Lua bindings are generated.** A release cannot bind by reflection (§2), so a tool or a source generator
  writes the binding code.
- **One capability for each service,** shown in the link report as `vm` is.
- **Which parts of (a) are worth a binding,** when `maid.vm.call` of a helper script already reaches them.

## 6. Part 3: port cells

**The problem they answer.** A game's banks are fixed arrays with no allocator, and a save keeps a fixed prefix of
each (`docs/sys4-format-notes.md`, "SYS4INI file header: VM bank dimensions"). Himegari saves 402,459 of its
1,419,442 integer cells and 789 of its 18,180 string cells. Each game has 2 float cells, 1 of them saved.

- **No ownership map.** The scripts hold baked addresses, and many accesses are computed, so a cell no script names
  can still be touched.
- **A value that must survive a save** needs a free cell inside the prefix, which is the region the game fills.
- **No allocator.** A `global` pins its address by hand, so two mods can pick one cell.

The plan to prove free ranges is in the Open Maid Script spec's §13 and the backlog's "Mod-owned globals". It serves
`native` mods and stays open.

**The idea.** For `extended` mods the port supplies cells above the game's banks.

- **It needs no new opcode.** In bytecode a cell is a type tag and an address. The port keeps each bank as a sparse
  dictionary, and neither `engine/Ome.Engine/Natives/ScriptMemory.cs` nor `engine/Ome.Engine.Vm/` bounds an address
  by the bank's size. So an address above the bank should already work as a cell in the port. This was read, not
  run.
- **Every opcode can use such a cell:** arithmetic, arrays, string opcodes, pointer cells, an intrinsic's `out`
  operand. On native AGE the address lies outside the allocated bank.
- **It beats accessor calls.** The Open Maid Script spec has `extended` globals "reached through extension
  intrinsics" (§9.2). As plain operands, `kills += 1` is one instruction. Watches, the trace and the console keep
  working, since they hang off script memory's setters.
- **Ownership is solved there.** No shipped script uses the range, so the linker's allocation table is the
  ownership map.

**What has to be built.**

- **A reserved range:** a fixed base far above any game's bank, the same for every game.
- **Linker allocation,** with the syntax the language design already has (its §5.2):
  ```
  saved global int kills        // kept with the save
  global int scratch[64]        // zeroed by a load, as native's unsaved cells are
  ```
  The reference stays symbolic until the link, so mods never collide and no mod writes an address.
- **The save.** Values go beside the save, keyed by mod id and variable name, so an address may change between
  links. A removed mod's values are carried, as its `maid.state` block is.
- **The restore's moment.** A load zeroes each bank in full and then copies the saved prefixes
  (`ScriptMemory.ReadSaveRecords`). The saved port cells must come back there, before `CALLBACK_LOAD.BIN` and the
  saved scripts' prologues run (Lua scripting spec §11, "What a full load runs"), not when the Lua session is built.
- **A return to title.** The banks are scene state (`ScriptMemory`'s `Scene`), so a root reload empties them, and
  the port cells with them.
- **Lua by name.** A layout generated for a mod's declared globals, as the base mods' is for a game's. The
  accessors, `[lua] writes` and watches then work on them, and `maid.state` stays for table-shaped data.
- **A gate** that runs each cell-addressing opcode against the range, since no opcode has been checked there.

**What else the port could give.**

- **Floats and strings in quantity,** where native has 2 float cells.
- **A per-profile lifetime,** for values that outlive a save, such as clear counts and unlocks.
- **Bounds reports.** The linker knows every array's extent in the range, so a development flag can report a write
  past an array's end or into another mod's block.
- **Exports.** A mod could export its names, so a dependent's scripts `use` them.

**What it does not do.** It does not enlarge the game's own tables. The game's scripts reach those through baked
addresses, strides and loop bounds, so more cells do not add a 446th unit (roadmap §3.2).

**To settle.**

- **The base's value,** and whether all five banks get a range.
- **String cells.** A native save writes them in CP932. A file beside the save could hold any text, but the text
  opcodes measure in CP932.
- **Renames,** through aliases as the base mods keep them.
- **A stray index.** A game script whose computed index runs past its table could reach the range. A far base makes
  that unlikely, and the port could report a write into the range by a script no mod owns.
- **Mod-owned record tables,** applied through the data kinds and `mods/ome-records/`.

## 7. Already built: mods extending mods

A mod can declare a data kind that other mods ship files for (`docs/modding/data-files.md`), declare events that
other mods subscribe to, and export read-only helpers (`docs/modding/lua.md`). The games' own systems are bytecode,
so a new system is a framework mod and not engine work. That mod owns a kind, raises events and, with §4, exports
functions to scripts; content mods ship TOML.

## 8. What stays engine source

Renderer features, shaders, codecs and input devices. A mod that ships a shader would tie packages to one front
end, which the device layer exists to prevent (engine layering spec §3.1). They belong with the port-only options
(engine shell spec §5.6).

## 9. If it is picked up

A suggested order, not a decision:

1. **Port cells** (§6). The engine change is the smallest, and the compiler needs its mode and symbolic references
   for the other parts anyway.
2. **Functions** (§4).
3. **The extension registry** (§5), with the first service a real mod asks for.

A spec that settles these also settles two backlog items: "The `extended` compile mode" (which of the three parts
are still wanted beside Lua) and, for `extended` mods, "Mod-owned globals".

## 10. Read for these notes

- `docs/remake-architecture-and-roadmap.md` §3.1, §3.2, §3.8.
- `docs/superpowers/specs/2026-10-06-open-maid-script-design.md` §3.3, §5.2, §6.3, §8.1, §9.2, §9.4, §13.
- `docs/superpowers/specs/2026-11-03-mod-loader-design.md` §1, §6.2, §11, §12.
- `docs/superpowers/specs/2026-11-10-lua-scripting-design.md` §1, §3, §5.2 to §5.4, §8, §10, §11, §14 to §16.
- `docs/superpowers/specs/2026-11-12-engine-layering-design.md` §2 to §5, §8, §10.
- `docs/superpowers/specs/2026-11-15-launcher-design.md` §6.
- `docs/modding/README.md`, `docs/modding/lua.md`, `docs/modding/data-files.md`, `docs/modding/oms-language.md`.
- `docs/sys4-format-notes.md`, "SYS4INI file header: VM bank dimensions".
- `docs/platform-portability.md`, "Fewer files in a package".
- `docs/backlog.md`: "The `extended` compile mode", "Mod-owned globals", "Accepted future work".
