using System;
using System.IO;
using WhatYouCarry.Core.Bots;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Procgen;
using WhatYouCarry.Core.Replay;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Core.World;
using WhatYouCarry.Tools;
using WhatYouCarry.Tools.BitIdentity;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The bit-identity sweep and its command (D-69, D-71, D-201; PR-3 exit test 8).</summary>
[Collection(ConsoleCollection.Name)]
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
    /// sweep gained the camera. The PR #23 review moved it to `afed0063a6cf8a50`, because the sweep folds the
    /// camera from the replay observer and not from a second live loop (F-85). PR-9 moved it again when the
    /// state gained the floor number, the sweep gained the floor generator, and the replay moved to a dug floor.
    /// PR-59 moved it from `036df5c08e2682e3` when the detail pass changed every dug floor (D-260). PR-10 moved
    /// it from `62c5e1d152fe94fe` when the state gained the projectiles and the sweep intents began to fire, and
    /// the PR #31 review moved it again when the spread draw became one angle and one roll inside the cone (F-87).
    /// PR-11 moved it from `d8943df12fefcbee` when the sweep gained the Bot stream (D-272). PR-15 moved it from
    /// `6ec00e90c1c85cdb` when the state gained the player and the run end kind, the sweep intents began to swing and roll,
    /// and the sweep gained a projectile run and the arc test (D-320, D-322, D-325). PR-63 moved it from `2258ba8b9cc94b3f`
    /// when the sweep floors took the dig sizes of D-341 and the simulation version rose to 8 (D-342, G-20). PR-67 moved it
    /// from `b00814dbf25e61e8` when the simulation version rose to 9 (D-359, G-20). The sweep floors dig inside the job
    /// budget and stay the same, and the replay header holds the version, so only the version moved the hash. PR-64 moved it
    /// from `efcce6816cec980e` when the simulation version rose to 10 and the sweep gained the ramp courses (D-345, D-367,
    /// G-20). The ramp collision with the version at 9 and no ramp run gave the old answer, so no grid without a ramp moved.
    /// PR-68 moved it from `24c37100cd99edf4` when the simulation version rose to 11, because the pillar rule of F-101
    /// moves every floor that holds a pillar in the column of a shaft (D-260, G-20). PR-66 moved it from
    /// `a0b32bad006b3dfe` when the simulation version rose to 12, the sweep floors took the ramps of D-347 and the
    /// chamber tiers of D-348, and the sweep gained the count of the ramps of each floor (D-260, G-20). PR-16 moved
    /// it from `15904316a1b4ec07` when the simulation version rose to 13, the state gained the enemies and their
    /// brains, the sweep content gained one enemy family with a second weapon for it, and the sweep gained the
    /// enemy spawns of each floor (D-395 to D-405, G-20). PR-17 moved it from `d701dca6d5cee4d8` when the simulation
    /// version rose to 14, the state gained the floor timer, the Overseer, and the waves, and the sweep content gained
    /// a timer of 3 seconds, waves each 2 seconds, and a hunter with a pick of one damage, so the replay runs past
    /// expiry (D-407 to D-425, G-20). PR-72 moved it from `6f7da3d2313688bd` when the simulation version rose to 15,
    /// the path search took the diagonal move of D-486 and D-489, and the jump rule read the slope under the feet
    /// (F-107, F-108, G-20). The new walk with the version at 14 gave `5edea237bc4e2fae`, so the walks of the sweep
    /// moved the hash as well as the version. PR-81 moved it from `dc4258105649a548` when the simulation version rose
    /// to 16, a diagonal drop needed an open fall in the corner column, a waypoint arrival started the wedge count
    /// again, and the sweep read the box of the caller (D-545, D-546, D-549, F-111, F-112, G-20). The new rules with
    /// the version at 15 gave `dc4258105649a548`, so the version alone moved the hash. PR-88 moved it from
    /// `a2e1c2c6f72bc19e` when the simulation version rose to 17, a death on the tick of a stairwell press stayed a
    /// death, and a descend on the deepest floor did nothing (D-322, D-579, G-20). The sweep masks the stairwell bits,
    /// and the new rules with the version at 16 gave `a2e1c2c6f72bc19e`, so the version alone moved the hash. The
    /// same PR then moved it from `241070d5189efb3c` when the state gained the stored path and the wedge count of
    /// each path follower (D-160, F-123). The path alone gave `d8eb1260bd5aa55e`. The same PR then moved it from
    /// `f1c35ddccb2cd0bb` when the sweep gained one floor of the content of the checkout and two stairwell records
    /// on it: a descent to the offered floor 2 and a death there, and an ascend at floor 1 (F-133). The simulation
    /// version stayed at 17, because no Core number changed. The earlier parts of the sweep fold first and are the
    /// same, and the Debug and the Release builds give the new answer.
    /// </remarks>
    private const string ExpectedHash = "9c79047da9c82a0e";

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
        // the replay observer (PR #23 review P2-1). The one live loop plays a bot policy to write the intents of a
        // stairwell record, and the sweep folds the replay of that record and never the live states (F-133).
        Assert.Contains("loop.Camera()", sweep, StringComparison.Ordinal);
        Assert.Contains("loop.Aim(", sweep, StringComparison.Ordinal);
        Assert.Contains(": IReplayObserver", sweep, StringComparison.Ordinal);
        int liveLoops = sweep.Split("new(seed, content)").Length - 1;
        Assert.Equal(1, liveLoops);
        Assert.Contains("SimulationLoop live = new(seed, content);", sweep, StringComparison.Ordinal);
        Assert.DoesNotContain("new SimulationLoop(", sweep, StringComparison.Ordinal);

        // PR-9 exit test 7: the sweep digs floors and folds every block, so the three platforms compare the generator.
        Assert.Contains("FloorGenerator.Generate(", sweep, StringComparison.Ordinal);
        Assert.Contains("plan.Grid.Get(", sweep, StringComparison.Ordinal);

        // PR-10 exit test 5, as D-320 revises it: no intent fires a shot, so the sweep fires its projectile definition in a run of its own.
        Assert.Contains("new(\"sweep-shot\"", sweep, StringComparison.Ordinal);
        Assert.Contains("AddProjectileRun(", sweep, StringComparison.Ordinal);

        // PR-15 exit test 6: the sweep content holds a sword, so the sweep intents swing and roll in the replay, and the sweep folds the arc test.
        Assert.Contains("new(SimulationLoop.MainWeaponId, 0,", sweep, StringComparison.Ordinal);
        Assert.Contains("MeleeWeapon.WedgeHits(", sweep, StringComparison.Ordinal);

        // PR-17 exit test 7: the sweep content holds a hunter and a timer of 3 seconds, so the replay runs past expiry.
        Assert.Contains("HunterDefinition hunter = new(\"sweep-overseer\"", sweep, StringComparison.Ordinal);
        Assert.Contains("[2, 3, 4], 3, 2, 2, 12)", sweep, StringComparison.Ordinal);

        // PR-59 exit test 4: the sweep content names every band, so the folded floors hold every block of the detail pass.
        Assert.Contains("DetailPass.WorkingMine", sweep, StringComparison.Ordinal);
        Assert.Contains("DetailPass.OlderWorkings", sweep, StringComparison.Ordinal);
        Assert.Contains("DetailPass.Deep", sweep, StringComparison.Ordinal);

        // PR-64 exit test 6: no dug floor holds a ramp before PR-66, so the sweep builds ramp courses and walks, rolls, and marches over them.
        Assert.Contains("AddRampRun(", sweep, StringComparison.Ordinal);
        Assert.Contains("new Ramp(", sweep, StringComparison.Ordinal);

        // F-133: one floor of the content of the checkout, a descend record with a death, and an ascend record, on
        // that content, with the next floor from the worker.
        Assert.Contains("AddFloor(ref hash, FloorGenerator.Generate(DescendSeed, SimulationLoop.FirstFloor, repository));", sweep, StringComparison.Ordinal);
        Assert.Contains("RecordStairwellRun(new GreedyDescender(repository), DescendSeed, repository), repository, RunEnd.Death,", sweep, StringComparison.Ordinal);
        Assert.Contains("RecordStairwellRun(new Coward(), AscendSeed, repository), repository, RunEnd.Ascend,", sweep, StringComparison.Ordinal);
        Assert.Contains("live.OfferNextFloor(", sweep, StringComparison.Ordinal);
    }

    /// <summary>
    /// F-133. On the content of the checkout, the descend record takes the offered floor 2 at the stairwell of floor 1
    /// and dies there, and the ascend record ends as an ascend at the stairwell of floor 1. Each replay gives the
    /// end state of its live loop, which took the worker floor, so the dig at the descent and the offered floor meet.
    /// </summary>
    [Fact]
    public void TheStairwellRecordsDescendDieAndAscend()
    {
        ContentSet content = BitIdentitySweep.RepositoryContent();
        VoxelGrid folded = FloorGenerator.Generate(BitIdentitySweep.DescendSeed, SimulationLoop.FirstFloor, content).Grid;
        Assert.Equal(64, folded.SizeX);
        Assert.Equal(20, folded.SizeY);
        Assert.Equal(64, folded.SizeZ);

        StairwellRecord descend = BitIdentitySweep.RecordStairwellRun(new GreedyDescender(content), BitIdentitySweep.DescendSeed, content);
        Assert.Equal(RunEnd.Death, descend.Live.End);
        Assert.Equal(SimulationLoop.FirstFloor + 1, descend.Live.Floor);
        Assert.Equal(2, descend.Offers);
        Assert.NotEqual(string.Empty, descend.Live.DeathCause);
        Assert.True(descend.Live.Enemies.Count > 0, "The descent placed no enemy on floor 2.");
        ReplayResult descendReplay = RunReplayer.Replay(descend.Bytes, content, new JsonlLogger(new ThrowingSink()));
        Assert.Equal(descend.Live.Hash(), descendReplay.Loop.Hash());
        Assert.Equal(RunEnd.Death, descendReplay.Loop.End);

        StairwellRecord ascend = BitIdentitySweep.RecordStairwellRun(new Coward(), BitIdentitySweep.AscendSeed, content);
        Assert.Equal(RunEnd.Ascend, ascend.Live.End);
        Assert.Equal(SimulationLoop.FirstFloor, ascend.Live.Floor);
        Assert.Equal(1, ascend.Offers);
        ReplayResult ascendReplay = RunReplayer.Replay(ascend.Bytes, content, new JsonlLogger(new ThrowingSink()));
        Assert.Equal(ascend.Live.Hash(), ascendReplay.Loop.Hash());
        Assert.Equal(RunEnd.Ascend, ascendReplay.Loop.End);
    }

    /// <summary>
    /// F-133, the boundary. A policy whose run does not end inside the tick limit stops the record with an error that
    /// names the policy and the seed, and never gives a short record that the sweep folds in silence (T-2).
    /// </summary>
    [Fact]
    public void ARecordThatDoesNotEndStopsTheSweep()
    {
        // On this seed no enemy reaches the spawn inside the tick limit, so a player that stands there lives on.
        const ulong QuietSeed = 1;
        ContentSet content = BitIdentitySweep.RepositoryContent();
        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() => BitIdentitySweep.RecordStairwellRun(new StandStill(), QuietSeed, content));
        Assert.Contains(StandStill.PolicyName, error.Message, StringComparison.Ordinal);
        Assert.Contains($"seed {QuietSeed}", error.Message, StringComparison.Ordinal);
        Assert.Contains($"{BitIdentitySweep.StairwellTickLimit} ticks", error.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// F-133. Each platform job of the workflow builds and runs the sweep in Release too, and fails when the Release
    /// hash is not the Debug hash, before it passes its hash up to the compare job.
    /// </summary>
    [Fact]
    public void EachPlatformJobChecksTheReleaseSweep()
    {
        string workflow = RepositoryRoot.ReadFile(".github/workflows/bit-identity.yml");
        foreach (string job in new[] { "linux-x64", "windows-x64", "macos-arm64" })
        {
            string text = WorkflowText.JobText(workflow, job);
            Assert.Contains("run: dotnet build WhatYouCarry.Tools/WhatYouCarry.Tools.csproj -c Release\n", text, StringComparison.Ordinal);
            Assert.Contains("release_hash=\"$(dotnet run --project WhatYouCarry.Tools/WhatYouCarry.Tools.csproj -c Release --no-build -- bit-identity)\"", text, StringComparison.Ordinal);
            int check = text.IndexOf("[ \"${hash}\" != \"${release_hash}\" ]", StringComparison.Ordinal);
            int output = text.IndexOf("echo \"hash=${hash}\" >> \"$GITHUB_OUTPUT\"", StringComparison.Ordinal);
            Assert.True(check > 0, $"The job '{job}' does not compare the Release hash with the Debug hash.");
            Assert.True(output > check, $"The job '{job}' passes its hash up before the Release check.");
        }
    }

    /// <summary>A policy that stands at the spawn and presses nothing. Its run does not end inside the tick limit of a record.</summary>
    private sealed class StandStill : IBotPolicy
    {
        public const string PolicyName = "stand-still";

        public string Name => PolicyName;

        public bool PromisesProgress => false;

        public Intent Next(SimulationLoop loop)
        {
            return new Intent(loop.Tick, 0, 0, 0, 0, 0);
        }
    }

    /// <summary>A replay of a whole record writes no line, so a line here fails the test (T-2).</summary>
    private sealed class ThrowingSink : ILogSink
    {
        public void Write(string line)
        {
            throw new InvalidOperationException($"The replay of a whole record wrote a line: {line}");
        }
    }
}
