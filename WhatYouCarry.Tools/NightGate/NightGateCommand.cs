using System;
using System.Globalization;

namespace WhatYouCarry.Tools.NightGate;

/// <summary>
/// <c>night-gate --root &lt;checkout&gt; --remote &lt;name&gt; --base &lt;revision&gt; --now &lt;time&gt;</c>. Fetches the
/// branch <c>night-results</c> of the remote, reads the record from git, asks git whether its commit is on the
/// base branch, and applies the rules (D-177, D-274, D-275). Exit 0 means the gate passes, exit 1 means it
/// fails and the line names the case, and exit 2 means the command itself is wrong. A git failure other than an
/// absent branch is an error that names the command (T-2).
/// </summary>
public static class NightGateCommand
{
    private const string Usage = "Usage: night-gate --root <checkout> --remote <name> --base <revision> --now <yyyy-MM-ddTHH:mm:ssZ>";

    public static int Run(string[] args)
    {
        string? root = null;
        string? remote = null;
        string? baseRef = null;
        string? now = null;
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
                case "--base": baseRef = args[i + 1]; break;
                case "--now": now = args[i + 1]; break;
                default:
                    Console.Error.WriteLine($"Unexpected argument '{args[i]}'. {Usage}");
                    return 2;
            }

            i += 2;
        }

        if (root is null || remote is null || baseRef is null || now is null)
        {
            Console.Error.WriteLine($"Every option is required. {Usage}");
            return 2;
        }

        if (!DateTimeOffset.TryParseExact(now, NightRecordParser.TimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTimeOffset time))
        {
            Console.Error.WriteLine($"The time '{now}' is not in the form {NightRecordParser.TimeFormat}. {Usage}");
            return 2;
        }

        NightGateFacts facts = NightGateFacts.Gather(root, remote, baseRef, time);
        NightGateResult result = NightGateRules.Evaluate(facts);
        Console.Out.WriteLine($"{NightGateRules.JobName}: {(result.Passes ? "pass" : "fail")} ({result.Case}). {result.Message}");
        return result.Passes ? 0 : 1;
    }
}
