using System.Globalization;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Simulation;

namespace WhatYouCarry.Game.Measure;

/// <summary>
/// The bot session of M-3 (D-295, D-296): the greedy descender drives the Game layer over one full floor, so the
/// frame log covers a walk through every chamber on the path with no hand on the controls. The session ends one
/// second after the loop leaves the first floor, or when the floor budget of the bot runs passes with no descent.
/// </summary>
/// <remarks>
/// The transitions flag of exit test 6 of PR-18 sets the count of descents before the end (D-435). The frame log
/// then marks each transition, and the session fails when the slowest frame of one window is over the hitch
/// budget of D-427. The flag needs the bot flag and the frame log flag (D-317). A run with the enemies dies before
/// ten floors, so the session loads the content with no enemy family (D-437).
/// </remarks>
public static class BotSession
{
    /// <summary>The user argument that starts the session.</summary>
    public const string Flag = "--bot";

    /// <summary>The user argument that sets the count of descents of the session. Its one word is the count (D-435).</summary>
    public const string TransitionsFlag = "--transitions";

    /// <summary>The largest count of transitions: floor 1 to floor 15 holds fourteen descents (D-3).</summary>
    public const int MaxTransitions = 14;

    /// <summary>
    /// The ticks that the session gives the bot for each floor before it ends as stuck: five minutes, the floor budget
    /// of D-271. The timer of floor 1 expires first, so a stuck bot meets the Overseer before this budget (D-407).
    /// </summary>
    public const uint TickBudget = 18000;

    /// <summary>The ticks that the session runs on the last floor after its last descent, so the frame log holds the frames after the swap: one second.</summary>
    public const uint SettleTicks = 60;

    /// <summary>
    /// The hitch budget of a transition on the Steam Deck, in microseconds: two frames at the 90 frames per second of
    /// D-295 (D-427). A fallback of M-3 to 60 frames per second makes it 33000.
    /// </summary>
    public const long HitchBudgetMicros = 22000;

    private const string BadCountMessage = "The count of transitions is not a whole number from 1 to 14.";
    private const string CountField = "count";

    /// <summary>Answers whether the user arguments ask for the session.</summary>
    public static bool IsRequested(UserArguments userArguments)
    {
        return userArguments.Has(Flag);
    }

    /// <summary>The count of descents of the session: the word of the transitions flag, or one with no flag.</summary>
    /// <exception cref="ContextException">The word is not a whole number from 1 to <see cref="MaxTransitions"/>.</exception>
    public static int TransitionsOf(UserArguments userArguments)
    {
        if (!userArguments.Has(TransitionsFlag))
        {
            return 1;
        }

        string word = userArguments.WordsOf(TransitionsFlag)[0];
        if (!int.TryParse(word, NumberStyles.None, CultureInfo.InvariantCulture, out int count) || count < 1 || count > MaxTransitions)
        {
            ContextException error = new(BadCountMessage);
            error.AddContext(CountField, word);
            throw error;
        }

        return count;
    }

    /// <summary>
    /// Answers whether the session reached its end: the loop is <paramref name="transitions"/> floors below the first
    /// and ran <see cref="SettleTicks"/> ticks there, or the run ascended. A death is no completion of the session,
    /// and the session ends on the next tick, because a death is an outcome of a fight and never a fault of the code
    /// (D-322, D-403).
    /// </summary>
    /// <param name="ticksOnFloor">The ticks since the loop came to its floor.</param>
    public static bool IsComplete(SimulationLoop loop, int transitions, uint ticksOnFloor)
    {
        bool deepEnough = loop.Floor >= SimulationLoop.FirstFloor + transitions;
        return (deepEnough && ticksOnFloor >= SettleTicks) || loop.End == RunEnd.Ascend;
    }

    /// <summary>Answers whether the tick budget of the floor passed with no descent, above the last floor of the session.</summary>
    /// <param name="ticksOnFloor">The ticks since the loop came to its floor.</param>
    public static bool IsStuck(SimulationLoop loop, int transitions, uint ticksOnFloor)
    {
        bool deepEnough = loop.Floor >= SimulationLoop.FirstFloor + transitions;
        return !deepEnough && loop.End != RunEnd.Ascend && ticksOnFloor >= TickBudget;
    }
}
