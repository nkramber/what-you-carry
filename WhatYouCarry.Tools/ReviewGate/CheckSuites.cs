using System;
using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Tools.CodexReview;

namespace WhatYouCarry.Tools.ReviewGate;

/// <summary>
/// The check suites that GitHub made for one commit. The earliest creation time is the push time of the commit, and
/// the override label compares its event time with the push time of the work head (D-653).
/// </summary>
public static class CheckSuites
{
    /// <summary>
    /// Reads the creation time of each check suite of the commit through <c>gh api</c>, from every page of
    /// <c>GET /repos/{owner}/{repo}/commits/{sha}/check-suites</c>. The job token of the workflow authenticates gh.
    /// </summary>
    /// <exception cref="InvalidOperationException">gh did not start or exited nonzero. The message names the command, the exit code, and stderr (T-2).</exception>
    /// <exception cref="FormatException">A line of the output is not a creation time. The message names the commit and the line.</exception>
    public static IReadOnlyList<DateTimeOffset> ReadCreationTimes(string workingDirectory, string repository, string sha)
    {
        string lines = ExternalProcess.Run(
            "gh",
            ["api", "--paginate", $"repos/{repository}/commits/{sha}/check-suites?per_page=100", "--jq", ".check_suites[].created_at"],
            workingDirectory).RequireSuccess();
        return ParseCreationTimes(repository, sha, lines);
    }

    /// <summary>
    /// Reads one creation time from each line. No line means no check suite, and the list is then empty. The jq text
    /// of an absent creation time is <c>null</c>, and that is an error, never a time (T-2).
    /// </summary>
    /// <exception cref="FormatException">A line is not a time. The message names the repository, the commit, and the line.</exception>
    public static IReadOnlyList<DateTimeOffset> ParseCreationTimes(string repository, string sha, string lines)
    {
        var times = new List<DateTimeOffset>();
        foreach (string line in lines.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!DateTimeOffset.TryParse(line, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset time))
            {
                throw new FormatException($"A check suite of {sha} in {repository} gave the creation time '{line}', and the expected form is an ISO 8601 time (D-653).");
            }

            times.Add(time);
        }

        return times;
    }
}
