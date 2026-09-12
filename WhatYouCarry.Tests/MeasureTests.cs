using System;
using WhatYouCarry.Core.Bots;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Game;
using WhatYouCarry.Game.Measure;
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

    /// <summary>The flag takes the next argument as the path, and a flag with no path is an error.</summary>
    [Fact]
    public void FrameLogReadsThePathAfterTheFlag()
    {
        Assert.True(FrameLog.IsRequested([BotSession.Flag, FrameLog.Flag, "frames.txt"]));
        Assert.False(FrameLog.IsRequested([BotSession.Flag]));
        Assert.Equal("frames.txt", FrameLog.PathOf([BotSession.Flag, FrameLog.Flag, "frames.txt"]));
        Assert.Throws<ContextException>(() => FrameLog.PathOf([FrameLog.Flag]));
        Assert.Throws<ContextException>(() => FrameLog.PathOf([BotSession.Flag]));
    }

    /// <summary>The bot flag starts the session, and nothing else does.</summary>
    [Fact]
    public void BotSessionReadsTheFlag()
    {
        Assert.True(BotSession.IsRequested([BotSession.Flag]));
        Assert.True(BotSession.IsRequested(["--other", BotSession.Flag]));
        Assert.False(BotSession.IsRequested([]));
        Assert.False(BotSession.IsRequested([FrameLog.Flag, "frames.txt"]));
    }

    /// <summary>The greedy descender drives the loop of the first seed off the first floor inside the tick budget.</summary>
    [Fact]
    public void BotSessionCompletesOneFloor()
    {
        SimulationLoop loop = new(Main.FirstSeed, TestWorld.Content);
        GreedyDescender bot = new(TestWorld.Content);

        Assert.False(BotSession.IsComplete(loop));
        Assert.False(BotSession.IsStuck(loop));
        while (!BotSession.IsComplete(loop) && !BotSession.IsStuck(loop))
        {
            loop.Step(bot.Next(loop));
        }

        Assert.True(BotSession.IsComplete(loop), $"The bot is stuck on floor 1 of seed {Main.FirstSeed} at tick {loop.Tick}.");
        Assert.Equal(SimulationLoop.FirstFloor + 1, loop.Floor);
        Assert.Equal(BotRun.FloorBudget, BotSession.TickBudget);
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
