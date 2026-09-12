using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Game.Measure;

/// <summary>
/// The frame log of M-3 (D-295, D-296): the time of every render frame, in microseconds, and the 99th
/// percentile over them. The game keeps one on the flag, writes the lines to the named file at the end of the
/// session, and puts the percentile in the end line of the log.
/// </summary>
public sealed class FrameLog
{
    /// <summary>The user argument that starts the log. The next argument is the file path.</summary>
    public const string Flag = "--frame-log";

    /// <summary>The count of microseconds in one second.</summary>
    public const long MicrosecondsPerSecond = 1000000;

    private const string FlagField = "flag";
    private const string NoFrames = "The frame log holds no frame, and a percentile needs at least one.";
    private const string NoPath = "The frame log flag needs a file path after it, and the arguments end there.";

    private readonly List<long> frames = [];

    /// <summary>Every frame time so far, in microseconds, in frame order.</summary>
    public IReadOnlyList<long> Frames => this.frames;

    /// <summary>Answers whether the user arguments ask for the log.</summary>
    public static bool IsRequested(string[] userArguments)
    {
        foreach (string argument in userArguments)
        {
            if (argument == Flag)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>The file path after the flag.</summary>
    /// <exception cref="ContextException">The flag is absent, or no argument follows it.</exception>
    public static string PathOf(string[] userArguments)
    {
        for (int index = 0; index < userArguments.Length; index++)
        {
            if (userArguments[index] != Flag)
            {
                continue;
            }

            if (index + 1 >= userArguments.Length)
            {
                throw new ContextException(NoPath);
            }

            return userArguments[index + 1];
        }

        ContextException absent = new($"The arguments hold no {Flag} flag.");
        absent.AddContext(FlagField, Flag);
        throw absent;
    }

    /// <summary>Adds one frame from its time in seconds, rounded to the nearest microsecond.</summary>
    public void Add(double seconds)
    {
        this.frames.Add((long)Math.Round(seconds * MicrosecondsPerSecond));
    }

    /// <summary>
    /// The 99th percentile of the frame times, in microseconds: the value that 99 percent of the frames stay at
    /// or under, by the nearest-rank rule.
    /// </summary>
    /// <exception cref="ContextException">The log holds no frame.</exception>
    public long Percentile99()
    {
        if (this.frames.Count == 0)
        {
            throw new ContextException(NoFrames);
        }

        long[] sorted = new long[this.frames.Count];
        this.frames.CopyTo(sorted);
        Array.Sort(sorted);
        int rank = (int)Math.Ceiling(0.99 * sorted.Length);
        return sorted[rank - 1];
    }

    /// <summary>The text of the file: one frame time per line, in microseconds.</summary>
    public string Text()
    {
        StringBuilder builder = new();
        foreach (long frame in this.frames)
        {
            builder.Append(frame.ToString(CultureInfo.InvariantCulture));
            builder.Append('\n');
        }

        return builder.ToString();
    }
}
