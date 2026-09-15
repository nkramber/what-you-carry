using System;
using System.Collections.Generic;
using WhatYouCarry.Core.Camera;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Entities;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.Projectiles;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Core.World;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The ray march, the camera boom, and the shots against the slope of a ramp (D-246, D-345, D-367; PR-64 exit test 4).</summary>
public sealed class RampRayTests
{
    /// <summary>Every rise with every run of D-346.</summary>
    public static TheoryData<RampRise, int> Courses => RampCourse.EveryRiseAndRun();

    /// <summary>A segment straight down over each place of the ramp hits the slope at its height under the segment.</summary>
    [Theory]
    [MemberData(nameof(Courses))]
    public void ARayDownHitsTheSlope(RampRise rise, int run)
    {
        RampCourse course = new(rise, run);
        for (int place = 0; place < run; place++)
        {
            float along = RampCourse.RampStart + place + 0.25f;
            RayHit hit = GridRay.FirstSolid(course.Grid, course.Point(along, 5.0f, RampCourse.Middle), course.Point(along, 0.5f, RampCourse.Middle));
            float slope = 1.0f + ((place + 0.25f) / run);
            Assert.True(hit.Hit, $"Rise {rise}, run {run}, place {place}: the segment missed the slope.");
            Assert.InRange(hit.Distance, (5.0f - slope) - 1e-4f, (5.0f - slope) + 1e-4f);
        }
    }

    /// <summary>A level segment up the rise meets the slope where the slope reaches its height, and not at the face of a cell.</summary>
    [Theory]
    [MemberData(nameof(Courses))]
    public void ARayAlongTheRiseMeetsTheSlope(RampRise rise, int run)
    {
        RampCourse course = new(rise, run);
        RayHit hit = GridRay.FirstSolid(course.Grid, course.Point(3.5f, 1.3f, RampCourse.Middle), course.Point(13.5f, 1.3f, RampCourse.Middle));
        float expected = (RampCourse.RampStart + (0.3f * run)) - 3.5f;
        Assert.True(hit.Hit);
        Assert.InRange(hit.Distance, expected - 1e-4f, expected + 1e-4f);
    }

    /// <summary>A segment half a meter over the slope, parallel to it, passes the whole ramp with no hit.</summary>
    [Theory]
    [MemberData(nameof(Courses))]
    public void ARayOverTheSlopeHasNoHit(RampRise rise, int run)
    {
        RampCourse course = new(rise, run);
        Vector3 start = course.Point(RampCourse.RampStart, 1.5f, RampCourse.Middle);
        Vector3 end = course.Point(course.HighStart, 2.5f, RampCourse.Middle);
        RayHit hit = GridRay.FirstSolid(course.Grid, start, end);
        Assert.False(hit.Hit);
        Assert.Equal((end - start).Length(), hit.Distance);
    }

    /// <summary>A start under the slope is inside a solid part, and the march names it (T-2). A start over the slope in the same cell is in air.</summary>
    [Fact]
    public void ARayUnderASlopeIsAnError()
    {
        RampCourse course = new(RampRise.PlusZ, 2);
        Vector3 under = course.Point(RampCourse.RampStart + 0.5f, 1.2f, RampCourse.Middle);
        ContextException error = Assert.Throws<ContextException>(() => GridRay.FirstSolid(course.Grid, under, under + new Vector3(0.0f, 3.0f, 0.0f)));
        Assert.Contains("starts inside a solid cell", error.Message, StringComparison.Ordinal);

        Vector3 over = course.Point(RampCourse.RampStart + 0.5f, 1.3f, RampCourse.Middle);
        Assert.False(GridRay.FirstSolid(course.Grid, over, over + new Vector3(0.0f, 3.0f, 0.0f)).Hit);
    }

    /// <summary>
    /// Over one thousand random grids of blocks and ramps, the march agrees with a walk along the segment in
    /// one-millimeter steps that reads each slope. A failure names its seed (D-66).
    /// </summary>
    [Fact]
    public void TheMarchAgreesWithAFineWalkOverRamps()
    {
        for (int seed = 1; seed <= 1000; seed++)
        {
            Random random = new(seed);
            VoxelGrid grid = RampMotionTests.RandomRampWorld(random);
            Vector3 start = new(0.0f, 0.0f, 0.0f);
            bool placed = false;
            for (int attempt = 0; attempt < 1000 && !placed; attempt++)
            {
                start = new Vector3(0.05f + ((float)random.NextDouble() * 9.9f), 1.0f + ((float)random.NextDouble() * 6.9f), 0.05f + ((float)random.NextDouble() * 9.9f));
                placed = !RampCourse.IsSolidAt(grid, start);
            }

            Assert.True(placed, $"Seed {seed}: no start in air, and the test setup is wrong.");
            Vector3 end = new((float)(random.NextDouble() * 14.0) - 2.0f, (float)(random.NextDouble() * 12.0) - 2.0f, (float)(random.NextDouble() * 14.0) - 2.0f);
            RayHit hit = GridRay.FirstSolid(grid, start, end);
            Vector3 delta = end - start;
            float length = delta.Length();

            float firstSolid = -1.0f;
            for (float distance = 0.0f; distance <= length; distance += 0.001f)
            {
                if (RampCourse.IsSolidAt(grid, start + (delta * (distance / length))))
                {
                    firstSolid = distance;
                    break;
                }
            }

            if (hit.Hit)
            {
                Assert.True(firstSolid < 0.0f || hit.Distance <= firstSolid + 1e-3f, $"Seed {seed}: the march hit at {hit.Distance}, and the walk found a solid part at {firstSolid}.");
                Assert.True(firstSolid < 0.0f || firstSolid >= hit.Distance - 1e-3f, $"Seed {seed}: the walk found a solid part at {firstSolid}, before the march hit at {hit.Distance}.");
            }
            else
            {
                Assert.True(firstSolid < 0.0f, $"Seed {seed}: the march found no solid part, and the walk found one at {firstSolid}.");
            }
        }
    }

    /// <summary>
    /// PR-64 exit test 4, the camera. On every course, a body at rest at a random place and a random look never put
    /// the camera or the shoulder point under a slope or inside a block. A boom that meets a ramp stops at its slope,
    /// with the camera radius before the point where the boom meets it. A failure names its seed (D-66).
    /// </summary>
    [Fact]
    public void CameraNeverEntersARamp()
    {
        int boomsOnASlope = 0;
        foreach (object[] data in Courses)
        {
            RampCourse course = new((RampRise)data[0], (int)data[1]);
            for (int seed = 1; seed <= 30; seed++)
            {
                Random random = new(seed);
                PlayerBody body = new(course.Grid, course.Point(1.0f + ((float)random.NextDouble() * 14.0f), RampCourse.HighTop + 0.05f, 1.0f + ((float)random.NextDouble() * 7.0f)));
                for (int tick = 0; tick < 60; tick++)
                {
                    body.Move(new Vector3(0.0f, 0.0f, 0.0f), false, false, true);
                }

                for (int look = 0; look < 20; look++)
                {
                    int yaw = random.Next(SimulationLoop.FullTurn);
                    int pitch = random.Next(-SimulationLoop.PitchLimit, SimulationLoop.PitchLimit + 1);
                    CameraPose pose = OrbitCamera.Place(course.Grid, body.Position, yaw, pitch);
                    string context = $"Rise {course.Rise}, run {course.Run}, seed {seed}, look {look}";
                    Assert.False(RampCourse.IsSolidAt(course.Grid, pose.Position), $"{context}: the camera at {pose.Position} is inside a solid part.");
                    Assert.False(RampCourse.IsSolidAt(course.Grid, pose.Shoulder), $"{context}: the shoulder at {pose.Shoulder} is inside a solid part.");

                    Vector3 boomEnd = pose.Shoulder - (pose.Forward * OrbitCamera.BoomLength);
                    RayHit hit = GridRay.FirstSolid(course.Grid, pose.Shoulder, boomEnd);
                    if (!hit.Hit || hit.Distance <= OrbitCamera.CameraRadius)
                    {
                        continue;
                    }

                    // The camera stands the camera radius before the point where the boom meets a solid part: a block,
                    // the side of a ramp, or a slope. The boom passes air up to that point.
                    Vector3 direction = (boomEnd - pose.Shoulder) * (1.0f / OrbitCamera.BoomLength);
                    Vector3 expected = pose.Shoulder + (direction * (hit.Distance - OrbitCamera.CameraRadius));
                    Assert.InRange((pose.Position - expected).Length(), 0.0f, 1e-3f);
                    Assert.False(RampCourse.IsSolidAt(course.Grid, pose.Shoulder + (direction * (hit.Distance - 1e-3f))), $"{context}: the boom passed a solid part before its hit at {hit.Distance}.");

                    // A boom that meets a slope inside a ramp cell counts, so the test proves that it reads slopes.
                    Vector3 meet = pose.Shoulder + (direction * hit.Distance);
                    Vector3 past = pose.Shoulder + (direction * (hit.Distance + 1e-3f));
                    int x = (int)MathF.Floor(past.X);
                    int y = (int)MathF.Floor(past.Y);
                    int z = (int)MathF.Floor(past.Z);
                    if (course.Grid.TryGetRamp(x, y, z, out Ramp ramp) && MathF.Abs(ramp.HeightOver(x, y, z, meet.X, meet.Y, meet.Z)) <= 1e-3f)
                    {
                        boomsOnASlope++;
                    }
                }
            }
        }

        Assert.True(boomsOnASlope > 0, "No boom of the test met a slope, so the test checks nothing about the slope.");
    }

    /// <summary>PR-64 exit test 4, the shots. A shot straight down stops on the slope, and a level shot up the rise stops where the slope reaches its height.</summary>
    [Theory]
    [MemberData(nameof(Courses))]
    public void ProjectileHitsARampSlope(RampRise rise, int run)
    {
        RampCourse course = new(rise, run);
        ProjectileDefinition straight = new("test-straight", 4000, 0, 120, 1, 0, 0);
        ProjectileSimulation simulation = new(course.Grid, [straight]);
        Rng rng = Rng.ForStream(1UL, RngStream.Projectile);
        float along = RampCourse.RampStart + (run - 1) + 0.5f;
        simulation.Fire(0, -1, course.Point(along, 4.0f, RampCourse.Middle), new Vector3(0.0f, -1.0f, 0.0f), rng);
        simulation.Fire(0, -1, course.Point(3.5f, 1.3f, RampCourse.Middle), course.Uphill, rng);

        List<ProjectileEnd> ends = [];
        for (int tick = 0; tick < 120 && simulation.Live.Count > 0; tick++)
        {
            ends.AddRange(simulation.Step([]));
        }

        Assert.Equal(2, ends.Count);
        ProjectileEnd down = ends[0].Point.Y > 1.4f ? ends[0] : ends[1];
        ProjectileEnd level = ends[0].Point.Y > 1.4f ? ends[1] : ends[0];
        Assert.Equal(ProjectileEndKind.Grid, down.Kind);
        Assert.Equal(ProjectileEndKind.Grid, level.Kind);
        Assert.InRange(down.Point.Y, 1.0f + ((run - 0.5f) / run) - 1e-3f, 1.0f + ((run - 0.5f) / run) + 1e-3f);
        Assert.InRange(course.AlongOf(level.Point), RampCourse.RampStart + (0.3f * run) - 1e-3f, RampCourse.RampStart + (0.3f * run) + 1e-3f);
    }
}
