using System;
using System.Collections.Generic;
using WhatYouCarry.Core.Entities;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.Replay;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Core.World;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The player body and the loop that moves it (D-165, D-231 to D-238; PR-7 exit tests 2 to 5).</summary>
public sealed class PlayerBodyTests
{
    private const string Hash = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";
    private const float Skin = SweptAabb.ContactSkin;
    private const float TickMove = PlayerBody.WalkSpeed * PlayerBody.TickSeconds;

    /// <summary>An intent with no look, one movement pair, and one button mask, for tick zero unless the caller says otherwise.</summary>
    private static Intent Move(sbyte strafe, sbyte forward, ushort buttons = 0, uint tick = 0U)
    {
        return new Intent(tick, 0, 0, strafe, forward, buttons);
    }

    /// <summary>A sink that keeps the record in memory.</summary>
    private sealed class MemorySink : IRunRecordSink
    {
        public List<byte> Bytes { get; } = [];

        public void Append(byte[] bytes)
        {
            this.Bytes.AddRange(bytes);
        }
    }

    /// <summary>A log sink that keeps the lines.</summary>
    private sealed class CollectingSink : ILogSink
    {
        public List<string> Lines { get; } = [];

        public void Write(string line)
        {
            this.Lines.Add(line);
        }
    }

    /// <summary>
    /// A random grid: a stone floor, then stone in one quarter of the cells of the five rows above it. The spawn
    /// is a column with two air cells over the floor, so the body fits. A failure of the search is a defect of
    /// the test and never a pass.
    /// </summary>
    private static (VoxelGrid Grid, Vector3 Spawn) RandomWorld(Random random)
    {
        VoxelGrid grid = new(10, 8, 10);
        for (int x = 0; x < 10; x++)
        {
            for (int z = 0; z < 10; z++)
            {
                grid.Set(x, 0, z, BlockId.RawStone);
                for (int y = 1; y <= 5; y++)
                {
                    if (random.Next(4) == 0)
                    {
                        grid.Set(x, y, z, BlockId.RawStone);
                    }
                }
            }
        }

        for (int attempt = 0; attempt < 1000; attempt++)
        {
            int x = random.Next(10);
            int z = random.Next(10);
            if (!grid.IsSolid(x, 1, z) && !grid.IsSolid(x, 2, z))
            {
                return (grid, new Vector3(x + 0.5f, 1.0f, z + 0.5f));
            }
        }

        throw new InvalidOperationException("No column of the random grid has two air cells over the floor.");
    }

    /// <summary>The constants of D-165, D-231, D-233, and D-235 hold.</summary>
    [Fact]
    public void TheConstantsHold()
    {
        Assert.Equal(0.3f, PlayerBody.HalfWidth);
        Assert.Equal(1.8f, PlayerBody.Height);
        Assert.Equal(20.0f, PlayerBody.Gravity);
        Assert.Equal(4.0f, PlayerBody.WalkSpeed);
        Assert.Equal(6.5f, PlayerBody.SprintSpeed);
        Assert.Equal(7.0f, PlayerBody.JumpVelocity);
        Assert.Equal(1.0f / 60.0f, PlayerBody.TickSeconds);
        Assert.Equal(2.0f * SweptAabb.ContactSkin, PlayerBody.GroundProbe);
        Assert.Equal(127, PlayerBody.MoveScale);
    }

    /// <summary>The button bits of D-232, one by one, and the two masks.</summary>
    [Fact]
    public void TheButtonBitsHold()
    {
        Assert.Equal(0x0001, Button.Jump);
        Assert.Equal(0x0002, Button.Sprint);
        Assert.Equal(0x0004, Button.Dodge);
        Assert.Equal(0x0008, Button.Attack);
        Assert.Equal(0x0010, Button.Use);
        Assert.Equal(0x0020, Button.Interact);
        Assert.Equal(0x0040, Button.QuickSlotNext);
        Assert.Equal(0x0080, Button.QuickSlotPrevious);
        Assert.Equal(0x00FF, Button.AssignedMask);
        Assert.Equal(0xFF00, Button.ReservedMask);
    }

    /// <summary>
    /// PR-7 exit test 2. Over one thousand seeds, a body on a floor of a random height takes one thousand random
    /// intents, with jumps, and its feet never go below the top of the floor. A failure names its seed (D-66).
    /// </summary>
    [Fact]
    public void NoFallThroughFloor()
    {
        for (int seed = 1; seed <= 1000; seed++)
        {
            Random random = new(seed);
            int floorRow = random.Next(0, 3);
            VoxelGrid grid = new(8, 8, 8);
            for (int x = 0; x < 8; x++)
            {
                for (int z = 0; z < 8; z++)
                {
                    for (int y = 0; y <= floorRow; y++)
                    {
                        grid.Set(x, y, z, BlockId.RawStone);
                    }
                }
            }

            float floorTop = floorRow + 1;
            SimulationLoop loop = new((ulong)seed, grid, new Vector3(4.5f, floorTop, 4.5f));
            for (uint tick = 0; tick < 1000; tick++)
            {
                loop.Step(SimulationTests.RandomIntent(random, tick));
                Assert.True(loop.Body.Position.Y >= floorTop, $"Seed {seed}, tick {tick}: the feet are at {loop.Body.Position.Y}, below the floor top {floorTop}.");
            }

            Assert.True(loop.Body.IsOnGround() || loop.Body.Position.Y > floorTop + Skin, $"Seed {seed}: the body is neither on the ground nor above it at {loop.Body.Position}.");
        }
    }

    /// <summary>
    /// PR-7 exit test 3. A jump from flat ground with a walk toward a one-block step lands on the step. The
    /// same jump toward a two-block step never passes the wall of the step and ends back on the floor (D-165,
    /// D-231). The test asserts the two outcomes and never the apex, because a fixed step changes the apex.
    /// </summary>
    [Theory]
    [InlineData(1, true)]
    [InlineData(2, false)]
    public void JumpClearsOneBlock(int stepHeight, bool lands)
    {
        VoxelGrid grid = new(12, 8, 4);
        for (int x = 0; x < 12; x++)
        {
            for (int z = 0; z < 4; z++)
            {
                grid.Set(x, 0, z, BlockId.RawStone);
                if (x >= 6)
                {
                    for (int y = 1; y <= stepHeight; y++)
                    {
                        grid.Set(x, y, z, BlockId.RawStone);
                    }
                }
            }
        }

        // The body walks toward plus X, which is strafe at yaw zero (D-234), and jumps on the first tick.
        SimulationLoop loop = new(1UL, grid, new Vector3(4.5f, 1.0f, 2.0f));
        float stepTop = 1.0f + stepHeight;
        float highestFeet = 0.0f;
        for (uint tick = 0; tick < 90; tick++)
        {
            loop.Step(Move(127, 0, tick == 0 ? Button.Jump : (ushort)0, tick));
            highestFeet = MathF.Max(highestFeet, loop.Body.Position.Y);
            if (!lands)
            {
                Assert.True(loop.Body.Box.Max.X <= 6.0f, $"Tick {tick}: the body passed the two-block step at {loop.Body.Position}.");
            }
        }

        if (lands)
        {
            Assert.InRange(loop.Body.Position.Y, stepTop, stepTop + PlayerBody.GroundProbe);
            Assert.True(loop.Body.Box.Min.X >= 6.0f, $"The body is at {loop.Body.Position}, and not on the step.");
        }
        else
        {
            Assert.True(highestFeet < stepTop, $"The feet reached {highestFeet}, at or above the two-block step top {stepTop}.");
            Assert.InRange(loop.Body.Position.Y, 1.0f, 1.0f + PlayerBody.GroundProbe);
            Assert.InRange(loop.Body.Box.Max.X, 6.0f - Skin - 1e-5f, 6.0f - Skin + 1e-5f);
        }

        Assert.True(loop.Body.IsOnGround());
        Assert.Equal(0.0f, loop.Body.VerticalVelocity);
    }

    /// <summary>
    /// PR-7 exit test 4. Over one thousand seeds, a body in a random grid takes three hundred random intents and
    /// never overlaps a solid block after any tick. A failure names its seed (D-66).
    /// </summary>
    [Fact]
    public void NoOverlapAfterAnyTick()
    {
        for (int seed = 1; seed <= 1000; seed++)
        {
            Random random = new(seed);
            (VoxelGrid grid, Vector3 spawn) = RandomWorld(random);
            SimulationLoop loop = new((ulong)seed, grid, spawn);
            for (uint tick = 0; tick < 300; tick++)
            {
                loop.Step(SimulationTests.RandomIntent(random, tick));
                Assert.False(SweptAabb.Overlaps(grid, loop.Body.Box), $"Seed {seed}, tick {tick}: the body at {loop.Body.Position} overlaps a solid block.");
            }
        }
    }

    /// <summary>
    /// PR-7 exit test 5. Over one hundred seeds in random grids, a recorded run replays to the live end hash, and
    /// a second live run of the same intents gives the same hash. A failure names its seed (D-66).
    /// </summary>
    [Fact]
    public void PlayerBodyIsDeterministic()
    {
        for (int seed = 1; seed <= 100; seed++)
        {
            Random random = new(seed);
            (VoxelGrid grid, Vector3 spawn) = RandomWorld(random);

            MemorySink sink = new();
            RunRecorder recorder = new(sink, RunRecord.NewHeader(Hash, (ulong)seed));
            SimulationLoop live = new((ulong)seed, grid, spawn);
            SimulationLoop twin = new((ulong)seed, grid, spawn);
            for (uint tick = 0; tick < 200; tick++)
            {
                Intent intent = SimulationTests.RandomIntent(random, tick);
                recorder.Record(intent);
                live.Step(intent);
                twin.Step(intent);
            }

            CollectingSink logs = new();
            ReplayResult replay = RunReplayer.Replay(sink.Bytes, Hash, grid, spawn, new JsonlLogger(logs));

            Assert.True(live.Hash().Value == replay.Loop.Hash().Value, $"Seed {seed}: the live hash is {live.Hash()}, and the replay gives {replay.Loop.Hash()}.");
            Assert.True(live.Hash().Value == twin.Hash().Value, $"Seed {seed}: two live runs of one intent stream give {live.Hash()} and {twin.Hash()}.");
            Assert.True(live.Body.Position == replay.Loop.Body.Position, $"Seed {seed}: the live body is at {live.Body.Position}, and the replay body at {replay.Loop.Body.Position}.");
            Assert.Empty(logs.Lines);
        }
    }

    /// <summary>A set reserved bit is an error that names the tick and the buttons, and the tick does not advance (D-232, T-2).</summary>
    [Theory]
    [InlineData(0x0100)]
    [InlineData(0x8000)]
    [InlineData(0xFFFF)]
    public void AReservedButtonBitIsAnError(int buttons)
    {
        SimulationLoop loop = TestWorld.NewLoop(1UL);
        loop.Step(Move(0, 0));

        ContextException error = Assert.Throws<ContextException>(() => loop.Step(Move(0, 0, (ushort)buttons, 1U)));
        Assert.Contains("tick=1", error.Message, StringComparison.Ordinal);
        Assert.Contains("buttons=0x" + ((ushort)buttons).ToString("x4"), error.Message, StringComparison.Ordinal);
        Assert.Contains("D-232", error.Message, StringComparison.Ordinal);
        Assert.Equal(1U, loop.Tick);
    }

    /// <summary>Every assigned bit passes, alone and together.</summary>
    [Fact]
    public void EveryAssignedBitIsAccepted()
    {
        SimulationLoop loop = TestWorld.NewLoop(1UL);
        uint tick = 0;
        for (int bit = 0; bit < 8; bit++)
        {
            loop.Step(Move(0, 0, (ushort)(1 << bit), tick++));
        }

        loop.Step(Move(0, 0, Button.AssignedMask, tick++));
        Assert.Equal(9U, loop.Tick);
    }

    /// <summary>Forward at yaw zero is minus Z, and strafe is plus X, at the walk speed (D-233, D-234).</summary>
    [Fact]
    public void ForwardAtYawZeroIsMinusZ()
    {
        SimulationLoop forward = TestWorld.NewLoop(1UL);
        forward.Step(Move(0, 127));
        Assert.Equal(TestWorld.Spawn.X, forward.Body.Position.X);
        Assert.InRange(forward.Body.Position.Z, TestWorld.Spawn.Z - TickMove - 1e-6f, TestWorld.Spawn.Z - TickMove + 1e-6f);

        SimulationLoop strafe = TestWorld.NewLoop(1UL);
        strafe.Step(Move(127, 0));
        Assert.InRange(strafe.Body.Position.X, TestWorld.Spawn.X + TickMove - 1e-6f, TestWorld.Spawn.X + TickMove + 1e-6f);
        Assert.Equal(TestWorld.Spawn.Z, strafe.Body.Position.Z);

        SimulationLoop backward = TestWorld.NewLoop(1UL);
        backward.Step(Move(0, -127));
        Assert.InRange(backward.Body.Position.Z, TestWorld.Spawn.Z + TickMove - 1e-6f, TestWorld.Spawn.Z + TickMove + 1e-6f);
    }

    /// <summary>
    /// The yaw turns counterclockwise seen from above, so forward at 90 degrees is minus X and strafe is minus
    /// Z (D-234). The look delta of an intent applies before its movement.
    /// </summary>
    [Fact]
    public void ForwardAtNinetyDegreesIsMinusX()
    {
        SimulationLoop forward = TestWorld.NewLoop(1UL);
        forward.Step(new Intent(0U, 9000, 0, 0, 127, 0));
        Assert.Equal(9000, forward.Yaw);
        Assert.InRange(forward.Body.Position.X, TestWorld.Spawn.X - TickMove - 1e-5f, TestWorld.Spawn.X - TickMove + 1e-5f);
        Assert.InRange(forward.Body.Position.Z, TestWorld.Spawn.Z - 1e-5f, TestWorld.Spawn.Z + 1e-5f);

        SimulationLoop strafe = TestWorld.NewLoop(1UL);
        strafe.Step(new Intent(0U, 9000, 0, 127, 0, 0));
        Assert.InRange(strafe.Body.Position.Z, TestWorld.Spawn.Z - TickMove - 1e-5f, TestWorld.Spawn.Z - TickMove + 1e-5f);
        Assert.InRange(strafe.Body.Position.X, TestWorld.Spawn.X - 1e-5f, TestWorld.Spawn.X + 1e-5f);

        SimulationLoop half = TestWorld.NewLoop(1UL);
        half.Step(new Intent(0U, 18000, 0, 0, 127, 0));
        Assert.InRange(half.Body.Position.Z, TestWorld.Spawn.Z + TickMove - 1e-5f, TestWorld.Spawn.Z + TickMove + 1e-5f);
    }

    /// <summary>A diagonal moves at the walk speed and not faster, because the pair clamps to a length of one (D-233).</summary>
    [Fact]
    public void ADiagonalIsNotFasterThanAStraightLine()
    {
        SimulationLoop loop = TestWorld.NewLoop(1UL);
        loop.Step(Move(127, 127));

        float dx = loop.Body.Position.X - TestWorld.Spawn.X;
        float dz = loop.Body.Position.Z - TestWorld.Spawn.Z;
        float moved = MathF.Sqrt((dx * dx) + (dz * dz));
        Assert.InRange(moved, TickMove - 1e-5f, TickMove + 1e-5f);
        Assert.True(dx > 0.0f && dz < 0.0f, $"A diagonal of strafe right and forward moved by ({dx}, {dz}).");

        // A pair inside the unit circle keeps its length, so a half push moves at half speed.
        SimulationLoop half = TestWorld.NewLoop(1UL);
        half.Step(Move(64, 0));
        Assert.InRange(half.Body.Position.X - TestWorld.Spawn.X, (64.0f / 127.0f * TickMove) - 1e-6f, (64.0f / 127.0f * TickMove) + 1e-6f);
    }

    /// <summary>A movement byte of -128 clamps to -127, so both directions have one magnitude (D-233).</summary>
    [Fact]
    public void MinusOneTwentyEightClampsToMinusOneTwentySeven()
    {
        SimulationLoop left = TestWorld.NewLoop(1UL);
        left.Step(Move(-128, 0));
        SimulationLoop right = TestWorld.NewLoop(1UL);
        right.Step(Move(127, 0));

        float leftMove = TestWorld.Spawn.X - left.Body.Position.X;
        float rightMove = right.Body.Position.X - TestWorld.Spawn.X;
        Assert.Equal(rightMove, leftMove);

        SimulationLoop back = TestWorld.NewLoop(1UL);
        back.Step(Move(0, -128));
        Assert.Equal(rightMove, back.Body.Position.Z - TestWorld.Spawn.Z);
    }

    /// <summary>The sprint bit selects the sprint speed, and sprint is free (D-28, D-231, D-233).</summary>
    [Fact]
    public void TheSprintBitSelectsTheSprintSpeed()
    {
        SimulationLoop loop = TestWorld.NewLoop(1UL);
        loop.Step(Move(127, 0, Button.Sprint));

        float expected = PlayerBody.SprintSpeed * PlayerBody.TickSeconds;
        Assert.InRange(loop.Body.Position.X - TestWorld.Spawn.X, expected - 1e-6f, expected + 1e-6f);
    }

    /// <summary>A body at rest keeps its exact position and a vertical velocity of zero, tick after tick.</summary>
    [Fact]
    public void ABodyAtRestKeepsItsPosition()
    {
        SimulationLoop loop = TestWorld.NewLoop(1UL);
        Assert.True(loop.Body.IsOnGround());
        for (uint tick = 0; tick < 100; tick++)
        {
            loop.Step(Move(0, 0, 0, tick));
            Assert.Equal(TestWorld.Spawn, loop.Body.Position);
            Assert.Equal(0.0f, loop.Body.VerticalVelocity);
            Assert.True(loop.Body.IsOnGround());
        }
    }

    /// <summary>A body in the air falls, lands on the floor, and comes to rest with no vertical velocity.</summary>
    [Fact]
    public void AFallLandsOnTheFloor()
    {
        SimulationLoop loop = new(1UL, TestWorld.FlatFloor(), new Vector3(4.5f, 4.0f, 4.5f));
        Assert.False(loop.Body.IsOnGround());

        bool fell = false;
        for (uint tick = 0; tick < 120; tick++)
        {
            loop.Step(Move(0, 0, 0, tick));
            if (loop.Body.VerticalVelocity < 0.0f)
            {
                fell = true;
            }
        }

        Assert.True(fell);
        Assert.InRange(loop.Body.Position.Y, TestWorld.FloorTop, TestWorld.FloorTop + PlayerBody.GroundProbe);
        Assert.Equal(0.0f, loop.Body.VerticalVelocity);
        Assert.True(loop.Body.IsOnGround());
    }

    /// <summary>A jump needs the ground. A second press in the air changes nothing, and the body lands again.</summary>
    [Fact]
    public void AJumpNeedsTheGround()
    {
        SimulationLoop single = TestWorld.NewLoop(1UL);
        SimulationLoop repeated = TestWorld.NewLoop(1UL);
        for (uint tick = 0; tick < 60; tick++)
        {
            single.Step(Move(0, 0, tick == 0 ? Button.Jump : (ushort)0, tick));
            repeated.Step(Move(0, 0, tick == 0 || tick == 10 ? Button.Jump : (ushort)0, tick));
            Assert.Equal(single.Body.Position, repeated.Body.Position);
            Assert.Equal(single.Body.VerticalVelocity, repeated.Body.VerticalVelocity);
        }

        Assert.True(single.Body.IsOnGround());
        Assert.InRange(single.Body.Position.Y, TestWorld.FloorTop, TestWorld.FloorTop + PlayerBody.GroundProbe);

        // The first tick of a jump leaves the ground at the jump velocity less one tick of gravity.
        SimulationLoop first = TestWorld.NewLoop(1UL);
        first.Step(Move(0, 0, Button.Jump));
        Assert.Equal(PlayerBody.JumpVelocity - (PlayerBody.Gravity * PlayerBody.TickSeconds), first.Body.VerticalVelocity);
        Assert.True(first.Body.Position.Y > TestWorld.FloorTop);
        Assert.False(first.Body.IsOnGround());
    }

    /// <summary>A jump under a low ceiling stops at the ceiling with no vertical velocity, and the body falls back.</summary>
    [Fact]
    public void ACeilingStopsAJump()
    {
        VoxelGrid grid = TestWorld.FlatFloor();
        for (int x = 0; x < TestWorld.Side; x++)
        {
            for (int z = 0; z < TestWorld.Side; z++)
            {
                grid.Set(x, 3, z, BlockId.RawStone);
            }
        }

        SimulationLoop loop = new(1UL, grid, TestWorld.Spawn);
        loop.Step(Move(0, 0, Button.Jump));
        loop.Step(Move(0, 0, 0, 1U));

        // The head is at feet plus 1.8, and the ceiling face is at 3, so the feet stop at 1.2 less one skin.
        Assert.InRange(loop.Body.Box.Max.Y, 3.0f - Skin - 1e-5f, 3.0f - Skin + 1e-5f);
        Assert.Equal(0.0f, loop.Body.VerticalVelocity);

        for (uint tick = 2; tick < 60; tick++)
        {
            loop.Step(Move(0, 0, 0, tick));
        }

        Assert.True(loop.Body.IsOnGround());
    }

    /// <summary>A walk into a wall at an angle slides along it: the wall cuts one axis, and the other runs.</summary>
    [Fact]
    public void AWalkIntoAWallSlidesAlongIt()
    {
        VoxelGrid grid = TestWorld.FlatFloor();
        for (int y = 1; y < TestWorld.Height; y++)
        {
            for (int z = 0; z < TestWorld.Side; z++)
            {
                grid.Set(6, y, z, BlockId.RawStone);
            }
        }

        SimulationLoop loop = new(1UL, grid, TestWorld.Spawn);
        for (uint tick = 0; tick < 60; tick++)
        {
            loop.Step(Move(127, 127, 0, tick));
        }

        Assert.InRange(loop.Body.Box.Max.X, 6.0f - Skin - 1e-5f, 6.0f - Skin + 1e-5f);
        Assert.True(loop.Body.Position.Z < TestWorld.Spawn.Z - 1.0f, $"The body slid to {loop.Body.Position}, and it should have moved along the wall.");
    }

    /// <summary>The edge of the grid stops a walk, so a body never leaves the world (D-237).</summary>
    [Fact]
    public void TheEdgeOfTheGridStopsAWalk()
    {
        SimulationLoop loop = TestWorld.NewLoop(1UL);
        for (uint tick = 0; tick < 120; tick++)
        {
            loop.Step(Move(127, 0, Button.Sprint, tick));
        }

        Assert.InRange(loop.Body.Box.Max.X, TestWorld.Side - Skin - 1e-5f, TestWorld.Side - Skin + 1e-5f);
    }

    /// <summary>The hash covers the position and the vertical velocity, after the five fields of D-227.</summary>
    [Fact]
    public void TheHashCoversTheBody()
    {
        SimulationLoop still = TestWorld.NewLoop(1UL);
        SimulationLoop moved = TestWorld.NewLoop(1UL);
        SimulationLoop jumped = TestWorld.NewLoop(1UL);
        still.Step(Move(0, 0));
        moved.Step(Move(127, 0));
        jumped.Step(Move(0, 0, Button.Jump));

        Assert.NotEqual(still.Hash(), moved.Hash());
        Assert.NotEqual(still.Hash(), jumped.Hash());
        Assert.NotEqual(moved.Hash(), jumped.Hash());
        Assert.Equal(TestWorld.NewLoop(1UL).Hash(), TestWorld.NewLoop(1UL).Hash());

        SimulationLoop elsewhere = new(1UL, TestWorld.FlatFloor(), new Vector3(2.5f, 1.0f, 2.5f));
        Assert.NotEqual(TestWorld.NewLoop(1UL).Hash(), elsewhere.Hash());
    }

    /// <summary>A spawn point inside rock, or past the grid, is an error that names the point (T-2).</summary>
    [Fact]
    public void ASpawnInsideRockIsAnError()
    {
        VoxelGrid grid = TestWorld.FlatFloor();

        ContextException inRock = Assert.Throws<ContextException>(() => new SimulationLoop(1UL, grid, new Vector3(4.5f, 0.5f, 4.5f)));
        Assert.Contains("spawn=", inRock.Message, StringComparison.Ordinal);

        Assert.Throws<ContextException>(() => new PlayerBody(grid, new Vector3(7.9f, 1.0f, 4.5f)));
        Assert.Throws<ContextException>(() => new PlayerBody(grid, new Vector3(4.5f, 4.5f, 4.5f)));
        Assert.Throws<ContextException>(() => new PlayerBody(grid, new Vector3(4.5f, 1.0f, -0.1f)));

        // A body that touches the floor and the walls is contact and not overlap.
        PlayerBody touching = new(grid, new Vector3(0.3f, 1.0f, 0.3f));
        Assert.Equal(new Vector3(0.0f, 1.0f, 0.0f), touching.Box.Min);
    }

    /// <summary>The box of a body is 0.6 by 1.8 by 0.6 meters around the feet center (D-165).</summary>
    [Fact]
    public void TheBoxSurroundsTheFeetCenter()
    {
        PlayerBody body = new(TestWorld.FlatFloor(), new Vector3(3.0f, 2.0f, 5.0f));
        Assert.Equal(new Vector3(2.7f, 2.0f, 4.7f), body.Box.Min);
        Assert.Equal(new Vector3(3.3f, 3.8f, 5.3f), body.Box.Max);
    }
}
