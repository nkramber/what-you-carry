using System;

namespace WhatYouCarry.Tools.SteCheck;

/// <summary>
/// <c>ste-check --root &lt;checkout&gt;</c>. Prints one line per finding. Exit 0 means no finding.
/// Exit 1 means at least one finding. Exit 2 means a usage error. Any other failure is an exception with its context.
/// </summary>
public static class SteCheckCommand
{
    public static int Run(string[] args)
    {
        string? root = null;
        for (int i = 0; i + 1 < args.Length; i += 2)
        {
            if (args[i] == "--root")
            {
                root = args[i + 1];
                continue;
            }

            Console.Error.WriteLine($"Unknown option '{args[i]}'. Options: --root <checkout>.");
            return 2;
        }

        if (root is null)
        {
            Console.Error.WriteLine("The option is required: --root <checkout>.");
            return 2;
        }

        RepositoryCheckResult result = RepositoryCheck.Run(root);
        foreach (Finding finding in result.SteFindings)
        {
            Console.WriteLine(finding);
        }

        foreach (Finding finding in result.ReferenceFindings)
        {
            Console.WriteLine(finding);
        }

        foreach (Finding finding in result.SessionFindings)
        {
            Console.WriteLine(finding);
        }

        Console.WriteLine($"ste-check: {result.FindingCount} finding(s) in {result.Files.Count} file(s). STE {result.SteFindings.Count}, references {result.ReferenceFindings.Count}, session numbers {result.SessionFindings.Count}.");
        return result.FindingCount == 0 ? 0 : 1;
    }
}
