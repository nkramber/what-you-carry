using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using WhatYouCarry.Tools.DocGate;
using WhatYouCarry.Tools.NightGate;
using Xunit;

namespace WhatYouCarry.Tests;

[Trait("Category", DocumentsCategoryTests.DocumentsCategory)]
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
    public void AssetsReferencesNoEngine()
    {
        // D-299: Assets has no package reference, and its one project reference is Core.
        XDocument project = XDocument.Parse(RepositoryRoot.ReadFile("WhatYouCarry.Assets/WhatYouCarry.Assets.csproj"));
        List<string> references = project.Descendants()
            .Where(element => element.Name.LocalName is "PackageReference" or "ProjectReference" or "Reference")
            .Select(element => $"{element.Name.LocalName} {element.Attribute("Include")?.Value}")
            .ToList();
        Assert.Equal(["ProjectReference ../WhatYouCarry.Core/WhatYouCarry.Core.csproj"], references);
        Assert.Equal("Microsoft.NET.Sdk", project.Root?.Attribute("Sdk")?.Value);
    }

    [Fact]
    public void SolutionIsSlnx()
    {
        // D-194: the solution is WhatYouCarry.slnx, no .sln exists, and the five projects are listed (D-299).
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
            "WhatYouCarry.Assets/WhatYouCarry.Assets.csproj",
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
        // D-71, D-100, D-157: each platform runs the tests, and the macOS job selects the self-hosted runner label.
        // The ci-skip job and the documents job run on Linux (D-474, D-476).
        string workflow = RepositoryRoot.ReadFile(".github/workflows/ci.yml");
        Dictionary<string, string> runsOnByJob = WorkflowText.RunsOnByJob(workflow);
        // Each hosted leg runs its tests in two jobs (D-479).
        Assert.Equal(7, runsOnByJob.Count);
        Assert.Equal("ubuntu-latest", runsOnByJob["ci-skip"]);
        Assert.Equal("ubuntu-latest", runsOnByJob["documents"]);
        Assert.Equal("ubuntu-latest", runsOnByJob["linux-x64"]);
        Assert.Equal("ubuntu-latest", runsOnByJob["linux-x64-sweeps"]);
        Assert.Equal("windows-latest", runsOnByJob["windows-x64"]);
        Assert.Equal("windows-latest", runsOnByJob["windows-x64-sweeps"]);
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
        Assert.Equal(5, runsOnByJob.Count);
        Assert.Equal("ubuntu-latest", runsOnByJob["ci-skip"]);
        Assert.Equal("ubuntu-latest", runsOnByJob["linux-x64"]);
        Assert.Equal("windows-latest", runsOnByJob["windows-x64"]);
        Assert.Contains("macos-arm64-self-hosted", runsOnByJob["macos-arm64"], StringComparison.Ordinal);
        Assert.Equal("ubuntu-latest", runsOnByJob["compare"]);

        // The compare job must wait for all three, or it would compare an absent hash. It skips with them (D-474).
        Assert.Contains("needs: [ci-skip, linux-x64, windows-x64, macos-arm64]", workflow, StringComparison.Ordinal);

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

        // D-538, D-547: the head branch and the head commit reach the tool through the environment alone.
        Assert.Contains("HEAD_BRANCH: ${{ github.head_ref }}", workflow, StringComparison.Ordinal);
        Assert.Contains("HEAD_SHA: ${{ github.event.pull_request.head.sha }}", workflow, StringComparison.Ordinal);
        Assert.Contains("--head-branch \"$HEAD_BRANCH\" --head \"$HEAD_SHA\"", workflow, StringComparison.Ordinal);
        Assert.Equal(2, workflow.Split("${{ github.head_ref }}").Length);
        Assert.DoesNotContain("night.json", workflow.Replace("night.json in the checkout", string.Empty), StringComparison.Ordinal);
        Assert.DoesNotContain("git fetch", workflow, StringComparison.Ordinal);
    }

    [Fact]
    public void NightWorkflowRunsAtOneCentralStandardTime()
    {
        // D-571: the night runs at 07:07 UTC, which is 01:07 Central Standard Time, off the start of the hour (D-285), and
        // by hand on demand. The comment names the run that never came (F-94) and the run that started late (F-95).
        string workflow = RepositoryRoot.ReadFile(".github/workflows/night.yml");
        Assert.Contains("- cron: \"7 7 * * *\"", workflow, StringComparison.Ordinal);
        Assert.Equal(2, workflow.Split("- cron:").Length);
        Assert.Contains("workflow_dispatch:", workflow, StringComparison.Ordinal);
        Assert.Contains("07:07 UTC, which is 01:07 Central Standard Time", workflow, StringComparison.Ordinal);
        Assert.Contains("(D-571)", workflow, StringComparison.Ordinal);
        Assert.Contains("(F-94)", workflow, StringComparison.Ordinal);
        Assert.Contains("(F-95)", workflow, StringComparison.Ordinal);
    }

    [Fact]
    public void TheNightRunsOnHostedLinuxAsOneJobForEachSweep()
    {
        // D-572, D-573: one plan job, one sweep job for each sweep of NightSeeds.Sweeps, and one record job, all on hosted
        // Linux. A failed sweep does not stop the others, and a hosted job stops at 6 hours. The record job alone
        // holds the write permissions.
        string workflow = RepositoryRoot.ReadFile(".github/workflows/night.yml");
        Dictionary<string, string> jobs = WorkflowText.RunsOnByJob(workflow);
        Assert.Equal(new[] { "plan", "record", "sweep" }, jobs.Keys.Order(StringComparer.Ordinal));
        Assert.All(jobs.Values, runsOn => Assert.Equal("ubuntu-latest", runsOn));
        Assert.DoesNotContain("self-hosted", workflow, StringComparison.Ordinal);

        Assert.Contains($"\n        sweep: [{string.Join(", ", NightSeeds.Sweeps)}]\n", workflow, StringComparison.Ordinal);
        Assert.Contains("\n      fail-fast: false\n", workflow, StringComparison.Ordinal);
        Assert.Contains("\n    timeout-minutes: 360\n", workflow, StringComparison.Ordinal);
        Assert.Contains("\n  sweep:\n    needs: plan\n", workflow, StringComparison.Ordinal);
        Assert.Contains("\n  record:\n    needs: [plan, sweep]\n", workflow, StringComparison.Ordinal);
        Assert.Contains("\npermissions:\n  contents: read\n\njobs:\n", workflow, StringComparison.Ordinal);
        Assert.Contains("\n    permissions:\n      contents: write\n      actions: write\n      pull-requests: read\n    steps:\n", workflow, StringComparison.Ordinal);
        Assert.Equal(2, workflow.Split("contents: write").Length);
    }

    [Fact]
    public void NightWorkflowKeepsTheLogsOfAFailedNight()
    {
        // D-280: a failed sweep uploads its bot logs as a run artifact, on failure alone, after both sweep steps.
        string workflow = RepositoryRoot.ReadFile(".github/workflows/night.yml");
        Assert.Null(UploadStepDefect(workflow));
    }

    [Fact]
    public void NightWorkflowUploadStepMustFollowEveryBotStep()
    {
        // PR #43 review P2-1: the upload step moved before the seed sweep, or before the bot sweep, fails the check by name.
        string workflow = RepositoryRoot.ReadFile(".github/workflows/night.yml");
        string beforeSeeds = MoveStepBefore(workflow, "Keep the bot logs of a failed night", "Seed sweep, the fixed seeds and the slice");
        Assert.Equal("The upload step comes before the step 'Seed sweep, the fixed seeds and the slice'.", UploadStepDefect(beforeSeeds));
        string beforeBots = MoveStepBefore(workflow, "Keep the bot logs of a failed night", "Bot sweep, the fixed seeds and the slice");
        Assert.Equal("The upload step comes before the step 'Bot sweep, the fixed seeds and the slice'.", UploadStepDefect(beforeBots));
        Assert.Equal("The bot log step uploads no artifact.", UploadStepDefect(workflow.Replace("actions/upload-artifact@v4", "actions/other@v4", StringComparison.Ordinal)));
    }

    [Fact]
    public void NightAndBotWorkflowsRunEveryPolicyAndGatherTheDeaths()
    {
        // D-403: the record of a night carries the count of deaths of each policy, so every bot sweep writes one
        // summary file, and the record job gathers them and reads the result. D-149: the full clearer joins the two
        // policies of PR-11, and the timer tester joins them in PR-17 (D-421, D-426).
        string night = RepositoryRoot.ReadFile(".github/workflows/night.yml");
        string bots = RepositoryRoot.ReadFile(".github/workflows/bots.yml");
        string[] policies = ["random-walker", "greedy-descender", "full-clearer", "timer-tester"];
        foreach (string policy in policies)
        {
            Assert.Contains(policy, NightSeeds.Sweeps);
            Assert.Contains($"--policy {policy} --seeds 1-100 --output bot-logs --root .", bots, StringComparison.Ordinal);
        }

        string botSweep = StepText(night, "Bot sweep, the fixed seeds and the slice");
        Assert.Contains("if: matrix.sweep != 'reachability'", botSweep, StringComparison.Ordinal);
        Assert.Contains("bot-run --policy \"$SWEEP\" --seeds \"$NIGHT_SEEDS\" --output bot-logs --root . --summary \"${RUNNER_TEMP}/sweep/bot-deaths.txt\"", botSweep, StringComparison.Ordinal);
        Assert.Contains("SWEEP: ${{ matrix.sweep }}", night, StringComparison.Ordinal);
        Assert.Contains(": > \"${RUNNER_TEMP}/sweep/bot-deaths.txt\"", StepText(night, "Start the sweep result"), StringComparison.Ordinal);
        Assert.Contains("--summary \"${RUNNER_TEMP}/bot-deaths.txt\"", StepText(night, "Write the night record"), StringComparison.Ordinal);
    }

    [Fact]
    public void TheNightPlansTheSeedsOfEachSweep()
    {
        // D-564 to D-567: the plan job takes the UTC date and the record of main one time, and each sweep job reads both.
        // Each sweep job lists its seeds with night-seeds and keeps its failure line in its result. The record job
        // gathers the results and reads the date, the failures, and the record of main of the plan.
        string workflow = RepositoryRoot.ReadFile(".github/workflows/night.yml");
        string plan = StepText(workflow, "Plan the seeds");
        Assert.Contains("echo \"date=$(date -u +%Y-%m-%d)\" >> \"$GITHUB_OUTPUT\"", plan, StringComparison.Ordinal);
        Assert.Contains("git fetch --quiet origin refs/heads/night-results", plan, StringComparison.Ordinal);
        Assert.Contains("git show FETCH_HEAD:night.json > \"${RUNNER_TEMP}/main-night.json\"", plan, StringComparison.Ordinal);
        Assert.Contains("date: ${{ steps.plan.outputs.date }}", workflow, StringComparison.Ordinal);
        string keep = StepText(workflow, "Keep the record of main for each job");
        Assert.Contains("name: main-night\n", keep, StringComparison.Ordinal);
        Assert.Contains("if-no-files-found: error", keep, StringComparison.Ordinal);
        Assert.Equal(4, workflow.Split("name: main-night\n").Length);
        Assert.Equal(3, workflow.Split("NIGHT_DATE: ${{ needs.plan.outputs.date }}").Length);

        string start = StepText(workflow, "Start the sweep result");
        Assert.Contains(": > \"${RUNNER_TEMP}/sweep/seed-failures.txt\"", start, StringComparison.Ordinal);
        Assert.True(workflow.IndexOf("- name: Start the sweep result", StringComparison.Ordinal) < workflow.IndexOf("run: dotnet build WhatYouCarry.slnx", StringComparison.Ordinal), "The sweep result starts before the build, so a broken build still leaves a result.");
        string list = StepText(workflow, "List the seeds of the sweep");
        Assert.Contains("set -euo pipefail", list, StringComparison.Ordinal);
        Assert.Contains("seeds=$(dotnet run --project WhatYouCarry.Tools/WhatYouCarry.Tools.csproj --no-build -- night-seeds --sweep \"$SWEEP\" --date \"$NIGHT_DATE\" --root . --carry \"${RUNNER_TEMP}/main-night.json\")", list, StringComparison.Ordinal);
        Assert.Contains("echo \"NIGHT_SEEDS=${seeds}\" >> \"$GITHUB_ENV\"", list, StringComparison.Ordinal);
        Assert.Contains("--failures \"${RUNNER_TEMP}/sweep/seed-failures.txt\"", StepText(workflow, "Bot sweep, the fixed seeds and the slice"), StringComparison.Ordinal);

        string sweep = StepText(workflow, "Seed sweep, the fixed seeds and the slice");
        Assert.Contains($"if: matrix.sweep == '{NightSeeds.ReachabilitySweep}'", sweep, StringComparison.Ordinal);
        Assert.Contains("WYC_NIGHT_SEEDS=\"$NIGHT_SEEDS\"", sweep, StringComparison.Ordinal);
        Assert.Contains("export WYC_NIGHT_SEEDS", sweep, StringComparison.Ordinal);
        Assert.Contains("WYC_NIGHT_FAILURES: ${{ runner.temp }}/sweep/seed-failures.txt", sweep, StringComparison.Ordinal);
        Assert.Contains("WYC_NIGHT_SWEEP: \"1\"", sweep, StringComparison.Ordinal);

        string result = StepText(workflow, "Keep the sweep result for the record");
        Assert.Contains("if: always()", result, StringComparison.Ordinal);
        Assert.Contains("name: night-sweep-${{ matrix.sweep }}", result, StringComparison.Ordinal);
        Assert.Contains("pattern: night-sweep-*", StepText(workflow, "Take the result of each sweep"), StringComparison.Ordinal);
        string gather = StepText(workflow, "Gather the sweep results");
        Assert.Contains("status=$(bash .github/scripts/night-gather.sh \"${RUNNER_TEMP}/sweeps\" \"${RUNNER_TEMP}\" \"$PLAN_RESULT\" \"$SWEEP_RESULT\" \"$RECORD_STATUS\")", gather, StringComparison.Ordinal);
        Assert.Contains("SWEEP_RESULT: ${{ needs.sweep.result }}", gather, StringComparison.Ordinal);
        Assert.Contains("RECORD_STATUS: ${{ job.status }}", gather, StringComparison.Ordinal);

        // PR #100 review P1-1: a night with no binary writes its failure record with the script, from the record of main.
        string publish = StepText(workflow, "Publish the night record");
        Assert.Contains("bash .github/scripts/night-failure-record.sh \"${GITHUB_SHA}\" \"${RUNNER_TEMP}/main-night-now.json\" \"${RUNNER_TEMP}/night.json\"", publish, StringComparison.Ordinal);
        Assert.Contains("git show FETCH_HEAD:night.json > \"${RUNNER_TEMP}/main-night-now.json\"", publish, StringComparison.Ordinal);
        Assert.Contains("git commit -qm \"night: ${GITHUB_SHA} ${status}\"", publish, StringComparison.Ordinal);
        Assert.DoesNotContain("printf '{\"commit\"", publish, StringComparison.Ordinal);

        string record = StepText(workflow, "Write the night record");
        Assert.Contains("--status \"${NIGHT_STATUS}\"", record, StringComparison.Ordinal);
        Assert.Contains("--date \"${NIGHT_DATE}\" --failures \"${RUNNER_TEMP}/seed-failures.txt\" --carry \"${RUNNER_TEMP}/main-night.json\"", record, StringComparison.Ordinal);
        Assert.DoesNotContain("job.status }}\"", workflow, StringComparison.Ordinal);
        Assert.Contains("(D-565)", workflow, StringComparison.Ordinal);
        Assert.Contains("(D-569)", workflow, StringComparison.Ordinal);
    }

    [Fact]
    public void ABranchNightWritesARecordOfItsOwnAndReRunsTheGate()
    {
        // D-373: the record of main is the state that the night-gate job reads (D-275), and main alone writes it.
        // D-538: a night on another branch writes its record to night-branch/<branch>, and a night on a tag writes
        // none, because the record job runs on a branch ref alone. D-548: a branch night then re-runs the newest
        // night-gate run of its branch, with the actions permission.
        string workflow = RepositoryRoot.ReadFile(".github/workflows/night.yml");
        Assert.Contains("\n  record:\n    needs: [plan, sweep]\n", workflow, StringComparison.Ordinal);
        Assert.Contains("\n    if: always() && startsWith(github.ref, 'refs/heads/')\n    runs-on: ubuntu-latest\n", workflow, StringComparison.Ordinal);
        Assert.Contains("if: always()", StepText(workflow, "Write the night record"), StringComparison.Ordinal);
        string publish = StepText(workflow, "Publish the night record");
        Assert.Contains("if: always()", publish, StringComparison.Ordinal);
        Assert.Contains("if [ \"${GITHUB_REF}\" = \"refs/heads/main\" ]; then\n            target=\"night-results\"\n          else\n            target=\"night-branch/${GITHUB_REF_NAME}\"", publish, StringComparison.Ordinal);
        Assert.Contains("git push --force origin \"HEAD:refs/heads/${target}\"", publish, StringComparison.Ordinal);
        Assert.DoesNotContain("origin night-results", workflow, StringComparison.Ordinal);
        string rerun = StepText(workflow, "Re-run the night gate of the branch");
        Assert.Contains("if: always() && github.ref != 'refs/heads/main'", rerun, StringComparison.Ordinal);
        Assert.Contains("--workflow night-gate.yml --branch \"$GITHUB_REF_NAME\" --event pull_request", rerun, StringComparison.Ordinal);
        Assert.Contains("gh run rerun \"$id\"", rerun, StringComparison.Ordinal);
        Assert.Contains("\n      actions: write\n", workflow, StringComparison.Ordinal);
        Assert.Contains("(D-373)", workflow, StringComparison.Ordinal);
        Assert.Contains("(D-538", workflow, StringComparison.Ordinal);
        Assert.Contains("(D-548)", workflow, StringComparison.Ordinal);
    }

    [Fact]
    public void APushToMainPromotesTheBranchNightOfItsPr()
    {
        // D-557: a job on each push to main finds the merged PR, and the tool night-promote decides from git and the two
        // record branches (D-555, D-556, D-558). The push takes a lease on the record it read. D-559: a promotion
        // re-runs the night gate of each open PR. The head branch reaches the tool through the environment alone.
        string workflow = RepositoryRoot.ReadFile(".github/workflows/night-promote.yml");
        KeyValuePair<string, string> job = Assert.Single(WorkflowText.RunsOnByJob(workflow));
        Assert.Equal("promote", job.Key);
        Assert.Equal("ubuntu-latest", job.Value);
        Assert.Contains("on:\n  push:\n    branches: [main]\n", workflow, StringComparison.Ordinal);
        Assert.Contains("\n  contents: write\n  actions: write\n  pull-requests: read\n", workflow, StringComparison.Ordinal);
        Assert.Contains("concurrency:\n  group: night-promotion\n  cancel-in-progress: false\n", workflow, StringComparison.Ordinal);
        Assert.Contains("fetch-depth: 0", workflow, StringComparison.Ordinal);
        Assert.Contains("select(.merge_commit_sha == $sha)", StepText(workflow, "Find the merged PR"), StringComparison.Ordinal);

        string promote = StepText(workflow, "Promote the branch night");
        Assert.Contains("HEAD_BRANCH: ${{ steps.pr.outputs.branch }}", promote, StringComparison.Ordinal);
        Assert.Equal(2, workflow.Split("${{ steps.pr.outputs.branch }}").Length);
        Assert.Contains("git fetch --quiet origin \"refs/pull/${PR_NUMBER}/head\"", promote, StringComparison.Ordinal);
        Assert.Contains("night-promote --root \"$GITHUB_WORKSPACE\" --remote origin --merge \"$MERGE_SHA\" --head-branch \"$HEAD_BRANCH\"", promote, StringComparison.Ordinal);
        Assert.Contains("git push --force-with-lease=\"refs/heads/night-results:${lease}\" origin \"HEAD:refs/heads/night-results\"", promote, StringComparison.Ordinal);

        string rerun = StepText(workflow, "Re-run the night gate of each open PR");
        Assert.Contains("if: steps.promote.outputs.promoted == 'true'", rerun, StringComparison.Ordinal);
        Assert.Contains("uses: ./.github/actions/rerun-night-gates", rerun, StringComparison.Ordinal);
    }

    [Fact]
    public void ANightOnMainKeepsALaterRecordAndReRunsEveryGate()
    {
        // D-562: a night on main writes its record only when the record of main is not at a later commit, and the push
        // takes a lease on the record it read. D-559: a night on main re-runs the night gate of each open PR.
        string workflow = RepositoryRoot.ReadFile(".github/workflows/night.yml");
        Assert.Contains("\n      pull-requests: read\n", workflow, StringComparison.Ordinal);
        Assert.Contains("fetch-depth: 0", workflow, StringComparison.Ordinal);
        string publish = StepText(workflow, "Publish the night record");
        Assert.Contains("BUILD_OUTCOME: ${{ steps.build.outcome }}", publish, StringComparison.Ordinal);
        Assert.Contains("night-publish-check --root . --remote origin --commit \"${GITHUB_SHA}\"", publish, StringComparison.Ordinal);
        Assert.Contains("git push --force-with-lease=\"refs/heads/night-results:${lease}\" origin \"HEAD:refs/heads/${target}\"", publish, StringComparison.Ordinal);
        Assert.True(publish.IndexOf("night-publish-check", StringComparison.Ordinal) < publish.IndexOf("git worktree add", StringComparison.Ordinal), "The order check runs before the record commit.");

        string rerun = StepText(workflow, "Re-run the night gate of each open PR");
        Assert.Contains("if: always() && github.ref == 'refs/heads/main'", rerun, StringComparison.Ordinal);
        Assert.Contains("uses: ./.github/actions/rerun-night-gates", rerun, StringComparison.Ordinal);
        Assert.Contains("(D-559)", workflow, StringComparison.Ordinal);
        Assert.Contains("(D-562)", workflow, StringComparison.Ordinal);
    }

    [Fact]
    public void TheGateReRunActionReRunsTheNewestGateOfEachOpenPr()
    {
        // D-559: the action re-runs the newest night-gate run of each open PR on main that has ended.
        string action = RepositoryRoot.ReadFile(".github/actions/rerun-night-gates/action.yml");
        Assert.Contains("using: composite", action, StringComparison.Ordinal);
        Assert.Contains("gh pr list --repo \"$REPOSITORY\" --state open --base main", action, StringComparison.Ordinal);
        Assert.Contains("--workflow night-gate.yml --branch \"$branch\" --event pull_request --limit 1", action, StringComparison.Ordinal);
        Assert.Contains("if [ \"$state\" != \"completed\" ]; then", action, StringComparison.Ordinal);
        Assert.Contains("gh run rerun \"$id\" --repo \"$REPOSITORY\"", action, StringComparison.Ordinal);
    }

    /// <summary>The concurrency block that every workflow on a pull request carries (D-356).</summary>
    private const string PullRequestConcurrency = """
        concurrency:
          group: ${{ github.workflow }}-${{ github.event.pull_request.number || github.sha }}
          cancel-in-progress: ${{ github.event_name == 'pull_request' || github.event_name == 'pull_request_target' }}
        """;

    [Fact]
    public void EveryPullRequestWorkflowCancelsItsOlderRuns()
    {
        // D-356, F-99: a newer event on a PR cancels the older run of each workflow for that PR, so the one Mac runner
        // serves the newest head. The group keys on the PR number, because the ref of pull_request_target is the base
        // branch for every PR, and a push to main takes the group of its own commit.
        string directory = Path.Combine(RepositoryRoot.Find(), ".github", "workflows");
        List<string> pullRequestWorkflows = [];
        foreach (string path in Directory.GetFiles(directory, "*.yml").OrderBy(path => path, StringComparer.Ordinal))
        {
            string workflow = File.ReadAllText(path);
            bool onPullRequest = workflow.Contains("\n  pull_request:\n", StringComparison.Ordinal) || workflow.Contains("\n  pull_request_target:\n", StringComparison.Ordinal);
            if (!onPullRequest)
            {
                continue;
            }

            string name = Path.GetFileName(path);
            pullRequestWorkflows.Add(name);
            Assert.True(workflow.Contains("\n" + PullRequestConcurrency + "\n", StringComparison.Ordinal), $"The workflow '{name}' does not carry the concurrency group of D-356.");
        }

        // The doc gate of D-376 is the tenth.
        Assert.Equal(10, pullRequestWorkflows.Count);
    }

    [Fact]
    public void TheNightNeverCancels()
    {
        // D-356: the night gate fails a cancelled night record (D-274), so the night workflow takes no concurrency group.
        string workflow = RepositoryRoot.ReadFile(".github/workflows/night.yml");
        Assert.DoesNotContain("concurrency:", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("cancel-in-progress", workflow, StringComparison.Ordinal);
    }

    /// <summary>The text of one workflow step, from its name line to the next step or the end of the workflow.</summary>
    private static string StepText(string workflow, string stepName)
    {
        int start = workflow.IndexOf($"- name: {stepName}", StringComparison.Ordinal);
        Assert.True(start >= 0, $"The step '{stepName}' is absent.");
        int end = workflow.IndexOf("- name:", start + 1, StringComparison.Ordinal);
        return end < 0 ? workflow[start..] : workflow[start..end];
    }

    /// <summary>The first defect of the bot log upload step in a night workflow text, or null when the step is right (D-280).</summary>
    private static string? UploadStepDefect(string workflow)
    {
        int stepStart = workflow.IndexOf("- name: Keep the bot logs of a failed night", StringComparison.Ordinal);
        if (stepStart < 0)
        {
            return "The night workflow has no bot log step.";
        }

        int nextStep = workflow.IndexOf("- name:", stepStart + 1, StringComparison.Ordinal);
        string step = nextStep < 0 ? workflow[stepStart..] : workflow[stepStart..nextStep];
        if (!step.Contains("uses: actions/upload-artifact@v4", StringComparison.Ordinal))
        {
            return "The bot log step uploads no artifact.";
        }

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

        foreach (string sweep in new[] { "Bot sweep, the fixed seeds and the slice", "Seed sweep, the fixed seeds and the slice" })
        {
            if (workflow.IndexOf($"- name: {sweep}", StringComparison.Ordinal) > stepStart)
            {
                return $"The upload step comes before the step '{sweep}'.";
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
    public void DocGateWorkflowReadsTheDescriptionAsData()
    {
        // D-376: the gate runs again on a description edit, and the description and the title reach the shell
        // through the environment alone, so no PR text runs as a command.
        string workflow = RepositoryRoot.ReadFile(".github/workflows/doc-gate.yml");
        Assert.Contains("types: [opened, edited, reopened, synchronize]", workflow, StringComparison.Ordinal);
        Assert.Contains("PR_BODY: ${{ github.event.pull_request.body }}", workflow, StringComparison.Ordinal);
        Assert.Contains("PR_TITLE: ${{ github.event.pull_request.title }}", workflow, StringComparison.Ordinal);
        Assert.Contains("-- doc-gate --root", workflow, StringComparison.Ordinal);
        string[] runLines = workflow.Split('\n').Where(line => line.TrimStart().StartsWith("run:", StringComparison.Ordinal)).ToArray();
        Assert.Equal(2, runLines.Length);
        Assert.All(runLines, line => Assert.DoesNotContain("${{", line, StringComparison.Ordinal));
    }

    [Fact]
    public void PullRequestTemplateHoldsEveryDocumentCategory()
    {
        // D-376: the template gives one matrix line for each category that the doc gate requires, in the same order.
        string template = RepositoryRoot.ReadFile(".github/pull_request_template.md");
        int matrix = template.IndexOf("\n" + DocGateRules.MatrixHeading + "\n", StringComparison.Ordinal);
        Assert.True(matrix >= 0, "The PR template has no documents matrix heading.");
        int previous = matrix;
        foreach (DocumentCategory category in DocGateRules.Categories)
        {
            int line = template.IndexOf("\n- " + category.Label + ":", StringComparison.Ordinal);
            Assert.True(line > previous, $"The PR template has no matrix line for {category.Label} after the one before it.");
            previous = line;
        }
    }

    [Fact]
    public void PullRequestTemplateGitarLineMatchesThePrGate()
    {
        // D-551, D-554: the gitar line of the template is the gitar line of the PR gate in the agent files, so a
        // gitar notice needs no answer in both (D-550). AgentFilesAreIdentical covers CLAUDE.md.
        string[] template = GitarGateLines(".github/pull_request_template.md");
        string[] agents = GitarGateLines("AGENTS.md");
        Assert.True(template.Length == 1, $"The PR template has {template.Length} gitar gate lines, and it needs one.");
        Assert.True(agents.Length == 1, $"AGENTS.md has {agents.Length} gitar gate lines, and it needs one.");
        Assert.Equal(agents[0], template[0]);
    }

    private static string[] GitarGateLines(string relativePath)
    {
        return RepositoryRoot.ReadFile(relativePath)
            .Split('\n')
            .Where(line => line.StartsWith("- [ ] The automated pass of gitar", StringComparison.Ordinal))
            .ToArray();
    }

    [Fact]
    public void EverySkillHasValidFrontMatter()
    {
        // D-131, D-155: each project skill is .claude/skills/<name>/SKILL.md. Its front matter opens the file, names
        // the directory, and gives a description, or the harness cannot load it.
        string directory = Path.Combine(RepositoryRoot.Find(), ".claude", "skills");
        string[] skills = Directory.GetDirectories(directory).OrderBy(path => path, StringComparer.Ordinal).ToArray();
        Assert.Contains(skills, path => Path.GetFileName(path) == OnePrOneSessionSkill);
        foreach (string skill in skills)
        {
            string name = Path.GetFileName(skill);
            string file = Path.Combine(skill, "SKILL.md");
            Assert.True(File.Exists(file), $"The skill directory '{name}' has no SKILL.md.");
            string[] lines = File.ReadAllText(file).Split('\n');
            int close = Array.IndexOf(lines, "---", 1);
            Assert.True(lines[0] == "---" && close > 0, $"The skill '{name}' does not open with front matter.");
            string[] frontMatter = lines[1..close];
            Assert.Contains($"name: {name}", frontMatter);
            string? description = frontMatter.FirstOrDefault(line => line.StartsWith("description: ", StringComparison.Ordinal));
            Assert.True(description is not null && description.Length > "description: ".Length + 40, $"The skill '{name}' has no description of at least 40 characters.");
        }
    }

    [Fact]
    public void AgentFilesRequireTheSessionSkill()
    {
        // D-375: the root instructions name the skill path for all PR work, and they do not copy the skill.
        string agents = RepositoryRoot.ReadFile("AGENTS.md");
        Assert.Contains($".claude/skills/{OnePrOneSessionSkill}/SKILL.md", agents, StringComparison.Ordinal);
        Assert.DoesNotContain("Blocked: start a new clean session for this PR.", agents, StringComparison.Ordinal);
    }

    [Fact]
    public void SessionSkillStaysSmall()
    {
        // D-375: every PR session loads this skill, so its size is a cost on each session. The limit holds the size
        // measured at the first version, 2026-09-16, with room for a short correction. A larger skill needs a decision.
        string skill = RepositoryRoot.ReadFile($".claude/skills/{OnePrOneSessionSkill}/SKILL.md");
        Assert.True(skill.Length <= SessionSkillCharacterLimit, $"The skill holds {skill.Length} characters, and the limit is {SessionSkillCharacterLimit}.");
        Assert.Contains("`Blocked: start a new clean session for this PR.`", skill, StringComparison.Ordinal);
        Assert.Contains("`This session is bound to PR #N and is complete. End this session. Start a new clean session before beginning another PR.`", skill, StringComparison.Ordinal);
    }

    [Fact]
    public void SessionSkillTriggersTheTransitionalPrompt()
    {
        // D-375, D-385: the merge message of the owner starts the transitional prompt, and no request from the owner
        // does. The skill file names the trigger and the reference file. The reference file holds the block, and the
        // block names the merge commit, the next PR, each open question, and each owner answer that no OQ-# holds.
        string skill = RepositoryRoot.ReadFile($".claude/skills/{OnePrOneSessionSkill}/SKILL.md");
        Assert.Contains("`Merged PR #N`", skill, StringComparison.Ordinal);
        Assert.Contains("references/merge-prompt.md", skill, StringComparison.Ordinal);
        string reference = RepositoryRoot.ReadFile($".claude/skills/{OnePrOneSessionSkill}/references/merge-prompt.md");
        Assert.Contains("the owner merges the PR and says `Merged PR #x`", reference, StringComparison.Ordinal);
        Assert.Contains("GitHub auto-merge merges it on the green light", reference, StringComparison.Ordinal);
        Assert.Contains("it does no other work", reference, StringComparison.Ordinal);
        foreach (string part in MergePromptBlockParts)
        {
            Assert.Contains(part, reference, StringComparison.Ordinal);
        }
    }

    /// <summary>The four parts that the transitional prompt block names. A questions sweep alone misses the last one.</summary>
    private static readonly string[] MergePromptBlockParts =
    [
        "merged to `main` as <sha>",
        "Start PR-<n>:",
        "Open questions for this PR:",
        "Owner answers that the roadmap names and no OQ-# holds:",
    ];

    private const string OnePrOneSessionSkill = "one-pr-one-session";

    private const int SessionSkillCharacterLimit = 7000;

    [Fact]
    public void ReviewGateModeFileHoldsEnforced()
    {
        // D-521: a missing review record fails the required check. Advisory mode gives neutral, and GitHub counts a
        // neutral conclusion as a pass for a required check (D-181).
        string mode = RepositoryRoot.ReadFile(".github/review-gate-mode");
        Assert.Equal("enforced", mode.Trim());
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

    /// <summary>
    /// Maps each job id under <c>jobs:</c> to its check name: the <c>name:</c> line of the job, or the job id when the
    /// job has none. A step name stands deeper than four spaces, so it never counts.
    /// </summary>
    public static Dictionary<string, string> CheckNameByJob(string workflow)
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

            if (line.Length > 2 && line.StartsWith("  ", StringComparison.Ordinal) && line[2] != ' ' && line[2] != '#' && line.TrimEnd().EndsWith(':'))
            {
                currentJob = line.Trim().TrimEnd(':');
                result[currentJob] = currentJob;
                continue;
            }

            if (currentJob is not null && line.StartsWith("    name: ", StringComparison.Ordinal))
            {
                result[currentJob] = line["    name: ".Length..].Trim();
            }
        }

        return result;
    }

    /// <summary>The text of one job under <c>jobs:</c>, from its id line to the next job id or the end of the workflow.</summary>
    public static string JobText(string workflow, string job)
    {
        int start = workflow.IndexOf($"\n  {job}:\n", StringComparison.Ordinal);
        if (start < 0)
        {
            throw new InvalidOperationException($"The workflow has no job '{job}'.");
        }

        int end = start + 1;
        while (true)
        {
            end = workflow.IndexOf("\n  ", end + 1, StringComparison.Ordinal);
            if (end < 0)
            {
                return workflow[start..];
            }

            if (workflow.Length > end + 3 && workflow[end + 3] != ' ' && workflow[end + 3] != '#')
            {
                return workflow[start..end];
            }
        }
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
