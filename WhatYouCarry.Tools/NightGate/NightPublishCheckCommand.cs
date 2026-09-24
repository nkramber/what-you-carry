using System;
using WhatYouCarry.Tools.ReviewGate;

namespace WhatYouCarry.Tools.NightGate;

/// <summary>The decision of the order check: whether a night on main writes its record over the record of main, and the message that names the case (T-2).</summary>
public sealed record NightPublishDecision(bool Writes, string Message);

/// <summary>
/// <c>night-publish-check --root &lt;checkout&gt; --remote &lt;name&gt; --commit &lt;night commit&gt;</c>. A night on main
/// can end after a promotion wrote a record at a later merge commit (D-557). The night then keeps that record, because
/// a branch night tested the later code (D-562). Exit 0 means the night writes its record. Exit 1 means the record of
/// main stays, and the line names its commit. Exit 2 means the command itself is wrong. A git failure other than an
/// absent branch is an error that names the command (T-2).
/// </summary>
public static class NightPublishCheckCommand
{
    /// <summary>The name of the command.</summary>
    public const string CommandName = "night-publish-check";

    private const string Usage = "Usage: night-publish-check --root <checkout> --remote <name> --commit <night commit>";

    public static int Run(string[] args)
    {
        string? root = null;
        string? remote = null;
        string? commit = null;
        int i = 0;
        while (i < args.Length)
        {
            if (i + 1 >= args.Length)
            {
                Console.Error.WriteLine($"The option '{args[i]}' needs a value. {Usage}");
                return 2;
            }

            switch (args[i])
            {
                case "--root": root = args[i + 1]; break;
                case "--remote": remote = args[i + 1]; break;
                case "--commit": commit = args[i + 1]; break;
                default:
                    Console.Error.WriteLine($"Unexpected argument '{args[i]}'. {Usage}");
                    return 2;
            }

            i += 2;
        }

        if (root is null || remote is null || commit is null)
        {
            Console.Error.WriteLine($"Every option is required. {Usage}");
            return 2;
        }

        var git = new GitRepository(root);
        string nightCommit = git.Run(["rev-parse", "--verify", $"{commit}^{{commit}}"]).Trim();
        NightRecordRead main = NightGateFacts.ReadRecord(git, remote, NightGateFacts.RecordBranch);
        bool? mainAfterNight = null;
        if (main.Record is not null)
        {
            string recordCommit = main.Record.Commit;
            mainAfterNight = recordCommit != nightCommit && git.HasCommit(recordCommit) && git.IsAncestor(nightCommit, recordCommit);
        }

        NightPublishDecision decision = Decide(main, mainAfterNight, nightCommit);
        Console.Out.WriteLine($"{CommandName}: {(decision.Writes ? "write" : "keep")}. {decision.Message}");
        return decision.Writes ? 0 : 1;
    }

    /// <summary>
    /// Keeps the record of main when its commit comes after the night commit on main (D-562). Every other record is
    /// replaced: an absent record, a malformed one, and one at the night commit, at an earlier commit, or at a commit
    /// off the history. To keep one of those would block every later night.
    /// </summary>
    /// <param name="main">The record of main.</param>
    /// <param name="mainAfterNight">True when the commit of the record of main is a later commit than the night commit, and null without a record.</param>
    /// <param name="nightCommit">The commit that the night tested.</param>
    public static NightPublishDecision Decide(NightRecordRead main, bool? mainAfterNight, string nightCommit)
    {
        if (main.Text is null)
        {
            return new NightPublishDecision(true, $"The record of main is absent: {main.AbsentReason}. The night at {nightCommit} writes the first record.");
        }

        if (main.Record is null)
        {
            return new NightPublishDecision(true, $"The record of main is malformed: {main.ParseError}. The night at {nightCommit} replaces it.");
        }

        if (mainAfterNight == true)
        {
            return new NightPublishDecision(false, $"The record of main names the commit {main.Record.Commit}, which comes after the night commit {nightCommit}, so it stays (D-562).");
        }

        return new NightPublishDecision(true, $"The record of main names the commit {main.Record.Commit}, which does not come after the night commit {nightCommit}, so the night replaces it.");
    }
}
