using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Tools.AssetQa;

/// <summary>
/// <c>asset-qa --root &lt;checkout&gt;</c>: the asset QA gate v1 (D-135, D-149). It reads every model,
/// overlay, and animation under the content directory and runs the clip check, the overlay check, and the
/// file case check. Prints one line per finding. Exit 0 means no finding.
/// </summary>
public static class AssetQaCommand
{
    /// <summary>The content directory under the checkout root.</summary>
    public const string ContentDirectoryName = "content";

    /// <summary>The whole run as an exit code: 0 with no finding, 1 with any, 2 on a bad command line or an absent directory.</summary>
    public static int Run(string[] args)
    {
        string? root = null;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--root" && i + 1 < args.Length)
            {
                root = args[i + 1];
                i++;
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

        string contentRoot = Path.Combine(root, ContentDirectoryName);
        AssetSet set;
        try
        {
            set = AssetSet.Read(contentRoot);
        }
        catch (ContextException error)
        {
            Console.Error.WriteLine(error.Message);
            return 2;
        }

        IReadOnlyList<AssetFinding> findings = Findings(set, contentRoot);
        foreach (AssetFinding finding in findings)
        {
            Console.WriteLine(finding.Line());
        }

        Console.WriteLine(Summary(set, findings.Count));
        return findings.Count == 0 ? 0 : 1;
    }

    /// <summary>Every finding of the three checks and the loaders, in that order: load, clip, overlay, then case.</summary>
    public static IReadOnlyList<AssetFinding> Findings(AssetSet set, string contentRoot)
    {
        List<AssetFinding> findings = [];
        findings.AddRange(set.LoadFindings);
        findings.AddRange(ClipCheck.Run(set));
        findings.AddRange(OverlayCheck.Run(set));
        findings.AddRange(FileCaseCheck.Run(contentRoot));
        return findings;
    }

    /// <summary>The end line of the report: the counts of findings, models, overlays, and animations.</summary>
    public static string Summary(AssetSet set, int findingCount)
    {
        return $"asset-qa: {Count(findingCount)} finding(s). {Count(set.Bodies.Count)} model(s), {Count(set.Overlays.Count)} overlay(s), {Count(set.Animations.Count)} animation(s).";
    }

    private static string Count(int count)
    {
        return count.ToString(CultureInfo.InvariantCulture);
    }
}
