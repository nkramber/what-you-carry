using System;
using System.Diagnostics;

namespace WhatYouCarry.Tools;

/// <summary>
/// The time limits of the child processes of the tools (D-627, F-163). A child that hangs made the tool hang with no
/// error. A child past its limit now stops with its whole process tree, and the error names the command, the
/// directory, and the limit (T-2).
/// </summary>
public static class ProcessLimit
{
    /// <summary>The limit of each short command: a <c>gh</c> call, a <c>git</c> call, and each short <c>codex</c> call.</summary>
    public static readonly TimeSpan Short = TimeSpan.FromMinutes(5);

    /// <summary>The limit of the review run of <c>codex exec</c>, about twice the longest review so far (D-627).</summary>
    public static readonly TimeSpan Review = TimeSpan.FromMinutes(60);

    /// <summary>
    /// Waits for the process to end inside the limit. The caller reads both redirected streams at the same time before
    /// this call, so a full pipe never blocks the child.
    /// </summary>
    /// <exception cref="InvalidOperationException">The process ran past the limit. The process and its children are stopped. The type is the type of every other process failure of the tools, so each handler of the tools takes it.</exception>
    public static void WaitOrStop(Process process, TimeSpan limit, string command, string directory)
    {
        if (process.WaitForExit(limit))
        {
            // The wait with no limit also waits for the end of the redirected streams.
            process.WaitForExit();
            return;
        }

        process.Kill(entireProcessTree: true);
        process.WaitForExit();
        throw new InvalidOperationException($"'{command}' ran past its limit of {limit} in '{directory}', and the tool stopped it and its child processes (D-627).");
    }
}
