using System.Collections.Generic;
using Godot;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.World;
using WhatYouCarry.Game.Models;
using WhatYouCarry.Game.Render;
using WhatYouCarry.Game.World;

namespace WhatYouCarry.Game.Review;

/// <summary>The nodes of the contact sheet: the viewport that renders each shot, its camera, and the lantern that each shot moves.</summary>
public sealed record ContactSheetNodes(SubViewport Viewport, Camera3D Camera, OmniLight3D Lantern);

/// <summary>
/// Builds the scene of the contact sheet (D-306) in a viewport with a world of its own: each block in a small grid
/// and each ramp in its ramp scene through the greedy mesher and the world material, the two bodies with the sword in
/// the hand and the model material (D-336), the scene light of play (D-678), and the camera of play. Every subject stands at the place
/// that its shot gives. Each model shot draws the model that it names, with the sword in the hand (D-673).
/// </summary>
public static class ContactSheetScene
{
    /// <summary>The side of the small grid of one block. The block fills the middle cell, with air on every side, so all six faces show.</summary>
    public const int GridSide = 3;

    private const string NoSceneModel = "A shot of the contact sheet names a model that the scene does not hold.";
    private const string ModelField = "model";

    /// <summary>A point far from every subject. Both ends of the fade segment sit there, so no fragment of a subject fades (D-292).</summary>
    public static readonly Vector3 FarPoint = new(0.0f, -1000.0f, 0.0f);

    /// <summary>The viewport and the camera, with the subject of every shot in the viewport.</summary>
    /// <param name="blockAtlas">The block atlas of <see cref="BlockAtlas"/>, which the world material reads.</param>
    /// <param name="layout">The texture layout, which places each face of every model and block.</param>
    /// <param name="shots">The shots of <see cref="ContactSheet.Shots"/>.</param>
    /// <param name="models">Each model that a shot names, by its path.</param>
    /// <param name="sword">The weapon model that each model holds.</param>
    /// <param name="modelMaterial">The one model material of the scene (D-85).</param>
    /// <exception cref="Core.Logging.ContextException">A shot names a model that the models do not hold.</exception>
    public static ContactSheetNodes Build(Texture2D blockAtlas, TextureLayout layout, IReadOnlyList<SheetShot> shots, IReadOnlyDictionary<string, BlockbenchModel> models, BlockbenchModel sword, Material modelMaterial)
    {
        SubViewport viewport = new()
        {
            Size = new Vector2I(ContactSheet.RenderPixels, ContactSheet.RenderPixels),
            RenderTargetUpdateMode = SubViewport.UpdateMode.Always,
            OwnWorld3D = true,
            Msaa3D = PlaceholderScene.EdgeSmoothing,
        };

        ShaderMaterial worldMaterial = WorldMaterial.Create(blockAtlas);
        BlockTiles tiles = new(layout);
        WorldMaterial.SetFade(worldMaterial, FarPoint, FarPoint);
        foreach (SheetShot shot in shots)
        {
            if (shot.Model is not null)
            {
                if (!models.TryGetValue(shot.Model, out BlockbenchModel? model))
                {
                    Core.Logging.ContextException error = new(NoSceneModel);
                    error.AddContext(ModelField, shot.Model);
                    throw error;
                }

                ModelNodeTree nodes = ModelNodes.Build(model, modelMaterial, layout);
                ModelNodes.Hold(nodes, EquipmentSlots.Weapon, ModelNodes.Build(sword, modelMaterial, layout).Root);
                nodes.Root.Position = shot.Origin;
                nodes.Root.RotationDegrees = new Vector3(0.0f, shot.BodyYawDegrees, 0.0f);
                viewport.AddChild(nodes.Root);
                continue;
            }

            if (Ramp.IsRamp(shot.Block))
            {
                foreach (MeshInstance3D chunk in ChunkNodes.Build(ContactSheet.RampScene(Ramp.FromId(shot.Block).Run), worldMaterial, tiles))
                {
                    // The ramp scene grid starts at the origin of the shot.
                    chunk.Position = shot.Origin;
                    viewport.AddChild(chunk);
                }

                continue;
            }

            VoxelGrid grid = new(GridSide, GridSide, GridSide);
            grid.Set(1, 1, 1, shot.Block);
            foreach (MeshInstance3D chunk in ChunkNodes.Build(grid, worldMaterial, tiles))
            {
                // The middle cell of the grid starts at (1, 1, 1), so the chunk moves back by one meter on each axis.
                chunk.Position = shot.Origin - new Vector3(1.0f, 1.0f, 1.0f);
                viewport.AddChild(chunk);
            }
        }

        viewport.AddChild(SceneLight.Environment());
        OmniLight3D lantern = SceneLight.Lantern();
        viewport.AddChild(lantern);
        Camera3D camera = PlaceholderScene.Camera();
        viewport.AddChild(camera);
        return new ContactSheetNodes(viewport, camera, lantern);
    }
}
