using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Simulation;

namespace WhatYouCarry.Game.Review;

/// <summary>
/// The screenshot fixture of the HUD for the Tier 4 pass at the phase gate (D-133, PR-19). The Game flag
/// <c>--hud-shot &lt;png&gt;</c> renders one frame of 1280 by 800 pixels, the Deck screen, with the HUD over the spawn
/// of the first run, and quits.
/// </summary>
/// <remarks>
/// <para>
/// The frame shows every element of the HUD in one state: the health at <see cref="Health"/>, the timer paused at the
/// open stairwell prompt with the keyboard buttons, the boss bar placeholder at <see cref="BossHealth"/> of
/// <see cref="BossMost"/> (D-445), and one damage number of each kind above the body. The loop takes no tick, so the
/// timer shows the full length of the first floor.
/// </para>
/// <para>
/// The render goes to a viewport of the Deck size that shares the world of the scene, so the frame has the scale 1.0
/// on any window (D-446). The render needs a window, as the contact sheet does, so the shot runs on a desktop and not in
/// CI, and a headless run is an error line and exit code 1 (D-306).
/// </para>
/// </remarks>
public static class HudShot
{
    /// <summary>The user argument that starts the shot. The next argument is the path of the PNG file.</summary>
    public const string Flag = "--hud-shot";

    /// <summary>The width of the frame, in pixels: the Deck screen (D-15).</summary>
    public const int PixelsWide = 1280;

    /// <summary>The height of the frame, in pixels: the Deck screen (D-15).</summary>
    public const int PixelsHigh = 800;

    /// <summary>The frames that the scene draws before the shot, so every shader and the HUD are ready.</summary>
    public const int WarmUpFrames = 30;

    /// <summary>The health of the player in the frame.</summary>
    public const int Health = 72;

    /// <summary>The health of the boss bar placeholder in the frame (D-445).</summary>
    public const int BossHealth = 640;

    /// <summary>The most health of the boss bar placeholder in the frame (D-445).</summary>
    public const int BossMost = 1000;

    /// <summary>The damage number of the health that the player loses in the frame (D-444).</summary>
    public const int TakenAmount = 12;

    /// <summary>The damage number of the health that an enemy loses in the frame. It stands on the body, as no enemy stands at the spawn (D-444).</summary>
    public const int DealtAmount = 25;

    /// <summary>The owner of both damage numbers of the frame: the player, whose body stands at the spawn.</summary>
    public const int NumberOwner = SimulationLoop.PlayerOwner;

    /// <summary>Answers whether the arguments ask for the shot.</summary>
    public static bool IsRequested(UserArguments arguments)
    {
        return arguments.Has(Flag);
    }

    /// <summary>The path of the PNG file: the one word after the flag.</summary>
    /// <exception cref="ContextException">The arguments hold no shot flag.</exception>
    public static string PathOf(UserArguments arguments)
    {
        return arguments.WordsOf(Flag)[0];
    }
}
