using Godot;

namespace WhatYouCarry.Game.World;

/// <summary>
/// The vertex ambient occlusion of the world mesh (D-81). Each vertex of a face reads the three cells that touch
/// it on the outer side of the face: the two along the edges and the one at the corner. The level counts the
/// open cells, and the brightness of the level goes into the vertex color.
/// </summary>
/// <remarks>
/// The rule is the one the voxel renderers share: two solid edge cells give the darkest level whatever the
/// corner holds, because the corner is then out of sight. The four brightness values are OQ-160.
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
}
