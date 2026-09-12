using Godot;

namespace WhatYouCarry.Game.Render;

/// <summary>
/// Hands one <see cref="MeshData"/> to the engine as an <c>ArrayMesh</c> with one surface. This is the one place
/// where the vertex buffers of the mesher and the model loader meet the engine, so every test reads the buffers
/// and none reads a mesh.
/// </summary>
public static class ArrayMeshBuilder
{
    /// <summary>One mesh with one triangle surface from the buffers. A mesh with no quad has no surface.</summary>
    public static ArrayMesh Build(MeshData data)
    {
        ArrayMesh mesh = new();
        if (data.QuadCount == 0)
        {
            return mesh;
        }

        int count = data.Positions.Count;
        Vector3[] positions = new Vector3[count];
        Vector3[] normals = new Vector3[count];
        Color[] colors = new Color[count];
        Vector2[] uvs = new Vector2[count];
        Vector2[] tileOrigins = new Vector2[count];
        for (int index = 0; index < count; index++)
        {
            positions[index] = data.Positions[index];
            normals[index] = data.Normals[index];
            colors[index] = data.Colors[index];
            uvs[index] = data.Uvs[index];
            tileOrigins[index] = data.TileOrigins[index];
        }

        int[] indices = new int[data.Indices.Count];
        for (int index = 0; index < indices.Length; index++)
        {
            indices[index] = data.Indices[index];
        }

        Godot.Collections.Array arrays = [];
        arrays.Resize((int)Mesh.ArrayType.Max);
        arrays[(int)Mesh.ArrayType.Vertex] = positions;
        arrays[(int)Mesh.ArrayType.Normal] = normals;
        arrays[(int)Mesh.ArrayType.Color] = colors;
        arrays[(int)Mesh.ArrayType.TexUV] = uvs;
        arrays[(int)Mesh.ArrayType.TexUV2] = tileOrigins;
        arrays[(int)Mesh.ArrayType.Index] = indices;
        mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
        return mesh;
    }
}
