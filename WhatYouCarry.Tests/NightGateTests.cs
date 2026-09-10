using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using WhatYouCarry.Tools;
using WhatYouCarry.Tools.BotRunner;
using WhatYouCarry.Tools.NightGate;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// PR-58 exit tests 1 to 6 and 8: the night gate rules over fixture records, the commit check over real
/// commits, and the exit codes of the command (D-115, D-177, D-274, D-275).
/// </summary>
[Collection(ConsoleCollection.Name)]
public sealed class NightGateTests
{
    private const string Commit = "0123456789abcdef0123456789abcdef01234567";
    private const string BaseRef = "main";
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-09-11T12:00:00Z", CultureInfo.InvariantCulture);

    /// <summary>PR-58 exit test 1. A failure record fails the gate.</summary>
    [Fact]
    public void NightGateFailsOnRedNight()
    {
        NightGateResult result = NightGateRules.Evaluate(Facts(Record(Now.AddHours(-1), "failure"), commitOnBase: true));

        Assert.False(result.Passes);
        Assert.Equal(NightGateRules.FailedCase, result.Case);
    }

    /// <summary>PR-58 exit test 2. No record file fails the gate, and the message names the file and the branch.</summary>
    [Fact]
    public void NightGateFailsOnMissingResult()
    {
        NightGateResult result = NightGateRules.Evaluate(Facts(null, commitOnBase: null));

        Assert.False(result.Passes);
        Assert.Equal(NightGateRules.AbsentCase, result.Case);
        Assert.Contains("night.json", result.Message, StringComparison.Ordinal);
        Assert.Contains("night-results", result.Message, StringComparison.Ordinal);
    }

    /// <summary>PR-58 exit test 3. A record older than 48 hours is stale, and one at exactly 48 hours is not (D-177).</summary>
    [Fact]
    public void NightGateFailsOnStaleResult()
    {
        Assert.Equal(TimeSpan.FromHours(48), NightGateRules.StaleAfter);
        NightGateResult stale = NightGateRules.Evaluate(Facts(Record(Now.AddHours(-48).AddSeconds(-1), "success"), commitOnBase: true));
        NightGateResult fresh = NightGateRules.Evaluate(Facts(Record(Now.AddHours(-48), "success"), commitOnBase: true));

        Assert.False(stale.Passes);
        Assert.Equal(NightGateRules.StaleCase, stale.Case);
        Assert.True(fresh.Passes);
    }

    /// <summary>PR-58 exit test 4. A cancelled record fails the gate.</summary>
    [Fact]
    public void NightGateFailsOnCancelledResult()
    {
        NightGateResult result = NightGateRules.Evaluate(Facts(Record(Now.AddHours(-1), "cancelled"), commitOnBase: true));

        Assert.False(result.Passes);
        Assert.Equal(NightGateRules.CancelledCase, result.Case);
    }

    /// <summary>PR-58 exit test 5. A success record inside the window, at a commit on the base branch, passes.</summary>
    [Fact]
    public void NightGatePassesOnGreenNight()
    {
        NightGateResult result = NightGateRules.Evaluate(Facts(Record(Now.AddHours(-9), "success"), commitOnBase: true));

        Assert.True(result.Passes);
        Assert.Equal(NightGateRules.PassCase, result.Case);
        Assert.Contains(Commit, result.Message, StringComparison.Ordinal);
        Assert.Contains("2026-09-11T03:00:00Z", result.Message, StringComparison.Ordinal);
        Assert.Contains(BaseRef, result.Message, StringComparison.Ordinal);
    }

    /// <summary>PR-58 exit test 6. Each failure message names the case, the record commit, and the record time (T-2).</summary>
    [Fact]
    public void EveryFailureNamesTheCaseTheCommitAndTheTime()
    {
        DateTimeOffset endedAt = Now.AddHours(-3);
        string time = "2026-09-11T09:00:00Z";
        (NightGateFacts Facts, string Case, string Word)[] failures =
        [
            (Facts(Record(endedAt, "failure"), commitOnBase: true), NightGateRules.FailedCase, "failed"),
            (Facts(Record(endedAt, "cancelled"), commitOnBase: true), NightGateRules.CancelledCase, "cancelled"),
            (Facts(Record(Now.AddHours(-49), "success"), commitOnBase: true), NightGateRules.StaleCase, "stale"),
            (Facts(Record(endedAt, "success"), commitOnBase: false), NightGateRules.ForeignCase, "not on " + BaseRef),
        ];
        foreach ((NightGateFacts facts, string expectedCase, string word) in failures)
        {
            NightGateResult result = NightGateRules.Evaluate(facts);
            Assert.False(result.Passes);
            Assert.Equal(expectedCase, result.Case);
            Assert.Contains(word, result.Message, StringComparison.Ordinal);
            Assert.Contains(Commit, result.Message, StringComparison.Ordinal);
            Assert.Contains(expectedCase == NightGateRules.StaleCase ? "2026-09-09T11:00:00Z" : time, result.Message, StringComparison.Ordinal);
        }
    }

    /// <summary>A text that is not a record fails the gate, and the message names the defect (T-2).</summary>
    [Fact]
    public void NightGateFailsOnMalformedRecord()
    {
        (string Text, string Names)[] malformed =
        [
            ("not json", "not JSON"),
            ("[]", "object"),
            ("{\"commit\":\"" + Commit + "\",\"status\":\"success\"}", NightRecordCommand.EndedAtName),
            ("{\"commit\":\"abc\",\"endedAt\":\"2026-09-11T03:00:00Z\",\"status\":\"success\"}", NightRecordCommand.CommitName),
            ("{\"commit\":\"" + Commit + "\",\"endedAt\":\"2026-09-11 03:00\",\"status\":\"success\"}", NightRecordCommand.EndedAtName),
            ("{\"commit\":\"" + Commit + "\",\"endedAt\":\"2026-09-11T03:00:00Z\",\"status\":\"green\"}", NightRecordCommand.StatusName),
            ("{\"commit\":\"" + Commit + "\",\"endedAt\":\"2026-09-11T03:00:00Z\",\"status\":7}", NightRecordCommand.StatusName),
        ];
        foreach ((string text, string names) in malformed)
        {
            NightGateResult result = NightGateRules.Evaluate(Facts(text, commitOnBase: null));
            Assert.False(result.Passes);
            Assert.Equal(NightGateRules.MalformedCase, result.Case);
            Assert.Contains(names, result.Message, StringComparison.Ordinal);
        }
    }

    /// <summary>PR-58 exit test 8. A record at a commit on another branch, or at a commit the checkout lacks, fails the gate (D-275).</summary>
    [Fact]
    public void NightGateFailsOnForeignCommit()
    {
        using var repo = new TemporaryGitRepository();
        string onMain = repo.Commit("feat: on main", Files(("a.txt", "a")));
        repo.CreateBranch("feature");
        string onFeature = repo.Commit("feat: on feature", Files(("b.txt", "b")));
        string directory = TempDirectory();
        try
        {
            string path = Path.Combine(directory, "night.json");

            File.WriteAllText(path, Record(Now.AddHours(-1), "success", onFeature));
            NightGateResult foreign = NightGateRules.Evaluate(NightGateFacts.Gather(path, repo.Path, BaseRef, Now));
            Assert.False(foreign.Passes);
            Assert.Equal(NightGateRules.ForeignCase, foreign.Case);
            Assert.Contains(onFeature, foreign.Message, StringComparison.Ordinal);

            File.WriteAllText(path, Record(Now.AddHours(-1), "success", "ffffffffffffffffffffffffffffffffffffffff"));
            NightGateResult unknown = NightGateRules.Evaluate(NightGateFacts.Gather(path, repo.Path, BaseRef, Now));
            Assert.False(unknown.Passes);
            Assert.Equal(NightGateRules.ForeignCase, unknown.Case);

            File.WriteAllText(path, Record(Now.AddHours(-1), "success", onMain));
            NightGateResult green = NightGateRules.Evaluate(NightGateFacts.Gather(path, repo.Path, BaseRef, Now));
            Assert.True(green.Passes);
            Assert.Contains(onMain, green.Message, StringComparison.Ordinal);

            NightGateResult absent = NightGateRules.Evaluate(NightGateFacts.Gather(Path.Combine(directory, "none.json"), repo.Path, BaseRef, Now));
            Assert.Equal(NightGateRules.AbsentCase, absent.Case);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    /// <summary>The command exits 0 on a pass, 1 on a failure, and 2 on a wrong option or time.</summary>
    [Fact]
    public void NightGateCommandReportsEachExitCode()
    {
        using var repo = new TemporaryGitRepository();
        string onMain = repo.Commit("feat: on main", Files(("a.txt", "a")));
        string directory = TempDirectory();
        try
        {
            string path = Path.Combine(directory, "night.json");
            string[] tail = ["--root", repo.Path, "--base", BaseRef, "--now", "2026-09-11T12:00:00Z"];

            File.WriteAllText(path, Record(Now.AddHours(-1), "success", onMain));
            Assert.Equal(0, Program.Main(["night-gate", "--record", path, .. tail]));

            File.WriteAllText(path, Record(Now.AddHours(-1), "failure", onMain));
            Assert.Equal(1, Program.Main(["night-gate", "--record", path, .. tail]));

            Assert.Equal(1, Program.Main(["night-gate", "--record", Path.Combine(directory, "none.json"), .. tail]));
            Assert.Equal(2, Program.Main(["night-gate", "--record", path, "--root", repo.Path, "--base", BaseRef]));
            Assert.Equal(2, Program.Main(["night-gate", "--record", path, "--root", repo.Path, "--base", BaseRef, "--now", "yesterday"]));
            Assert.Equal(2, Program.Main(["night-gate", "--record"]));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static NightGateFacts Facts(string? text, bool? commitOnBase)
    {
        NightRecord? record = null;
        string? parseError = null;
        if (text is not null)
        {
            record = NightRecordParser.TryParse(text, out string error);
            if (record is null)
            {
                parseError = error;
            }
        }

        return new NightGateFacts
        {
            RecordText = text,
            Record = record,
            ParseError = parseError,
            CommitOnBase = commitOnBase,
            BaseRef = BaseRef,
            Now = Now,
        };
    }

    private static string Record(DateTimeOffset endedAt, string status, string commit = Commit)
    {
        return NightRecordCommand.Build(commit, endedAt.UtcDateTime, status);
    }

    private static Dictionary<string, string> Files(params (string Path, string Content)[] files)
    {
        var result = new Dictionary<string, string>();
        foreach ((string path, string content) in files)
        {
            result[path] = content;
        }

        return result;
    }

    private static string TempDirectory()
    {
        string directory = Path.Combine(Path.GetTempPath(), "wyc-night-gate-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }
}
