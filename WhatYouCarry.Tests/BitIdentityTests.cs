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
    /// the version bump that goes with it.
    /// </remarks>
    private const string ExpectedHash = "283aa4b8cd1281be";

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
    }
}
