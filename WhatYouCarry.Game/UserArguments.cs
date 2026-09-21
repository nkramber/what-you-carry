using System;
using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Game.Input;
using WhatYouCarry.Game.Measure;
using WhatYouCarry.Game.Review;
using WhatYouCarry.Game.Smoke;

namespace WhatYouCarry.Game;

/// <summary>
/// The user arguments of the Game layer: the words after the separator <c>--</c> of the engine, read once at boot by
/// one parser (D-313). The parser holds each flag and the count of words after it. A bad argument stops the boot with
/// an error that names it, and <c>Main</c> quits with exit code 1 (T-2).
/// </summary>
/// <remarks>
/// <para>
/// A word that starts with <see cref="FlagStart"/> is a flag. Any other word belongs to the flag before it, up to the
/// count of that flag. Five kinds of argument stop the boot: a word that no flag takes, a flag outside the table, a
/// flag that appears twice, a flag with fewer words than it takes, and a flag that the session ignores (D-317). A flag
/// is never a word of the flag before it, so a short flag is an error, and never a flag that the session loses.
/// </para>
/// <para>
/// The session types read their flags through this type, and no other type reads the argument array. A PR that adds a
/// flag adds it to the table, and to the ignored flag rule when a session ignores it (D-313, D-317).
/// </para>
/// </remarks>
public sealed class UserArguments
{
    /// <summary>The start of every flag.</summary>
    public const string FlagStart = "--";

    /// <summary>The message of the error for a word that no flag takes.</summary>
    public const string UnknownWordMessage = "The user arguments hold a word that no flag takes.";

    /// <summary>The message of the error for a flag outside the table.</summary>
    public const string UnknownFlagMessage = "The flag is not in the flag table of the Game layer.";

    /// <summary>The message of the error for a flag that appears twice.</summary>
    public const string RepeatedFlagMessage = "The user arguments hold a flag twice, and each flag can appear once.";

    /// <summary>The message of the error for a flag with fewer words after it than it takes.</summary>
    public const string ShortFlagMessage = "The flag has fewer words after it than it takes.";

    /// <summary>The message of the error for the contact sheet flag with any other flag (D-317).</summary>
    public const string SheetTakesNoFlagMessage = "The contact sheet starts no loop, so it takes no other flag.";

    /// <summary>The message of the error for the smoke flag with the bot flag (D-317).</summary>
    public const string SmokeTakesNoBotMessage = "The smoke script gives every intent, so the smoke flag takes no bot flag.";

    /// <summary>The message of the error for the transitions flag with no bot flag or no frame log flag (D-317, D-435).</summary>
    public const string TransitionsNeedBotAndLogMessage = "The transitions flag counts the descents of the bot session into the frame log, so it needs the bot flag and the frame log flag.";

    /// <summary>The message of the error for a read of the words of a flag that the arguments do not hold.</summary>
    public const string AbsentFlagMessage = "The user arguments hold no such flag.";

    private const string ArgumentField = "argument";
    private const string FlagField = "flag";
    private const string FlagsField = "flags";
    private const string WordsField = "words";
    private const string OtherField = "other";
    private const string FlagListSeparator = ", ";

    /// <summary>Every flag of the Game layer and the count of words after it (D-313).</summary>
    private static readonly Dictionary<string, int> WordsTaken = new(StringComparer.Ordinal)
    {
        [SmokeSession.Flag] = 0,
        [BotSession.Flag] = 0,
        [BotSession.TransitionsFlag] = 1,
        [FrameLog.Flag] = 1,
        [ContactSheet.Flag] = 1,
        [TestExit.PressFlag] = 2,
    };

    /// <summary>The words of each flag that the arguments hold, by flag.</summary>
    private readonly Dictionary<string, string[]> flags;

    private UserArguments(Dictionary<string, string[]> flags)
    {
        this.flags = flags;
    }

    /// <summary>
    /// Reads every user argument once, from the first to the last. A bad argument is an error that names it. An empty
    /// list is a session with no flag.
    /// </summary>
    /// <exception cref="ContextException">A word that no flag takes, a flag outside the table, a flag twice, a short flag, or a flag that the session ignores.</exception>
    public static UserArguments Parse(string[] userArguments)
    {
        Dictionary<string, string[]> flags = new(StringComparer.Ordinal);
        int index = 0;
        while (index < userArguments.Length)
        {
            string argument = userArguments[index];
            if (!argument.StartsWith(FlagStart, StringComparison.Ordinal))
            {
                ContextException unknownWord = new(UnknownWordMessage);
                unknownWord.AddContext(ArgumentField, argument);
                throw unknownWord;
            }

            if (!WordsTaken.TryGetValue(argument, out int count))
            {
                throw UnknownFlagError(argument);
            }

            if (flags.ContainsKey(argument))
            {
                ContextException repeated = new(RepeatedFlagMessage);
                repeated.AddContext(FlagField, argument);
                throw repeated;
            }

            string[] words = new string[count];
            for (int offset = 0; offset < count; offset++)
            {
                // The next flag is never a word of this one, so a short flag stops here and no flag is lost.
                int position = index + 1 + offset;
                if (position >= userArguments.Length || userArguments[position].StartsWith(FlagStart, StringComparison.Ordinal))
                {
                    ContextException shortFlag = new(ShortFlagMessage);
                    shortFlag.AddContext(FlagField, argument);
                    shortFlag.AddContext(WordsField, count.ToString(CultureInfo.InvariantCulture));
                    throw shortFlag;
                }

                words[offset] = userArguments[position];
            }

            flags.Add(argument, words);
            index += 1 + count;
        }

        RejectIgnoredFlags(flags);
        return new UserArguments(flags);
    }

    /// <summary>Answers whether the arguments hold one flag of the table.</summary>
    /// <exception cref="ContextException">The flag is not in the table. That is a defect of the caller, and never an absent flag (T-2).</exception>
    public bool Has(string flag)
    {
        if (!WordsTaken.ContainsKey(flag))
        {
            throw UnknownFlagError(flag);
        }

        return this.flags.ContainsKey(flag);
    }

    /// <summary>The words after one flag, as many as the table gives that flag.</summary>
    /// <exception cref="ContextException">The flag is not in the table, or the arguments do not hold it.</exception>
    public IReadOnlyList<string> WordsOf(string flag)
    {
        if (!WordsTaken.ContainsKey(flag))
        {
            throw UnknownFlagError(flag);
        }

        if (!this.flags.TryGetValue(flag, out string[]? words))
        {
            ContextException absent = new(AbsentFlagMessage);
            absent.AddContext(FlagField, flag);
            throw absent;
        }

        return words;
    }

    /// <summary>
    /// Stops the boot on a flag that the session ignores (D-317). The contact sheet starts no loop, so it ignores every
    /// other flag. The smoke script gives the intent of every tick, so the bot of the bot flag never drives the loop.
    /// The transitions flag counts the descents of the bot session into the frame log, so it needs both (D-435).
    /// The error names both flags.
    /// </summary>
    /// <exception cref="ContextException">The contact sheet flag with another flag, the smoke flag with the bot flag, or the transitions flag with no bot flag or no frame log flag.</exception>
    private static void RejectIgnoredFlags(Dictionary<string, string[]> flags)
    {
        if (flags.ContainsKey(ContactSheet.Flag))
        {
            foreach (string flag in flags.Keys)
            {
                if (flag == ContactSheet.Flag)
                {
                    continue;
                }

                ContextException withSheet = new(SheetTakesNoFlagMessage);
                withSheet.AddContext(FlagField, ContactSheet.Flag);
                withSheet.AddContext(OtherField, flag);
                throw withSheet;
            }
        }

        if (flags.ContainsKey(SmokeSession.Flag) && flags.ContainsKey(BotSession.Flag))
        {
            ContextException withBot = new(SmokeTakesNoBotMessage);
            withBot.AddContext(FlagField, SmokeSession.Flag);
            withBot.AddContext(OtherField, BotSession.Flag);
            throw withBot;
        }

        if (flags.ContainsKey(BotSession.TransitionsFlag) && !(flags.ContainsKey(BotSession.Flag) && flags.ContainsKey(FrameLog.Flag)))
        {
            ContextException alone = new(TransitionsNeedBotAndLogMessage);
            alone.AddContext(FlagField, BotSession.TransitionsFlag);
            alone.AddContext(OtherField, flags.ContainsKey(BotSession.Flag) ? FrameLog.Flag : BotSession.Flag);
            throw alone;
        }
    }

    /// <summary>The error for a flag outside the table. It names the flag and every flag of the table (T-2).</summary>
    private static ContextException UnknownFlagError(string flag)
    {
        ContextException error = new(UnknownFlagMessage);
        error.AddContext(FlagField, flag);
        error.AddContext(FlagsField, string.Join(FlagListSeparator, WordsTaken.Keys));
        return error;
    }
}
