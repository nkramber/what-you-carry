using WhatYouCarry.Core.Bots;
using WhatYouCarry.Core.Simulation;

namespace WhatYouCarry.Game.Measure;

/// <summary>
/// The bot session of M-3 (D-295, D-296): the greedy descender drives the Game layer over one full floor, so the
/// frame log covers a walk through every chamber on the path with no hand on the controls. The session ends
/// when the loop leaves the first floor, or when the floor budget of the bot runs passes with no descent.
/// </summary>
public static class BotSession
{
    /// <summary>The user argument that starts the session.</summary>
    public const string Flag = "--bot";

    /// <summary>The ticks that the session gives the bot for the floor before it ends as a softlock (D-271).</summary>
    public const uint TickBudget = BotRun.FloorBudget;

    /// <summary>Answers whether the user arguments ask for the session.</summary>
    public static bool IsRequested(UserArguments userArguments)
    {
        return userArguments.Has(Flag);
    }

    /// <summary>
    /// Answers whether the loop left the first floor, by a descent or by an ascend. A death is no completion of the
    /// session, and the session ends clean on the next tick, because a death is an outcome of a fight and never a
    /// fault of the code (D-322, D-403).
    /// </summary>
    public static bool IsComplete(SimulationLoop loop)
    {
        return loop.Floor > SimulationLoop.FirstFloor || loop.End == RunEnd.Ascend;
    }

    /// <summary>Answers whether the tick budget passed with the loop still on the first floor.</summary>
    public static bool IsStuck(SimulationLoop loop)
    {
        return !IsComplete(loop) && loop.Tick >= TickBudget;
    }
}
