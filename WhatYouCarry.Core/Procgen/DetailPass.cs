using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Core.Procgen;

/// <summary>
/// The detail that makes a dug floor read as the mine of D-210 (D-253, D-254, D-258, D-259): the wall blocks of
/// the band, the pools of the older workings, the pillars in the chambers, and the rubble of collapsed dead ends.
/// </summary>
/// <remarks>
/// <para>
/// A wall block replaces rock that borders air and removes no air, so the tunnels keep the cross-section of
/// D-166 as the dig plan left it. The working mine takes timber posts and planks, the older workings take hewn
/// stone, and the deep takes ore veins in the raw stone.
/// </para>
/// <para>
/// A pool is a small rectangle of chamber floor cells that turn to still water, over rock. Every pool cell keeps
/// a dry floor cell of the chamber beside it, so a body leaves the pool by one jump (D-258, D-262). A pillar is
/// one column of a chamber whose eight neighbors are chamber floor, so the chamber stays one connected floor. A
/// collapse fills the last stamp of a walker that ended in rock with a heap of rubble, one to three rows high.
/// It touches only a walker with no dependent, in cells that this walker alone dug, so no other walker, chamber,
/// or drift loses its way.
/// </para>
/// <para>
/// Every draw comes from the Procgen stream of the floor, after the dig, in scan order (D-159).
/// </para>
/// </remarks>
public static class DetailPass
{
    /// <summary>The band of floors 1 to 5 (D-210).</summary>
    public const string WorkingMine = "working-mine";

    /// <summary>The band of floors 6 to 10 (D-210).</summary>
    public const string OlderWorkings = "older-workings";

    /// <summary>The band of floors 11 to 15 (D-210).</summary>
    public const string Deep = "what-the-miners-reached";

    /// <summary>The spacing of the timber posts of the working mine, in cells along a wall.</summary>
    public const int PostSpacing = 6;

    /// <summary>The smallest chamber footprint, in cells, that takes a pool.</summary>
    public const int PoolChamberCells = 12;

    /// <summary>The count of chamber footprint cells per pillar try.</summary>
    public const int CellsPerPillar = 20;

    // One wall cell in this many takes a plank, or an ore vein.
    private const int PlankChance = 5;
    private const int OreChance = 8;

    /// <summary>Applies the detail of the band of the template to the dug floor.</summary>
    /// <exception cref="ContextException">The band of the template is not one of the three bands of D-210.</exception>
    public static DetailResult Apply(Rng rng, DigCanvas canvas, FloorTemplate template, DigPlan plan)
    {
        string band = template.Band;
        if (band != WorkingMine && band != OlderWorkings && band != Deep)
        {
            ContextException error = new($"The floor template '{template.Id}' names the band '{band}', and the detail pass knows the three bands of D-210 alone: '{WorkingMine}', '{OlderWorkings}', and '{Deep}'.");
            error.AddContext("floorTemplate", template.Id);
            error.AddContext("band", band);
            throw error;
        }

        // The pillars come last, so no wall block lands on a pillar and no pool opens under one.
        List<Cell> collapses = CollapseDeadEnds(rng, canvas, plan);
        List<Cell> pools = band == OlderWorkings ? DigPools(rng, canvas, plan) : [];
        DressWalls(rng, canvas, band);
        List<Cell> pillars = RaisePillars(rng, canvas, plan, band);
        return new DetailResult(pools, pillars, collapses);
    }

    /// <summary>The block of a pillar in a band: a timber post, a hewn stone column, or a rock column.</summary>
    public static BlockId PillarBlock(string band)
    {
        return band == WorkingMine ? BlockId.TimberBeam : band == OlderWorkings ? BlockId.HewnStone : BlockId.RawStone;
    }

    /// <summary>
    /// Fills the last stamp of each walker that ended in rock with rubble, one to three rows high per column. A
    /// walker with a dependent stays open, and so does a dead end whose stamp holds a cell of another walker or
    /// of a chamber.
    /// </summary>
    private static List<Cell> CollapseDeadEnds(Rng rng, DigCanvas canvas, DigPlan plan)
    {
        List<Cell> filled = [];
        foreach (DeadEnd deadEnd in plan.DeadEnds)
        {
            if (plan.HasDependent(deadEnd.Job))
            {
                continue;
            }

            List<Cell> region = [];
            bool safe = true;
            for (int z = deadEnd.End.Z - deadEnd.Radius; z <= deadEnd.End.Z + deadEnd.Radius && safe; z++)
            {
                for (int x = deadEnd.End.X - deadEnd.Radius; x <= deadEnd.End.X + deadEnd.Radius && safe; x++)
                {
                    for (int y = deadEnd.End.Y + 1; y <= deadEnd.End.Y + DigPlan.TunnelHeight; y++)
                    {
                        if (!canvas.IsAir(x, y, z))
                        {
                            continue;
                        }

                        if (!plan.IsDugByJobAlone(x, y, z, deadEnd.Job))
                        {
                            safe = false;
                            break;
                        }

                        region.Add(new Cell(x, y, z));
                    }
                }
            }

            if (!safe || region.Count == 0)
            {
                continue;
            }

            // The heap height of each column comes from one draw per column of the stamp, in scan order.
            int side = (2 * deadEnd.Radius) + 1;
            int[] heights = new int[side * side];
            for (int index = 0; index < heights.Length; index++)
            {
                heights[index] = 1 + rng.NextInt(DigPlan.TunnelHeight);
            }

            foreach (Cell cell in region)
            {
                int local = (cell.X - deadEnd.End.X + deadEnd.Radius) + (side * (cell.Z - deadEnd.End.Z + deadEnd.Radius));
                if (cell.Y - deadEnd.End.Y <= heights[local])
                {
                    canvas.Grid.Set(cell.X, cell.Y, cell.Z, BlockId.Rubble);
                    filled.Add(cell);
                }
            }
        }

        return filled;
    }

    /// <summary>Raises a pillar at a random interior column of each large chamber, one try per twenty cells.</summary>
    private static List<Cell> RaisePillars(Rng rng, DigCanvas canvas, DigPlan plan, string band)
    {
        List<Cell> pillars = [];
        BlockId block = PillarBlock(band);
        foreach (Chamber chamber in plan.Chambers)
        {
            int tries = chamber.Footprint.Count / CellsPerPillar;
            for (int attempt = 0; attempt < tries; attempt++)
            {
                Column center = chamber.Footprint[rng.NextInt(chamber.Footprint.Count)];
                if (!IsInterior(canvas, chamber, center, 1) || IsAnchor(plan, center))
                {
                    continue;
                }

                for (int row = chamber.FloorRow + 1; row <= chamber.FloorRow + chamber.Height; row++)
                {
                    canvas.Grid.Set(center.X, row, center.Z, block);
                }

                pillars.Add(new Cell(center.X, chamber.FloorRow, center.Z));
            }
        }

        return pillars;
    }

    /// <summary>
    /// Turns a small rectangle of floor cells of each large chamber to still water, over rock, when every cell
    /// of it keeps a dry chamber floor cell beside it.
    /// </summary>
    private static List<Cell> DigPools(Rng rng, DigCanvas canvas, DigPlan plan)
    {
        List<Cell> pools = [];
        foreach (Chamber chamber in plan.Chambers)
        {
            if (chamber.Footprint.Count < PoolChamberCells)
            {
                continue;
            }

            Column corner = chamber.Footprint[rng.NextInt(chamber.Footprint.Count)];
            int sizeX = 2 + rng.NextInt(2);
            int sizeZ = 2 + rng.NextInt(2);
            List<Column> pool = [];
            bool fits = true;
            for (int z = corner.Z; z < corner.Z + sizeZ && fits; z++)
            {
                for (int x = corner.X; x < corner.X + sizeX && fits; x++)
                {
                    Column column = new(x, z);
                    int floor = chamber.FloorRow;
                    bool dryRock = IsChamberFloor(canvas, chamber, column) && !canvas.IsAir(column.X, floor - 1, column.Z) && canvas.IsAir(column.X, floor + 1, column.Z);
                    bool hasDryNeighbor = HasDryNeighborOutside(canvas, chamber, column, corner, sizeX, sizeZ);
                    if (!dryRock || IsAnchor(plan, column) || !hasDryNeighbor)
                    {
                        fits = false;
                        break;
                    }

                    pool.Add(column);
                }
            }

            if (!fits)
            {
                continue;
            }

            foreach (Column column in pool)
            {
                canvas.Grid.Set(column.X, chamber.FloorRow, column.Z, BlockId.StillWater);
                pools.Add(new Cell(column.X, chamber.FloorRow, column.Z));
            }
        }

        return pools;
    }

    /// <summary>
    /// Replaces each rock cell that borders air on a side, with rock over it, by the wall block of the band:
    /// timber posts and planks, hewn stone, or ore veins.
    /// </summary>
    private static void DressWalls(Rng rng, DigCanvas canvas, string band)
    {
        VoxelGrid grid = canvas.Grid;
        for (int y = 1; y < grid.SizeY - 1; y++)
        {
            for (int z = 1; z < grid.SizeZ - 1; z++)
            {
                for (int x = 1; x < grid.SizeX - 1; x++)
                {
                    if (grid.Get(x, y, z) != BlockId.RawStone || canvas.IsAir(x, y + 1, z))
                    {
                        continue;
                    }

                    bool bordersAir = canvas.IsAir(x + 1, y, z) || canvas.IsAir(x - 1, y, z) || canvas.IsAir(x, y, z + 1) || canvas.IsAir(x, y, z - 1);
                    if (!bordersAir)
                    {
                        continue;
                    }

                    if (band == WorkingMine)
                    {
                        if ((x + z) % PostSpacing == 0)
                        {
                            grid.Set(x, y, z, BlockId.TimberBeam);
                        }
                        else if (rng.NextInt(PlankChance) == 0)
                        {
                            grid.Set(x, y, z, BlockId.Plank);
                        }
                    }
                    else if (band == OlderWorkings)
                    {
                        grid.Set(x, y, z, BlockId.HewnStone);
                    }
                    else if (rng.NextInt(OreChance) == 0)
                    {
                        grid.Set(x, y, z, BlockId.OreVein);
                    }
                }
            }
        }
    }

    /// <summary>Answers whether the column is a floor cell of the chamber: in the footprint, with rock at the floor row.</summary>
    private static bool IsChamberFloor(DigCanvas canvas, Chamber chamber, Column column)
    {
        foreach (Column cell in chamber.Footprint)
        {
            if (cell == column)
            {
                return canvas.Grid.Get(column.X, chamber.FloorRow, column.Z) == BlockId.RawStone;
            }
        }

        return false;
    }

    /// <summary>Answers whether the column and every column within the radius are dry floor cells of the chamber with air over them.</summary>
    private static bool IsInterior(DigCanvas canvas, Chamber chamber, Column center, int radius)
    {
        for (int z = center.Z - radius; z <= center.Z + radius; z++)
        {
            for (int x = center.X - radius; x <= center.X + radius; x++)
            {
                if (!IsChamberFloor(canvas, chamber, new Column(x, z)) || !canvas.IsAir(x, chamber.FloorRow + 1, z))
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>Answers whether a neighbor of the column outside the pool rectangle is a floor cell of the chamber.</summary>
    private static bool HasDryNeighborOutside(DigCanvas canvas, Chamber chamber, Column column, Column corner, int sizeX, int sizeZ)
    {
        Column[] neighbors = [new(column.X + 1, column.Z), new(column.X - 1, column.Z), new(column.X, column.Z + 1), new(column.X, column.Z - 1)];
        foreach (Column neighbor in neighbors)
        {
            bool insidePool = neighbor.X >= corner.X && neighbor.X < corner.X + sizeX && neighbor.Z >= corner.Z && neighbor.Z < corner.Z + sizeZ;
            if (!insidePool && IsChamberFloor(canvas, chamber, neighbor))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Answers whether the column is the anchor of a chamber. The spawn stands on the anchor of the first one.</summary>
    private static bool IsAnchor(DigPlan plan, Column column)
    {
        foreach (Chamber chamber in plan.Chambers)
        {
            if (chamber.Anchor == column)
            {
                return true;
            }
        }

        return false;
    }
}

/// <summary>What the detail pass placed: the water cells of the pools, the floor cells under the pillars, and the rubble cells.</summary>
public sealed record DetailResult(IReadOnlyList<Cell> Pools, IReadOnlyList<Cell> Pillars, IReadOnlyList<Cell> Collapses);
