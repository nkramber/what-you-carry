using System.Collections.Generic;
using System.IO;

namespace WhatYouCarry.Tools.SteCheck;

/// <summary>The three checks over one checkout: the STE rules, the reference check, and the session number check.</summary>
public sealed record RepositoryCheckResult(IReadOnlyList<string> Files, List<Finding> SteFindings, List<Finding> ReferenceFindings, List<Finding> SessionFindings)
{
    public int FindingCount => SteFindings.Count + ReferenceFindings.Count + SessionFindings.Count;
}

public static class RepositoryCheck
{
    public static RepositoryCheckResult Run(string root)
    {
        List<string> files = DocumentSet.Enumerate(root);
        string decisionsText = ReadRequired(root, DocumentSet.DecisionsPath);
        Dictionary<string, string> superseded = ReferenceCheck.SupersededDecisions(decisionsText);

        var steFindings = new List<Finding>();
        var referenceFindings = new List<Finding>();
        foreach (string relativePath in files)
        {
            string text = File.ReadAllText(Path.Combine(root, relativePath));
            steFindings.AddRange(SteChecker.Check(relativePath, text));
            if (relativePath != DocumentSet.DecisionsPath)
            {
                referenceFindings.AddRange(ReferenceCheck.Check(relativePath, text, superseded));
            }
        }

        List<Finding> sessionFindings = SessionNumberCheck.Check(DocumentSet.HandoffPath, ReadRequired(root, DocumentSet.HandoffPath));
        return new RepositoryCheckResult(files, steFindings, referenceFindings, sessionFindings);
    }

    private static string ReadRequired(string root, string relativePath)
    {
        string path = Path.Combine(root, relativePath);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"The checkout '{root}' has no file '{relativePath}'.", path);
        }

        return File.ReadAllText(path);
    }
}
