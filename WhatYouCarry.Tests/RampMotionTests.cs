using System;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Entities;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.Projectiles;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Core.World;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>A body on a ramp (D-345, D-362 to D-366; PR-64 exit tests 1 to 3). Each theory runs the course of <see cref="RampCourse"/> along every rise with every run.</summary>
public sealed class RampMotionTests
{
    private const float Skin = SweptAabb.ContactSkin;

    private static readonly EntityBox[] NoTargets = [];

    private static WeaponDefinition Sword => SimulationLoop.MainWeapon(TestWorld.Content);

    /// <summary>Every rise with every run of D-346.</summary>
    public static TheoryData<RampRise, int> Courses => RampCourse.EveryRiseAndRun();

    /// <summary>The yaw at which forward points up the rise (D-234): forward at yaw zero is minus Z, and the yaw turns counterclockwise seen from above.</summary>
    private static int YawUphill(RampRise rise)
    {
        switch (rise)
        {
            case RampRise.PlusX:
                return 27000;
            case RampRise.MinusX:
                return 9000;
            case RampRise.PlusZ:
                return 18000;
            default:
                return 0;
        }
    }

    /// <summary>A body that falls from over the ramp onto its slope, in the middle of the run, and comes to rest.</summary>
    private static PlayerBody BodyOnTheSlope(RampCourse course)
    {
        PlayerBody body = new(course.Grid, course.Point(RampCourse.RampStart + (course.Run / 2.0f), RampCourse.HighTop + 0.05f, RampCourse.Middle));
        for (int tick = 0; tick < 60; tick++)
        {
            body.Move(new Vector3(0.0f, 0.0f, 0.0f), false, false, true);
        }

        Assert.True(body.IsOnGround(), $"Rise {course.Rise}, run {course.Run}: the body did not come to rest on the slope at {body.Position}.");
        return body;
    }

    /// <summary>PR-64 exit test 1. A walk from the low floor reaches the high floor with no jump, on the ground on every tick (D-345).</summary>
    [Theory]
    [MemberData(nameof(Courses))]
    public void BodyWalksUpARamp(RampRise rise, int run)
    {
        RampCourse course = new(rise, run);
        PlayerBody body = new(course.Grid, course.Point(3.5f, RampCourse.LowTop, RampCourse.Middle));
        for (int tick = 0; tick < 150; tick++)
        {
            body.Move(course.Uphill * PlayerBody.WalkSpeed, false, false, true);
            Assert.True(body.IsOnGround(), $"Rise {rise}, run {run}, tick {tick}: the body left the ground at {body.Position}.");
            Assert.True(body.VerticalVelocity <= 0.0f, $"Rise {rise}, run {run}, tick {tick}: the body rose with a velocity of {body.VerticalVelocity}.");
        }

        Assert.True(course.AlongOf(body.Position) > course.HighStart + 1.0f, $"Rise {rise}, run {run}: the walk ended at {body.Position}, before the high floor.");
        Assert.InRange(body.Position.Y, RampCourse.HighTop, RampCourse.HighTop + (2.0f * Skin));
    }

    /// <summary>PR-64 exit test 2. A walk and a sprint from the high floor reach the low floor and stay on the slope on every tick (D-363).</summary>
    [Theory]
    [MemberData(nameof(Courses))]
    public void BodyWalksDownARamp(RampRise rise, int run)
    {
        foreach (float speed in new[] { PlayerBody.WalkSpeed, PlayerBody.SprintSpeed })
        {
            RampCourse course = new(rise, run);
            PlayerBody body = new(course.Grid, course.Point(13.5f, RampCourse.HighTop, RampCourse.Middle));
            for (int tick = 0; tick < 150; tick++)
            {
                body.Move(course.Uphill * -speed, false, false, true);
                Assert.True(body.IsOnGround(), $"Rise {rise}, run {run}, speed {speed}, tick {tick}: the body left the slope at {body.Position}.");
            }

            Assert.True(course.AlongOf(body.Position) < RampCourse.RampStart - 1.0f, $"Rise {rise}, run {run}, speed {speed}: the walk ended at {body.Position}, before the low floor.");
            Assert.InRange(body.Position.Y, RampCourse.LowTop, RampCourse.LowTop + (2.0f * Skin));
        }
    }

    /// <summary>Up and down a slope, the speed along the slope is the flat speed, and across the slope the horizontal speed is the flat speed (D-362).</summary>
    [Theory]
    [MemberData(nameof(Courses))]
    public void TheSpeedAlongASlopeIsTheFlatSpeed(RampRise rise, int run)
    {
        RampCourse course = new(rise, run);
        float expected = PlayerBody.WalkSpeed * PlayerBody.TickSeconds;
        float horizontal = expected / MathF.Sqrt(1.0f + (1.0f / (run * run)));
        foreach (float direction in new[] { 1.0f, -1.0f })
        {
            PlayerBody body = BodyOnTheSlope(course);
            Vector3 before = body.Position;
            body.Move(course.Uphill * (direction * PlayerBody.WalkSpeed), false, false, true);
            float moved = MathF.Abs(course.AlongOf(body.Position) - course.AlongOf(before));
            float climb = body.Position.Y - before.Y;
            Assert.InRange(moved, horizontal - 1e-5f, horizontal + 1e-5f);
            Assert.InRange(MathF.Sqrt((moved * moved) + (climb * climb)), expected - 1e-5f, expected + 1e-5f);
            Assert.True(direction > 0.0f ? climb > 0.0f : climb < 0.0f, $"Rise {rise}, run {run}: a move of sign {direction} changed the height by {climb}.");
        }

        PlayerBody across = BodyOnTheSlope(course);
        Vector3 start = across.Position;
        across.Move(course.Across * PlayerBody.WalkSpeed, false, false, true);
        Vector3 step = across.Position - start;
        Assert.InRange(MathF.Sqrt((step.X * step.X) + (step.Z * step.Z)), expected - 1e-6f, expected + 1e-6f);
        Assert.Equal(start.Y, across.Position.Y);
    }

    /// <summary>A body with no walk stays where it stands on every slope, whether it follows the slope or not (D-364).</summary>
    [Theory]
    [MemberData(nameof(Courses))]
    public void ABodyAtRestStaysOnARamp(RampRise rise, int run)
    {
        PlayerBody body = BodyOnTheSlope(new RampCourse(rise, run));
        Vector3 rest = body.Position;
        for (int tick = 0; tick < 120; tick++)
        {
            body.Move(new Vector3(0.0f, 0.0f, 0.0f), false, false, tick % 2 == 0);
            Assert.Equal(rest, body.Position);
        }
    }

    /// <summary>A jump starts on a slope, and its apex stays near the one block of a jump on flat ground (D-165, D-365).</summary>
    [Theory]
    [MemberData(nameof(Courses))]
    public void AJumpStartsOnARamp(RampRise rise, int run)
    {
        PlayerBody body = BodyOnTheSlope(new RampCourse(rise, run));
        float takeOff = body.Position.Y;
        body.Move(new Vector3(0.0f, 0.0f, 0.0f), true, false, true);
        Assert.True(body.VerticalVelocity > 6.0f, $"Rise {rise}, run {run}: the jump from the slope gave a velocity of {body.VerticalVelocity}.");

        float apex = body.Position.Y;
        for (int tick = 0; tick < 120; tick++)
        {
            body.Move(new Vector3(0.0f, 0.0f, 0.0f), false, false, true);
            apex = body.Position.Y > apex ? body.Position.Y : apex;
        }

        Assert.InRange(apex - takeOff, 1.1f, 1.25f);
        Assert.Equal(takeOff, body.Position.Y, 4);
        Assert.True(body.IsOnGround());
    }

    /// <summary>A roll starts on the low floor, climbs the slope, and stays on the ground on every tick of the roll (D-366).</summary>
    [Theory]
    [MemberData(nameof(Courses))]
    public void ARollClimbsARamp(RampRise rise, int run)
    {
        RampCourse course = new(rise, run);
        Player player = new(course.Grid, course.Point(RampCourse.RampStart - 0.5f, RampCourse.LowTop, RampCourse.Middle), Sword, Player.MaxHealth);
        int yaw = YawUphill(rise);
        ushort previous = 0;
        for (uint tick = 0; tick < Player.RollTicks; tick++)
        {
            ushort buttons = tick == 0 ? Button.Dodge : (ushort)0;
            player.Step(new Intent(tick, 0, 0, 0, 127, buttons), previous, yaw, NoTargets);
            previous = buttons;
            Assert.True(player.Body.IsOnGround(), $"Rise {rise}, run {run}, tick {tick}: the roll left the slope at {player.Body.Position}.");
        }

        Assert.Equal(0, player.RollRemaining);
        Assert.True(player.Body.Position.Y > RampCourse.LowTop + 0.5f, $"Rise {rise}, run {run}: the roll ended at {player.Body.Position}, and it did not climb.");
    }

    /// <summary>A roll from the high floor down the slope leaves the slope, and gravity takes over, while a walk stays on it (D-363).</summary>
    [Theory]
    [MemberData(nameof(Courses))]
    public void ARollLeavesARampOnTheWayDown(RampRise rise, int run)
    {
        RampCourse course = new(rise, run);
        Player player = new(course.Grid, course.Point(course.HighStart + 0.5f, RampCourse.HighTop, RampCourse.Middle), Sword, Player.MaxHealth);
        int yaw = (YawUphill(rise) + 18000) % SimulationLoop.FullTurn;
        ushort previous = 0;
        bool airborne = false;
        for (uint tick = 0; tick < Player.RollTicks; tick++)
        {
            ushort buttons = tick == 0 ? Button.Dodge : (ushort)0;
            player.Step(new Intent(tick, 0, 0, 0, 127, buttons), previous, yaw, NoTargets);
            previous = buttons;
            airborne |= !player.Body.IsOnGround();
        }

        Assert.True(airborne, $"Rise {rise}, run {run}: the roll down the slope never left the ground.");
    }

    /// <summary>A staggered body on a slope stays where it stands through the stagger (D-326, D-364).</summary>
    [Theory]
    [MemberData(nameof(Courses))]
    public void AStaggerStaysOnARamp(RampRise rise, int run)
    {
        RampCourse course = new(rise, run);
        Player player = new(course.Grid, course.Point(RampCourse.RampStart + (run / 2.0f), RampCourse.HighTop + 0.05f, RampCourse.Middle), Sword, Player.MaxHealth);
        for (uint tick = 0; tick < 60; tick++)
        {
            player.Step(new Intent(tick, 0, 0, 0, 0, 0), 0, 0, NoTargets);
        }

        Vector3 rest = player.Body.Position;
        player.TakeHit(10);
        Assert.Equal(Player.StaggerTicks, player.StaggerRemaining);
        for (uint tick = 60; tick < 60 + Player.StaggerTicks; tick++)
        {
            player.Step(new Intent(tick, 0, 0, 0, 127, 0), 0, 0, NoTargets);
            Assert.Equal(rest, player.Body.Position);
        }
    }

    /// <summary>
    /// PR-64 exit test 3, the body. On every course, random intents at the sprint speed with jumps, and random moves
    /// at the roll speed that follow the slope or not, never put the body inside a block or under a slope, and its
    /// feet never go below the low floor. A failure names its seed (D-66).
    /// </summary>
    [Fact]
    public void NoFallThroughASlope()
    {
        foreach (object[] course in Courses)
        {
            RampCourse ramp = new((RampRise)course[0], (int)course[1]);
            for (int seed = 1; seed <= 40; seed++)
            {
                Random random = new(seed);
                PlayerBody body = new(ramp.Grid, ramp.Point(1.0f + ((float)random.NextDouble() * 14.0f), RampCourse.HighTop + 0.05f, 1.0f + ((float)random.NextDouble() * 7.0f)));
                for (uint tick = 0; tick < 400; tick++)
                {
                    if (random.Next(3) == 0)
                    {
                        float angle = (float)(random.NextDouble() * 2.0 * Math.PI);
                        Vector3 roll = new(MathF.Sin(angle) * Player.RollSpeed, 0.0f, MathF.Cos(angle) * Player.RollSpeed);
                        body.Move(roll, random.Next(2) == 0, false, random.Next(2) == 0);
                    }
                    else
                    {
                        body.Step(SimulationTests.RandomIntent(random, tick), random.Next(SimulationLoop.FullTurn));
                    }

                    Assert.False(SweptAabb.Overlaps(ramp.Grid, body.Box), $"Rise {ramp.Rise}, run {ramp.Run}, seed {seed}, tick {tick}: the body at {body.Position} overlaps a solid part.");
                    Assert.True(body.Position.Y >= RampCourse.LowTop, $"Rise {ramp.Rise}, run {ramp.Run}, seed {seed}, tick {tick}: the feet at {body.Position} are below the low floor.");
                }
            }
        }
    }

    /// <summary>
    /// PR-64 exit test 3, the sweep. Over random grids of blocks and ramps of every rise, run, and place, a box moves
    /// up to twenty meters in one tick, five times, and never ends inside a block or under a slope, and never falls
    /// through the stone floor. A failure names its seed (D-66).
    /// </summary>
    [Fact]
    public void NoTunnelThroughARamp()
    {
        for (int seed = 1; seed <= 4000; seed++)
        {
            Random random = new(seed);
            VoxelGrid grid = RandomRampWorld(random);
            Aabb box = FreeBox(grid, random, seed);
            for (int step = 0; step < 5; step++)
            {
                float speed = (float)(random.NextDouble() * (random.Next(2) == 0 ? 20.0 : 0.3));
                Vector3 delta = new(((float)random.NextDouble() - 0.5f) * 2.0f * speed, ((float)random.NextDouble() - 0.5f) * 2.0f * speed, ((float)random.NextDouble() - 0.5f) * 2.0f * speed);
                SweepResult result = SweptAabb.Sweep(grid, box, delta);
                box = box.Moved(result.Allowed);
                Assert.False(SweptAabb.Overlaps(grid, box), $"Seed {seed}, step {step}: the box {box} ends inside a solid part after the move {delta}.");
                Assert.True(box.Min.Y >= 1.0f, $"Seed {seed}, step {step}: the box {box} fell through the floor after the move {delta}.");
            }
        }
    }

    /// <summary>A ten by eight by ten grid: a stone floor, then in rows 1 to 5 a block in one cell of five and a ramp of a random rise, run, and place in one cell of five.</summary>
    internal static VoxelGrid RandomRampWorld(Random random)
    {
        VoxelGrid grid = new(10, 8, 10);
        for (int x = 0; x < 10; x++)
        {
            for (int z = 0; z < 10; z++)
            {
                grid.Set(x, 0, z, BlockId.RawStone);
                for (int y = 1; y <= 5; y++)
                {
                    int pick = random.Next(5);
                    if (pick == 0)
                    {
                        grid.Set(x, y, z, BlockId.RawStone);
                    }
                    else if (pick == 1)
                    {
                        int run = random.Next(Ramp.SteepestRun, Ramp.ShallowestRun + 1);
                        grid.Set(x, y, z, new Ramp((RampRise)random.Next(4), run, random.Next(run)).Id);
                    }
                }
            }
        }

        return grid;
    }

    /// <summary>A box of the player size at a random place that overlaps no solid part. A grid with no such place is a defect of the test and never a pass.</summary>
    private static Aabb FreeBox(VoxelGrid grid, Random random, int seed)
    {
        for (int attempt = 0; attempt < 1000; attempt++)
        {
            float x = 0.1f + ((float)random.NextDouble() * 9.2f);
            float y = 1.0f + ((float)random.NextDouble() * 5.0f);
            float z = 0.1f + ((float)random.NextDouble() * 9.2f);
            Aabb box = new(new Vector3(x, y, z), new Vector3(x + 0.6f, y + 1.8f, z + 0.6f));
            if (!SweptAabb.Overlaps(grid, box))
            {
                return box;
            }
        }

        throw new InvalidOperationException($"Seed {seed}: no place in the random ramp grid holds the box.");
    }
}
