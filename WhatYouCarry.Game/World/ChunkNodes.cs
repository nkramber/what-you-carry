using System.Collections.Generic;
using Godot;
using WhatYouCarry.Core.World;
using WhatYouCarry.Game.Render;

namespace WhatYouCarry.Game.World;

/// <summary>
/// The mesh instances of one floor: one per chunk that shows a face, with the world material on each (D-291).
/// A chunk with no visible face, such as one deep in rock, gets no node.
/// </summary>
public static class ChunkNodes
{
    /// <summary>The mesh instances of every chunk of the grid that shows a face, in chunk order.</summary>
    public static IReadOnlyList<MeshInstance3D> Build(VoxelGrid grid, Material material)
    {
        List<MeshInstance3D> instances = [];
        for (int chunkZ = 0; chunkZ < ChunkLayout.CountZ(grid); chunkZ++)
        {
            for (int chunkX = 0; chunkX < ChunkLayout.CountX(grid); chunkX++)
            {
                MeshData data = GreedyMesher.MeshChunk(grid, chunkX, chunkZ);
                if (data.QuadCount == 0)
                {
                    continue;
                }

                instances.Add(new MeshInstance3D
                {
                    Mesh = ArrayMeshBuilder.Build(data),
                    MaterialOverride = material,
                });
            }
        }

        return instances;
    }
}
