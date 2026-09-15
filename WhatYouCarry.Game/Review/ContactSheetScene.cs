using Godot;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.World;
using WhatYouCarry.Game.Models;
using WhatYouCarry.Game.Render;
using WhatYouCarry.Game.World;

namespace WhatYouCarry.Game.Review;

/// <summary>The nodes of the contact sheet: the viewport that renders each shot, and its camera.</summary>
public sealed record ContactSheetNodes(SubViewport Viewport, Camera3D Camera);

/// <summary>
/// Builds the scene of the contact sheet (D-306) in a viewport with a world of its own: each block in a small grid
/// and each ramp in its ramp scene through the greedy mesher and the world material, the two bodies with the sword in
/// the hand and the model material (D-336), the scene light, and the camera of play. Every subject stands at the place
/// that its shot gives.
/// </summary>
public static class ContactSheetScene
{
    /// <summary>The side of the small grid of one block. The block fills the middle cell, with air on every side, so all six faces show.</summary>
    public const int GridSide = 3;

    /// <summary>A point far from every subject. Both ends of the fade segment sit there, so no fragment of a subject fades (D-292).</summary>
    public static readonly Vector3 FarPoint = new(0.0f, -1000.0f, 0.0f);

    /// <summary>The viewport and the camera, with every subject of <see cref="ContactSheet.Shots"/> in the viewport.</summary>
    public static ContactSheetNodes Build(Texture2D atlas, BlockbenchModel body, BlockbenchModel sword, Material modelMaterial)
    {
        SubViewport viewport = new()
        {
            Size = new Vector2I(ContactSheet.RenderPixels, ContactSheet.RenderPixels),
            RenderTargetUpdateMode = SubViewport.UpdateMode.Always,
            OwnWorld3D = true,
        };

        ShaderMaterial worldMaterial = WorldMaterial.Create(atlas);
        WorldMaterial.SetFade(worldMaterial, FarPoint, FarPoint);
        foreach (SheetShot shot in ContactSheet.Shots())
        {
            if (shot.IsBody)
            {
                ModelNodeTree nodes = ModelNodes.Build(body, modelMaterial);
                ModelNodes.Hold(nodes, EquipmentSlots.Weapon, ModelNodes.Build(sword, modelMaterial).Root);
                nodes.Root.Position = shot.Origin;
                nodes.Root.RotationDegrees = new Vector3(0.0f, shot.BodyYawDegrees, 0.0f);
                viewport.AddChild(nodes.Root);
                continue;
            }

            if (Ramp.IsRamp(shot.Block))
            {
                foreach (MeshInstance3D chunk in ChunkNodes.Build(ContactSheet.RampScene(Ramp.FromId(shot.Block).Run), worldMaterial))
                {
                    // The ramp scene grid starts at the origin of the shot.
                    chunk.Position = shot.Origin;
                    viewport.AddChild(chunk);
                }

                continue;
            }

            VoxelGrid grid = new(GridSide, GridSide, GridSide);
            grid.Set(1, 1, 1, shot.Block);
            foreach (MeshInstance3D chunk in ChunkNodes.Build(grid, worldMaterial))
            {
                // The middle cell of the grid starts at (1, 1, 1), so the chunk moves back by one meter on each axis.
                chunk.Position = shot.Origin - new Vector3(1.0f, 1.0f, 1.0f);
                viewport.AddChild(chunk);
            }
        }

        viewport.AddChild(PlaceholderScene.Light());
        Camera3D camera = PlaceholderScene.Camera();
        viewport.AddChild(camera);
        return new ContactSheetNodes(viewport, camera);
    }
}
