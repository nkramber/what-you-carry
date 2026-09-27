using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Core.Bots;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Simulation;

namespace WhatYouCarry.Game.Measure;

/// <summary>
/// The bot session of M-3 (D-295, D-296): a bot policy of Core drives the Game layer with no hand on the controls.
/// With no policy flag, the greedy descender walks one full floor, so the frame log covers a walk through every
/// chamber on the path. The session ends one second after the loop leaves the first floor, at the end of the run,
/// or when the floor budget of the bot runs passes with no descent.
/// </summary>
/// <remarks>
/// <para>
/// The transitions flag of exit test 6 of PR-18 sets the count of descents before the end (D-435). The frame log
/// then marks each transition, and the session fails when the slowest frame of one window is over the hitch
/// budget of D-427. The flag needs the bot flag and the frame log flag (D-317). A run with the enemies dies before
/// ten floors, so the session loads the content with no enemy family (D-437).
/// </para>
/// <para>
/// The policy flag names the bot policy of Core that drives the session. The timer tester stands at the floor
/// entry, so the session reaches the expiry of the floor timer and the spawn of the Overseer with the enemies, and
/// the frame log marks that tick (D-646, RR-P3-16). The transitions test counts the descents of the greedy
/// descender on floors with no enemy (D-437), so the policy flag needs the bot flag and takes no transitions flag
/// (D-317).
/// </para>
/// </remarks>
public static class BotSession
{
    /// <summary>The user argument that starts the session.</summary>
    public const string Flag = "--bot";

    /// <summary>The user argument that sets the count of descents of the session. Its one word is the count (D-435).</summary>
    public const string TransitionsFlag = "--transitions";

    /// <summary>The user argument that names the bot policy of the session. Its one word is the name of a policy of Core (D-646).</summary>
    public const string PolicyFlag = "--policy";

    /// <summary>The message of the error for a policy flag whose word names no bot policy of Core.</summary>
    public const string UnknownPolicyMessage = "The policy flag names no bot policy of Core.";

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
    /// D-295, 22222 (D-427, D-635). The frame log reads the real time of each frame, so one missed vsync reads its own
    /// time and not the rounded interval. A fallback of M-3 to 60 frames per second makes it 33333.
    /// </summary>
    public const long HitchBudgetMicros = 22222;

    private const string BadCountMessage = "The count of transitions is not a whole number from 1 to 14.";
    private const string CountField = "count";
    private const string PolicyField = "policy";
    private const string PoliciesField = "policies";
    private const string NameListSeparator = ", ";

    /// <summary>The name of each bot policy of Core that the policy flag takes, in the order of the bot runner.</summary>
    public static readonly IReadOnlyList<string> PolicyNames =
    [
        RandomWalker.PolicyName,
        GreedyDescender.PolicyName,
        FullClearer.PolicyName,
        TimerTester.PolicyName,
        Coward.PolicyName,
    ];

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
    /// The name of the bot policy of the session: the word of the policy flag, or the greedy descender with no flag. The
    /// boot reads it before the content loads, so a bad name stops the boot early.
    /// </summary>
    /// <exception cref="ContextException">The word names no policy of <see cref="PolicyNames"/>. The error names the word and lists the names.</exception>
    public static string PolicyNameOf(UserArguments userArguments)
    {
        if (!userArguments.Has(PolicyFlag))
        {
            return GreedyDescender.PolicyName;
        }

        string word = userArguments.WordsOf(PolicyFlag)[0];
        foreach (string name in PolicyNames)
        {
            if (name == word)
            {
                return name;
            }
        }

        throw UnknownPolicyError(word);
    }

    /// <summary>
    /// A new bot policy of one name. The random walker draws from the Bot stream of the run seed (D-272), and the
    /// greedy descender and the full clearer read the deepest floor of the content.
    /// </summary>
    /// <param name="name">A name of <see cref="PolicyNames"/>.</param>
    /// <param name="content">The content of the loop.</param>
    /// <param name="seed">The seed of the run.</param>
    /// <exception cref="ContextException">The name is not in <see cref="PolicyNames"/>.</exception>
    public static IBotPolicy PolicyFor(string name, ContentSet content, ulong seed)
    {
        switch (name)
        {
            case RandomWalker.PolicyName:
                return new RandomWalker(seed);
            case GreedyDescender.PolicyName:
                return new GreedyDescender(content);
            case FullClearer.PolicyName:
                return new FullClearer(content);
            case TimerTester.PolicyName:
                return new TimerTester();
            case Coward.PolicyName:
                return new Coward();
            default:
                throw UnknownPolicyError(name);
        }
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

    /// <summary>The error for a word that names no bot policy. It names the word and every name of <see cref="PolicyNames"/> (T-2).</summary>
    private static ContextException UnknownPolicyError(string word)
    {
        ContextException error = new(UnknownPolicyMessage);
        error.AddContext(PolicyField, word);
        error.AddContext(PoliciesField, string.Join(NameListSeparator, PolicyNames));
        return error;
    }
}
