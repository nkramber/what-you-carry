using System;
using Godot;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Game.Ui;

/// <summary>
/// The one layout scale of every screen (D-446, G-15). Each screen lays out on a base of 1280 by 800 pixels, the
/// screen of the Steam Deck, and one scale fits the base to the screen: the smaller of the two ratios of the screen
/// size to the base.
/// </summary>
/// <remarks>
/// The Deck gets the scale 1.0, and a screen of 1920 by 1080 gets 1.35. The layout size is the screen size over
/// the scale, so it holds the whole base, and a wide screen gets more width and never less height. The scale goes on
/// the canvas layer of a screen alone, so the mouse motion of the look and the 3D render do not change (D-243).
/// </remarks>
public static class UiScale
{
    /// <summary>The width of the base, in pixels: the Deck screen (D-15).</summary>
    public const float BaseWide = 1280.0f;

    /// <summary>The height of the base, in pixels: the Deck screen (D-15).</summary>
    public const float BaseHigh = 800.0f;

    /// <summary>The message of the error for a screen with a side that is not above zero.</summary>
    public const string EmptyScreenMessage = "A screen has a width and a height above zero, and the layout scale reads both.";

    private const string ScreenField = "screen";

    /// <summary>The base as one size.</summary>
    public static readonly Vector2 Base = new(BaseWide, BaseHigh);

    /// <summary>The scale of one screen size: the smaller of the two ratios to the base.</summary>
    /// <exception cref="ContextException">A side of the screen is not above zero.</exception>
    public static float Factor(Vector2 screen)
    {
        if (!(screen.X > 0.0f) || !(screen.Y > 0.0f))
        {
            ContextException error = new(EmptyScreenMessage);
            error.AddContext(ScreenField, screen.ToString());
            throw error;
        }

        return MathF.Min(screen.X / BaseWide, screen.Y / BaseHigh);
    }

    /// <summary>The layout size of one screen size: the screen size over its scale. It holds the whole base.</summary>
    public static Vector2 LayoutSize(Vector2 screen)
    {
        return screen / Factor(screen);
    }
}
