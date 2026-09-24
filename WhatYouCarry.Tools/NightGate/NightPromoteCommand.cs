using System;
using System.Globalization;
using System.IO;

namespace WhatYouCarry.Tools.NightGate;

/// <summary>
/// <c>night-promote --root &lt;checkout&gt; --remote &lt;name&gt; --merge &lt;commit&gt; --head-branch &lt;name&gt; --now &lt;time&gt; --output &lt;file&gt;</c>.
/// Reads the record of main and the record of the night on the head branch of the merged PR, compares the trees of
/// the night commit and the merge commit, and applies the promotion rules (D-555 to D-558). Exit 0 means the branch
/// night promotes, and the command writes the promoted record to the output file. Exit 1 means no promotion, and the
/// line names the case. Exit 2 means the command itself is wrong. A git failure other than an absent branch is an
/// error that names the command (T-2).
/// </summary>
public static class NightPromoteCommand
{
    private const string Usage = "Usage: night-promote --root <checkout> --remote <name> --merge <commit> --head-branch <name> --now <yyyy-MM-ddTHH:mm:ssZ> --output <file>";

    public static int Run(string[] args)
    {
        string? root = null;
        string? remote = null;
        string? merge = null;
        string? headBranch = null;
        string? now = null;
        string? output = null;
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
                case "--merge": merge = args[i + 1]; break;
                case "--head-branch": headBranch = args[i + 1]; break;
                case "--now": now = args[i + 1]; break;
                case "--output": output = args[i + 1]; break;
                default:
                    Console.Error.WriteLine($"Unexpected argument '{args[i]}'. {Usage}");
                    return 2;
            }

            i += 2;
        }

        if (root is null || remote is null || merge is null || headBranch is null || now is null || output is null)
        {
            Console.Error.WriteLine($"Every option is required. {Usage}");
            return 2;
        }

        if (!DateTimeOffset.TryParseExact(now, NightRecordParser.TimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTimeOffset time))
        {
            Console.Error.WriteLine($"The time '{now}' is not in the form {NightRecordParser.TimeFormat}. {Usage}");
            return 2;
        }

        NightPromotionFacts facts = NightPromotionFacts.Gather(root, remote, merge, headBranch, time);
        NightPromotionResult result = NightPromotionRules.Evaluate(facts);
        Console.Out.WriteLine($"{NightPromotionRules.CommandName}: {(result.Promotes ? "promote" : "no promotion")} ({result.Case}). {result.Message}");
        if (!result.Promotes)
        {
            return 1;
        }

        File.WriteAllText(output, NightPromotionRules.PromotedRecord(facts));
        return 0;
    }
}
