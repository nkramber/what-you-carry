using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using WhatYouCarry.Core.Bots;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Procgen;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Tools;
using WhatYouCarry.Tools.BotRunner;
using WhatYouCarry.Tools.NightGate;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The bot policies, the run, the runner command, and the night record (D-115, D-127, D-270 to D-273; PR-11 exit tests 1 to 4 and 7).</summary>
[Collection(ConsoleCollection.Name)]
[Trait("Category", SweepScope.SweepCategory)]
public sealed class BotTests
{
    /// <summary>A fixture policy that stands still and promises progress, so its run reads softlock.</summary>
    private sealed class StandStill : IBotPolicy
    {
        public string Name => "stand-still";

        public bool PromisesProgress => true;

        public Intent Next(SimulationLoop loop)
        {
            return new Intent(loop.Tick, 0, 0, 0, 0, 0);
        }
    }

    /// <summary>A fixture policy that throws on its tenth tick.</summary>
    private sealed class Thrower : IBotPolicy
    {
        public string Name => "thrower";

        public bool PromisesProgress => true;

        public Intent Next(SimulationLoop loop)
        {
            if (loop.Tick == 10)
            {
                ContextException error = new("The fixture throws on purpose.");
                error.AddContext("fixture", "thrower");
                throw error;
            }

            return new Intent(loop.Tick, 0, 0, 0, 0, 0);
        }
    }

    private sealed class CollectingSink : ILogSink
    {
        public List<string> Lines { get; } = [];

        public void Write(string line)
        {
            this.Lines.Add(line);
        }
    }

    private static string TempDirectory(string name)
    {
        string path = Path.Combine(Path.GetTempPath(), $"wyc-bots-{name}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }

    /// <summary>
    /// A timer tester run logs the expiry, the spawn of the Overseer, and each wave between its start and its end, and
    /// the end line of a death names its cause (D-411, D-421, M-5).
    /// </summary>
    [Fact]
    public void TheRunLogHoldsTheTimerEventsAndTheCause()
    {
        string output = TempDirectory("timer");
        try
        {
            Assert.Equal(0, Program.Main(["bot-run", "--policy", TimerTester.PolicyName, "--seeds", "1-1", "--output", output, "--root", RepositoryRoot.Find()]));
            string[] lines = File.ReadAllLines(Path.Combine(output, "timer-tester-1.jsonl"));
            List<string> events = [];
            for (int index = 1; index < lines.Length - 1; index++)
            {
                using JsonDocument line = JsonDocument.Parse(lines[index]);
                events.Add(line.RootElement.GetProperty(BotRunCommand.EventName).GetString()!);
            }

            Assert.Equal(["expiry", "hunter-spawn"], events.GetRange(0, 2));
            using JsonDocument end = JsonDocument.Parse(lines[^1]);
            Assert.Equal("death", end.RootElement.GetProperty(BotRunCommand.EndStateName).GetString());
            string cause = end.RootElement.GetProperty(BotRunCommand.CauseName).GetString()!;
            Assert.True(cause == TestWorld.Content.Hunter.Id || cause == TestWorld.Content.Enemies[0].Id, $"The cause is '{cause}'.");
            Assert.IsType<TimerTester>(BotRunCommand.CreatePolicy(TimerTester.PolicyName, 1UL, TestWorld.Content));
        }
        finally
        {
            Directory.Delete(output, recursive: true);
        }
    }

    /// <summary>
    /// PR-11 exit test 1. The runner writes one log per run, and every log holds the policy, the seed, the end
    /// state, the floors reached, and the ticks, beside the required run fields of D-113.
    /// </summary>
    [Fact]
    public void RunLogHasRequiredFields()
    {
        string output = TempDirectory("logs");
        try
        {
            int walker = Program.Main(["bot-run", "--policy", RandomWalker.PolicyName, "--seeds", "1-2", "--output", output, "--root", RepositoryRoot.Find()]);
            int descender = Program.Main(["bot-run", "--policy", GreedyDescender.PolicyName, "--seeds", "3-3", "--output", output, "--root", RepositoryRoot.Find()]);
            Assert.Equal(0, walker);
            Assert.Equal(0, descender);

            string[] logs = Directory.GetFiles(output, "*.jsonl");
            Assert.Equal(3, logs.Length);
            foreach (string log in logs)
            {
                // A run past expiry writes a timer event line for each event between the start and the end (M-5).
                string[] lines = File.ReadAllLines(log);
                Assert.True(lines.Length >= 2, $"{Path.GetFileName(log)} holds {lines.Length} lines.");
                using JsonDocument end = JsonDocument.Parse(lines[^1]);
                JsonElement root = end.RootElement;
                foreach (string field in new[] { "seed", "floor", "tick", "subsystem", "entities", BotRunCommand.PolicyName, BotRunCommand.EndStateName, BotRunCommand.FloorsReachedName })
                {
                    Assert.True(root.TryGetProperty(field, out _), $"{Path.GetFileName(log)}: the end line lacks '{field}'.");
                }

                Assert.Equal(BotRunCommand.Subsystem, root.GetProperty("subsystem").GetString());
                string state = root.GetProperty(BotRunCommand.EndStateName).GetString()!;
                string policy = root.GetProperty(BotRunCommand.PolicyName).GetString()!;
                // The walker ends by its budget or by a death, and the descender at the bottom or by a death. A
                // fight can kill either one, and a death is its own end state (D-403).
                string[] ends = policy == RandomWalker.PolicyName ? ["budget", "death"] : ["bottom", "death"];
                Assert.Contains(state, ends);
                Assert.True(root.GetProperty("tick").GetInt64() > 0);
            }
        }
        finally
        {
            Directory.Delete(output, recursive: true);
        }
    }

    /// <summary>PR-11 exit test 2. A policy that promises progress and makes none ends as a softlock when the floor timer expires (D-270, D-420).</summary>
    [Fact]
    public void SoftlockIsDetected()
    {
        // The floor holds no enemy, so the run reads the softlock rule alone and never a death (D-403).
        BotRunResult result = BotRun.Play(new StandStill(), 5UL, TestWorld.PeacefulContent);
        Assert.Equal(BotRunEnd.Softlock, result.End);
        Assert.Equal(FloorTimer.For(FloorGenerator.TemplateFor(1, TestWorld.PeacefulContent), 1).Length, result.Ticks);
        Assert.Equal(1, result.FloorsReached);
        Assert.Equal("stand-still", result.Policy);
        Assert.Equal(string.Empty, result.Error);
    }

    /// <summary>PR-11 exit test 3. A policy that throws ends the run as a crash, the result and the log hold the exception with its context, and the runner goes on (D-270).</summary>
    [Fact]
    public void CrashIsLogged()
    {
        BotRunResult result = BotRun.Play(new Thrower(), 6UL, TestWorld.Content);
        Assert.Equal(BotRunEnd.Crash, result.End);
        Assert.Equal(10U, result.Ticks);
        Assert.Contains("throws on purpose", result.Error, StringComparison.Ordinal);
        Assert.Contains("fixture=thrower", result.Error, StringComparison.Ordinal);

        CollectingSink sink = new();
        BotRunCommand.WriteLog(result, new JsonlLogger(sink));
        Assert.Equal(2, sink.Lines.Count);
        using JsonDocument end = JsonDocument.Parse(sink.Lines[1]);
        Assert.Equal("crash", end.RootElement.GetProperty(BotRunCommand.EndStateName).GetString());
        Assert.Equal("error", end.RootElement.GetProperty("level").GetString());
        Assert.Contains("throws on purpose", end.RootElement.GetProperty(BotRunCommand.ErrorName).GetString(), StringComparison.Ordinal);
    }

    /// <summary>
    /// PR-11 exit test 4 and PR-16 exit test 7. Over one hundred seeds, twenty on a pull request (D-480), every
    /// run of the greedy descender and of the full clearer ends at the bottom or by a death, and never by a crash
    /// and never by a softlock. A run at the bottom reached fifteen floors. A failure names its seed (D-66, D-403).
    /// </summary>
    /// <remarks>
    /// Before PR-16 nothing could kill the player, and the descender reached the bottom on every seed. The
    /// scavengers of D-399 now kill most runs, and a death is an outcome of a fight and never a fault of the code
    /// (D-403). The test asserts that some runs still reach the bottom, so a floor that no bot can leave still
    /// fails it.
    /// </remarks>
    [Fact]
    public void EveryPolicyEndsAtTheBottomOrByDeath()
    {
        int bottoms = 0;
        int seeds = SweepScope.Seeds(100);
        for (ulong seed = 1; seed <= (ulong)seeds; seed++)
        {
            IBotPolicy[] policies = [new GreedyDescender(TestWorld.Content), new FullClearer(TestWorld.Content)];
            foreach (IBotPolicy policy in policies)
            {
                BotRunResult result = BotRun.Play(policy, seed, TestWorld.Content);
                Assert.True(
                    result.End == BotRunEnd.Bottom || result.End == BotRunEnd.Death,
                    $"Seed {seed}: the run of '{policy.Name}' ended as {result.End} on floor {result.FloorsReached} after {result.Ticks} ticks. {result.Error}");
                if (result.End == BotRunEnd.Bottom)
                {
                    Assert.Equal(15, result.FloorsReached);
                    bottoms++;
                }
            }
        }

        Assert.True(bottoms > 0, "No run of one hundred seeds reached the bottom, so no bot can leave a floor.");
    }

    /// <summary>
    /// The full clearer leaves in time (D-439). On seeds 2100 and 2109 the night of 2026-09-21 read a softlock: the
    /// clear ended near 5,750 ticks, and the walk to the stairwell wound past expiry. The runs now end at the bottom
    /// or by a death, and never as a softlock.
    /// </summary>
    [Theory]
    [InlineData(2100UL)]
    [InlineData(2109UL)]
    public void FullClearerLeavesInTime(ulong seed)
    {
        BotRunResult result = BotRun.Play(new FullClearer(TestWorld.Content), seed, TestWorld.Content);
        Assert.True(
            result.End == BotRunEnd.Bottom || result.End == BotRunEnd.Death,
            $"Seed {seed}: the full clearer ended as {result.End} on floor {result.FloorsReached} after {result.Ticks} ticks. {result.Error}");
    }

    /// <summary>
    /// The greedy descender leaves every floor of the night of 2026-09-23 (F-111, D-545, D-546). On 26 seeds that
    /// night read a softlock at `e069e16`. Seed 1268 wedged on a detour away from the stairwell, seed 947 also took a
    /// diagonal drop onto an overhang, and seeds 2669 and 2879 softlocked on that drop alone. The runs now reach the
    /// bottom.
    /// </summary>
    /// <remarks>
    /// The content holds no enemy family, so no death ends a run before the floor that softlocked. With enemies, the
    /// test took a death as a pass, so a run that died early proved nothing of the fix (F-124). With the fix reverted,
    /// each of the four seeds softlocks here: 947 on floor 1, 1268 on floor 2, 2879 on floor 3, and 2669 on floor 8.
    /// </remarks>
    [Theory]
    [InlineData(947UL)]
    [InlineData(1268UL)]
    [InlineData(2669UL)]
    [InlineData(2879UL)]
    public void GreedyDescenderLeavesTheFloorsOfTheNight(ulong seed)
    {
        BotRunResult result = BotRun.Play(new GreedyDescender(TestWorld.PeacefulContent), seed, TestWorld.PeacefulContent);
        Assert.True(
            result.End == BotRunEnd.Bottom,
            $"Seed {seed}: the greedy descender ended as {result.End} on floor {result.FloorsReached} after {result.Ticks} ticks, and a run with no enemy reaches the bottom. {result.Error}");
        Assert.Equal(15, result.FloorsReached);
    }

    /// <summary>
    /// Two seeds of the night need the enemies, so each run ends at the bottom or by a death, and never as a softlock or
    /// an error. Seed 940 wedged on a detour away from the stairwell (F-111, D-546). With no enemy it reaches the bottom
    /// also with the fix reverted, and with enemies it softlocks on floor 3 then, so it stays on this content, where it
    /// dies on floor 2 with the fix. Seed 4119 crashed in the local night of PR-81, when an enemy box ended one ulp
    /// inside a block (F-112, D-549).
    /// </summary>
    [Theory]
    [InlineData(940UL)]
    [InlineData(4119UL)]
    public void GreedyDescenderEndsTheEnemyRunsOfTheNight(ulong seed)
    {
        BotRunResult result = BotRun.Play(new GreedyDescender(TestWorld.Content), seed, TestWorld.Content);
        Assert.True(
            result.End == BotRunEnd.Bottom || result.End == BotRunEnd.Death,
            $"Seed {seed}: the greedy descender ended as {result.End} on floor {result.FloorsReached} after {result.Ticks} ticks. {result.Error}");
    }

    /// <summary>
    /// The clearer stops the hunt when the timer left is no more than the walk to the stairwell, and it hunts while
    /// the time is long (D-439). A floor with a short timer makes it leave at once.
    /// </summary>
    [Fact]
    public void FullClearerLeavesWhenTheTimeIsShort()
    {
        List<FloorTemplate> shortFloors = [];
        foreach (FloorTemplate floor in TestWorld.Content.Floors)
        {
            shortFloors.Add(floor with { TimerSeconds = 5, BossTimerSeconds = 0 });
        }

        ContentSet hurried = TestWorld.Content with { Floors = shortFloors };
        SimulationLoop quick = new(1, hurried);
        FullClearer leaver = new(hurried);
        quick.Step(leaver.Next(quick));
        Assert.True(leaver.IsLeaving, "The clearer hunts with five seconds on the timer.");

        SimulationLoop slow = new(1, TestWorld.Content);
        FullClearer hunter = new(TestWorld.Content);
        slow.Step(hunter.Next(slow));
        Assert.False(hunter.IsLeaving, "The clearer leaves with three minutes on the timer.");
    }

    /// <summary>PR-11 exit test 7. The night record holds the commit, the end time, and the status, and a bad commit or status is an error (D-273).</summary>
    [Fact]
    public void NightResultIsPublished()
    {
        string output = TempDirectory("night");
        string file = Path.Combine(output, "night.json");
        try
        {
            string commit = "0123456789abcdef0123456789abcdef01234567";
            DateTime before = DateTime.UtcNow.AddSeconds(-1);

            // A night that succeeds ends each sweep with no failure, and the record of main carries no seed (D-567).
            string failures = Path.Combine(output, "failures.txt");
            string carry = Path.Combine(output, "main.json");
            foreach (string sweep in NightSeeds.Sweeps)
            {
                File.AppendAllText(failures, NightSeeds.FailureLine(sweep, []));
            }

            File.WriteAllText(carry, NightRecordCommand.Build(commit, before, "success", string.Empty));
            string[] seedOptions = ["--date", "2026-09-25", "--failures", failures, "--carry", carry];
            Assert.Equal(2, Program.Main(["night-record", "--commit", commit, "--status", "success", "--output", file]));
            Assert.Equal(0, Program.Main(["night-record", "--commit", commit, "--status", "success", "--output", file, .. seedOptions]));

            // The file starts with the object, not a byte-order mark, so a reader that takes bytes sees JSON from the first byte.
            Assert.Equal((byte)'{', File.ReadAllBytes(file)[0]);
            using JsonDocument record = JsonDocument.Parse(File.ReadAllText(file));
            Assert.Equal(commit, record.RootElement.GetProperty(NightRecordCommand.CommitName).GetString());
            Assert.Equal("success", record.RootElement.GetProperty(NightRecordCommand.StatusName).GetString());
            DateTime endedAt = DateTime.Parse(record.RootElement.GetProperty(NightRecordCommand.EndedAtName).GetString()!, null, System.Globalization.DateTimeStyles.AdjustToUniversal);
            Assert.InRange(endedAt, before, DateTime.UtcNow.AddSeconds(1));

            Assert.Equal(2, Program.Main(["night-record", "--commit", "abc", "--status", "success", "--output", file, .. seedOptions]));
            Assert.Equal(2, Program.Main(["night-record", "--commit", commit, "--status", "green", "--output", file, .. seedOptions]));
            Assert.Equal("{\"commit\":\"" + commit + "\",\"endedAt\":\"2026-09-10T03:00:00Z\",\"status\":\"failure\",\"deaths\":{},\"deathCauses\":{},\"ascends\":{}}\n", NightRecordCommand.Build(commit, new DateTime(2026, 9, 10, 3, 0, 0, DateTimeKind.Utc), "failure", string.Empty));

            // The record carries the count of deaths of each policy, in the order of the summary lines (D-403).
            SortedDictionary<string, int> greedyCauses = new(StringComparer.Ordinal) { ["scavenger"] = 10, ["overseer"] = 2 };
            SortedDictionary<string, int> clearerCauses = new(StringComparer.Ordinal) { ["scavenger"] = 7 };
            string summary = BotRunCommand.DeathLine(GreedyDescender.PolicyName, 12, 0, greedyCauses) + BotRunCommand.DeathLine(FullClearer.PolicyName, 7, 0, clearerCauses);
            Assert.Equal("greedy-descender=12 ascends=0 overseer:2 scavenger:10\n", BotRunCommand.DeathLine(GreedyDescender.PolicyName, 12, 0, greedyCauses));
            Assert.Equal("{\"greedy-descender\":12,\"full-clearer\":7}", NightRecordCommand.DeathsObject(summary));

            // The record carries the count of ascends of each policy (D-430). A line of an older night holds no
            // ascend word, so its policy has no entry, and the record states no count that the night did not measure.
            string withCoward = summary + BotRunCommand.DeathLine(Coward.PolicyName, 3, 4997, new SortedDictionary<string, int>(StringComparer.Ordinal) { ["scavenger"] = 3 });
            Assert.Equal("{\"greedy-descender\":0,\"full-clearer\":0,\"coward\":4997}", NightRecordCommand.AscendsObject(withCoward));
            Assert.Equal("{\"coward\":{\"scavenger\":3}}", NightRecordCommand.CausesObject("coward=3 ascends=4997 scavenger:3\n"));
            Assert.Equal("{}", NightRecordCommand.AscendsObject("greedy-descender=3 overseer:1\n"));
            Assert.Throws<FormatException>(() => NightRecordCommand.AscendsObject("coward=3 ascends=many"));

            // The record carries the count of each cause for each policy, in ordinal cause order (D-411). A line of
            // an older night holds no cause word and reads as no cause (D-177).
            Assert.Equal("{\"greedy-descender\":{\"overseer\":2,\"scavenger\":10},\"full-clearer\":{\"scavenger\":7}}", NightRecordCommand.CausesObject(summary));
            Assert.Equal("{\"greedy-descender\":{}}", NightRecordCommand.CausesObject("greedy-descender=3\n"));
            Assert.Throws<FormatException>(() => NightRecordCommand.CausesObject("greedy-descender=3 overseer"));
            Assert.Throws<FormatException>(() => NightRecordCommand.CausesObject("greedy-descender=3 overseer:x"));
            Assert.Equal("{}", NightRecordCommand.DeathsObject("\n  \n"));
            Assert.Throws<FormatException>(() => NightRecordCommand.DeathsObject("greedy-descender=many"));
            Assert.Throws<FormatException>(() => NightRecordCommand.DeathsObject("=3"));
        }
        finally
        {
            Directory.Delete(output, recursive: true);
        }
    }

    /// <summary>
    /// The random walker ends by its wander budget or by a death, and two walkers of one seed give one intent
    /// stream (D-270 to D-272, D-403).
    /// </summary>
    /// <remarks>
    /// The walker promises no progress, so it never reads a softlock. It wanders into the scavengers of D-399, and
    /// a run that dies ends before its budget. The walker of seed 7 lived out its budget before PR-16.
    /// </remarks>
    [Fact]
    public void RandomWalkerIsDeterministicAndEndsByBudgetOrDeath()
    {
        BotRunResult result = BotRun.Play(new RandomWalker(7UL), 7UL, TestWorld.Content);
        Assert.True(result.End == BotRunEnd.Budget || result.End == BotRunEnd.Death, $"The walker ended as {result.End} after {result.Ticks} ticks. {result.Error}");
        Assert.True(result.Ticks <= BotRun.WanderBudget, $"The walker ran {result.Ticks} ticks, and its budget is {BotRun.WanderBudget}.");
        Assert.Equal(RandomWalker.PolicyName, result.Policy);

        SimulationLoop first = TestWorld.NewLoop(8UL);
        SimulationLoop second = TestWorld.NewLoop(8UL);
        RandomWalker one = new(8UL);
        RandomWalker two = new(8UL);
        RandomWalker other = new(9UL);
        int differ = 0;
        for (uint tick = 0; tick < 600; tick++)
        {
            Intent a = one.Next(first);
            Intent b = two.Next(second);
            Assert.Equal(a, b);
            if (a != other.Next(first))
            {
                differ++;
            }

            first.Step(a);
            second.Step(b);
        }

        Assert.True(differ > 100, $"Two seeds gave {differ} different intents of six hundred.");
        Assert.Equal(first.Hash(), second.Hash());
    }

    /// <summary>The runner names an unknown policy and a bad seed range, and refuses to run (T-2).</summary>
    [Fact]
    public void TheRunnerRejectsBadArguments()
    {
        string output = TempDirectory("bad");
        try
        {
            ContextException error = Assert.Throws<ContextException>(() => BotRunCommand.CreatePolicy("sleeper", 1UL, TestWorld.Content));
            Assert.Contains("policy=sleeper", error.Message, StringComparison.Ordinal);
            Assert.Equal(2, Program.Main(["bot-run", "--policy", RandomWalker.PolicyName, "--seeds", "5-1", "--output", output, "--root", RepositoryRoot.Find()]));

            // A range that ends at the largest seed, or one past the largest span, is refused before any run (PR #34 automated pass).
            Assert.Equal(2, Program.Main(["bot-run", "--policy", RandomWalker.PolicyName, "--seeds", "0-18446744073709551615", "--output", output, "--root", RepositoryRoot.Find()]));
            Assert.Equal(2, Program.Main(["bot-run", "--policy", RandomWalker.PolicyName, "--seeds", "1-1000001", "--output", output, "--root", RepositoryRoot.Find()]));
            Assert.Empty(Directory.GetFiles(output));
            Assert.Equal(2, Program.Main(["bot-run", "--policy", RandomWalker.PolicyName, "--seeds", "1-1", "--output", output]));
            Assert.Equal(2, Program.Main(["bot-run", "--policy"]));
        }
        finally
        {
            Directory.Delete(output, recursive: true);
        }
    }

    /// <summary>The Bot stream is the fifth value of D-159, and it is the one that the walker draws from (D-272).</summary>
    [Fact]
    public void TheBotStreamIsTheFifthValue()
    {
        Assert.Equal(4, (int)Core.Determinism.RngStream.Bot);
        Assert.NotNull(Core.Determinism.Rng.ForStream(1UL, Core.Determinism.RngStream.Bot));
    }
}
