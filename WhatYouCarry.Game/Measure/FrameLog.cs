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
/// <remarks>
/// The log also marks the frame of each floor transition. The slowest frame of the window around a mark is the
/// hitch of that transition, which exit test 6 of PR-18 holds under the budget of D-427 (D-435).
/// </remarks>
public sealed class FrameLog
{
    /// <summary>The user argument that starts the log. Its one word is the file path.</summary>
    public const string Flag = "--frame-log";

    /// <summary>The count of microseconds in one second.</summary>
    public const long MicrosecondsPerSecond = 1000000;

    /// <summary>
    /// The count of frames before a transition mark and after it that the window of the mark holds: half a second
    /// at 60 frames per second, and a third of a second at 90. The swap of the chunks and the rebuild of the enemies
    /// fall in the first frames after the mark (D-435).
    /// </summary>
    public const int TransitionWindowFrames = 30;

    private const string NoFrames = "The frame log holds no frame, and a percentile needs at least one.";
    private const string NoWindowFrames = "The window of a transition mark holds no frame.";
    private const string MarkField = "mark";

    private readonly List<long> frames = [];
    private readonly List<int> marks = [];

    /// <summary>Every frame time so far, in microseconds, in frame order.</summary>
    public IReadOnlyList<long> Frames => this.frames;

    /// <summary>Answers whether the user arguments ask for the log.</summary>
    public static bool IsRequested(UserArguments userArguments)
    {
        return userArguments.Has(Flag);
    }

    /// <summary>The file path: the one word of the flag. The parser stops a flag with no word (D-313).</summary>
    /// <exception cref="ContextException">The flag is absent.</exception>
    public static string PathOf(UserArguments userArguments)
    {
        return userArguments.WordsOf(Flag)[0];
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

    /// <summary>The count of transition marks so far.</summary>
    public int Transitions => this.marks.Count;

    /// <summary>Marks a floor transition at the next frame. The frames before it and after it form its window.</summary>
    public void MarkTransition()
    {
        this.marks.Add(this.frames.Count);
    }

    /// <summary>
    /// The slowest frame of the window of each transition mark, in microseconds, in mark order. The window holds up
    /// to <see cref="TransitionWindowFrames"/> frames before the mark and as many from the mark on.
    /// </summary>
    /// <exception cref="ContextException">A window holds no frame, so the log cannot state the hitch of that transition (T-2).</exception>
    public IReadOnlyList<long> TransitionMaxima()
    {
        List<long> maxima = [];
        for (int index = 0; index < this.marks.Count; index++)
        {
            int mark = this.marks[index];
            int first = Math.Max(0, mark - TransitionWindowFrames);
            int end = Math.Min(this.frames.Count, mark + TransitionWindowFrames);
            if (first >= end)
            {
                ContextException error = new(NoWindowFrames);
                error.AddContext(MarkField, index.ToString(CultureInfo.InvariantCulture));
                throw error;
            }

            long slowest = this.frames[first];
            for (int frame = first + 1; frame < end; frame++)
            {
                slowest = Math.Max(slowest, this.frames[frame]);
            }

            maxima.Add(slowest);
        }

        return maxima;
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
