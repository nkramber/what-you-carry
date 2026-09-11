using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace WhatYouCarry.Tests;

public sealed class RepositoryShapeTests
{
    [Fact]
    public void AgentFilesAreIdentical()
    {
        // D-122: CLAUDE.md and AGENTS.md are identical pointer files.
        string root = RepositoryRoot.Find();
        byte[] claude = File.ReadAllBytes(Path.Combine(root, "CLAUDE.md"));
        byte[] agents = File.ReadAllBytes(Path.Combine(root, "AGENTS.md"));
        Assert.Equal(claude, agents);
    }

    [Fact]
    public void CoreReferencesNoEngine()
    {
        // G-1: Core has no package reference and no project reference.
        XDocument project = XDocument.Parse(RepositoryRoot.ReadFile("WhatYouCarry.Core/WhatYouCarry.Core.csproj"));
        List<string> references = project.Descendants()
            .Where(element => element.Name.LocalName is "PackageReference" or "ProjectReference" or "Reference")
            .Select(element => $"{element.Name.LocalName} {element.Attribute("Include")?.Value}")
            .ToList();
        Assert.Empty(references);
        Assert.Equal("Microsoft.NET.Sdk", project.Root?.Attribute("Sdk")?.Value);
    }

    [Fact]
    public void SolutionIsSlnx()
    {
        // D-194: the solution is WhatYouCarry.slnx, no .sln exists, and the four projects are listed.
        string root = RepositoryRoot.Find();
        string[] legacySolutions = Directory.GetFiles(root, "*.sln", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}.git{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .ToArray();
        Assert.Empty(legacySolutions);

        XDocument solution = XDocument.Parse(RepositoryRoot.ReadFile(RepositoryRoot.SolutionFileName));
        string[] projectPaths = solution.Descendants("Project")
            .Select(element => element.Attribute("Path")?.Value ?? throw new InvalidOperationException("A Project element has no Path."))
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();
        string[] expected =
        [
            "WhatYouCarry.Core/WhatYouCarry.Core.csproj",
            "WhatYouCarry.Game/WhatYouCarry.Game.csproj",
            "WhatYouCarry.Tests/WhatYouCarry.Tests.csproj",
            "WhatYouCarry.Tools/WhatYouCarry.Tools.csproj",
        ];
        Assert.Equal(expected, projectPaths);
    }

    [Fact]
    public void CiWorkflowHasOneJobPerPlatform()
    {
        // D-71, D-100, D-157: one job per platform, and the macOS job selects the self-hosted runner label.
        string workflow = RepositoryRoot.ReadFile(".github/workflows/ci.yml");
        Dictionary<string, string> runsOnByJob = WorkflowText.RunsOnByJob(workflow);
        Assert.Equal(3, runsOnByJob.Count);
        Assert.Equal("ubuntu-latest", runsOnByJob["linux-x64"]);
        Assert.Equal("windows-latest", runsOnByJob["windows-x64"]);
        Assert.Contains("macos-arm64-self-hosted", runsOnByJob["macos-arm64"], StringComparison.Ordinal);
        Assert.Contains("self-hosted", runsOnByJob["macos-arm64"], StringComparison.Ordinal);
    }

    [Fact]
    public void ReviewGateRunsOnLabelEvent()
    {
        // D-190: the labeled and unlabeled event types start the review-gate workflow.
        string workflow = RepositoryRoot.ReadFile(".github/workflows/review-gate.yml");
        string[] types = WorkflowText.PullRequestTypes(workflow);
        Assert.Contains("labeled", types);
        Assert.Contains("unlabeled", types);
        Assert.Contains("opened", types);
        Assert.Contains("reopened", types);
        Assert.Contains("synchronize", types);
    }

    [Fact]
    public void ReviewGateRunsOnPullRequestTarget()
    {
        // D-197: the workflow and the tool come from the base branch, and no step checks out the PR head.
        string workflow = RepositoryRoot.ReadFile(".github/workflows/review-gate.yml");
        Assert.Contains("\n  pull_request_target:\n", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("\n  pull_request:\n", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("ref: ${{ github.event.pull_request.head", workflow, StringComparison.Ordinal);
        Assert.Contains("refs/pull/${PR_NUMBER}/head", workflow, StringComparison.Ordinal);
    }

    [Fact]
    public void BitIdentityWorkflowHasOneJobPerPlatformAndACompareJob()
    {
        // D-69, D-71, G-9: the sweep runs on the three platforms, and one job compares the three hashes.
        string workflow = RepositoryRoot.ReadFile(".github/workflows/bit-identity.yml");
        Dictionary<string, string> runsOnByJob = WorkflowText.RunsOnByJob(workflow);
        Assert.Equal(4, runsOnByJob.Count);
        Assert.Equal("ubuntu-latest", runsOnByJob["linux-x64"]);
        Assert.Equal("windows-latest", runsOnByJob["windows-x64"]);
        Assert.Contains("macos-arm64-self-hosted", runsOnByJob["macos-arm64"], StringComparison.Ordinal);
        Assert.Equal("ubuntu-latest", runsOnByJob["compare"]);

        // The compare job must wait for all three, or it would compare an absent hash.
        Assert.Contains("needs: [linux-x64, windows-x64, macos-arm64]", workflow, StringComparison.Ordinal);

        // Each platform job passes its hash up, and the compare job reads all three.
        foreach (string job in new[] { "linux-x64", "windows-x64", "macos-arm64" })
        {
            Assert.Contains($"needs.{job}.outputs.hash", workflow, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void DetLintWorkflowScansTheCheckout()
    {
        // D-67, D-202, G-2: one job, on Linux, because the tool parses source text.
        string workflow = RepositoryRoot.ReadFile(".github/workflows/det-lint.yml");
        Dictionary<string, string> runsOnByJob = WorkflowText.RunsOnByJob(workflow);
        KeyValuePair<string, string> job = Assert.Single(runsOnByJob);
        Assert.Equal("det-lint", job.Key);
        Assert.Equal("ubuntu-latest", job.Value);
        Assert.Contains("det-lint --root .", workflow, StringComparison.Ordinal);
    }

    [Fact]
    public void NightGateWorkflowFetchesTheRecordWithFullHistory()
    {
        // D-273, D-275, PR-58: one job on Linux, the full history for the ancestry check, the record from the
        // branch night-results through the tool, and the base branch of the PR as the revision the commit must
        // be on. No shell step writes or reads a night.json in the checkout (PR #40 review P1-1).
        string workflow = RepositoryRoot.ReadFile(".github/workflows/night-gate.yml");
        Dictionary<string, string> runsOnByJob = WorkflowText.RunsOnByJob(workflow);
        KeyValuePair<string, string> job = Assert.Single(runsOnByJob);
        Assert.Equal("night-gate", job.Key);
        Assert.Equal("ubuntu-latest", job.Value);
        Assert.Contains("\n  pull_request:\n", workflow, StringComparison.Ordinal);
        Assert.Contains("fetch-depth: 0", workflow, StringComparison.Ordinal);
        Assert.Contains("night-gate --root \"$GITHUB_WORKSPACE\" --remote origin", workflow, StringComparison.Ordinal);
        Assert.Contains("--base \"origin/${{ github.base_ref }}\"", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("night.json", workflow.Replace("night.json in the checkout", string.Empty), StringComparison.Ordinal);
        Assert.DoesNotContain("git fetch", workflow, StringComparison.Ordinal);
    }

    [Fact]
    public void NightWorkflowRunsAtTwoCentralStandardTime()
    {
        // D-284, D-285, D-288: the night runs at 08:07 UTC, which is 02:07 Central Standard Time, off the start of the hour, and by
        // hand on demand. The comment names the run that never came (F-94) and the run that started late (F-95).
        string workflow = RepositoryRoot.ReadFile(".github/workflows/night.yml");
        Assert.Contains("- cron: \"7 8 * * *\"", workflow, StringComparison.Ordinal);
        Assert.Contains("workflow_dispatch:", workflow, StringComparison.Ordinal);
        Assert.Contains("08:07 UTC, which is 02:07 Central Standard Time", workflow, StringComparison.Ordinal);
        Assert.Contains("(F-94)", workflow, StringComparison.Ordinal);
        Assert.Contains("(F-95)", workflow, StringComparison.Ordinal);
    }

    [Fact]
    public void NightWorkflowKeepsTheLogsOfAFailedNight()
    {
        // D-280: a failed night uploads its bot logs as a run artifact, on failure alone, after every bot step.
        string workflow = RepositoryRoot.ReadFile(".github/workflows/night.yml");
        Assert.Null(UploadStepDefect(workflow));
    }

    [Fact]
    public void NightWorkflowUploadStepMustFollowEveryBotStep()
    {
        // PR #43 review P2-1: the upload step moved before the sweep, or before a bot step, fails the check by name.
        string workflow = RepositoryRoot.ReadFile(".github/workflows/night.yml");
        string beforeSweep = MoveStepBefore(workflow, "Keep the bot logs of a failed night", "Reachability sweep, one hundred thousand seeds");
        Assert.Equal("The upload step comes before the step 'Reachability sweep, one hundred thousand seeds'.", UploadStepDefect(beforeSweep));
        string beforeWalker = MoveStepBefore(workflow, "Keep the bot logs of a failed night", "Random walker, five thousand seeds");
        Assert.Equal("The upload step comes before the step 'Random walker, five thousand seeds'.", UploadStepDefect(beforeWalker));
        Assert.Equal("The night workflow has no upload-artifact step.", UploadStepDefect(workflow.Replace("actions/upload-artifact@v4", "actions/other@v4", StringComparison.Ordinal)));
    }

    /// <summary>The first defect of the upload step in a night workflow text, or null when the step is right (D-280).</summary>
    private static string? UploadStepDefect(string workflow)
    {
        int upload = workflow.IndexOf("uses: actions/upload-artifact@v4", StringComparison.Ordinal);
        if (upload < 0)
        {
            return "The night workflow has no upload-artifact step.";
        }

        int stepStart = workflow.LastIndexOf("- name:", upload, StringComparison.Ordinal);
        int nextStep = workflow.IndexOf("- name:", upload, StringComparison.Ordinal);
        string step = nextStep < 0 ? workflow[stepStart..] : workflow[stepStart..nextStep];
        if (!step.Contains("if: failure()", StringComparison.Ordinal))
        {
            return "The upload step does not run on failure alone.";
        }

        if (!step.Contains("path: bot-logs", StringComparison.Ordinal))
        {
            return "The upload step does not take the bot-logs directory.";
        }

        if (!step.Contains("if-no-files-found: warn", StringComparison.Ordinal))
        {
            return "The upload step does not warn on a night without logs.";
        }

        foreach (string bot in new[] { "Random walker, five thousand seeds", "Greedy descender, five thousand seeds", "Reachability sweep, one hundred thousand seeds" })
        {
            if (workflow.IndexOf(bot, StringComparison.Ordinal) > upload)
            {
                return $"The upload step comes before the step '{bot}'.";
            }
        }

        return null;
    }

    /// <summary>The workflow text with one step cut from its place and put in front of another step, for a regression case.</summary>
    private static string MoveStepBefore(string workflow, string stepName, string targetName)
    {
        int stepStart = workflow.IndexOf($"- name: {stepName}", StringComparison.Ordinal);
        int stepEnd = workflow.IndexOf("- name:", stepStart + 1, StringComparison.Ordinal);
        Assert.True(stepStart >= 0 && stepEnd > stepStart, $"The step '{stepName}' is not followed by another step.");
        int lineStart = workflow.LastIndexOf('\n', stepStart) + 1;
        int lineEnd = workflow.LastIndexOf('\n', stepEnd) + 1;
        string block = workflow[lineStart..lineEnd];
        string without = workflow.Remove(lineStart, lineEnd - lineStart);
        int target = without.IndexOf($"- name: {targetName}", StringComparison.Ordinal);
        Assert.True(target >= 0, $"The step '{targetName}' is absent.");
        int targetLine = without.LastIndexOf('\n', target) + 1;
        return without.Insert(targetLine, block);
    }

    /// <summary>
    /// The last step of the review-gate workflow passes the job on a success conclusion alone, so a neutral
    /// verdict, and a missing or unexpected conclusion, read red. A job cannot be neutral by its exit code
    /// (D-251, F-84, T-2).
    /// </summary>
    [Fact]
    public void ReviewGateJobFailsOnNeutral()
    {
        string workflow = RepositoryRoot.ReadFile(".github/workflows/review-gate.yml");
        Assert.Contains("if [ \"${conclusion}\" != \"success\" ]; then", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("\"${conclusion}\" = \"failure\"", workflow, StringComparison.Ordinal);
    }

    [Fact]
    public void ReviewGateModeFileHoldsAdvisory()
    {
        // D-185: PR-1 creates the mode file with advisory.
        string mode = RepositoryRoot.ReadFile(".github/review-gate-mode");
        Assert.Equal("advisory", mode.Trim());
    }
}

/// <summary>Reads the two workflow facts the tests need from the YAML text. It is not a YAML parser.</summary>
internal static class WorkflowText
{
    /// <summary>Maps each job id under <c>jobs:</c> to the text of its <c>runs-on</c> line.</summary>
    public static Dictionary<string, string> RunsOnByJob(string workflow)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        bool inJobs = false;
        string? currentJob = null;
        foreach (string line in workflow.Split('\n'))
        {
            if (line.StartsWith("jobs:", StringComparison.Ordinal))
            {
                inJobs = true;
                continue;
            }

            if (!inJobs)
            {
                continue;
            }

            if (line.Length > 2 && line.StartsWith("  ", StringComparison.Ordinal) && line[2] != ' ' && line.TrimEnd().EndsWith(':'))
            {
                currentJob = line.Trim().TrimEnd(':');
                continue;
            }

            string trimmed = line.Trim();
            if (currentJob is not null && trimmed.StartsWith("runs-on:", StringComparison.Ordinal))
            {
                result[currentJob] = trimmed["runs-on:".Length..].Trim();
            }
        }

        return result;
    }

    /// <summary>Reads the list in the <c>types:</c> line under <c>pull_request:</c>.</summary>
    public static string[] PullRequestTypes(string workflow)
    {
        foreach (string line in workflow.Split('\n'))
        {
            string trimmed = line.Trim();
            if (!trimmed.StartsWith("types:", StringComparison.Ordinal))
            {
                continue;
            }

            string list = trimmed["types:".Length..].Trim().TrimStart('[').TrimEnd(']');
            return list.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        throw new InvalidOperationException("The workflow has no 'types:' line.");
    }
}
