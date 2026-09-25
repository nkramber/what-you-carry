using System;
using System.Collections.Generic;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Game;
using WhatYouCarry.Game.Input;
using WhatYouCarry.Game.Measure;
using WhatYouCarry.Game.Review;
using WhatYouCarry.Game.Smoke;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// The user argument check of PR-61 (D-313, D-317; PR-61 exit tests 1 to 5 and 7). No test here starts the engine. The
/// engine test, exit test 6, is in <see cref="SmokeSessionTests"/>.
/// </summary>
[Trait("Category", DocumentsCategoryTests.DocumentsCategory)]
public sealed class UserArgumentsTests
{
    /// <summary>The separator of the engine before the user arguments, with a space on each side, as a command in `CLAUDE.md` writes it.</summary>
    private const string Separator = " -- ";

    /// <summary>
    /// Each flag takes its count of words: the smoke and bot flags none, the frame log and contact sheet flags one, and the
    /// press flag two (D-313). The flags come in any order, and an empty list holds no flag.
    /// </summary>
    [Fact]
    public void ParseReadsEachFlagAndItsWords()
    {
        UserArguments none = UserArguments.Parse([]);
        Assert.False(none.Has(SmokeSession.Flag));
        Assert.False(none.Has(BotSession.Flag));
        Assert.False(none.Has(FrameLog.Flag));
        Assert.False(none.Has(ContactSheet.Flag));
        Assert.False(none.Has(TestExit.PressFlag));

        UserArguments smoke = UserArguments.Parse([SmokeSession.Flag, FrameLog.Flag, "frames.txt"]);
        Assert.True(smoke.Has(SmokeSession.Flag));
        Assert.Empty(smoke.WordsOf(SmokeSession.Flag));
        Assert.Equal(new[] { "frames.txt" }, smoke.WordsOf(FrameLog.Flag));
        Assert.False(smoke.Has(BotSession.Flag));

        UserArguments bot = UserArguments.Parse([BotSession.Flag, FrameLog.Flag, "frames.txt", TestExit.PressFlag, TestExit.StartName, "7"]);
        Assert.True(bot.Has(BotSession.Flag));
        Assert.Empty(bot.WordsOf(BotSession.Flag));
        Assert.Equal(new[] { "frames.txt" }, bot.WordsOf(FrameLog.Flag));
        Assert.Equal(new[] { TestExit.StartName, "7" }, bot.WordsOf(TestExit.PressFlag));
        Assert.False(bot.Has(SmokeSession.Flag));

        UserArguments sheet = UserArguments.Parse([ContactSheet.Flag, "sheet.png"]);
        Assert.Equal(new[] { "sheet.png" }, sheet.WordsOf(ContactSheet.Flag));

        // The press flag before the smoke flag reads the same as after it.
        UserArguments pressFirst = UserArguments.Parse([TestExit.PressFlag, TestExit.EscapeName, "100", SmokeSession.Flag]);
        Assert.True(pressFirst.Has(SmokeSession.Flag));
        Assert.Equal(new[] { TestExit.EscapeName, "100" }, pressFirst.WordsOf(TestExit.PressFlag));

        // One dash starts no flag, so the word goes to the press flag, and the press reader rejects the tick.
        UserArguments negative = UserArguments.Parse([TestExit.PressFlag, TestExit.StartName, "-1"]);
        Assert.Equal(new[] { TestExit.StartName, "-1" }, negative.WordsOf(TestExit.PressFlag));
    }

    /// <summary>
    /// PR-61 exit test 1. A word that no flag takes stops the boot with an error that names the word: a word after the
    /// smoke flag, a word before any flag, a word with one dash, a word after the tick of the press flag (PR #58 review
    /// P2-2), and a second tick.
    /// </summary>
    [Fact]
    public void UnknownWordStopsTheBoot()
    {
        AssertStops(UserArguments.UnknownWordMessage, [SmokeSession.Flag, "unexpected"], "unexpected");
        AssertStops(UserArguments.UnknownWordMessage, ["smoke"], "smoke");
        AssertStops(UserArguments.UnknownWordMessage, ["-smoke"], "-smoke");
        AssertStops(UserArguments.UnknownWordMessage, [SmokeSession.Flag, TestExit.PressFlag, TestExit.EscapeName, "100", "unexpected"], "unexpected");
        AssertStops(UserArguments.UnknownWordMessage, [TestExit.PressFlag, TestExit.StartName, "100", "200"], "200");
    }

    /// <summary>
    /// PR-61 exit test 2. A flag outside the table stops the boot with an error that names it and every flag of the table:
    /// a misspelled flag, a flag in capitals, an engine flag after the separator, and a second separator.
    /// </summary>
    [Fact]
    public void UnknownFlagStopsTheBoot()
    {
        AssertStops(UserArguments.UnknownFlagMessage, ["--smok"], "--smok");
        AssertStops(UserArguments.UnknownFlagMessage, ["--SMOKE"], "--SMOKE");
        AssertStops(UserArguments.UnknownFlagMessage, [BotSession.Flag, "--windowed"], "--windowed");
        AssertStops(UserArguments.UnknownFlagMessage, [SmokeSession.Flag, "--"], "--");

        ContextException misspelled = Assert.Throws<ContextException>(() => UserArguments.Parse(["--smok"]));
        foreach (string flag in new[] { SmokeSession.Flag, BotSession.Flag, FrameLog.Flag, ContactSheet.Flag, TestExit.PressFlag })
        {
            Assert.Contains(flag, misspelled.Message, StringComparison.Ordinal);
        }
    }

    /// <summary>PR-61 exit test 3. A flag that appears twice stops the boot with an error that names it, and never a second read that wins.</summary>
    [Fact]
    public void RepeatedFlagStopsTheBoot()
    {
        AssertStops(UserArguments.RepeatedFlagMessage, [SmokeSession.Flag, TestExit.PressFlag, TestExit.EscapeName, "100", TestExit.PressFlag, TestExit.StartName, "200"], TestExit.PressFlag);
        AssertStops(UserArguments.RepeatedFlagMessage, [SmokeSession.Flag, SmokeSession.Flag], SmokeSession.Flag);
        AssertStops(UserArguments.RepeatedFlagMessage, [BotSession.Flag, FrameLog.Flag, "a.txt", FrameLog.Flag, "b.txt"], FrameLog.Flag);
    }

    /// <summary>
    /// PR-61 exit test 4. A flag with fewer words after it than it takes stops the boot with an error that names the flag
    /// and its count: at the end of the arguments, and before the next flag, which is never a word of the flag before it.
    /// </summary>
    [Fact]
    public void ShortFlagStopsTheBoot()
    {
        AssertStops(UserArguments.ShortFlagMessage, [TestExit.PressFlag], TestExit.PressFlag, "2");
        AssertStops(UserArguments.ShortFlagMessage, [SmokeSession.Flag, TestExit.PressFlag, TestExit.EscapeName], TestExit.PressFlag, "2");
        AssertStops(UserArguments.ShortFlagMessage, [TestExit.PressFlag, TestExit.EscapeName, SmokeSession.Flag], TestExit.PressFlag, "2");
        AssertStops(UserArguments.ShortFlagMessage, [BotSession.Flag, FrameLog.Flag], FrameLog.Flag, "1");
        AssertStops(UserArguments.ShortFlagMessage, [FrameLog.Flag, BotSession.Flag], FrameLog.Flag, "1");
        AssertStops(UserArguments.ShortFlagMessage, [ContactSheet.Flag], ContactSheet.Flag, "1");
    }

    /// <summary>
    /// F-122. An empty word or a word of white space alone after a flag stops the boot with an error that names the flag:
    /// each path flag, and a word of the press flag. The old parse took an empty frame log path, and the session hung at
    /// its end.
    /// </summary>
    [Fact]
    public void EmptyWordStopsTheBoot()
    {
        AssertStops(UserArguments.EmptyWordMessage, [SmokeSession.Flag, FrameLog.Flag, string.Empty], FrameLog.Flag);
        AssertStops(UserArguments.EmptyWordMessage, [BotSession.Flag, FrameLog.Flag, "   "], FrameLog.Flag);
        AssertStops(UserArguments.EmptyWordMessage, [ContactSheet.Flag, string.Empty], ContactSheet.Flag);
        AssertStops(UserArguments.EmptyWordMessage, [HudShot.Flag, "\t"], HudShot.Flag);
        AssertStops(UserArguments.EmptyWordMessage, [SmokeSession.Flag, TestExit.PressFlag, TestExit.EscapeName, " "], TestExit.PressFlag);
    }

    /// <summary>
    /// PR-61 exit test 5. The user arguments of the smoke, test exit, bot, and contact sheet commands in `CLAUDE.md` parse
    /// with no error, and so do the user arguments of every other command there with the separator.
    /// </summary>
    [Fact]
    public void SessionCommandsParse()
    {
        List<string> commands = [];
        foreach (string line in RepositoryRoot.ReadFile("CLAUDE.md").Split('\n'))
        {
            string[] spans = line.Split('`');
            for (int index = 1; index < spans.Length; index += 2)
            {
                int separator = spans[index].IndexOf(Separator, StringComparison.Ordinal);
                if (spans[index].StartsWith(SmokeSessionTests.LocalGodot, StringComparison.Ordinal) && separator >= 0)
                {
                    commands.Add(spans[index][(separator + Separator.Length)..]);
                }
            }
        }

        Assert.Contains("--smoke", commands);
        Assert.Contains("--smoke --press escape 100", commands);
        Assert.Contains("--bot --frame-log frames.txt", commands);
        Assert.Contains("--contact-sheet sheet.png", commands);
        foreach (string command in commands)
        {
            Exception? error = Record.Exception(() => UserArguments.Parse(command.Split(' ')));
            Assert.True(error is null, $"The user arguments '{command}' of a command in CLAUDE.md do not parse. {error?.Message}");
        }
    }

    /// <summary>
    /// PR-61 exit test 7. A flag that the session ignores stops the boot with an error that names both flags (D-317): the
    /// contact sheet flag with each other flag, in either order, and the smoke flag with the bot flag, in either order.
    /// </summary>
    [Fact]
    public void IgnoredFlagStopsTheBoot()
    {
        AssertStops(UserArguments.SheetTakesNoFlagMessage, [ContactSheet.Flag, "sheet.png", SmokeSession.Flag], ContactSheet.Flag, SmokeSession.Flag);
        AssertStops(UserArguments.SheetTakesNoFlagMessage, [BotSession.Flag, ContactSheet.Flag, "sheet.png"], ContactSheet.Flag, BotSession.Flag);
        AssertStops(UserArguments.SheetTakesNoFlagMessage, [ContactSheet.Flag, "sheet.png", FrameLog.Flag, "frames.txt"], ContactSheet.Flag, FrameLog.Flag);
        AssertStops(UserArguments.SheetTakesNoFlagMessage, [TestExit.PressFlag, TestExit.EscapeName, "100", ContactSheet.Flag, "sheet.png"], ContactSheet.Flag, TestExit.PressFlag);
        AssertStops(UserArguments.SmokeTakesNoBotMessage, [SmokeSession.Flag, BotSession.Flag], SmokeSession.Flag, BotSession.Flag);
        AssertStops(UserArguments.SmokeTakesNoBotMessage, [BotSession.Flag, SmokeSession.Flag], SmokeSession.Flag, BotSession.Flag);
    }

    /// <summary>
    /// The HUD shot takes its path and no other flag, because it takes no tick, and the error names both flags
    /// (D-133, D-317). The contact sheet and the shot also exclude each other.
    /// </summary>
    [Fact]
    public void HudShotTakesNoOtherFlag()
    {
        UserArguments shot = UserArguments.Parse([HudShot.Flag, "hud.png"]);
        Assert.True(HudShot.IsRequested(shot));
        Assert.Equal("hud.png", HudShot.PathOf(shot));
        Assert.False(HudShot.IsRequested(UserArguments.Parse([SmokeSession.Flag])));

        AssertStops(UserArguments.ShotTakesNoFlagMessage, [HudShot.Flag, "hud.png", SmokeSession.Flag], HudShot.Flag, SmokeSession.Flag);
        AssertStops(UserArguments.ShotTakesNoFlagMessage, [BotSession.Flag, HudShot.Flag, "hud.png"], HudShot.Flag, BotSession.Flag);
        AssertStops(UserArguments.SheetTakesNoFlagMessage, [ContactSheet.Flag, "sheet.png", HudShot.Flag, "hud.png"], ContactSheet.Flag, HudShot.Flag);
    }

    /// <summary>
    /// The transitions flag counts the descents of the bot session into the frame log, so it stops the boot without
    /// either of them, and the error names the flag that is absent (D-317, D-435).
    /// </summary>
    [Fact]
    public void TransitionsNeedTheBotAndTheFrameLog()
    {
        AssertStops(UserArguments.TransitionsNeedBotAndLogMessage, [BotSession.TransitionsFlag, "10"], BotSession.TransitionsFlag, BotSession.Flag);
        AssertStops(UserArguments.TransitionsNeedBotAndLogMessage, [BotSession.TransitionsFlag, "10", FrameLog.Flag, "frames.txt"], BotSession.TransitionsFlag, BotSession.Flag);
        AssertStops(UserArguments.TransitionsNeedBotAndLogMessage, [BotSession.Flag, BotSession.TransitionsFlag, "10"], BotSession.TransitionsFlag, FrameLog.Flag);
        AssertStops(UserArguments.ShortFlagMessage, [BotSession.Flag, FrameLog.Flag, "frames.txt", BotSession.TransitionsFlag], BotSession.TransitionsFlag);

        UserArguments ten = UserArguments.Parse([BotSession.Flag, FrameLog.Flag, "frames.txt", BotSession.TransitionsFlag, "10"]);
        Assert.Equal(10, BotSession.TransitionsOf(ten));
        Assert.Equal(1, BotSession.TransitionsOf(UserArguments.Parse([BotSession.Flag])));
        foreach (string bad in new[] { "0", "15", "-1", "ten", "1.5" })
        {
            UserArguments arguments = UserArguments.Parse([BotSession.Flag, FrameLog.Flag, "frames.txt", BotSession.TransitionsFlag, bad]);
            ContextException error = Assert.Throws<ContextException>(() => BotSession.TransitionsOf(arguments));
            Assert.Contains(error.Context, field => field.Value == bad);
        }
    }

    /// <summary>
    /// A read of a flag outside the table is an error that names it, and never an absent flag (T-2). A read of the words of
    /// a flag that the arguments do not hold is an error that names the flag.
    /// </summary>
    [Fact]
    public void ReadsRejectAFlagOutsideTheTableAndAnAbsentFlag()
    {
        UserArguments smoke = UserArguments.Parse([SmokeSession.Flag]);

        ContextException has = Assert.Throws<ContextException>(() => smoke.Has("--smok"));
        Assert.StartsWith(UserArguments.UnknownFlagMessage, has.Message, StringComparison.Ordinal);
        Assert.Contains(has.Context, field => field.Value == "--smok");

        ContextException words = Assert.Throws<ContextException>(() => smoke.WordsOf("--smok"));
        Assert.StartsWith(UserArguments.UnknownFlagMessage, words.Message, StringComparison.Ordinal);
        Assert.Contains(words.Context, field => field.Value == "--smok");

        ContextException absent = Assert.Throws<ContextException>(() => smoke.WordsOf(FrameLog.Flag));
        Assert.StartsWith(UserArguments.AbsentFlagMessage, absent.Message, StringComparison.Ordinal);
        Assert.Contains(absent.Context, field => field.Value == FrameLog.Flag);
    }

    /// <summary>Asserts that the parse of one argument list stops with the message of one kind, and that a context field holds each named word.</summary>
    private static void AssertStops(string message, string[] arguments, params string[] words)
    {
        ContextException error = Assert.Throws<ContextException>(() => UserArguments.Parse(arguments));
        Assert.StartsWith(message, error.Message, StringComparison.Ordinal);
        foreach (string word in words)
        {
            Assert.Contains(error.Context, field => field.Value == word);
        }
    }
}
