using System;
using System.Collections.Generic;
using WhatYouCarry.Core.Bots;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Entities;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.Pathfinding;
using WhatYouCarry.Core.Procgen;
using WhatYouCarry.Core.Replay;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Core.World;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The stairwell transition and the floor state of the loop (D-3, D-50, D-236, D-257; PR-9 exit test 8).</summary>
public sealed class StairwellTests
{
    /// <summary>The count of ticks that a walk may take before the test fails with the position of the body.</summary>
    private const int MaxWalkTicks = 60000;

    /// <summary>A sink that keeps the record in memory.</summary>
    private sealed class MemorySink : IRunRecordSink
    {
        public List<byte> Bytes { get; } = [];

        public void Append(byte[] bytes)
        {
            this.Bytes.AddRange(bytes);
        }
    }

    private sealed class CollectingSink : ILogSink
    {
        public List<string> Lines { get; } = [];

        public void Write(string line)
        {
            this.Lines.Add(line);
        }
    }

    /// <summary>The floor cell under the feet center of a body.</summary>
    private static Cell FloorCellOf(PlayerBody body)
    {
        return PathWalk.FloorCellOf(body.Position);
    }

    /// <summary>
    /// Drives the loop with the greedy descender, and records each intent, until the policy asks for the
    /// stairwell choice. The test then presses the bits itself, so the stairwell rules stay under its control.
    /// </summary>
    private static void WalkTo(SimulationLoop loop, Cell target, RunRecorder recorder, string context)
    {
        GreedyDescender policy = new(TestWorld.Content);
        int ticks = 0;
        while (true)
        {
            Assert.True(ticks++ < MaxWalkTicks, $"{context}: the walk to {target} passed {MaxWalkTicks} ticks, with the body at {loop.Body.Position}.");
            Intent intent = policy.Next(loop);
            if ((intent.Buttons & (Button.Interact | Button.Ascend)) != 0)
            {
                Assert.Equal(target, FloorCellOf(loop.Body));
                return;
            }

            recorder.Record(intent);
            loop.Step(intent);
        }
    }

    /// <summary>The stairwell of a loop, as the cell that the body stands on when it is there.</summary>
    private static Intent Press(SimulationLoop loop, ushort buttons)
    {
        return new Intent(loop.Tick, 0, 0, 0, 0, buttons);
    }

    /// <summary>
    /// PR-9 exit test 8. A body walks from the spawn to the stairwell of floor 1. The interact bit there digs
    /// floor 2 from the same run seed, and the body stands at its spawn. The body walks to the stairwell of floor
    /// 2, and the ascend bit there ends the run. A replay of the record reproduces both, tick for tick (D-257).
    /// </summary>
    [Fact]
    public void DescendAdvancesFloor()
    {
        const ulong seed = 1UL;
        MemorySink sink = new();
        RunRecorder recorder = new(sink, RunRecord.NewHeader(TestWorld.PeacefulContent.Hash, seed));
        SimulationLoop loop = TestWorld.NewLoop(seed);
        Assert.Equal(1, loop.Floor);
        Assert.False(loop.Ended);

        // Both bits at the spawn do nothing, because they act at the stairwell alone.
        Assert.NotEqual(FloorCellOf(loop.Body), loop.Plan.Stairwell);
        Intent away = Press(loop, (ushort)(Button.Interact | Button.Ascend));
        recorder.Record(away);
        loop.Step(away);
        Assert.Equal(1, loop.Floor);
        Assert.False(loop.Ended);

        WalkTo(loop, loop.Plan.Stairwell, recorder, "Floor 1");
        Assert.True(StairwellTransition.IsAtStairwell(loop.Body, loop.Plan.Stairwell));
        Assert.Equal(StairwellAction.Descend, StairwellTransition.Choose(Button.Interact, loop.Body, loop.Plan.Stairwell));

        Intent descend = Press(loop, Button.Interact);
        recorder.Record(descend);
        uint descendTick = loop.Tick;
        loop.Step(descend);

        Assert.Equal(2, loop.Floor);
        Assert.Equal(2, loop.Plan.Floor);
        Assert.False(loop.Ended);
        Assert.Equal(descendTick + 1, loop.Tick);
        FloorPlan expected = FloorGenerator.Generate(seed, 2, TestWorld.PeacefulContent);
        Assert.Equal(expected.Spawn, loop.Body.Position);
        Assert.Equal(expected.Stairwell, loop.Plan.Stairwell);
        Assert.True(SameBlocks(expected.Grid, loop.Grid), "Floor 2 of the loop is not the floor 2 of the generator.");

        // The interact bit held into the next tick does nothing at the spawn of floor 2.
        Intent held = Press(loop, Button.Interact);
        recorder.Record(held);
        loop.Step(held);
        Assert.Equal(2, loop.Floor);

        WalkTo(loop, loop.Plan.Stairwell, recorder, "Floor 2");
        Intent ascend = Press(loop, (ushort)(Button.Ascend | Button.Interact));
        recorder.Record(ascend);
        loop.Step(ascend);
        Assert.True(loop.Ended);
        Assert.Equal(2, loop.Floor);

        ContextException afterEnd = Assert.Throws<ContextException>(() => loop.Step(Press(loop, 0)));
        Assert.Contains("ended", afterEnd.Message, StringComparison.Ordinal);
        Assert.Contains("floor=2", afterEnd.Message, StringComparison.Ordinal);

        CollectingSink logs = new();
        ReplayResult replay = RunReplayer.Replay(sink.Bytes, TestWorld.PeacefulContent, new JsonlLogger(logs));
        Assert.Equal(2, replay.Loop.Floor);
        Assert.True(replay.Loop.Ended);
        Assert.Equal(loop.Tick, replay.Loop.Tick);
        Assert.Equal(loop.Hash(), replay.Loop.Hash());
        Assert.Equal(loop.Body.Position, replay.Loop.Body.Position);
        Assert.Empty(logs.Lines);

        // A frame after the end is an error that names the frame, and never a tick that runs.
        List<byte> longer = [.. sink.Bytes, .. new Intent(loop.Tick, 0, 0, 0, 0, 0).Encode()];
        ContextException extra = Assert.Throws<ContextException>(() => RunReplayer.Replay(longer, TestWorld.PeacefulContent, new JsonlLogger(new CollectingSink())));
        Assert.Contains($"frame={replay.FrameCount}", extra.Message, StringComparison.Ordinal);
    }

    /// <summary>Two grids hold the same blocks.</summary>
    private static bool SameBlocks(VoxelGrid first, VoxelGrid second)
    {
        if (first.SizeX != second.SizeX || first.SizeY != second.SizeY || first.SizeZ != second.SizeZ)
        {
            return false;
        }

        for (int y = 0; y < first.SizeY; y++)
        {
            for (int z = 0; z < first.SizeZ; z++)
            {
                for (int x = 0; x < first.SizeX; x++)
                {
                    if (first.Get(x, y, z) != second.Get(x, y, z))
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    /// <summary>
    /// The intents of a run with the enemies of the repository content: the greedy descender walks to the stairwell of
    /// floor 1, and the body then waits on the cell until a hit takes the last health. The last intent is the one of
    /// the lethal tick. Null when the run ends or leaves the cell before that, or the walk passes its ticks.
    /// </summary>
    private static List<Intent>? DeathOnTheStairwell(ulong seed)
    {
        SimulationLoop loop = new(seed, TestWorld.Content);
        GreedyDescender policy = new(TestWorld.Content);
        List<Intent> intents = [];
        while (true)
        {
            if (loop.Ended || loop.Tick >= MaxWalkTicks)
            {
                return null;
            }

            Intent intent = policy.Next(loop);
            if ((intent.Buttons & (Button.Interact | Button.Ascend)) != 0)
            {
                break;
            }

            intents.Add(intent);
            loop.Step(intent);
        }

        while (!loop.Ended)
        {
            if (!StairwellPrompt.IsOpen(loop) || loop.Tick >= MaxWalkTicks)
            {
                return null;
            }

            Intent idle = Press(loop, 0);
            intents.Add(idle);
            loop.Step(idle);
        }

        return loop.End == RunEnd.Death ? intents : null;
    }

    /// <summary>A loop that replays every intent of a list but the last ones, which the caller steps itself.</summary>
    private static SimulationLoop ReplayAllBut(ulong seed, List<Intent> intents, int leftOut)
    {
        SimulationLoop loop = new(seed, TestWorld.Content);
        for (int index = 0; index < intents.Count - leftOut; index++)
        {
            loop.Step(intents[index]);
        }

        return loop;
    }

    /// <summary>
    /// F-113. A hit that takes the last health on the tick of a stairwell press ends the run as a death, and the
    /// press does nothing (D-322, D-403). The old loop took the press after the hit: the descend built a player with
    /// no health and threw, and the ascend ended a dead run as an ascend. The same press one tick earlier, while the
    /// player lives, still descends or ascends.
    /// </summary>
    [Theory]
    [InlineData(Button.Interact)]
    [InlineData(Button.Ascend)]
    public void ALethalHitOnTheTickOfAStairwellPressIsADeath(ushort press)
    {
        int deaths = 0;
        for (ulong seed = 1; seed <= 12; seed++)
        {
            List<Intent>? intents = DeathOnTheStairwell(seed);
            if (intents is null)
            {
                continue;
            }

            deaths++;
            SimulationLoop lethal = ReplayAllBut(seed, intents, 1);
            Assert.True(StairwellPrompt.IsOpen(lethal), $"Seed {seed}: the prompt is not open before the lethal tick {lethal.Tick}.");
            lethal.Step(intents[^1] with { Buttons = press });
            Assert.True(lethal.End == RunEnd.Death, $"Seed {seed}: the press {press} on the lethal tick ended the run as {lethal.End}.");
            Assert.True(lethal.Player.IsDead, $"Seed {seed}: the player lives after the lethal tick.");
            Assert.NotEqual(string.Empty, lethal.DeathCause);
            Assert.Equal(SimulationLoop.FirstFloor, lethal.Floor);
            Assert.Equal(SimulationLoop.FirstFloor, lethal.Plan.Floor);

            SimulationLoop earlier = ReplayAllBut(seed, intents, 2);
            earlier.Step(intents[^2] with { Buttons = press });
            if (press == Button.Ascend)
            {
                Assert.True(earlier.End == RunEnd.Ascend, $"Seed {seed}: the ascend one tick before the lethal tick ended the run as {earlier.End}.");
            }
            else
            {
                Assert.False(earlier.Ended, $"Seed {seed}: the descend one tick before the lethal tick ended the run as {earlier.End}.");
                Assert.Equal(SimulationLoop.FirstFloor + 1, earlier.Floor);
            }
        }

        Assert.True(deaths > 0, "No seed from 1 to 12 gave a death on the stairwell of floor 1, so the test read no lethal tick.");
    }

    /// <summary>The repository content with no enemy family and one floor template, cut to floor 1, so floor 1 is the deepest floor.</summary>
    private static ContentSet OneFloorContent()
    {
        FloorTemplate? first = null;
        foreach (FloorTemplate template in TestWorld.PeacefulContent.Floors)
        {
            if (template.MinDepth == SimulationLoop.FirstFloor)
            {
                first = template;
            }
        }

        Assert.NotNull(first);
        return TestWorld.PeacefulContent with { Floors = [first with { MaxDepth = SimulationLoop.FirstFloor }] };
    }

    /// <summary>Steps the loop with the greedy descender until the policy asks for the stairwell choice.</summary>
    private static void WalkToTheChoice(SimulationLoop loop, ContentSet content)
    {
        GreedyDescender policy = new(content);
        while (true)
        {
            Assert.True(!loop.Ended && loop.Tick < MaxWalkTicks, $"Seed {loop.Seed}: the walk to the stairwell ended or passed its ticks at tick {loop.Tick}.");
            Intent intent = policy.Next(loop);
            if ((intent.Buttons & (Button.Interact | Button.Ascend)) != 0)
            {
                return;
            }

            loop.Step(intent);
        }
    }

    /// <summary>
    /// F-114. The stairwell of the deepest floor offers the ascend alone. A descend press there does nothing: the
    /// run goes on, on the same floor, and the old loop threw because no template covers the floor under it
    /// (D-5, D-579). The HUD hides the descend line, and the ascend still ends the run.
    /// </summary>
    [Fact]
    public void ADescendOnTheDeepestFloorDoesNothing()
    {
        ContentSet oneFloor = OneFloorContent();
        SimulationLoop loop = new(3UL, oneFloor);
        Assert.Equal(SimulationLoop.FirstFloor, loop.DeepestFloor);
        WalkToTheChoice(loop, oneFloor);
        Assert.True(StairwellPrompt.IsOpen(loop));
        Assert.False(StairwellPrompt.OffersDescend(loop));
        Assert.False(WhatYouCarry.Game.Ui.HudState.Of(loop, false).DescendOffered);

        loop.Step(Press(loop, Button.Interact));
        Assert.False(loop.Ended);
        Assert.Equal(SimulationLoop.FirstFloor, loop.Floor);
        Assert.Equal(SimulationLoop.FirstFloor, loop.Plan.Floor);
        Assert.True(StairwellPrompt.IsOpen(loop));

        loop.Step(Press(loop, Button.Ascend));
        Assert.Equal(RunEnd.Ascend, loop.End);
    }

    /// <summary>The boundary beside the deepest floor: a floor above it offers the descend, and the HUD shows the line (D-50, D-579).</summary>
    [Fact]
    public void AFloorAboveTheDeepestOffersTheDescend()
    {
        SimulationLoop loop = TestWorld.NewLoop(3UL);
        Assert.Equal(15, loop.DeepestFloor);
        WalkToTheChoice(loop, TestWorld.PeacefulContent);
        Assert.True(StairwellPrompt.OffersDescend(loop));
        Assert.True(WhatYouCarry.Game.Ui.HudState.Of(loop, false).DescendOffered);
    }

    /// <summary>A body is at the stairwell when it stands on the ground on that cell, and not one cell over, and not in the air.</summary>
    [Fact]
    public void AtTheStairwellMeansOnTheCell()
    {
        VoxelGrid grid = TestWorld.FlatFloor();
        Cell stairwell = new(4, 0, 4);

        Assert.True(StairwellTransition.IsAtStairwell(new PlayerBody(grid, new Vector3(4.5f, 1.0f, 4.5f)), stairwell));
        Assert.True(StairwellTransition.IsAtStairwell(new PlayerBody(grid, new Vector3(4.05f, 1.0f, 4.95f)), stairwell));
        Assert.False(StairwellTransition.IsAtStairwell(new PlayerBody(grid, new Vector3(5.5f, 1.0f, 4.5f)), stairwell));
        Assert.False(StairwellTransition.IsAtStairwell(new PlayerBody(grid, new Vector3(4.5f, 1.0f, 3.5f)), stairwell));
        Assert.False(StairwellTransition.IsAtStairwell(new PlayerBody(grid, new Vector3(4.5f, 2.5f, 4.5f)), stairwell));

        PlayerBody body = new(grid, new Vector3(4.5f, 1.0f, 4.5f));
        Assert.Equal(StairwellAction.None, StairwellTransition.Choose(0, body, stairwell));
        Assert.Equal(StairwellAction.None, StairwellTransition.Choose(Button.Jump | Button.Sprint, body, stairwell));
        Assert.Equal(StairwellAction.Descend, StairwellTransition.Choose(Button.Interact, body, stairwell));
        Assert.Equal(StairwellAction.Ascend, StairwellTransition.Choose(Button.Ascend, body, stairwell));
        Assert.Equal(StairwellAction.Ascend, StairwellTransition.Choose(Button.Ascend | Button.Interact, body, stairwell));
        Assert.Equal(StairwellAction.None, StairwellTransition.Choose(Button.Interact, body, new Cell(3, 0, 4)));
    }

    /// <summary>The hash covers the floor number and the run end after the body fields and before the projectiles, so two runs that differ there alone give two hashes (D-160, G-20).</summary>
    [Fact]
    public void TheHashCoversTheFloorAndTheEnd()
    {
        Assert.Equal(1, SimulationLoop.FirstFloor);
        SimulationLoop loop = TestWorld.NewLoop(3UL);
        Assert.Equal(1, loop.Floor);
        Assert.False(loop.Ended);
        Assert.Equal(loop.Plan.Grid, loop.Grid);
        Assert.Equal(loop.Plan.Spawn, loop.Body.Position);
        Assert.Equal(TestWorld.NewLoop(3UL).Hash(), loop.Hash());

        // The same fields in the same order, with the floor and the end changed, give another hash.
        Assert.NotEqual(HashWith(loop, 1, false), HashWith(loop, 2, false));
        Assert.NotEqual(HashWith(loop, 1, false), HashWith(loop, 1, true));
        Assert.Equal(HashWith(loop, 1, false), loop.Hash());
    }

    /// <summary>
    /// The hash of a loop state with a floor and an end of the caller's choice, in the declared order: the run end is
    /// one byte of its kind, an ascend here, the player follows the projectiles, and the enemies with their brains
    /// follow the player (D-322, PR-15, PR-16).
    /// </summary>
    private static Core.Determinism.StateHash HashWith(SimulationLoop loop, int floor, bool ended)
    {
        Core.Determinism.StateHash hash = Core.Determinism.StateHash.Start();
        hash.Add(loop.Seed);
        hash.Add(loop.Tick);
        hash.Add(loop.Yaw);
        hash.Add(loop.Pitch);
        hash.Add((uint)loop.Buttons);
        hash.Add(loop.Body.Position.X);
        hash.Add(loop.Body.Position.Y);
        hash.Add(loop.Body.Position.Z);
        hash.Add(loop.Body.VerticalVelocity);
        hash.Add(floor);
        hash.Add(ended ? (byte)RunEnd.Ascend : (byte)RunEnd.None);
        loop.Projectiles.AddTo(ref hash);
        loop.Player.AddTo(ref hash);
        hash.Add(loop.Enemies.Count);
        for (int index = 0; index < loop.Enemies.Count; index++)
        {
            loop.Enemies[index].AddTo(ref hash);
            loop.Brains[index].AddTo(ref hash);
        }

        loop.Timer.AddTo(ref hash);
        hash.Add(loop.Hunter is not null);
        loop.Hunter?.AddTo(ref hash);
        loop.Escalation.AddTo(ref hash);
        return hash;
    }
}
