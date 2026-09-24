using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using WhatYouCarry.Core.Bots;
using WhatYouCarry.Tools;
using WhatYouCarry.Tools.BotRunner;
using WhatYouCarry.Tools.NightGate;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// PR-84: the seeds of each night (D-564 to D-569). The fixed set stays the gate, a slice of the date runs past it,
/// the extra fixed seeds and the carried seeds join the list, and the record names the slice, the carried seeds that
/// ran, and the failed seeds.
/// </summary>
[Collection(ConsoleCollection.Name)]
public sealed class NightSeedsTests
{
    private const string Commit = "0123456789abcdef0123456789abcdef01234567";

    /// <summary>PR-84 exit test 1. Day 0 takes the first window past the fixed range, and each window follows the last, one tenth in size (D-566).</summary>
    [Fact]
    public void EachSliceFollowsTheLastPastTheFixedRange()
    {
        Assert.Equal(new SeedRange(1, 5000), NightSeeds.FixedRange(GreedyDescender.PolicyName));
        Assert.Equal(new SeedRange(1, 100000), NightSeeds.FixedRange(NightSeeds.ReachabilitySweep));
        Assert.Equal(new SeedRange(5001, 5500), NightSeeds.Slice(GreedyDescender.PolicyName, NightSeeds.DayZero));
        Assert.Equal(new SeedRange(5501, 6000), NightSeeds.Slice(Coward.PolicyName, NightSeeds.DayZero.AddDays(1)));
        Assert.Equal(new SeedRange(100001, 110000), NightSeeds.Slice(NightSeeds.ReachabilitySweep, NightSeeds.DayZero));
        Assert.Equal(new SeedRange(120001, 130000), NightSeeds.Slice(NightSeeds.ReachabilitySweep, NightSeeds.DayZero.AddDays(2)));

        foreach (string sweep in NightSeeds.Sweeps)
        {
            SeedRange previous = NightSeeds.FixedRange(sweep);
            ulong size = NightSeeds.Slice(sweep, NightSeeds.DayZero).To - NightSeeds.Slice(sweep, NightSeeds.DayZero).From + 1;
            Assert.Equal(previous.To / 10, size);
            for (int day = 0; day < 400; day++)
            {
                SeedRange slice = NightSeeds.Slice(sweep, NightSeeds.DayZero.AddDays(day));
                Assert.Equal(previous.To + 1, slice.From);
                previous = slice;
            }
        }

        ArgumentException early = Assert.Throws<ArgumentException>(() => NightSeeds.Slice(Coward.PolicyName, NightSeeds.DayZero.AddDays(-1)));
        Assert.Contains("2026-09-23", early.Message, StringComparison.Ordinal);
        Assert.Throws<ArgumentException>(() => NightSeeds.Slice("walker", NightSeeds.DayZero));
    }

    /// <summary>PR-84 exit test 2. The plan holds the fixed range, the slice, and each extra and carried seed once, in order (D-564, D-567).</summary>
    [Fact]
    public void ThePlanHoldsEachSeedOnce()
    {
        DateOnly date = NightSeeds.DayZero.AddDays(3);
        List<SeedRange> plan = NightSeeds.Plan(FullClearer.PolicyName, date, [9000, 7000], [7000, 6600, 3, 12000]);

        Assert.Equal("1-5000,6501-7000,9000,12000", NightSeeds.FormatList(plan));
        Assert.Equal(5502UL, NightSeeds.Count(plan));
        Assert.Equal("1-5000,6501-7000", NightSeeds.FormatList(NightSeeds.Plan(FullClearer.PolicyName, date, [], [])));
    }

    /// <summary>A seed list reads ranges and single seeds, and a wrong item names itself (T-2).</summary>
    [Fact]
    public void ASeedListReadsRangesAndSingleSeeds()
    {
        List<SeedRange>? list = NightSeeds.TryParseList("1-5000,7123,8000-8001", out string error);
        Assert.True(list is not null, error);
        Assert.Equal(new[] { new SeedRange(1, 5000), new SeedRange(7123, 7123), new SeedRange(8000, 8001) }, list);
        Assert.Equal(5003UL, NightSeeds.Count(list!));
        Assert.Equal("1-5000,7123,8000-8001", NightSeeds.FormatList(list!));

        foreach (string wrong in new[] { "5-1", "a", string.Empty, "1-2-3", "1,,2", "-4", "1-5000 " })
        {
            Assert.Null(NightSeeds.TryParseList(wrong, out string reason));
            Assert.Contains("seed list", reason, StringComparison.Ordinal);
        }
    }

    /// <summary>PR-84 exit test 3. The extra seed file of the repository reads, and each wrong file names the file, the sweep, and the seed (D-567, T-2).</summary>
    [Fact]
    public void TheExtraSeedFileHoldsEachSweepPastItsFixedRange()
    {
        string path = Path.Combine(RepositoryRoot.Find(), NightSeeds.ExtraSeedsPath);
        Dictionary<string, List<ulong>> extra = NightSeeds.ReadExtraSeeds(File.ReadAllText(path), path);
        Assert.Equal(NightSeeds.Sweeps.Length, extra.Count);

        const string empty = "\"random-walker\":[],\"greedy-descender\":[],\"full-clearer\":[],\"timer-tester\":[],\"coward\":[]";
        (string Text, string Names)[] wrong =
        [
            ("{" + empty + ",\"reachability\":[100000]}", "100000"),
            ("{" + empty + ",\"reachability\":[100001,100001]}", "two times"),
            ("{" + empty + "}", "'reachability'"),
            ("{" + empty + ",\"reachability\":[],\"walker\":[]}", "'walker'"),
            ("{" + empty + ",\"reachability\":[\"100001\"]}", "whole number"),
            ("{" + empty + ",\"reachability\":{}}", "array"),
            ("[]", "object"),
            ("not json", "not JSON"),
        ];
        foreach ((string text, string names) in wrong)
        {
            FormatException exception = Assert.Throws<FormatException>(() => NightSeeds.ReadExtraSeeds(text, "extra.json"));
            Assert.Contains("extra.json", exception.Message, StringComparison.Ordinal);
            Assert.Contains(names, exception.Message, StringComparison.Ordinal);
        }
    }

    /// <summary>A failure line names its sweep and each failed seed, and a line with no seed proves that the sweep ended (D-567).</summary>
    [Fact]
    public void FailureLinesNameEachSweepThatEnded()
    {
        string text = NightSeeds.FailureLine(Coward.PolicyName, []) + NightSeeds.FailureLine(NightSeeds.ReachabilitySweep, [100250, 104000]);
        Assert.Equal("coward:\nreachability: 100250 104000\n", text);

        Dictionary<string, List<ulong>> failures = NightSeeds.ReadFailures(text + "\n", "failures.txt");
        Assert.Empty(failures[Coward.PolicyName]);
        Assert.Equal(new ulong[] { 100250, 104000 }, failures[NightSeeds.ReachabilitySweep]);
        Assert.False(failures.ContainsKey(GreedyDescender.PolicyName));

        foreach (string wrong in new[] { "coward", "walker: 1", "coward: x", "coward:\ncoward:" })
        {
            FormatException exception = Assert.Throws<FormatException>(() => NightSeeds.ReadFailures(wrong, "failures.txt"));
            Assert.Contains("failures.txt", exception.Message, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// PR-84 exit test 4. A sweep that ended drops the carried seeds that passed and names its failures. A sweep that
    /// did not end keeps the carried seeds. The record names the slice of the date and the carried seeds that ran (D-567, D-569).
    /// </summary>
    [Fact]
    public void TheRecordCarriesEachFailedSeedUntilANightPassesIt()
    {
        DateOnly date = NightSeeds.DayZero.AddDays(1);
        Dictionary<string, List<ulong>> carry = new(StringComparer.Ordinal)
        {
            [GreedyDescender.PolicyName] = [5200],
            [NightSeeds.ReachabilitySweep] = [100500],
        };
        Dictionary<string, List<ulong>> failures = Ended([RandomWalker.PolicyName, GreedyDescender.PolicyName]);
        failures[GreedyDescender.PolicyName] = [5601];

        using JsonDocument failed = Parse(NightSeeds.RecordFields(date, "failure", failures, carry));
        JsonElement slice = failed.RootElement.GetProperty(NightSeeds.SliceName);
        Assert.Equal("2026-09-25", slice.GetProperty(NightSeeds.SliceDateName).GetString());
        Assert.Equal("5501-6000", slice.GetProperty(GreedyDescender.PolicyName).GetString());
        Assert.Equal("110001-120000", slice.GetProperty(NightSeeds.ReachabilitySweep).GetString());
        Assert.Equal(new ulong[] { 5200 }, Seeds(failed, NightSeeds.CarriedSeedsName, GreedyDescender.PolicyName));
        Assert.Empty(Seeds(failed, NightSeeds.CarriedSeedsName, NightSeeds.ReachabilitySweep));
        Assert.Equal(new ulong[] { 5601 }, Seeds(failed, NightSeeds.FailedSeedsName, GreedyDescender.PolicyName));
        Assert.Equal(new ulong[] { 100500 }, Seeds(failed, NightSeeds.FailedSeedsName, NightSeeds.ReachabilitySweep));
        Assert.Empty(Seeds(failed, NightSeeds.FailedSeedsName, RandomWalker.PolicyName));

        using JsonDocument passed = Parse(NightSeeds.RecordFields(date, "success", Ended(NightSeeds.Sweeps), carry));
        Assert.Equal(new ulong[] { 100500 }, Seeds(passed, NightSeeds.CarriedSeedsName, NightSeeds.ReachabilitySweep));
        foreach (string sweep in NightSeeds.Sweeps)
        {
            Assert.Empty(Seeds(passed, NightSeeds.FailedSeedsName, sweep));
        }

        // A success that names a failure, or a sweep that did not end, is a contradiction, and no record hides it (T-2).
        FormatException notEnded = Assert.Throws<FormatException>(() => NightSeeds.RecordFields(date, "success", Ended([RandomWalker.PolicyName]), carry));
        Assert.Contains("did not end", notEnded.Message, StringComparison.Ordinal);
        Dictionary<string, List<ulong>> allEnded = Ended(NightSeeds.Sweeps);
        allEnded[Coward.PolicyName] = [5510];
        FormatException named = Assert.Throws<FormatException>(() => NightSeeds.RecordFields(date, "success", allEnded, carry));
        Assert.Contains("5510", named.Message, StringComparison.Ordinal);
    }

    /// <summary>A record names the slice window of each sweep, and a night ran the fixed range, that slice, and its carried seeds (D-569).</summary>
    [Fact]
    public void ARecordNamesTheSeedsThatItsNightRan()
    {
        DateOnly date = NightSeeds.DayZero.AddDays(2);
        Dictionary<string, List<ulong>> carry = new(StringComparer.Ordinal) { [Coward.PolicyName] = [9100] };
        string record = "{" + NightSeeds.RecordFields(date, "success", Ended(NightSeeds.Sweeps), carry) + "}";
        Dictionary<string, SeedRange> slices = NightSeeds.ReadSlices(record, "night.json");
        Dictionary<string, List<ulong>> carried = NightSeeds.ReadRecordSeeds(record, NightSeeds.CarriedSeedsName, "night.json");
        Assert.Equal(new SeedRange(6001, 6500), slices[Coward.PolicyName]);
        Assert.Equal(new SeedRange(120001, 130000), slices[NightSeeds.ReachabilitySweep]);

        Assert.True(NightSeeds.Ran(Coward.PolicyName, 4119, slices, carried));
        Assert.True(NightSeeds.Ran(Coward.PolicyName, 6500, slices, carried));
        Assert.True(NightSeeds.Ran(Coward.PolicyName, 9100, slices, carried));
        Assert.False(NightSeeds.Ran(Coward.PolicyName, 6501, slices, carried));
        Assert.False(NightSeeds.Ran(GreedyDescender.PolicyName, 9100, slices, carried));

        Assert.Empty(NightSeeds.ReadSlices(NightRecordCommand.Build(Commit, new DateTime(2026, 9, 24, 5, 6, 18, DateTimeKind.Utc), "success", string.Empty), "night.json"));
        Assert.Throws<FormatException>(() => NightSeeds.ReadSlices("{\"slice\":{\"coward\":\"7-1\"}}", "night.json"));
        Assert.Throws<FormatException>(() => NightSeeds.ReadSlices("{\"slice\":{\"walker\":\"1-2\"}}", "night.json"));
        Assert.Throws<FormatException>(() => NightSeeds.ReadSlices("{\"slice\":[]}", "night.json"));
    }

    /// <summary>A record of a night before PR-84 holds no seed field, so it carries nothing, and a wrong field names itself (T-2).</summary>
    [Fact]
    public void AnOlderRecordCarriesNoSeed()
    {
        string older = NightRecordCommand.Build(Commit, new DateTime(2026, 9, 24, 5, 6, 18, DateTimeKind.Utc), "success", string.Empty);
        Assert.Empty(NightSeeds.ReadRecordSeeds(older, NightSeeds.FailedSeedsName, "night.json"));
        Assert.Empty(NightSeeds.ReadRecordSeeds("﻿" + older, NightSeeds.CarriedSeedsName, "night.json"));

        FormatException wrong = Assert.Throws<FormatException>(() => NightSeeds.ReadRecordSeeds("{\"failedSeeds\":[1]}", NightSeeds.FailedSeedsName, "night.json"));
        Assert.Contains("'failedSeeds'", wrong.Message, StringComparison.Ordinal);
        Assert.Throws<FormatException>(() => NightSeeds.ReadRecordSeeds("{\"failedSeeds\":{\"walker\":[1]}}", NightSeeds.FailedSeedsName, "night.json"));
    }

    /// <summary>PR-84 exit test 5. The command prints the list alone on standard output, and standard error names the window and each part (D-564).</summary>
    [Fact]
    public void NightSeedsCommandPrintsTheListAndNamesTheWindow()
    {
        string directory = TempDirectory();
        string carry = Path.Combine(directory, "night.json");
        File.WriteAllText(carry, NightRecordCommand.Build(Commit, new DateTime(2026, 9, 25, 5, 0, 0, DateTimeKind.Utc), "failure", string.Empty, NightSeeds.RecordFields(NightSeeds.DayZero, "failure", Failed(TimerTester.PolicyName, 5250), new Dictionary<string, List<ulong>>())));
        TextWriter savedOut = Console.Out;
        TextWriter savedError = Console.Error;
        try
        {
            StringWriter output = new();
            StringWriter errors = new();
            Console.SetOut(output);
            Console.SetError(errors);

            int exit = Program.Main(["night-seeds", "--sweep", TimerTester.PolicyName, "--date", "2026-09-26", "--root", RepositoryRoot.Find(), "--carry", carry]);
            Assert.Equal(0, exit);
            Assert.Equal("1-5000,6001-6500,5250", output.ToString().Trim());
            Assert.Contains("day 2", errors.ToString(), StringComparison.Ordinal);
            Assert.Contains("the slice 6001-6500", errors.ToString(), StringComparison.Ordinal);
            Assert.Contains("the carried seeds [5250]", errors.ToString(), StringComparison.Ordinal);

            string[] common = ["--root", RepositoryRoot.Find(), "--carry", carry];
            Assert.Equal(2, Program.Main(["night-seeds", "--sweep", "walker", "--date", "2026-09-26", .. common]));
            Assert.Equal(2, Program.Main(["night-seeds", "--sweep", Coward.PolicyName, "--date", "26-09-2026", .. common]));
            Assert.Equal(2, Program.Main(["night-seeds", "--sweep", Coward.PolicyName, "--date", "2026-09-23", .. common]));
            Assert.Equal(2, Program.Main(["night-seeds", "--sweep", Coward.PolicyName, "--date", "2026-09-26", "--root", RepositoryRoot.Find(), "--carry", Path.Combine(directory, "absent.json")]));
            Assert.Equal(2, Program.Main(["night-seeds", "--sweep", Coward.PolicyName, "--date", "2026-09-26", "--root", RepositoryRoot.Find()]));
            Assert.Contains("absent.json", errors.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            Console.SetOut(savedOut);
            Console.SetError(savedError);
            Directory.Delete(directory, recursive: true);
        }
    }

    /// <summary>PR-84 exit test 6. The record command writes the seed fields, and an absent failures file or date is an error (D-567, T-2).</summary>
    [Fact]
    public void NightRecordCommandWritesTheSeedFields()
    {
        string directory = TempDirectory();
        string carry = Path.Combine(directory, "main.json");
        string failures = Path.Combine(directory, "failures.txt");
        string output = Path.Combine(directory, "night.json");
        File.WriteAllText(carry, NightRecordCommand.Build(Commit, new DateTime(2026, 9, 25, 5, 0, 0, DateTimeKind.Utc), "success", string.Empty));
        File.WriteAllText(failures, NightSeeds.FailureLine(RandomWalker.PolicyName, []) + NightSeeds.FailureLine(GreedyDescender.PolicyName, [5777]));
        TextWriter savedOut = Console.Out;
        TextWriter savedError = Console.Error;
        try
        {
            Console.SetOut(new StringWriter());
            StringWriter errors = new();
            Console.SetError(errors);
            string[] record = ["night-record", "--commit", Commit, "--status", "failure", "--output", output];

            Assert.Equal(0, Program.Main([.. record, "--date", "2026-09-27", "--failures", failures, "--carry", carry]));
            string text = File.ReadAllText(output);
            Assert.NotNull(NightRecordParser.TryParse(text, out _));
            Assert.Equal(new ulong[] { 5777 }, NightSeeds.SeedsOf(NightSeeds.ReadRecordSeeds(text, NightSeeds.FailedSeedsName, output), GreedyDescender.PolicyName));
            Assert.Contains("\"slice\":{\"date\":\"2026-09-27\",\"random-walker\":\"6501-7000\"", text, StringComparison.Ordinal);

            Assert.Equal(2, Program.Main([.. record, "--date", "2026-09-27", "--failures", Path.Combine(directory, "absent.txt"), "--carry", carry]));
            Assert.Contains("absent.txt", errors.ToString(), StringComparison.Ordinal);
            Assert.Equal(2, Program.Main([.. record, "--date", "2026-09-23", "--failures", failures, "--carry", carry]));
            Assert.Equal(2, Program.Main([.. record, "--failures", failures, "--carry", carry]));
        }
        finally
        {
            Console.SetOut(savedOut);
            Console.SetError(savedError);
            Directory.Delete(directory, recursive: true);
        }
    }

    /// <summary>PR-84 exit test 7. The runner takes a seed list, runs each seed of it once, and writes the failure line of its policy (D-564, D-567).</summary>
    [Fact]
    public void BotRunTakesASeedListAndWritesItsFailureLine()
    {
        string directory = TempDirectory();
        string logs = Path.Combine(directory, "logs");
        string failures = Path.Combine(directory, "failures.txt");
        try
        {
            Assert.Equal(0, Program.Main(["bot-run", "--policy", Coward.PolicyName, "--seeds", "1,3-4", "--output", logs, "--root", RepositoryRoot.Find(), "--failures", failures]));
            Assert.Equal("coward:\n", File.ReadAllText(failures));
            Assert.True(File.Exists(Path.Combine(logs, "coward-3.jsonl")));
            Assert.False(File.Exists(Path.Combine(logs, "coward-2.jsonl")));
            Assert.Equal(3, Directory.GetFiles(logs, "*.jsonl").Length);

            Assert.Equal(2, Program.Main(["bot-run", "--policy", Coward.PolicyName, "--seeds", "4-1", "--output", logs, "--root", RepositoryRoot.Find()]));
            Assert.Equal(2, Program.Main(["bot-run", "--policy", Coward.PolicyName, "--seeds", "1-600000,700000-1300000", "--output", logs, "--root", RepositoryRoot.Find()]));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }

        Assert.True(BotRunCommand.IsFailure(BotRunEnd.Crash, promisesProgress: false));
        Assert.True(BotRunCommand.IsFailure(BotRunEnd.Softlock, promisesProgress: true));
        Assert.False(BotRunCommand.IsFailure(BotRunEnd.Softlock, promisesProgress: false));
        Assert.False(BotRunCommand.IsFailure(BotRunEnd.Death, promisesProgress: true));
    }

    /// <summary>PR-84 exit test 8. The night reachability sweep reads the seed list of the night, and the count of D-480 off the night (D-564).</summary>
    [Fact]
    public void TheReachabilitySweepReadsTheSeedListOfTheNight()
    {
        Assert.Equal(SweepScope.Seeds(ProcgenTests.ReachabilitySeeds), ProcgenTests.ReachabilitySeedList(null, null).Count);
        Assert.Equal(SweepScope.Seeds(ProcgenTests.ReachabilitySeeds), ProcgenTests.ReachabilitySeedList(null, "1-3").Count);
        Assert.Equal(ProcgenTests.ReachabilitySeedsPerNight, ProcgenTests.ReachabilitySeedList("1", null).Count);
        Assert.Equal(new[] { 1, 2, 3, 100007 }, ProcgenTests.ReachabilitySeedList("1", "1-3,100007"));
        Assert.Throws<InvalidOperationException>(() => ProcgenTests.ReachabilitySeedList("1", "x"));
        Assert.Throws<InvalidOperationException>(() => ProcgenTests.ReachabilitySeedList("1", "2147483647"));
    }

    /// <summary>A map of failure lines where each named sweep ended with no failure.</summary>
    private static Dictionary<string, List<ulong>> Ended(IEnumerable<string> sweeps)
    {
        Dictionary<string, List<ulong>> failures = new(StringComparer.Ordinal);
        foreach (string sweep in sweeps)
        {
            failures[sweep] = [];
        }

        return failures;
    }

    /// <summary>A map of failure lines where every sweep ended, and one sweep failed one seed.</summary>
    private static Dictionary<string, List<ulong>> Failed(string sweep, ulong seed)
    {
        Dictionary<string, List<ulong>> failures = Ended(NightSeeds.Sweeps);
        failures[sweep] = [seed];
        return failures;
    }

    /// <summary>The seed fields as one JSON object.</summary>
    private static JsonDocument Parse(string fields)
    {
        return JsonDocument.Parse("{" + fields + "}");
    }

    /// <summary>The seeds of one sweep in one seed field.</summary>
    private static List<ulong> Seeds(JsonDocument record, string field, string sweep)
    {
        List<ulong> seeds = [];
        foreach (JsonElement item in record.RootElement.GetProperty(field).GetProperty(sweep).EnumerateArray())
        {
            seeds.Add(item.GetUInt64());
        }

        return seeds;
    }

    private static string TempDirectory()
    {
        string path = Path.Combine(Path.GetTempPath(), $"wyc-night-seeds-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }
}
