# Your first mod

This walks through one complete mod: insert one line of narration into the opening scene, check that it links, run
it in the port, and export it for the original engine. The steps are the same for every game. The values that
differ per game (the page, the site, the fingerprint) are in your game's guide, under "First mod values" (the
[games page](games/README.md) links each one). Each game's guide also links the finished mod.

## 0. What you need

- **A release** of Open Maid Engine, `OME-windows-x64.zip` or `OME-linux-x64.tar.gz`, unpacked anywhere. It unpacks
  into one folder, `OME-windows-x64` or `OME-linux-x64`: the *release's folder*, which holds the engine (`OME`), the
  launcher (`OME-Launcher`) and the mod tool (`OME-Tool`).
- **The game**, installed anywhere.
- **A profile for it**, made in the launcher: start `OME-Launcher` and make a new profile on the game's install.
  Compatibility mode is enough for this mod. The profile's **id** is the name of its folder, which its Overview page
  shows. The commands below write it as `<profile>`.
- **A terminal in the release's folder.** Every command below runs there. The guides write the tool as `OME-Tool`;
  type `.\OME-Tool` in PowerShell on Windows (`./OME-Tool.exe` in Git Bash), `./OME-Tool` on Linux.
  `OME-Tool --help` lists its commands.

Every command names the game through the profile, `--profile <profile>`. The profile gives the game, the install,
its overlays (such as a translation's loose files) and its mods, so each step works on the same scripts. Your game
needs nothing to be generated first: the game's OMS toolchain (`<game>.globals`, `<game>.adv` and its patch points)
is part of the release ([commands and installing mods](cli.md), "Where the toolchain comes from"). A game whose
guide says it has no registry yet has no `<game>.globals` module: leave the `use <game>.globals` line out of the
fragment below.

## 1. Find the place to edit

Start the game on the profile from the terminal, and play to the page you want to change:

```text
OME-Tool start --profile <profile>
```

A new profile may start the game fullscreen; its Settings page in the launcher can switch it to a window. Press **F2**
to show the page locator and **F3** to copy it. A locator reads like `SC0000 P003 · wait SC0000@0x8a6 · text
SC0000@0x89e`. Close the game (its window's close button, or Ctrl+C in the terminal), then resolve the locator's page to
bytecode:

```text
OME-Tool locate SC0000 3 --profile <profile>
```

The output gives the page's start (`page start: SC0000@0x874`), its last `show-text`, its wait, its text, and the
disassembly around the wait. Offsets are dword offsets into the script; those are the *sites* a fragment names. The
page map it reads is the one the run wrote, in the profile's `diagnostics/page-maps/`, so it knows only the pages
that run reached.

To insert a new page before this one, attach it to the instruction that starts the page's drawing: the
`reset-adv-text-layout` right after the page's start, which is the previous page's last `end-text-line`. List the
instructions there with the page start's offset:

```text
OME-Tool locate SC0000@0x874 --context 4 --profile <profile>
```

```text
    0x00871: wait-for-input 0x0
>>> 0x00874: end-text-line 0x0           ; the page start: the previous page ends here
    0x00877: reset-adv-text-layout 0x1   ; insert before this
```

The [patching guide](patching.md) covers choosing sites in more detail.

## 2. Get the base fingerprint

A fragment names the exact bytes it edits. Print the header line for the script:

```text
OME-Tool oms-patch --fingerprint SC0000.BIN --profile <profile>
```

It prints a line such as `patch "SC0000.BIN" fingerprint "sha256:…"` to paste into the fragment. Use the full file
name, including `.BIN`.

## 3. Write the mod

Create a folder anywhere; its name is up to you. Give the mod a lowercase dotted id, ideally one that starts with a
name you own:

```text
my.first-line/
├── manifest.toml
└── patches/
    └── opening.oms
```

`manifest.toml`:

```toml
omp = 1
id = "my.first-line"
name = "My first line"
version = "1.0.0"
description = "Adds a line of narration to the opening."

[game]
profile = "<game>"
target = "native"
```

`[game] profile` is the game's id, such as `himegari`, not your player profile's.

`patches/opening.oms` (your game guide has the complete file with its fingerprint and site, which can differ from
the pattern above: Arterial's pages start at another instruction):

```text
oms 1
patch "SC0000.BIN" fingerprint "sha256:<the fingerprint from step 2>"
use <game>.globals
use <game>.adv

before site <the site from step 1> {
    "[MOD] Hello from my first mod."
}
```

The quoted line is a *dialogue statement* from the game's ADV module, `<game>.adv`. The engine carries it in the game's
base mod, so a release has no file of it to open; the [games page](games/README.md) links each game's source in the
repository. It expands to the same instructions as the game's own narration pages: show the text, wait for a click,
reset the text layout. Add a speaker and voice the same way the decompiled scenes do ([the language
guide](oms-language.md), "Dialogue lines").

## 4. Check that it links

Pass the mod's folder with `--mod`, as a path from the release's folder (`path/to/my.first-line` below):

```text
OME-Tool omp check --mod path/to/my.first-line --profile <profile>
```

A good result ends with `omp check: links: 1 mods; 0 errors, …`. The note "compiled … at load time (no stored
plan)" is expected while you develop. `omp build` removes it (step 7).

A wrong fingerprint, a site that is not an instruction start, or a syntax error stops here with the file, line and
column ([troubleshooting](troubleshooting.md)).

## 5. Run it in the port

```text
OME-Tool start --profile <profile> --mod path/to/my.first-line
```

`start` runs the launcher's checks with your mod among the profile's, then starts the game and prints its output in
the terminal. Start a new game. Your line appears as its own page at the place you chose. The output's `[mods]` lines
give the load order and every file the mod supplied. The profile's `diagnostics/engine.log` keeps the same output,
and its `diagnostics/mod-link.txt` the link report's lines ([commands and installing mods](cli.md)). Ctrl+C in the
terminal closes the game, as its window's close button does.

`--mod` loads the mod for that run only. To keep it installed, add its folder (or its `.omp`) on the profile's Mods
page in the launcher and switch it on ([commands and installing mods](cli.md), "Installing mods").

## 6. Run it in native AGE

Native AGE cannot read mods, so export the linked result as game files:

```text
OME-Tool omp export --output-dir path/to/my-export --mod path/to/my.first-line --profile <profile>
```

For this mod that writes one file, the patched `SC0000.BIN`, in `my-export`. Native AGE reads loose `.BIN` files beside
`AGE.EXE` before its archives, so the file overrides the original. Put it into a *copy* of the game, never the original:
copy the install's folder, put `my-export/SC0000.BIN` into the copy's folder beside `AGE.EXE`, and run the copy's
`AGE.EXE`.

## 7. Package it

```text
OME-Tool omp build path/to/my.first-line --profile <profile>
```

`omp build` takes the mod's folder itself, not `--mod` (which names the packages it depends on). It compiles every
fragment against the base and writes `my.first-line-1.0.0.omp` beside the folder. The package is a ZIP file with fixed
timestamps, so the same sources always produce the same bytes. Inside it, each fragment's compiled transform plan sits
beside its source (`patches/opening.omspatch.json`). Your folder is not changed. Players load the stored plan instead of
compiling your source on their machine. The source stays in the package so anyone can read and rebuild it.

## Where next

- More operations (`after`, `replace`, operand edits) and how mods combine: [patching](patching.md).
- A scene of your own, or a data edit at boot: [new scenes](new-scenes.md).
- Every manifest field: [packages](packages.md).
