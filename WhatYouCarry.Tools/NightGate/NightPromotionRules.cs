using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Nodes;
using WhatYouCarry.Tools.BotRunner;

namespace WhatYouCarry.Tools.NightGate;

/// <summary>The outcome of the promotion: whether the branch night becomes the record of main, the case name, and the message that names the case, the commits, and the time (T-2).</summary>
public sealed record NightPromotionResult(bool Promotes, string Case, string Message);

/// <summary>
/// The rules of the promotion of a branch night to the record of main (D-555 to D-558). A success record of the
/// night on the head branch of a merged PR becomes the record of main at the merge commit when four things hold. The
/// night ended inside the window of D-115. The trees of the night commit and the merge commit differ only in the skip
/// set of D-475. The record of main is absent or names a strict ancestor of the merge commit. The branch night ran
/// each failed seed of the record of main (D-569). The promoted record keeps the end time of the branch night.
/// </summary>
public static class NightPromotionRules
{
    /// <summary>The name of the command.</summary>
    public const string CommandName = "night-promote";

    /// <summary>The name of the field that names the source of a promoted record: the branch and the commit of the branch night (D-557).</summary>
    public const string PromotedFromName = "promotedFrom";

    /// <summary>The case names, in the order the rules read them.</summary>
    public const string BranchAbsentCase = "branch-absent";
    public const string BranchMalformedCase = "branch-malformed";
    public const string BranchNotSuccessCase = "branch-not-success";
    public const string BranchFutureCase = "branch-future";
    public const string BranchStaleCase = "branch-stale";
    public const string BranchCommitUnknownCase = "branch-commit-unknown";
    public const string CodeChangedCase = "code-changed";
    public const string MainMalformedCase = "main-malformed";
    public const string MainKeptCase = "main-kept";
    public const string CarryMissingCase = "carry-missing";
    public const string PromoteCase = "promote";

    /// <summary>
    /// Reads the branch record first, then the trees, then the record of main. The first rule that fails names the
    /// case, and a pass after all of them promotes.
    /// </summary>
    public static NightPromotionResult Evaluate(NightPromotionFacts facts)
    {
        NightRecordRead branch = facts.Branch;
        string label = $"night record of the branch {branch.Branch}";
        if (branch.Text is null)
        {
            return Keep(BranchAbsentCase, $"The {label} is absent: {branch.AbsentReason}. No branch night stands for the merge commit {facts.Merge}.");
        }

        if (branch.Record is null)
        {
            return Keep(BranchMalformedCase, $"The {label} is malformed: {branch.ParseError}.");
        }

        NightRecord night = branch.Record;
        string nightTime = night.EndedAt.ToString(NightRecordParser.TimeFormat, CultureInfo.InvariantCulture);
        string now = facts.Now.ToString(NightRecordParser.TimeFormat, CultureInfo.InvariantCulture);
        string identity = $"commit {night.Commit}, ended at {nightTime}";
        if (night.Status != "success")
        {
            return Keep(BranchNotSuccessCase, $"The {label} holds a night with the status {night.Status}: {identity}.");
        }

        // A night cannot end after the evaluation, so a later end time is false, and the window cannot judge it (F-125).
        if (night.EndedAt > facts.Now)
        {
            return Keep(BranchFutureCase, $"The {label} ends in the future: {identity}, which is later than {now}.");
        }

        if (facts.Now - night.EndedAt > NightGateRules.StaleAfter)
        {
            return Keep(BranchStaleCase, $"The {label} is stale: {identity}, more than {NightGateRules.StaleAfter.TotalHours} hours before {now} (D-556).");
        }

        if (facts.BranchCommitKnown != true || facts.CodePaths is null)
        {
            return Keep(BranchCommitUnknownCase, $"The checkout lacks the commit of the {label}: {identity}. The trees cannot be compared.");
        }

        if (facts.CodePaths.Count > 0)
        {
            return Keep(CodeChangedCase, $"The merge commit {facts.Merge} differs from the {label} ({identity}) in {facts.CodePaths.Count} path(s) outside the skip set of D-475: {string.Join(", ", facts.CodePaths)}. The branch night did not test the code of main (D-555).");
        }

        string tested = $"The {label} ({identity}) tested the code of the merge commit {facts.Merge}, because the two trees differ only in the skip set of D-475.";
        NightRecordRead main = facts.Main;
        if (main.Text is null)
        {
            return new NightPromotionResult(true, PromoteCase, $"{tested} The record of main is absent: {main.AbsentReason}.");
        }

        if (main.Record is null)
        {
            return Keep(MainMalformedCase, $"{tested} The night record of the branch {main.Branch} is malformed: {main.ParseError}. A promotion does not replace a record that nobody can read.");
        }

        NightRecord record = main.Record;
        string mainIdentity = $"commit {record.Commit}, status {record.Status}, ended at {record.EndedAt.ToString(NightRecordParser.TimeFormat, CultureInfo.InvariantCulture)}";
        if (facts.MainBeforeMerge != true)
        {
            return Keep(MainKeptCase, $"{tested} The record of main ({mainIdentity}) is not at a strict ancestor of the merge commit, so it stays (D-558).");
        }

        List<string> missing;
        try
        {
            missing = MissingCarriedSeeds(main, branch);
        }
        catch (FormatException exception)
        {
            return Keep(MainMalformedCase, $"{tested} The seed fields of a record are malformed: {exception.Message}");
        }

        if (missing.Count > 0)
        {
            return Keep(CarryMissingCase, $"{tested} The record of main ({mainIdentity}) names failed seeds that the branch night did not run: {string.Join(", ", missing)}. The next night on main runs them (D-569).");
        }

        return new NightPromotionResult(true, PromoteCase, $"{tested} The record of main ({mainIdentity}) is at an older commit, so the promoted record replaces it (D-558).");
    }

    /// <summary>
    /// Each failed seed of the record of main that the branch night did not run, in the form <c>sweep seed</c> (D-569).
    /// The branch night ran the fixed range, the slice of its record, and the carried seeds of its record. A record of
    /// main of a night before PR-84 names no failed seed.
    /// </summary>
    /// <exception cref="FormatException">A seed field of either record is malformed.</exception>
    private static List<string> MissingCarriedSeeds(NightRecordRead main, NightRecordRead branch)
    {
        Dictionary<string, List<ulong>> failed = NightSeeds.ReadRecordSeeds(main.Text!, NightSeeds.FailedSeedsName, $"the branch {main.Branch}");
        Dictionary<string, List<ulong>> carried = NightSeeds.ReadRecordSeeds(branch.Text!, NightSeeds.CarriedSeedsName, $"the branch {branch.Branch}");
        Dictionary<string, SeedRange> slices = NightSeeds.ReadSlices(branch.Text!, $"the branch {branch.Branch}");
        List<string> missing = [];
        foreach (string sweep in NightSeeds.Sweeps)
        {
            foreach (ulong seed in NightSeeds.SeedsOf(failed, sweep))
            {
                if (!NightSeeds.Ran(sweep, seed, slices, carried))
                {
                    missing.Add($"{sweep} {seed.ToString(CultureInfo.InvariantCulture)}");
                }
            }
        }

        return missing;
    }

    /// <summary>
    /// The promoted record: the branch record with the merge commit in place of the night commit, the end time of the
    /// branch night (D-556), and a field that names the branch and the commit of the night (D-557). The death and
    /// ascend counts of the night stay as they are.
    /// </summary>
    /// <exception cref="InvalidOperationException">The facts hold no branch record.</exception>
    public static string PromotedRecord(NightPromotionFacts facts)
    {
        if (facts.Branch.Text is null || facts.Branch.Record is null)
        {
            throw new InvalidOperationException($"The {CommandName} command has no branch record to promote for the merge commit {facts.Merge}.");
        }

        // The records of the first two nights start with a byte-order mark (NightRecordParser.TryParse).
        string text = facts.Branch.Text.TrimStart('\uFEFF');
        JsonObject record = JsonNode.Parse(text)?.AsObject()
            ?? throw new InvalidOperationException($"The night record of the branch {facts.Branch.Branch} is the JSON null.");
        record[NightRecordCommand.CommitName] = facts.Merge;
        record[PromotedFromName] = new JsonObject
        {
            ["branch"] = facts.HeadBranch,
            ["commit"] = facts.Branch.Record.Commit,
        };
        return record.ToJsonString() + "\n";
    }

    private static NightPromotionResult Keep(string caseName, string message)
    {
        return new NightPromotionResult(false, caseName, message);
    }
}
