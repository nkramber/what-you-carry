using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using WhatYouCarry.Core.Bots;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Tools;
using WhatYouCarry.Tools.BotRunner;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The bot policies, the run, the runner command, and the night record (D-115, D-127, D-270 to D-273; PR-11 exit tests 1 to 4 and 7).</summary>
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
                string[] lines = File.ReadAllLines(log);
                Assert.Equal(2, lines.Length);
                using JsonDocument end = JsonDocument.Parse(lines[1]);
                JsonElement root = end.RootElement;
                foreach (string field in new[] { "seed", "floor", "tick", "subsystem", "entities", BotRunCommand.PolicyName, BotRunCommand.EndStateName, BotRunCommand.FloorsReachedName })
                {
                    Assert.True(root.TryGetProperty(field, out _), $"{Path.GetFileName(log)}: the end line lacks '{field}'.");
                }

                Assert.Equal(BotRunCommand.Subsystem, root.GetProperty("subsystem").GetString());
                string state = root.GetProperty(BotRunCommand.EndStateName).GetString()!;
                string policy = root.GetProperty(BotRunCommand.PolicyName).GetString()!;
                Assert.Equal(policy == RandomWalker.PolicyName ? "budget" : "bottom", state);
                Assert.True(root.GetProperty("tick").GetInt64() > 0);
            }
        }
        finally
        {
            Directory.Delete(output, recursive: true);
        }
    }

    /// <summary>PR-11 exit test 2. A policy that promises progress and makes none ends as a softlock at the floor budget (D-270, D-271).</summary>
    [Fact]
    public void SoftlockIsDetected()
    {
        BotRunResult result = BotRun.Play(new StandStill(), 5UL, TestWorld.Content);
        Assert.Equal(BotRunEnd.Softlock, result.End);
        Assert.Equal(BotRun.FloorBudget, result.Ticks);
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

    /// <summary>PR-11 exit test 4. The greedy descender reaches the bottom on one hundred seeds: fifteen floors, and the ascend at the last stairwell. A failure names its seed (D-66).</summary>
    [Fact]
    public void GreedyDescenderReachesBottom()
    {
        for (ulong seed = 1; seed <= 100; seed++)
        {
            BotRunResult result = BotRun.Play(new GreedyDescender(TestWorld.Content), seed, TestWorld.Content);
            Assert.True(result.End == BotRunEnd.Bottom, $"Seed {seed}: the run ended as {result.End} on floor {result.FloorsReached} after {result.Ticks} ticks. {result.Error}");
            Assert.Equal(15, result.FloorsReached);
        }
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
            Assert.Equal(0, Program.Main(["night-record", "--commit", commit, "--status", "success", "--output", file]));

            using JsonDocument record = JsonDocument.Parse(File.ReadAllText(file));
            Assert.Equal(commit, record.RootElement.GetProperty(NightRecordCommand.CommitName).GetString());
            Assert.Equal("success", record.RootElement.GetProperty(NightRecordCommand.StatusName).GetString());
            DateTime endedAt = DateTime.Parse(record.RootElement.GetProperty(NightRecordCommand.EndedAtName).GetString()!, null, System.Globalization.DateTimeStyles.AdjustToUniversal);
            Assert.InRange(endedAt, before, DateTime.UtcNow.AddSeconds(1));

            Assert.Equal(2, Program.Main(["night-record", "--commit", "abc", "--status", "success", "--output", file]));
            Assert.Equal(2, Program.Main(["night-record", "--commit", commit, "--status", "green", "--output", file]));
            Assert.Equal("{\"commit\":\"" + commit + "\",\"endedAt\":\"2026-09-10T03:00:00Z\",\"status\":\"failure\"}\n", NightRecordCommand.Build(commit, new DateTime(2026, 9, 10, 3, 0, 0, DateTimeKind.Utc), "failure"));
        }
        finally
        {
            Directory.Delete(output, recursive: true);
        }
    }

    /// <summary>The random walker ends by its wander budget, and two walkers of one seed give one intent stream (D-270 to D-272).</summary>
    [Fact]
    public void RandomWalkerIsDeterministicAndEndsByBudget()
    {
        BotRunResult result = BotRun.Play(new RandomWalker(7UL), 7UL, TestWorld.Content);
        Assert.Equal(BotRunEnd.Budget, result.End);
        Assert.Equal(BotRun.WanderBudget, result.Ticks);
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
