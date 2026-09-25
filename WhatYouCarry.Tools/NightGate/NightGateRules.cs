using System;
using System.Globalization;

namespace WhatYouCarry.Tools.NightGate;

/// <summary>The outcome of the night gate: whether the PR may merge, the case name, and the message that names the case, the commit, and the time (T-2).</summary>
public sealed record NightGateResult(bool Passes, string Case, string Message);

/// <summary>
/// The rules of the night gate (D-115, D-177, D-274, D-275, D-538). The gate passes on a success record from a
/// night that ended inside the window, at a commit on the base branch, whatever event ran the night. It also
/// passes on a success record of a night on the head branch of the PR, inside the window, at the effective head of
/// the PR or a later commit of the PR (D-538, D-547). Every other pair of records fails, and the message names the case of each record.
/// </summary>
public static class NightGateRules
{
    /// <summary>The name of the job and the command.</summary>
    public const string JobName = "night-gate";

    /// <summary>A record older than this is stale (D-177).</summary>
    public static readonly TimeSpan StaleAfter = TimeSpan.FromHours(48);

    /// <summary>The case names, in the order the rules read them.</summary>
    public const string AbsentCase = "absent";
    public const string MalformedCase = "malformed";
    public const string FutureCase = "future";
    public const string StaleCase = "stale";
    public const string ForeignCase = "foreign";
    public const string CancelledCase = "cancelled";
    public const string FailedCase = "failed";
    public const string PassCase = "pass";

    /// <summary>The case of a pass on the record of a night on the head branch (D-538).</summary>
    public const string BranchPassCase = "branch-pass";

    /// <summary>
    /// Reads the record of main first. When it fails, the record of the head branch can pass the gate. When both
    /// fail, the case is the case of the record of main, and the message names the cases of both.
    /// </summary>
    public static NightGateResult Evaluate(NightGateFacts facts)
    {
        NightGateResult main = Judge(facts.Main, facts.CommitOnBase == true, $"on {facts.BaseRef}", facts.Now);
        if (main.Passes)
        {
            return main;
        }

        string where = facts.EffectiveHead is null ? "the effective head of the PR, which has no commit outside the documents" : $"the effective head {facts.EffectiveHead} of the PR, or a later commit of the PR";
        NightGateResult branch = Judge(facts.Branch, facts.BranchAtEffectiveHead == true, where, facts.Now);
        if (branch.Passes)
        {
            return new NightGateResult(true, BranchPassCase, branch.Message);
        }

        return new NightGateResult(false, main.Case, $"{main.Message} {branch.Message}");
    }

    /// <summary>
    /// Applies the rules to one record: absent, malformed, ended after the time of the evaluation, stale, at the wrong
    /// commit, cancelled, or failed, in that order, and a pass after them. <paramref name="where"/> names the commit
    /// that the record must name.
    /// </summary>
    private static NightGateResult Judge(NightRecordRead read, bool atTheCommit, string where, DateTimeOffset nowTime)
    {
        string label = $"night record of the branch {read.Branch}";
        if (read.Text is null)
        {
            return new NightGateResult(false, AbsentCase, $"The {label} is absent: {read.AbsentReason}. No {NightGateFacts.RecordFile} reached the gate from the branch {read.Branch}.");
        }

        if (read.Record is null)
        {
            return new NightGateResult(false, MalformedCase, $"The {label} is malformed: {read.ParseError}.");
        }

        NightRecord record = read.Record;
        string time = record.EndedAt.ToString(NightRecordParser.TimeFormat, CultureInfo.InvariantCulture);
        string now = nowTime.ToString(NightRecordParser.TimeFormat, CultureInfo.InvariantCulture);
        string identity = $"commit {record.Commit}, ended at {time}";
        // A night cannot end after the evaluation. A record that says so would stay inside the window until 48 hours
        // after its false end time (F-125).
        if (record.EndedAt > nowTime)
        {
            return new NightGateResult(false, FutureCase, $"The {label} ends in the future: {identity}, which is later than {now}.");
        }

        if (nowTime - record.EndedAt > StaleAfter)
        {
            return new NightGateResult(false, StaleCase, $"The {label} is stale: {identity}, more than {StaleAfter.TotalHours} hours before {now}.");
        }

        if (!atTheCommit)
        {
            return new NightGateResult(false, ForeignCase, $"The {label} names a commit that is not {where}: {identity}.");
        }

        if (record.Status == "cancelled")
        {
            return new NightGateResult(false, CancelledCase, $"The {label} holds a cancelled night: {identity}.");
        }

        if (record.Status == "failure")
        {
            return new NightGateResult(false, FailedCase, $"The {label} holds a failed night: {identity}.");
        }

        return new NightGateResult(true, PassCase, $"The {label} holds a passed night: {identity}, inside {StaleAfter.TotalHours} hours before {now}, and the commit is {where}.");
    }
}
