using System;
using System.Collections.Generic;
using WhatYouCarry.Core.Camera;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Entities;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.Replay;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Core.World;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The orbit camera and aim assist (D-13, D-14, D-75, D-77, D-88, D-241 to D-249; PR-8 exit tests 1 to 4).</summary>
[Trait("Category", SweepScope.SweepCategory)]
public sealed class CameraTests
{
    private const float Sin80 = 0.98480775f;
    private const float Cos80 = 0.17364818f;

    /// <summary>The feet center in the middle of the open room.</summary>
    private static readonly Vector3 Feet = new(8.0f, 1.0f, 8.0f);

    private static readonly Vector3[] NoTargets = [];

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

    /// <summary>The angle between two directions, in degrees, from a double reference.</summary>
    private static double DegreesBetween(Vector3 first, Vector3 second)
    {
        double dot = ((double)first.X * second.X) + ((double)first.Y * second.Y) + ((double)first.Z * second.Z);
        double lengths = Math.Sqrt(((double)first.X * first.X) + ((double)first.Y * first.Y) + ((double)first.Z * first.Z))
            * Math.Sqrt(((double)second.X * second.X) + ((double)second.Y * second.Y) + ((double)second.Z * second.Z));
        return Math.Acos(Math.Clamp(dot / lengths, -1.0, 1.0)) * 180.0 / Math.PI;
    }

    /// <summary>A target ten meters ahead of the origin (0, 2, 0) along minus Z, turned by an angle toward plus X.</summary>
    private static Vector3 TargetAt(double degrees)
    {
        double radians = degrees * Math.PI / 180.0;
        return new Vector3((float)(10.0 * Math.Sin(radians)), 2.0f, (float)(-10.0 * Math.Cos(radians)));
    }

    /// <summary>Folds the camera pose and the aim ray of every replayed tick, so a test compares the replay with a live loop tick by tick.</summary>
    private sealed class ReplayCameraFold : IReplayObserver
    {
        private readonly Vector3[] targets;
        private StateHash hash = StateHash.Start();

        public ReplayCameraFold(Vector3[] targets)
        {
            this.targets = targets;
        }

        public StateHash Hash => this.hash;

        public void AfterTick(SimulationLoop loop)
        {
            AddPose(ref this.hash, loop.Camera(), loop.Aim(this.targets));
        }
    }

    /// <summary>Folds a pose and an aim ray into a hash, so a test compares two runs of a camera in one number.</summary>
    private static void AddPose(ref StateHash hash, CameraPose pose, AimRay aim)
    {
        hash.Add(pose.Position.X);
        hash.Add(pose.Position.Y);
        hash.Add(pose.Position.Z);
        hash.Add(pose.Forward.X);
        hash.Add(pose.Forward.Y);
        hash.Add(pose.Forward.Z);
        hash.Add(aim.Direction.X);
        hash.Add(aim.Direction.Y);
        hash.Add(aim.Direction.Z);
    }

    /// <summary>The constants of D-241, D-242, D-244, D-246, D-719, and D-720 hold. PR-97 regression: the shoulder offsets of D-242 fail it.</summary>
    [Fact]
    public void TheConstantsHold()
    {
        Assert.Equal(8000, SimulationLoop.PitchLimit);
        Assert.Equal(1.5f, OrbitCamera.PivotHeight);
        Assert.Equal(0.9f, OrbitCamera.ShoulderRight);
        Assert.Equal(0.7f, OrbitCamera.ShoulderUp);
        Assert.Equal(3.0f, OrbitCamera.BoomLength);
        Assert.Equal(0.25f, OrbitCamera.CameraRadius);
        Assert.Equal(2.0f, OrbitCamera.ClosestView);
        Assert.Equal(5.0f, AimAssist.ConeDegrees);
        Assert.Equal(0.5f, AimAssist.Strength);
    }

    /// <summary>
    /// At rest the camera sits behind the right shoulder: 0.9 right, 0.7 up, and 3 back from the pivot 1.5 over the feet
    /// (D-242, D-719). The camera sits 0.4 over the head of the body of 1.8 (F-195). With no wall the drawn camera is the
    /// end of the boom (D-720).
    /// </summary>
    [Fact]
    public void TheCameraSitsBehindTheRightShoulder()
    {
        CameraPose pose = OrbitCamera.Place(TestWorld.FlatFloor(16, 8), Feet, 0, 0);

        Assert.Equal(new Vector3(8.9f, 3.2f, 11.0f), pose.Position);
        Assert.Equal(pose.Position, pose.View);
        Assert.InRange(pose.Position.Y - Feet.Y - PlayerBody.Height, 0.4f - 1e-5f, 0.4f + 1e-5f);
        Assert.Equal(new Vector3(0.0f, 0.0f, -1.0f), pose.Forward);
        Assert.Equal(new Vector3(1.0f, 0.0f, 0.0f), pose.Right);
        Assert.Equal(new Vector3(0.0f, 1.0f, 0.0f), pose.Up);
    }

    /// <summary>A yaw of 90 degrees turns forward to minus X and right to minus Z, and the camera follows (D-234).</summary>
    [Fact]
    public void TheYawTurnsTheCamera()
    {
        CameraPose pose = OrbitCamera.Place(TestWorld.FlatFloor(16, 8), Feet, 9000, 0);

        Assert.InRange(pose.Forward.X, -1.0f - 1e-5f, -1.0f + 1e-5f);
        Assert.InRange(pose.Forward.Z, -1e-5f, 1e-5f);
        Assert.InRange(pose.Right.Z, -1.0f - 1e-5f, -1.0f + 1e-5f);
        Assert.InRange(pose.Position.X, 11.0f - 1e-4f, 11.0f + 1e-4f);
        Assert.InRange(pose.Position.Y, 3.2f - 1e-4f, 3.2f + 1e-4f);
        Assert.InRange(pose.Position.Z, 7.1f - 1e-4f, 7.1f + 1e-4f);
    }

    /// <summary>A positive pitch looks up, so forward gains a positive Y and the camera drops below the pivot (D-248).</summary>
    [Fact]
    public void APositivePitchLooksUp()
    {
        CameraPose up = OrbitCamera.Place(TestWorld.FlatFloor(16, 16), new Vector3(8.0f, 6.0f, 8.0f), 0, 4500);
        Assert.InRange(up.Forward.Y, 0.70710f - 1e-5f, 0.70710f + 1e-5f);
        Assert.InRange(up.Forward.Z, -0.70710f - 1e-5f, -0.70710f + 1e-5f);
        Assert.True(up.Position.Y < 7.5f, $"The camera at {up.Position} is not below the pivot for a look up.");
        Assert.True(up.Up.Y > 0.0f && up.Up.Z > 0.0f, $"The up vector {up.Up} does not tilt back for a look up.");

        CameraPose down = OrbitCamera.Place(TestWorld.FlatFloor(16, 16), new Vector3(8.0f, 6.0f, 8.0f), 0, -4500);
        Assert.InRange(down.Forward.Y, -0.70710f - 1e-5f, -0.70710f + 1e-5f);
        Assert.True(down.Position.Y > 7.5f, $"The camera at {down.Position} is not above the pivot for a look down.");
    }

    /// <summary>PR-8 exit test 3. A large pitch delta stops at the limit, and the forward vector at the limit reads 80 degrees (D-241).</summary>
    [Fact]
    public void PitchClamps()
    {
        SimulationLoop loop = TestWorld.NewLoop(1UL);
        loop.Step(new Intent(0U, 0, 30000, 0, 0, 0));
        Assert.Equal(SimulationLoop.PitchLimit, loop.Pitch);
        Assert.InRange(loop.Camera().Forward.Y, Sin80 - 1e-5f, Sin80 + 1e-5f);

        loop.Step(new Intent(1U, 0, -30000, 0, 0, 0));
        loop.Step(new Intent(2U, 0, -30000, 0, 0, 0));
        Assert.Equal(-SimulationLoop.PitchLimit, loop.Pitch);
        Assert.InRange(loop.Camera().Forward.Y, -Sin80 - 1e-5f, -Sin80 + 1e-5f);
        Assert.InRange(loop.Camera().Forward.Z, -Cos80 - 1e-5f, -Cos80 + 1e-5f);
    }

    /// <summary>
    /// A wall behind the player pulls the camera in to one camera radius before the wall (D-246). The drawn camera stays
    /// 2.0 behind the shoulder point, inside the wall, because the air past the wall is 3.0 behind it (D-720, F-197).
    /// </summary>
    [Fact]
    public void AWallBehindPullsTheCameraIn()
    {
        VoxelGrid grid = TestWorld.FlatFloor(16, 8);
        for (int y = 1; y < 8; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                grid.Set(x, y, 10, BlockId.RawStone);
            }
        }

        CameraPose pose = OrbitCamera.Place(grid, Feet, 0, 0);
        Assert.InRange(pose.Position.Z, 9.75f - 1e-5f, 9.75f + 1e-5f);
        Assert.Equal(8.9f, pose.Position.X);
        Assert.Equal(3.2f, pose.Position.Y);
        Assert.False(grid.IsSolid(8, 3, 9));

        Assert.Equal(new Vector3(8.9f, 3.2f, 10.0f), pose.View);
        Assert.Equal(OrbitCamera.ClosestView, (pose.View - pose.Shoulder).Length());
    }

    /// <summary>
    /// PR-97: the drawn camera goes into the rock of a wall, and stops one camera radius before the air past it, so it
    /// never enters a second air pocket (D-720). The aim ray still starts at the end of the boom march (D-247).
    /// </summary>
    [Fact]
    public void TheDrawnCameraStopsBeforeASecondAirPocket()
    {
        VoxelGrid grid = TestWorld.FlatFloor(16, 8);
        for (int y = 1; y < 8; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                grid.Set(x, y, 9, BlockId.RawStone);
            }
        }

        // The shoulder point sits at z = 8.85. The wall fills z = 9 to 10, and the air past it starts 1.15 behind the
        // shoulder point, so the drawn camera stops at 0.9 behind it.
        CameraPose pose = OrbitCamera.Place(grid, new Vector3(8.0f, 1.0f, 8.85f), 0, 0);
        Assert.Equal(new Vector3(8.9f, 3.2f, 8.85f), pose.Position);
        Assert.InRange(pose.View.Z, 9.75f - 1e-5f, 9.75f + 1e-5f);
        Assert.Equal(8.9f, pose.View.X);
        Assert.Equal(3.2f, pose.View.Y);
        Assert.True(grid.IsSolid(8, 3, 9));
    }

    /// <summary>PR-97: a look up sends the drawn camera into the floor, 2.0 behind the shoulder point along the boom line (D-720).</summary>
    [Fact]
    public void ALookUpSendsTheDrawnCameraIntoTheFloor()
    {
        VoxelGrid grid = TestWorld.FlatFloor(16, 8);
        CameraPose pose = OrbitCamera.Place(grid, Feet, 0, 8000);

        Vector3 expected = pose.Shoulder - (pose.Forward * OrbitCamera.ClosestView);
        Assert.Equal(expected, pose.View);
        Assert.True(pose.View.Y < 1.0f, $"The drawn camera at {pose.View} is not inside the floor.");
    }

    /// <summary>A player who hugs a right wall gets a shoulder point pulled in along the offset, and never a camera inside rock (D-249).</summary>
    [Fact]
    public void ARightWallPullsTheShoulderIn()
    {
        VoxelGrid grid = TestWorld.FlatFloor(16, 8);
        for (int y = 1; y < 8; y++)
        {
            for (int z = 0; z < 16; z++)
            {
                grid.Set(9, y, z, BlockId.RawStone);
            }
        }

        // The feet at 8.65 put the box face at 8.95, and the shoulder target at 9.55 sits inside the wall.
        Vector3 feet = new(8.65f, 1.0f, 8.0f);
        CameraPose pose = OrbitCamera.Place(grid, feet, 0, 0);

        // The offset march of 0.9 right and 0.7 up meets the wall 0.35 along X, and it keeps 0.25 of room.
        float along = (0.35f / 0.9f * MathF.Sqrt(1.3f)) - OrbitCamera.CameraRadius;
        float fraction = along / MathF.Sqrt(1.3f);
        Assert.InRange(pose.Position.X, 8.65f + (0.9f * fraction) - 1e-3f, 8.65f + (0.9f * fraction) + 1e-3f);
        Assert.InRange(pose.Position.Y, 2.5f + (0.7f * fraction) - 1e-3f, 2.5f + (0.7f * fraction) + 1e-3f);
        Assert.InRange(pose.Position.Z, 11.0f - 1e-4f, 11.0f + 1e-4f);
        Assert.False(grid.IsSolid((int)MathF.Floor(pose.Position.X), (int)MathF.Floor(pose.Position.Y), (int)MathF.Floor(pose.Position.Z)));
    }

    /// <summary>A wall nearer than the camera radius leaves the camera at the start of its march, which is in air.</summary>
    [Fact]
    public void AWallNearerThanTheRadiusLeavesTheCameraAtTheShoulder()
    {
        VoxelGrid grid = TestWorld.FlatFloor(16, 8);
        for (int y = 1; y < 8; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                grid.Set(x, y, 9, BlockId.RawStone);
            }
        }

        // The shoulder point sits at z = 8.85, 0.15 before the wall at 9, which is less than the radius.
        CameraPose pose = OrbitCamera.Place(grid, new Vector3(8.0f, 1.0f, 8.85f), 0, 0);
        Assert.Equal(new Vector3(8.9f, 3.2f, 8.85f), pose.Position);
    }

    /// <summary>A look straight up sends the boom down, and the floor stops it one radius short (D-241, D-246).</summary>
    [Fact]
    public void TheFloorStopsTheBoomOnALookUp()
    {
        VoxelGrid grid = TestWorld.FlatFloor(16, 8);
        CameraPose pose = OrbitCamera.Place(grid, Feet, 0, 8000);

        Assert.InRange(pose.Forward.Y, Sin80 - 1e-5f, Sin80 + 1e-5f);
        Assert.InRange(pose.Position.Y, 1.0f, 1.3f);
        Assert.False(grid.IsSolid((int)MathF.Floor(pose.Position.X), (int)MathF.Floor(pose.Position.Y), (int)MathF.Floor(pose.Position.Z)));
    }

    /// <summary>A look outside the ranges of the loop, or a pivot inside rock, is an error that names the fault (T-2).</summary>
    [Fact]
    public void ABadLookOrPivotIsAnError()
    {
        VoxelGrid grid = TestWorld.FlatFloor(16, 8);

        ContextException yaw = Assert.Throws<ContextException>(() => OrbitCamera.Place(grid, Feet, SimulationLoop.FullTurn, 0));
        Assert.Contains("yaw=36000", yaw.Message, StringComparison.Ordinal);
        Assert.Throws<ContextException>(() => OrbitCamera.Place(grid, Feet, -1, 0));

        ContextException pitch = Assert.Throws<ContextException>(() => OrbitCamera.Place(grid, Feet, 0, SimulationLoop.PitchLimit + 1));
        Assert.Contains("pitch=8001", pitch.Message, StringComparison.Ordinal);
        Assert.Throws<ContextException>(() => OrbitCamera.Place(grid, Feet, 0, -SimulationLoop.PitchLimit - 1));

        ContextException rock = Assert.Throws<ContextException>(() => OrbitCamera.Place(grid, new Vector3(8.0f, -1.0f, 8.0f), 0, 0));
        Assert.Contains("starts inside a solid cell", rock.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// Answers whether a point lies inside the material of the grid. A ramp cell is solid under its slope and
    /// open over it, so the answer reads the slope like the ray of D-246 (D-345, D-367). PR-66 gave the dug
    /// floors their first ramps, and the camera boom stops at a slope and rests over one.
    /// </summary>
    private static bool InsideMaterial(VoxelGrid grid, Vector3 point)
    {
        int x = (int)MathF.Floor(point.X);
        int y = (int)MathF.Floor(point.Y);
        int z = (int)MathF.Floor(point.Z);
        if (grid.TryGetRamp(x, y, z, out Ramp ramp))
        {
            return ramp.HeightOver(x, y, z, point.X, point.Y, point.Z) < 0.0f;
        }

        return grid.IsSolid(x, y, z);
    }

    /// <summary>
    /// PR-8 exit test 1. Over one thousand seeds on dug floors, one fifth on a pull request (D-480), with random
    /// look deltas and movement, the camera position is never inside the material after any tick, and the aim ray
    /// starts at the camera. A failure names its seed (D-66).
    /// </summary>
    [Fact]
    public void CameraNeverInsideSolid()
    {
        int seeds = SweepScope.Seeds(1000);
        for (int seed = 1; seed <= seeds; seed++)
        {
            Random random = new(seed);
            SimulationLoop loop = TestWorld.NewLoop((ulong)seed);
            for (uint tick = 0; tick < 200; tick++)
            {
                loop.Step(SimulationTests.RandomIntent(random, tick));
                CameraPose pose = loop.Camera();
                Assert.False(InsideMaterial(loop.Grid, pose.Position), $"Seed {seed}, tick {tick}: the camera at {pose.Position} is inside a solid block.");
                Assert.Equal(pose.Position, loop.Aim(NoTargets).Origin);
            }
        }
    }

    /// <summary>
    /// Answers whether a cell is a block for the rock march of D-720: solid, and not a ramp. The outside of the grid is a
    /// block (D-237).
    /// </summary>
    private static bool IsBlock(VoxelGrid grid, Vector3 point)
    {
        int x = (int)MathF.Floor(point.X);
        int y = (int)MathF.Floor(point.Y);
        int z = (int)MathF.Floor(point.Z);
        return !grid.TryGetRamp(x, y, z, out _) && grid.IsSolid(x, y, z);
    }

    /// <summary>
    /// PR-97 (D-720). Over one thousand seeds on dug floors, one fifth on a pull request (D-480), with random look deltas
    /// and movement, the drawn camera stands on the boom line, never nearer the shoulder point than the end of the boom
    /// march. It is the end of the boom march, or it stands at most 2.0 behind the shoulder point. Samples every
    /// centimeter from the end of the boom march to the drawn camera meet no open cell after a block. A failure names
    /// its seed (D-66).
    /// </summary>
    [Fact]
    public void DrawnCameraStandsOnTheBoomLine()
    {
        int seeds = SweepScope.Seeds(1000);
        for (int seed = 1; seed <= seeds; seed++)
        {
            Random random = new(seed);
            SimulationLoop loop = TestWorld.NewLoop((ulong)seed);
            for (uint tick = 0; tick < 200; tick++)
            {
                loop.Step(SimulationTests.RandomIntent(random, tick));
                CameraPose pose = loop.Camera();
                string where = $"Seed {seed}, tick {tick}: the drawn camera at {pose.View}, with the boom end at {pose.Position}";
                float boom = (pose.Position - pose.Shoulder).Length();
                float drawn = (pose.View - pose.Shoulder).Length();
                Vector3 expected = pose.Shoulder - (pose.Forward * drawn);
                Assert.True((pose.View - expected).Length() < 1e-4f, $"{where}, is off the boom line.");
                Assert.True(drawn >= boom - 1e-4f, $"{where}, is nearer the shoulder point than the boom end.");
                Assert.True(pose.View == pose.Position || drawn <= OrbitCamera.ClosestView + 1e-4f, $"{where}, is farther than the closest view.");

                bool metBlock = false;
                int samples = (int)MathF.Ceiling((drawn - boom) * 100.0f);
                for (int sample = 0; sample <= samples; sample++)
                {
                    float distance = boom + ((drawn - boom) * sample / Math.Max(samples, 1));
                    bool block = IsBlock(loop.Grid, pose.Shoulder - (pose.Forward * distance));
                    Assert.False(metBlock && !block, $"{where}, passes into an open cell after the rock at {distance} behind the shoulder point.");
                    metBlock |= block;
                }
            }
        }
    }

    /// <summary>
    /// PR-8 exit test 2. Over one hundred seeds on dug floors, two live runs of one record give one hash of
    /// every camera pose and aim ray, the replay folds the same hash tick by tick through its observer, and the
    /// replay ends at the same pose and ray. A failure names its seed (D-66).
    /// </summary>
    [Fact]
    public void AimRayIsDeterministic()
    {
        for (int seed = 1; seed <= 100; seed++)
        {
            Random random = new(seed);
            SimulationLoop live = TestWorld.NewLoop((ulong)seed);
            SimulationLoop twin = TestWorld.NewLoop((ulong)seed);
            Vector3 spawn = live.Plan.Spawn;
            Vector3[] targets =
            [
                spawn + new Vector3((float)(random.NextDouble() * 10.0) - 5.0f, (float)(random.NextDouble() * 3.0), (float)(random.NextDouble() * 10.0) - 5.0f),
                spawn + new Vector3((float)(random.NextDouble() * 10.0) - 5.0f, (float)(random.NextDouble() * 3.0), (float)(random.NextDouble() * 10.0) - 5.0f),
                spawn + new Vector3((float)(random.NextDouble() * 10.0) - 5.0f, (float)(random.NextDouble() * 3.0), (float)(random.NextDouble() * 10.0) - 5.0f),
            ];

            MemorySink sink = new();
            RunRecorder recorder = new(sink, RunRecord.NewHeader(TestWorld.PeacefulContent.Hash, (ulong)seed));
            StateHash liveHash = StateHash.Start();
            StateHash twinHash = StateHash.Start();
            for (uint tick = 0; tick < 200; tick++)
            {
                Intent intent = SimulationTests.RandomIntent(random, tick);
                recorder.Record(intent);
                live.Step(intent);
                twin.Step(intent);
                AddPose(ref liveHash, live.Camera(), live.Aim(targets));
                AddPose(ref twinHash, twin.Camera(), twin.Aim(targets));
            }

            Assert.True(liveHash.Value == twinHash.Value, $"Seed {seed}: two live runs give the aim-ray hashes {liveHash} and {twinHash}.");

            ReplayCameraFold replayed = new(targets);
            ReplayResult replay = RunReplayer.Replay(sink.Bytes, TestWorld.PeacefulContent, new JsonlLogger(new CollectingSink()), replayed);
            Assert.True(liveHash.Value == replayed.Hash.Value, $"Seed {seed}: the live aim-ray hash is {liveHash}, and the replay folds {replayed.Hash}.");
            Assert.True(live.Camera() == replay.Loop.Camera(), $"Seed {seed}: the live camera is {live.Camera()}, and the replay camera is {replay.Loop.Camera()}.");
            Assert.True(live.Aim(targets) == replay.Loop.Aim(targets), $"Seed {seed}: the live aim ray is {live.Aim(targets)}, and the replay aim ray is {replay.Loop.Aim(targets)}.");
        }
    }

    /// <summary>
    /// PR-8 exit test 4. With the controller aim flag set, a target three degrees off the ray pulls the ray to
    /// one and a half degrees. No target, a target outside the cone, or a clear flag leaves the ray as it is (D-244).
    /// </summary>
    [Fact]
    public void AssistPullsTowardTarget()
    {
        AimRay raw = new(new Vector3(0.0f, 2.0f, 0.0f), new Vector3(0.0f, 0.0f, -1.0f));
        Vector3 target = TargetAt(3.0);
        Vector3 toTarget = target - raw.Origin;

        AimRay pulled = AimAssist.Apply(raw, [target], controllerAim: true);
        Assert.Equal(raw.Origin, pulled.Origin);
        Assert.InRange(DegreesBetween(pulled.Direction, toTarget), 1.5 - 0.01, 1.5 + 0.01);
        Assert.InRange(DegreesBetween(pulled.Direction, raw.Direction), 1.5 - 0.01, 1.5 + 0.01);
        Assert.True(DegreesBetween(pulled.Direction, toTarget) < DegreesBetween(raw.Direction, toTarget));
        Assert.InRange(pulled.Direction.Length(), 1.0f - 1e-5f, 1.0f + 1e-5f);

        Assert.Equal(raw, AimAssist.Apply(raw, NoTargets, controllerAim: true));
        Assert.Equal(raw, AimAssist.Apply(raw, [target], controllerAim: false));
        Assert.Equal(raw, AimAssist.Apply(raw, [TargetAt(10.0)], controllerAim: true));
        Assert.Equal(raw, AimAssist.Apply(raw, [TargetAt(5.5)], controllerAim: true));
    }

    /// <summary>The target with the smallest angle wins, a target behind the ray never pulls, and a target on the ray or at the origin changes nothing.</summary>
    [Fact]
    public void TheNearestTargetWins()
    {
        AimRay raw = new(new Vector3(0.0f, 2.0f, 0.0f), new Vector3(0.0f, 0.0f, -1.0f));
        Vector3 nearer = TargetAt(3.0);
        Vector3 farther = TargetAt(-4.0);

        AimRay pulled = AimAssist.Apply(raw, [farther, nearer], controllerAim: true);
        Assert.True(pulled.Direction.X > 0.0f, $"The pull went to {pulled.Direction}, away from the nearer target on plus X.");
        Assert.Equal(pulled, AimAssist.Apply(raw, [nearer, farther], controllerAim: true));

        Assert.Equal(raw, AimAssist.Apply(raw, [new Vector3(0.0f, 2.0f, 10.0f)], controllerAim: true));
        Assert.Equal(raw, AimAssist.Apply(raw, [new Vector3(0.0f, 2.0f, -10.0f)], controllerAim: true));
        Assert.Equal(raw, AimAssist.Apply(raw, [raw.Origin], controllerAim: true));
        Assert.Equal(raw, AimAssist.Apply(raw, [raw.Origin, TargetAt(10.0)], controllerAim: true));
    }

    /// <summary>The loop reads the controller aim bit of the last intent, so the same state aims two ways with and without it (D-243).</summary>
    [Fact]
    public void TheLoopAimReadsTheLastIntent()
    {
        SimulationLoop controller = TestWorld.NewLoop(1UL);
        SimulationLoop mouse = TestWorld.NewLoop(1UL);
        controller.Step(new Intent(0U, 0, 0, 0, 0, Button.ControllerAim));
        mouse.Step(new Intent(0U, 0, 0, 0, 0, 0));

        // The camera looks along minus Z at yaw zero. A target three degrees to the right sits ten meters ahead of it.
        Vector3 camera = controller.Camera().Position;
        Vector3[] targets = [camera + (TargetAt(3.0) - new Vector3(0.0f, 2.0f, 0.0f))];

        Assert.Equal(controller.Camera(), mouse.Camera());
        Assert.Equal(mouse.Camera().Forward, mouse.Aim(targets).Direction);
        Assert.NotEqual(controller.Camera().Forward, controller.Aim(targets).Direction);
        Assert.InRange(DegreesBetween(controller.Aim(targets).Direction, controller.Camera().Forward), 1.5 - 0.01, 1.5 + 0.01);
    }
}
