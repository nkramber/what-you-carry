using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace WhatYouCarry.Tools.BotRunner;

/// <summary>
/// <c>night-record --commit &lt;sha&gt; --status &lt;success|failure|cancelled&gt; --output &lt;file&gt;</c>. Writes the
/// record of one night, scheduled or by hand: the commit it tested, the end time, and the status (D-177, D-273,
/// D-274). The night job commits the file to the branch <c>night-results</c>, and the <c>night-gate</c> command reads it.
/// </summary>
public static class NightRecordCommand
{
    /// <summary>The three field names of the record.</summary>
    public const string CommitName = "commit";
    public const string EndedAtName = "endedAt";
    public const string StatusName = "status";

    /// <summary>The statuses that a night can end with. The gate passes on success alone (D-177).</summary>
    public static readonly string[] Statuses = ["success", "failure", "cancelled"];

    private const string Usage = "Usage: night-record --commit <sha> --status <success|failure|cancelled> --output <file>";

    public static int Run(string[] args)
    {
        string? commit = null;
        string? status = null;
        string? output = null;
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

        // UTF-8 without the byte-order mark: Encoding.UTF8 writes one, and the record is one JSON object from its first byte.
        File.WriteAllText(output, Build(commit, DateTime.UtcNow, status), new UTF8Encoding(false));
        Console.Out.WriteLine($"night-record: {output} holds commit {commit} with status {status}.");
        return 0;
    }

    /// <summary>The record as one JSON object with a line break. The time is UTC in the round-trip form.</summary>
    public static string Build(string commit, DateTime endedAt, string status)
    {
        string time = endedAt.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);
        return $"{{\"{CommitName}\":\"{commit}\",\"{EndedAtName}\":\"{time}\",\"{StatusName}\":\"{status}\"}}\n";
    }

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
