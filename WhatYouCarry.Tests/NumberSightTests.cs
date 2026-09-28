using Godot;
using WhatYouCarry.Core.World;
using WhatYouCarry.Game.Ui;
using Xunit;
using CoreVector3 = WhatYouCarry.Core.Physics.Vector3;

namespace WhatYouCarry.Tests;

/// <summary>The rules that hide the damage number of an enemy that the camera cannot see (PR-98, D-727, D-729).</summary>
public sealed class NumberSightTests
{
    /// <summary>A sixteen by eight by sixteen room over a stone floor, with walls across it at x = 6 and at x = 10.</summary>
    private static VoxelGrid TwoWalls()
    {
        VoxelGrid grid = TestWorld.FlatFloor(16, 8);
        for (int y = 1; y < 8; y++)
        {
            for (int z = 0; z < 16; z++)
            {
                grid.Set(6, y, z, BlockId.RawStone);
                grid.Set(10, y, z, BlockId.RawStone);
            }
        }

        return grid;
    }

    /// <summary>PR-98 exit test 1. A wall between the drawn camera in air and the owner hides the number, and open air shows it.</summary>
    [Fact]
    public void AWallHidesTheOwner()
    {
        VoxelGrid grid = TwoWalls();
        CoreVector3 camera = new(3.5f, 2.5f, 4.5f);

        Assert.True(NumberSight.WallHides(grid, camera, new CoreVector3(8.5f, 2.5f, 4.5f)));
        Assert.False(NumberSight.WallHides(grid, camera, new CoreVector3(5.5f, 2.5f, 7.5f)));
    }

    /// <summary>
    /// PR-98 exit test 1. The rock that holds the drawn camera does not count, because the wall fade clears it
    /// (D-729). The next wall after the first open cell still hides the owner, and a segment that never leaves the
    /// rock hides it too.
    /// </summary>
    [Fact]
    public void TheRockOfTheCameraDoesNotHide()
    {
        VoxelGrid grid = TwoWalls();
        CoreVector3 camera = new(6.5f, 2.5f, 4.5f);

        Assert.False(NumberSight.WallHides(grid, camera, new CoreVector3(8.5f, 2.5f, 4.5f)));
        Assert.False(NumberSight.WallHides(grid, camera, new CoreVector3(3.5f, 2.5f, 6.5f)));
        Assert.True(NumberSight.WallHides(grid, camera, new CoreVector3(12.5f, 2.5f, 4.5f)));
        Assert.True(NumberSight.WallHides(grid, camera, new CoreVector3(6.9f, 2.5f, 5.5f)));
    }

    /// <summary>
    /// A segment that enters the first open cell near an edge leaves it again within <see cref="NumberSight.PastFace"/>,
    /// so the step past the face lands in the next block. The segment meets rock there, and the owner hides. The march
    /// from a start in rock threw before this rule.
    /// </summary>
    [Fact]
    public void AStepPastAnEdgeIntoRockHides()
    {
        VoxelGrid grid = TestWorld.FlatFloor(16, 8);
        for (int x = 2; x <= 6; x++)
        {
            for (int y = 1; y <= 5; y++)
            {
                for (int z = 2; z <= 9; z++)
                {
                    grid.Set(x, y, z, BlockId.RawStone);
                }
            }
        }

        grid.Set(5, 3, 5, BlockId.Air);
        CoreVector3 camera = new(4.5f, 3.5f, 5.5f);
        CoreVector3 target = new(7.5f, 6.4985f, 5.5f);

        // The segment enters the open cell at x = 5, y = 3.99975, and it crosses y = 4 into rock 0.35 mm later.
        Assert.True(NumberSight.WallHides(grid, camera, target));
    }

    /// <summary>A drawn camera under the slope of a ramp stands in a solid part, so no open cell shows the owner.</summary>
    [Fact]
    public void ACameraUnderASlopeSeesNoOwner()
    {
        VoxelGrid grid = TestWorld.FlatFloor(16, 8);
        grid.Set(4, 1, 4, new Ramp(RampRise.PlusX, 2, 1).Id);
        CoreVector3 camera = new(4.9f, 1.1f, 4.5f);
        Assert.True(grid.TryGetRamp(4, 1, 4, out Ramp ramp));
        Assert.True(ramp.HeightOver(4, 1, 4, camera.X, camera.Y, camera.Z) < 0.0f, "The fixture camera stands under the slope.");

        Assert.True(NumberSight.WallHides(grid, camera, new CoreVector3(8.5f, 1.5f, 4.5f)));
        Assert.False(NumberSight.WallHides(grid, new CoreVector3(4.1f, 1.9f, 4.5f), new CoreVector3(1.5f, 1.9f, 4.5f)));
    }

    /// <summary>PR-98 exit test 1. The body hides an owner whose center falls inside the body box on the screen and stands farther away (D-727).</summary>
    [Fact]
    public void TheBodyHidesAnOwnerBehindIt()
    {
        Rect2 body = new(300.0f, 400.0f, 250.0f, 400.0f);

        Assert.True(NumberSight.BodyHides(new Vector2(420.0f, 500.0f), body, 6.0f, 2.3f));
        Assert.False(NumberSight.BodyHides(new Vector2(420.0f, 500.0f), body, 1.5f, 2.3f));
        Assert.False(NumberSight.BodyHides(new Vector2(700.0f, 500.0f), body, 6.0f, 2.3f));
    }
}
