namespace Ome.Engine.Model;

/// <summary>What a movie's flags word (the last operand of ops 0x20f, 0x236, 0x241 and 0x24d) asks of native's tick.
/// The sound route bits are <see cref="MovieSoundRoute"/>'s.</summary>
public static class MovieFlagBits
{
    /// <summary>A service movie: bit 2 set and bit 0 clear, as surface_movie_is_playing tests (4.24N @0x4051f0, 4.33G
    /// @0x405150, 4.38D @0x404eb0). Such a movie holds op 0x21c's effect-service wait, and action 4 skips it: the
    /// run-state 0x2000 and 0x400 services seek every service movie to its end (4.24N
    /// movie_skip_to_end_if_service@0x407cf0, 4.33G @0x407800). Any other movie plays to its end whatever the player
    /// presses. Himegari's and Verita's LOGO and Himegari's OP pass 4, Kamidori's LOGO and OP and Arterial's LOGO
    /// 0x20004; every game's ED, and Verita's and Arterial's OP, pass <c>unlocked &lt;&lt; 2</c> (op 0x19d), so their first showing cannot be skipped.</summary>
    public static bool IsServiceMovie(long movieFlags) => (movieFlags & 5) == 4;

    /// <summary>The loop flag, bit 0, which the movie start keeps at the object's +0x43c. At the movie's end the tick
    /// stops it, seeks it to 0 and starts it again, and the movie lives on until its surface is released (4.24N
    /// engine_main_tick 0x412368; 4.24J 0x411bd6, 4.33G 0x411e0c and 4.38D 0x41223c branch alike). Verita's CHMODE3
    /// plays its Power charge sparkle so (MVB001I.AGF, flags 0x40001); no Himegari, Kamidori or Arterial script sets
    /// it.</summary>
    public static bool Loops(long movieFlags) => (movieFlags & 1) != 0;
}
