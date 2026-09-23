using System;
using System.IO;

namespace WhatYouCarry.Tools.CiSkip;

/// <summary>
/// <c>ci-skip --root &lt;checkout&gt; --event &lt;name&gt; --action &lt;name&gt; --base &lt;sha&gt; --head &lt;sha&gt; --before &lt;sha&gt; --runs &lt;file&gt; --output &lt;file&gt;</c>.
/// Reads the paths from git and the runs of the previous head from a file, applies the rules, and appends
/// <c>skip=true</c> or <c>skip=false</c> to the output file, which is the job output file of GitHub (D-474).
/// Exit 0 means the command wrote a decision. Exit 2 means the command itself is wrong. A git failure or a bad runs
/// file is an error that names the cause, and the heavy jobs then run (T-2).
/// </summary>
/// <remarks>
/// The event values of a push to <c>main</c> hold no PR, so <c>--action</c>, <c>--base</c>, <c>--head</c>, and
/// <c>--before</c> can be empty strings. A pull request event needs the base and the head.
/// </remarks>
public static class CiSkipCommand
{
    public const string OutputName = "skip";

    private const string Usage = "Usage: ci-skip --root <checkout> --event <name> --action <name> --base <sha> --head <sha> --before <sha> --runs <file> --output <file>";

    public static int Run(string[] args)
    {
        string? root = null;
        string? eventName = null;
        string? action = null;
        string? baseCommit = null;
        string? head = null;
        string? before = null;
        string? runs = null;
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
                case "--event": eventName = args[i + 1]; break;
                case "--action": action = args[i + 1]; break;
                case "--base": baseCommit = args[i + 1]; break;
                case "--head": head = args[i + 1]; break;
                case "--before": before = args[i + 1]; break;
                case "--runs": runs = args[i + 1]; break;
                case "--output": output = args[i + 1]; break;
                default:
                    Console.Error.WriteLine($"Unexpected argument '{args[i]}'. {Usage}");
                    return 2;
            }

            i += 2;
        }

        if (root is null || eventName is null || action is null || baseCommit is null || head is null || before is null || runs is null || output is null)
        {
            Console.Error.WriteLine($"Every option is required. An option with no value takes an empty string. {Usage}");
            return 2;
        }

        if (eventName == CiSkipRules.PullRequestEvent && (baseCommit.Length == 0 || head.Length == 0))
        {
            Console.Error.WriteLine($"The event '{CiSkipRules.PullRequestEvent}' needs the base '{baseCommit}' and the head '{head}'. {Usage}");
            return 2;
        }

        CiSkipFacts facts = CiSkipFacts.Gather(root, eventName, action, baseCommit, head, before, runs);
        CiSkipDecision decision = CiSkipRules.Decide(facts);
        string value = decision.Skip ? "true" : "false";
        File.AppendAllText(output, $"{OutputName}={value}\n");
        Console.Out.WriteLine($"ci-skip: {OutputName}={value}. {decision.Reason}");
        return 0;
    }
}
