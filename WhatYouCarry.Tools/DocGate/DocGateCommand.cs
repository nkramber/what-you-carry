using System;

namespace WhatYouCarry.Tools.DocGate;

/// <summary>
/// <c>doc-gate --root &lt;checkout&gt; --base &lt;revision&gt; --head &lt;revision&gt; --body &lt;file&gt; --title &lt;text&gt; --branch &lt;name&gt;</c>.
/// Reads the PR diff, the handoff, and the commit messages from git and the description from a file, then applies the
/// rules (D-375, D-376, F-138).
/// Exit 0 means the gate passes. Exit 1 means it fails, and one line names each problem. Exit 2 means the command
/// itself is wrong. A git failure is an error that names the command (T-2).
/// </summary>
public static class DocGateCommand
{
    private const string Usage = "Usage: doc-gate --root <checkout> --base <revision> --head <revision> --body <file> --title <text> --branch <name>";

    public static int Run(string[] args)
    {
        string? root = null;
        string? baseRef = null;
        string? head = null;
        string? body = null;
        string? title = null;
        string? branch = null;
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
                case "--base": baseRef = args[i + 1]; break;
                case "--head": head = args[i + 1]; break;
                case "--body": body = args[i + 1]; break;
                case "--title": title = args[i + 1]; break;
                case "--branch": branch = args[i + 1]; break;
                default:
                    Console.Error.WriteLine($"Unexpected argument '{args[i]}'. {Usage}");
                    return 2;
            }

            i += 2;
        }

        if (root is null || baseRef is null || head is null || body is null || title is null || branch is null)
        {
            Console.Error.WriteLine($"Every option is required. {Usage}");
            return 2;
        }

        DocGateFacts facts = DocGateFacts.Gather(root, baseRef, head, body, title, branch);
        DocGateResult result = DocGateRules.Evaluate(facts);
        foreach (string problem in result.Problems)
        {
            Console.Out.WriteLine($"{DocGateRules.JobName}: {problem}");
        }

        Console.Out.WriteLine($"{DocGateRules.JobName}: {(result.Passes ? "pass" : "fail")}, {result.Problems.Count} problem(s) over {facts.ChangedPaths.Count} changed path(s) and {facts.CommitMessages.Count} commit(s).");
        return result.Passes ? 0 : 1;
    }
}
