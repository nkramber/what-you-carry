using System;
using Godot;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Game.Logging;

/// <summary>
/// A log sink that prints each line to the engine output and counts the error lines (D-211). The smoke
/// session reads the count for its exit code (D-114). PR-31 and PR-55 write the sink of the user directory.
/// </summary>
public sealed class PrintLogSink : ILogSink
{
    /// <summary>The start of every error line. The logger writes the level first (D-212), and a test pins this prefix to the logger.</summary>
    public const string ErrorPrefix = "{\"level\":\"error\"";

    /// <summary>The count of error lines that reached this sink.</summary>
    public int ErrorCount { get; private set; }

    /// <inheritdoc/>
    public void Write(string line)
    {
        if (line.StartsWith(ErrorPrefix, StringComparison.Ordinal))
        {
            this.ErrorCount++;
        }

        GD.Print(line);
    }
}
