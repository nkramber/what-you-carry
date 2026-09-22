using System.Collections.Generic;
using Godot;

namespace WhatYouCarry.Game.Ui;

/// <summary>
/// The place and the size of each fixed element of the HUD, in layout pixels on the base of <see cref="UiScale"/>
/// (D-446). The health display stands at the bottom left (D-442), the timer and its paused mark at the top center
/// (D-443), the boss bar under them (D-445), and the stairwell prompt at the lower center (D-447).
/// </summary>
/// <remarks>
/// Each element keeps a margin from the screen edge, and no two elements overlap, so a wide screen moves the
/// elements apart and never onto each other. The damage numbers have no fixed place, and
/// <see cref="DamageNumbers"/> places each one.
/// </remarks>
public sealed record HudLayout(
    Rect2 HealthText,
    Rect2 HealthBar,
    Rect2 Timer,
    Rect2 Paused,
    Rect2 BossName,
    Rect2 BossBar,
    Rect2 Prompt)
{
    /// <summary>The space between an element and the screen edge.</summary>
    public const float Margin = 24.0f;

    /// <summary>The space between two elements of one group.</summary>
    public const float Gap = 4.0f;

    /// <summary>The width of the health bar and of its number.</summary>
    public const float HealthWide = 240.0f;

    /// <summary>The height of the health bar.</summary>
    public const float HealthBarHigh = 16.0f;

    /// <summary>The height of a line of text of the health number and the boss name, at <see cref="TextFontPixels"/>.</summary>
    public const float TextHigh = 28.0f;

    /// <summary>The font size of the health number, the boss name, and the boss number.</summary>
    public const int TextFontPixels = 20;

    /// <summary>The width of the timer and of its paused mark.</summary>
    public const float TimerWide = 200.0f;

    /// <summary>The height of the timer, at <see cref="TimerFontPixels"/>.</summary>
    public const float TimerHigh = 44.0f;

    /// <summary>The font size of the timer.</summary>
    public const int TimerFontPixels = 32;

    /// <summary>The height of the paused mark, at <see cref="PausedFontPixels"/>.</summary>
    public const float PausedHigh = 24.0f;

    /// <summary>The font size of the paused mark.</summary>
    public const int PausedFontPixels = 16;

    /// <summary>The space from the top edge to the timer.</summary>
    public const float TopMargin = 16.0f;

    /// <summary>The width of the boss bar and of its name line.</summary>
    public const float BossWide = 480.0f;

    /// <summary>The height of the boss bar.</summary>
    public const float BossBarHigh = 14.0f;

    /// <summary>The width of the stairwell prompt.</summary>
    public const float PromptWide = 480.0f;

    /// <summary>The height of the stairwell prompt: two lines at <see cref="PromptFontPixels"/>.</summary>
    public const float PromptHigh = 64.0f;

    /// <summary>The font size of the stairwell prompt.</summary>
    public const int PromptFontPixels = 22;

    /// <summary>The space from the bottom edge to the stairwell prompt, above the health display.</summary>
    public const float PromptBottom = 120.0f;

    /// <summary>The layout of one layout size (D-446).</summary>
    public static HudLayout For(Vector2 size)
    {
        float healthBarTop = size.Y - Margin - HealthBarHigh;
        Rect2 healthBar = new(Margin, healthBarTop, HealthWide, HealthBarHigh);
        Rect2 healthText = new(Margin, healthBarTop - Gap - TextHigh, HealthWide, TextHigh);

        float timerLeft = (size.X - TimerWide) / 2.0f;
        Rect2 timer = new(timerLeft, TopMargin, TimerWide, TimerHigh);
        Rect2 paused = new(timerLeft, timer.End.Y, TimerWide, PausedHigh);

        float bossLeft = (size.X - BossWide) / 2.0f;
        Rect2 bossName = new(bossLeft, paused.End.Y + Gap, BossWide, TextHigh);
        Rect2 bossBar = new(bossLeft, bossName.End.Y, BossWide, BossBarHigh);

        Rect2 prompt = new((size.X - PromptWide) / 2.0f, size.Y - PromptBottom - PromptHigh, PromptWide, PromptHigh);
        return new HudLayout(healthText, healthBar, timer, paused, bossName, bossBar, prompt);
    }

    /// <summary>Every element, in the order of the record.</summary>
    public IReadOnlyList<Rect2> Elements()
    {
        return [this.HealthText, this.HealthBar, this.Timer, this.Paused, this.BossName, this.BossBar, this.Prompt];
    }
}
