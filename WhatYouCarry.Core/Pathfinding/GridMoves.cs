using WhatYouCarry.Core.World;

namespace WhatYouCarry.Core.Pathfinding;

/// <summary>
/// The move rule of a body on the voxel grid (D-76, D-165, D-345, D-486): one block up, any drop, or a walk along a
/// ramp to a side column, and a diagonal move to a corner column. It holds no state, and it reads the grid alone.
/// </summary>
/// <remarks>
/// <para>
/// A floor cell is a solid cell with two open cells above it, so the box of D-165 stands on it. A ramp is a floor
/// cell too, and the body stands on its slope. A side move goes to one of the four neighbor columns. A step up needs the
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
/// A diagonal move goes to one of the four corner columns (D-486). It is legal when two side moves reach the corner
/// column, in either order, and it rises one block at most, so it reaches no cell that the side moves do not.
/// <see cref="Procgen.Reachability"/> reads the side moves alone (D-488), and the floors that the generator digs do
/// not change.
/// </para>
/// <para>
/// Every answer steps in integers and reads the grid alone, so one grid gives one answer everywhere (G-9).
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

    /// <summary>
    /// The units of one block in a floor height. The middle and the edges of every place of every run of D-346 meet
    /// a whole unit, so the rule compares heights in integers.
    /// </summary>
    private const int HeightUnits = 24;

    /// <summary>The count of corner columns of a diagonal move (D-486).</summary>
    public const int Corners = 4;

    /// <summary>The X step of each corner, in the order that the path search reads them.</summary>
    public static readonly int[] CornerStepX = [1, 1, -1, -1];

    /// <summary>The Z step of each corner, in the order that the path search reads them.</summary>
    public static readonly int[] CornerStepZ = [1, -1, 1, -1];

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
    /// The floor row that a diagonal move reaches in a corner column from a floor cell, or <see cref="NoMove"/> when
    /// no such move leads there (D-486). The move takes two side moves in one order: along X first, or along Z
    /// first. It rises one block at most, as a jump does (D-165). The two orders can land on two rows, so a search
    /// reads both.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A jump starts from the middle of the start cell, and the body enters the corner column at the corner that the
    /// two cells share. The floor of the landing at that corner stands one block over the floor under the middle of
    /// the start at most. Two side moves can each rise, for example a walk up a ramp into the next row and then a step
    /// onto a block, and the diagonal move then cuts out the climb that the ramp gave.
    /// </para>
    /// <para>
    /// The body crosses the corner point of the four columns on a straight line, at the height of the higher of the
    /// two floors. The start column and the corner column need two open cells over that floor: a drop leaves the
    /// start at its own height, and a step up jumps in place from the start. A solid side column stops the box on one
    /// axis, and the body slides along the block edge as a player does. Two solid side columns shut the line, so the
    /// move needs one side column open over that floor. A step up needs both side columns open, because a slide
    /// during the jump takes the body past the top of its arc, and it lands short (D-489).
    /// </para>
    /// <para>
    /// A drop crosses the corner point at the height of the start and falls straight down the corner column, so the
    /// cells of that column between the landing and the start are open. The two side moves can reach a landing that
    /// this line does not: a drop into a side column, then a walk under an overhang into the corner column (D-545,
    /// F-111).
    /// </para>
    /// </remarks>
    /// <param name="grid">The grid of the floor.</param>
    /// <param name="x">The X of the floor cell of the start.</param>
    /// <param name="y">The row of the floor cell of the start.</param>
    /// <param name="z">The Z of the floor cell of the start.</param>
    /// <param name="stepX">The X step to the corner column: 1 or -1.</param>
    /// <param name="stepZ">The Z step to the corner column: 1 or -1.</param>
    /// <param name="alongXFirst">True for the order along X first, and false for the order along Z first.</param>
    public static int DiagonalMove(VoxelGrid grid, int x, int y, int z, int stepX, int stepZ, bool alongXFirst)
    {
        int sideX = alongXFirst ? x + stepX : x;
        int sideZ = alongXFirst ? z : z + stepZ;
        int sideY = Move(grid, x, y, z, sideX, sideZ);
        if (sideY == NoMove)
        {
            return NoMove;
        }

        // Two side moves can each rise one block, and one jump clears one block alone (D-165).
        int cornerY = Move(grid, sideX, sideY, sideZ, x + stepX, z + stepZ);
        if (cornerY == NoMove || cornerY > y + 1)
        {
            return NoMove;
        }

        // The heights are in half blocks: the corner that the two cells share, and the middle of the start.
        Cell start = new(x, y, z);
        Cell landing = new(x + stepX, cornerY, z + stepZ);
        int shareX = stepX > 0 ? (x + 1) * 2 : x * 2;
        int shareZ = stepZ > 0 ? (z + 1) * 2 : z * 2;
        int landingFloor = FloorHeightAt(grid, landing, shareX, shareZ);
        if (landingFloor - FloorHeightAt(grid, start, (x * 2) + 1, (z * 2) + 1) > HeightUnits)
        {
            return NoMove;
        }

        int higher = cornerY > y ? cornerY : y;
        bool openEnds = IsOpenOver(grid, x, z, higher) && IsOpenOver(grid, landing.X, landing.Z, higher);
        bool openAlongX = IsOpenOver(grid, landing.X, z, higher);
        bool openAlongZ = IsOpenOver(grid, x, landing.Z, higher);
        bool stepUp = landingFloor > FloorHeightAt(grid, start, shareX, shareZ);
        bool openSides = stepUp ? openAlongX && openAlongZ : openAlongX || openAlongZ;

        // A drop falls straight down the corner column from the height of the start. A side column can reach a floor
        // under an overhang of the corner column, and the body then lands on top of that overhang (F-111).
        bool openFall = IsOpenBetween(grid, landing.X, landing.Z, cornerY + 1, higher);
        return openEnds && openSides && openFall ? cornerY : NoMove;
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

    /// <summary>
    /// The height of the floor of a floor cell at one point of its top face, in units of <see cref="HeightUnits"/>
    /// to a block. The point is in half blocks, so the middle of a cell and each of its corners is a whole number. The
    /// floor of a block is flat, and the floor of a ramp is its slope at that point (D-345).
    /// </summary>
    private static int FloorHeightAt(VoxelGrid grid, Cell cell, int halfX, int halfZ)
    {
        if (!grid.TryGetRamp(cell.X, cell.Y, cell.Z, out Ramp slope))
        {
            return (cell.Y + 1) * HeightUnits;
        }

        int halfAlong;
        switch (slope.Rise)
        {
            case RampRise.PlusX:
                halfAlong = halfX - (cell.X * 2);
                break;
            case RampRise.MinusX:
                halfAlong = ((cell.X + 1) * 2) - halfX;
                break;
            case RampRise.PlusZ:
                halfAlong = halfZ - (cell.Z * 2);
                break;
            default:
                halfAlong = ((cell.Z + 1) * 2) - halfZ;
                break;
        }

        return (cell.Y * HeightUnits) + ((((slope.Place * 2) + halfAlong) * HeightUnits) / (2 * slope.Run));
    }

    /// <summary>Answers whether every cell of a column from one row to another, both included, holds no solid part. A range whose first row is over its last row reads true. A cell outside the grid is solid.</summary>
    private static bool IsOpenBetween(VoxelGrid grid, int x, int z, int fromRow, int toRow)
    {
        for (int row = fromRow; row <= toRow; row++)
        {
            if (grid.IsSolid(x, row, z))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>Answers whether the two cells of a column over one floor row hold no solid part, so the box of D-165 passes there. A cell outside the grid is solid.</summary>
    private static bool IsOpenOver(VoxelGrid grid, int x, int z, int floorRow)
    {
        return !grid.IsSolid(x, floorRow + 1, z) && !grid.IsSolid(x, floorRow + 2, z);
    }
}
