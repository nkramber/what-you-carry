using System.Collections.Generic;
using Godot;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.World;
using WhatYouCarry.Game.Render;
using WhatYouCarry.Game.World;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The greedy mesher, the chunk layout, and the vertex occlusion (D-78, D-81, D-164, D-291; PR-13 exit tests 3 to 6).</summary>
public sealed class GreedyMesherTests
{
    /// <summary>PR-13 exit test 3. A 4 by 4 by 1 slab in open air gives six faces, not ninety-six.</summary>
    [Fact]
    public void MesherMergesFaces()
    {
        VoxelGrid grid = new(6, 3, 6);
        for (int x = 1; x <= 4; x++)
        {
            for (int z = 1; z <= 4; z++)
            {
                grid.Set(x, 1, z, BlockId.HewnStone);
            }
        }

        MeshData mesh = GreedyMesher.MeshChunk(grid, 0, 0);

        Assert.Equal(6, mesh.QuadCount);
        Assert.Equal(6 * MeshData.QuadVertices, mesh.Positions.Count);
        Assert.Equal(6 * MeshData.QuadIndices, mesh.Indices.Count);
        foreach (Vector3 normal in BoxNormals())
        {
            Assert.Equal(MeshData.QuadVertices, CountVertices(mesh, normal));
        }
    }

    /// <summary>PR-13 exit test 4. One grid gives one buffer, twice over, on every chunk of floor 1 of seed 1.</summary>
    [Fact]
    public void MesherIsDeterministic()
    {
        VoxelGrid grid = TestWorld.NewLoop(1).Grid;
        int quads = 0;
        for (int chunkZ = 0; chunkZ < ChunkLayout.CountZ(grid); chunkZ++)
        {
            for (int chunkX = 0; chunkX < ChunkLayout.CountX(grid); chunkX++)
            {
                MeshData first = GreedyMesher.MeshChunk(grid, chunkX, chunkZ);
                MeshData second = GreedyMesher.MeshChunk(grid, chunkX, chunkZ);
                Assert.Equal(first.Positions, second.Positions);
                Assert.Equal(first.Normals, second.Normals);
                Assert.Equal(first.Colors, second.Colors);
                Assert.Equal(first.Uvs, second.Uvs);
                Assert.Equal(first.TileOrigins, second.TileOrigins);
                Assert.Equal(first.Indices, second.Indices);
                quads += first.QuadCount;
            }
        }

        Assert.True(quads > 0, "Floor 1 of seed 1 shows no face.");
    }

    /// <summary>
    /// PR-13 exit test 5. A maximum floor of D-164 takes 64 chunks, which is the world budget of D-291, and the
    /// player model is the one entity mesh on top. Every chunk of the floor meshes, and the three real templates
    /// stay under the budget too.
    /// </summary>
    [Fact]
    public void MeshBudgetTest()
    {
        VoxelGrid maximum = MaximumFloor();
        int worldMeshes = 0;
        for (int chunkZ = 0; chunkZ < ChunkLayout.CountZ(maximum); chunkZ++)
        {
            for (int chunkX = 0; chunkX < ChunkLayout.CountX(maximum); chunkX++)
            {
                if (GreedyMesher.MeshChunk(maximum, chunkX, chunkZ).QuadCount > 0)
                {
                    worldMeshes++;
                }
            }
        }

        const int entityMeshes = 1;
        Assert.Equal(ChunkLayout.WorldMeshBudget, ChunkLayout.Count(maximum));
        Assert.True(worldMeshes <= ChunkLayout.WorldMeshBudget, $"A maximum floor takes {worldMeshes} world meshes, and the budget is {ChunkLayout.WorldMeshBudget} (D-291).");
        Assert.True(worldMeshes + entityMeshes <= ChunkLayout.WorldMeshBudget + entityMeshes);

        foreach (ulong seed in new ulong[] { 1, 2, 3 })
        {
            VoxelGrid grid = TestWorld.NewLoop(seed).Grid;
            Assert.True(ChunkLayout.Count(grid) <= ChunkLayout.WorldMeshBudget, $"Seed {seed} takes {ChunkLayout.Count(grid)} chunks.");
        }
    }

    /// <summary>
    /// PR-13 exit test 6. A floor vertex in the inner corner of two wall blocks is darker than a floor vertex in
    /// the open: the darkest level against the open level.
    /// </summary>
    [Fact]
    public void AmbientOcclusionDarkensCorners()
    {
        VoxelGrid grid = TestWorld.FlatFloor();
        grid.Set(3, 1, 4, BlockId.RawStone);
        grid.Set(4, 1, 3, BlockId.RawStone);

        MeshData mesh = GreedyMesher.MeshChunk(grid, 0, 0);

        // Every floor vertex at the inner corner has two solid edge cells, so every one takes the darkest level.
        Vector3 innerCorner = new(4.0f, 1.0f, 4.0f);
        int cornerVertices = 0;
        Color brightest = AmbientOcclusion.ColorOf(0);
        for (int vertex = 0; vertex < mesh.Positions.Count; vertex++)
        {
            if (mesh.Normals[vertex] != Vector3.Up)
            {
                continue;
            }

            if (mesh.Colors[vertex].R > brightest.R)
            {
                brightest = mesh.Colors[vertex];
            }

            if (mesh.Positions[vertex] == innerCorner)
            {
                cornerVertices++;
                Assert.Equal(AmbientOcclusion.ColorOf(0), mesh.Colors[vertex]);
            }
        }

        Assert.True(cornerVertices > 0, "No floor vertex sits at the inner corner.");
        Assert.Equal(AmbientOcclusion.ColorOf(AmbientOcclusion.Open), brightest);
        Assert.True(AmbientOcclusion.ColorOf(0).R < brightest.R);
    }

    /// <summary>The four levels: two solid edges give the darkest level, and each solid cell takes one level off the open one.</summary>
    [Fact]
    public void AmbientOcclusionLevelsFollowTheRule()
    {
        Assert.Equal(3, AmbientOcclusion.Level(false, false, false));
        Assert.Equal(2, AmbientOcclusion.Level(true, false, false));
        Assert.Equal(2, AmbientOcclusion.Level(false, false, true));
        Assert.Equal(1, AmbientOcclusion.Level(true, false, true));
        Assert.Equal(0, AmbientOcclusion.Level(true, true, false));
        Assert.Equal(0, AmbientOcclusion.Level(true, true, true));
        Assert.Equal(AmbientOcclusion.Levels, AmbientOcclusion.Brightness.Length);
        for (int level = 1; level < AmbientOcclusion.Levels; level++)
        {
            Assert.True(AmbientOcclusion.Brightness[level - 1] < AmbientOcclusion.Brightness[level]);
        }
    }

    /// <summary>A grid of rock alone shows no face, because the outside is rock too (D-237).</summary>
    [Fact]
    public void EdgeOfTheWorldShowsNoFace()
    {
        VoxelGrid grid = new(4, 4, 4);
        for (int y = 0; y < 4; y++)
        {
            for (int z = 0; z < 4; z++)
            {
                for (int x = 0; x < 4; x++)
                {
                    grid.Set(x, y, z, BlockId.RawStone);
                }
            }
        }

        Assert.Equal(0, GreedyMesher.MeshChunk(grid, 0, 0).QuadCount);
    }

    /// <summary>A pool shows its surface against the air, the stone shows through the water, and the water hides its own sides.</summary>
    [Fact]
    public void WaterShowsItsSurfaceAndHidesItsSides()
    {
        VoxelGrid grid = TestWorld.FlatFloor(8, 6);
        for (int x = 0; x < 8; x++)
        {
            for (int z = 0; z < 8; z++)
            {
                grid.Set(x, 1, z, BlockId.RawStone);
            }
        }

        grid.Set(4, 1, 4, BlockId.StillWater);

        MeshData mesh = GreedyMesher.MeshChunk(grid, 0, 0);

        // The surface of the pool at y = 2, and the stone floor around it at y = 2.
        Assert.Equal(AtlasLayout.TileOrigin(BlockId.StillWater), TileAt(mesh, new Vector3(4.5f, 2.0f, 4.5f), Vector3.Up));

        // The stone under the pool shows its top face against the water, and the pool walls show their sides.
        Assert.Equal(AtlasLayout.TileOrigin(BlockId.RawStone), TileAt(mesh, new Vector3(4.5f, 1.0f, 4.5f), Vector3.Up));
        Assert.Equal(AtlasLayout.TileOrigin(BlockId.RawStone), TileAt(mesh, new Vector3(4.0f, 1.5f, 4.5f), Vector3.Right));

        // The water shows no side face and no bottom face of its own.
        Assert.False(HasFace(mesh, new Vector3(4.0f, 1.5f, 4.5f), Vector3.Left));
        Assert.False(HasFace(mesh, new Vector3(4.5f, 1.0f, 4.5f), Vector3.Down));
    }

    /// <summary>A slab across two chunks shows no face at the chunk border, because each chunk reads its neighbor through the grid.</summary>
    [Fact]
    public void ChunksShareNoSeam()
    {
        VoxelGrid grid = new(20, 3, 4);
        for (int x = 0; x < 20; x++)
        {
            for (int z = 1; z <= 2; z++)
            {
                grid.Set(x, 1, z, BlockId.Plank);
            }
        }

        Assert.Equal(2, ChunkLayout.Count(grid));
        MeshData first = GreedyMesher.MeshChunk(grid, 0, 0);
        MeshData second = GreedyMesher.MeshChunk(grid, 1, 0);

        // The slab continues past the border, so neither chunk shows a face toward the other. The ends of the
        // slab touch the edge of the world, which is rock, so no face shows there either (D-237).
        Assert.Equal(0, CountVertices(first, Vector3.Right));
        Assert.Equal(0, CountVertices(first, Vector3.Left));
        Assert.Equal(0, CountVertices(second, Vector3.Left));
        Assert.Equal(0, CountVertices(second, Vector3.Right));
        Assert.True(CountVertices(first, Vector3.Up) > 0);
        Assert.True(CountVertices(second, Vector3.Up) > 0);
        Assert.True(CountVertices(first, Vector3.Down) > 0);
        Assert.True(CountVertices(second, Vector3.Forward) > 0);
    }

    /// <summary>Every triangle runs clockwise as seen from its normal, which is the front-face order of the engine.</summary>
    [Fact]
    public void QuadsAreClockwiseFromOutside()
    {
        VoxelGrid grid = TestWorld.FlatFloor();
        grid.Set(4, 1, 4, BlockId.RawStone);
        grid.Set(4, 2, 4, BlockId.RawStone);
        MeshData mesh = GreedyMesher.MeshChunk(grid, 0, 0);

        Assert.True(mesh.QuadCount > 6);
        for (int triangle = 0; triangle < mesh.Indices.Count; triangle += 3)
        {
            Vector3 a = mesh.Positions[mesh.Indices[triangle]];
            Vector3 b = mesh.Positions[mesh.Indices[triangle + 1]];
            Vector3 c = mesh.Positions[mesh.Indices[triangle + 2]];
            Vector3 normal = mesh.Normals[mesh.Indices[triangle]];
            Assert.True((b - a).Cross(c - a).Dot(normal) < 0.0f, $"The triangle at index {triangle} runs counterclockwise as seen from its normal {normal}.");
        }
    }

    /// <summary>The face coordinate of a merged face spans its size in tiles, so the tile repeats across it.</summary>
    [Fact]
    public void MergedFaceCoordinatesSpanTheFace()
    {
        VoxelGrid grid = new(6, 3, 6);
        for (int x = 1; x <= 4; x++)
        {
            for (int z = 1; z <= 2; z++)
            {
                grid.Set(x, 1, z, BlockId.OreVein);
            }
        }

        MeshData mesh = GreedyMesher.MeshChunk(grid, 0, 0);

        float maxU = 0.0f;
        float maxV = 0.0f;
        for (int vertex = 0; vertex < mesh.Positions.Count; vertex++)
        {
            if (mesh.Normals[vertex] != Vector3.Up)
            {
                continue;
            }

            maxU = System.Math.Max(maxU, mesh.Uvs[vertex].X);
            maxV = System.Math.Max(maxV, mesh.Uvs[vertex].Y);
            Assert.Equal(AtlasLayout.TileOrigin(BlockId.OreVein), mesh.TileOrigins[vertex]);
        }

        Assert.Equal(4.0f, maxU);
        Assert.Equal(2.0f, maxV);
    }

    /// <summary>The chunk counts of the three template sizes and of the maximum floor, and the budget of D-291.</summary>
    [Fact]
    public void ChunkLayoutCountsThePartialChunk()
    {
        Assert.Equal(64, ChunkLayout.WorldMeshBudget);
        Assert.Equal(16, ChunkLayout.SizeX);
        Assert.Equal(32, ChunkLayout.SizeY);
        Assert.Equal(16, ChunkLayout.SizeZ);
        Assert.Equal(9, ChunkLayout.Count(new VoxelGrid(48, 12, 48)));
        Assert.Equal(25, ChunkLayout.Count(new VoxelGrid(72, 16, 72)));
        Assert.Equal(36, ChunkLayout.Count(new VoxelGrid(96, 20, 96)));
        Assert.Equal(64, ChunkLayout.Count(new VoxelGrid(128, 32, 128)));
        Assert.Equal(1, ChunkLayout.Count(new VoxelGrid(1, 1, 1)));
    }

    /// <summary>A chunk outside the layout is an error that names it, and never an empty mesh (T-2).</summary>
    [Fact]
    public void MeshChunkRejectsAChunkOutsideTheLayout()
    {
        VoxelGrid grid = new(20, 3, 4);
        ContextException error = Assert.Throws<ContextException>(() => GreedyMesher.MeshChunk(grid, 2, 0));
        Assert.Contains("(2, 0)", error.Message, System.StringComparison.Ordinal);
        Assert.Throws<ContextException>(() => GreedyMesher.MeshChunk(grid, 0, -1));
    }

    /// <summary>The tile of a block is its id, eight tiles per row of 32 pixels (D-85, D-259).</summary>
    [Fact]
    public void TileOriginsFollowTheBlockId()
    {
        Assert.Equal(new Vector2(0.0f, 0.0f), AtlasLayout.TileOrigin(BlockId.Air));
        Assert.Equal(new Vector2(0.125f, 0.0f), AtlasLayout.TileOrigin(BlockId.RawStone));
        Assert.Equal(new Vector2(0.875f, 0.0f), AtlasLayout.TileOrigin(BlockId.Plank));
        Assert.Equal(256, AtlasLayout.AtlasPixels);
        Assert.Equal(8, PlaceholderAtlas.TileColors.Length);
    }

    /// <summary>A maximum floor of D-164: rock with a grid of chambers carved out, so every chunk shows faces.</summary>
    private static VoxelGrid MaximumFloor()
    {
        VoxelGrid grid = new(VoxelGrid.MaxSizeX, VoxelGrid.MaxSizeY, VoxelGrid.MaxSizeZ);
        for (int y = 0; y < grid.SizeY; y++)
        {
            for (int z = 0; z < grid.SizeZ; z++)
            {
                for (int x = 0; x < grid.SizeX; x++)
                {
                    bool chamber = x % 8 >= 2 && z % 8 >= 2 && y >= 2 && y < 6;
                    grid.Set(x, y, z, chamber ? BlockId.Air : BlockId.RawStone);
                }
            }
        }

        return grid;
    }

    private static IEnumerable<Vector3> BoxNormals()
    {
        return [Vector3.Right, Vector3.Left, Vector3.Up, Vector3.Down, Vector3.Back, Vector3.Forward];
    }

    private static int CountVertices(MeshData mesh, Vector3 normal)
    {
        int count = 0;
        foreach (Vector3 candidate in mesh.Normals)
        {
            if (candidate == normal)
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>The color of the one vertex at a position with a normal.</summary>
    private static Color ColorAt(MeshData mesh, Vector3 position, Vector3 normal)
    {
        for (int vertex = 0; vertex < mesh.Positions.Count; vertex++)
        {
            if (mesh.Positions[vertex] == position && mesh.Normals[vertex] == normal)
            {
                return mesh.Colors[vertex];
            }
        }

        throw new System.InvalidOperationException($"The mesh has no vertex at {position} with normal {normal}.");
    }

    /// <summary>Answers whether a quad with the normal covers the point, which lies on the face plane.</summary>
    private static bool HasFace(MeshData mesh, Vector3 point, Vector3 normal)
    {
        for (int quad = 0; quad < mesh.QuadCount; quad++)
        {
            int first = quad * MeshData.QuadVertices;
            if (mesh.Normals[first] != normal)
            {
                continue;
            }

            Vector3 low = mesh.Positions[first];
            Vector3 high = mesh.Positions[first];
            for (int corner = 1; corner < MeshData.QuadVertices; corner++)
            {
                low = low.Min(mesh.Positions[first + corner]);
                high = high.Max(mesh.Positions[first + corner]);
            }

            if (point.X >= low.X && point.X <= high.X && point.Y >= low.Y && point.Y <= high.Y && point.Z >= low.Z && point.Z <= high.Z)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>The tile origin of the quad with the normal that covers the point.</summary>
    private static Vector2 TileAt(MeshData mesh, Vector3 point, Vector3 normal)
    {
        for (int quad = 0; quad < mesh.QuadCount; quad++)
        {
            int first = quad * MeshData.QuadVertices;
            if (mesh.Normals[first] != normal)
            {
                continue;
            }

            Vector3 low = mesh.Positions[first];
            Vector3 high = mesh.Positions[first];
            for (int corner = 1; corner < MeshData.QuadVertices; corner++)
            {
                low = low.Min(mesh.Positions[first + corner]);
                high = high.Max(mesh.Positions[first + corner]);
            }

            if (point.X >= low.X && point.X <= high.X && point.Y >= low.Y && point.Y <= high.Y && point.Z >= low.Z && point.Z <= high.Z)
            {
                return mesh.TileOrigins[first];
            }
        }

        throw new System.InvalidOperationException($"The mesh has no face at {point} with normal {normal}.");
    }
}
