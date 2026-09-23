using System;
using System.Collections.Generic;
using Godot;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Procgen;
using WhatYouCarry.Core.World;
using WhatYouCarry.Game.Render;
using WhatYouCarry.Game.World;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// The greedy mesher, the chunk layout, and the vertex occlusion (D-78, D-81, D-164, D-291; PR-13 exit tests 3 to 6),
/// and the faces of the ramp cells (D-345, D-367, D-368; PR-65 exit tests 1 and 2).
/// </summary>
public sealed class GreedyMesherTests
{
    /// <summary>The first cell across of the narrow ramp of <see cref="NarrowCourse"/>. Air stands in the cell before it.</summary>
    private const int NarrowFirst = 3;

    /// <summary>The last cell across of the narrow ramp. A hewn stone wall stands in the cell after it.</summary>
    private const int NarrowLast = 5;

    /// <summary>The tolerance of a point on a face and of a length, in meters.</summary>
    private const float Tolerance = 0.0001f;

    /// <summary>Every rise with every run of D-346, for a theory.</summary>
    public static TheoryData<RampRise, int> Courses => RampCourse.EveryRiseAndRun();

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

        MeshData mesh = GreedyMesher.MeshChunk(grid, 0, 0, RepositoryTextures.Tiles);

        Assert.Equal(6, mesh.QuadCount);
        Assert.Equal(6 * MeshData.QuadVertices, mesh.Positions.Count);
        Assert.Equal(6 * MeshData.QuadIndices, mesh.Indices.Count);
        foreach (Vector3 normal in BoxNormals())
        {
            Assert.Equal(MeshData.QuadVertices, CountVertices(mesh, normal));
        }
    }

    /// <summary>PR-13 exit test 4. One grid gives one buffer, twice over, on every chunk of floor 1 of seed 1 and on every ramp course.</summary>
    [Fact]
    public void MesherIsDeterministic()
    {
        List<VoxelGrid> grids = [TestWorld.NewLoop(1).Grid];
        foreach (RampRise rise in Enum.GetValues<RampRise>())
        {
            for (int run = Ramp.SteepestRun; run <= Ramp.ShallowestRun; run++)
            {
                grids.Add(NarrowCourse(rise, run).Grid);
            }
        }

        foreach (VoxelGrid grid in grids)
        {
            int triangles = 0;
            for (int chunkZ = 0; chunkZ < ChunkLayout.CountZ(grid); chunkZ++)
            {
                for (int chunkX = 0; chunkX < ChunkLayout.CountX(grid); chunkX++)
                {
                    MeshData first = GreedyMesher.MeshChunk(grid, chunkX, chunkZ, RepositoryTextures.Tiles);
                    MeshData second = GreedyMesher.MeshChunk(grid, chunkX, chunkZ, RepositoryTextures.Tiles);
                    Assert.Equal(first.Positions, second.Positions);
                    Assert.Equal(first.Normals, second.Normals);
                    Assert.Equal(first.Colors, second.Colors);
                    Assert.Equal(first.Uvs, second.Uvs);
                    Assert.Equal(first.TileOrigins, second.TileOrigins);
                    Assert.Equal(first.Indices, second.Indices);
                    triangles += first.TriangleCount;
                }
            }

            Assert.True(triangles > 0, "A grid of the test shows no face.");
        }
    }

    /// <summary>
    /// PR-13 exit test 5 and PR-65 exit test 2. A maximum floor of D-164 takes 64 chunks, which is the world budget of
    /// D-291, and the player model is the one entity mesh on top. Every chunk of the floor meshes with a ramp in each
    /// chamber, and the three real templates stay under the budget too.
    /// </summary>
    [Fact]
    public void MeshBudgetTest()
    {
        VoxelGrid maximum = MaximumFloor();
        int worldMeshes = 0;
        int slopeTriangles = 0;
        for (int chunkZ = 0; chunkZ < ChunkLayout.CountZ(maximum); chunkZ++)
        {
            for (int chunkX = 0; chunkX < ChunkLayout.CountX(maximum); chunkX++)
            {
                MeshData mesh = GreedyMesher.MeshChunk(maximum, chunkX, chunkZ, RepositoryTextures.Tiles);
                if (mesh.TriangleCount > 0)
                {
                    worldMeshes++;
                }

                slopeTriangles += CountSlopeTriangles(mesh);
            }
        }

        const int entityMeshes = 1;
        Assert.True(slopeTriangles > 0, "The maximum floor shows no slope, so the budget test reads no ramp.");
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

        MeshData mesh = GreedyMesher.MeshChunk(grid, 0, 0, RepositoryTextures.Tiles);

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

        Assert.Equal(0, GreedyMesher.MeshChunk(grid, 0, 0, RepositoryTextures.Tiles).TriangleCount);
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

        MeshData mesh = GreedyMesher.MeshChunk(grid, 0, 0, RepositoryTextures.Tiles);

        // The surface of the pool at y = 2, and the stone floor around it at y = 2.
        Assert.Equal(RepositoryTextures.Tiles.Origin(BlockId.StillWater), TileAt(mesh, new Vector3(4.5f, 2.0f, 4.5f), Vector3.Up));

        // The stone under the pool shows its top face against the water, and the pool walls show their sides.
        Assert.Equal(RepositoryTextures.Tiles.Origin(BlockId.RawStone), TileAt(mesh, new Vector3(4.5f, 1.0f, 4.5f), Vector3.Up));
        Assert.Equal(RepositoryTextures.Tiles.Origin(BlockId.RawStone), TileAt(mesh, new Vector3(4.0f, 1.5f, 4.5f), Vector3.Right));

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
        MeshData first = GreedyMesher.MeshChunk(grid, 0, 0, RepositoryTextures.Tiles);
        MeshData second = GreedyMesher.MeshChunk(grid, 1, 0, RepositoryTextures.Tiles);

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

    /// <summary>A ramp across a chunk border shows no end at the border, and each chunk holds the slope of its own cells.</summary>
    [Fact]
    public void ChunksShareNoSeamOnARamp()
    {
        VoxelGrid grid = new(20, 4, 4);
        for (int z = 0; z < 4; z++)
        {
            for (int x = 0; x < 20; x++)
            {
                grid.Set(x, 0, z, BlockId.RawStone);
            }

            for (int x = 14; x < 18; x++)
            {
                grid.Set(x, 1, z, new Ramp(RampRise.PlusX, 4, x - 14).Id);
            }

            grid.Set(18, 1, z, BlockId.RawStone);
            grid.Set(19, 1, z, BlockId.RawStone);
        }

        MeshData first = GreedyMesher.MeshChunk(grid, 0, 0, RepositoryTextures.Tiles);
        MeshData second = GreedyMesher.MeshChunk(grid, 1, 0, RepositoryTextures.Tiles);

        // Places 1 and 2 meet at the border at one height, so neither chunk shows an end there.
        Assert.False(HasFace(first, new Vector3(16.0f, 1.25f, 2.0f), Vector3.Right));
        Assert.False(HasFace(second, new Vector3(16.0f, 1.25f, 2.0f), Vector3.Left));

        Vector3 slopeNormal = new Vector3(-1.0f, 4.0f, 0.0f).Normalized();
        Assert.True(HasFace(first, new Vector3(15.5f, 1.375f, 2.0f), slopeNormal));
        Assert.True(HasFace(second, new Vector3(16.5f, 1.625f, 2.0f), slopeNormal));
        Assert.False(HasFace(first, new Vector3(16.5f, 1.625f, 2.0f), slopeNormal));
    }

    /// <summary>Every triangle runs clockwise as seen from its normal, which is the front-face order of the engine.</summary>
    [Fact]
    public void QuadsAreClockwiseFromOutside()
    {
        VoxelGrid grid = TestWorld.FlatFloor();
        grid.Set(4, 1, 4, BlockId.RawStone);
        grid.Set(4, 2, 4, BlockId.RawStone);
        MeshData mesh = GreedyMesher.MeshChunk(grid, 0, 0, RepositoryTextures.Tiles);

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

        MeshData mesh = GreedyMesher.MeshChunk(grid, 0, 0, RepositoryTextures.Tiles);

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
            Assert.Equal(RepositoryTextures.Tiles.Origin(BlockId.OreVein), mesh.TileOrigins[vertex]);
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
        ContextException error = Assert.Throws<ContextException>(() => GreedyMesher.MeshChunk(grid, 2, 0, RepositoryTextures.Tiles));
        Assert.Contains("(2, 0)", error.Message, System.StringComparison.Ordinal);
        Assert.Throws<ContextException>(() => GreedyMesher.MeshChunk(grid, 0, -1, RepositoryTextures.Tiles));
    }

    /// <summary>The origin of each block canvas is its place in the committed layout, as a fraction of the atlas of 512 pixels (D-85, D-505, D-506).</summary>
    [Fact]
    public void TileOriginsFollowTheLayout()
    {
        foreach (BlockPlace place in RepositoryTextures.Layout.Blocks)
        {
            Vector2 expected = new((float)place.At.X / AtlasLayout.AtlasPixels, (float)place.At.Y / AtlasLayout.AtlasPixels);
            Assert.Equal(expected, RepositoryTextures.Tiles.Origin((BlockId)place.Block));
        }

        Assert.Equal(1.0f / 16.0f, BlockTiles.Size);
        ContextException air = Assert.Throws<ContextException>(() => RepositoryTextures.Tiles.Origin(BlockId.Air));
        Assert.Contains("no canvas for the block", air.Message, System.StringComparison.Ordinal);
    }

    /// <summary>
    /// PR-65 exit test 1. For each slope and each rise, a ramp three cells wide gives a raw stone slope over each cell and
    /// a side toward the air beside it. A wall beside the ramp keeps its face over the slope, and the faces that a ramp
    /// covers stay hidden: its side toward the wall, the floor under it, and the end of the landing (D-345, D-368).
    /// </summary>
    [Theory]
    [MemberData(nameof(Courses))]
    public void RampGivesItsSlopeAndSideFaces(RampRise rise, int run)
    {
        RampCourse course = NarrowCourse(rise, run);
        MeshData mesh = GreedyMesher.MeshChunk(course.Grid, 0, 0, RepositoryTextures.Tiles);
        Vector3 slopeNormal = SlopeNormal(course);
        Vector3 across = AcrossOf(course);
        Vector3 uphill = UphillOf(course);
        Vector2 rawStone = RepositoryTextures.Tiles.Origin(BlockId.RawStone);
        for (int place = 0; place < run; place++)
        {
            float along = RampCourse.RampStart + place + 0.5f;
            float slope = RampCourse.LowTop + ((place + 0.5f) / run);
            for (int cell = NarrowFirst; cell <= NarrowLast; cell++)
            {
                Assert.Equal(rawStone, TileAt(mesh, PointOf(course, along, slope, cell + 0.5f), slopeNormal));
                Assert.False(HasFace(mesh, PointOf(course, along, RampCourse.LowTop, cell + 0.5f), Vector3.Up), $"The floor under place {place} shows through the ramp.");
            }

            // Half way up the side of the cell, under the slope.
            float underSlope = RampCourse.LowTop + ((place + 0.5f) / run / 2.0f);
            Assert.Equal(rawStone, TileAt(mesh, PointOf(course, along, underSlope, NarrowFirst), -across));
            Assert.False(HasFace(mesh, PointOf(course, along, underSlope, NarrowLast + 1), across), $"The side of place {place} toward the wall shows.");
            Assert.Equal(RepositoryTextures.Tiles.Origin(BlockId.HewnStone), TileAt(mesh, PointOf(course, along, RampCourse.HighTop - 0.05f, NarrowLast + 1), -across));
        }

        // The floor at the foot shows. The top of the run and the end of the landing meet at one height, so neither shows.
        Assert.Equal(rawStone, TileAt(mesh, PointOf(course, RampCourse.RampStart - 0.5f, RampCourse.LowTop, RampCourse.Middle), Vector3.Up));
        Assert.False(HasFace(mesh, PointOf(course, course.HighStart, RampCourse.LowTop + 0.5f, RampCourse.Middle), -uphill));
        Assert.False(HasFace(mesh, PointOf(course, course.HighStart, RampCourse.LowTop + 0.5f, RampCourse.Middle), uphill));
        Assert.True(HasFace(mesh, PointOf(course, course.HighStart + 0.5f, RampCourse.HighTop, RampCourse.Middle), Vector3.Up));
    }

    /// <summary>
    /// Every triangle of a ramp course has an area and runs clockwise as seen from its normal. The side of the low end of
    /// the run comes to a point, so the course holds exactly one triangle face.
    /// </summary>
    [Theory]
    [MemberData(nameof(Courses))]
    public void RampTrianglesRunClockwiseWithArea(RampRise rise, int run)
    {
        MeshData mesh = GreedyMesher.MeshChunk(NarrowCourse(rise, run).Grid, 0, 0, RepositoryTextures.Tiles);

        for (int triangle = 0; triangle < mesh.Indices.Count; triangle += MeshData.TriangleVertices)
        {
            Vector3 a = mesh.Positions[mesh.Indices[triangle]];
            Vector3 b = mesh.Positions[mesh.Indices[triangle + 1]];
            Vector3 c = mesh.Positions[mesh.Indices[triangle + 2]];
            Vector3 normal = mesh.Normals[mesh.Indices[triangle]];
            Vector3 cross = (b - a).Cross(c - a);
            Assert.True(cross.Length() > Tolerance, $"The triangle at index {triangle} has no area.");
            Assert.True(cross.Dot(normal) < 0.0f, $"The triangle at index {triangle} runs counterclockwise as seen from its normal {normal}.");
        }

        int triangleFaces = (mesh.Positions.Count - (MeshData.QuadVertices * mesh.QuadCount)) / MeshData.TriangleVertices;
        Assert.Equal(1, triangleFaces);
    }

    /// <summary>Every vertex of a slope lies on the slope of the course, between the foot and the top.</summary>
    [Theory]
    [MemberData(nameof(Courses))]
    public void SlopeVerticesLieOnTheSlope(RampRise rise, int run)
    {
        RampCourse course = NarrowCourse(rise, run);
        MeshData mesh = GreedyMesher.MeshChunk(course.Grid, 0, 0, RepositoryTextures.Tiles);
        Vector3 slopeNormal = SlopeNormal(course);

        int slopeVertices = 0;
        for (int vertex = 0; vertex < mesh.Positions.Count; vertex++)
        {
            Vector3 normal = mesh.Normals[vertex];
            if (normal.Y <= Tolerance || normal.Y >= 1.0f - Tolerance)
            {
                continue;
            }

            slopeVertices++;
            Vector3 position = mesh.Positions[vertex];
            float along = AlongOf(course, position);
            Assert.True(normal.Dot(slopeNormal) > 1.0f - Tolerance, $"The slope normal {normal} is not the normal of the course {slopeNormal}.");
            Assert.InRange(along, RampCourse.RampStart - Tolerance, course.HighStart + Tolerance);
            float expected = RampCourse.LowTop + ((along - RampCourse.RampStart) / run);
            Assert.True(Math.Abs(position.Y - expected) < Tolerance, $"The slope vertex {position} is at height {position.Y}, and the slope there is at {expected}.");
        }

        Assert.True(slopeVertices > 0, "The course shows no slope.");
    }

    /// <summary>Every face of a ramp course keeps 32 texels per meter: between any two corners of a triangle, the face coordinate moves as far in tiles as the corners are apart in meters (D-308).</summary>
    [Theory]
    [MemberData(nameof(Courses))]
    public void EveryFaceKeepsTheTexelDensity(RampRise rise, int run)
    {
        MeshData mesh = GreedyMesher.MeshChunk(NarrowCourse(rise, run).Grid, 0, 0, RepositoryTextures.Tiles);

        for (int triangle = 0; triangle < mesh.Indices.Count; triangle += MeshData.TriangleVertices)
        {
            for (int first = 0; first < MeshData.TriangleVertices; first++)
            {
                int second = (first + 1) % MeshData.TriangleVertices;
                int one = mesh.Indices[triangle + first];
                int other = mesh.Indices[triangle + second];
                float meters = mesh.Positions[one].DistanceTo(mesh.Positions[other]);
                float tiles = mesh.Uvs[one].DistanceTo(mesh.Uvs[other]);
                Assert.True(Math.Abs(meters - tiles) < Tolerance, $"The edge from {mesh.Positions[one]} to {mesh.Positions[other]} is {meters} meters, and its face coordinate moves {tiles} tiles.");
            }
        }
    }

    /// <summary>
    /// The slope of a ramp across the whole course merges into three rectangles: the two cells at the edge of the world,
    /// which the rock outside darkens, and the cells between them.
    /// </summary>
    [Theory]
    [MemberData(nameof(Courses))]
    public void SlopesMergeAcrossTheWidth(RampRise rise, int run)
    {
        MeshData mesh = GreedyMesher.MeshChunk(new RampCourse(rise, run).Grid, 0, 0, RepositoryTextures.Tiles);

        Assert.Equal(3 * 2, CountSlopeTriangles(mesh));
    }

    /// <summary>
    /// A ramp darkens a vertex by the upper half of its run (D-81, D-367). The floor at the foot keeps the open level, and
    /// the slope darkens along a wall.
    /// </summary>
    [Fact]
    public void OcclusionReadsTheUpperHalfOfARamp()
    {
        Assert.True(AmbientOcclusion.Occludes(BlockId.RawStone));
        Assert.False(AmbientOcclusion.Occludes(BlockId.Air));
        Assert.False(AmbientOcclusion.Occludes(BlockId.StillWater));
        foreach (RampRise rise in Enum.GetValues<RampRise>())
        {
            for (int run = Ramp.SteepestRun; run <= Ramp.ShallowestRun; run++)
            {
                for (int place = 0; place < run; place++)
                {
                    bool upperHalf = (place + 0.5f) / run >= 0.5f;
                    Assert.Equal(upperHalf, AmbientOcclusion.Occludes(new Ramp(rise, run, place).Id));
                }

                RampCourse course = new(rise, run);
                Cell overFoot = course.CellAt(RampCourse.RampStart - 1, 1, 4);
                foreach (int du in new[] { -1, 1 })
                {
                    foreach (int dv in new[] { -1, 1 })
                    {
                        Assert.Equal(AmbientOcclusion.Open, AmbientOcclusion.CornerLevel(course.Grid, [overFoot.X, overFoot.Y, overFoot.Z], 0, 2, du, dv));
                    }
                }

                RampCourse narrow = NarrowCourse(rise, run);
                MeshData mesh = GreedyMesher.MeshChunk(narrow.Grid, 0, 0, RepositoryTextures.Tiles);
                Vector3 slopeNormal = SlopeNormal(narrow);
                Color open = AmbientOcclusion.ColorOf(AmbientOcclusion.Open);
                Assert.Equal(open, ColorAt(mesh, PointOf(narrow, RampCourse.RampStart, RampCourse.LowTop, NarrowFirst), slopeNormal));
                Assert.True(ColorAt(mesh, PointOf(narrow, RampCourse.RampStart, RampCourse.LowTop, NarrowLast + 1), slopeNormal).R < open.R, $"The slope of rise {rise} and run {run} does not darken at the wall.");
            }
        }
    }

    /// <summary>The shape of each side of a ramp follows its slope, and two neighbors of one ramp meet at one shape.</summary>
    [Fact]
    public void FaceShapesOfARampFollowItsSlope()
    {
        Ramp plusX = new(RampRise.PlusX, 3, 1);
        Assert.Equal(new FaceShape(8, 8), FaceShape.OfRamp(plusX, FaceDirection.PlusX));
        Assert.Equal(new FaceShape(4, 4), FaceShape.OfRamp(plusX, FaceDirection.MinusX));
        Assert.Equal(new FaceShape(4, 8), FaceShape.OfRamp(plusX, FaceDirection.PlusZ));
        Assert.Equal(new FaceShape(4, 8), FaceShape.OfRamp(plusX, FaceDirection.MinusZ));
        Assert.Equal(FaceShape.Empty, FaceShape.OfRamp(plusX, FaceDirection.Up));
        Assert.Equal(FaceShape.Full, FaceShape.OfRamp(plusX, FaceDirection.Down));

        Ramp minusZ = new(RampRise.MinusZ, 4, 0);
        Assert.Equal(new FaceShape(3, 3), FaceShape.OfRamp(minusZ, FaceDirection.MinusZ));
        Assert.True(FaceShape.OfRamp(minusZ, FaceDirection.PlusZ).IsEmpty);
        Assert.Equal(new FaceShape(3, 0), FaceShape.OfRamp(minusZ, FaceDirection.PlusX));

        Ramp minusX = new(RampRise.MinusX, 2, 1);
        Assert.Equal(FaceShape.Full, FaceShape.OfRamp(minusX, FaceDirection.MinusX));
        Assert.Equal(new FaceShape(12, 6), FaceShape.OfRamp(minusX, FaceDirection.PlusZ));

        Ramp plusZ = new(RampRise.PlusZ, 2, 0);
        Assert.Equal(new FaceShape(0, 6), FaceShape.OfRamp(plusZ, FaceDirection.MinusX));

        foreach (RampRise rise in Enum.GetValues<RampRise>())
        {
            int uphill = FaceDirection.Uphill(rise);
            for (int run = Ramp.SteepestRun; run <= Ramp.ShallowestRun; run++)
            {
                for (int place = 0; place < run; place++)
                {
                    Ramp ramp = new(rise, run, place);
                    for (int direction = 0; direction < FaceDirection.Count; direction++)
                    {
                        if (FaceDirection.Axis(direction) != 1 && FaceDirection.Axis(direction) != FaceDirection.Axis(uphill))
                        {
                            Assert.Equal(FaceShape.OfRamp(ramp, direction), FaceShape.OfRamp(ramp, FaceDirection.Opposite(direction)));
                        }
                    }

                    if (place + 1 < run)
                    {
                        Assert.Equal(FaceShape.OfRamp(ramp, uphill), FaceShape.OfRamp(new Ramp(rise, run, place + 1), FaceDirection.Opposite(uphill)));
                    }
                }
            }
        }

        Assert.Throws<ContextException>(() => FaceShape.OfRamp(plusX, FaceDirection.Count));
    }

    /// <summary>A face shows unless the side of the neighbor covers it. The PR-13 rules for whole blocks stand.</summary>
    [Fact]
    public void FaceVisibleReadsTheSideOfTheNeighbor()
    {
        Assert.True(GreedyMesher.FaceVisible(BlockId.RawStone, BlockId.Air, FaceDirection.PlusX));
        Assert.False(GreedyMesher.FaceVisible(BlockId.RawStone, BlockId.HewnStone, FaceDirection.PlusX));
        Assert.True(GreedyMesher.FaceVisible(BlockId.RawStone, BlockId.StillWater, FaceDirection.Up));
        Assert.False(GreedyMesher.FaceVisible(BlockId.StillWater, BlockId.StillWater, FaceDirection.PlusX));
        Assert.True(GreedyMesher.FaceVisible(BlockId.StillWater, BlockId.Air, FaceDirection.Up));
        Assert.False(GreedyMesher.FaceVisible(BlockId.Air, BlockId.RawStone, FaceDirection.PlusX));

        BlockId foot = new Ramp(RampRise.PlusX, 2, 0).Id;
        BlockId top = new Ramp(RampRise.PlusX, 2, 1).Id;

        // A block beside a ramp shows over the slope, and the side of the ramp toward the block hides.
        Assert.True(GreedyMesher.FaceVisible(BlockId.RawStone, foot, FaceDirection.MinusZ));
        Assert.False(GreedyMesher.FaceVisible(foot, BlockId.RawStone, FaceDirection.PlusZ));

        // The top of a run and a block past it meet at the top of the row. The low end of place 1 is half a block high.
        Assert.False(GreedyMesher.FaceVisible(BlockId.RawStone, top, FaceDirection.MinusX));
        Assert.False(GreedyMesher.FaceVisible(top, BlockId.RawStone, FaceDirection.PlusX));
        Assert.True(GreedyMesher.FaceVisible(BlockId.RawStone, top, FaceDirection.PlusX));

        // The bottom of a ramp covers the block under it, and a block over a ramp shows its bottom.
        Assert.False(GreedyMesher.FaceVisible(BlockId.RawStone, foot, FaceDirection.Up));
        Assert.False(GreedyMesher.FaceVisible(foot, BlockId.RawStone, FaceDirection.Down));
        Assert.True(GreedyMesher.FaceVisible(BlockId.RawStone, foot, FaceDirection.Down));
        Assert.False(GreedyMesher.FaceVisible(foot, BlockId.Air, FaceDirection.Up));

        // Two cells of one ramp side by side share no face, and two places of one run share no end.
        Assert.False(GreedyMesher.FaceVisible(foot, foot, FaceDirection.PlusZ));
        Assert.False(GreedyMesher.FaceVisible(foot, top, FaceDirection.PlusX));
        Assert.False(GreedyMesher.FaceVisible(top, foot, FaceDirection.MinusX));

        // Water hides nothing, and a ramp does not hide the whole face of the water beside it.
        Assert.True(GreedyMesher.FaceVisible(BlockId.StillWater, foot, FaceDirection.PlusZ));
        Assert.True(GreedyMesher.FaceVisible(foot, BlockId.StillWater, FaceDirection.PlusZ));
    }

    /// <summary>The sweep takes the widest run first, then the tallest stack, and it clears the mask.</summary>
    [Fact]
    public void SweepTakesTheWidestRunThenTheTallestStack()
    {
        int[] mask = [1, 1, 2, 1, 1, 2, 0, 1, 1];

        List<MaskRectangle<int>> rectangles = GreedySweep.Rectangles(mask, 3, 3);

        Assert.Equal(
            new[]
            {
                new MaskRectangle<int>(0, 0, 2, 2, 1),
                new MaskRectangle<int>(2, 0, 1, 2, 2),
                new MaskRectangle<int>(1, 2, 2, 1, 1),
            },
            rectangles);
        Assert.All(mask, value => Assert.Equal(0, value));
        Assert.Throws<ContextException>(() => GreedySweep.Rectangles(new int[5], 3, 2));
    }

    /// <summary>A maximum floor of D-164: rock with a grid of chambers carved out, so every chunk shows faces, and a ramp of 1:3 onto a landing on the floor of each chamber.</summary>
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
                    bool rampRow = chamber && y == 2 && z % 8 >= 3 && z % 8 <= 6;
                    BlockId block = chamber ? BlockId.Air : BlockId.RawStone;
                    if (rampRow && x % 8 >= 3 && x % 8 <= 5)
                    {
                        block = new Ramp(RampRise.PlusX, 3, (x % 8) - 3).Id;
                    }
                    else if (rampRow && x % 8 == 6)
                    {
                        block = BlockId.RawStone;
                    }

                    grid.Set(x, y, z, block);
                }
            }
        }

        return grid;
    }

    /// <summary>
    /// A ramp course with the ramp narrowed to the cells <see cref="NarrowFirst"/> to <see cref="NarrowLast"/> across.
    /// Air stands on the low side across, and a hewn stone wall two blocks high stands on the high side, along the ramp.
    /// </summary>
    private static RampCourse NarrowCourse(RampRise rise, int run)
    {
        RampCourse course = new(rise, run);
        for (int along = RampCourse.RampStart; along < course.HighStart; along++)
        {
            for (int across = 0; across < RampCourse.Width; across++)
            {
                if (across >= NarrowFirst && across <= NarrowLast)
                {
                    continue;
                }

                BlockId block = across == NarrowLast + 1 ? BlockId.HewnStone : BlockId.Air;
                Cell lower = course.CellAt(along, 1, across);
                Cell upper = course.CellAt(along, 2, across);
                course.Grid.Set(lower.X, lower.Y, lower.Z, block);
                course.Grid.Set(upper.X, upper.Y, upper.Z, block);
            }
        }

        return course;
    }

    /// <summary>The unit normal of the slope of a course: up by the run, and back against the rise by one.</summary>
    private static Vector3 SlopeNormal(RampCourse course)
    {
        WhatYouCarry.Core.Physics.Vector3 uphill = course.Uphill;
        return new Vector3(-uphill.X, course.Run, -uphill.Z).Normalized();
    }

    /// <summary>The engine vector of a point of the course frame.</summary>
    private static Vector3 PointOf(RampCourse course, float along, float height, float across)
    {
        WhatYouCarry.Core.Physics.Vector3 point = course.Point(along, height, across);
        return new Vector3(point.X, point.Y, point.Z);
    }

    /// <summary>The engine vector of the unit horizontal vector toward the rise of a course.</summary>
    private static Vector3 UphillOf(RampCourse course)
    {
        WhatYouCarry.Core.Physics.Vector3 uphill = course.Uphill;
        return new Vector3(uphill.X, uphill.Y, uphill.Z);
    }

    /// <summary>The engine vector of the unit horizontal vector across the rise of a course.</summary>
    private static Vector3 AcrossOf(RampCourse course)
    {
        WhatYouCarry.Core.Physics.Vector3 across = course.Across;
        return new Vector3(across.X, across.Y, across.Z);
    }

    /// <summary>The distance along the rise of a course of an engine point.</summary>
    private static float AlongOf(RampCourse course, Vector3 position)
    {
        return course.AlongOf(new WhatYouCarry.Core.Physics.Vector3(position.X, position.Y, position.Z));
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

    /// <summary>The count of triangles whose normal is neither level nor upright: the triangles of the slopes.</summary>
    private static int CountSlopeTriangles(MeshData mesh)
    {
        int count = 0;
        for (int triangle = 0; triangle < mesh.Indices.Count; triangle += MeshData.TriangleVertices)
        {
            float normalY = mesh.Normals[mesh.Indices[triangle]].Y;
            if (normalY > Tolerance && normalY < 1.0f - Tolerance)
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>The color of the vertex at a position whose normal is the given unit normal.</summary>
    private static Color ColorAt(MeshData mesh, Vector3 position, Vector3 normal)
    {
        for (int vertex = 0; vertex < mesh.Positions.Count; vertex++)
        {
            if (mesh.Positions[vertex].DistanceTo(position) < Tolerance && mesh.Normals[vertex].Dot(normal) > 1.0f - Tolerance)
            {
                return mesh.Colors[vertex];
            }
        }

        throw new InvalidOperationException($"The mesh has no vertex at {position} with normal {normal}.");
    }

    /// <summary>Answers whether a triangle with the unit normal covers the point.</summary>
    private static bool HasFace(MeshData mesh, Vector3 point, Vector3 normal)
    {
        return FirstVertexOfFaceAt(mesh, point, normal) >= 0;
    }

    /// <summary>The tile origin of the triangle with the unit normal that covers the point.</summary>
    private static Vector2 TileAt(MeshData mesh, Vector3 point, Vector3 normal)
    {
        int vertex = FirstVertexOfFaceAt(mesh, point, normal);
        if (vertex < 0)
        {
            throw new InvalidOperationException($"The mesh has no face at {point} with normal {normal}.");
        }

        return mesh.TileOrigins[vertex];
    }

    /// <summary>
    /// The first vertex of the first triangle whose normal is the unit normal and which covers the point, or -1 when no
    /// triangle does. The point covers a triangle when it lies in its plane and inside its three edges.
    /// </summary>
    private static int FirstVertexOfFaceAt(MeshData mesh, Vector3 point, Vector3 normal)
    {
        for (int triangle = 0; triangle < mesh.Indices.Count; triangle += MeshData.TriangleVertices)
        {
            int first = mesh.Indices[triangle];
            if (mesh.Normals[first].Dot(normal) < 1.0f - Tolerance)
            {
                continue;
            }

            Vector3 a = mesh.Positions[first];
            if (Math.Abs((point - a).Dot(normal)) > Tolerance)
            {
                continue;
            }

            // The point as a + s (b - a) + t (c - a), by the dot products of the two edges.
            Vector3 edgeB = mesh.Positions[mesh.Indices[triangle + 1]] - a;
            Vector3 edgeC = mesh.Positions[mesh.Indices[triangle + 2]] - a;
            Vector3 toPoint = point - a;
            float bb = edgeB.Dot(edgeB);
            float bc = edgeB.Dot(edgeC);
            float cc = edgeC.Dot(edgeC);
            float pb = toPoint.Dot(edgeB);
            float pc = toPoint.Dot(edgeC);
            float determinant = (bb * cc) - (bc * bc);
            float s = ((cc * pb) - (bc * pc)) / determinant;
            float t = ((bb * pc) - (bc * pb)) / determinant;
            if (s >= -Tolerance && t >= -Tolerance && s + t <= 1.0f + Tolerance)
            {
                return first;
            }
        }

        return -1;
    }
}
