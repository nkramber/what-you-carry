using WhatYouCarry.Core.Entities;
using WhatYouCarry.Core.Simulation;

namespace WhatYouCarry.Game.Ui;

/// <summary>
/// The values that one frame of the HUD shows: the health and the most health (D-442), the ticks of the countdown that
/// remain and whether it expired (D-443), whether the stairwell prompt is open, and whether the controller gave the
/// last input (D-447).
/// </summary>
/// <remarks>
/// A frame of play reads the state of the loop with <see cref="Of"/>. The screenshot fixture gives its own values, so
/// the Game layer never writes a Core value to show one (G-3).
/// </remarks>
public readonly record struct HudState(int Health, int MostHealth, long RemainingTicks, bool Expired, bool PromptOpen, bool Controller)
{
    /// <summary>The countdown pauses at the open prompt before expiry, and the timer then shows its paused mark (D-140, D-443).</summary>
    public bool Paused => this.PromptOpen && !this.Expired;

    /// <summary>The state of one frame of play, from the loop.</summary>
    public static HudState Of(SimulationLoop loop, bool controller)
    {
        return new HudState(loop.Player.Health, Player.MaxHealth, loop.Timer.Remaining, loop.Timer.Expired, StairwellPrompt.IsOpen(loop), controller);
    }
}
