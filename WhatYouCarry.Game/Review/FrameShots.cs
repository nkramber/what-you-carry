using System.Globalization;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Game.Review;

/// <summary>
/// The frame capture of the bot session for the Tier 4 pass at Gate 2 (D-133, D-711). The Game flag
/// <c>--frame-shots &lt;directory&gt;</c> writes one frame of 1280 by 800 pixels, the Deck screen, with the HUD, to
/// the directory once each second of game time. The model of the Tier 4 pass reads the frames (D-712).
/// </summary>
/// <remarks>
/// <para>
/// The render goes to a viewport of the Deck size that shares the world of the scene, as the HUD shot does, so each
/// frame has the scale 1.0 on any window (D-446). The HUD of the session draws into that viewport, and the window shows
/// the world alone.
/// </para>
/// <para>
/// A shot reads the image that the viewport drew last, which is the frame before the tick of the shot. The capture
/// needs a window, so a headless run is an error line and exit code 1 (D-306). The flag needs the bot flag, because
/// only a bot session plays the same run each time (D-714).
/// </para>
/// <para>
/// The engine draws no frame while the system hides or covers the window, and the ticks go on. A shot with no new
/// frame since the last shot then repeats an old image, so it is an error line and exit code 1 (T-2). The first capture
/// of PR-96 wrote 204 copies of one image after a second window covered the capture window.
/// </para>
/// </remarks>
public static class FrameShots
{
    /// <summary>The user argument that starts the capture. The next argument is the path of the directory.</summary>
    public const string Flag = "--frame-shots";

    /// <summary>The ticks between two shots: one second of game time at 60 Hz (D-73, D-711).</summary>
    public const uint TicksPerShot = 60;

    /// <summary>The message of the error for a shot with no new frame since the last shot.</summary>
    public const string NoNewFrameMessage = "The engine drew no frame since the last frame shot, so the shot repeats an old image. A window that the system hides or covers draws no frame.";

    private const string NameFormat = "floor-{0:D2}-tick-{1:D6}.png";
    private const string FramesDrawnField = "framesDrawn";
    private const string FramesAtLastShotField = "framesAtLastShot";

    /// <summary>Answers whether the arguments ask for the capture.</summary>
    public static bool IsRequested(UserArguments arguments)
    {
        return arguments.Has(Flag);
    }

    /// <summary>The path of the directory of the frames: the one word after the flag.</summary>
    /// <exception cref="ContextException">The arguments hold no capture flag.</exception>
    public static string DirectoryOf(UserArguments arguments)
    {
        return arguments.WordsOf(Flag)[0];
    }

    /// <summary>Answers whether the tick takes a shot: each whole second of game time after the start of the run.</summary>
    public static bool IsShotTick(uint tick)
    {
        return tick > 0 && tick % TicksPerShot == 0;
    }

    /// <summary>
    /// The file name of the shot of a tick. The floor and the tick have a fixed width, so the names sort in the order
    /// of the run, and the tick locates the frame in the log of a bot run of the same seed.
    /// </summary>
    public static string FileName(int floor, uint tick)
    {
        return string.Format(CultureInfo.InvariantCulture, NameFormat, floor, tick);
    }

    /// <summary>
    /// Stops a shot when the engine drew no frame since the last shot, because the viewport then holds an old image.
    /// The error names both frame counts (T-2).
    /// </summary>
    /// <param name="framesDrawn">The count of frames that the engine drew, now.</param>
    /// <param name="framesAtLastShot">The count of frames that the engine drew at the last shot, or at the start of the capture.</param>
    /// <exception cref="ContextException">The engine drew no frame since the last shot.</exception>
    public static void RequireNewFrame(long framesDrawn, long framesAtLastShot)
    {
        if (framesDrawn > framesAtLastShot)
        {
            return;
        }

        ContextException error = new(NoNewFrameMessage);
        error.AddContext(FramesDrawnField, framesDrawn.ToString(CultureInfo.InvariantCulture));
        error.AddContext(FramesAtLastShotField, framesAtLastShot.ToString(CultureInfo.InvariantCulture));
        throw error;
    }
}
