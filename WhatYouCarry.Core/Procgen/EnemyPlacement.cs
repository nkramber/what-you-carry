using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Pathfinding;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Core.Procgen;

/// <summary>One enemy spawn of a dug floor: the floor cell that the body stands on, its family, and the chamber that holds it.</summary>
/// <param name="Cell">The floor cell. The feet center of the spawn is the center of the cell above it.</param>
/// <param name="Family">The enemy family of the spawn (D-395).</param>
/// <param name="ChamberIndex">The chamber whose weight paid for the spawn (D-167).</param>
public readonly record struct EnemySpawn(Cell Cell, EnemyDefinition Family, int ChamberIndex);

/// <summary>
/// The enemy spawns of one floor (D-167, D-398): every chamber past the first takes enemies for its weight, and
/// the chamber that holds the player spawn takes none.
/// </summary>
/// <remarks>
/// <para>
/// The count comes from a running total and never from a chamber alone. After each chamber, the count of enemies
/// placed is the total weight so far divided by the weight of the family, rounded to the nearest whole number. A
/// count per chamber would drop the remainder of each one, and the floor would then hold far less than its budget
/// (D-167). The running total drops at most half the weight of one enemy over the whole floor.
/// </para>
/// <para>
/// The cells of one chamber come from its footprint in scan order: the floor under the lowest air row of each
/// column, which is the chamber floor, or the floor of a tier over it (D-348). A ramp cell holds no spawn, because
/// a body on a slope slides to the foot of it (D-363). The cells that a chamber uses are spread evenly over that
/// list, so two enemies of one chamber do not stand side by side.
/// </para>
/// <para>
/// The pass draws nothing, so it adds no number to the Procgen stream and every floor digs as it dug before
/// (D-159). The pass runs after the whole dig and after the detail pass, so a pillar or a heap of rubble takes its
/// cell out of the list before the choice.
/// </para>
/// </remarks>
public static class EnemyPlacement
{
    /// <summary>The chamber that holds the player spawn, which takes no enemy (D-398).</summary>
    public const int SpawnChamberIndex = 0;

    /// <summary>
    /// The spawns of one floor, in chamber order and then in the order of the cells of the chamber.
    /// </summary>
    /// <param name="grid">The grid of the floor, after the detail pass.</param>
    /// <param name="chambers">The chambers of the floor, in dig order. The first one holds the player spawn.</param>
    /// <param name="reach">The reachability search from the player spawn.</param>
    /// <param name="floor">The floor number, from one (D-3).</param>
    /// <param name="families">Every enemy family of the content set (D-395).</param>
    /// <exception cref="ContextException">Two families cover the floor, or a chamber holds fewer free floor cells than its count of enemies.</exception>
    public static IReadOnlyList<EnemySpawn> Place(VoxelGrid grid, IReadOnlyList<Chamber> chambers, Reachability reach, int floor, IReadOnlyList<EnemyDefinition> families)
    {
        List<EnemySpawn> spawns = [];
        if (!TryFamilyOf(floor, families, out EnemyDefinition? family))
        {
            return spawns;
        }

        long weight = family.Weight;
        long total = 0;
        long placed = 0;
        for (int index = SpawnChamberIndex + 1; index < chambers.Count; index++)
        {
            Chamber chamber = chambers[index];
            total += chamber.Kind.Weight;

            // The running total rounds to the nearest whole enemy, so the floor keeps the weight that the
            // chambers before it left over (D-167).
            long target = (total + (weight / 2)) / weight;
            int count = (int)(target - placed);
            placed = target;
            if (count == 0)
            {
                continue;
            }

            IReadOnlyList<Cell> cells = FreeCells(grid, chamber, reach);
            if (cells.Count < count)
            {
                ContextException error = new($"Chamber {index} of kind '{chamber.Kind.Id}' holds {cells.Count} free floor cells, and the budget of D-167 gives it {count} enemies of the family '{family.Id}' (D-398).");
                error.AddContext("chamber", ((long)index).ToString(CultureInfo.InvariantCulture));
                error.AddContext("chamberKind", chamber.Kind.Id);
                error.AddContext("enemyFamily", family.Id);
                error.AddContext("freeCells", ((long)cells.Count).ToString(CultureInfo.InvariantCulture));
                error.AddContext("enemyCount", ((long)count).ToString(CultureInfo.InvariantCulture));
                throw error;
            }

            // The cells spread over the list, so the first one is at the start and the rest follow at one step
            // of the list for each enemy.
            for (int enemy = 0; enemy < count; enemy++)
            {
                spawns.Add(new EnemySpawn(cells[enemy * cells.Count / count], family, index));
            }
        }

        return spawns;
    }

    /// <summary>
    /// The one family that covers a floor, or false when no family does (D-395). PR-36 to PR-42 add the other
    /// seven families, and the PR that adds the second family of one floor band decides how a floor mixes two.
    /// </summary>
    /// <exception cref="ContextException">Two or more families cover the floor, and no rule mixes them yet (T-2).</exception>
    public static bool TryFamilyOf(int floor, IReadOnlyList<EnemyDefinition> families, out EnemyDefinition family)
    {
        List<EnemyDefinition> covering = [];
        foreach (EnemyDefinition candidate in families)
        {
            if (candidate.CoversFloor(floor))
            {
                covering.Add(candidate);
            }
        }

        if (covering.Count > 1)
        {
            ContextException error = new($"{covering.Count} enemy families cover floor {floor}, and PR-16 holds no rule that mixes two families on one floor (D-395).");
            error.AddContext("floor", ((long)floor).ToString(CultureInfo.InvariantCulture));
            error.AddContext("families", ((long)covering.Count).ToString(CultureInfo.InvariantCulture));
            throw error;
        }

        if (covering.Count == 0)
        {
            family = null!;
            return false;
        }

        family = covering[0];
        return true;
    }

    /// <summary>
    /// The floor cells of one chamber that a spawn can stand on, in scan order: the floor under the lowest air row
    /// of each column, when the player spawn reaches it and it is no ramp cell (D-348, D-363).
    /// </summary>
    public static IReadOnlyList<Cell> FreeCells(VoxelGrid grid, Chamber chamber, Reachability reach)
    {
        List<Cell> cells = [];
        foreach (Column column in chamber.Footprint)
        {
            Cell cell = new(column.X, chamber.LowestAirRow(column) - 1, column.Z);
            if (grid.TryGetRamp(cell.X, cell.Y, cell.Z, out _))
            {
                continue;
            }

            if (GridMoves.IsFloor(grid, cell) && reach.IsReachable(cell))
            {
                cells.Add(cell);
            }
        }

        return cells;
    }
}
