using System;
using System.Collections.Generic;

namespace WhatYouCarry.Tools.CodexReview;

/// <summary>One author entry of the handoff: the heading, and the value of the <c>Author:</c> field, or null when the entry has none.</summary>
public sealed record AuthorEntry(string Heading, string? Author);

/// <summary>
/// The provider gate of the review commands (T-4, D-649). The provider that wrote the PR does not review it. Each
/// author entry of the handoff that names the PR branch gives the provider of the author in its <c>Author:</c> field,
/// which holds exactly <c>Claude Code</c> or <c>Codex</c>. A reviewer entry of the same branch names the reviewing
/// provider, so the gate reads the author entries alone. The session line gives the role after the PR, as in
/// <c>Session: PR-91, author, merge. Branch `feat/pr-91-review-fixes`.</c>
/// </summary>
public static class ProviderGate
{
    public const string HandoffPath = "docs/session-handoff.md";
    public const string EntryHeading = "## Session ";
    public const string SessionField = "Session: ";
    public const string AuthorField = "Author: ";
    public const string AuthorRole = "author";

    /// <summary>
    /// The problem of the handoff at the PR head, or null when the handoff has an author entry of the branch and each
    /// author entry of the branch names <paramref name="requiredAuthor"/>. A PR that both providers wrote has no
    /// eligible reviewer, so one entry of the reviewing provider is a problem too.
    /// </summary>
    public static string? Problem(string? handoff, string branch, string requiredAuthor)
    {
        if (handoff is null)
        {
            return $"The PR head has no '{HandoffPath}', so the provider gate cannot read the `Author:` field of an author entry (T-4, D-649).";
        }

        List<AuthorEntry> entries = AuthorEntries(handoff, branch);
        if (entries.Count == 0)
        {
            return $"No author entry of '{HandoffPath}' names Branch `{branch}`, so the provider gate cannot read the `Author:` field. The review needs an author entry with `Author: {requiredAuthor}` (T-4, D-649).";
        }

        foreach (AuthorEntry entry in entries)
        {
            if (entry.Author != requiredAuthor)
            {
                string found = entry.Author is null ? "no `Author:` field" : $"the field `Author: {entry.Author}`";
                return $"The author entry '{entry.Heading}' of Branch `{branch}` in '{HandoffPath}' has {found}, and this review needs `Author: {requiredAuthor}`, the other provider (T-4, D-649).";
            }
        }

        return null;
    }

    /// <summary>
    /// The author entries of the branch, newest first. An entry starts at a line that starts with
    /// <see cref="EntryHeading"/>. It is an author entry of the branch when its session line names the branch in the
    /// form Branch and the name in backticks (D-376), and the role after the first comma is <see cref="AuthorRole"/>.
    /// </summary>
    public static List<AuthorEntry> AuthorEntries(string handoff, string branch)
    {
        var entries = new List<AuthorEntry>();
        string branchText = $"Branch `{branch}`";
        string? heading = null;
        string? author = null;
        bool isAuthorEntry = false;
        foreach (string rawLine in (handoff + "\n" + EntryHeading).Split('\n'))
        {
            string line = rawLine.TrimEnd('\r');
            if (line.StartsWith(EntryHeading, StringComparison.Ordinal))
            {
                // The sentinel heading at the end closes the last entry.
                if (heading is not null && isAuthorEntry)
                {
                    entries.Add(new AuthorEntry(heading, author));
                }

                heading = line;
                author = null;
                isAuthorEntry = false;
                continue;
            }

            if (heading is null)
            {
                continue;
            }

            if (line.StartsWith(AuthorField, StringComparison.Ordinal))
            {
                author = line[AuthorField.Length..].Trim();
            }
            else if (line.StartsWith(SessionField, StringComparison.Ordinal) && line.Contains(branchText, StringComparison.Ordinal))
            {
                isAuthorEntry = RoleOf(line) == AuthorRole;
            }
        }

        return entries;
    }

    /// <summary>The role in a session line: the text after the first comma, up to the next comma or period.</summary>
    private static string RoleOf(string sessionLine)
    {
        int comma = sessionLine.IndexOf(", ", StringComparison.Ordinal);
        if (comma < 0)
        {
            return string.Empty;
        }

        string rest = sessionLine[(comma + 2)..];
        int end = rest.IndexOfAny([',', '.']);
        return end < 0 ? rest.Trim() : rest[..end].Trim();
    }
}
