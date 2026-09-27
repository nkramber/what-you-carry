using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json.Nodes;
using WhatYouCarry.Tools.BotRunner;
using WhatYouCarry.Tools.ReviewGate;

namespace WhatYouCarry.Tools.NightGate;

/// <summary>
/// The decision of the publish check of a night on main: whether the step writes a record over the record of main,
/// the record that it writes, and the message that names the case (T-2).
/// </summary>
/// <param name="Writes">True when the step writes <paramref name="Record"/>, and false when the record of main stays.</param>
/// <param name="Record">The text of the record to write, or null when the record of main stays.</param>
/// <param name="Message">The case, with the commits and the seeds that decide it.</param>
public sealed record NightPublishDecision(bool Writes, string? Record, string Message);

/// <summary>
/// <c>night-publish-check --root &lt;checkout&gt; --remote &lt;name&gt; --commit &lt;night commit&gt; --lease &lt;sha or empty&gt;
/// --night &lt;night.json&gt; --failures &lt;file&gt; --output &lt;file&gt;</c>. Decides the record that a night on main
/// publishes, from the record of main as it is at publish time (D-562, D-648).
/// </summary>
/// <remarks>
/// <para>
/// The lease is the commit of the branch <c>night-results</c> that the step read before this command, or an empty
/// text when the branch was absent. The command reads the record of main at that commit alone, and the step pushes
/// with that lease, so no record that a promotion or another night writes in between is lost. When the branch moved
/// from the lease, the command stops with a fault, and a re-run of the job reads the new record.
/// </para>
/// <para>
/// The night file is the record that this night wrote. The failures file is the gathered failure lines of the sweeps,
/// which <c>night-record</c> read: they tell which sweeps ended and which seeds this night failed.
/// </para>
/// <para>
/// The exit codes, each one a branch of the publish step:
/// <list type="bullet">
/// <item><see cref="WriteExit"/> (0): the output file holds the record to publish.</item>
/// <item><see cref="KeepExit"/> (1): the record of main stays (D-562), and the output file is not written.</item>
/// <item><see cref="UsageExit"/> (2): a wrong option.</item>
/// <item>
/// <see cref="FaultExit"/> (3): a git failure, or a branch that moved from the lease. The step pushes nothing, because
/// a push with the lease fails anyway, and a re-run of the job reads the new record.
/// </item>
/// <item>
/// <see cref="BadNightExit"/> (4): the inputs of this night are bad: an unreadable or malformed failures file, an
/// unreadable or malformed night file, or a night record at another commit. The record of main at the lease is read,
/// so the step fails closed (T-2, D-565): it publishes the failure record of <c>night-failure-record.sh</c> from the
/// record of main at the lease, which keeps its failed seeds, pushes it with the lease, and then fails the job.
/// </item>
/// </list>
/// </para>
/// </remarks>
public static class NightPublishCheckCommand
{
    /// <summary>The name of the command.</summary>
    public const string CommandName = "night-publish-check";

    /// <summary>The exit code of a record to publish in the output file.</summary>
    public const int WriteExit = 0;

    /// <summary>The exit code of a record of main that stays (D-562).</summary>
    public const int KeepExit = 1;

    /// <summary>The exit code of a wrong option.</summary>
    public const int UsageExit = 2;

    /// <summary>The exit code of a git failure or a moved lease: the step pushes nothing (D-648).</summary>
    public const int FaultExit = 3;

    /// <summary>The exit code of bad inputs of this night: the step publishes the failure record, and the job fails (T-2, D-565).</summary>
    public const int BadNightExit = 4;

    private const string Usage = "Usage: night-publish-check --root <checkout> --remote <name> --commit <night commit> --lease <commit of night-results, or empty> --night <night.json> --failures <file> --output <file>";

    public static int Run(string[] args)
    {
        string? root = null;
        string? remote = null;
        string? commit = null;
        string? lease = null;
        string? night = null;
        string? failures = null;
        string? output = null;
        int i = 0;
        while (i < args.Length)
        {
            if (i + 1 >= args.Length)
            {
                Console.Error.WriteLine($"The option '{args[i]}' needs a value. {Usage}");
                return UsageExit;
            }

            switch (args[i])
            {
                case "--root": root = args[i + 1]; break;
                case "--remote": remote = args[i + 1]; break;
                case "--commit": commit = args[i + 1]; break;
                case "--lease": lease = args[i + 1]; break;
                case "--night": night = args[i + 1]; break;
                case "--failures": failures = args[i + 1]; break;
                case "--output": output = args[i + 1]; break;
                default:
                    Console.Error.WriteLine($"Unexpected argument '{args[i]}'. {Usage}");
                    return UsageExit;
            }

            i += 2;
        }

        if (root is null || remote is null || commit is null || lease is null || night is null || failures is null || output is null)
        {
            Console.Error.WriteLine($"Every option is required. {Usage}");
            return UsageExit;
        }

        if (lease.Length > 0 && !NightRecordCommand.IsHash(lease))
        {
            Console.Error.WriteLine($"The lease '{lease}' is not 40 lowercase hexadecimal digits or an empty text. {Usage}");
            return UsageExit;
        }

        string nightCommit;
        NightRecordRead main;
        bool? mainAfterNight = null;
        try
        {
            var git = new GitRepository(root);
            nightCommit = git.Run(["rev-parse", "--verify", $"{commit}^{{commit}}"]).Trim();
            main = ReadRecordAtLease(git, remote, lease);
            if (main.Record is not null)
            {
                string recordCommit = main.Record.Commit;
                mainAfterNight = recordCommit != nightCommit && git.HasCommit(recordCommit) && git.IsAncestor(nightCommit, recordCommit);
            }
        }
        catch (InvalidOperationException exception)
        {
            Console.Error.WriteLine($"{CommandName}: fault, and the step pushes no record: {exception.Message}");
            return FaultExit;
        }

        NightPublishDecision decision;
        try
        {
            string nightText = File.ReadAllText(night);
            Dictionary<string, List<ulong>> ended = NightSeeds.ReadFailures(File.ReadAllText(failures), failures);
            decision = Decide(main, mainAfterNight, nightCommit, nightText, ended, DateTimeOffset.UtcNow);
        }
        catch (Exception exception) when (exception is FormatException or IOException or UnauthorizedAccessException)
        {
            // Fail closed (T-2, D-565): the step publishes the failure record from the record of main at the lease.
            Console.Error.WriteLine($"{CommandName}: the inputs of this night are bad, so the step publishes the failure record of this night and fails the job: {exception.Message}");
            return BadNightExit;
        }

        Console.Out.WriteLine($"{CommandName}: {(decision.Writes ? "write" : "keep")}. {decision.Message}");
        if (!decision.Writes)
        {
            return KeepExit;
        }

        // UTF-8 without the byte-order mark, as night-record writes it.
        File.WriteAllText(output, decision.Record, new UTF8Encoding(false));
        return WriteExit;
    }

    /// <summary>
    /// Decides the record that a night on main publishes (D-562, D-648). The record of main is the one at publish time.
    /// <list type="number">
    /// <item>An absent or malformed record of main, a malformed seed field included, is replaced by the night record as it is.</item>
    /// <item>
    /// A record of main at a later commit stays (D-562), unless this night failed a seed that it does not name and that
    /// the night of the later record did not run. Then the decision writes a failure record at the later commit: the
    /// later record with those seeds added to its failed seeds, the end time <paramref name="now"/>, and the status
    /// failure (D-648). A seed that the night of the later record ran and does not name passed on the later code, so it
    /// goes away (D-567). That night ran the fixed range, the slice of the later record, and its carried seeds.
    /// </item>
    /// <item>
    /// Any other record of main is replaced by the night record. Each failed seed of the record of main that this
    /// night did not run stays in the failed seeds of that record, and the record then reads failure (D-648). A seed
    /// that this night ran and passed goes away (D-567).
    /// </item>
    /// </list>
    /// A night ran a seed when the sweep of the seed ended, and <see cref="NightSeeds.Ran"/> holds for the slice and the
    /// carried seeds of the night record. A sweep that did not end ran none of its seeds to the end. The seeds that
    /// this night failed are the seeds of the failure lines of its sweeps that ended. The failed seeds of a sweep that
    /// did not end are the carry of the plan, and this night did not fail them.
    /// </summary>
    /// <param name="main">The record of main at publish time.</param>
    /// <param name="mainAfterNight">True when the commit of the record of main is a later commit than the night commit, and null without a record.</param>
    /// <param name="nightCommit">The commit that the night tested.</param>
    /// <param name="nightText">The record that this night wrote.</param>
    /// <param name="ended">The failed seeds of each sweep of this night that ended, from <see cref="NightSeeds.ReadFailures"/>.</param>
    /// <param name="now">The time of the publish, the end time of a failure record at a later commit.</param>
    /// <exception cref="FormatException">The night record is not a record at the night commit, or one of its seed fields is malformed (T-2).</exception>
    public static NightPublishDecision Decide(NightRecordRead main, bool? mainAfterNight, string nightCommit, string nightText, IReadOnlyDictionary<string, List<ulong>> ended, DateTimeOffset now)
    {
        NightRecord night = NightRecordParser.TryParse(nightText, out string nightError)
            ?? throw new FormatException($"The record of this night is not a night record: {nightError}.");
        if (night.Commit != nightCommit)
        {
            throw new FormatException($"The record of this night names the commit {night.Commit}, and the night tested {nightCommit}.");
        }

        // Each seed field of this night is read in every case, so a malformed night record fails closed (T-2, D-565).
        Dictionary<string, List<ulong>> nightFailed = NightSeeds.ReadRecordSeeds(nightText, NightSeeds.FailedSeedsName, "the record of this night");
        Dictionary<string, List<ulong>> nightCarried = NightSeeds.ReadRecordSeeds(nightText, NightSeeds.CarriedSeedsName, "the record of this night");
        Dictionary<string, SeedRange> nightSlices = NightSeeds.ReadSlices(nightText, "the record of this night");

        if (main.Text is null)
        {
            return new NightPublishDecision(true, nightText, $"The record of main is absent: {main.AbsentReason}. The night at {nightCommit} writes the first record.");
        }

        if (main.Record is null)
        {
            return new NightPublishDecision(true, nightText, $"The record of main is malformed: {main.ParseError}. The night at {nightCommit} replaces it.");
        }

        Dictionary<string, List<ulong>> mainFailed;
        Dictionary<string, List<ulong>> mainCarried;
        Dictionary<string, SeedRange> mainSlices;
        try
        {
            mainFailed = NightSeeds.ReadRecordSeeds(main.Text, NightSeeds.FailedSeedsName, $"the branch {main.Branch}");
            mainCarried = NightSeeds.ReadRecordSeeds(main.Text, NightSeeds.CarriedSeedsName, $"the branch {main.Branch}");
            mainSlices = NightSeeds.ReadSlices(main.Text, $"the branch {main.Branch}");
        }
        catch (FormatException exception)
        {
            return new NightPublishDecision(true, nightText, $"The record of main is malformed: {exception.Message} The night at {nightCommit} replaces it.");
        }

        if (mainAfterNight == true)
        {
            return DecideLater(main, mainFailed, mainSlices, mainCarried, nightCommit, ended, now);
        }

        return DecideReplace(main, mainFailed, nightCommit, nightText, nightFailed, nightSlices, nightCarried, ended);
    }

    /// <summary>
    /// The record of main at a later commit stays, unless this night failed a seed that it does not name and that its
    /// night did not run. Then the decision writes the later record with those seeds added to its failed seeds, the end
    /// time of the publish, and the status failure (D-562, D-648). A seed that the later night ran passed on the later
    /// code, so it goes away (D-567).
    /// </summary>
    private static NightPublishDecision DecideLater(NightRecordRead main, Dictionary<string, List<ulong>> mainFailed, Dictionary<string, SeedRange> laterSlices, Dictionary<string, List<ulong>> laterCarried, string nightCommit, IReadOnlyDictionary<string, List<ulong>> ended, DateTimeOffset now)
    {
        string laterCommit = main.Record!.Commit;
        List<string> added = [];
        List<string> passedLater = [];
        Dictionary<string, List<ulong>> union = new(StringComparer.Ordinal);
        foreach (string sweep in NightSeeds.Sweeps)
        {
            SortedSet<ulong> seeds = [.. NightSeeds.SeedsOf(mainFailed, sweep)];
            foreach (ulong seed in NightSeeds.SeedsOf(ended, sweep))
            {
                if (seeds.Contains(seed))
                {
                    continue;
                }

                string name = $"{sweep} {seed.ToString(CultureInfo.InvariantCulture)}";
                if (NightSeeds.Ran(sweep, seed, laterSlices, laterCarried))
                {
                    passedLater.Add(name);
                    continue;
                }

                seeds.Add(seed);
                added.Add(name);
            }

            union[sweep] = [.. seeds];
        }

        string passedText = passedLater.Count == 0 ? string.Empty : $" The night of the later record ran and passed the failed seeds {string.Join(", ", passedLater)} of this night, so they go away (D-567).";
        if (added.Count == 0)
        {
            return new NightPublishDecision(false, null, $"The record of main names the commit {laterCommit}, which comes after the night commit {nightCommit}, and it names each seed that this night failed, or its night passed the seed, so it stays (D-562).{passedText}");
        }

        JsonObject record = ParseObject(main.Text!, $"the record of main at {laterCommit}");
        record[NightRecordCommand.EndedAtName] = now.UtcDateTime.ToString(NightRecordParser.TimeFormat, CultureInfo.InvariantCulture);
        record[NightRecordCommand.StatusName] = "failure";
        record[NightSeeds.FailedSeedsName] = SeedsObject(union);
        return new NightPublishDecision(true, record.ToJsonString() + "\n", $"The record of main names the commit {laterCommit}, which comes after the night commit {nightCommit}. This night failed seeds that it does not name and that its night did not run: {string.Join(", ", added)}. The failure record at {laterCommit} adds them to its failed seeds (D-648).{passedText}");
    }

    /// <summary>
    /// The night record replaces the record of main. Each failed seed of the record of main that this night did not run
    /// stays in the failed seeds of the night record, and the record then reads failure (D-648).
    /// </summary>
    private static NightPublishDecision DecideReplace(NightRecordRead main, Dictionary<string, List<ulong>> mainFailed, string nightCommit, string nightText, Dictionary<string, List<ulong>> nightFailed, Dictionary<string, SeedRange> slices, Dictionary<string, List<ulong>> carried, IReadOnlyDictionary<string, List<ulong>> ended)
    {
        List<string> kept = [];
        Dictionary<string, List<ulong>> union = new(StringComparer.Ordinal);
        foreach (string sweep in NightSeeds.Sweeps)
        {
            SortedSet<ulong> seeds = [.. NightSeeds.SeedsOf(nightFailed, sweep)];
            bool sweepEnded = ended.ContainsKey(sweep);
            foreach (ulong seed in NightSeeds.SeedsOf(mainFailed, sweep))
            {
                bool ran = sweepEnded && NightSeeds.Ran(sweep, seed, slices, carried);
                if (!ran && seeds.Add(seed))
                {
                    kept.Add($"{sweep} {seed.ToString(CultureInfo.InvariantCulture)}");
                }
            }

            union[sweep] = [.. seeds];
        }

        string mainCommit = main.Record!.Commit;
        if (kept.Count == 0)
        {
            return new NightPublishDecision(true, nightText, $"The record of main names the commit {mainCommit}, which does not come after the night commit {nightCommit}, so the night replaces it. The night record names or ran each failed seed of it (D-648).");
        }

        JsonObject record = ParseObject(nightText, "the record of this night");
        record[NightRecordCommand.StatusName] = "failure";
        record[NightSeeds.FailedSeedsName] = SeedsObject(union);
        return new NightPublishDecision(true, record.ToJsonString() + "\n", $"The record of main names the commit {mainCommit}, which does not come after the night commit {nightCommit}, so the night replaces it. The night did not run the failed seeds {string.Join(", ", kept)} of it, so they stay, and the record reads failure (D-648).");
    }

    /// <summary>
    /// Reads the record of main at the lease (D-648). An empty lease means that the branch was absent. A branch that
    /// moved from the lease is a fault, because a record that the step did not read would be lost.
    /// </summary>
    /// <exception cref="InvalidOperationException">The branch moved from the lease, or a git command failed.</exception>
    private static NightRecordRead ReadRecordAtLease(GitRepository git, string remote, string lease)
    {
        string branch = NightGateFacts.RecordBranch;
        bool present = git.HasRemoteBranch(remote, branch);
        if (!present && lease.Length == 0)
        {
            return new NightRecordRead(branch, null, $"the remote '{remote}' has no branch {branch}", null, null);
        }

        string fetched = string.Empty;
        if (present)
        {
            git.Fetch(remote, branch);
            fetched = git.Run(["rev-parse", "--verify", "FETCH_HEAD^{commit}"]).Trim();
        }

        if (fetched != lease)
        {
            string now = fetched.Length == 0 ? "no branch" : fetched;
            string before = lease.Length == 0 ? "no branch" : lease;
            throw new InvalidOperationException($"The branch {branch} of the remote '{remote}' moved from {before} to {now} after the publish step read its lease. A re-run of the record job reads the new record (D-648).");
        }

        string? text = git.ReadFileOrNull(fetched, NightGateFacts.RecordFile);
        if (text is null)
        {
            return new NightRecordRead(branch, null, $"the branch {branch} of the remote '{remote}' holds no {NightGateFacts.RecordFile}", null, null);
        }

        NightRecord? record = NightRecordParser.TryParse(text, out string error);
        return new NightRecordRead(branch, text, null, record, record is null ? error : null);
    }

    /// <summary>The seeds of each sweep as one JSON object, in the order of <see cref="NightSeeds.Sweeps"/>, as <see cref="NightSeeds.RecordFields"/> writes the field.</summary>
    private static JsonObject SeedsObject(Dictionary<string, List<ulong>> seeds)
    {
        JsonObject field = new();
        foreach (string sweep in NightSeeds.Sweeps)
        {
            JsonArray array = new();
            foreach (ulong seed in NightSeeds.SeedsOf(seeds, sweep))
            {
                array.Add(JsonValue.Create(seed));
            }

            field[sweep] = array;
        }

        return field;
    }

    /// <exception cref="FormatException">The text is not a JSON object.</exception>
    private static JsonObject ParseObject(string text, string source)
    {
        // The records of the first two nights start with a byte-order mark (NightRecordParser.TryParse).
        JsonNode? node = JsonNode.Parse(text.TrimStart('﻿'));
        if (node is not JsonObject record)
        {
            throw new FormatException($"The text of {source} is not a JSON object.");
        }

        return record;
    }
}
