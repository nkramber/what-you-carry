using System;
using System.IO;
using WhatYouCarry.Tools.ReviewGate;

namespace WhatYouCarry.Tools.NightGate;

/// <summary>Everything the night gate rules need, read once from the record file and git. The rules do no I/O.</summary>
public sealed class NightGateFacts
{
    /// <summary>The text of the record file, or null when the file is absent.</summary>
    public required string? RecordText { get; init; }

    /// <summary>The parsed record, or null when the file is absent or the text is not a record.</summary>
    public required NightRecord? Record { get; init; }

    /// <summary>The reason the text is not a record, or null when it is one or the file is absent.</summary>
    public required string? ParseError { get; init; }

    /// <summary>True when the record commit is on the base branch, false when it is not or the checkout lacks it, and null without a record.</summary>
    public required bool? CommitOnBase { get; init; }

    /// <summary>The base branch as a git revision, for example <c>origin/main</c>.</summary>
    public required string BaseRef { get; init; }

    /// <summary>The time of the evaluation, in UTC.</summary>
    public required DateTimeOffset Now { get; init; }

    /// <summary>Reads the record file and asks git about its commit.</summary>
    public static NightGateFacts Gather(string recordPath, string root, string baseRef, DateTimeOffset now)
    {
        string? text = File.Exists(recordPath) ? File.ReadAllText(recordPath) : null;
        NightRecord? record = null;
        string? parseError = null;
        bool? commitOnBase = null;
        if (text is not null)
        {
            record = NightRecordParser.TryParse(text, out string error);
            if (record is null)
            {
                parseError = error;
            }
            else
            {
                var git = new GitRepository(root);
                commitOnBase = git.HasCommit(record.Commit) && git.IsAncestor(record.Commit, baseRef);
            }
        }

        return new NightGateFacts
        {
            RecordText = text,
            Record = record,
            ParseError = parseError,
            CommitOnBase = commitOnBase,
            BaseRef = baseRef,
            Now = now,
        };
    }
}
