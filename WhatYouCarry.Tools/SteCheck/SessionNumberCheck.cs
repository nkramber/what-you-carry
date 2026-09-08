using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace WhatYouCarry.Tools.SteCheck;

/// <summary>The session number check (D-187). Two entries with the same number in the handoff file are a finding.</summary>
public static class SessionNumberCheck
{
    public const string Rule = "D-187";

    private static readonly Regex SessionHeading = new(@"^## Session (\d+)\b", RegexOptions.Compiled);

    public static List<Finding> Check(string relativePath, string text)
    {
        var findings = new List<Finding>();
        var firstLineByNumber = new Dictionary<int, int>();
        string[] lines = text.Split('\n');
        for (int index = 0; index < lines.Length; index++)
        {
            string line = lines[index].TrimEnd('\r');
            Match match = SessionHeading.Match(line);
            if (!match.Success)
            {
                continue;
            }

            int number = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            if (firstLineByNumber.TryGetValue(number, out int firstLine))
            {
                findings.Add(new Finding(relativePath, index + 1, Rule, $"session {number} also starts on line {firstLine}", line));
                continue;
            }

            firstLineByNumber[number] = index + 1;
        }

        if (firstLineByNumber.Count == 0)
        {
            throw new InvalidOperationException($"'{relativePath}' has no '## Session <number>' heading.");
        }

        return findings;
    }
}
