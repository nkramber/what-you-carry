using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using WhatYouCarry.Tools.ReviewGate;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// The ruleset of <c>main</c> in <c>.github/rulesets/main.json</c> (D-516, D-520, D-522). A required check that no job
/// reports blocks every merge, and a job that the ruleset does not require passes a merge in silence. These tests
/// bind the file to the workflows, so a renamed or a new job fails here.
/// </summary>
public sealed class RulesetTests
{
    private const string RulesetPath = ".github/rulesets/main.json";

    /// <summary>The app id of GitHub Actions. Each required check comes from it, so another app cannot report a name.</summary>
    private const int GitHubActionsAppId = 15368;

    /// <summary>
    /// The pull request jobs that no rule requires: the ci-skip job of each heavy workflow, which a heavy job never
    /// trusts in silence (D-477), and the evaluate job, which posts the required review-gate check run (D-181).
    /// </summary>
    private static readonly string[] NotRequired = ["ci-skip", "evaluate"];

    [Fact]
    public void TheRulesetRequiresEachCheckOnceFromGitHubActions()
    {
        List<(string Context, int AppId)> checks = RequiredChecks();

        Assert.Equal(checks.Count, checks.Select(check => check.Context).Distinct(StringComparer.Ordinal).Count());
        Assert.All(checks, check => Assert.Equal(GitHubActionsAppId, check.AppId));
        Assert.Contains((ReviewGateRules.CheckName, GitHubActionsAppId), checks);
    }

    [Fact]
    public void EachRequiredCheckIsTheNameOfOneJob()
    {
        Dictionary<string, int> jobCounts = PullRequestCheckNames();
        foreach ((string context, _) in RequiredChecks())
        {
            if (context == ReviewGateRules.CheckName)
            {
                continue;
            }

            Assert.True(jobCounts.TryGetValue(context, out int count), $"The required check '{context}' is the name of no job of a pull request workflow, so it never reports.");
            Assert.True(count == 1, $"The required check '{context}' is the name of {count} jobs, so a pass of one hides a failure of another.");
        }
    }

    [Fact]
    public void EachPullRequestJobIsRequired()
    {
        HashSet<string> required = RequiredChecks().Select(check => check.Context).ToHashSet(StringComparer.Ordinal);
        foreach (string name in PullRequestCheckNames().Keys)
        {
            Assert.True(required.Contains(name) || NotRequired.Contains(name), $"The job '{name}' runs on each PR, and the ruleset of main does not require it.");
        }
    }

    [Fact]
    public void TheReviewGateWorkflowPostsTheRequiredCheck()
    {
        string workflow = RepositoryRoot.ReadFile(".github/workflows/review-gate.yml");
        Assert.Contains("\"repos/${GITHUB_REPOSITORY}/check-runs\"", workflow, StringComparison.Ordinal);
        Assert.Equal("review-gate", ReviewGateRules.CheckName);
    }

    [Fact]
    public void TheRulesetGuardsMainWithSquashAndResolvedThreads()
    {
        using JsonDocument document = JsonDocument.Parse(RepositoryRoot.ReadFile(RulesetPath));
        JsonElement ruleset = document.RootElement;

        Assert.Equal("active", ruleset.GetProperty("enforcement").GetString());
        Assert.Equal("refs/heads/main", Assert.Single(ruleset.GetProperty("conditions").GetProperty("ref_name").GetProperty("include").EnumerateArray()).GetString());

        JsonElement pullRequest = Parameters(ruleset, "pull_request");
        Assert.Equal("squash", Assert.Single(pullRequest.GetProperty("allowed_merge_methods").EnumerateArray()).GetString());
        Assert.True(pullRequest.GetProperty("required_review_thread_resolution").GetBoolean());
        Assert.Equal(0, pullRequest.GetProperty("required_approving_review_count").GetInt32());

        // GitHub adds these two fields when the file omits them, and its default of true asks for an approval that
        // the one owner cannot give to a commit of an unlinked author, so auto-merge waits forever (owner choice,
        // 2026-09-23). The file declares both, so the comparison of the live ruleset stays empty.
        Assert.False(pullRequest.GetProperty("require_extra_approval_for_unattributed_changes").GetBoolean());
        Assert.Empty(pullRequest.GetProperty("required_reviewers").EnumerateArray());

        // PRs go one at a time, so a rule that the branch holds the newest main only forces a rebase (D-522).
        Assert.False(Parameters(ruleset, "required_status_checks").GetProperty("strict_required_status_checks_policy").GetBoolean());
    }

    [Fact]
    public void TheOwnerBypassesThroughAPullRequestAlone()
    {
        // D-520: the admin role bypasses on a PR merge, for a night gate deadlock, and never by a direct push.
        using JsonDocument document = JsonDocument.Parse(RepositoryRoot.ReadFile(RulesetPath));
        JsonElement actor = Assert.Single(document.RootElement.GetProperty("bypass_actors").EnumerateArray());

        Assert.Equal("RepositoryRole", actor.GetProperty("actor_type").GetString());
        Assert.Equal(5, actor.GetProperty("actor_id").GetInt32());
        Assert.Equal("pull_request", actor.GetProperty("bypass_mode").GetString());
    }

    [Fact]
    public void TheMakeTargetUpdatesTheCliAndRunsTheTool()
    {
        // D-511, D-512: the target stays thin, and the update comes first.
        string makefile = RepositoryRoot.ReadFile("Makefile");
        int update = makefile.IndexOf("\tnpm install -g @openai/codex@latest\n", StringComparison.Ordinal);
        int review = makefile.IndexOf("\t$(TOOLS) codex-review --root . --pr $(PR) --codex $(CODEX) $(CODEX_REVIEW_FLAGS)\n", StringComparison.Ordinal);

        Assert.True(update > 0, "The codex-review target does not update the CLI.");
        Assert.True(review > update, "The codex-review target does not run the tool after the update.");
        Assert.Contains("codex-review: ## ", makefile, StringComparison.Ordinal);
    }

    [Fact]
    public void TheMakeTargetPassesTheFlagsAfterTheDoubleDash()
    {
        // D-543: make reads each word after `--` as a goal. The target passes each goal that starts with `--` to the
        // tool, and a rule that does nothing keeps make from a "No rule to make target" stop.
        string makefile = RepositoryRoot.ReadFile("Makefile");

        Assert.Contains("CODEX_REVIEW_FLAGS := $(filter --%,$(MAKECMDGOALS))\n", makefile, StringComparison.Ordinal);
        Assert.Contains("\n--%:\n\t@:\n", makefile, StringComparison.Ordinal);
    }

    private static List<(string Context, int AppId)> RequiredChecks()
    {
        using JsonDocument document = JsonDocument.Parse(RepositoryRoot.ReadFile(RulesetPath));
        var checks = new List<(string, int)>();
        foreach (JsonElement check in Parameters(document.RootElement, "required_status_checks").GetProperty("required_status_checks").EnumerateArray())
        {
            checks.Add((check.GetProperty("context").GetString() ?? throw new InvalidOperationException("A required check has a null context."), check.GetProperty("integration_id").GetInt32()));
        }

        return checks;
    }

    private static JsonElement Parameters(JsonElement ruleset, string type)
    {
        foreach (JsonElement rule in ruleset.GetProperty("rules").EnumerateArray())
        {
            if (rule.GetProperty("type").GetString() == type)
            {
                return rule.GetProperty("parameters").Clone();
            }
        }

        throw new InvalidOperationException($"'{RulesetPath}' has no rule of the type '{type}'.");
    }

    /// <summary>The check name of each job of each workflow that runs on a PR, with the count of jobs that carry it.</summary>
    private static Dictionary<string, int> PullRequestCheckNames()
    {
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);
        string directory = Path.Combine(RepositoryRoot.Find(), ".github", "workflows");
        foreach (string path in Directory.GetFiles(directory, "*.yml").OrderBy(path => path, StringComparer.Ordinal))
        {
            string workflow = File.ReadAllText(path).Replace("\r\n", "\n", StringComparison.Ordinal);
            if (!workflow.Contains("\n  pull_request:", StringComparison.Ordinal) && !workflow.Contains("\n  pull_request_target:", StringComparison.Ordinal))
            {
                continue;
            }

            foreach (string name in WorkflowText.CheckNameByJob(workflow).Values)
            {
                counts[name] = counts.GetValueOrDefault(name) + 1;
            }
        }

        return counts;
    }
}
