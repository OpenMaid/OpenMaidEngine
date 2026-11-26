using System.Collections.Immutable;
using Ome.Engine.Sys4;

namespace Ome.Engine.Profiles;

/// <summary>How a game's dialog 10 accepts what was typed (<see cref="GameTextEntry"/>).</summary>
public enum TextEntryLimit
{
    /// <summary>At most <see cref="GameTextEntry.MaxBytes"/> CP932 bytes, every character double-byte; two errors
    /// (Himegari's name entry, AGERC.DLL <c>agerc_fullwidth_text_dialog_proc@0x10002140</c>).</summary>
    FullwidthBytes,

    /// <summary>At most <see cref="GameTextEntry.MaxBytes"/> CP932 bytes of any characters; one error.</summary>
    Bytes,

    /// <summary>The longest prefix whose width fits <see cref="GameTextEntry.MaxWidth"/>, kept without an error.</summary>
    Width,
}

/// <summary>What the accept made of the text: an error to show with the dialog kept open, or the text to return.</summary>
public readonly record struct TextEntryCheck(FullwidthTextValidationError Error, string Text);

/// <summary>
/// The profile's <c>textEntry</c> entry: AGERC command 10's dialog, which op 0x144 opens. Each game's AGERC.DLL has its
/// own dialog resource 10 and its own procedure (docs/engine-re.md, "AGERC's dialog 10 in each game"), so the rule and
/// the dialog's words are the game's. <see cref="Text"/> holds the words by language, keyed by the shell's
/// <c>name-entry.*</c> keys; the shell puts them above its own table for that language and below a package's entries.
/// A profile without the entry has Himegari's.
/// </summary>
public sealed record GameTextEntry
{
    public GameTextEntry(TextEntryLimit limit, int maxBytes = 0, int doubleByteWidth = 0, int singleByteWidth = 0,
                         int maxWidth = 0, IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>? text = null)
    {
        if (limit is TextEntryLimit.FullwidthBytes or TextEntryLimit.Bytes && maxBytes < 1)
            throw new ArgumentException($"textEntry: limit {limit} needs maxBytes of 1 or more", nameof(maxBytes));
        if (limit == TextEntryLimit.Width && (doubleByteWidth < 1 || singleByteWidth < 1 || maxWidth < 1))
            throw new ArgumentException("textEntry: limit Width needs doubleByteWidth, singleByteWidth and maxWidth of 1 or more",
                                        nameof(maxWidth));
        Limit = limit;
        MaxBytes = maxBytes;
        DoubleByteWidth = doubleByteWidth;
        SingleByteWidth = singleByteWidth;
        MaxWidth = maxWidth;
        Text = (text ?? new Dictionary<string, IReadOnlyDictionary<string, string>>())
            .ToImmutableDictionary(pair => pair.Key, pair => pair.Value.ToImmutableDictionary(StringComparer.Ordinal),
                                   StringComparer.Ordinal);
    }

    /// <summary>Himegari's: 16 bytes of double-byte characters, in the shell's own words.</summary>
    public static GameTextEntry NameEntry { get; } = new(TextEntryLimit.FullwidthBytes, maxBytes: Cp932Text.NativeNameByteLimit);

    public TextEntryLimit Limit { get; }
    public int MaxBytes { get; }
    public int DoubleByteWidth { get; }
    public int SingleByteWidth { get; }
    public int MaxWidth { get; }
    public ImmutableDictionary<string, ImmutableDictionary<string, string>> Text { get; }

    /// <summary>What the dialog's accept does with <paramref name="text"/>.</summary>
    public TextEntryCheck Check(string text) => Limit switch
    {
        TextEntryLimit.FullwidthBytes => new(Cp932Text.ValidateFullwidthName(text, MaxBytes), text),
        TextEntryLimit.Bytes => new(Cp932Text.ValidateByteLength(text, MaxBytes), text),
        _ => new(FullwidthTextValidationError.None, Cp932Text.WidthPrefix(text, DoubleByteWidth, SingleByteWidth, MaxWidth)),
    };
}
