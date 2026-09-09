using System;
using System.IO;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Tools;
using WhatYouCarry.Tools.BitIdentity;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The bit-identity sweep and its command (D-69, D-71, D-201; PR-3 exit test 8).</summary>
public sealed class BitIdentityTests
{
    /// <summary>
    /// The hash of the sweep. The `bit-identity` CI job compares this number across Linux x64, Windows x64, and
    /// macOS arm64. This test pins it, so a change to a Core number fails here first, on one platform, with a
    /// clear name, instead of failing later as a disagreement between three CI legs.
    /// </summary>
    /// <remarks>
    /// A deliberate change to the simulation changes this number. G-20 asks the review to confirm the change and
    /// the version bump that goes with it. PR-6 set `283aa4b8cd1281be`, PR-7 moved it to `e8ef2b1fad938845` when
    /// the state gained a position, and PR-8 moved it to `92ef27ee175b3e7e` when the pitch clamp changed and the
    /// sweep gained the camera. The PR #23 review moved it again, because the sweep folds the camera from the
    /// replay observer now and not from a second live loop (F-85).
    /// </remarks>
    private const string ExpectedHash = "afed0063a6cf8a50";

    /// <summary>The sweep gives the recorded hash on this platform.</summary>
    [Fact]
    public void BitIdentityKnownAnswer()
    {
        Assert.Equal(ExpectedHash, BitIdentitySweep.Run().ToString());
    }

    /// <summary>The sweep is pure: two runs in one process give one hash.</summary>
    [Fact]
    public void TheSweepRepeats()
    {
        StateHash first = BitIdentitySweep.Run();
        StateHash second = BitIdentitySweep.Run();
        Assert.Equal(first, second);
    }

    /// <summary>The command prints the hash alone on standard output, so the CI job can compare it.</summary>
    [Fact]
    public void TheCommandPrintsTheHashAlone()
    {
        TextWriter savedOut = Console.Out;
        TextWriter savedError = Console.Error;
        try
        {
            StringWriter output = new();
            StringWriter errors = new();
            Console.SetOut(output);
            Console.SetError(errors);

            int exitCode = Program.Main(["bit-identity"]);

            Assert.Equal(0, exitCode);
            Assert.Equal(ExpectedHash, output.ToString().Trim());

            // The context belongs on standard error, and it must name the seed.
            Assert.Contains("bit-identity", errors.ToString(), StringComparison.Ordinal);
            Assert.Contains("5eed1234abcd9876", errors.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            Console.SetOut(savedOut);
            Console.SetError(savedError);
        }
    }

    /// <summary>The command takes no option, and it names any argument that it gets (T-2).</summary>
    [Fact]
    public void TheCommandTakesNoOption()
    {
        TextWriter savedError = Console.Error;
        try
        {
            StringWriter errors = new();
            Console.SetError(errors);

            Assert.Equal(2, Program.Main(["bit-identity", "--root"]));
            Assert.Contains("--root", errors.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            Console.SetError(savedError);
        }
    }

    /// <summary>
    /// The sweep reads every DetMath function and every declared stream. A function that no sweep reads would
    /// not take part in the three-platform comparison.
    /// </summary>
    [Fact]
    public void TheSweepReadsEveryFunctionAndStream()
    {
        string sweep = RepositoryRoot.ReadFile("WhatYouCarry.Tools/BitIdentity/BitIdentitySweep.cs");
        string[] functions = ["Sin", "Cos", "Atan2", "Sqrt", "Pow", "Floor", "Clamp", "Lerp"];
        foreach (string function in functions)
        {
            Assert.Contains($"DetMath.{function}(", sweep, StringComparison.Ordinal);
        }

        foreach (RngStream stream in Enum.GetValues<RngStream>())
        {
            Assert.Contains($"RngStream.{stream}", sweep, StringComparison.Ordinal);
        }

        // PR-6 exit test 7: the sweep replays one fixed record, so the three platforms compare the replay too.
        Assert.Contains("RunReplayer.Replay(", sweep, StringComparison.Ordinal);
        Assert.Contains("Crc32.Of(", sweep, StringComparison.Ordinal);

        // PR-8 exit test 5: the sweep folds in the camera pose and the aim ray of every replayed tick, through
        // the replay observer, and it builds no live loop of its own beside the replay (PR #23 review P2-1).
        Assert.Contains("loop.Camera()", sweep, StringComparison.Ordinal);
        Assert.Contains("loop.Aim(", sweep, StringComparison.Ordinal);
        Assert.Contains(": IReplayObserver", sweep, StringComparison.Ordinal);
        Assert.DoesNotContain("new SimulationLoop(", sweep, StringComparison.Ordinal);
    }
}
