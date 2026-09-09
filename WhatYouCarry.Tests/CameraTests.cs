using System;
using System.Collections.Generic;
using WhatYouCarry.Core.Camera;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.Replay;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Core.World;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The orbit camera and aim assist (D-13, D-14, D-75, D-77, D-88, D-241 to D-249; PR-8 exit tests 1 to 4).</summary>
public sealed class CameraTests
{
    private const string Hash = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";
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

    /// <summary>The constants of D-241, D-242, D-244, and D-246 hold.</summary>
    [Fact]
    public void TheConstantsHold()
    {
        Assert.Equal(8000, SimulationLoop.PitchLimit);
        Assert.Equal(1.5f, OrbitCamera.PivotHeight);
        Assert.Equal(0.6f, OrbitCamera.ShoulderRight);
        Assert.Equal(0.3f, OrbitCamera.ShoulderUp);
        Assert.Equal(3.0f, OrbitCamera.BoomLength);
        Assert.Equal(0.25f, OrbitCamera.CameraRadius);
        Assert.Equal(5.0f, AimAssist.ConeDegrees);
        Assert.Equal(0.5f, AimAssist.Strength);
    }

    /// <summary>At rest the camera sits behind the right shoulder: 0.6 right, 0.3 up, and 3 back from the pivot 1.5 over the feet (D-242).</summary>
    [Fact]
    public void TheCameraSitsBehindTheRightShoulder()
    {
        CameraPose pose = OrbitCamera.Place(TestWorld.FlatFloor(16, 8), Feet, 0, 0);

        Assert.Equal(new Vector3(8.6f, 2.8f, 11.0f), pose.Position);
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
        Assert.InRange(pose.Position.Y, 2.8f - 1e-4f, 2.8f + 1e-4f);
        Assert.InRange(pose.Position.Z, 7.4f - 1e-4f, 7.4f + 1e-4f);
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
        SimulationLoop loop = new(1UL, TestWorld.FlatFloor(16, 16), new Vector3(8.0f, 6.0f, 8.0f));
        loop.Step(new Intent(0U, 0, 30000, 0, 0, 0));
        Assert.Equal(SimulationLoop.PitchLimit, loop.Pitch);
        Assert.InRange(loop.Camera().Forward.Y, Sin80 - 1e-5f, Sin80 + 1e-5f);

        loop.Step(new Intent(1U, 0, -30000, 0, 0, 0));
        loop.Step(new Intent(2U, 0, -30000, 0, 0, 0));
        Assert.Equal(-SimulationLoop.PitchLimit, loop.Pitch);
        Assert.InRange(loop.Camera().Forward.Y, -Sin80 - 1e-5f, -Sin80 + 1e-5f);
        Assert.InRange(loop.Camera().Forward.Z, -Cos80 - 1e-5f, -Cos80 + 1e-5f);
    }

    /// <summary>A wall behind the player pulls the camera in to one camera radius before the wall (D-246).</summary>
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
        Assert.Equal(8.6f, pose.Position.X);
        Assert.Equal(2.8f, pose.Position.Y);
        Assert.False(grid.IsSolid(8, 2, 9));
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

        // The feet at 8.65 put the box face at 8.95, and the shoulder target at 9.25 sits inside the wall.
        Vector3 feet = new(8.65f, 1.0f, 8.0f);
        SimulationLoop loop = new(1UL, grid, feet);
        CameraPose pose = loop.Camera();

        // The offset march of 0.6 right and 0.3 up meets the wall 0.35 along X, and it keeps 0.25 of room.
        float along = (0.35f / 0.6f * MathF.Sqrt(0.45f)) - OrbitCamera.CameraRadius;
        float fraction = along / MathF.Sqrt(0.45f);
        Assert.InRange(pose.Position.X, 8.65f + (0.6f * fraction) - 1e-3f, 8.65f + (0.6f * fraction) + 1e-3f);
        Assert.InRange(pose.Position.Y, 2.5f + (0.3f * fraction) - 1e-3f, 2.5f + (0.3f * fraction) + 1e-3f);
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
        Assert.Equal(new Vector3(8.6f, 2.8f, 8.85f), pose.Position);
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
    /// PR-8 exit test 1. Over one thousand seeds in random grids, with random look deltas and movement, the
    /// camera position is never inside a solid block after any tick, and the aim ray starts at the camera.
    /// A failure names its seed (D-66).
    /// </summary>
    [Fact]
    public void CameraNeverInsideSolid()
    {
        for (int seed = 1; seed <= 1000; seed++)
        {
            Random random = new(seed);
            (VoxelGrid grid, Vector3 spawn) = TestWorld.RandomWorld(random);
            SimulationLoop loop = new((ulong)seed, grid, spawn);
            for (uint tick = 0; tick < 200; tick++)
            {
                loop.Step(SimulationTests.RandomIntent(random, tick));
                CameraPose pose = loop.Camera();
                bool inside = grid.IsSolid((int)MathF.Floor(pose.Position.X), (int)MathF.Floor(pose.Position.Y), (int)MathF.Floor(pose.Position.Z));
                Assert.False(inside, $"Seed {seed}, tick {tick}: the camera at {pose.Position} is inside a solid block.");
                Assert.Equal(pose.Position, loop.Aim(NoTargets).Origin);
            }
        }
    }

    /// <summary>
    /// PR-8 exit test 2. Over one hundred seeds in random grids, two live runs of one record give one hash of
    /// every camera pose and aim ray, and the replay ends at the same pose and ray. A failure names its seed (D-66).
    /// </summary>
    [Fact]
    public void AimRayIsDeterministic()
    {
        for (int seed = 1; seed <= 100; seed++)
        {
            Random random = new(seed);
            (VoxelGrid grid, Vector3 spawn) = TestWorld.RandomWorld(random);
            Vector3[] targets =
            [
                new((float)(random.NextDouble() * 10.0), (float)(random.NextDouble() * 6.0), (float)(random.NextDouble() * 10.0)),
                new((float)(random.NextDouble() * 10.0), (float)(random.NextDouble() * 6.0), (float)(random.NextDouble() * 10.0)),
                new((float)(random.NextDouble() * 10.0), (float)(random.NextDouble() * 6.0), (float)(random.NextDouble() * 10.0)),
            ];

            MemorySink sink = new();
            RunRecorder recorder = new(sink, RunRecord.NewHeader(Hash, (ulong)seed));
            SimulationLoop live = new((ulong)seed, grid, spawn);
            SimulationLoop twin = new((ulong)seed, grid, spawn);
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

            ReplayResult replay = RunReplayer.Replay(sink.Bytes, Hash, grid, spawn, new JsonlLogger(new CollectingSink()));
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
        VoxelGrid grid = TestWorld.FlatFloor(16, 8);
        SimulationLoop controller = new(1UL, grid, Feet);
        SimulationLoop mouse = new(1UL, grid, Feet);
        controller.Step(new Intent(0U, 0, 0, 0, 0, Button.ControllerAim));
        mouse.Step(new Intent(0U, 0, 0, 0, 0, 0));

        // The camera looks along minus Z from (8.6, 2.8, 11). A target three degrees to the right sits ahead of it.
        Vector3 camera = controller.Camera().Position;
        Vector3[] targets = [camera + (TargetAt(3.0) - new Vector3(0.0f, 2.0f, 0.0f))];

        Assert.Equal(controller.Camera(), mouse.Camera());
        Assert.Equal(mouse.Camera().Forward, mouse.Aim(targets).Direction);
        Assert.NotEqual(controller.Camera().Forward, controller.Aim(targets).Direction);
        Assert.InRange(DegreesBetween(controller.Aim(targets).Direction, controller.Camera().Forward), 1.5 - 0.01, 1.5 + 0.01);
    }
}
