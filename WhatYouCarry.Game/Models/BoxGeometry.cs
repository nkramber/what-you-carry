using Godot;
using WhatYouCarry.Assets;
using WhatYouCarry.Game.Render;

namespace WhatYouCarry.Game.Models;

/// <summary>
/// The six quads of one box, relative to its pivot, so the mesh instance sits at the pivot and a bone rotation
/// of PR-15 turns the box about it (D-87). Each face takes its texture rectangle from the model file.
/// </summary>
/// <remarks>
/// The corners of each face run clockwise as seen from outside, which is the front-face order of the engine.
/// The texture of a vertical face stands upright, with its top edge at the top of the box, and the up face has
/// its top edge at north (minus Z). PR-14 confirms the orientation of every face on the contact sheet (D-83).
/// </remarks>
public static class BoxGeometry
{
    /// <summary>The count of faces of a box.</summary>
    public const int Faces = 6;

    /// <summary>The white color of a model vertex. A model has no vertex occlusion.</summary>
    public static readonly Color Unshaded = new(1.0f, 1.0f, 1.0f);

    /// <summary>The unit normal of each side, in the order of <see cref="BoxSide"/>.</summary>
    public static readonly Vector3[] Normals =
    [
        new(0.0f, 0.0f, -1.0f),
        new(1.0f, 0.0f, 0.0f),
        new(0.0f, 0.0f, 1.0f),
        new(-1.0f, 0.0f, 0.0f),
        new(0.0f, 1.0f, 0.0f),
        new(0.0f, -1.0f, 0.0f),
    ];

    /// <summary>The mesh of one box: six quads, twenty-four vertices, relative to the pivot of the box.</summary>
    public static MeshData Build(ModelBox box)
    {
        MeshData data = new();
        Vector3 low = RenderInterpolation.ToGodot(box.From - box.Pivot);
        Vector3 high = RenderInterpolation.ToGodot(box.To - box.Pivot);
        for (int side = 0; side < Faces; side++)
        {
            Vector3[] corners = Corners((BoxSide)side, low, high);
            FaceUv uv = box.Faces[side];
            Vector2[] uvs =
            [
                new(uv.LowU, uv.LowV),
                new(uv.HighU, uv.LowV),
                new(uv.HighU, uv.HighV),
                new(uv.LowU, uv.HighV),
            ];
            data.AddQuad(corners, Normals[side], [Unshaded, Unshaded, Unshaded, Unshaded], uvs, Vector2.Zero, flipDiagonal: false);
        }

        return data;
    }

    /// <summary>
    /// The four corners of one side, clockwise as seen from outside: the top left, the top right, the bottom
    /// right, and the bottom left of the face as the texture shows it.
    /// </summary>
    public static Vector3[] Corners(BoxSide side, Vector3 low, Vector3 high)
    {
        switch (side)
        {
            case BoxSide.North:
                return [new(high.X, high.Y, low.Z), new(low.X, high.Y, low.Z), new(low.X, low.Y, low.Z), new(high.X, low.Y, low.Z)];
            case BoxSide.East:
                return [new(high.X, high.Y, high.Z), new(high.X, high.Y, low.Z), new(high.X, low.Y, low.Z), new(high.X, low.Y, high.Z)];
            case BoxSide.South:
                return [new(low.X, high.Y, high.Z), new(high.X, high.Y, high.Z), new(high.X, low.Y, high.Z), new(low.X, low.Y, high.Z)];
            case BoxSide.West:
                return [new(low.X, high.Y, low.Z), new(low.X, high.Y, high.Z), new(low.X, low.Y, high.Z), new(low.X, low.Y, low.Z)];
            case BoxSide.Up:
                return [new(low.X, high.Y, low.Z), new(high.X, high.Y, low.Z), new(high.X, high.Y, high.Z), new(low.X, high.Y, high.Z)];
            default:
                return [new(low.X, low.Y, high.Z), new(high.X, low.Y, high.Z), new(high.X, low.Y, low.Z), new(low.X, low.Y, low.Z)];
        }
    }
}
