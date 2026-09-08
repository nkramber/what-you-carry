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
