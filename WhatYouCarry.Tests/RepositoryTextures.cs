using System.Collections.Generic;
using System.IO;
using WhatYouCarry.Assets;
using WhatYouCarry.Game.World;

namespace WhatYouCarry.Tests;

/// <summary>The committed texture layout of the repository, and the block canvases of it, for the mesher and model tests (D-505).</summary>
internal static class RepositoryTextures
{
    /// <summary>The layout that <c>content/textures/layout.json</c> holds.</summary>
    public static TextureLayout Layout { get; } = TextureLayout.Parse(
        AssetPaths.LayoutFile,
        File.ReadAllBytes(Path.Combine(RepositoryRoot.Find(), "content", AssetPaths.LayoutFile)));

    /// <summary>The block canvases of the committed layout.</summary>
    public static BlockTiles Tiles { get; } = new(Layout);

    /// <summary>
    /// A layout that places every face of one test model at the top left of the atlas. A model of a test fixture has
    /// no paint file, so the committed layout does not name it.
    /// </summary>
    public static TextureLayout LayoutFor(BlockbenchModel model)
    {
        List<FacePlace> faces = [];
        foreach (ModelBox box in model.Boxes)
        {
            for (int side = 0; side < BoxFaces.Names.Count; side++)
            {
                (int width, int height) = BoxFaces.CanvasTexels(box, (BoxSide)side);
                faces.Add(new FacePlace(model.Path, box.Name, (BoxSide)side, "test", new AtlasRect(1, 1, System.Math.Max(width, 1), System.Math.Max(height, 1))));
            }
        }

        return new TextureLayout([], faces);
    }
}
