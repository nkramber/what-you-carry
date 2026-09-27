using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using WhatYouCarry.Core.Bots;
using WhatYouCarry.Tools;
using WhatYouCarry.Tools.BotRunner;
using WhatYouCarry.Tools.NightGate;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// PR-58 exit tests 1 to 6 and 8: the night gate rules over fixture records, the commit check and the remote
/// read over real repositories, and the exit codes of the command (D-115, D-177, D-274, D-275). PR-83: the promotion
/// of a branch night to the record of main, and the order check of a night on main (D-555 to D-558, D-562). PR-84:
/// the promotion needs the carried seeds of the record of main (D-569).
/// </summary>
[Collection(ConsoleCollection.Name)]
[Trait("Category", DocumentsCategoryTests.DocumentsCategory)]
public sealed class NightGateTests
{
    private const string Commit = "0123456789abcdef0123456789abcdef01234567";
    private const string BaseRef = "origin/main";
    private const string HeadBranch = "fix/pr-81-night";
    private const string EffectiveHead = "89abcdef0123456789abcdef0123456789abcdef";
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

    /// <summary>
    /// F-125. A record that ends after the time of the evaluation fails the gate, and the message names both times. A
    /// record that ends at the time of the evaluation passes.
    /// </summary>
    [Fact]
    public void NightGateFailsOnARecordThatEndsInTheFuture()
    {
        NightGateResult future = NightGateRules.Evaluate(Facts(Record(Now.AddSeconds(1), "success"), commitOnBase: true));
        NightGateResult yearAhead = NightGateRules.Evaluate(Facts(Record(Now.AddYears(1), "success"), commitOnBase: true));
        NightGateResult present = NightGateRules.Evaluate(Facts(Record(Now, "success"), commitOnBase: true));

        Assert.False(future.Passes);
        Assert.Equal(NightGateRules.FutureCase, future.Case);
        Assert.Contains("ended at 2026-09-11T12:00:01Z", future.Message, StringComparison.Ordinal);
        Assert.Contains("later than 2026-09-11T12:00:00Z", future.Message, StringComparison.Ordinal);
        Assert.Equal(NightGateRules.FutureCase, yearAhead.Case);
        Assert.True(present.Passes, present.Message);
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
        using var remote = new TemporaryGitRepository();
        string onMain = remote.Commit("feat: on main", Files(("a.txt", "a")));
        remote.CreateBranch("feature");
        string onFeature = remote.Commit("feat: on feature", Files(("b.txt", "b")));
        remote.Git(["checkout", "-q", "main"]);
        using TemporaryGitRepository local = CloneOf(remote);

        PublishNight(remote, ("night.json", Record(Now.AddHours(-1), "success", onFeature)));
        NightGateResult foreign = NightGateRules.Evaluate(NightGateFacts.Gather(local.Path, "origin", BaseRef, HeadBranch, "origin/main", Now));
        Assert.False(foreign.Passes);
        Assert.Equal(NightGateRules.ForeignCase, foreign.Case);
        Assert.Contains(onFeature, foreign.Message, StringComparison.Ordinal);

        PublishNight(remote, ("night.json", Record(Now.AddHours(-1), "success", "ffffffffffffffffffffffffffffffffffffffff")));
        NightGateResult unknown = NightGateRules.Evaluate(NightGateFacts.Gather(local.Path, "origin", BaseRef, HeadBranch, "origin/main", Now));
        Assert.False(unknown.Passes);
        Assert.Equal(NightGateRules.ForeignCase, unknown.Case);

        PublishNight(remote, ("night.json", Record(Now.AddHours(-1), "success", onMain)));
        NightGateResult green = NightGateRules.Evaluate(NightGateFacts.Gather(local.Path, "origin", BaseRef, HeadBranch, "origin/main", Now));
        Assert.True(green.Passes);
        Assert.Contains(onMain, green.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// PR #40 review P1-1. The gate reads the record from the branch of the remote and never from the working
    /// tree of the checkout, so a pull request that carries a fresh success record still meets the absent case.
    /// A branch without the file is absent too, a record on the branch wins over the planted file, and a remote
    /// that git cannot reach is an error that names it, not an absent record (T-2).
    /// </summary>
    [Fact]
    public void NightGateReadsTheRecordFromTheRemoteAndNeverFromTheCheckout()
    {
        using var remote = new TemporaryGitRepository();
        string onMain = remote.Commit("feat: on main", Files(("a.txt", "a")));
        using TemporaryGitRepository local = CloneOf(remote);
        File.WriteAllText(Path.Combine(local.Path, "night.json"), Record(Now.AddHours(-1), "success", onMain));

        NightGateResult absent = NightGateRules.Evaluate(NightGateFacts.Gather(local.Path, "origin", BaseRef, HeadBranch, "origin/main", Now));
        Assert.False(absent.Passes);
        Assert.Equal(NightGateRules.AbsentCase, absent.Case);
        Assert.Contains("has no branch night-results", absent.Message, StringComparison.Ordinal);

        PublishNight(remote, ("other.txt", "not the record"));
        NightGateResult noFile = NightGateRules.Evaluate(NightGateFacts.Gather(local.Path, "origin", BaseRef, HeadBranch, "origin/main", Now));
        Assert.False(noFile.Passes);
        Assert.Equal(NightGateRules.AbsentCase, noFile.Case);
        Assert.Contains("holds no night.json", noFile.Message, StringComparison.Ordinal);

        PublishNight(remote, ("night.json", Record(Now.AddHours(-1), "failure", onMain)));
        NightGateResult failed = NightGateRules.Evaluate(NightGateFacts.Gather(local.Path, "origin", BaseRef, HeadBranch, "origin/main", Now));
        Assert.False(failed.Passes);
        Assert.Equal(NightGateRules.FailedCase, failed.Case);

        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() => NightGateFacts.Gather(local.Path, "nowhere", BaseRef, HeadBranch, "origin/main", Now));
        Assert.Contains("nowhere", error.Message, StringComparison.Ordinal);
        Assert.Contains("ls-remote", error.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// F-125. The gate fetches the branch night-results by its full ref. A tag of the same name on the remote once took
    /// the place of the branch, because git reads a short name as a tag first. A tag alone, or a branch whose last
    /// part is the name, is an absent record and not an error.
    /// </summary>
    [Fact]
    public void NightGateReadsTheBranchAndNotATagOfTheSameName()
    {
        using var remote = new TemporaryGitRepository();
        string onMain = remote.Commit("feat: on main", Files(("a.txt", "a")));
        remote.Git(["tag", NightGateFacts.RecordBranch, onMain]);
        remote.CreateBranch("archive/" + NightGateFacts.RecordBranch);
        remote.Git(["checkout", "-q", "main"]);
        using TemporaryGitRepository local = CloneOf(remote);

        NightGateResult absent = NightGateRules.Evaluate(NightGateFacts.Gather(local.Path, "origin", BaseRef, HeadBranch, "origin/main", Now));
        Assert.Equal(NightGateRules.AbsentCase, absent.Case);
        Assert.Contains("has no branch night-results", absent.Message, StringComparison.Ordinal);

        PublishNight(remote, ("night.json", Record(Now.AddHours(-1), "success", onMain)));
        NightGateResult green = NightGateRules.Evaluate(NightGateFacts.Gather(local.Path, "origin", BaseRef, HeadBranch, "origin/main", Now));

        Assert.True(green.Passes, green.Message);
        Assert.Equal(NightGateRules.PassCase, green.Case);
    }

    /// <summary>
    /// D-538, D-547. A success record of a night on the head branch passes the gate when the record of main fails,
    /// and the record of main still wins when it passes.
    /// </summary>
    [Fact]
    public void BranchNightAtTheEffectiveHeadPassesTheGate()
    {
        string branchNight = Record(Now.AddHours(-2), "success", EffectiveHead);

        NightGateResult branch = NightGateRules.Evaluate(BranchFacts(Record(Now.AddHours(-1), "failure"), commitOnBase: true, branchNight));
        Assert.True(branch.Passes);
        Assert.Equal(NightGateRules.BranchPassCase, branch.Case);
        Assert.Contains(EffectiveHead, branch.Message, StringComparison.Ordinal);
        Assert.Contains("night-branch/" + HeadBranch, branch.Message, StringComparison.Ordinal);

        NightGateResult main = NightGateRules.Evaluate(BranchFacts(Record(Now.AddHours(-1), "success"), commitOnBase: true, branchNight));
        Assert.True(main.Passes);
        Assert.Equal(NightGateRules.PassCase, main.Case);
    }

    /// <summary>
    /// D-538, D-547. A record of the head branch that is absent, stale, red, or at a commit before the effective head
    /// passes nothing. The case stays the case of the record of main, and the message names both records (T-2).
    /// </summary>
    [Fact]
    public void BranchNightFailsOffTheEffectiveHeadStaleOrRed()
    {
        string mainRed = Record(Now.AddHours(-1), "failure");
        (NightGateFacts Facts, string Names)[] failures =
        [
            (BranchFacts(mainRed, commitOnBase: true, null), "has no branch night-branch/" + HeadBranch),
            (BranchFacts(mainRed, commitOnBase: true, Record(Now.AddHours(-2), "success"), branchAtEffectiveHead: false), "is not the effective head " + EffectiveHead),
            (BranchFacts(mainRed, commitOnBase: true, Record(Now.AddHours(-49), "success", EffectiveHead)), "is stale"),
            (BranchFacts(mainRed, commitOnBase: true, Record(Now.AddHours(-2), "failure", EffectiveHead)), "holds a failed night"),
            (BranchFacts(mainRed, commitOnBase: true, Record(Now.AddHours(-2), "cancelled", EffectiveHead)), "holds a cancelled night"),
        ];
        foreach ((NightGateFacts facts, string names) in failures)
        {
            NightGateResult result = NightGateRules.Evaluate(facts);
            Assert.False(result.Passes);
            Assert.Equal(NightGateRules.FailedCase, result.Case);
            Assert.Contains("night-results holds a failed night", result.Message, StringComparison.Ordinal);
            Assert.Contains(names, result.Message, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// D-538, D-547 over real repositories. A night record on the branch night-branch/&lt;head branch&gt; counts at the
    /// effective head and at a later documents commit, and not at an earlier code commit. The record of main stays
    /// as it was.
    /// </summary>
    [Fact]
    public void BranchNightCountsFromTheEffectiveHeadAndNeverReplacesMain()
    {
        using var remote = new TemporaryGitRepository();
        string onMain = remote.Commit("feat: on main", Files(("a.txt", "a")));
        remote.CreateBranch(HeadBranch);
        string firstCode = remote.Commit("fix: first code", Files(("b.cs", "b")));
        string code = remote.Commit("fix: code", Files(("c.cs", "c")));
        string documents = remote.Commit("docs: handoff", Files(("docs/note.md", "note")));
        remote.Git(["checkout", "-q", "main"]);
        using TemporaryGitRepository local = CloneOf(remote);
        PublishNight(remote, ("night.json", Record(Now.AddHours(-1), "failure", onMain)));
        string head = "origin/" + HeadBranch;
        string branchRecord = NightGateFacts.BranchRecordPrefix + HeadBranch;

        foreach (string commit in new[] { code, documents })
        {
            PublishNightOn(remote, branchRecord, ("night.json", Record(Now.AddHours(-1), "success", commit)));
            NightGateFacts facts = NightGateFacts.Gather(local.Path, "origin", BaseRef, HeadBranch, head, Now);
            NightGateResult result = NightGateRules.Evaluate(facts);
            Assert.True(result.Passes, result.Message);
            Assert.Equal(NightGateRules.BranchPassCase, result.Case);
            Assert.Equal(code, facts.EffectiveHead);
            Assert.Equal("failure", facts.Main.Record!.Status);
        }

        PublishNightOn(remote, branchRecord, ("night.json", Record(Now.AddHours(-1), "success", firstCode)));
        NightGateResult early = NightGateRules.Evaluate(NightGateFacts.Gather(local.Path, "origin", BaseRef, HeadBranch, head, Now));
        Assert.False(early.Passes);
        Assert.Contains(firstCode, early.Message, StringComparison.Ordinal);
        Assert.Contains("is not the effective head " + code, early.Message, StringComparison.Ordinal);
    }

    /// <summary>The records of the first two nights start with a byte-order mark, and the parser reads them.</summary>
    [Fact]
    public void NightRecordParserAcceptsAByteOrderMark()
    {
        NightRecord? record = NightRecordParser.TryParse("\uFEFF" + Record(Now.AddHours(-1), "success"), out string error);

        Assert.True(record is not null, error);
        Assert.Equal(Commit, record!.Commit);
        Assert.Equal("success", record.Status);
    }

    /// <summary>The command exits 0 on a pass, 1 on a failure, and 2 on a wrong option or time. A planted record in the checkout changes nothing.</summary>
    [Fact]
    public void NightGateCommandReportsEachExitCode()
    {
        using var remote = new TemporaryGitRepository();
        string onMain = remote.Commit("feat: on main", Files(("a.txt", "a")));
        using TemporaryGitRepository local = CloneOf(remote);
        File.WriteAllText(Path.Combine(local.Path, "night.json"), Record(Now.AddHours(-1), "success", onMain));
        string[] tail = ["--root", local.Path, "--remote", "origin", "--base", BaseRef, "--head-branch", HeadBranch, "--head", "origin/main", "--now", "2026-09-11T12:00:00Z"];

        Assert.Equal(1, Program.Main(["night-gate", .. tail]));

        PublishNight(remote, ("night.json", Record(Now.AddHours(-1), "success", onMain)));
        Assert.Equal(0, Program.Main(["night-gate", .. tail]));

        PublishNight(remote, ("night.json", Record(Now.AddHours(-1), "failure", onMain)));
        Assert.Equal(1, Program.Main(["night-gate", .. tail]));

        Assert.Equal(2, Program.Main(["night-gate", "--root", local.Path, "--remote", "origin", "--base", BaseRef]));
        Assert.Equal(2, Program.Main(["night-gate", "--root", local.Path, "--remote", "origin", "--base", BaseRef, "--head-branch", HeadBranch, "--now", "2026-09-11T12:00:00Z"]));
        Assert.Equal(2, Program.Main(["night-gate", "--root", local.Path, "--remote", "origin", "--base", BaseRef, "--now", "yesterday"]));
        Assert.Equal(2, Program.Main(["night-gate", "--root"]));
    }

    /// <summary>
    /// D-555 to D-558, the regression of the PR-81 case. The record of main failed at the base. The branch night
    /// passed at the code commit of the PR. The squash merge adds only paths of the skip set of D-475. The branch night
    /// promotes: the record names the merge commit, keeps the end time of the night, and names its source. The gate of
    /// the next PR then reads main green, where PR #98 read it red.
    /// </summary>
    [Fact]
    public void BranchNightOfTheMergedPrPromotesAndTheNextPrReadsGreen()
    {
        using var remote = new TemporaryGitRepository();
        string onBase = remote.Commit("feat: base", Files(("a.cs", "a")));
        remote.CreateBranch(HeadBranch);
        string night = remote.Commit("fix: code", Files(("b.cs", "b")));
        remote.Git(["checkout", "-q", "main"]);
        string merge = remote.Commit("fix: code (#97)", Files(("b.cs", "b"), ("docs/decisions.md", "d"), (".claude/skills/s/SKILL.md", "s"), ("CLAUDE.md", "c"), ("AGENTS.md", "c")));
        DateTimeOffset nightEnd = Now.AddHours(-9);
        PublishNight(remote, ("night.json", Record(Now.AddHours(-20), "failure", onBase)));
        PublishNightOn(remote, NightGateFacts.BranchRecordPrefix + HeadBranch, ("night.json", Record(nightEnd, "success", night)));
        using TemporaryGitRepository local = CloneOf(remote);

        NightPromotionFacts facts = NightPromotionFacts.Gather(local.Path, "origin", BaseRef, HeadBranch, Now);
        NightPromotionResult result = NightPromotionRules.Evaluate(facts);
        Assert.True(result.Promotes, result.Message);
        Assert.Equal(NightPromotionRules.PromoteCase, result.Case);
        Assert.Contains(night, result.Message, StringComparison.Ordinal);
        Assert.Contains(onBase, result.Message, StringComparison.Ordinal);

        string promoted = NightPromotionRules.PromotedRecord(facts);
        NightRecord? record = NightRecordParser.TryParse(promoted, out string error);
        Assert.True(record is not null, error);
        Assert.Equal(merge, record!.Commit);
        Assert.Equal(nightEnd, record.EndedAt);
        Assert.Equal("success", record.Status);
        Assert.Contains($"\"{NightPromotionRules.PromotedFromName}\":{{\"branch\":\"{HeadBranch}\",\"commit\":\"{night}\"}}", promoted, StringComparison.Ordinal);

        PublishNight(remote, ("night.json", promoted));
        NightGateResult nextPr = NightGateRules.Evaluate(NightGateFacts.Gather(local.Path, "origin", BaseRef, "chore/next", "origin/main", Now));
        Assert.True(nextPr.Passes, nextPr.Message);
        Assert.Equal(NightGateRules.PassCase, nextPr.Case);
        Assert.Contains(merge, nextPr.Message, StringComparison.Ordinal);
    }

    /// <summary>D-555. A merge commit that also holds a code change of another PR does not promote, and the message names the path.</summary>
    [Fact]
    public void BranchNightDoesNotPromoteOverACodeChangeOfAnotherPr()
    {
        using var remote = new TemporaryGitRepository();
        string onBase = remote.Commit("feat: base", Files(("a.cs", "a")));
        remote.CreateBranch(HeadBranch);
        string night = remote.Commit("fix: code", Files(("b.cs", "b")));
        remote.Git(["checkout", "-q", "main"]);
        remote.Commit("feat: another PR (#96)", Files(("c.cs", "c")));
        remote.Commit("fix: code (#97)", Files(("b.cs", "b"), ("docs/decisions.md", "d")));
        PublishNight(remote, ("night.json", Record(Now.AddHours(-20), "failure", onBase)));
        PublishNightOn(remote, NightGateFacts.BranchRecordPrefix + HeadBranch, ("night.json", Record(Now.AddHours(-9), "success", night)));
        using TemporaryGitRepository local = CloneOf(remote);

        NightPromotionResult result = NightPromotionRules.Evaluate(NightPromotionFacts.Gather(local.Path, "origin", BaseRef, HeadBranch, Now));

        Assert.False(result.Promotes);
        Assert.Equal(NightPromotionRules.CodeChangedCase, result.Case);
        Assert.Contains("c.cs", result.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("decisions.md", result.Message, StringComparison.Ordinal);
    }

    /// <summary>D-556. A branch night older than 48 hours does not promote, and one at exactly 48 hours does.</summary>
    [Fact]
    public void BranchNightOlderThanTheWindowDoesNotPromote()
    {
        NightPromotionResult stale = NightPromotionRules.Evaluate(PromotionFacts(Record(Now.AddHours(-48).AddSeconds(-1), "success", EffectiveHead), mainBeforeMerge: true));
        NightPromotionResult fresh = NightPromotionRules.Evaluate(PromotionFacts(Record(Now.AddHours(-48), "success", EffectiveHead), mainBeforeMerge: true));

        Assert.False(stale.Promotes);
        Assert.Equal(NightPromotionRules.BranchStaleCase, stale.Case);
        Assert.Contains(EffectiveHead, stale.Message, StringComparison.Ordinal);
        Assert.True(fresh.Promotes, fresh.Message);
    }

    /// <summary>F-125. A branch night that ends after the time of the evaluation does not promote, and one that ends at that time does.</summary>
    [Fact]
    public void BranchNightThatEndsInTheFutureDoesNotPromote()
    {
        NightPromotionResult future = NightPromotionRules.Evaluate(PromotionFacts(Record(Now.AddYears(1), "success", EffectiveHead), mainBeforeMerge: true));
        NightPromotionResult present = NightPromotionRules.Evaluate(PromotionFacts(Record(Now, "success", EffectiveHead), mainBeforeMerge: true));

        Assert.False(future.Promotes);
        Assert.Equal(NightPromotionRules.BranchFutureCase, future.Case);
        Assert.Contains("ended at 2027-09-11T12:00:00Z", future.Message, StringComparison.Ordinal);
        Assert.Contains("later than 2026-09-11T12:00:00Z", future.Message, StringComparison.Ordinal);
        Assert.True(present.Promotes, present.Message);
    }

    /// <summary>
    /// F-125. The night-promote step maps exit 1 to no promotion, so the tools build in a step of their own, and the
    /// step runs the built tool. A restore or compile failure then fails the job and never reads as no promotion.
    /// </summary>
    [Fact]
    public void NightPromoteWorkflowBuildsTheToolsBeforeItRuns()
    {
        string workflow = RepositoryRoot.ReadFile(".github/workflows/night-promote.yml");
        int build = workflow.IndexOf("- name: Build the tools", StringComparison.Ordinal);
        int promote = workflow.IndexOf("- name: Promote the branch night", StringComparison.Ordinal);
        Assert.True(build >= 0, "The workflow has no step 'Build the tools'.");
        Assert.True(promote > build, "The step 'Build the tools' does not come before the step 'Promote the branch night'.");

        string buildStep = workflow[build..promote];
        Assert.Contains("if: steps.pr.outputs.number != ''", buildStep, StringComparison.Ordinal);
        Assert.Contains("run: dotnet build WhatYouCarry.Tools/WhatYouCarry.Tools.csproj\n", buildStep, StringComparison.Ordinal);
        Assert.Contains("dotnet run --project WhatYouCarry.Tools/WhatYouCarry.Tools.csproj --no-build -- night-promote", workflow[promote..], StringComparison.Ordinal);
        Assert.Equal(1, workflow.Split("dotnet run").Length - 1);
    }

    /// <summary>
    /// D-558. A record of main at the merge commit or at a later commit stays, success or failure. A newer failed
    /// night on main is never replaced.
    /// </summary>
    [Fact]
    public void BranchNightNeverReplacesANewerNightOfMain()
    {
        using var remote = new TemporaryGitRepository();
        remote.Commit("feat: base", Files(("a.cs", "a")));
        remote.CreateBranch(HeadBranch);
        string night = remote.Commit("fix: code", Files(("b.cs", "b")));
        remote.Git(["checkout", "-q", "main"]);
        string merge = remote.Commit("fix: code (#97)", Files(("b.cs", "b")));
        string later = remote.Commit("docs: later", Files(("docs/note.md", "n")));
        PublishNightOn(remote, NightGateFacts.BranchRecordPrefix + HeadBranch, ("night.json", Record(Now.AddHours(-9), "success", night)));
        using TemporaryGitRepository local = CloneOf(remote);

        foreach (string commit in new[] { merge, later })
        {
            PublishNight(remote, ("night.json", Record(Now.AddHours(-1), "failure", commit)));
            NightPromotionFacts facts = NightPromotionFacts.Gather(local.Path, "origin", merge, HeadBranch, Now);
            NightPromotionResult result = NightPromotionRules.Evaluate(facts);
            Assert.False(result.Promotes);
            Assert.Equal(NightPromotionRules.MainKeptCase, result.Case);
            Assert.Contains(commit, result.Message, StringComparison.Ordinal);
            Assert.False(facts.MainBeforeMerge);
        }
    }

    /// <summary>Each case that keeps the record of main names itself and the commit, and an absent record of main takes the promotion (T-2).</summary>
    [Fact]
    public void EveryCaseThatKeepsTheRecordNamesItself()
    {
        string night = Record(Now.AddHours(-2), "success", EffectiveHead);
        (NightPromotionFacts Facts, string Case, string Names)[] kept =
        [
            (PromotionFacts(null, mainBeforeMerge: true), NightPromotionRules.BranchAbsentCase, "has no branch night-branch/" + HeadBranch),
            (PromotionFacts("not json", mainBeforeMerge: true), NightPromotionRules.BranchMalformedCase, "not JSON"),
            (PromotionFacts(Record(Now.AddHours(-2), "failure", EffectiveHead), mainBeforeMerge: true), NightPromotionRules.BranchNotSuccessCase, "status failure"),
            (PromotionFacts(Record(Now.AddHours(-2), "cancelled", EffectiveHead), mainBeforeMerge: true), NightPromotionRules.BranchNotSuccessCase, "status cancelled"),
            (PromotionFacts(night, mainBeforeMerge: true, branchCommitKnown: false), NightPromotionRules.BranchCommitUnknownCase, "lacks the commit"),
            (PromotionFacts(night, mainBeforeMerge: true, codePaths: ["WhatYouCarry.Core/A.cs"]), NightPromotionRules.CodeChangedCase, "WhatYouCarry.Core/A.cs"),
            (PromotionFacts(night, mainBeforeMerge: null, mainText: "[]"), NightPromotionRules.MainMalformedCase, "object"),
            (PromotionFacts(night, mainBeforeMerge: false), NightPromotionRules.MainKeptCase, Commit),
        ];
        foreach ((NightPromotionFacts facts, string expectedCase, string names) in kept)
        {
            NightPromotionResult result = NightPromotionRules.Evaluate(facts);
            Assert.False(result.Promotes, expectedCase);
            Assert.Equal(expectedCase, result.Case);
            Assert.Contains(names, result.Message, StringComparison.Ordinal);
        }

        Assert.Throws<InvalidOperationException>(() => NightPromotionRules.PromotedRecord(PromotionFacts(null, mainBeforeMerge: true)));

        NightPromotionResult first = NightPromotionRules.Evaluate(PromotionFacts(night, mainBeforeMerge: null, mainText: null));
        Assert.True(first.Promotes, first.Message);
        Assert.Contains("record of main is absent", first.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// PR-84 exit test 9. A failed record of main that names a slice seed stays until a branch night that ran the seed
    /// promotes. A branch night of another date, or of a night before PR-84, did not run it. A seed of the fixed range
    /// or of the slice of the branch night needs no carry (D-569).
    /// </summary>
    [Fact]
    public void ABranchNightPromotesOnlyWhenItRanEachFailedSeedOfMain()
    {
        DateOnly date = NightSeeds.DayZero.AddDays(1);
        Dictionary<string, List<ulong>> noCarry = new(StringComparer.Ordinal);
        Dictionary<string, List<ulong>> mainFailures = Ended();
        mainFailures[GreedyDescender.PolicyName] = [5600];
        string mainText = NightRecordCommand.Build(Commit, Now.AddHours(-20).UtcDateTime, "failure", string.Empty, NightSeeds.RecordFields(date, "failure", mainFailures, noCarry));
        Dictionary<string, List<ulong>> carry = NightSeeds.ReadRecordSeeds(mainText, NightSeeds.FailedSeedsName, "main");

        string ranNoSeed = NightRecordCommand.Build(EffectiveHead, Now.AddHours(-2).UtcDateTime, "success", string.Empty, NightSeeds.RecordFields(date.AddDays(1), "success", Ended(), noCarry));
        string olderForm = Record(Now.AddHours(-2), "success", EffectiveHead);
        string ranTheSeed = NightRecordCommand.Build(EffectiveHead, Now.AddHours(-2).UtcDateTime, "success", string.Empty, NightSeeds.RecordFields(date.AddDays(1), "success", Ended(), carry));

        foreach (string branch in new[] { ranNoSeed, olderForm })
        {
            NightPromotionResult kept = NightPromotionRules.Evaluate(PromotionFacts(branch, mainBeforeMerge: true, mainText: mainText));
            Assert.False(kept.Promotes, kept.Message);
            Assert.Equal(NightPromotionRules.CarryMissingCase, kept.Case);
            Assert.Contains("greedy-descender 5600", kept.Message, StringComparison.Ordinal);
        }

        NightPromotionResult promoted = NightPromotionRules.Evaluate(PromotionFacts(ranTheSeed, mainBeforeMerge: true, mainText: mainText));
        Assert.True(promoted.Promotes, promoted.Message);

        // The branch night ran its fixed range and its slice too: a fixed seed, or a seed of the slice of its date, needs no carry.
        Dictionary<string, List<ulong>> fixedFailure = Ended();
        fixedFailure[GreedyDescender.PolicyName] = [2669];
        string mainFixed = NightRecordCommand.Build(Commit, Now.AddHours(-20).UtcDateTime, "failure", string.Empty, NightSeeds.RecordFields(date, "failure", fixedFailure, noCarry));
        string sameDate = NightRecordCommand.Build(EffectiveHead, Now.AddHours(-2).UtcDateTime, "success", string.Empty, NightSeeds.RecordFields(date, "success", Ended(), noCarry));
        Assert.True(NightPromotionRules.Evaluate(PromotionFacts(olderForm, mainBeforeMerge: true, mainText: mainFixed)).Promotes);
        Assert.True(NightPromotionRules.Evaluate(PromotionFacts(sameDate, mainBeforeMerge: true, mainText: mainText)).Promotes);

        // A record of main of a night before PR-84 names no failed seed, so the rule of D-558 alone decides.
        NightPromotionResult older = NightPromotionRules.Evaluate(PromotionFacts(olderForm, mainBeforeMerge: true));
        Assert.True(older.Promotes, older.Message);

        NightPromotionResult malformed = NightPromotionRules.Evaluate(PromotionFacts(ranTheSeed, mainBeforeMerge: true, mainText: "{\"commit\":\"" + Commit + "\",\"endedAt\":\"2026-09-11T00:00:00Z\",\"status\":\"failure\",\"failedSeeds\":[5600]}"));
        Assert.False(malformed.Promotes);
        Assert.Equal(NightPromotionRules.MainMalformedCase, malformed.Case);
    }

    /// <summary>A map of failure lines where every sweep ended with no failure.</summary>
    private static Dictionary<string, List<ulong>> Ended()
    {
        Dictionary<string, List<ulong>> failures = new(StringComparer.Ordinal);
        foreach (string sweep in NightSeeds.Sweeps)
        {
            failures[sweep] = [];
        }

        return failures;
    }

    /// <summary>The command exits 0 and writes the record on a promotion, 1 and writes nothing without one, and 2 on a wrong option.</summary>
    [Fact]
    public void NightPromoteCommandReportsEachExitCode()
    {
        using var remote = new TemporaryGitRepository();
        string onBase = remote.Commit("feat: base", Files(("a.cs", "a")));
        remote.CreateBranch(HeadBranch);
        string night = remote.Commit("fix: code", Files(("b.cs", "b")));
        remote.Git(["checkout", "-q", "main"]);
        string merge = remote.Commit("fix: code (#97)", Files(("b.cs", "b")));
        PublishNight(remote, ("night.json", Record(Now.AddHours(-20), "failure", onBase)));
        using TemporaryGitRepository local = CloneOf(remote);
        string output = Path.Combine(local.Path, "promoted.json");
        string[] tail = ["--root", local.Path, "--remote", "origin", "--merge", "origin/main", "--head-branch", HeadBranch, "--now", "2026-09-11T12:00:00Z", "--output", output];

        Assert.Equal(1, Program.Main(["night-promote", .. tail]));
        Assert.False(File.Exists(output));

        PublishNightOn(remote, NightGateFacts.BranchRecordPrefix + HeadBranch, ("night.json", Record(Now.AddHours(-9), "success", night)));
        Assert.Equal(0, Program.Main(["night-promote", .. tail]));
        Assert.Equal(merge, NightRecordParser.TryParse(File.ReadAllText(output), out _)!.Commit);

        Assert.Equal(2, Program.Main(["night-promote", "--root", local.Path, "--remote", "origin", "--merge", "origin/main"]));
        Assert.Equal(2, Program.Main(["night-promote", .. tail[..^2], "--now", "yesterday", "--output", output]));
        Assert.Equal(2, Program.Main(["night-promote", "--root"]));
    }

    /// <summary>
    /// D-562, D-648. A night on main keeps a record at a later commit, which a promotion wrote while the night ran,
    /// unless the night failed a seed that the record does not name. It replaces a record at its own commit or an
    /// earlier one, and it writes the first record. The command reads the record of main at the lease alone.
    /// </summary>
    [Fact]
    public void NightOnMainKeepsARecordAtALaterCommit()
    {
        using var remote = new TemporaryGitRepository();
        string earlier = remote.Commit("feat: earlier", Files(("a.cs", "a")));
        string nightCommit = remote.Commit("feat: night", Files(("b.cs", "b")));
        string later = remote.Commit("fix: merged later (#97)", Files(("c.cs", "c")));
        using TemporaryGitRepository local = CloneOf(remote);
        string directory = Path.Combine(Path.GetTempPath(), $"wyc-publish-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            string night = Path.Combine(directory, "night.json");
            string failures = Path.Combine(directory, "seed-failures.txt");
            string output = Path.Combine(directory, "night-publish.json");
            File.WriteAllText(night, SeedRecord(nightCommit, NightDate, "success", EndedWith(), NoSeeds()));
            File.WriteAllText(failures, FailureLines(EndedWith()));
            string[] Args(string lease) => ["night-publish-check", "--root", local.Path, "--remote", "origin", "--commit", nightCommit, "--lease", lease, "--night", night, "--failures", failures, "--output", output];

            Assert.Equal(0, Program.Main(Args(string.Empty)));
            Assert.Equal(File.ReadAllText(night), File.ReadAllText(output));

            PublishNight(remote, ("night.json", Record(Now.AddHours(-1), "success", later)));
            File.Delete(output);
            Assert.Equal(1, Program.Main(Args(RecordHead(remote))));
            Assert.False(File.Exists(output));

            foreach (string commit in new[] { nightCommit, earlier })
            {
                PublishNight(remote, ("night.json", Record(Now.AddHours(-1), "success", commit)));
                Assert.Equal(0, Program.Main(Args(RecordHead(remote))));
                Assert.Equal(File.ReadAllText(night), File.ReadAllText(output));
            }

            PublishNight(remote, ("night.json", "not json"));
            Assert.Equal(0, Program.Main(Args(RecordHead(remote))));

            // D-648: a lease that is not the head of night-results is a fault, and the command writes nothing.
            string stale = RecordHead(remote);
            PublishNight(remote, ("night.json", Record(Now.AddHours(-1), "success", earlier)));
            File.Delete(output);
            Assert.Equal(3, Program.Main(Args(stale)));
            Assert.Equal(3, Program.Main(Args(string.Empty)));
            Assert.False(File.Exists(output));

            // D-648, trigger (b): the night failed a slice seed that the later record does not name, so a failure
            // record at the later commit holds it.
            PublishNight(remote, ("night.json", SeedRecord(later, NightDate, "success", EndedWith(), NoSeeds())));
            File.WriteAllText(night, SeedRecord(nightCommit, NightDate, "failure", EndedWith((FullClearer.PolicyName, 6620)), NoSeeds()));
            File.WriteAllText(failures, FailureLines(EndedWith((FullClearer.PolicyName, 6620))));
            Assert.Equal(0, Program.Main(Args(RecordHead(remote))));
            string published = File.ReadAllText(output);
            Assert.Equal(later, NightRecordParser.TryParse(published, out _)!.Commit);
            Assert.Equal("failure", NightRecordParser.TryParse(published, out _)!.Status);
            Assert.Equal(new ulong[] { 6620 }, FailedOf(published, FullClearer.PolicyName));

            Assert.Equal(2, Program.Main(["night-publish-check", "--root", local.Path, "--remote", "origin"]));
            Assert.Equal(2, Program.Main(Args("abc")));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    /// <summary>The publish check names each case, and keeps a record only at a later commit (D-562, D-648, T-2).</summary>
    [Fact]
    public void NightPublishDecisionNamesEachCase()
    {
        string nightText = SeedRecord(EffectiveHead, NightDate, "success", EndedWith(), NoSeeds());
        Dictionary<string, List<ulong>> ended = EndedWith();
        NightPublishDecision absent = NightPublishCheckCommand.Decide(Read(NightGateFacts.RecordBranch, null), null, EffectiveHead, nightText, ended, Now);
        NightPublishDecision malformed = NightPublishCheckCommand.Decide(Read(NightGateFacts.RecordBranch, "[]"), null, EffectiveHead, nightText, ended, Now);
        NightPublishDecision later = NightPublishCheckCommand.Decide(Read(NightGateFacts.RecordBranch, Record(Now, "failure")), true, EffectiveHead, nightText, ended, Now);
        NightPublishDecision earlier = NightPublishCheckCommand.Decide(Read(NightGateFacts.RecordBranch, Record(Now, "success")), false, EffectiveHead, nightText, ended, Now);

        Assert.True(absent.Writes);
        Assert.Equal(nightText, absent.Record);
        Assert.Contains("absent", absent.Message, StringComparison.Ordinal);
        Assert.True(malformed.Writes);
        Assert.Equal(nightText, malformed.Record);
        Assert.Contains("malformed", malformed.Message, StringComparison.Ordinal);
        Assert.False(later.Writes);
        Assert.Null(later.Record);
        Assert.Contains(Commit, later.Message, StringComparison.Ordinal);
        Assert.Contains("D-562", later.Message, StringComparison.Ordinal);
        Assert.True(earlier.Writes);
        Assert.Equal(nightText, earlier.Record);
        Assert.Contains(EffectiveHead, earlier.Message, StringComparison.Ordinal);

        // A night record that is not a record, or names another commit, is a fault and never a publish (T-2).
        Assert.Throws<FormatException>(() => NightPublishCheckCommand.Decide(Read(NightGateFacts.RecordBranch, null), null, EffectiveHead, "not json", ended, Now));
        Assert.Throws<FormatException>(() => NightPublishCheckCommand.Decide(Read(NightGateFacts.RecordBranch, null), null, Commit, nightText, ended, Now));
    }

    /// <summary>
    /// D-648, RR-P2-5 trigger (a). Night B at one commit failed a slice seed, and its record is the record of main. A
    /// re-run of the failed jobs of an older night A at the same commit then publishes. The plan of A read the record
    /// of main before B, so A did not run the seed. The old step published the success record of A, and the seed was
    /// lost. The record now keeps the seed and reads failure.
    /// </summary>
    [Fact]
    public void ARerunAtOneCommitKeepsTheFailedSeedOfTheOtherNight()
    {
        DateOnly dateB = NightDate.AddDays(1);
        ulong seed = NightSeeds.Slice(GreedyDescender.PolicyName, dateB).From + 9;
        string recordB = SeedRecord(EffectiveHead, dateB, "failure", EndedWith((GreedyDescender.PolicyName, seed)), NoSeeds());
        string recordA = SeedRecord(EffectiveHead, NightDate, "success", EndedWith(), NoSeeds());

        NightPublishDecision decision = NightPublishCheckCommand.Decide(Read(NightGateFacts.RecordBranch, recordB), false, EffectiveHead, recordA, EndedWith(), Now);

        Assert.True(decision.Writes);
        Assert.Equal("failure", NightRecordParser.TryParse(decision.Record!, out _)!.Status);
        Assert.Equal(new[] { seed }, FailedOf(decision.Record!, GreedyDescender.PolicyName));
        Assert.Equal(EffectiveHead, NightRecordParser.TryParse(decision.Record!, out _)!.Commit);
        Assert.Contains($"{GreedyDescender.PolicyName} {seed}", decision.Message, StringComparison.Ordinal);
        Assert.Contains("D-648", decision.Message, StringComparison.Ordinal);

        // The slice, the carried seeds, and the counts of night A stay as night A wrote them.
        Assert.Equal(NightSeeds.ReadSlices(recordA, "A"), NightSeeds.ReadSlices(decision.Record!, "published"));
        foreach (string sweep in NightSeeds.Sweeps)
        {
            Assert.Equal(NightSeeds.SeedsOf(NightSeeds.ReadRecordSeeds(recordA, NightSeeds.CarriedSeedsName, "A"), sweep), NightSeeds.SeedsOf(NightSeeds.ReadRecordSeeds(decision.Record!, NightSeeds.CarriedSeedsName, "published"), sweep));
        }
    }

    /// <summary>
    /// D-648, RR-P2-5 trigger (b). A night on main started at M0, a PR merged at M1, and a promotion wrote a success
    /// record at M1 (D-557). The M0 night then failed a slice seed. The old check kept the record at M1 (D-562), and no
    /// record named the seed. The step now writes a failure record at M1 with the failed seeds of both records and the
    /// other fields of the record at M1.
    /// </summary>
    [Fact]
    public void ALaterRecordTakesTheNewFailedSeedOfTheNight()
    {
        ulong sliceSeed = NightSeeds.Slice(FullClearer.PolicyName, NightDate).From + 120;
        ulong reachSeed = NightSeeds.Slice(NightSeeds.ReachabilitySweep, NightDate).To;
        Dictionary<string, List<ulong>> laterFailed = EndedWith((FullClearer.PolicyName, sliceSeed + 3), (Coward.PolicyName, 5100));
        string promoted = PromotedRecord(SeedRecord(Commit, NightDate, "failure", laterFailed, NoSeeds(), "coward=2 ascends=4998 scavenger:2\n"));
        Dictionary<string, List<ulong>> ended = EndedWith((FullClearer.PolicyName, sliceSeed + 3), (FullClearer.PolicyName, sliceSeed), (NightSeeds.ReachabilitySweep, reachSeed));
        string nightText = SeedRecord(EffectiveHead, NightDate, "failure", ended, NoSeeds());
        DateTimeOffset publishTime = Now.AddMinutes(7);

        NightPublishDecision decision = NightPublishCheckCommand.Decide(Read(NightGateFacts.RecordBranch, promoted), true, EffectiveHead, nightText, ended, publishTime);

        Assert.True(decision.Writes);
        NightRecord record = NightRecordParser.TryParse(decision.Record!, out string error) ?? throw new InvalidOperationException(error);
        Assert.Equal(Commit, record.Commit);
        Assert.Equal("failure", record.Status);
        Assert.Equal(publishTime, record.EndedAt);
        Assert.Equal(new[] { sliceSeed, sliceSeed + 3 }, FailedOf(decision.Record!, FullClearer.PolicyName));
        Assert.Equal(new ulong[] { 5100 }, FailedOf(decision.Record!, Coward.PolicyName));
        Assert.Equal(new[] { reachSeed }, FailedOf(decision.Record!, NightSeeds.ReachabilitySweep));
        Assert.Empty(FailedOf(decision.Record!, GreedyDescender.PolicyName));
        Assert.Contains($"{FullClearer.PolicyName} {sliceSeed}", decision.Message, StringComparison.Ordinal);
        Assert.DoesNotContain($"{FullClearer.PolicyName} {sliceSeed + 3}", decision.Message, StringComparison.Ordinal);

        // Each other field of the record at M1 stays as it is: the slice, the carried seeds, the counts, and the source.
        using JsonDocument before = JsonDocument.Parse(promoted);
        using JsonDocument after = JsonDocument.Parse(decision.Record!);
        foreach (JsonProperty property in before.RootElement.EnumerateObject())
        {
            if (property.Name is NightRecordCommand.EndedAtName or NightRecordCommand.StatusName or NightSeeds.FailedSeedsName)
            {
                continue;
            }

            Assert.Equal(property.Value.GetRawText(), after.RootElement.GetProperty(property.Name).GetRawText());
        }

        Assert.Equal(before.RootElement.EnumerateObject().Count(), after.RootElement.EnumerateObject().Count());
    }

    /// <summary>D-562 stands: a later record that names each seed that the night failed stays, and so does a later record after a night with no failure.</summary>
    [Fact]
    public void ALaterRecordStaysWhenTheNightFailedNoNewSeed()
    {
        ulong seed = NightSeeds.Slice(FullClearer.PolicyName, NightDate).From + 5;
        string laterRecord = SeedRecord(Commit, NightDate, "failure", EndedWith((FullClearer.PolicyName, seed)), NoSeeds());
        Dictionary<string, List<ulong>> sameFailure = EndedWith((FullClearer.PolicyName, seed));
        string failedNight = SeedRecord(EffectiveHead, NightDate, "failure", sameFailure, NoSeeds());
        Assert.False(NightPublishCheckCommand.Decide(Read(NightGateFacts.RecordBranch, laterRecord), true, EffectiveHead, failedNight, sameFailure, Now).Writes);

        string passedNight = SeedRecord(EffectiveHead, NightDate, "success", EndedWith(), NoSeeds());
        string laterSuccess = SeedRecord(Commit, NightDate, "success", EndedWith(), NoSeeds());
        Assert.False(NightPublishCheckCommand.Decide(Read(NightGateFacts.RecordBranch, laterSuccess), true, EffectiveHead, passedNight, EndedWith(), Now).Writes);

        // A sweep that did not end names the carry of its plan as failed seeds, and this night did not fail them. A
        // promotion proved them on the later code (D-569), so the later record stays.
        Dictionary<string, List<ulong>> carry = new(StringComparer.Ordinal) { [GreedyDescender.PolicyName] = [5200] };
        Dictionary<string, List<ulong>> notEnded = EndedWith();
        notEnded.Remove(GreedyDescender.PolicyName);
        string brokenNight = SeedRecord(EffectiveHead, NightDate, "failure", notEnded, carry);
        Assert.Equal(new ulong[] { 5200 }, FailedOf(brokenNight, GreedyDescender.PolicyName));
        Assert.False(NightPublishCheckCommand.Decide(Read(NightGateFacts.RecordBranch, laterSuccess), true, EffectiveHead, brokenNight, notEnded, Now).Writes);
    }

    /// <summary>
    /// D-567, D-648. A failed seed of the record of main that this night ran and passed goes away: a carried seed, and
    /// a seed of the slice of this night. A seed of a sweep that did not end stays, because the sweep did not run it to
    /// the end.
    /// </summary>
    [Fact]
    public void ASeedThatTheNightRanAndPassedGoesAway()
    {
        ulong carriedSeed = NightSeeds.Slice(GreedyDescender.PolicyName, NightDate.AddDays(-1)).From + 1;
        ulong sliceSeed = NightSeeds.Slice(Coward.PolicyName, NightDate).From + 2;
        string mainRecord = SeedRecord(Commit, NightDate.AddDays(-1), "failure", EndedWith((GreedyDescender.PolicyName, carriedSeed), (Coward.PolicyName, sliceSeed)), NoSeeds());
        Dictionary<string, List<ulong>> carry = new(StringComparer.Ordinal) { [GreedyDescender.PolicyName] = [carriedSeed] };
        string passed = SeedRecord(EffectiveHead, NightDate, "success", EndedWith(), carry);

        NightPublishDecision decision = NightPublishCheckCommand.Decide(Read(NightGateFacts.RecordBranch, mainRecord), false, EffectiveHead, passed, EndedWith(), Now);
        Assert.True(decision.Writes);
        Assert.Equal(passed, decision.Record);

        // The coward sweep did not end, so its slice seed stays, and the record reads failure.
        Dictionary<string, List<ulong>> notEnded = EndedWith();
        notEnded.Remove(Coward.PolicyName);
        string broken = SeedRecord(EffectiveHead, NightDate, "failure", notEnded, carry);
        NightPublishDecision kept = NightPublishCheckCommand.Decide(Read(NightGateFacts.RecordBranch, mainRecord), false, EffectiveHead, broken, notEnded, Now);
        Assert.True(kept.Writes);
        Assert.Equal(new[] { sliceSeed }, FailedOf(kept.Record!, Coward.PolicyName));
        Assert.Empty(FailedOf(kept.Record!, GreedyDescender.PolicyName));
        Assert.Equal("failure", NightRecordParser.TryParse(kept.Record!, out _)!.Status);
    }

    /// <summary>D-562 stands: an absent record of main, or one with a malformed record or seed field, is replaced by the night record as it is.</summary>
    [Fact]
    public void AMalformedOrAbsentRecordOfMainIsReplaced()
    {
        string nightText = SeedRecord(EffectiveHead, NightDate, "success", EndedWith(), NoSeeds());
        string badSeeds = Record(Now, "failure").Replace("}\n", ",\"failedSeeds\":[7]}\n", StringComparison.Ordinal);
        Assert.NotNull(NightRecordParser.TryParse(badSeeds, out _));
        foreach (string? mainText in new[] { null, "not json", "{}", badSeeds })
        {
            foreach (bool? after in new bool?[] { null, true, false })
            {
                NightRecordRead main = Read(NightGateFacts.RecordBranch, mainText);
                if (after is not null && main.Record is null)
                {
                    continue;
                }

                NightPublishDecision decision = NightPublishCheckCommand.Decide(main, after, EffectiveHead, nightText, EndedWith(), Now);
                Assert.True(decision.Writes, $"The record of main '{mainText}' stays.");
                Assert.Equal(nightText, decision.Record);
                Assert.Contains(mainText is null ? "absent" : "malformed", decision.Message, StringComparison.Ordinal);
            }
        }
    }

    /// <summary>The date of the nights of the publish tests, three days after day 0 of the slices.</summary>
    private static readonly DateOnly NightDate = NightSeeds.DayZero.AddDays(3);

    /// <summary>A night record with the seed fields of <see cref="NightSeeds.RecordFields"/>, as night-record writes it.</summary>
    private static string SeedRecord(string commit, DateOnly date, string status, Dictionary<string, List<ulong>> ended, Dictionary<string, List<ulong>> carry, string summary = "")
    {
        return NightRecordCommand.Build(commit, Now.UtcDateTime.AddHours(-2), status, summary, NightSeeds.RecordFields(date, status, ended, carry));
    }

    /// <summary>A record at a merge commit with the source field of a promotion, which names the branch night at <see cref="EffectiveHead"/> (D-557).</summary>
    private static string PromotedRecord(string mergeRecord)
    {
        return mergeRecord.Replace("}\n", $",\"{NightPromotionRules.PromotedFromName}\":{{\"branch\":\"{HeadBranch}\",\"commit\":\"{EffectiveHead}\"}}}}\n", StringComparison.Ordinal);
    }

    /// <summary>A map of failure lines where each sweep ended, with the failed seeds given.</summary>
    private static Dictionary<string, List<ulong>> EndedWith(params (string Sweep, ulong Seed)[] failed)
    {
        Dictionary<string, List<ulong>> ended = new(StringComparer.Ordinal);
        foreach (string sweep in NightSeeds.Sweeps)
        {
            ended[sweep] = [];
        }

        foreach ((string sweep, ulong seed) in failed)
        {
            ended[sweep].Add(seed);
        }

        return ended;
    }

    private static Dictionary<string, List<ulong>> NoSeeds()
    {
        return new Dictionary<string, List<ulong>>(StringComparer.Ordinal);
    }

    /// <summary>The failure lines of a map of ended sweeps.</summary>
    private static string FailureLines(Dictionary<string, List<ulong>> ended)
    {
        string text = string.Empty;
        foreach (KeyValuePair<string, List<ulong>> sweep in ended)
        {
            text += NightSeeds.FailureLine(sweep.Key, sweep.Value);
        }

        return text;
    }

    private static List<ulong> FailedOf(string record, string sweep)
    {
        return [.. NightSeeds.SeedsOf(NightSeeds.ReadRecordSeeds(record, NightSeeds.FailedSeedsName, "the record"), sweep)];
    }

    /// <summary>The head commit of the branch night-results of the remote, the lease of a publish.</summary>
    private static string RecordHead(TemporaryGitRepository remote)
    {
        return remote.Git(["rev-parse", NightGateFacts.RecordBranch]).Trim();
    }

    /// <summary>The facts of one promotion, with <see cref="EffectiveHead"/> as the commit of the branch night and a record of main at <see cref="Commit"/>.</summary>
    private static NightPromotionFacts PromotionFacts(string? branchText, bool? mainBeforeMerge, bool branchCommitKnown = true, string[]? codePaths = null, string? mainText = "default")
    {
        return new NightPromotionFacts
        {
            Merge = "fedcba9876543210fedcba9876543210fedcba98",
            HeadBranch = HeadBranch,
            Main = Read(NightGateFacts.RecordBranch, mainText == "default" ? Record(Now.AddHours(-20), "failure") : mainText),
            MainBeforeMerge = mainBeforeMerge,
            Branch = Read(NightGateFacts.BranchRecordPrefix + HeadBranch, branchText),
            BranchCommitKnown = branchText is null ? null : branchCommitKnown,
            CodePaths = branchText is null || !branchCommitKnown ? null : codePaths ?? [],
            Now = Now,
        };
    }

    /// <summary>A checkout with the remote added and fetched, so the base ref of the tests exists in it.</summary>
    private static TemporaryGitRepository CloneOf(TemporaryGitRepository remote)
    {
        var local = new TemporaryGitRepository();
        local.Git(["remote", "add", "origin", remote.Path]);
        local.Git(["fetch", "-q", "origin"]);
        return local;
    }

    /// <summary>Publishes the files as the one commit of the orphan branch night-results of the remote, as the night job does (D-273).</summary>
    private static void PublishNight(TemporaryGitRepository remote, params (string Path, string Content)[] files)
    {
        PublishNightOn(remote, NightGateFacts.RecordBranch, files);
    }

    /// <summary>Publishes the files as the one commit of an orphan record branch of the remote, then goes back to main.</summary>
    private static void PublishNightOn(TemporaryGitRepository remote, string branch, params (string Path, string Content)[] files)
    {
        if (remote.Git(["branch", "--list", branch]).Trim().Length > 0)
        {
            remote.Git(["branch", "-q", "-D", branch]);
        }

        remote.Git(["checkout", "-q", "--orphan", branch]);
        remote.Git(["rm", "-rfq", "--ignore-unmatch", "."]);
        remote.Commit("night", Files(files));
        remote.Git(["checkout", "-q", "main"]);
    }

    /// <summary>The facts of a record of main and no record of the head branch.</summary>
    private static NightGateFacts Facts(string? text, bool? commitOnBase)
    {
        return BranchFacts(text, commitOnBase, null);
    }

    /// <summary>The facts of a record of main and a record of the head branch, with <see cref="EffectiveHead"/> as the effective head of the PR.</summary>
    private static NightGateFacts BranchFacts(string? mainText, bool? commitOnBase, string? branchText, bool? branchAtEffectiveHead = true)
    {
        return new NightGateFacts
        {
            Main = Read(NightGateFacts.RecordBranch, mainText),
            CommitOnBase = commitOnBase,
            BaseRef = BaseRef,
            Branch = Read(NightGateFacts.BranchRecordPrefix + HeadBranch, branchText),
            EffectiveHead = EffectiveHead,
            BranchAtEffectiveHead = branchText is null ? null : branchAtEffectiveHead,
            Now = Now,
        };
    }

    private static NightRecordRead Read(string branch, string? text)
    {
        if (text is null)
        {
            return new NightRecordRead(branch, null, $"the remote 'origin' has no branch {branch}", null, null);
        }

        NightRecord? record = NightRecordParser.TryParse(text, out string error);
        return new NightRecordRead(branch, text, null, record, record is null ? error : null);
    }

    private static string Record(DateTimeOffset endedAt, string status, string commit = Commit)
    {
        return NightRecordCommand.Build(commit, endedAt.UtcDateTime, status, string.Empty);
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

}
