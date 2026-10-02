using Godot;
using WhatYouCarry.Assets;
using WhatYouCarry.Game.Render;

namespace WhatYouCarry.Game.Models;

/// <summary>
/// The six quads of one box, relative to its pivot, so the mesh instance sits at the pivot and a bone rotation
/// of PR-15 turns the box about it (D-87). Each face reads its canvas in the atlas, from the texture layout (D-505).
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

    /// <summary>
    /// The mesh of one box: a quad for each drawn side, six for a box with no undrawn side, relative to the pivot of the
    /// box. Each face reads the top left part of its canvas at 64 texels per meter (D-308, D-603): a face of 43.2 texels
    /// reads that much of a canvas of 44.
    /// </summary>
    /// <param name="modelPath">The path of the model file that holds the box, which the layout names each face by.</param>
    /// <param name="box">The box.</param>
    /// <param name="layout">The texture layout of the atlas.</param>
    /// <exception cref="WhatYouCarry.Core.Logging.ContextException">The layout has no canvas for a drawn face of the box.</exception>
    public static MeshData Build(string modelPath, ModelBox box, TextureLayout layout)
    {
        return Build(modelPath, box, layout, 0.0f);
    }

    /// <summary>
    /// The mesh of one box, grown by an outset on each side. An armor overlay grows, so a face in the plane of a body
    /// face draws in front of it (D-300). Each face reads the canvas of the box at its own size.
    /// </summary>
    /// <param name="modelPath">The path of the model file that holds the box, which the layout names each face by.</param>
    /// <param name="box">The box.</param>
    /// <param name="layout">The texture layout of the atlas.</param>
    /// <param name="outset">The growth of the mesh on each side, in meters, from zero.</param>
    /// <exception cref="WhatYouCarry.Core.Logging.ContextException">The layout has no canvas for a drawn face of the box.</exception>
    public static MeshData Build(string modelPath, ModelBox box, TextureLayout layout, float outset)
    {
        MeshData data = new();
        Vector3 grow = new(outset, outset, outset);
        Vector3 low = RenderInterpolation.ToGodot(box.From - box.Pivot) - grow;
        Vector3 high = RenderInterpolation.ToGodot(box.To - box.Pivot) + grow;
        for (int side = 0; side < Faces; side++)
        {
            // An undrawn face has no quad, so the box under an open head piece shows (D-767).
            if (!box.IsDrawn((BoxSide)side))
            {
                continue;
            }

            Vector3[] corners = Corners((BoxSide)side, low, high);
            AtlasRect canvas = layout.Face(modelPath, box.Name, (BoxSide)side);
            (float width, float height) = BoxFaces.Texels(box, (BoxSide)side);
            float lowU = (float)canvas.X / AtlasLayout.AtlasPixels;
            float lowV = (float)canvas.Y / AtlasLayout.AtlasPixels;
            float highU = (canvas.X + width) / AtlasLayout.AtlasPixels;
            float highV = (canvas.Y + height) / AtlasLayout.AtlasPixels;
            Vector2[] uvs =
            [
                new(lowU, lowV),
                new(highU, lowV),
                new(highU, highV),
                new(lowU, highV),
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
