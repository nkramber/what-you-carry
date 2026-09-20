using WhatYouCarry.Core.World;

namespace WhatYouCarry.Core.Pathfinding;

/// <summary>
/// The move rule of a body on the voxel grid (D-76, D-165, D-345): one block up, any drop, or a walk along a
/// ramp. It holds no state, and it reads the grid alone.
/// </summary>
/// <remarks>
/// <para>
/// A floor cell is a solid cell with two open cells above it, so the box of D-165 stands on it. A ramp is a floor
/// cell too, and the body stands on its slope. A move goes to one of the four neighbor columns. A step up needs the
/// block of the step, two open cells over it, and a third open cell over the start for the jump. A flat move or a
/// drop needs two open cells in the neighbor column at the height of the start, and the body then lands on the
/// first solid cell below.
/// </para>
/// <para>
/// One rule serves two readers (D-111): <see cref="Procgen.Reachability"/> walks it breadth first, so the generator
/// knows what a body reaches, and <see cref="GridPathfinder"/> searches it, so the AI walks what the generator dug.
/// A second copy of the rule would let the two drift apart, and a floor that the generator calls reachable would
/// then hold a path that no enemy can walk.
/// </para>
/// <para>
/// Every answer steps in integers and reads the grid alone, so one grid gives one answer everywhere (G-9). A body
/// can cut a corner that this rule does not, so the rule names no move that a body cannot make.
/// </para>
/// </remarks>
public static class GridMoves
{
    /// <summary>The answer of <see cref="RampWalk"/> and <see cref="Landing"/> when no move leads to the neighbor column.</summary>
    public const int NoMove = -1;

    /// <summary>The count of neighbor columns of a move: the four sides.</summary>
    public const int Directions = 4;

    /// <summary>The X step of each direction, in the order that every search reads them.</summary>
    public static readonly int[] StepX = [1, -1, 0, 0];

    /// <summary>The Z step of each direction, in the order that every search reads them.</summary>
    public static readonly int[] StepZ = [0, 0, 1, -1];

    /// <summary>Answers whether a body stands on the cell: a block or a ramp, with two open cells above it. A cell outside the grid is no floor.</summary>
    public static bool IsFloor(VoxelGrid grid, Cell cell)
    {
        return grid.Contains(cell.X, cell.Y, cell.Z)
            && grid.IsSolid(cell.X, cell.Y, cell.Z)
            && !grid.IsSolid(cell.X, cell.Y + 1, cell.Z)
            && !grid.IsSolid(cell.X, cell.Y + 2, cell.Z);
    }

    /// <summary>
    /// The floor row that a move reaches in the neighbor column from a floor cell, or <see cref="NoMove"/> when no
    /// move leads there. A walk along a ramp comes first, then a step up, a flat move, or a drop.
    /// </summary>
    public static int Move(VoxelGrid grid, int x, int y, int z, int neighborX, int neighborZ)
    {
        int walk = RampWalk(grid, x, y, z, neighborX, neighborZ);
        if (walk != NoMove)
        {
            return walk;
        }

        return Landing(grid, x, y, z, neighborX, neighborZ);
    }

    /// <summary>
    /// The floor row that a walk along a ramp reaches in the neighbor column from a floor cell, or
    /// <see cref="NoMove"/> when no such walk leads there (D-345). A walk goes to the next place up or down the same
    /// ramp, to the same place of a ramp beside it, from the top of a ramp to a block or to the low end of a ramp one
    /// row up, and from the low end of a ramp to a block or to the top of a ramp one row down. It also goes from a
    /// block to the top of a ramp in its row or to the low end of a ramp one row up. Of two cells a row apart, the
    /// lower one needs a third open cell over it, because a body over both columns rises into that cell.
    /// </summary>
    public static int RampWalk(VoxelGrid grid, int x, int y, int z, int neighborX, int neighborZ)
    {
        RampRise toward;
        RampRise away;
        if (neighborX > x)
        {
            toward = RampRise.PlusX;
            away = RampRise.MinusX;
        }
        else if (neighborX < x)
        {
            toward = RampRise.MinusX;
            away = RampRise.PlusX;
        }
        else if (neighborZ > z)
        {
            toward = RampRise.PlusZ;
            away = RampRise.MinusZ;
        }
        else
        {
            toward = RampRise.MinusZ;
            away = RampRise.PlusZ;
        }

        bool levelRamp = grid.TryGetRamp(neighborX, y, neighborZ, out Ramp level);
        bool levelFloor = IsFloor(grid, new Cell(neighborX, y, neighborZ));
        bool upperRamp = grid.TryGetRamp(neighborX, y + 1, neighborZ, out Ramp upper);
        bool upperFloor = IsFloor(grid, new Cell(neighborX, y + 1, neighborZ));
        bool lowEndAhead = upperRamp && upperFloor && upper.Rise == toward && upper.Place == 0 && !grid.IsSolid(x, y + 3, z);

        if (!grid.TryGetRamp(x, y, z, out Ramp start))
        {
            bool topAhead = levelRamp && levelFloor && level.Rise == away && level.Place == level.Run - 1;
            if (topAhead)
            {
                return y;
            }

            return lowEndAhead ? y + 1 : NoMove;
        }

        if (start.Rise == toward && start.Place == start.Run - 1)
        {
            if (levelFloor && !levelRamp)
            {
                return y;
            }

            return lowEndAhead ? y + 1 : NoMove;
        }

        if (start.Rise == away && start.Place == 0)
        {
            bool lowerRamp = grid.TryGetRamp(neighborX, y - 1, neighborZ, out Ramp lower);
            bool lowerTop = !lowerRamp || (lower.Rise == away && lower.Place == lower.Run - 1);
            bool lowerRoom = IsFloor(grid, new Cell(neighborX, y - 1, neighborZ)) && !grid.IsSolid(neighborX, y + 2, neighborZ);
            return lowerTop && lowerRoom ? y - 1 : NoMove;
        }

        int place = start.Place;
        if (start.Rise == toward)
        {
            place++;
        }
        else if (start.Rise == away)
        {
            place--;
        }

        bool joins = levelRamp && levelFloor && level.Rise == start.Rise && level.Run == start.Run && level.Place == place;
        return joins ? y : NoMove;
    }

    /// <summary>
    /// The floor row that a step or a drop reaches in the neighbor column from a floor cell, or <see cref="NoMove"/>
    /// when no such move leads there. A step up comes first, then a flat move or a drop. A ramp counts as solid for
    /// the step and for the landing. From a ramp, the feet stand under the top of the cell, so a drop needs the
    /// neighbor open from the ramp row up, and a step up reaches a block in the ramp row alone.
    /// </summary>
    public static int Landing(VoxelGrid grid, int x, int y, int z, int neighborX, int neighborZ)
    {
        if (grid.TryGetRamp(x, y, z, out _))
        {
            bool openAhead = !grid.IsSolid(neighborX, y, neighborZ) && !grid.IsSolid(neighborX, y + 1, neighborZ) && !grid.IsSolid(neighborX, y + 2, neighborZ);
            if (openAhead)
            {
                int dropY = y - 1;
                while (dropY >= 0 && !grid.IsSolid(neighborX, dropY, neighborZ))
                {
                    dropY--;
                }

                return dropY;
            }

            bool blockAhead = grid.IsSolid(neighborX, y, neighborZ) && !grid.TryGetRamp(neighborX, y, neighborZ, out _);
            bool rampStep = blockAhead
                && !grid.IsSolid(neighborX, y + 1, neighborZ)
                && !grid.IsSolid(neighborX, y + 2, neighborZ)
                && !grid.IsSolid(x, y + 3, z);
            return rampStep ? y : NoMove;
        }

        // A step up takes a block or a ramp cell. The slope of a ramp cell in the row over the feet stands at most
        // one block over them, because a slope rises one block over its whole run, so a jump clears it (D-165,
        // D-345).
        bool stepUp = grid.IsSolid(neighborX, y + 1, neighborZ)
            && !grid.IsSolid(neighborX, y + 2, neighborZ)
            && !grid.IsSolid(neighborX, y + 3, neighborZ)
            && !grid.IsSolid(x, y + 3, z);
        if (stepUp)
        {
            return y + 1;
        }

        if (grid.IsSolid(neighborX, y + 1, neighborZ) || grid.IsSolid(neighborX, y + 2, neighborZ))
        {
            return NoMove;
        }

        // The body falls to the first solid cell at or below the height of the start. The cells on the way down
        // are open by the scan, so the landing has its two open cells.
        int landingY = y;
        while (landingY >= 0 && !grid.IsSolid(neighborX, landingY, neighborZ))
        {
            landingY--;
        }

        return landingY;
    }
}
