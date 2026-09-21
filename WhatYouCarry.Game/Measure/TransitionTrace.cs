using System;
using System.Collections.Generic;

namespace WhatYouCarry.Game.Measure;

/// <summary>
/// The trace of the frames after each floor transition of the transition test (D-435): for each frame, the frame
/// time, the time of the ticks and of the chunk upload in that frame, the pause of the .NET garbage collector, the
/// collections of each generation, and whether the worker still digs. One summary closes each window.
/// </summary>
/// <remarks>
/// The first Deck run of exit test 6 showed three to five frames of 33 to 52 milliseconds after each swap, and no
/// slow frame between the swaps. The trace names the cost of those frames before a change tunes it (D-109).
/// </remarks>
public sealed class TransitionTrace
{
    private readonly List<TraceFrame> open = [];
    private readonly List<TransitionSummary> summaries = [];
    private bool tracing;

    /// <summary>The summary of each transition whose window closed, in transition order.</summary>
    public IReadOnlyList<TransitionSummary> Summaries => this.summaries;

    /// <summary>Opens the window of a transition at the next frame. A window that is still open closes first.</summary>
    public void MarkTransition()
    {
        if (this.tracing)
        {
            this.Close();
        }

        this.tracing = true;
    }

    /// <summary>
    /// Adds one frame to the open window, and closes the window after <see cref="FrameLog.TransitionWindowFrames"/>
    /// frames. A frame with no open window is not traced.
    /// </summary>
    /// <returns>The summary of the window that this frame closed, or null.</returns>
    public TransitionSummary? AddFrame(TraceFrame frame)
    {
        if (!this.tracing)
        {
            return null;
        }

        this.open.Add(frame);
        if (this.open.Count < FrameLog.TransitionWindowFrames)
        {
            return null;
        }

        return this.Close();
    }

    /// <summary>Closes the open window, and adds its summary.</summary>
    private TransitionSummary Close()
    {
        long slowest = 0;
        long physics = 0;
        long upload = 0;
        long pause = 0;
        int gen0 = 0;
        int gen1 = 0;
        int gen2 = 0;
        int digging = 0;
        foreach (TraceFrame frame in this.open)
        {
            slowest = Math.Max(slowest, frame.FrameMicros);
            physics = Math.Max(physics, frame.PhysicsMicros);
            upload = Math.Max(upload, frame.UploadMicros);
            pause += frame.GcPauseMicros;
            gen0 += frame.Gen0;
            gen1 += frame.Gen1;
            gen2 += frame.Gen2;
            digging += frame.Digging ? 1 : 0;
        }

        TransitionSummary summary = new(this.summaries.Count + 1, this.open.Count, slowest, physics, upload, pause, gen0, gen1, gen2, digging);
        this.summaries.Add(summary);
        this.open.Clear();
        this.tracing = false;
        return summary;
    }
}

/// <summary>One traced frame: the frame time, the most time of the ticks and of the upload in it, the collector pause, the collections of each generation, and whether the worker digs. Times are in microseconds.</summary>
public readonly record struct TraceFrame(long FrameMicros, long PhysicsMicros, long UploadMicros, long GcPauseMicros, int Gen0, int Gen1, int Gen2, bool Digging);

/// <summary>
/// The summary of the window of one transition: the count of frames, the slowest frame, the most tick time and
/// upload time of one frame, the sum of the collector pauses, the collections of each generation, and the count of
/// frames in which the worker dug. Times are in microseconds.
/// </summary>
public sealed record TransitionSummary(int Transition, int Frames, long SlowestFrameMicros, long MaxPhysicsMicros, long MaxUploadMicros, long GcPauseMicros, int Gen0, int Gen1, int Gen2, int DiggingFrames);
