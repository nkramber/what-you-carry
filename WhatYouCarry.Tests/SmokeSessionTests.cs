using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Game;
using WhatYouCarry.Game.Input;
using WhatYouCarry.Game.Logging;
using WhatYouCarry.Game.Smoke;
using Xunit;
using CoreVector3 = WhatYouCarry.Core.Physics.Vector3;

namespace WhatYouCarry.Tests;

/// <summary>
/// The smoke session (D-114, D-149; PR-12 exit test 4), the two engine tests of the test exit (D-311; PR-60 exit
/// tests 2 and 3), and the engine test of a bad user argument (D-313; PR-61 exit test 6). The script tests run in
/// this process, and the tests of the Smoke category start the engine headless with the built Game assembly.
/// </summary>
public sealed class SmokeSessionTests
{
    /// <summary>The environment variable that names the Godot executable. The smoke workflow sets it on each platform.</summary>
    public const string GodotVariable = "WYC_GODOT";

    /// <summary>The executable of the local Godot .NET build that `CLAUDE.md` names, for a run on the Mac of the owner with no variable set.</summary>
    public const string LocalGodot = "/Applications/Godot_mono.app/Contents/MacOS/Godot";

    /// <summary>The category trait of the one test that needs the engine. The CI workflow leaves that category to the smoke workflow.</summary>
    public const string SmokeCategory = "Smoke";

    /// <summary>The longest wait for the engine to end, in milliseconds. A local run ends inside two seconds.</summary>
    public const int TimeoutMilliseconds = 180000;

    /// <summary>The tick of the scripted press in the two test exit runs: inside the first part of the smoke script.</summary>
    public const uint PressTick = 100;

    /// <summary>The flag starts the session, and nothing else does.</summary>
    [Fact]
    public void IsRequestedReadsTheFlag()
    {
        Assert.True(SmokeSession.IsRequested(UserArguments.Parse([SmokeSession.Flag])));
        Assert.True(SmokeSession.IsRequested(UserArguments.Parse([TestExit.PressFlag, TestExit.EscapeName, "1", SmokeSession.Flag])));
        Assert.False(SmokeSession.IsRequested(UserArguments.Parse([])));
        Assert.False(SmokeSession.IsRequested(UserArguments.Parse([TestExit.PressFlag, TestExit.EscapeName, "1"])));
    }

    /// <summary>Every tick of the script has an intent for that tick, with no reserved bit (D-232).</summary>
    [Fact]
    public void ScriptCoversEveryTickWithNoReservedBit()
    {
        for (uint tick = 0; tick < SmokeSession.Ticks; tick++)
        {
            Intent intent = SmokeSession.IntentAt(tick);
            Assert.Equal(tick, intent.Tick);
            Assert.Equal(0, intent.Buttons & Button.ReservedMask);
        }
    }

    /// <summary>The four parts: a walk, a walk with a turn, a sprint with a jump, and a strafe with a look up.</summary>
    [Fact]
    public void ScriptHasFourParts()
    {
        Intent walk = SmokeSession.IntentAt(0);
        Assert.Equal(SmokeSession.FullMove, walk.MoveY);
        Assert.Equal((short)0, walk.YawDelta);

        Intent turn = SmokeSession.IntentAt(SmokeSession.PartTicks);
        Assert.Equal(SmokeSession.FullMove, turn.MoveY);
        Assert.Equal(SmokeSession.TurnRate, turn.YawDelta);

        Intent sprint = SmokeSession.IntentAt(2 * SmokeSession.PartTicks);
        Assert.Equal(SmokeSession.FullMove, sprint.MoveY);
        Assert.Equal(Button.Sprint | Button.Jump, sprint.Buttons);

        Intent strafe = SmokeSession.IntentAt(3 * SmokeSession.PartTicks);
        Assert.Equal(SmokeSession.FullMove, strafe.MoveX);
        Assert.Equal(SmokeSession.LookUpRate, strafe.PitchDelta);
    }

    /// <summary>A tick past the script is an error, and never an intent of the last part (T-2).</summary>
    [Fact]
    public void ScriptRejectsATickPastTheEnd()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => SmokeSession.IntentAt(SmokeSession.Ticks));
    }

    /// <summary>The script runs on the loop of the first seed with no error, and the body moves from its spawn.</summary>
    [Fact]
    public void ScriptRunsOnTheLoopAndMovesTheBody()
    {
        SimulationLoop loop = new(Main.FirstSeed, TestWorld.Content);
        CoreVector3 spawn = loop.Body.Position;

        for (uint tick = 0; tick < SmokeSession.Ticks; tick++)
        {
            loop.Step(SmokeSession.IntentAt(tick));
        }

        Assert.Equal(SmokeSession.Ticks, loop.Tick);
        Assert.NotEqual(spawn, loop.Body.Position);
        Assert.False(loop.Ended);
    }

    /// <summary>
    /// PR-12 exit test 4. The engine boots headless, plays the script, and quits with exit code 0, with no
    /// error line in the log and no engine error. The end line carries the last tick.
    /// </summary>
    [Fact]
    [Trait("Category", SmokeCategory)]
    public async Task SmokeSessionPasses()
    {
        EngineRun run = await RunEngine("smoke session", ["--headless", "--fixed-fps", "60"], [SmokeSession.Flag]);
        string[] lines = run.Output.Split('\n');

        Assert.True(run.ExitCode == Main.ExitSuccess, $"The smoke session ended with exit code {run.ExitCode}.{Environment.NewLine}{run.Output}");
        Assert.DoesNotContain(lines, line => line.StartsWith(PrintLogSink.ErrorPrefix, StringComparison.Ordinal));
        Assert.DoesNotContain(lines, line => line.Contains("ERROR:", StringComparison.Ordinal));
        Assert.Contains(lines, line => line.Contains($"\"message\":\"{Main.StartMessage}\"", StringComparison.Ordinal) && line.Contains("\"tick\":0,", StringComparison.Ordinal));
        Assert.Contains(lines, line => line.Contains($"\"message\":\"{Main.EndMessage}\"", StringComparison.Ordinal) && line.Contains($"\"tick\":{SmokeSession.Ticks},", StringComparison.Ordinal));
    }

    /// <summary>
    /// PR-60 exit test 2. The engine boots headless, the press flag gives it the Escape key at a tick of the smoke
    /// script, and the session ends with the test exit line and exit code 0 before the script ends (D-311).
    /// </summary>
    [Fact]
    [Trait("Category", SmokeCategory)]
    public async Task EscapeEndsTheSession()
    {
        await PressEndsTheSession(TestExit.EscapeName);
    }

    /// <summary>PR-60 exit test 3. The same with the Start button of the first controller (D-311).</summary>
    [Fact]
    [Trait("Category", SmokeCategory)]
    public async Task StartButtonEndsTheSession()
    {
        await PressEndsTheSession(TestExit.StartName);
    }

    /// <summary>
    /// PR-61 exit test 6. The engine boots headless with a word that no flag takes, and the boot ends with exit code 1 and
    /// an error line that names the word, before the run starts (D-313, T-2).
    /// </summary>
    [Fact]
    [Trait("Category", SmokeCategory)]
    public async Task BadArgumentEndsTheSession()
    {
        EngineRun run = await RunEngine("bad argument session", ["--headless", "--fixed-fps", "60"], [SmokeSession.Flag, "unexpected"]);
        string[] lines = run.Output.Split('\n');

        Assert.True(run.ExitCode == Main.ExitFailure, $"The bad argument session ended with exit code {run.ExitCode}.{Environment.NewLine}{run.Output}");
        Assert.Contains(lines, line => line.StartsWith(PrintLogSink.ErrorPrefix, StringComparison.Ordinal)
            && line.Contains(Main.BootFailedMessage, StringComparison.Ordinal)
            && line.Contains(UserArguments.UnknownWordMessage, StringComparison.Ordinal)
            && line.Contains("unexpected", StringComparison.Ordinal));
        Assert.DoesNotContain(lines, line => line.Contains($"\"message\":\"{Main.StartMessage}\"", StringComparison.Ordinal));
        Assert.DoesNotContain(lines, line => line.Contains($"\"message\":\"{Main.EndMessage}\"", StringComparison.Ordinal));
    }

    /// <summary>
    /// Runs the smoke session with one scripted press at <see cref="PressTick"/>, and asserts the test exit line at a
    /// tick past the press and before the end of the script, exit code 0, no error line, and no smoke end line.
    /// </summary>
    private static async Task PressEndsTheSession(string input)
    {
        EngineRun run = await RunEngine(
            $"{input} press session",
            ["--headless", "--fixed-fps", "60"],
            [SmokeSession.Flag, TestExit.PressFlag, input, PressTick.ToString(CultureInfo.InvariantCulture)]);
        string[] lines = run.Output.Split('\n');

        Assert.True(run.ExitCode == Main.ExitSuccess, $"The {input} press session ended with exit code {run.ExitCode}.{Environment.NewLine}{run.Output}");
        Assert.DoesNotContain(lines, line => line.StartsWith(PrintLogSink.ErrorPrefix, StringComparison.Ordinal));
        Assert.DoesNotContain(lines, line => line.Contains("ERROR:", StringComparison.Ordinal));
        Assert.DoesNotContain(lines, line => line.Contains($"\"message\":\"{Main.EndMessage}\"", StringComparison.Ordinal));

        string endLine = Assert.Single(lines, line => line.Contains($"\"message\":\"{Main.TestExitMessage}\"", StringComparison.Ordinal));
        uint endTick = EndTick(endLine);
        Assert.InRange(endTick, PressTick + 1, SmokeSession.Ticks - 1);
    }

    /// <summary>The tick field of one log line.</summary>
    private static uint EndTick(string line)
    {
        const string field = "\"tick\":";
        int start = line.IndexOf(field, StringComparison.Ordinal) + field.Length;
        int end = line.IndexOf(',', start);
        return uint.Parse(line[start..end], CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// The contact sheet on the headless display is an error line and exit code 1, and never a blank sheet (D-306,
    /// T-2). The engine boots with the flag, loads the atlas, finds no window, and writes no file.
    /// </summary>
    [Fact]
    [Trait("Category", SmokeCategory)]
    public async Task ContactSheetFailsHeadless()
    {
        string sheet = Path.Combine(Path.GetTempPath(), "wyc-contact-sheet-" + Guid.NewGuid().ToString("N") + ".png");

        EngineRun run = await RunEngine("contact sheet", ["--headless"], [WhatYouCarry.Game.Review.ContactSheet.Flag, sheet]);
        string[] lines = run.Output.Split('\n');

        Assert.True(run.ExitCode == Main.ExitFailure, $"The contact sheet ended with exit code {run.ExitCode} on the headless display.{Environment.NewLine}{run.Output}");
        Assert.Contains(lines, line => line.StartsWith(PrintLogSink.ErrorPrefix, StringComparison.Ordinal)
            && line.Contains(Main.ContactSheetFailedMessage, StringComparison.Ordinal)
            && line.Contains(Main.ContactSheetNeedsWindow, StringComparison.Ordinal));
        Assert.False(File.Exists(sheet), $"The headless contact sheet wrote '{sheet}'.");
    }

    /// <summary>
    /// Runs the engine on the Game project with its own arguments and the user arguments after the separator, and
    /// waits for the end. An engine that never quits is a failure with its output, and never a test that hangs (T-2).
    /// </summary>
    private static async Task<EngineRun> RunEngine(string what, string[] engineArguments, string[] userArguments)
    {
        string godot = GodotExecutable();
        string root = RepositoryRoot.Find();
        ProcessStartInfo start = new(godot)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            WorkingDirectory = root,
        };
        foreach (string argument in engineArguments)
        {
            start.ArgumentList.Add(argument);
        }

        start.ArgumentList.Add("--path");
        start.ArgumentList.Add(Path.Combine(root, "WhatYouCarry.Game"));
        start.ArgumentList.Add("--");
        foreach (string argument in userArguments)
        {
            start.ArgumentList.Add(argument);
        }

        using Process process = Process.Start(start) ?? throw new InvalidOperationException($"The engine at '{godot}' did not start.");
        Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
        Task<string> errorTask = process.StandardError.ReadToEndAsync();
        using CancellationTokenSource timeout = new(TimeoutMilliseconds);
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync();
            Assert.Fail($"The {what} did not end inside {TimeoutMilliseconds} ms.{Environment.NewLine}{await outputTask}{await errorTask}");
        }

        return new EngineRun(process.ExitCode, await outputTask + await errorTask);
    }

    /// <summary>The exit code of one engine run, and its standard output followed by its standard error.</summary>
    private sealed record EngineRun(int ExitCode, string Output);

    /// <summary>The Godot executable: the variable when it is set, or the local build of the Mac. Neither one absent is a skip.</summary>
    private static string GodotExecutable()
    {
        string? fromVariable = Environment.GetEnvironmentVariable(GodotVariable);
        if (fromVariable is not null && fromVariable.Length > 0)
        {
            Assert.True(File.Exists(fromVariable), $"The variable {GodotVariable} names '{fromVariable}', and no file exists there.");
            return fromVariable;
        }

        Assert.True(File.Exists(LocalGodot), $"No Godot executable. Set {GodotVariable} to one, or install the build at '{LocalGodot}'.");
        return LocalGodot;
    }
}
