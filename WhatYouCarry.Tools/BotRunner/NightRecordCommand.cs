using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace WhatYouCarry.Tools.BotRunner;

/// <summary>
/// <c>night-record --commit &lt;sha&gt; --status &lt;success|failure|cancelled&gt; --output &lt;file&gt; [--summary
/// &lt;file&gt;]</c>. Writes the record of one night, scheduled or by hand: the commit it tested, the end time, the
/// status, and the count of deaths of each bot policy (D-177, D-273, D-274, D-403). The night job commits the file
/// to the branch <c>night-results</c>, and the <c>night-gate</c> command reads it.
/// </summary>
/// <remarks>
/// The summary file holds one line for each policy, in the form <c>policy=count</c> and then one <c>cause:count</c>
/// word for each cause (D-411), which
/// <see cref="BotRunCommand.DeathLine"/> writes. An absent file gives a record with no death counts, and the gate
/// reads the status alone, so every record that an older night wrote still parses (D-177).
/// </summary>
public static class NightRecordCommand
{
    /// <summary>The field names of the record.</summary>
    public const string CommitName = "commit";
    public const string EndedAtName = "endedAt";
    public const string StatusName = "status";

    /// <summary>The field that holds the count of deaths of each policy, by policy name (D-403).</summary>
    public const string DeathsName = "deaths";

    /// <summary>The field that holds the count of deaths of each cause for each policy, by policy name and then by cause (D-411).</summary>
    public const string DeathCausesName = "deathCauses";

    /// <summary>The statuses that a night can end with. The gate passes on success alone (D-177).</summary>
    public static readonly string[] Statuses = ["success", "failure", "cancelled"];

    private const string Usage = "Usage: night-record --commit <sha> --status <success|failure|cancelled> --output <file> [--summary <file>]";

    public static int Run(string[] args)
    {
        string? commit = null;
        string? status = null;
        string? output = null;
        string? summary = null;
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
                case "--commit": commit = args[i + 1]; break;
                case "--status": status = args[i + 1]; break;
                case "--output": output = args[i + 1]; break;
                case "--summary": summary = args[i + 1]; break;
                default:
                    Console.Error.WriteLine($"Unexpected argument '{args[i]}'. {Usage}");
                    return 2;
            }

            i += 2;
        }

        if (commit is null || status is null || output is null)
        {
            Console.Error.WriteLine($"Every option is required. {Usage}");
            return 2;
        }

        if (!IsHash(commit))
        {
            Console.Error.WriteLine($"The commit '{commit}' is not 40 lowercase hexadecimal digits. {Usage}");
            return 2;
        }

        if (Array.IndexOf(Statuses, status) < 0)
        {
            Console.Error.WriteLine($"The status '{status}' is not one of: {string.Join(", ", Statuses)}. {Usage}");
            return 2;
        }

        string deaths = summary is not null && File.Exists(summary) ? File.ReadAllText(summary) : string.Empty;

        // UTF-8 without the byte-order mark: Encoding.UTF8 writes one, and the record is one JSON object from its first byte.
        File.WriteAllText(output, Build(commit, DateTime.UtcNow, status, deaths), new UTF8Encoding(false));
        Console.Out.WriteLine($"night-record: {output} holds commit {commit} with status {status}.");
        return 0;
    }

    /// <summary>
    /// The record as one JSON object with a line break. The time is UTC in the round-trip form. The death counts
    /// come from the summary text, one <c>policy=count</c> line for each policy, and an empty text gives an empty
    /// object (D-403).
    /// </summary>
    /// <exception cref="FormatException">A summary line is not one name, one equals sign, and one whole number (T-2).</exception>
    public static string Build(string commit, DateTime endedAt, string status, string summary)
    {
        string time = endedAt.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);
        return $"{{\"{CommitName}\":\"{commit}\",\"{EndedAtName}\":\"{time}\",\"{StatusName}\":\"{status}\",\"{DeathsName}\":{DeathsObject(summary)},\"{DeathCausesName}\":{CausesObject(summary)}}}\n";
    }

    /// <summary>The death counts of the summary text as one JSON object, in the order of the lines.</summary>
    /// <exception cref="FormatException">A line is not one name, one equals sign, and one whole number, with cause words after it (T-2).</exception>
    public static string DeathsObject(string summary)
    {
        StringBuilder text = new("{");
        bool first = true;
        foreach (SummaryLine line in ReadSummary(summary))
        {
            if (!first)
            {
                text.Append(',');
            }

            text.Append('"').Append(line.Policy).Append("\":").Append(line.Deaths.ToString(CultureInfo.InvariantCulture));
            first = false;
        }

        return text.Append('}').ToString();
    }

    /// <summary>
    /// The death counts of each cause of the summary text as one JSON object: one object for each policy, in the
    /// order of the lines, with the causes in the order of the words (D-411).
    /// </summary>
    /// <exception cref="FormatException">A line is not one name, one equals sign, and one whole number, with cause words after it (T-2).</exception>
    public static string CausesObject(string summary)
    {
        StringBuilder text = new("{");
        bool firstPolicy = true;
        foreach (SummaryLine line in ReadSummary(summary))
        {
            if (!firstPolicy)
            {
                text.Append(',');
            }

            text.Append('"').Append(line.Policy).Append("\":{");
            for (int index = 0; index < line.Causes.Count; index++)
            {
                if (index > 0)
                {
                    text.Append(',');
                }

                text.Append('"').Append(line.Causes[index].Cause).Append("\":").Append(line.Causes[index].Count.ToString(CultureInfo.InvariantCulture));
            }

            text.Append('}');
            firstPolicy = false;
        }

        return text.Append('}').ToString();
    }

    /// <summary>
    /// The lines of the summary text. A blank line is skipped. A line of an older night holds no cause word, and it
    /// reads as a policy with no cause (D-177).
    /// </summary>
    /// <exception cref="FormatException">A line is not one name, one equals sign, and one whole number, with cause words after it (T-2).</exception>
    private static List<SummaryLine> ReadSummary(string summary)
    {
        List<SummaryLine> lines = [];
        foreach (string raw in summary.Split('\n'))
        {
            string trimmed = raw.Trim();
            if (trimmed.Length == 0)
            {
                continue;
            }

            string[] words = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int mark = words[0].IndexOf('=');
            if (mark < 1 || !long.TryParse(words[0][(mark + 1)..], NumberStyles.Integer, CultureInfo.InvariantCulture, out long deaths) || deaths < 0)
            {
                throw new FormatException($"The bot summary line '{trimmed}' is not one policy name, one equals sign, and one count of zero or more (D-403).");
            }

            List<CauseCount> causes = [];
            for (int index = 1; index < words.Length; index++)
            {
                int colon = words[index].LastIndexOf(':');
                if (colon < 1 || !long.TryParse(words[index][(colon + 1)..], NumberStyles.Integer, CultureInfo.InvariantCulture, out long count) || count < 0)
                {
                    throw new FormatException($"The cause word '{words[index]}' of the bot summary line '{trimmed}' is not one cause, one colon, and one count of zero or more (D-411).");
                }

                causes.Add(new CauseCount(words[index][..colon], count));
            }

            lines.Add(new SummaryLine(words[0][..mark], deaths, causes));
        }

        return lines;
    }

    /// <summary>One line of the summary: the policy, its count of deaths, and the count of each cause.</summary>
    private sealed record SummaryLine(string Policy, long Deaths, List<CauseCount> Causes);

    /// <summary>The count of deaths of one cause.</summary>
    private sealed record CauseCount(string Cause, long Count);

    /// <summary>True when the text is 40 lowercase hexadecimal digits, the form of a commit in the record.</summary>
    public static bool IsHash(string text)
    {
        if (text.Length != 40)
        {
            return false;
        }

        foreach (char letter in text)
        {
            bool digit = letter >= '0' && letter <= '9';
            bool lower = letter >= 'a' && letter <= 'f';
            if (!digit && !lower)
            {
                return false;
            }
        }

        return true;
    }
}
