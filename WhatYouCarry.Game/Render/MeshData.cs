using System.Collections.Generic;
using Godot;

namespace WhatYouCarry.Game.Render;

/// <summary>
/// The vertex buffers of one mesh, before the engine sees them: a position, a normal, a color, and two texture
/// coordinates per vertex, and the triangle indices. The mesher and the model loader fill one, a test reads it,
/// and <see cref="ArrayMeshBuilder"/> hands it to the engine.
/// </summary>
/// <remarks>
/// <para>
/// The engine draws a triangle whose vertices run clockwise as seen from the front. Every quad here follows that
/// order, so a face shows from the side that its normal points to and the engine culls it from the other side.
/// </para>
/// <para>
/// The color carries the ambient occlusion of the vertex (D-81). The first texture coordinate is the local
/// coordinate of the face, which repeats past one on a merged face, and the second is the origin of the tile in
/// the atlas (D-85). The world shader adds the fraction of the first to the second.
/// </para>
/// </remarks>
public sealed class MeshData
{
    /// <summary>The count of vertices of one quad.</summary>
    public const int QuadVertices = 4;

    /// <summary>The count of indices of one quad: two triangles.</summary>
    public const int QuadIndices = 6;

    private readonly List<Vector3> positions = [];
    private readonly List<Vector3> normals = [];
    private readonly List<Color> colors = [];
    private readonly List<Vector2> uvs = [];
    private readonly List<Vector2> tileOrigins = [];
    private readonly List<int> indices = [];

    /// <summary>The position of each vertex, in meters.</summary>
    public IReadOnlyList<Vector3> Positions => this.positions;

    /// <summary>The unit normal of each vertex.</summary>
    public IReadOnlyList<Vector3> Normals => this.normals;

    /// <summary>The color of each vertex. The world mesher writes the ambient occlusion here.</summary>
    public IReadOnlyList<Color> Colors => this.colors;

    /// <summary>The first texture coordinate of each vertex.</summary>
    public IReadOnlyList<Vector2> Uvs => this.uvs;

    /// <summary>The second texture coordinate of each vertex: the tile origin in the atlas for a world face.</summary>
    public IReadOnlyList<Vector2> TileOrigins => this.tileOrigins;

    /// <summary>The vertex indices of every triangle, three per triangle.</summary>
    public IReadOnlyList<int> Indices => this.indices;

    /// <summary>The count of quads. Every face of a box or a chunk is one quad.</summary>
    public int QuadCount => this.indices.Count / QuadIndices;

    /// <summary>
    /// Adds one quad of four vertices. The corners run clockwise as seen from the side that the normal points to.
    /// The diagonal splits the quad from corner 0 to corner 2, or from corner 1 to corner 3 when
    /// <paramref name="flipDiagonal"/> is set, which the mesher uses to keep the occlusion gradient smooth.
    /// </summary>
    public void AddQuad(Vector3[] corners, Vector3 normal, Color[] cornerColors, Vector2[] cornerUvs, Vector2 tileOrigin, bool flipDiagonal)
    {
        int first = this.positions.Count;
        for (int corner = 0; corner < QuadVertices; corner++)
        {
            this.positions.Add(corners[corner]);
            this.normals.Add(normal);
            this.colors.Add(cornerColors[corner]);
            this.uvs.Add(cornerUvs[corner]);
            this.tileOrigins.Add(tileOrigin);
        }

        if (flipDiagonal)
        {
            this.indices.Add(first + 1);
            this.indices.Add(first + 2);
            this.indices.Add(first + 3);
            this.indices.Add(first + 1);
            this.indices.Add(first + 3);
            this.indices.Add(first + 0);
        }
        else
        {
            this.indices.Add(first + 0);
            this.indices.Add(first + 1);
            this.indices.Add(first + 2);
            this.indices.Add(first + 0);
            this.indices.Add(first + 2);
            this.indices.Add(first + 3);
        }
    }
}
