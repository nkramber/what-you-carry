using System.Collections.Generic;
using WhatYouCarry.Core.Pathfinding;
using WhatYouCarry.Core.Procgen;
using WhatYouCarry.Core.World;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The reachability search on ramps: the move rule of D-165 as D-345 revises it (PR-64 exit test 5).</summary>
public sealed class RampSearchTests
{
    /// <summary>Every rise with every run of D-346.</summary>
    public static TheoryData<RampRise, int> Courses => RampCourse.EveryRiseAndRun();

    /// <summary>
    /// PR-64 exit test 5. The search joins the low floor and the high floor of a ramp in both directions, with one
    /// move per cell along the rise, and a shortest path walks every place of the ramp in order.
    /// </summary>
    [Theory]
    [MemberData(nameof(Courses))]
    public void TheSearchWalksARamp(RampRise rise, int run)
    {
        RampCourse course = new(rise, run);
        Cell low = course.CellAt(3, 0, 4);
        Cell high = course.CellAt(12, 1, 4);

        Reachability up = Reachability.From(course.Grid, low);
        Assert.Equal(9, up.Distance(high));
        IReadOnlyList<Cell> path = up.PathTo(high);
        for (int step = 0; step < path.Count; step++)
        {
            int along = 3 + step;
            Assert.Equal(course.CellAt(along, along < RampCourse.RampStart ? 0 : 1, 4), path[step]);
        }

        Reachability down = Reachability.From(course.Grid, high);
        Assert.Equal(9, down.Distance(low));
        for (int place = 0; place < run; place++)
        {
            Cell ramp = course.CellAt(RampCourse.RampStart + place, 1, 4);
            Assert.True(GridMoves.IsFloor(course.Grid, ramp));
            Assert.Equal(RampCourse.RampStart + place - 3, up.Distance(ramp));
            Assert.Equal(12 - RampCourse.RampStart - place, down.Distance(ramp));
        }
    }

    /// <summary>
    /// The search follows the body rule beside a ramp one cell wide. The side of a ramp is a step up with a jump. A body
    /// on a ramp cannot reach a block two rows over the floor, because the slope stands under the top of its cell. A
    /// body drops off the side of a ramp. A ceiling three cells over the floor before a low end stops the walk, because
    /// a body over both columns rises into that cell.
    /// </summary>
    [Fact]
    public void TheSearchFollowsTheBodyRuleOnRamps()
    {
        VoxelGrid grid = new(10, 8, 5);
        for (int x = 0; x < 10; x++)
        {
            for (int z = 0; z < 5; z++)
            {
                grid.Set(x, 0, z, BlockId.RawStone);
            }
        }

        // A ramp of 1:2 along +X at x = 4 and 5 on z = 2, the high floor at x = 6 and 7 on z = 2, and a column two
        // blocks high at (4, 1, 3) and (4, 2, 3) beside the low place.
        grid.Set(4, 1, 2, new Ramp(RampRise.PlusX, 2, 0).Id);
        grid.Set(5, 1, 2, new Ramp(RampRise.PlusX, 2, 1).Id);
        grid.Set(6, 1, 2, BlockId.RawStone);
        grid.Set(7, 1, 2, BlockId.RawStone);
        grid.Set(4, 1, 3, BlockId.RawStone);
        grid.Set(4, 2, 3, BlockId.RawStone);

        Reachability reach = Reachability.From(grid, new Cell(1, 0, 2));
        Assert.Equal(3, reach.Distance(new Cell(4, 1, 2)));
        Assert.Equal(4, reach.Distance(new Cell(5, 1, 2)));
        Assert.Equal(5, reach.Distance(new Cell(6, 1, 2)));

        // The walks of the ramp in each direction, and the step onto its side.
        Assert.Equal(1, GridMoves.RampWalk(grid, 3, 0, 2, 4, 2));
        Assert.Equal(0, GridMoves.RampWalk(grid, 4, 1, 2, 3, 2));
        Assert.Equal(1, GridMoves.RampWalk(grid, 5, 1, 2, 6, 2));
        Assert.Equal(1, GridMoves.RampWalk(grid, 6, 1, 2, 5, 2));
        Assert.Equal(-1, GridMoves.RampWalk(grid, 4, 0, 1, 4, 2));
        Assert.Equal(1, GridMoves.Landing(grid, 4, 0, 1, 4, 2));

        // The column top is two blocks over the floor and more than one block over the slope, so nothing reaches it.
        Assert.Equal(-1, GridMoves.Landing(grid, 4, 1, 2, 4, 3));
        Assert.False(reach.IsReachable(new Cell(4, 2, 3)));

        // A drop off the side of the high place lands on the floor.
        Assert.Equal(0, GridMoves.Landing(grid, 5, 1, 2, 5, 3));

        // A ceiling at row 3 over the floor before the low end stops the walk both ways, and the jump has no room.
        grid.Set(3, 3, 2, BlockId.RawStone);
        Assert.Equal(-1, GridMoves.RampWalk(grid, 3, 0, 2, 4, 2));
        Assert.Equal(-1, GridMoves.RampWalk(grid, 4, 1, 2, 3, 2));
        Assert.Equal(-1, GridMoves.Landing(grid, 3, 0, 2, 4, 2));
    }

    /// <summary>The top of one ramp joins the low end of a ramp one row up, in both directions, when the lower cell has a third open cell over it.</summary>
    [Fact]
    public void TheSearchWalksAChainOfRamps()
    {
        VoxelGrid grid = new(10, 8, 3);
        for (int x = 0; x < 10; x++)
        {
            for (int z = 0; z < 3; z++)
            {
                grid.Set(x, 0, z, BlockId.RawStone);
            }
        }

        grid.Set(2, 1, 1, new Ramp(RampRise.PlusX, 2, 0).Id);
        grid.Set(3, 1, 1, new Ramp(RampRise.PlusX, 2, 1).Id);
        grid.Set(4, 1, 1, BlockId.RawStone);
        grid.Set(4, 2, 1, new Ramp(RampRise.PlusX, 3, 0).Id);
        grid.Set(5, 1, 1, BlockId.RawStone);
        grid.Set(5, 2, 1, new Ramp(RampRise.PlusX, 3, 1).Id);
        grid.Set(6, 1, 1, BlockId.RawStone);
        grid.Set(6, 2, 1, new Ramp(RampRise.PlusX, 3, 2).Id);
        grid.Set(7, 1, 1, BlockId.RawStone);
        grid.Set(7, 2, 1, BlockId.RawStone);

        Assert.Equal(2, GridMoves.RampWalk(grid, 3, 1, 1, 4, 1));
        Assert.Equal(1, GridMoves.RampWalk(grid, 4, 2, 1, 3, 1));

        Reachability reach = Reachability.From(grid, new Cell(0, 0, 1));
        Assert.Equal(7, reach.Distance(new Cell(7, 2, 1)));
        Assert.Equal(7, Reachability.From(grid, new Cell(7, 2, 1)).Distance(new Cell(0, 0, 1)));

        grid.Set(3, 4, 1, BlockId.RawStone);
        Assert.Equal(-1, GridMoves.RampWalk(grid, 3, 1, 1, 4, 1));
        Assert.Equal(-1, GridMoves.RampWalk(grid, 4, 2, 1, 3, 1));
    }
}
