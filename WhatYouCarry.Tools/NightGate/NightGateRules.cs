using System;
using System.Globalization;

namespace WhatYouCarry.Tools.NightGate;

/// <summary>The outcome of the night gate: whether the PR may merge, the case name, and the message that names the case, the commit, and the time (T-2).</summary>
public sealed record NightGateResult(bool Passes, string Case, string Message);

/// <summary>
/// The rules of the night gate (D-115, D-177, D-274, D-275). The gate passes on a success record from a night
/// that ended inside the window, at a commit on the base branch, whatever event ran the night. Every other
/// record fails, and the message names the case.
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
    public const string StaleCase = "stale";
    public const string ForeignCase = "foreign";
    public const string CancelledCase = "cancelled";
    public const string FailedCase = "failed";
    public const string PassCase = "pass";

    public static NightGateResult Evaluate(NightGateFacts facts)
    {
        if (facts.RecordText is null)
        {
            return new NightGateResult(false, AbsentCase, $"The night record is absent: {facts.AbsentReason}. No night.json reached the gate from the branch night-results.");
        }

        if (facts.Record is null)
        {
            return new NightGateResult(false, MalformedCase, $"The night record is malformed: {facts.ParseError}.");
        }

        NightRecord record = facts.Record;
        string time = record.EndedAt.ToString(NightRecordParser.TimeFormat, CultureInfo.InvariantCulture);
        string now = facts.Now.ToString(NightRecordParser.TimeFormat, CultureInfo.InvariantCulture);
        string identity = $"commit {record.Commit}, ended at {time}";
        if (facts.Now - record.EndedAt > StaleAfter)
        {
            return new NightGateResult(false, StaleCase, $"The night record is stale: {identity}, more than {StaleAfter.TotalHours} hours before {now}.");
        }

        if (facts.CommitOnBase != true)
        {
            return new NightGateResult(false, ForeignCase, $"The night record names a commit that is not on {facts.BaseRef}: {identity}.");
        }

        if (record.Status == "cancelled")
        {
            return new NightGateResult(false, CancelledCase, $"The night was cancelled: {identity}.");
        }

        if (record.Status == "failure")
        {
            return new NightGateResult(false, FailedCase, $"The night failed: {identity}.");
        }

        return new NightGateResult(true, PassCase, $"The night passed: {identity}, inside {StaleAfter.TotalHours} hours before {now}, at a commit on {facts.BaseRef}.");
    }
}
