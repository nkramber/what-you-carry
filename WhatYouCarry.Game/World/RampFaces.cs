using System;
using System.Collections.Generic;
using System.Globalization;
using Godot;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.World;
using WhatYouCarry.Game.Render;

namespace WhatYouCarry.Game.World;

/// <summary>
/// The faces of the ramp cells of one chunk (D-345, D-367): the slopes, merged into the largest rectangles of one
/// plane and equal occlusion, and each end, side, and bottom of a ramp cell that shows. Every face takes the raw stone
/// tile (D-368) at 32 texels per meter (D-308), with the vertex occlusion of D-81.
/// </summary>
/// <remarks>
/// <para>
/// The slopes of one run of a ramp lie in one plane, and so do the slopes of the same run in the cells beside it
/// across the rise. In one row, the plane is the rise, the run, and the foot: the coordinate along the rise where the
/// plane meets the bottom of the row. Two slopes merge when these and the occlusion levels of their corners are equal.
/// A slope reads the occlusion of an up face at the cell over the ramp, so two slopes of one plane get the same level
/// at a corner that they share. The face coordinate along the rise counts meters along the slope from the foot, so the
/// tile keeps 32 texels per meter on the slope, and a merge does not move it.
/// </para>
/// <para>
/// The other faces of a ramp cell do not merge. Each is a rectangle, or a triangle where a side comes to a point at
/// the low end of a run. A face shows unless the side of the neighbor covers it (<see cref="FaceShape"/>), and its
/// corners read the occlusion of the whole side of the cell. A slope always shows, because a neighbor touches no more
/// than its edge.
/// </para>
/// </remarks>
public static class RampFaces
{
    /// <summary>The block whose tile every face of a ramp takes: raw stone, the floor of every band (D-368).</summary>
    public const BlockId Tile = BlockId.RawStone;

    private const int AxisX = 0;
    private const int AxisY = 1;
    private const int AxisZ = 2;

    /// <summary>
    /// Adds the slopes of every ramp cell inside the bounds, one row at a time. The bounds are the low corner and the
    /// high corner of the chunk, the high corner outside it, as the mesher gives them.
    /// </summary>
    public static void AddSlopes(VoxelGrid grid, MeshData data, int[] low, int[] high, BlockTiles tiles)
    {
        int sizeX = high[AxisX] - low[AxisX];
        int sizeZ = high[AxisZ] - low[AxisZ];
        SlopeKey[] mask = new SlopeKey[sizeX * sizeZ];
        for (int row = low[AxisY]; row < high[AxisY]; row++)
        {
            bool rowHoldsRamp = false;
            for (int z = low[AxisZ]; z < high[AxisZ]; z++)
            {
                for (int x = low[AxisX]; x < high[AxisX]; x++)
                {
                    if (!grid.TryGetRamp(x, row, z, out Ramp ramp))
                    {
                        continue;
                    }

                    rowHoldsRamp = true;
                    int[] over = [x, row + 1, z];
                    mask[(x - low[AxisX]) + ((z - low[AxisZ]) * sizeX)] = new SlopeKey(
                        ramp.Rise,
                        ramp.Run,
                        FootOf(ramp, x, z),
                        AmbientOcclusion.CornerLevel(grid, over, AxisX, AxisZ, -1, -1),
                        AmbientOcclusion.CornerLevel(grid, over, AxisX, AxisZ, 1, -1),
                        AmbientOcclusion.CornerLevel(grid, over, AxisX, AxisZ, 1, 1),
                        AmbientOcclusion.CornerLevel(grid, over, AxisX, AxisZ, -1, 1));
                }
            }

            if (!rowHoldsRamp)
            {
                continue;
            }

            // The sweep clears every cell that it takes, so the mask is empty again for the next row.
            foreach (MaskRectangle<SlopeKey> rectangle in GreedySweep.Rectangles(mask, sizeX, sizeZ))
            {
                AddSlope(data, row, low[AxisX] + rectangle.U, low[AxisZ] + rectangle.V, rectangle.Width, rectangle.Height, rectangle.Key, tiles);
            }
        }
    }

    /// <summary>
    /// Adds each end, side, and bottom of every ramp cell inside the bounds that shows. A ramp has no top, because its
    /// slope stands in place of the top, so the face toward up never shows.
    /// </summary>
    public static void AddCellFaces(VoxelGrid grid, MeshData data, int[] low, int[] high, BlockTiles tiles)
    {
        for (int y = low[AxisY]; y < high[AxisY]; y++)
        {
            for (int z = low[AxisZ]; z < high[AxisZ]; z++)
            {
                for (int x = low[AxisX]; x < high[AxisX]; x++)
                {
                    BlockId block = grid.Get(x, y, z);
                    if (!Ramp.IsRamp(block))
                    {
                        continue;
                    }

                    for (int direction = 0; direction < FaceDirection.Count; direction++)
                    {
                        int[] outer = [x, y, z];
                        outer[FaceDirection.Axis(direction)] += FaceDirection.Sign(direction);
                        if (!GreedyMesher.FaceVisible(block, GreedyMesher.BlockAt(grid, outer[0], outer[1], outer[2]), direction))
                        {
                            continue;
                        }

                        if (direction == FaceDirection.Down)
                        {
                            AddBottom(grid, data, x, y, z, tiles);
                        }
                        else
                        {
                            AddSide(grid, data, x, y, z, direction, FaceShape.OfFace(block, direction), tiles);
                        }
                    }
                }
            }
        }
    }

    /// <summary>The foot of the plane of a ramp in its row: the coordinate along the rise where the slope meets the bottom of the row.</summary>
    private static int FootOf(Ramp ramp, int x, int z)
    {
        switch (ramp.Rise)
        {
            case RampRise.PlusX:
                return x - ramp.Place;
            case RampRise.MinusX:
                return x + 1 + ramp.Place;
            case RampRise.PlusZ:
                return z - ramp.Place;
            default:
                return z + 1 + ramp.Place;
        }
    }

    /// <summary>
    /// One merged slope over X from startX to startX + width and over Z from startZ to startZ + depth. The corners run
    /// clockwise as seen from above, and the diagonal follows the rule of a block face, so the dark of a corner does not
    /// spread across the slope.
    /// </summary>
    private static void AddSlope(MeshData data, int row, int startX, int startZ, int width, int depth, SlopeKey key, BlockTiles tiles)
    {
        // Clockwise as seen from above: the low X and low Z corner, then high X, then high X and high Z, then high Z.
        int[] cornersX = [startX, startX + width, startX + width, startX];
        int[] cornersZ = [startZ, startZ, startZ + depth, startZ + depth];
        int[] levels = [key.LevelLowXLowZ, key.LevelHighXLowZ, key.LevelHighXHighZ, key.LevelLowXHighZ];
        bool risesAlongX = key.Rise == RampRise.PlusX || key.Rise == RampRise.MinusX;
        int uphillSign = key.Rise == RampRise.PlusX || key.Rise == RampRise.PlusZ ? 1 : -1;

        // One meter along the rise axis is this many meters along the slope.
        float slopeLength = MathF.Sqrt((key.Run * key.Run) + 1.0f) / key.Run;

        Vector3[] positions = new Vector3[MeshData.QuadVertices];
        Color[] colors = new Color[MeshData.QuadVertices];
        Vector2[] uvs = new Vector2[MeshData.QuadVertices];
        for (int corner = 0; corner < MeshData.QuadVertices; corner++)
        {
            int fromFoot = (risesAlongX ? cornersX[corner] : cornersZ[corner]) - key.Foot;
            float height = row + ((float)(uphillSign * fromFoot) / key.Run);
            positions[corner] = new Vector3(cornersX[corner], height, cornersZ[corner]);
            colors[corner] = AmbientOcclusion.ColorOf(levels[corner]);

            // X across and Z down, as on an up face, with the meters along the slope on the rise axis.
            float alongSlope = fromFoot * slopeLength;
            uvs[corner] = risesAlongX ? new Vector2(alongSlope, cornersZ[corner] - startZ) : new Vector2(cornersX[corner] - startX, alongSlope);
        }

        Vector3 normal = risesAlongX ? new Vector3(-uphillSign, key.Run, 0.0f) : new Vector3(0.0f, key.Run, -uphillSign);
        bool flip = levels[0] + levels[2] > levels[1] + levels[3];
        data.AddQuad(positions, normal.Normalized(), colors, uvs, tiles.Origin(Tile), flip);
    }

    /// <summary>The bottom of one ramp cell: a whole square at the bottom of the row, clockwise as seen from below.</summary>
    private static void AddBottom(VoxelGrid grid, MeshData data, int x, int y, int z, BlockTiles tiles)
    {
        int[] under = [x, y - 1, z];

        // Clockwise as seen from below: the low X and low Z corner, then high Z, then high X and high Z, then high X.
        int[] offsetsX = [0, 0, 1, 1];
        int[] offsetsZ = [0, 1, 1, 0];
        int[] levels = new int[MeshData.QuadVertices];
        Vector3[] positions = new Vector3[MeshData.QuadVertices];
        Color[] colors = new Color[MeshData.QuadVertices];
        Vector2[] uvs = new Vector2[MeshData.QuadVertices];
        for (int corner = 0; corner < MeshData.QuadVertices; corner++)
        {
            int toX = offsetsX[corner] == 0 ? -1 : 1;
            int toZ = offsetsZ[corner] == 0 ? -1 : 1;
            levels[corner] = AmbientOcclusion.CornerLevel(grid, under, AxisX, AxisZ, toX, toZ);
            positions[corner] = new Vector3(x + offsetsX[corner], y, z + offsetsZ[corner]);
            colors[corner] = AmbientOcclusion.ColorOf(levels[corner]);
            uvs[corner] = new Vector2(offsetsX[corner], offsetsZ[corner]);
        }

        bool flip = levels[0] + levels[2] > levels[1] + levels[3];
        data.AddQuad(positions, Vector3.Down, colors, uvs, tiles.Origin(Tile), flip);
    }

    /// <summary>
    /// One end or side of a ramp cell: from the bottom of the row up to the heights of the shape at the two ends of the
    /// side. The corners run clockwise as seen from outside: the top of the left end, the top of the right end, and the
    /// bottoms of the right end and the left end. A top with no height is the bottom corner under it, so a side with
    /// one such end is a triangle.
    /// </summary>
    /// <exception cref="ContextException">The shape is empty, so the side has no face.</exception>
    private static void AddSide(VoxelGrid grid, MeshData data, int x, int y, int z, int direction, FaceShape shape, BlockTiles tiles)
    {
        if (shape.IsEmpty)
        {
            ContextException error = new($"The side of the ramp cell ({x}, {y}, {z}) toward direction {direction} is empty, so it has no face to add.");
            error.AddContext(nameof(direction), direction.ToString(CultureInfo.InvariantCulture));
            throw error;
        }

        int axis = FaceDirection.Axis(direction);
        int sign = FaceDirection.Sign(direction);
        int edgeAxis = axis == AxisX ? AxisZ : AxisX;
        int[] cell = [x, y, z];
        int[] outer = [x, y, z];
        outer[axis] += sign;

        // As seen from outside, the right end of the side is its high end for a face toward minus X or plus Z.
        bool rightIsHighEnd = direction == FaceDirection.MinusX || direction == FaceDirection.PlusZ;
        int leftOffset = rightIsHighEnd ? 0 : 1;
        int rightOffset = rightIsHighEnd ? 1 : 0;
        int leftHeight = rightIsHighEnd ? shape.LowEnd : shape.HighEnd;
        int rightHeight = rightIsHighEnd ? shape.HighEnd : shape.LowEnd;
        int[] edgeOffsets = [leftOffset, rightOffset, rightOffset, leftOffset];
        int[] heights = [leftHeight, rightHeight, 0, 0];
        int[] verticalOffsets = [1, 1, -1, -1];

        List<Vector3> positions = [];
        List<Color> colors = [];
        List<Vector2> uvs = [];
        List<int> levels = [];
        for (int corner = 0; corner < MeshData.QuadVertices; corner++)
        {
            if (verticalOffsets[corner] > 0 && heights[corner] == 0)
            {
                continue;
            }

            float[] point = new float[3];
            point[axis] = sign > 0 ? cell[axis] + 1 : cell[axis];
            point[AxisY] = y + ((float)heights[corner] / FaceShape.Whole);
            point[edgeAxis] = cell[edgeAxis] + edgeOffsets[corner];
            int level = AmbientOcclusion.CornerLevel(grid, outer, AxisY, edgeAxis, verticalOffsets[corner], edgeOffsets[corner] == 0 ? -1 : 1);
            positions.Add(new Vector3(point[0], point[1], point[2]));
            colors.Add(AmbientOcclusion.ColorOf(level));

            // Along the side and down from the top of the row, as on the side of a block in the same row.
            uvs.Add(new Vector2(edgeOffsets[corner], 1.0f - ((float)heights[corner] / FaceShape.Whole)));
            levels.Add(level);
        }

        float[] normal = new float[3];
        normal[axis] = sign;
        Vector3 faceNormal = new(normal[0], normal[1], normal[2]);
        if (positions.Count == MeshData.TriangleVertices)
        {
            data.AddTriangle(positions.ToArray(), faceNormal, colors.ToArray(), uvs.ToArray(), tiles.Origin(Tile));
            return;
        }

        bool flip = levels[0] + levels[2] > levels[1] + levels[3];
        data.AddQuad(positions.ToArray(), faceNormal, colors.ToArray(), uvs.ToArray(), tiles.Origin(Tile), flip);
    }

    /// <summary>
    /// The plane and the four corner levels of one slope. The corners are named by their place on X and on Z. The
    /// default value, with a run of zero, is no slope.
    /// </summary>
    private readonly record struct SlopeKey(RampRise Rise, int Run, int Foot, int LevelLowXLowZ, int LevelHighXLowZ, int LevelHighXHighZ, int LevelLowXHighZ);
}
