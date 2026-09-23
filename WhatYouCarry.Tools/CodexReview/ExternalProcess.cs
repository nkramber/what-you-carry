using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace WhatYouCarry.Tools.CodexReview;

/// <summary>The exit code and the output of one finished process.</summary>
public sealed record ProcessResult(string Command, string WorkingDirectory, int ExitCode, string StandardOutput, string StandardError)
{
    /// <summary>Returns stdout. Throws with the command, the directory, the exit code, and stderr when the exit code is not zero (T-2).</summary>
    public string RequireSuccess()
    {
        if (ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"'{Command}' exited {ExitCode} in '{WorkingDirectory}'. stderr: {StandardError.Trim()}");
        }

        return StandardOutput;
    }
}

/// <summary>
/// Runs gh and codex. Stdin closes at the start, because <c>codex exec</c> reads a piped stdin into the prompt.
/// A program that does not start throws with its name and the directory. Each variable in the removed list is absent
/// from the environment of the child, and the environment of this process does not change.
/// </summary>
public static class ExternalProcess
{
    public static ProcessResult Run(string fileName, IReadOnlyList<string> args, string workingDirectory, IReadOnlyList<string>? removedVariables = null)
    {
        using Process process = Start(fileName, args, workingDirectory, removedVariables ?? []);
        Task<string> standardError = process.StandardError.ReadToEndAsync();
        string standardOutput = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return new ProcessResult(Describe(fileName, args), workingDirectory, process.ExitCode, standardOutput, standardError.Result);
    }

    /// <summary>Runs a long process and writes stdout and stderr to two files as they arrive. Returns the exit code.</summary>
    public static int RunToFiles(string fileName, IReadOnlyList<string> args, string workingDirectory, string standardOutputPath, string standardErrorPath, IReadOnlyList<string> removedVariables)
    {
        using Process process = Start(fileName, args, workingDirectory, removedVariables);
        using FileStream standardOutputFile = File.Create(standardOutputPath);
        using FileStream standardErrorFile = File.Create(standardErrorPath);
        Task standardError = process.StandardError.BaseStream.CopyToAsync(standardErrorFile);
        process.StandardOutput.BaseStream.CopyTo(standardOutputFile);
        standardError.Wait();
        process.WaitForExit();
        return process.ExitCode;
    }

    private static Process Start(string fileName, IReadOnlyList<string> args, string workingDirectory, IReadOnlyList<string> removedVariables)
    {
        var startInfo = new ProcessStartInfo(fileName)
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        foreach (string arg in args)
        {
            startInfo.ArgumentList.Add(arg);
        }

        foreach (string variable in removedVariables)
        {
            startInfo.Environment.Remove(variable);
        }

        Process process;
        try
        {
            process = Process.Start(startInfo)
                ?? throw new InvalidOperationException($"'{fileName}' did not start in '{workingDirectory}'.");
        }
        catch (System.ComponentModel.Win32Exception exception)
        {
            throw new InvalidOperationException($"'{fileName}' did not start in '{workingDirectory}': {exception.Message}", exception);
        }

        process.StandardInput.Close();
        return process;
    }

    private static string Describe(string fileName, IReadOnlyList<string> args)
    {
        return $"{fileName} {string.Join(' ', args)}";
    }
}
