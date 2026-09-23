using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using WhatYouCarry.Tools.ReviewGate;

namespace WhatYouCarry.Tools.CiSkip;

/// <summary>One run of a workflow on one commit, as the GitHub API gives it. A run in progress has no conclusion.</summary>
public sealed record WorkflowRun(long Id, string Status, string? Conclusion, DateTimeOffset CreatedAt);

/// <summary>
/// Everything the CI skip rules need, read once from git and from the file of runs that the workflow step writes.
/// The rules do no I/O.
/// </summary>
/// <remarks>
/// The paths of the PR come from the merge base of the base commit and the head, as the review gate reads them
/// (D-179). The paths of the push come from the previous head of a synchronize event. A new PR, a reopen, and a force
/// push give no previous head that is an ancestor of the head, so the push paths stay null, and rule 2 does not apply.
/// </remarks>
public sealed class CiSkipFacts
{
    public const string SynchronizeAction = "synchronize";

    /// <summary>The <c>before</c> value of an event that gives no previous commit.</summary>
    public const string NoCommit = "0000000000000000000000000000000000000000";

    /// <summary>The name of the event that started the workflow, for example <c>pull_request</c> or <c>push</c>.</summary>
    public required string EventName { get; init; }

    /// <summary>Every path that the PR changes from the merge base, or null when the event is not a pull request.</summary>
    public required IReadOnlyList<string>? PullRequestPaths { get; init; }

    /// <summary>Every path that the push changes from the previous head, or null when no previous head applies.</summary>
    public required IReadOnlyList<string>? PushPaths { get; init; }

    /// <summary>The runs of the workflow on the previous head, or null when no previous head applies.</summary>
    public required IReadOnlyList<WorkflowRun>? PreviousRuns { get; init; }

    /// <summary>Reads the paths from git and the runs from their file.</summary>
    /// <exception cref="InvalidOperationException">A git command failed, or a line of the runs file is not a run. The message names it.</exception>
    /// <exception cref="FileNotFoundException">The runs file is absent when a previous head applies. The message names the path.</exception>
    public static CiSkipFacts Gather(string root, string eventName, string action, string baseCommit, string head, string before, string runsPath)
    {
        if (eventName != CiSkipRules.PullRequestEvent)
        {
            return new CiSkipFacts { EventName = eventName, PullRequestPaths = null, PushPaths = null, PreviousRuns = null };
        }

        var git = new GitRepository(root);
        IReadOnlyList<string> pullRequestPaths = git.ChangedPaths(git.MergeBase(baseCommit, head), head);
        if (!HasPreviousHead(git, action, before, head))
        {
            return new CiSkipFacts { EventName = eventName, PullRequestPaths = pullRequestPaths, PushPaths = null, PreviousRuns = null };
        }

        if (!File.Exists(runsPath))
        {
            throw new FileNotFoundException($"The runs file '{runsPath}' does not exist. The workflow step writes it from the GitHub API before the command runs.", runsPath);
        }

        return new CiSkipFacts
        {
            EventName = eventName,
            PullRequestPaths = pullRequestPaths,
            PushPaths = git.ChangedPaths(before, head),
            PreviousRuns = ParseRuns(runsPath, File.ReadAllText(runsPath)),
        };
    }

    /// <summary>Reads one JSON object per line: <c>id</c>, <c>status</c>, <c>conclusion</c>, and <c>created_at</c>. A blank line is no run.</summary>
    /// <exception cref="InvalidOperationException">A line is not a run. The message names the file, the line, and the field.</exception>
    public static IReadOnlyList<WorkflowRun> ParseRuns(string name, string text)
    {
        var runs = new List<WorkflowRun>();
        string[] lines = text.Split('\n');
        for (int index = 0; index < lines.Length; index++)
        {
            string line = lines[index].Trim();
            if (line.Length == 0)
            {
                continue;
            }

            runs.Add(ParseRun($"{name}:{index + 1}", line));
        }

        return runs;
    }

    private static bool HasPreviousHead(GitRepository git, string action, string before, string head)
    {
        if (action != SynchronizeAction || before.Length == 0 || before == NoCommit)
        {
            return false;
        }

        return git.HasCommit(before) && git.IsAncestor(before, head);
    }

    private static WorkflowRun ParseRun(string place, string line)
    {
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(line);
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException($"{place}: the line is not JSON: {exception.Message}", exception);
        }

        using (document)
        {
            JsonElement run = document.RootElement;
            if (run.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidOperationException($"{place}: the line is a {run.ValueKind}, and a run is an object.");
            }

            long id = Field(place, run, "id", JsonValueKind.Number).GetInt64();
            string status = Field(place, run, "status", JsonValueKind.String).GetString()!;
            string createdText = Field(place, run, "created_at", JsonValueKind.String).GetString()!;
            if (!DateTimeOffset.TryParse(createdText, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTimeOffset createdAt))
            {
                throw new InvalidOperationException($"{place}: the field 'created_at' holds '{createdText}', and the form is an ISO 8601 time.");
            }

            // The API gives a null conclusion for a run in progress, so null is a fact here, and an absent field is not.
            JsonElement conclusion = Field(place, run, "conclusion", JsonValueKind.Undefined);
            if (conclusion.ValueKind != JsonValueKind.Null && conclusion.ValueKind != JsonValueKind.String)
            {
                throw new InvalidOperationException($"{place}: the field 'conclusion' is a {conclusion.ValueKind}, and the API gives a string or null.");
            }

            return new WorkflowRun(id, status, conclusion.ValueKind == JsonValueKind.Null ? null : conclusion.GetString(), createdAt);
        }
    }

    /// <summary>Returns the field. <see cref="JsonValueKind.Undefined"/> as the kind accepts any kind.</summary>
    private static JsonElement Field(string place, JsonElement run, string name, JsonValueKind kind)
    {
        if (!run.TryGetProperty(name, out JsonElement element))
        {
            throw new InvalidOperationException($"{place}: the field '{name}' is absent.");
        }

        if (kind != JsonValueKind.Undefined && element.ValueKind != kind)
        {
            throw new InvalidOperationException($"{place}: the field '{name}' is a {element.ValueKind}, and the API gives a {kind}.");
        }

        return element;
    }
}
