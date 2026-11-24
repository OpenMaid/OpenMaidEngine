# Translating a game

This walks through one translation from an empty folder to a package players install. You extract the game's text,
translate a page, check and play it, then find the text you missed, fit long lines, and do the pictures, fonts and
menus. Last come PO files, tests, game updates and sharing the package. The steps are the same for every game; your
game's guide gives what its installs need (the [games page](games/README.md) links each one).

This page shows the order of the work. Every key and rule it uses is in [language packages](languages.md), and each
step links to the section it uses.

## 0. What you need

As for [your first mod](first-mod.md), step 0: a release unpacked anywhere, the game installed, a profile for it made
in the launcher, and a terminal in the release's folder. The commands below are the mod tool's, `OME-Tool` (type
`.\OME-Tool` in PowerShell on Windows, `./OME-Tool` on Linux), and name the game through the profile,
`--profile <profile>`.

A language package changes no script, so it needs no fingerprint (except for site entries, step 6). Use the same
profile in every step: the catalogs match the text of the install it names, its overlays included.

## 1. Extract the text

```text
OME-Tool omp text extract --language en --output my.lang --profile <profile>
```

`--language` takes the language's tag (`en`, `pt-br`, …), which names the catalog folder and goes in the manifest
([language packages](languages.md), "Layout"). This creates `my.lang/text/en/`: one catalog per script, holding its
messages and the patterns for strings it builds, and `_global.toml`, holding every literal the scripts draw or store.
The summary line counts the files and templates it wrote. Each entry is a template, with a `source` and no `text`:

```toml
# Catalog template written by omp text extract from the <game> install at …
script = "SC0000"

[[text]]
source = "――かつて、戦いがあった。"
```

A template is skipped, so the game shows its own text until you add a translation. Entries in `_global.toml` carry a
`note` naming where the scripts store or draw that string (`ITINIT@0x…`).

## 2. Translate a page

Start the game (`OME-Tool start --profile <profile>`) and stop on a page. Press **F2** to show the page locator and
**F3** to copy it, for example `SC0000 P001 · wait SC0000@0x83c · text SC0000@0x834`. The script name is the catalog
file: `text/en/SC0000.toml`. Find the line by its Japanese and add `text`:

```toml
[[text]]
source = "――かつて、戦いがあった。"
text = "――Once, there was a war."
```

When the screen alone does not tell you which line is which, `OME-Tool locate SC0000 1 --profile <profile>` prints the
page's text and instructions.

- Each sentence is one entry, even when the script draws it in pieces (a word with furigana). Its `source` is the
  pieces joined, without ruby or the script's line breaks ([language packages](languages.md), "Messages").
- The port wraps the translation at word breaks. Write `\n` only for a break that belongs to the text, such as a
  header line.
- The catalogs are TOML: write a double quote inside `text` as `\"` and a backslash as `\\`.
- Leave the templates you do not translate yet as they are. Delete those you never will, if you like.

## 3. Write the manifest

`my.lang/manifest.toml`:

```toml
omp = 2
id = "my.<game>-en"
name = "English text"
version = "0.1.0"

[game]
profile = "<game>"
target = "native"

[[language]]
tag = "en"
name = "English"
glyph_delay_scale = 0.5
```

`omp = 2` is the format version that carries languages. `glyph_delay_scale = 0.5` keeps the text reveal at the
original's pace for a language that needs about twice the characters. Every key is in
[language packages](languages.md), "Manifest".

## 4. Check it

```text
OME-Tool omp text check --mod my.lang --profile <profile>
```

A good result ends with `omp text check: passes: 0 errors, 0 warnings`. The note before it counts the entries that
have text. A file that does not read, an ambiguous pattern or a placeholder the source lacks is an error with its
file and line; a source that the installed scripts never draw is a warning ([language packages](languages.md),
"Tools").

```text
OME-Tool omp text stats --mod my.lang --profile <profile>
```

`stats` counts how much is translated, for example `text en: messages 3/27389 (0.0%), strings 0/5311 (0.0%), …`.
Add `--scripts` for a line per script.

## 5. Play it

```text
OME-Tool start --profile <profile> --mod my.lang -- --language en
```

Start a new game: the lines you translated show in English, and the rest in Japanese. The menus and dialogs are in
English too, since the engine has its own English (step 9). Restart the game to see an edit.

To keep the package installed, add it on the profile's Mods page in the launcher ([commands and installing
mods](cli.md), "Installing mods"), and choose the language on the profile's Settings page or in the game's settings
panel, OME section. The switch applies at once. Saves do not depend on the language: a save made in one loads in any
other ([language packages](languages.md), "Saves").

## 6. Find what you missed

Three tools find text that is still Japanese.

**The miss log.** Start with `--text-log`. The port writes every string it drew without an entry to
`diagnostics/text-misses.jsonl` in the profile's folder, one JSON object per line:

```json
{"source":"激しい戦いの末、ついに魔王は打ち倒される。","script":"SC0000","at":"0x908","count":1}
```

`source` is the exact text the entry needs, `script` and `at` the script and instruction that drew it, and `count`
how often. Most misses already have a template: search the catalogs for the `source`. A sentence drawn in pieces is
logged once, joined, at its first piece. The log is rewritten every two seconds while it changes, and at exit.

**The locator.** F2 and F3 name the script of any page you are looking at, as in step 2.

**The pseudo language.** Start with `-- --language x-pseudo`. Every string that passes through translation is drawn in
brackets and a third longer, with no catalog:

- text without brackets does not pass through translation, usually because it is part of a picture (step 8);
- text that runs out of its window or box is where a longer translation will not fit (step 7).

**One source, two translations.** A short line such as `はい` may need a different translation in one place. Give
that place a *site entry*: the same `source` with `at`, the offset of the message's first `show-text`. The miss log's
`at` is that offset. So is the locator's `text` coordinate for a line the script draws in one piece. A file with site
entries names the script's fingerprint at its top:

```text
OME-Tool oms-patch --fingerprint SC0000.BIN --profile <profile>
```

```toml
script = "SC0000"
fingerprint = "sha256:…"             # the hash the command printed

[[text]]
source = "はい"
at = 0x2a43
text = "Right."
```

The site entry wins over the script and global entries for the same text. `extract` keeps both the fingerprint and
the site entries. A site entry binds only to that version of the script ([language packages](languages.md),
"Scope"). If a line still shows in Japanese, see [troubleshooting](troubleshooting.md), "Translations".

## 7. Make long text fit

A translation is usually longer than the Japanese. The port wraps it at word breaks. A message that needs more rows
than its window has is narrowed just enough to fit, down to the language's `min_fit_scale`, and cut at the bottom of
the window when even that is not enough. With `--text-log`, the miss log lists each narrowed message with
`width_percent` and each cut one with `rows_cut`. Shorten those, or remove `\n` breaks they do not need
([language packages](languages.md), "Text too long for its window").

Text in a fixed place, such as a name plate, an item name in a box or a centred title, can be fitted to the room the
Japanese took:

```toml
[[text]]
source = "鉄槌"
text = "Iron Hammer"
fit = "shrink"
align = "center"
```

`fit = "shrink"` narrows the translation to the original's width, and `align` places it where the original sat
([language packages](languages.md), "Alignment and fit").

## 8. Translate pictures

Text drawn into a picture, such as a title logo, a menu button or a map label, is not text the port can translate.
Give the picture a language version: draw the translated picture and save it as `my.lang/assets/en/NAME.png`, where
`NAME` is the game's file name without `.AGF`. It replaces the original while the language is selected.

To find the file, go to the screen in the port and press **F5**. The port lists the pictures on screen, back to front,
under the page locator and copies the line, for example `… · pictures: SO013A.AGF, EV049AA.AGF, SO001.AGF`. It names
the file behind each picture shown, also one the game drew text on or copied into a picture of its own, and marks a
movie `(movie)`. The names are the game's, also for a picture your package already replaces.

Then write the original out as a PNG to draw on, decoded as the port decodes it (`--all` writes every picture):

```text
OME-Tool picture SO001.AGF --output-dir my-pictures --profile <profile>
```

Many pictures are sheets: one file holds every button of a menu, or several menus, and the game draws each piece
from its place in the sheet. Keep the sheet's size, and draw each translated piece where the original was.

```text
OME-Tool omp explain NAME.AGF --mod my.lang --profile <profile>
```

`explain` shows which version of the asset wins, for example
`en: language asset from my.<game>-en (PNG), while en or a language that falls back to it is selected`
([language packages](languages.md), "Images and other assets").

In a driven test (step 11), the `pictures` step returns the same list.

## 9. Fonts and the engine's menus

**Fonts.** The port draws translated text with the system's fonts. To ship a font, put the file in `my.lang/fonts/`
and map the game's faces to it with a `[[language.font]]` table in the manifest. Choose a fixed-width font: the
scripts count half cells, and Windows' rules squeeze a proportional one. `omp text check` lists the characters your
translation uses that no bundled font has, and warns about a proportional font ([language packages](languages.md),
"Fonts").

**Menus and dialogs.** The engine's own text (the menu bar, the settings panel, the quit dialog, the name entry) has
keys, not sources. The engine has it in Japanese and English. For any other language, `extract` also writes
`text/<tag>/_engine.toml` with a template per key, the Japanese and English in its `note`:

```toml
[[engine]]
key = "command.skip"
note = "ja: ﾒｯｾｰｼﾞｽｷｯﾌﾟ(&S)\nen: &Skip"
text = "Ü&berspringen"
```

`&` marks the access key. A language with `fallback = ["en"]` shows the engine's English for every key it leaves
out. A package of menus alone can serve every game ([language packages](languages.md), "Engine text").

## 10. Work in a translation tool

Poedit, Weblate and Lokalize read gettext PO. Export the catalogs as one PO file, translate there, and import the
result:

```text
OME-Tool omp text po-export --language en --package my.lang --output my-en.po
OME-Tool omp text po-import --language en --package my.lang --input my-en.po
```

Each entry is one message, with its scope (`SC0000`, or `SC0000@0x2a43` for a site) as the PO context. Import
changes only each entry's `text`, rewrites only the files that changed, and skips empty and fuzzy messages. The
catalogs stay the source: after an `extract`, export again ([language packages](languages.md), "Working in PO").

## 11. Test it with the driver

**From source:** `tools/drive.py` plays the port from a script of steps, so a test can walk to a page and take a
screenshot in each language:

```text
py -3.11 -X utf8 tools/drive.py --game <game> port start --mod my.lang --port-arg=--language --port-arg=en
py -3.11 -X utf8 tools/drive.py --game <game> port run my-lang.steps
py -3.11 -X utf8 tools/drive.py --game <game> port stop
```

The `language TAG` step switches the language as the settings panel does, so one scenario can compare a page in
each language. Text already on screen keeps its language until the game draws it again, so advance after a switch:

```text
language ja
advance
shot page-ja
language en
advance
shot page-en
```

A path in `--port-arg` must be absolute, since the port runs from `godot/`. Each start uses an empty save sandbox
([commands and installing mods](cli.md), "Automated testing"). The driver's
[port scenarios](../../tools/drive/scenarios/port/) hold a working example.

## 12. After a game update

Run `extract` again with the same profile. It keeps every entry, translated or not, adds templates for new text, and
marks `obsolete = true` each entry whose source is gone. It rewrites only the files that changed, and it does not
keep comments: write notes for other translators in `note`. A script whose fingerprint changed drops its site
entries, and `omp text check` warns about each one; take the new fingerprint and offsets as in step 6.

## 13. Share it

```text
OME-Tool omp build my.lang --profile <profile>
```

`omp build` validates the package and writes `my.<game>-en-0.1.0.omp` beside the folder, named after its `id`
and `version`. Players add that file on their profile's Mods page and choose the language.

- A language package runs in the port only. Native AGE draws the text its scripts hold, and `omp export` leaves the
  text out.
- Saves mark a package of only text, fonts and language assets as presentation only, so players can add, update or
  remove it without breaking a save ([language packages](languages.md), "Saves").
- A package built from someone else's translation ([language packages](languages.md), "From a translation made in
  place") remains their work: share it only with their permission.

## Where next

- Every manifest key, catalog key and rule: [language packages](languages.md).
- Every option of the `omp text` commands and the engine: [commands and installing mods](cli.md).
- A translation that already replaced the game's scripts: [language packages](languages.md), "From a translation
  made in place".
- A page too long to fit any other way: patch the script to break it ([your first mod](first-mod.md),
  [patching](patching.md)).
