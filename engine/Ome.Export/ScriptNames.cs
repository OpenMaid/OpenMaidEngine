using System.Text.RegularExpressions;
using Ome.Engine.Localization;

namespace Ome.Export;

/// <summary>Script names as the declarations write them (game data export spec §5.2): upper case, no <c>.BIN</c>, and
/// for a builder or a text script without an append's <c>$n$</c> prefix, so <c>*INIT</c> takes <c>$3$ITINIT</c>.
/// </summary>
public static class ScriptNames
{
    private static readonly Regex Layer = new(@"^\$\d+\$", RegexOptions.CultureInvariant);

    /// <summary>A script's name without its extension, upper case, its <c>$n$</c> prefix kept.</summary>
    public static string Normalized(string name) => TextCatalog.NormalizeScriptName(name);

    /// <summary>A script's name without its extension and its <c>$n$</c> prefix: the base script it updates.</summary>
    public static string Base(string name) => Layer.Replace(Normalized(name), "");

    /// <summary>Whether <paramref name="name"/> matches <paramref name="pattern"/>, where <c>*</c> is any run of
    /// characters.</summary>
    public static bool Matches(string pattern, string name)
        => Regex.IsMatch(name, "^" + string.Join(".*", pattern.Split('*').Select(Regex.Escape)) + "$",
                         RegexOptions.CultureInvariant);
}

/// <summary>Which scripts' writes may be published (spec §5.5): a script whose base name matches a builder pattern
/// and is no excluded builder.</summary>
public sealed class BuilderRule(IReadOnlyList<string> builders, IReadOnlyList<string> excluded)
{
    private readonly HashSet<string> _excluded = new(excluded, StringComparer.Ordinal);

    public bool Allows(string script)
    {
        string name = ScriptNames.Base(script);
        return !_excluded.Contains(name) && builders.Any(pattern => ScriptNames.Matches(pattern, name));
    }
}
