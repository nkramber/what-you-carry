using Godot;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Game.World;

/// <summary>
/// The vertex ambient occlusion of the world mesh (D-81). Each vertex of a face reads the three cells that touch
/// it on the outer side of the face: the two along the edges and the one at the corner. The level counts the
/// open cells, and the brightness of the level goes into the vertex color.
/// </summary>
/// <remarks>
/// <para>
/// The rule is the one the voxel renderers share: two solid edge cells give the darkest level whatever the
/// corner holds, because the corner is then out of sight. The four brightness values are OQ-160.
/// </para>
/// <para>
/// A ramp is solid under its slope alone (D-345), so it darkens a vertex by the upper half of its run. A cell of the
/// upper half reads as a block, and a cell of the lower half reads as air. The floor at the foot of a ramp then stays
/// open, and a wall beside the top of a ramp gets the dark of a floor crease.
/// </para>
/// </remarks>
public static class AmbientOcclusion
{
    /// <summary>The count of levels, from the darkest at zero to the open level at three.</summary>
    public const int Levels = 4;

    /// <summary>The level of a vertex with no solid cell around it.</summary>
    public const int Open = Levels - 1;

    /// <summary>The brightness of each level, from the darkest corner to the open vertex (OQ-160).</summary>
    public static readonly float[] Brightness = [0.55f, 0.70f, 0.85f, 1.0f];

    /// <summary>The level of one vertex from the three outer cells that touch it.</summary>
    public static int Level(bool firstEdgeSolid, bool secondEdgeSolid, bool cornerSolid)
    {
        if (firstEdgeSolid && secondEdgeSolid)
        {
            return 0;
        }

        int solid = (firstEdgeSolid ? 1 : 0) + (secondEdgeSolid ? 1 : 0) + (cornerSolid ? 1 : 0);
        return Open - solid;
    }

    /// <summary>The vertex color of one level: a gray of its brightness.</summary>
    public static Color ColorOf(int level)
    {
        float brightness = Brightness[level];
        return new Color(brightness, brightness, brightness);
    }

    /// <summary>
    /// Answers whether a cell darkens the vertices that it touches. A solid block does, and air and still water do
    /// not. A ramp does when its slope over the middle of the cell stands at least half a block over its row (D-367).
    /// </summary>
    public static bool Occludes(BlockId block)
    {
        if (!Ramp.IsRamp(block))
        {
            return VoxelGrid.IsSolidBlock(block);
        }

        // The slope over the middle of the cell is (Place + 1/2) / Run over the row. It is at least one half when
        // 2 Place + 1 is at least Run.
        Ramp ramp = Ramp.FromId(block);
        return (2 * ramp.Place) + 1 >= ramp.Run;
    }

    /// <summary>
    /// The occlusion level of one corner of a face: the three cells next to the outer cell of the face that touch the
    /// corner, one along each axis of the face and one at the corner. The offsets are -1 or 1 along each axis, and a
    /// cell outside the grid is rock (D-237).
    /// </summary>
    public static int CornerLevel(VoxelGrid grid, int[] outer, int axisU, int axisV, int du, int dv)
    {
        int[] edgeU = [outer[0], outer[1], outer[2]];
        edgeU[axisU] += du;
        int[] edgeV = [outer[0], outer[1], outer[2]];
        edgeV[axisV] += dv;
        int[] corner = [outer[0], outer[1], outer[2]];
        corner[axisU] += du;
        corner[axisV] += dv;
        return Level(
            Occludes(GreedyMesher.BlockAt(grid, edgeU[0], edgeU[1], edgeU[2])),
            Occludes(GreedyMesher.BlockAt(grid, edgeV[0], edgeV[1], edgeV[2])),
            Occludes(GreedyMesher.BlockAt(grid, corner[0], corner[1], corner[2])));
    }
}
