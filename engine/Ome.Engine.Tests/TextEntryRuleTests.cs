using System.Collections.Immutable;
using Ome.Engine.Frontend.Shell;
using Ome.Engine.Localization;
using Ome.Engine.Profiles;
using Ome.Engine.Sys4;
using Xunit;

/// <summary>
/// AGERC's dialog 10, which op 0x144 opens, as each game's AGERC.DLL has it (docs/engine-re.md, "AGERC's dialog 10 in
/// each game"): the profile's rule and words, and the place its words take among the shell's text.
/// </summary>
public class TextEntryRuleTests
{
    private static GameTextEntry Entry(string game) => GameProfileRegistry.BuiltIn.Find(game)!.TextEntry;

    [Fact]
    public void HimegarisNameEntryIsSixteenBytesOfDoubleByteCharacters()
    {
        GameTextEntry name = Entry("himegari");
        Assert.Same(GameTextEntry.NameEntry, name);
        Assert.Equal(new TextEntryCheck(FullwidthTextValidationError.None, "ひめがり"), name.Check("ひめがり"));
        Assert.Equal(FullwidthTextValidationError.TooLong, name.Check("あいうえおかきくけ").Error);
        Assert.Equal(FullwidthTextValidationError.NonDoubleByteCharacter, name.Check("ab").Error);
    }

    [Fact]
    public void ArterialsDeckNameIsTwentyFourBytesOfAnyCharacters()
    {
        // agerc_text_entry_dialog_proc@+0x1760: GetDlgItemTextA, then 字数オーバーです above 24 bytes.
        GameTextEntry deck = Entry("arterial");
        Assert.Equal(TextEntryLimit.Bytes, deck.Limit);
        Assert.Equal(new TextEntryCheck(FullwidthTextValidationError.None, "Deck 1 あいう"), deck.Check("Deck 1 あいう"));
        Assert.Equal(FullwidthTextValidationError.None, deck.Check(new string('あ', 12)).Error);
        Assert.Equal(FullwidthTextValidationError.TooLong, deck.Check(new string('あ', 12) + "a").Error);
    }

    [Fact]
    public void KoukakusCommentKeepsThePrefixThatFitsItsWidthWithoutAnError()
    {
        // agerc_text_entry_dialog_proc@+0x17c0: 15 for a double-byte character, 8 for any other byte, at most 564.
        GameTextEntry comment = Entry("koukaku");
        Assert.Equal(new TextEntryCheck(FullwidthTextValidationError.None, new string('あ', 37)),
                     comment.Check(new string('あ', 40)));                                  // 37 × 15 = 555
        Assert.Equal(new string('a', 70), comment.Check(new string('a', 80)).Text);          // 70 × 8 = 560
        Assert.Equal("ab", comment.Check("ab").Text);
        Assert.Equal(new string('あ', 36) + "ab", comment.Check(new string('あ', 36) + "ab").Text);  // 556
        Assert.Equal(new string('あ', 37) + "a", comment.Check(new string('あ', 37) + "ab").Text);   // 563, then 571
        Assert.Equal("ｱｲｳ", Cp932Text.WidthPrefix("ｱｲｳｴ", 15, 8, 24));                    // half-width kana are one byte
    }

    [Fact]
    public void EveryProfilesWordsAreShellKeysInJapaneseAndEnglish()
    {
        foreach (GameProfileManifest profile in GameProfileRegistry.BuiltIn.Profiles)
        {
            foreach (var (language, words) in profile.TextEntry.Text)
            {
                Assert.Contains(language, ShellStrings.Languages);
                foreach (var (key, text) in words)
                {
                    Assert.Contains(key, ShellBoxes.NameEntryKeys);
                    Assert.Null(ShellStrings.EntryProblem(key, text));
                }
            }
            if (profile.TextEntry.Text.Count > 0)
                Assert.Equal(["en", "ja"], profile.TextEntry.Text.Keys.Order());
        }
    }

    [Fact]
    public void AGamesWordsComeAboveTheShellsTableAndBelowAPackagesEntry()
    {
        var words = ImmutableDictionary.CreateRange(new Dictionary<string, ImmutableDictionary<string, string>>
        {
            ["ja"] = ImmutableDictionary.CreateRange(new Dictionary<string, string> { [ShellBoxes.NameEntryTitle] = "コメント編集" }),
            ["en"] = ImmutableDictionary.CreateRange(new Dictionary<string, string> { [ShellBoxes.NameEntryTitle] = "Edit the comment" }),
        });
        Assert.Equal("コメント編集", ShellStrings.Japanese.WithGameText(words)[ShellBoxes.NameEntryTitle]);
        Assert.Equal("決定", ShellStrings.Japanese.WithGameText(words)[ShellBoxes.NameEntryAccept]);
        Assert.Equal("Edit the comment", ShellStrings.Load("en").WithGameText(words)[ShellBoxes.NameEntryTitle]);

        var package = new EngineTextLayer("en", new Dictionary<string, EngineTextValue>
        {
            [ShellBoxes.NameEntryTitle] = new("Comment", "en/a.toml:1"),
        });
        ShellStrings layered = ShellStrings.Load("en", ["en"], [package]).WithGameText(words);
        Assert.Equal("Comment", layered[ShellBoxes.NameEntryTitle]);
        Assert.Equal("Enter the name in full-width characters.", layered[ShellBoxes.NameEntryPrompt]);
    }
}
