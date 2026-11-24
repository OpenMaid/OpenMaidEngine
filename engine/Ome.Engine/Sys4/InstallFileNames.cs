namespace Ome.Engine.Sys4;

/// <summary>
/// Finds a game's files by name as AGE finds them on Windows, where a name matches whatever its case: a patch or a
/// translation may ship <c>SC2600.bin</c> for the catalog's <c>SC2600.BIN</c> (Kamidori's English <c>patch/</c> folder
/// does). On a file system that matches names exactly (Linux), a name that is not there as written is looked for in
/// its directory's listing, case ignored. The listing is read again at each such lookup: a file's times there come
/// from a clock too coarse to tell whether the directory changed since a kept listing.
/// </summary>
public static class InstallFileNames
{
    /// <summary>The game's catalog, which marks an install's root.</summary>
    public const string CatalogName = "SYS4INI.BIN";

    // Windows and macOS (by default) already match a name whatever its case.
    private static readonly bool NamesMatchExactly = !(OperatingSystem.IsWindows() || OperatingSystem.IsMacOS());

    /// <summary>Enumeration that matches a pattern whatever the case, as Windows does.</summary>
    public static EnumerationOptions AnyCase { get; } = new() { MatchCasing = MatchCasing.CaseInsensitive };

    /// <summary>
    /// The path of the file <paramref name="name"/> (a bare name) in <paramref name="directory"/>: the name as written
    /// when that file exists, else the one file whose name differs from it only in case; null when there is none. Two
    /// such files and none as written are refused, since AGE on Windows could not tell them apart.
    /// </summary>
    public static string? Find(string directory, string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(directory);
        ArgumentException.ThrowIfNullOrEmpty(name);
        string written = Path.Combine(directory, name);
        if (File.Exists(written)) return written;
        if (!NamesMatchExactly || name.AsSpan().IndexOfAny('/', '\\') >= 0 || !Directory.Exists(directory)) return null;

        string[] matches;
        try
        {
            matches = Directory.EnumerateFiles(directory).Select(path => Path.GetFileName(path))
                .Where(other => string.Equals(other, name, StringComparison.OrdinalIgnoreCase))
                .Order(StringComparer.Ordinal).ToArray();
        }
        // A directory that cannot be listed serves nothing, as a loose file that cannot be opened serves nothing.
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            return null;
        }
        return matches.Length switch
        {
            0 => null,
            1 => Path.Combine(directory, matches[0]),
            _ => throw new InvalidDataException(
                $"{Path.GetFullPath(directory)}: {string.Join(" and ", matches)} differ only in case, which AGE could "
                + "not tell apart; keep one of them"),
        };
    }

    /// <summary>The path of the install's catalog in <paramref name="root"/>, whatever its case; where it would be when
    /// there is none, so that the caller's own check reports it missing.</summary>
    public static string CatalogPath(string root) => Find(root, CatalogName) ?? Path.Combine(root, CatalogName);

    /// <summary>Whether <paramref name="root"/> holds the install's catalog, whatever its case.</summary>
    public static bool HasCatalog(string root) => Find(root, CatalogName) != null;

    /// <summary>The files in <paramref name="directory"/> that <paramref name="pattern"/> matches whatever the case,
    /// in the order Windows lists them (by name, case ignored), not the file system's own; names that differ only in
    /// case, which only a file system that matches names exactly holds, by their exact spelling.</summary>
    public static IEnumerable<string> Enumerate(string directory, string pattern)
        => Directory.EnumerateFiles(directory, pattern, AnyCase).Order(StringComparer.OrdinalIgnoreCase)
            .ThenBy(path => path, StringComparer.Ordinal);
}
