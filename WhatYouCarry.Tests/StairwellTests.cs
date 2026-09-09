using System;
using System.Collections.Generic;
using WhatYouCarry.Core.Entities;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Physics;
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
        return new Cell(
            (int)MathF.Floor(body.Position.X),
            (int)MathF.Floor(body.Position.Y - PlayerBody.GroundProbe),
            (int)MathF.Floor(body.Position.Z));
    }

    /// <summary>The movement byte that moves the body toward a point on one axis, at the walk speed or less when the point is nearer than one tick of walk.</summary>
    private static sbyte Toward(float delta)
    {
        float perTick = PlayerBody.WalkSpeed * PlayerBody.TickSeconds;
        float fraction = Math.Clamp(delta / perTick, -1.0f, 1.0f);
        return (sbyte)MathF.Round(fraction * PlayerBody.MoveScale);
    }

    /// <summary>
    /// Drives the loop along the walkable path from its body to a floor cell, and records each intent. A step
    /// up is a jump in place, then a move once the feet clear the step (D-165). A drop is a walk off the edge.
    /// </summary>
    private static void WalkTo(SimulationLoop loop, Cell target, RunRecorder recorder, string context)
    {
        Reachability reach = Reachability.From(loop.Grid, FloorCellOf(loop.Body));
        IReadOnlyList<Cell> path = reach.PathTo(target);
        int ticks = 0;
        for (int index = 1; index < path.Count; index++)
        {
            Cell next = path[index];
            bool stepUp = next.Y == path[index - 1].Y + 1;
            while (true)
            {
                Assert.True(ticks++ < MaxWalkTicks, $"{context}: the walk to {target} passed {MaxWalkTicks} ticks at waypoint {index} of {path.Count}, toward {next}, with the body at {loop.Body.Position}.");

                Vector3 position = loop.Body.Position;
                float deltaX = next.X + 0.5f - position.X;
                float deltaZ = next.Z + 0.5f - position.Z;
                bool centered = MathF.Abs(deltaX) < 0.02f && MathF.Abs(deltaZ) < 0.02f;
                if (centered && loop.Body.IsOnGround() && FloorCellOf(loop.Body) == next)
                {
                    break;
                }

                Intent intent;
                if (stepUp && loop.Body.IsOnGround() && FloorCellOf(loop.Body).Y < next.Y)
                {
                    intent = new Intent(loop.Tick, 0, 0, 0, 0, Button.Jump);
                }
                else if (stepUp && !loop.Body.IsOnGround() && position.Y < next.Y + 1.0f)
                {
                    intent = new Intent(loop.Tick, 0, 0, 0, 0, 0);
                }
                else
                {
                    // Forward at yaw zero is minus Z, so a positive Z delta is a move backward (D-234).
                    intent = new Intent(loop.Tick, 0, 0, Toward(deltaX), (sbyte)(-Toward(deltaZ)), 0);
                }

                recorder.Record(intent);
                loop.Step(intent);
            }
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
        RunRecorder recorder = new(sink, RunRecord.NewHeader(TestWorld.Content.Hash, seed));
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
        FloorPlan expected = FloorGenerator.Generate(seed, 2, TestWorld.Content);
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
        ReplayResult replay = RunReplayer.Replay(sink.Bytes, TestWorld.Content, new JsonlLogger(logs));
        Assert.Equal(2, replay.Loop.Floor);
        Assert.True(replay.Loop.Ended);
        Assert.Equal(loop.Tick, replay.Loop.Tick);
        Assert.Equal(loop.Hash(), replay.Loop.Hash());
        Assert.Equal(loop.Body.Position, replay.Loop.Body.Position);
        Assert.Empty(logs.Lines);

        // A frame after the end is an error that names the frame, and never a tick that runs.
        List<byte> longer = [.. sink.Bytes, .. new Intent(loop.Tick, 0, 0, 0, 0, 0).Encode()];
        ContextException extra = Assert.Throws<ContextException>(() => RunReplayer.Replay(longer, TestWorld.Content, new JsonlLogger(new CollectingSink())));
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

    /// <summary>The hash covers the floor number and the run end after the body fields, so two runs that differ there alone give two hashes (D-160, G-20).</summary>
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

    /// <summary>The hash of a loop state with a floor and an end of the caller's choice, in the declared order.</summary>
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
        hash.Add(ended);
        return hash;
    }
}
