using System;
using System.Collections.Generic;
using System.Linq;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Game.Review;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// The frame shots of PR-96 (D-711, D-714): the tick of each shot and the file name of each frame. The render needs a
/// window, so these tests read the schedule, and the Tier 4 pass reads the frames (PR-96 exit test 5).
/// </summary>
public sealed class FrameShotsTests
{
    /// <summary>
    /// D-711. A shot falls on each whole second of game time, and on no other tick. The first tick of the run takes no
    /// shot, because the viewport drew no frame of the run before it.
    /// </summary>
    [Fact]
    public void AShotFallsOnEachSecondOfGameTime()
    {
        Assert.Equal(60U, FrameShots.TicksPerShot);
        Assert.False(FrameShots.IsShotTick(0));
        Assert.False(FrameShots.IsShotTick(1));
        Assert.False(FrameShots.IsShotTick(59));
        Assert.True(FrameShots.IsShotTick(60));
        Assert.False(FrameShots.IsShotTick(61));
        Assert.True(FrameShots.IsShotTick(120));
        Assert.True(FrameShots.IsShotTick(10800));

        // The timer-tester run of D-714 dies to the Overseer at tick 12873, so it takes 214 shots.
        int shots = Enumerable.Range(0, 12874).Count(tick => FrameShots.IsShotTick((uint)tick));
        Assert.Equal(214, shots);
    }

    /// <summary>
    /// The file name holds the floor and the tick at a fixed width, so the names sort in the order of the run, a descent
    /// included, and two shots never share a name.
    /// </summary>
    [Fact]
    public void TheNamesSortInTheOrderOfTheRun()
    {
        Assert.Equal("floor-01-tick-000060.png", FrameShots.FileName(1, 60));
        Assert.Equal("floor-15-tick-022016.png", FrameShots.FileName(15, 22016));

        List<string> run = [FrameShots.FileName(1, 60), FrameShots.FileName(1, 960), FrameShots.FileName(1, 1020), FrameShots.FileName(2, 1080), FrameShots.FileName(10, 9000)];
        List<string> sorted = [.. run.OrderBy(name => name, StringComparer.Ordinal)];
        Assert.Equal(run, sorted);
        Assert.Equal(run.Count, run.Distinct(StringComparer.Ordinal).Count());
    }

    /// <summary>
    /// A shot with no new frame since the last shot stops the capture, and the error names both frame counts (T-2). The
    /// first capture of PR-96 wrote 204 copies of one image with no error, after a second window covered the capture
    /// window and the engine drew no frame.
    /// </summary>
    [Fact]
    public void AShotWithNoNewFrameStopsTheCapture()
    {
        FrameShots.RequireNewFrame(61, 60);
        FrameShots.RequireNewFrame(1, 0);

        foreach ((long drawn, long last) in new[] { (60L, 60L), (0L, 0L), (59L, 60L) })
        {
            ContextException error = Assert.Throws<ContextException>(() => FrameShots.RequireNewFrame(drawn, last));
            Assert.StartsWith(FrameShots.NoNewFrameMessage, error.Message, StringComparison.Ordinal);
            Assert.Contains(error.Context, field => field.Name == "framesDrawn" && field.Value == drawn.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Assert.Contains(error.Context, field => field.Name == "framesAtLastShot" && field.Value == last.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
    }
}
