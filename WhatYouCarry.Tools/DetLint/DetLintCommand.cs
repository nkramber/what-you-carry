using System;
using System.Collections.Generic;

namespace WhatYouCarry.Tools.DetLint;

/// <summary>
/// <c>det-lint --root &lt;checkout&gt;</c>. Prints one line per finding. Exit 0 means no finding.
/// Exit 1 means at least one finding. Exit 2 means a usage error. Any other failure is an exception with its context.
/// </summary>
public static class DetLintCommand
{
    public static int Run(string[] args)
    {
        string? root = null;
        int i = 0;
        while (i < args.Length)
        {
            if (args[i] == "--root" && i + 1 < args.Length)
            {
                root = args[i + 1];
                i += 2;
                continue;
            }

            Console.Error.WriteLine($"Unexpected argument '{args[i]}'. The only option is --root <checkout>, and every argument must belong to it.");
            return 2;
        }

        if (root is null)
        {
            Console.Error.WriteLine("The option is required: --root <checkout>.");
            return 2;
        }

        // Two rule sets, one command. Core takes the determinism rules, and Game takes the string rule.
        // The two never mix: Game has an engine dependency, and no player reads a Core string (D-222).
        IReadOnlyList<string> coreFiles = CoreSourceScan.SourceFiles(root);
        IReadOnlyList<LintFinding> coreFindings = CoreSourceScan.Run(root);
        IReadOnlyList<string> gameFiles = GameStringScan.SourceFiles(root);
        IReadOnlyList<LintFinding> gameFindings = GameStringScan.Run(root);

        foreach (LintFinding finding in coreFindings)
        {
            Console.WriteLine(finding);
        }

        foreach (LintFinding finding in gameFindings)
        {
            Console.WriteLine(finding);
        }

        int total = coreFindings.Count + gameFindings.Count;
        Console.WriteLine($"det-lint: {total} finding(s). Core {coreFindings.Count} in {coreFiles.Count} file(s), Game {gameFindings.Count} in {gameFiles.Count} file(s).");
        return total == 0 ? 0 : 1;
    }
}
