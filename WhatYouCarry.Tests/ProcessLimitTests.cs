using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using WhatYouCarry.Tools;
using WhatYouCarry.Tools.CodexReview;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// F-163. Each child process of the tools has a time limit: five minutes for a short command, and sixty minutes for the
/// review run (D-627). A child that hung made the old tools hang with no error.
/// </summary>
public sealed class ProcessLimitTests
{
    /// <summary>The limits of D-627.</summary>
    [Fact]
    public void TheLimitsAreTheLimitsOfTheDecision()
    {
        Assert.Equal(TimeSpan.FromMinutes(5), ProcessLimit.Short);
        Assert.Equal(TimeSpan.FromMinutes(60), ProcessLimit.Review);
    }

    /// <summary>A child past its limit stops, and the error names the command, the directory, and the limit.</summary>
    [Fact]
    public void AChildPastItsLimitStopsWithAnError()
    {
        (string program, string[] args) = SleepCommand();
        string directory = Path.GetTempPath();
        TimeSpan limit = TimeSpan.FromMilliseconds(500);
        Stopwatch clock = Stopwatch.StartNew();

        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() => ExternalProcess.Run(program, args, directory, null, limit));

        Assert.True(clock.Elapsed < TimeSpan.FromSeconds(20), $"The run took {clock.Elapsed}.");
        Assert.Contains(program, error.Message, StringComparison.Ordinal);
        Assert.Contains(directory, error.Message, StringComparison.Ordinal);
        Assert.Contains(limit.ToString(), error.Message, StringComparison.Ordinal);
        Assert.Contains("D-627", error.Message, StringComparison.Ordinal);
    }

    /// <summary>A long run to files stops at its limit in the same way.</summary>
    [Fact]
    public void ALongRunPastItsLimitStopsWithAnError()
    {
        (string program, string[] args) = SleepCommand();
        string directory = Directory.CreateTempSubdirectory("wyc-process-limit-").FullName;
        try
        {
            string output = Path.Combine(directory, "out.txt");
            string errors = Path.Combine(directory, "err.txt");
            InvalidOperationException error = Assert.Throws<InvalidOperationException>(
                () => ExternalProcess.RunToFiles(program, args, directory, output, errors, [], TimeSpan.FromMilliseconds(500)));
            Assert.Contains("ran past its limit", error.Message, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    /// <summary>A child that ends inside its limit gives its output and its exit code.</summary>
    [Fact]
    public void AChildInsideItsLimitGivesItsOutput()
    {
        ProcessResult result = ExternalProcess.Run("dotnet", ["--version"], Path.GetTempPath(), null, TimeSpan.FromMinutes(2));
        Assert.Equal(0, result.ExitCode);
        Assert.False(string.IsNullOrWhiteSpace(result.StandardOutput), "dotnet --version wrote no version.");
    }

    /// <summary>The review run takes the review limit, and each git call of the gates takes the short limit.</summary>
    [Fact]
    public void EachCallerTakesItsLimit()
    {
        string review = RepositoryRoot.ReadFile("WhatYouCarry.Tools/CodexReview/CodexReviewCommand.cs");
        Assert.Contains("CodexReviewSettings.ApiCredentialVariables, ProcessLimit.Review);", review, StringComparison.Ordinal);
        string git = RepositoryRoot.ReadFile("WhatYouCarry.Tools/ReviewGate/GitRepository.cs");
        Assert.Contains("ProcessLimit.WaitOrStop(process, ProcessLimit.Short,", git, StringComparison.Ordinal);
        Assert.DoesNotContain("process.StandardOutput.ReadToEnd()", git, StringComparison.Ordinal);
    }

    /// <summary>A command that sleeps for thirty seconds on the platform of the test.</summary>
    private static (string Program, string[] Args) SleepCommand()
    {
        return OperatingSystem.IsWindows()
            ? ("powershell", ["-NoProfile", "-Command", "Start-Sleep -Seconds 30"])
            : ("sleep", ["30"]);
    }
}
