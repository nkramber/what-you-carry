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
    /// The log marks each expiry of the floor timer apart from the transitions, and the slowest frame of the window of
    /// the same size around each expiry mark is the cost of that expiry (D-646, RR-P3-16). A transition mark is no part of
    /// the expiry maxima, and an expiry mark is no part of the transition maxima.
    /// </summary>
    [Fact]
    public void FrameLogTakesTheSlowestFrameNearEachExpiry()
    {
        FrameLog log = new();
        int window = FrameLog.TransitionWindowFrames;
        for (int frame = 0; frame < 300; frame++)
        {
            if (frame == 60)
            {
                log.MarkTransition();
            }

            if (frame == 200)
            {
                log.MarkExpiry();
            }

            // A hitch on the transition, a slow frame just outside the expiry window, and one on its last frame.
            long micros = frame == 60 ? 40000 : frame == 200 - window - 1 ? 90000 : frame == 200 + window - 1 ? 33000 : 11000;
            log.AddMicros(micros);
        }

        Assert.Equal(1, log.Expiries);
        Assert.Equal([33000L], log.ExpiryMaxima());
        Assert.Equal(1, log.Transitions);
        Assert.Equal([40000L], log.TransitionMaxima());

        FrameLog none = new();
        none.AddMicros(11000);
        Assert.Equal(0, none.Expiries);
        Assert.Empty(none.ExpiryMaxima());
    }

    /// <summary>An expiry mark with no frame in its window has no cost, and the error names the kind of the mark (T-2).</summary>
    [Fact]
    public void FrameLogRejectsAnEmptyExpiryWindow()
    {
        FrameLog log = new();
        log.MarkExpiry();
        ContextException error = Assert.Throws<ContextException>(() => log.ExpiryMaxima());
        Assert.Contains(error.Context, field => field.Name == "kind" && field.Value == "expiry");
        Assert.Contains(error.Context, field => field.Name == "mark" && field.Value == "0");
    }

    /// <summary>The events of a tick hold the expiry only with an expiry event: the spawn of the Overseer or a wave alone is no expiry (D-646).</summary>
    [Fact]
    public void TheExpiryIsTheTickOfTheExpiryEvent()
    {
        Assert.False(FrameLog.HoldsExpiry([]));
        Assert.False(FrameLog.HoldsExpiry([new TimerEvent(TimerEventKind.HunterSpawn, 1, 10799, 0, 0)]));
        Assert.False(FrameLog.HoldsExpiry([new TimerEvent(TimerEventKind.Wave, 1, 12599, 1, 1), new TimerEvent(TimerEventKind.WaveSkip, 1, 12599, 1, 2)]));
        Assert.True(FrameLog.HoldsExpiry([new TimerEvent(TimerEventKind.Expiry, 1, 10799, 0, 0), new TimerEvent(TimerEventKind.HunterSpawn, 1, 10799, 0, 0)]));
    }

    /// <summary>
    /// The timer tester on the seed of the session, with the enemies, reaches the expiry of the floor timer on floor 1
    /// before a death and before the tick budget, so the command with the policy flag measures the expiry (D-646,
    /// RR-P3-16). The session then ends on its own, at the end of the run, inside the budget.
    /// </summary>
    [Fact]
    public void TheTimerTesterReachesTheExpiryWithTheEnemies()
    {
        SimulationLoop loop = new(Main.FirstSeed, TestWorld.Content);
        IBotPolicy bot = BotSession.PolicyFor(TimerTester.PolicyName, TestWorld.Content, Main.FirstSeed);
        Assert.NotEmpty(TestWorld.Content.Enemies);
        long expiryTick = -1;
        while (!loop.Ended && !BotSession.IsComplete(loop, 1, loop.Tick) && !BotSession.IsStuck(loop, 1, loop.Tick))
        {
            loop.Step(bot.Next(loop));
            if (FrameLog.HoldsExpiry(loop.LastEvents))
            {
                Assert.Equal(-1, expiryTick);
                expiryTick = loop.Tick;
                Assert.NotNull(loop.Hunter);
            }
        }

        Assert.True(expiryTick > 0, $"The timer tester of seed {Main.FirstSeed} met no expiry. It ended at tick {loop.Tick} as {loop.End}.");
        Assert.Equal(SimulationLoop.FirstFloor, loop.Floor);
        Assert.True(loop.Ended, $"The run of seed {Main.FirstSeed} did not end inside the tick budget, at tick {loop.Tick}.");
        Assert.True(loop.Tick < BotSession.TickBudget);
        Assert.True(loop.Tick > expiryTick + FrameLog.TransitionWindowFrames, "The run ends before the window after the expiry fills.");
    }

    /// <summary>Each name of the policy flag builds the policy of that name, and a name outside the list is an error that names it (T-2, D-646).</summary>
    [Fact]
    public void BotSessionBuildsEachPolicy()
    {
        foreach (string name in BotSession.PolicyNames)
        {
            IBotPolicy policy = BotSession.PolicyFor(name, TestWorld.Content, Main.FirstSeed);
            Assert.Equal(name, policy.Name);
        }

        ContextException error = Assert.Throws<ContextException>(() => BotSession.PolicyFor("idler", TestWorld.Content, Main.FirstSeed));
        Assert.StartsWith(BotSession.UnknownPolicyMessage, error.Message, StringComparison.Ordinal);
        Assert.Contains(error.Context, field => field.Name == "policy" && field.Value == "idler");
    }

    /// <summary>
    /// The Game drives the bot of the policy flag, marks the expiry from the timer events of the step, and writes the
    /// count and the slowest frame of the expiry windows into the end line (D-646).
    /// </summary>
    [Fact]
    public void TheGameMarksTheExpiryAndDrivesThePolicy()
    {
        string main = RepositoryRoot.ReadFile("WhatYouCarry.Game/Main.cs");
        Assert.Contains("this.bot = BotSession.PolicyFor(policyName, content, loop.Seed);", main, StringComparison.Ordinal);
        Assert.Contains("if (FrameLog.HoldsExpiry(this.loop.LastEvents))", main, StringComparison.Ordinal);
        Assert.Contains("this.frames?.MarkExpiry();", main, StringComparison.Ordinal);
        Assert.Contains("fields.Add(ExpiryMicrosMaxField, Slowest(this.frames.ExpiryMaxima()));", main, StringComparison.Ordinal);
        Assert.Equal("expiryMicrosMax", Main.ExpiryMicrosMaxField);
        Assert.Equal("expiries", Main.ExpiriesField);
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

    /// <summary>
    /// The hitch budget is two frames at 90 frames per second, 22222 microseconds (D-295, D-427, D-635, F-170). The old
    /// budget of 22000 failed a frame that spanned two vsync intervals, and the old log read the rounded interval.
    /// </summary>
    [Fact]
    public void HitchBudgetIsTwoFramesAtTheTarget()
    {
        Assert.Equal(22222, BotSession.HitchBudgetMicros);
        Assert.Equal(2 * FrameLog.MicrosecondsPerSecond / 90, BotSession.HitchBudgetMicros);
    }

    /// <summary>
    /// F-170. The real frame clock gives the microseconds between two readings, no time for the first reading, and an
    /// error for a reading that runs backward. The frame log takes the time in microseconds as it is.
    /// </summary>
    [Fact]
    public void TheRealFrameClockGivesTheTimeBetweenReadings()
    {
        RealFrameClock clock = new();
        Assert.Null(clock.Next(1000));
        Assert.Equal(11111L, clock.Next(12111));
        Assert.Equal(22223L, clock.Next(34334));
        Assert.Equal(0L, clock.Next(34334));
        Assert.Throws<ContextException>(() => clock.Next(34333));

        FrameLog log = new();
        log.AddMicros(22223);
        Assert.Equal([22223L], log.Frames);
    }

    /// <summary>F-170. The Game feeds the frame log and the transition trace from the real clock, and never from the engine delta.</summary>
    [Fact]
    public void TheFrameLogReadsTheRealClock()
    {
        string main = RepositoryRoot.ReadFile("WhatYouCarry.Game/Main.cs");
        Assert.Contains("this.frameClock.Next((long)Time.GetTicksUsec())", main, StringComparison.Ordinal);
        Assert.Contains("this.frames?.AddMicros(micros);", main, StringComparison.Ordinal);
        Assert.DoesNotContain("this.frames?.Add(delta)", main, StringComparison.Ordinal);
        Assert.DoesNotContain("delta * FrameLog.MicrosecondsPerSecond", main, StringComparison.Ordinal);
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
