using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WhatYouCarry.Core.Bots;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Procgen;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Game;
using WhatYouCarry.Game.Measure;
using WhatYouCarry.Game.Render;
using WhatYouCarry.Game.World;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The frame log and the bot session of M-3 (D-295, D-296), and the world shader text (D-292).</summary>
public sealed class MeasureTests
{
    /// <summary>The 99th percentile by the nearest rank: the 99th of one hundred frames, and the one frame of one.</summary>
    [Fact]
    public void FrameLogTakesTheNinetyNinthPercentile()
    {
        FrameLog log = new();
        for (int frame = 100; frame >= 1; frame--)
        {
            log.Add(frame / 1000000.0);
        }

        Assert.Equal(100, log.Frames.Count);
        Assert.Equal(99, log.Percentile99());

        FrameLog one = new();
        one.Add(0.0111);
        Assert.Equal(11100, one.Percentile99());
    }

    /// <summary>A log with no frame has no percentile, and the error says so (T-2).</summary>
    [Fact]
    public void FrameLogRejectsAnEmptyPercentile()
    {
        Assert.Throws<ContextException>(() => new FrameLog().Percentile99());
    }

    /// <summary>The file text is one microsecond count per line, in frame order.</summary>
    [Fact]
    public void FrameLogWritesOneFramePerLine()
    {
        FrameLog log = new();
        log.Add(0.0166667);
        log.Add(0.0111111);
        Assert.Equal("16667\n11111\n", log.Text());
    }

    /// <summary>
    /// The window of a transition mark holds the frames before the mark and from the mark on, up to the window size
    /// each way, and its slowest frame is the hitch of that transition (D-435). A frame outside every window is no
    /// hitch.
    /// </summary>
    [Fact]
    public void FrameLogTakesTheSlowestFrameNearEachTransition()
    {
        FrameLog log = new();
        int window = FrameLog.TransitionWindowFrames;
        for (int frame = 0; frame < 200; frame++)
        {
            if (frame == 50 || frame == 150)
            {
                log.MarkTransition();
            }

            // A slow frame far from both marks, one inside the first window before its mark, and one on the second mark.
            long micros = frame == 5 ? 90000 : frame == 50 - window ? 30000 : frame == 150 ? 25000 : 11000;
            log.Add(micros / 1000000.0);
        }

        Assert.Equal(2, log.Transitions);
        Assert.Equal([30000L, 25000L], log.TransitionMaxima());

        // The frame just outside the window before a mark is no part of it.
        FrameLog edge = new();
        for (int frame = 0; frame < 100; frame++)
        {
            if (frame == 60)
            {
                edge.MarkTransition();
            }

            edge.Add((frame == 60 - window - 1 ? 50000 : frame == 60 + window - 1 ? 12000 : 11000) / 1000000.0);
        }

        Assert.Equal([12000L], edge.TransitionMaxima());
    }

    /// <summary>
    /// The trace sums the collector pauses and the collections of the frames after a mark, takes the slowest frame and
    /// the most tick and upload time, counts the frames of a dig, and closes after the window (D-109, D-435). A frame
    /// with no open window is not traced, and a new mark closes a window that is still open.
    /// </summary>
    [Fact]
    public void TransitionTraceSummarizesTheWindowAfterAMark()
    {
        TransitionTrace trace = new();
        Assert.Null(trace.AddFrame(new TraceFrame(90000, 0, 0, 0, 0, 0, 0, false)));

        trace.MarkTransition();
        TransitionSummary? summary = null;
        for (int frame = 0; frame < FrameLog.TransitionWindowFrames; frame++)
        {
            bool first = frame < 3;
            Assert.Null(summary);
            summary = trace.AddFrame(new TraceFrame(first ? 44444 : 11111, first ? 2000 : 500, frame == 5 ? 3000 : 100, first ? 15000 : 0, first ? 1 : 0, frame == 1 ? 1 : 0, frame == 2 ? 1 : 0, frame < 10));
        }

        Assert.Equal(new TransitionSummary(1, FrameLog.TransitionWindowFrames, 44444, 2000, 3000, 45000, 3, 1, 1, 10), summary);
        Assert.Null(trace.AddFrame(new TraceFrame(90000, 0, 0, 0, 0, 0, 0, false)));

        trace.MarkTransition();
        trace.AddFrame(new TraceFrame(20000, 0, 0, 0, 0, 0, 0, true));
        trace.MarkTransition();
        Assert.Equal(new TransitionSummary(2, 1, 20000, 0, 0, 0, 0, 0, 0, 1), trace.Summaries[1]);
    }

    /// <summary>
    /// The chunk swap meshes a floor on the worker task: the meshes of a task equal the meshes of each chunk on the
    /// calling thread, in chunk order, and the run needs no engine. The first Deck run traced 38 to 46 milliseconds of
    /// main-thread mesh work in one frame after each swap (D-109, D-427).
    /// </summary>
    [Fact]
    public async Task ChunkSwapMeshesOnTheTask()
    {
        FloorPlan plan = FloorGenerator.Generate(1, 2, TestWorld.Content);
        IReadOnlyList<MeshData> onTask = await Task.Run(() => ChunkSwap.MeshAll(plan.Grid, RepositoryTextures.Tiles));
        Assert.Equal(ChunkLayout.Count(plan.Grid), onTask.Count);
        int index = 0;
        for (int chunkZ = 0; chunkZ < ChunkLayout.CountZ(plan.Grid); chunkZ++)
        {
            for (int chunkX = 0; chunkX < ChunkLayout.CountX(plan.Grid); chunkX++)
            {
                MeshData here = GreedyMesher.MeshChunk(plan.Grid, chunkX, chunkZ, RepositoryTextures.Tiles);
                Assert.Equal(here.Positions, onTask[index].Positions);
                Assert.Equal(here.Indices, onTask[index].Indices);
                Assert.Equal(here.Colors, onTask[index].Colors);
                index++;
            }
        }

        Assert.Contains(onTask, mesh => mesh.TriangleCount > 0);
    }

    /// <summary>A mark with no frame in its window has no hitch, and the error says so (T-2).</summary>
    [Fact]
    public void FrameLogRejectsAnEmptyWindow()
    {
        FrameLog log = new();
        log.MarkTransition();
        Assert.Throws<ContextException>(() => log.TransitionMaxima());
    }

    /// <summary>The hitch budget is two frames at 90 frames per second (D-295, D-427).</summary>
    [Fact]
    public void HitchBudgetIsTwoFramesAtTheTarget()
    {
        Assert.Equal(22000, BotSession.HitchBudgetMicros);
        Assert.True(BotSession.HitchBudgetMicros <= 2 * FrameLog.MicrosecondsPerSecond / 90);
    }

    /// <summary>
    /// The session with ten transitions ends one second after the tenth descent, or at an ascend, and a floor with no
    /// descent inside its budget is stuck (D-435).
    /// </summary>
    [Fact]
    public void BotSessionCountsItsTransitions()
    {
        SimulationLoop loop = new(Main.FirstSeed, TestWorld.PeacefulContent);
        GreedyDescender bot = new(TestWorld.PeacefulContent);
        const int transitions = 3;
        uint floorStart = 0;
        while (!BotSession.IsComplete(loop, transitions, loop.Tick - floorStart))
        {
            Assert.False(BotSession.IsStuck(loop, transitions, loop.Tick - floorStart), $"The bot is stuck on floor {loop.Floor} at tick {loop.Tick}.");
            Assert.False(loop.Ended, $"The run ended on floor {loop.Floor} at tick {loop.Tick}.");
            int floor = loop.Floor;
            loop.Step(bot.Next(loop));
            if (loop.Floor != floor)
            {
                floorStart = loop.Tick;
            }
        }

        Assert.Equal(SimulationLoop.FirstFloor + transitions, loop.Floor);
        Assert.True(BotSession.IsStuck(loop, transitions + 1, BotSession.TickBudget));
        Assert.False(BotSession.IsStuck(loop, transitions, BotSession.TickBudget));
    }

    /// <summary>The flag takes its one word as the path, and a read with no flag is an error. The parser stops a flag with no path (D-313).</summary>
    [Fact]
    public void FrameLogReadsThePathAfterTheFlag()
    {
        UserArguments withLog = UserArguments.Parse([BotSession.Flag, FrameLog.Flag, "frames.txt"]);
        Assert.True(FrameLog.IsRequested(withLog));
        Assert.Equal("frames.txt", FrameLog.PathOf(withLog));

        UserArguments noLog = UserArguments.Parse([BotSession.Flag]);
        Assert.False(FrameLog.IsRequested(noLog));
        Assert.Throws<ContextException>(() => FrameLog.PathOf(noLog));
    }

    /// <summary>The bot flag starts the session, and nothing else does.</summary>
    [Fact]
    public void BotSessionReadsTheFlag()
    {
        Assert.True(BotSession.IsRequested(UserArguments.Parse([BotSession.Flag])));
        Assert.True(BotSession.IsRequested(UserArguments.Parse([FrameLog.Flag, "frames.txt", BotSession.Flag])));
        Assert.False(BotSession.IsRequested(UserArguments.Parse([])));
        Assert.False(BotSession.IsRequested(UserArguments.Parse([FrameLog.Flag, "frames.txt"])));
    }

    /// <summary>
    /// The greedy descender drives the loop of the first seed off the first floor inside the tick budget, and the
    /// session ends one second after the descent.
    /// </summary>
    [Fact]
    public void BotSessionCompletesOneFloor()
    {
        SimulationLoop loop = new(Main.FirstSeed, TestWorld.Content);
        GreedyDescender bot = new(TestWorld.Content);
        uint floorStart = 0;

        Assert.False(BotSession.IsComplete(loop, 1, 0));
        Assert.False(BotSession.IsStuck(loop, 1, 0));
        while (!BotSession.IsComplete(loop, 1, loop.Tick - floorStart) && !BotSession.IsStuck(loop, 1, loop.Tick - floorStart))
        {
            int floor = loop.Floor;
            loop.Step(bot.Next(loop));
            if (loop.Floor != floor)
            {
                floorStart = loop.Tick;
                Assert.False(BotSession.IsComplete(loop, 1, 0), "The session ends one second after the descent, and not on its tick.");
            }
        }

        Assert.True(BotSession.IsComplete(loop, 1, loop.Tick - floorStart), $"The bot is stuck on floor 1 of seed {Main.FirstSeed} at tick {loop.Tick}.");
        Assert.Equal(SimulationLoop.FirstFloor + 1, loop.Floor);
        Assert.Equal(BotSession.SettleTicks, loop.Tick - floorStart);
        Assert.True(BotSession.TickBudget > loop.Timer.Length, "The timer of floor 1 expires before the budget of the session (D-407).");
    }

    /// <summary>The world shader declares every uniform that the material sets, and its fade radius equals the constant.</summary>
    [Fact]
    public void WorldShaderDeclaresTheFadeUniforms()
    {
        string shader = RepositoryRoot.ReadFile("WhatYouCarry.Game/World/world.gdshader");

        Assert.Contains($"uniform sampler2D {WorldMaterial.AtlasName} ", shader, StringComparison.Ordinal);
        Assert.Contains($"uniform vec2 {WorldMaterial.TileSizeName} ", shader, StringComparison.Ordinal);
        Assert.Contains($"uniform vec3 {WorldMaterial.FadeStartName} ", shader, StringComparison.Ordinal);
        Assert.Contains($"uniform vec3 {WorldMaterial.FadeEndName} ", shader, StringComparison.Ordinal);
        Assert.Contains($"uniform float {WorldMaterial.FadeRadiusName} = 0.75;", shader, StringComparison.Ordinal);
        Assert.Equal(0.75f, WorldMaterial.FadeRadius);
        Assert.Contains("discard;", shader, StringComparison.Ordinal);
        Assert.Contains("fract(UV)", shader, StringComparison.Ordinal);
        Assert.Contains("* COLOR.rgb", shader, StringComparison.Ordinal);
        Assert.Contains("res://World/world.gdshader", WorldMaterial.ShaderPath, StringComparison.Ordinal);
    }
}
