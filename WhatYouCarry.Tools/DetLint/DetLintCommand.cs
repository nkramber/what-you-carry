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

        IReadOnlyList<string> files = CoreSourceScan.SourceFiles(root);
        IReadOnlyList<LintFinding> findings = CoreSourceScan.Run(root);
        foreach (LintFinding finding in findings)
        {
            Console.WriteLine(finding);
        }

        Console.WriteLine($"det-lint: {findings.Count} finding(s) in {files.Count} Core file(s).");
        return findings.Count == 0 ? 0 : 1;
    }
}
