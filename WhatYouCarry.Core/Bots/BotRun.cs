using System;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Simulation;

namespace WhatYouCarry.Core.Bots;

/// <summary>
/// One headless bot run (D-115, D-127): a loop from the seed, one intent per tick from the policy, no sleep
/// between ticks, and one of the four end states of D-270 with the budgets of D-271.
/// </summary>
/// <remarks>
/// <para>
/// A run ends at the bottom when the loop ends, which the descender does by the ascend bit at the stairwell of
/// the last floor. A policy that promises no progress runs out its wander budget and ends by budget. A policy
/// that promises progress ends as a softlock when a floor budget passes with no floor change. Any exception
/// ends the run as a crash, and the result holds the exception text, so the harness runs the next seed and the
/// log names the fault (T-2).
/// </para>
/// <para>
/// The run holds no file and writes no line. The runner in the Tools project writes the log from the result.
/// </para>
/// </remarks>
public static class BotRun
{
    /// <summary>The ticks that a policy that promises progress has per floor before the run reads softlock: five minutes (D-271).</summary>
    public const uint FloorBudget = 18000;

    /// <summary>The ticks that a policy that promises no progress wanders before the run reads budget: ten minutes (D-271).</summary>
    public const uint WanderBudget = 36000;

    /// <summary>Plays one run to its end.</summary>
    public static BotRunResult Play(IBotPolicy policy, ulong seed, ContentSet content)
    {
        int floorsReached = 0;
        uint ticks = 0;
        try
        {
            SimulationLoop loop = new(seed, content);
            floorsReached = loop.Floor;
            uint floorStart = 0;
            while (true)
            {
                if (loop.Ended)
                {
                    return new BotRunResult(policy.Name, seed, BotRunEnd.Bottom, floorsReached, ticks, string.Empty);
                }

                if (!policy.PromisesProgress && ticks >= WanderBudget)
                {
                    return new BotRunResult(policy.Name, seed, BotRunEnd.Budget, floorsReached, ticks, string.Empty);
                }

                if (policy.PromisesProgress && ticks - floorStart >= FloorBudget)
                {
                    return new BotRunResult(policy.Name, seed, BotRunEnd.Softlock, floorsReached, ticks, string.Empty);
                }

                loop.Step(policy.Next(loop));
                ticks++;
                if (loop.Floor > floorsReached)
                {
                    floorsReached = loop.Floor;
                    floorStart = ticks;
                }
            }
        }
        catch (Exception error)
        {
            // The harness reads the crash from the result and goes on to the next seed. The message carries the
            // context of every ContextException, and the run log holds it (D-113).
            return new BotRunResult(policy.Name, seed, BotRunEnd.Crash, floorsReached, ticks, error.Message);
        }
    }
}

/// <summary>How a bot run ended (D-270).</summary>
public enum BotRunEnd
{
    /// <summary>The run ascended at the stairwell of the last floor.</summary>
    Bottom = 0,

    /// <summary>The wander budget of a policy that promises no progress ran out.</summary>
    Budget = 1,

    /// <summary>A floor budget passed with no floor progress on a policy that promises progress.</summary>
    Softlock = 2,

    /// <summary>An exception ended the run. The result holds its text.</summary>
    Crash = 3,
}

/// <summary>The result of one bot run: the policy, the seed, the end state, the deepest floor, the ticks, and the exception text of a crash, or empty.</summary>
public sealed record BotRunResult(string Policy, ulong Seed, BotRunEnd End, int FloorsReached, uint Ticks, string Error);
