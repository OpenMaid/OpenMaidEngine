# Modding Ikusa Megami ZERO

戦女神ZERO-specific facts for the generic guides. The identity block (game id, engine ABI, modules, tables, patch
points) is generated on [the games page](README.md#ikusa-megami-zero).

Script mods and text packages work for this game (modding level M1). Its data tables are not decoded yet, so its base
mod, `bg0-base`, carries only the game's OMS toolchain: it names no cell, declares no event and accepts no data file.
The generated page says which level the game is at.

## Install and options

The install this guide was written against mounts one append catalog, `APPEND.AAI` (selector 1), and its scripts
are 1,251 base and 328 append records. A profile made on it in the launcher needs nothing else, so the guides'
commands take `--profile <profile>` alone. Without a profile, they take the game's options:

| Install | Options |
|---|---|
| 戦女神ZERO | `--game bg0 --game-root DIR` |

- **From source:** the conventional install is `games/bg0/` beside the repository, so `--game-root ../games/bg0`
  from `age-reimpl/`. The port is `.\run-godot.cmd -Game bg0`, with `-Mod PATH` for a mod. The driver is
  `py -3.11 -X utf8 tools/drive.py --game bg0 port start --mod PATH`. Its points for the game include `title.start`
  (Game Start on the title screen), `mode.normal` (通常 in the game-mode box that Game Start opens, which goes to the
  opening scene), `adv.next`, and the character menu's `menu.save` and `menu.load` (the driver's
  [戦女神ZERO points](../../../tools/drive/bg0.toml)). The title's entries are a column on the right half of the
  canvas, from x 400, and the middle of the canvas, `adv.next`, is on the edge of Option's row: do not click it on
  the title.
- Game Start asks for a game mode, 通常 or 特別編, before a new game starts.
- Native AGE shows Japanese dialogs, so `dialogs` and `answer` use Japanese labels. On a machine with the LAV Filters
  codec pack, native AGE waits for ever in the game's movies; the driver keeps LAV's DirectShow classes from it (the
  `refuse_class` entries of the driver's points file).
- The menu bar, the fullscreen strip, the 終了 dialog and 設定 are the port's own.
- The game has no globals registry yet, so `use bg0.globals` names nothing: name a cell by its address
  (`G[0x2fd0]`), as the decompiled scripts print it. [Your first mod](../first-mod.md)'s step for `globals_build.py`
  does not apply either.

## First mod values

The values for [your first mod](../first-mod.md), on the opening scene:

| Step | Value |
|---|---|
| Page | The opening scene's first page (セリカ: `「よし、これでこっちの動きは封じた！」`). |
| Site | `0xdf3` |
| Fingerprint | `sha256:50d75547dd7ffcee34c3671133e67d39cd6ca031f03dd0cfc1258bddcac0929f` |

A page of this game starts at the instruction that resets the text layout, before the one that sets `G[0x2fd1]` (the
page has text). That instruction is the site to insert a page before:

```text
oms 1
patch "SC0000.BIN" fingerprint "sha256:50d75547dd7ffcee34c3671133e67d39cd6ca031f03dd0cfc1258bddcac0929f"
use bg0.adv

before site 0xdf3 {
    "[MOD] Hello from my first mod."
}
```

The finished mod is [example.bg0-prologue-line](../../../oms/examples/mods/example.bg0-prologue-line/). Its line is the
first page on both targets (native AGE on the exported `SC0000.BIN`, and the port with `--mod`), at `SC0000@0xe25`;
the game's own first page follows it, at `SC0000@0xefc`. The two targets' second pages are alike to the pixel, and
their first differ only in the turning wait icon's frame (the driver's
[prologue line](../../../tools/drive/scenarios/bg0-prologue-line.steps) scenario, 2026-11-25).

## Dialogue lines

The prelude's template is `line(text, speaker = none, voice = none)`:

```text
"[MOD] A narration line."
セリカ "「よし、これでこっちの動きは封じた！」" (voice: 0x2f)
カヤ "「やるわねセリカ！　さすがあたしの弟！」" (voice: 0x31)
```

- A **speaker** is a name of the prelude's `speaker` enum, the index into the name table that CNINIT writes, whose
  name the name plate shows. The enum names the 135 speakers the scenes set; a name that is not an identifier
  (`ルナ＝クリア`, `？？？？？`) or that two speakers share carries its id (`speaker_0xa0`), with the name in a comment.
  A line without a speaker is narration.
- `voice` is the voice clip, a resource id. The template mutes it as the game does, for a character whose voice the
  player turned off on the game's Option screen (CONFIG, which keeps the switches in `G[0x2be7]`). セリカ's lines all name `0x2f`, `SERIKA_A999.OGG`, a short clip of 4,558 bytes; the
  other characters' name their own numbered clips (`0x31` is `KAYA0001.OGG`). Copy a voice from a line of the
  decompiled scene (`OME-Tool oms-decompile --level idiomatic --script SC0000.BIN --profile <profile>`).

The scene library is an earlier form of Verita's: a page starts its voice and draws its name plate itself, which
Verita's scenes do through library functions. Pages that carry furigana, interpolate a name or span several
`show_text` calls stay core statements in a decompiled scene, as in the other games.

## Text and language packages

`omp text extract` writes a catalog for every script that draws text (152 files and about 33,000 entries for this
install). The game's text has no private-use characters, so a `source` can be typed as the page shows it. The names on
the name plates are strings CNINIT stores: an entry of `_global.toml` translates one wherever it is drawn
(`source = "セリカ"`, `text = "Serika"`).

A language package with three translated lines of the opening scene and the name セリカ was checked and run in the port
(2026-11-25, `--mod …` and the options panel's language `en`): the scene's first three pages read in English, and
セリカ's name plate too. Nothing larger has been tried on this game.

## Data edits and patch points

None yet. The game's tables are not decoded (modding level M2) and its base mod has no layout or events (M3), so
there are no table names, no events and no named patch points. A fragment can still patch any instruction by its
offset.

## Catalogs, scenes and saves

- **Selectors.** The install ships one append catalog (`APPEND.AAI`, selector 1), so a first mod's catalog normally
  gets selector 2.
- **Untested.** No 戦女神ZERO mod that adds a scene or a script has been tested, on either target. Start from one of
  the game's own append scenes, and test saving on native AGE as well as in the port.

## Examples

| Path | What it shows |
|---|---|
| [example.bg0-prologue-line](../../../oms/examples/mods/example.bg0-prologue-line/) | a line inserted before the opening scene's first page |
