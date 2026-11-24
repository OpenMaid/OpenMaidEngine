using Ome.Mods.Launch;

namespace Ome.Launcher.Core;

/// <summary>A profile a command line names that cannot be used: its message says why (exit 2 in the programs).</summary>
public sealed class ProfileLookupException(string message) : Exception(message);

/// <summary>
/// A player profile named on a command line (modding from a release spec §6.3): <c>OME-Tool</c>'s and <c>Ome.Cli</c>'s
/// <c>--profile ID</c>, and the id form of the launcher's own <c>--launch</c>, <c>--check</c> and
/// <c>--print-command</c> (spec §8.6). The profile's trust is honoured as the launcher's window honours it.
/// </summary>
public static class ProfileLookup
{
    /// <summary>Why a profile root that needs review opens nothing (launcher spec §12).</summary>
    public static string ReviewRefusal(ProfileEntry profile)
        => $"{profile.Root} is a profile root the launcher did not create: nothing opens its paths before the player "
           + "accepts its review in the launcher's window";

    /// <summary>The profile of <paramref name="home"/> with the id <paramref name="id"/>, or null.</summary>
    public static ProfileEntry? Find(string id, OmeHome home, LauncherSettings settings, Func<MachineState> machine)
        => ProfileId.IsValid(id) ? ProfileStore.Find(ProfileStore.List(home, settings, machine), id) : null;

    /// <summary>The usable, reviewed profile with the id <paramref name="id"/>. Throws
    /// <see cref="ProfileLookupException"/> for an id that names no profile, a profile that needs review, and one that
    /// cannot be used; <paramref name="warnings"/> gets the settings file's warnings.</summary>
    public static ProfileEntry Usable(string id, OmeHome home, Func<MachineState> machine, Action<string> warnings)
    {
        LauncherSettingsFile settings = LauncherSettingsStore.Load(home);
        foreach (string warning in settings.Warnings) warnings(warning);
        ProfileEntry profile = Find(id, home, settings.Settings, machine)
            ?? throw new ProfileLookupException(
                $"no player profile has the id '{id}' in OME home {home.Root}; a game is named with --game ID, and a "
                + "profile's folder with --data-root DIR");
        if (profile.NeedsReview) throw new ProfileLookupException(ReviewRefusal(profile));
        if (!profile.Usable)
            throw new ProfileLookupException($"{profile.Id} ({profile.Root}) cannot be used: {profile.Problem}");
        return profile;
    }
}
