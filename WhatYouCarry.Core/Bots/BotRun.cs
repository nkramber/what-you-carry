using System;
using System.Collections.Generic;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Simulation;

namespace WhatYouCarry.Core.Bots;

/// <summary>
/// One headless bot run (D-115, D-127): a loop from the seed, one intent per tick from the policy, no sleep
/// between ticks, and one of the five end states of D-270 and D-403 with the budgets of D-271.
/// </summary>
/// <remarks>
/// <para>
/// A run ends at the bottom when the loop ends by an ascend, which the descender does by the ascend bit at the
/// stairwell of the last floor. A policy that promises no progress runs out its wander budget and ends by budget.
/// A policy that promises progress ends as a softlock when the floor timer expires with no floor change, so the
/// floor budget of such a policy is the length of the timer (D-420). A bot that is stuck then never dies to the
/// Overseer, and a softlock never hides behind a death. Any
/// exception ends the run as a crash, and the result holds the exception text, so the harness runs the next seed
/// and the log names the fault (T-2).
/// </para>
/// <para>
/// A run whose player reaches zero health ends as a death, which is the fifth end state (D-322, D-403). A death is
/// a real outcome of a fight and never a fault of the code, so the bot gate and the night gate fail on a crash or
/// a softlock alone. The count of deaths of a policy measures how hard the floors are. The result carries the cause
/// of a death: the family or the hunter whose hit took the last health (D-411).
/// </para>
/// <para>
/// The result carries the timer events of the run in tick order, so the run log shows each expiry, each hunter
/// spawn, and each wave (M-5).
/// </para>
/// <para>
/// The run holds no file and writes no line. The runner in the Tools project writes the log from the result.
/// </para>
/// </remarks>
public static class BotRun
{
    /// <summary>The ticks that a policy that promises no progress wanders before the run reads budget: ten minutes (D-271).</summary>
    public const uint WanderBudget = 36000;

    /// <summary>Plays one run to its end.</summary>
    public static BotRunResult Play(IBotPolicy policy, ulong seed, ContentSet content)
    {
        int floorsReached = 0;
        uint ticks = 0;
        List<TimerEvent> events = [];
        try
        {
            SimulationLoop loop = new(seed, content);
            floorsReached = loop.Floor;
            while (true)
            {
                if (loop.End == RunEnd.Ascend)
                {
                    return new BotRunResult(policy.Name, seed, BotRunEnd.Bottom, floorsReached, ticks, string.Empty, string.Empty, events);
                }

                if (loop.End == RunEnd.Death)
                {
                    return new BotRunResult(policy.Name, seed, BotRunEnd.Death, floorsReached, ticks, string.Empty, loop.DeathCause, events);
                }

                if (!policy.PromisesProgress && ticks >= WanderBudget)
                {
                    return new BotRunResult(policy.Name, seed, BotRunEnd.Budget, floorsReached, ticks, string.Empty, string.Empty, events);
                }

                // A descent starts a new timer, so an expired timer means no floor change since the floor began (D-420).
                if (policy.PromisesProgress && loop.Timer.Expired)
                {
                    return new BotRunResult(policy.Name, seed, BotRunEnd.Softlock, floorsReached, ticks, string.Empty, string.Empty, events);
                }

                loop.Step(policy.Next(loop));
                ticks++;
                foreach (TimerEvent timerEvent in loop.LastEvents)
                {
                    events.Add(timerEvent);
                }

                if (loop.Floor > floorsReached)
                {
                    floorsReached = loop.Floor;
                }
            }
        }
        catch (Exception error)
        {
            // The harness reads the crash from the result and goes on to the next seed. The message carries the
            // context of every ContextException, and the run log holds it (D-113).
            return new BotRunResult(policy.Name, seed, BotRunEnd.Crash, floorsReached, ticks, error.Message, string.Empty, events);
        }
    }
}

/// <summary>How a bot run ended (D-270, D-403).</summary>
public enum BotRunEnd
{
    /// <summary>The run ascended at the stairwell of the last floor.</summary>
    Bottom = 0,

    /// <summary>The wander budget of a policy that promises no progress ran out.</summary>
    Budget = 1,

    /// <summary>The floor timer expired with no floor progress on a policy that promises progress (D-420).</summary>
    Softlock = 2,

    /// <summary>An exception ended the run. The result holds its text.</summary>
    Crash = 3,

    /// <summary>The health of the player reached zero (D-322, D-403).</summary>
    Death = 4,
}

/// <summary>
/// The result of one bot run: the policy, the seed, the end state, the deepest floor, the ticks, the exception text
/// of a crash or empty, the cause of a death or empty (D-411), and the timer events in tick order (M-5).
/// </summary>
public sealed record BotRunResult(string Policy, ulong Seed, BotRunEnd End, int FloorsReached, uint Ticks, string Error, string Cause, IReadOnlyList<TimerEvent> Events);
