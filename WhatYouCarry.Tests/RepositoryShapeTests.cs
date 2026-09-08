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
