using Godot;
using WhatYouCarry.Core.World;
using WhatYouCarry.Game.Render;

namespace WhatYouCarry.Game.World;

/// <summary>
/// The greedy mesher of one chunk (D-78, D-291): every visible block face in the chunk, merged into the largest
/// rectangles of equal block and equal occlusion, with the vertex occlusion of D-81 in the vertex colors.
/// </summary>
/// <remarks>
/// <para>
/// A face is visible when its block is not air, the cell past it is not solid, and the two blocks differ. So a
/// stone wall shows against air and against water, a pool shows its surface against air, and two water cells
/// share no face. A cell outside the grid is rock (D-237), so the edge of the world shows no face, and a face
/// at a chunk border reads the neighbor chunk through the grid, so no seam shows.
/// </para>
/// <para>
/// The mesher reads the grid and nothing else, so one grid gives one buffer on every machine, and it needs no
/// change for the wall fade, which the world shader does (D-292). Each direction runs one slice at a time: a
/// mask of the visible faces of the slice, then a sweep that takes the widest run and the tallest stack of equal
/// mask values as one quad.
/// </para>
/// </remarks>
public static class GreedyMesher
{
    /// <summary>The count of face directions.</summary>
    public const int Directions = 6;

    /// <summary>The block that stands in for a cell outside the grid (D-237).</summary>
    public const BlockId Outside = BlockId.RawStone;

    private const string ChunkXField = "chunkX";
    private const string ChunkZField = "chunkZ";

    /// <summary>The mesh of one chunk. The chunk indices count from zero along X and Z.</summary>
    /// <exception cref="Core.Logging.ContextException">The chunk is outside the layout of the grid.</exception>
    public static MeshData MeshChunk(VoxelGrid grid, int chunkX, int chunkZ)
    {
        if (chunkX < 0 || chunkX >= ChunkLayout.CountX(grid) || chunkZ < 0 || chunkZ >= ChunkLayout.CountZ(grid))
        {
            Core.Logging.ContextException error = new($"The chunk ({chunkX}, {chunkZ}) is outside the layout of {ChunkLayout.CountX(grid)} by {ChunkLayout.CountZ(grid)} chunks of the grid.");
            error.AddContext(ChunkXField, chunkX.ToString(System.Globalization.CultureInfo.InvariantCulture));
            error.AddContext(ChunkZField, chunkZ.ToString(System.Globalization.CultureInfo.InvariantCulture));
            throw error;
        }

        int lowX = chunkX * ChunkLayout.SizeX;
        int lowZ = chunkZ * ChunkLayout.SizeZ;
        int highX = System.Math.Min(lowX + ChunkLayout.SizeX, grid.SizeX);
        int highZ = System.Math.Min(lowZ + ChunkLayout.SizeZ, grid.SizeZ);
        int[] low = [lowX, 0, lowZ];
        int[] high = [highX, grid.SizeY, highZ];

        MeshData data = new();
        for (int direction = 0; direction < Directions; direction++)
        {
            int axis = direction / 2;
            int sign = direction % 2 == 0 ? 1 : -1;
            for (int slice = low[axis]; slice < high[axis]; slice++)
            {
                MeshSlice(grid, data, axis, sign, slice, low, high);
            }
        }

        return data;
    }

    /// <summary>The block of a cell, or rock for a cell outside the grid (D-237).</summary>
    public static BlockId BlockAt(VoxelGrid grid, int x, int y, int z)
    {
        return grid.Contains(x, y, z) ? grid.Get(x, y, z) : Outside;
    }

    /// <summary>Answers whether the face of a block toward a neighbor shows: the block is not air, the neighbor is not solid, and the two differ.</summary>
    public static bool FaceVisible(BlockId block, BlockId neighbor)
    {
        return block != BlockId.Air && !VoxelGrid.IsSolidBlock(neighbor) && neighbor != block;
    }

    /// <summary>
    /// One slice of one direction: the mask of visible faces, then the greedy sweep. The two axes of the slice
    /// are the next two after the face axis, in cyclic order, so their cross product points along the face axis.
    /// </summary>
    private static void MeshSlice(VoxelGrid grid, MeshData data, int axis, int sign, int slice, int[] low, int[] high)
    {
        int axisU = (axis + 1) % 3;
        int axisV = (axis + 2) % 3;
        int sizeU = high[axisU] - low[axisU];
        int sizeV = high[axisV] - low[axisV];
        FaceKey[] mask = new FaceKey[sizeU * sizeV];

        int[] cell = new int[3];
        int[] outer = new int[3];
        for (int v = 0; v < sizeV; v++)
        {
            for (int u = 0; u < sizeU; u++)
            {
                cell[axis] = slice;
                cell[axisU] = low[axisU] + u;
                cell[axisV] = low[axisV] + v;
                BlockId block = grid.Get(cell[0], cell[1], cell[2]);
                outer[0] = cell[0];
                outer[1] = cell[1];
                outer[2] = cell[2];
                outer[axis] += sign;
                if (!FaceVisible(block, BlockAt(grid, outer[0], outer[1], outer[2])))
                {
                    continue;
                }

                mask[u + (v * sizeU)] = new FaceKey(
                    block,
                    CornerLevel(grid, outer, axisU, axisV, -1, -1),
                    CornerLevel(grid, outer, axisU, axisV, 1, -1),
                    CornerLevel(grid, outer, axisU, axisV, 1, 1),
                    CornerLevel(grid, outer, axisU, axisV, -1, 1));
            }
        }

        for (int v = 0; v < sizeV; v++)
        {
            for (int u = 0; u < sizeU; u++)
            {
                FaceKey key = mask[u + (v * sizeU)];
                if (!key.Visible)
                {
                    continue;
                }

                int width = 1;
                while (u + width < sizeU && mask[u + width + (v * sizeU)] == key)
                {
                    width++;
                }

                int height = 1;
                while (v + height < sizeV && RowMatches(mask, sizeU, u, v + height, width, key))
                {
                    height++;
                }

                for (int dv = 0; dv < height; dv++)
                {
                    for (int du = 0; du < width; du++)
                    {
                        mask[u + du + ((v + dv) * sizeU)] = default;
                    }
                }

                EmitQuad(data, axis, sign, slice, low[axisU] + u, low[axisV] + v, width, height, key);
            }
        }
    }

    /// <summary>Answers whether a run of one row of the mask holds one key.</summary>
    private static bool RowMatches(FaceKey[] mask, int sizeU, int u, int v, int width, FaceKey key)
    {
        for (int du = 0; du < width; du++)
        {
            if (mask[u + du + (v * sizeU)] != key)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>The occlusion level of one corner of a face: the three outer cells that touch that corner.</summary>
    private static int CornerLevel(VoxelGrid grid, int[] outer, int axisU, int axisV, int du, int dv)
    {
        int[] edgeU = [outer[0], outer[1], outer[2]];
        edgeU[axisU] += du;
        int[] edgeV = [outer[0], outer[1], outer[2]];
        edgeV[axisV] += dv;
        int[] corner = [outer[0], outer[1], outer[2]];
        corner[axisU] += du;
        corner[axisV] += dv;
        return AmbientOcclusion.Level(
            VoxelGrid.IsSolidBlock(BlockAt(grid, edgeU[0], edgeU[1], edgeU[2])),
            VoxelGrid.IsSolidBlock(BlockAt(grid, edgeV[0], edgeV[1], edgeV[2])),
            VoxelGrid.IsSolidBlock(BlockAt(grid, corner[0], corner[1], corner[2])));
    }

    /// <summary>
    /// One merged quad. The face plane sits at the far side of the cell for a positive direction. The corners
    /// run clockwise as seen from the outer side, and the diagonal joins the two corners with the lower sum of
    /// occlusion, so the dark of a corner does not spread across the face.
    /// </summary>
    private static void EmitQuad(MeshData data, int axis, int sign, int slice, int startU, int startV, int width, int height, FaceKey key)
    {
        int axisU = (axis + 1) % 3;
        int axisV = (axis + 2) % 3;
        float plane = sign > 0 ? slice + 1 : slice;

        // The four corners in the plane, from the low corner counterclockwise about the face axis, with their
        // occlusion levels and their face coordinates in the same order.
        int[] offsetsU = [0, width, width, 0];
        int[] offsetsV = [0, 0, height, height];
        int[] levels = [key.LevelLowLow, key.LevelHighLow, key.LevelHighHigh, key.LevelLowHigh];
        Vector3[] positions = new Vector3[MeshData.QuadVertices];
        Color[] colors = new Color[MeshData.QuadVertices];
        Vector2[] uvs = new Vector2[MeshData.QuadVertices];
        for (int corner = 0; corner < MeshData.QuadVertices; corner++)
        {
            float[] point = new float[3];
            point[axis] = plane;
            point[axisU] = startU + offsetsU[corner];
            point[axisV] = startV + offsetsV[corner];
            positions[corner] = new Vector3(point[0], point[1], point[2]);
            colors[corner] = AmbientOcclusion.ColorOf(levels[corner]);
            uvs[corner] = FaceCoordinate(axis, offsetsU[corner], offsetsV[corner], width, height);
        }

        // Counterclockwise about the axis is clockwise from the outer side of a negative direction. A positive
        // direction takes the reverse order.
        int[] order = sign > 0 ? [0, 3, 2, 1] : [0, 1, 2, 3];
        Vector3[] orderedPositions = new Vector3[MeshData.QuadVertices];
        Color[] orderedColors = new Color[MeshData.QuadVertices];
        Vector2[] orderedUvs = new Vector2[MeshData.QuadVertices];
        int[] orderedLevels = new int[MeshData.QuadVertices];
        for (int corner = 0; corner < MeshData.QuadVertices; corner++)
        {
            orderedPositions[corner] = positions[order[corner]];
            orderedColors[corner] = colors[order[corner]];
            orderedUvs[corner] = uvs[order[corner]];
            orderedLevels[corner] = levels[order[corner]];
        }

        float[] normal = new float[3];
        normal[axis] = sign;
        bool flip = orderedLevels[0] + orderedLevels[2] > orderedLevels[1] + orderedLevels[3];
        data.AddQuad(orderedPositions, new Vector3(normal[0], normal[1], normal[2]), orderedColors, orderedUvs, AtlasLayout.TileOrigin(key.Block), flip);
    }

    /// <summary>
    /// The face coordinate of one corner, in tiles. A vertical face stands upright, so its second coordinate
    /// grows downward, and the up and down faces take X across and Z down.
    /// </summary>
    private static Vector2 FaceCoordinate(int axis, int offsetU, int offsetV, int width, int height)
    {
        switch (axis)
        {
            case 0:
                // The face axis is X: U runs along Y, and V runs along Z.
                return new Vector2(offsetV, width - offsetU);
            case 1:
                // The face axis is Y: U runs along Z, and V runs along X.
                return new Vector2(offsetV, offsetU);
            default:
                // The face axis is Z: U runs along X, and V runs along Y.
                return new Vector2(offsetU, height - offsetV);
        }
    }

    /// <summary>The block and the four corner levels of one visible face. The default value is no face.</summary>
    private readonly record struct FaceKey(BlockId Block, int LevelLowLow, int LevelHighLow, int LevelHighHigh, int LevelLowHigh)
    {
        /// <summary>Answers whether the key stands for a face. Air never shows a face, so the default is none.</summary>
        public bool Visible => this.Block != BlockId.Air;
    }
}
