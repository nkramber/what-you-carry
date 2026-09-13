using System;
using System.Collections.Generic;
using Godot;
using WhatYouCarry.Core.Camera;
using WhatYouCarry.Core.Entities;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.World;
using WhatYouCarry.Game.Render;

namespace WhatYouCarry.Game.Review;

/// <summary>
/// One shot of the contact sheet: its cell, whether it shows the body or a block, the block, the yaw of the body,
/// the origin of the subject in the scene, and the point that the camera looks at.
/// </summary>
public readonly record struct SheetShot(int Index, bool IsBody, BlockId Block, float BodyYawDegrees, Vector3 Origin, Vector3 Target);

/// <summary>
/// The contact sheet of PR-14 (D-83, D-306): every block material and the body at game zoom, in one PNG file for the
/// review of the owner. <c>Main</c> starts it on the flag, renders one shot per subject, and quits.
/// </summary>
/// <remarks>
/// <para>
/// Game zoom is the scale of the play camera on the Steam Deck: the view of the camera across the 800 rows of the
/// screen, at the boom length of the orbit camera (D-15). Each shot renders a square of 800 pixels at that distance
/// and keeps its middle cell, so a texel on the sheet has the size of a texel in play, about 5.4 pixels.
/// </para>
/// <para>
/// The camera looks from the south-east and from above, so a block shows its south, east, and up faces. The body
/// stands twice, and the second body turns half a circle, so the sheet shows all four sides and the top of the body.
/// The subjects stand far apart, so no cell shows a neighbor. The render needs a window, because the headless display
/// renders no image, so the sheet runs on a desktop and not in CI.
/// </para>
/// </remarks>
public static class ContactSheet
{
    /// <summary>The user argument that starts the sheet. The next argument is the path of the PNG file.</summary>
    public const string Flag = "--contact-sheet";

    /// <summary>The rows of the Steam Deck screen, and the side of each render (D-15).</summary>
    public const int RenderPixels = 800;

    /// <summary>The side of one cell of the sheet: the middle square of a render.</summary>
    public const int CellPixels = 400;

    /// <summary>The count of cells in one row of the sheet.</summary>
    public const int Columns = 3;

    /// <summary>The distance from the camera to the subject, in meters: the boom length of the orbit camera.</summary>
    public const float Distance = OrbitCamera.BoomLength;

    /// <summary>The turn of the camera about the subject, in degrees from the south toward the east.</summary>
    public const float CameraYawDegrees = 35.0f;

    /// <summary>The height of the camera over the subject, in degrees, so the up faces show.</summary>
    public const float CameraPitchDegrees = 25.0f;

    /// <summary>The meters between two subjects along X, so no cell shows a neighbor.</summary>
    public const float SubjectSpacing = 10.0f;

    /// <summary>The yaw of the second body, in degrees, which shows the faces that the first body hides.</summary>
    public const float TurnedBodyYawDegrees = 180.0f;

    /// <summary>The count of body shots.</summary>
    public const int BodyShots = 2;

    /// <summary>The frames that the scene draws before the first shot, so every shader is ready.</summary>
    public const int WarmUpFrames = 30;

    /// <summary>The frames that each shot waits for after the camera moves.</summary>
    public const int FramesPerShot = 3;

    /// <summary>The blocks of the sheet: every block but air, in id order (D-259).</summary>
    public static readonly BlockId[] Blocks =
    [
        BlockId.RawStone,
        BlockId.HewnStone,
        BlockId.TimberBeam,
        BlockId.OreVein,
        BlockId.StillWater,
        BlockId.Rubble,
        BlockId.Plank,
    ];

    /// <summary>Answers whether the user arguments ask for the sheet.</summary>
    public static bool IsRequested(UserArguments userArguments)
    {
        return userArguments.Has(Flag);
    }

    /// <summary>The path of the PNG file: the one word of the flag. The parser stops a flag with no word (D-313).</summary>
    /// <exception cref="ContextException">The flag is absent.</exception>
    public static string PathOf(UserArguments userArguments)
    {
        return userArguments.WordsOf(Flag)[0];
    }

    /// <summary>Every shot, in cell order: one per block, then the body, then the turned body.</summary>
    public static IReadOnlyList<SheetShot> Shots()
    {
        List<SheetShot> shots = [];
        foreach (BlockId block in Blocks)
        {
            Vector3 origin = OriginOf(shots.Count);
            shots.Add(new SheetShot(shots.Count, IsBody: false, block, 0.0f, origin, origin + new Vector3(0.5f, 0.5f, 0.5f)));
        }

        float[] bodyYaws = [0.0f, TurnedBodyYawDegrees];
        foreach (float yaw in bodyYaws)
        {
            Vector3 origin = OriginOf(shots.Count);
            shots.Add(new SheetShot(shots.Count, IsBody: true, BlockId.Air, yaw, origin, origin + new Vector3(0.0f, PlayerBody.Height / 2.0f, 0.0f)));
        }

        return shots;
    }

    /// <summary>The place of the camera of one shot: the boom length from the target, from the south-east and above.</summary>
    public static Vector3 CameraPosition(SheetShot shot)
    {
        double yaw = CameraYawDegrees * Math.PI / 180.0;
        double pitch = CameraPitchDegrees * Math.PI / 180.0;
        Vector3 direction = new((float)(Math.Sin(yaw) * Math.Cos(pitch)), (float)Math.Sin(pitch), (float)(Math.Cos(yaw) * Math.Cos(pitch)));
        return shot.Target + (direction * Distance);
    }

    /// <summary>The screen pixels along one meter at the camera distance: the rows of a render over the height of the view there.</summary>
    public static double PixelsPerMeter()
    {
        double halfView = PlaceholderScene.ViewDegrees * Math.PI / 360.0;
        return RenderPixels / (2.0 * Distance * Math.Tan(halfView));
    }

    /// <summary>The middle square of a render, which becomes one cell.</summary>
    public static Rect2I CropRect()
    {
        int margin = (RenderPixels - CellPixels) / 2;
        return new Rect2I(margin, margin, CellPixels, CellPixels);
    }

    /// <summary>The top left pixel of the cell of one shot on the sheet.</summary>
    public static Vector2I CellOrigin(int index)
    {
        return new Vector2I((index % Columns) * CellPixels, (index / Columns) * CellPixels);
    }

    /// <summary>The width of the sheet, in pixels.</summary>
    public static int SheetPixelsWide()
    {
        return Columns * CellPixels;
    }

    /// <summary>The height of the sheet, in pixels: enough rows for every shot.</summary>
    public static int SheetPixelsHigh()
    {
        int shots = Blocks.Length + BodyShots;
        int rows = (shots + Columns - 1) / Columns;
        return rows * CellPixels;
    }

    /// <summary>The origin of the subject of one shot, along X.</summary>
    private static Vector3 OriginOf(int index)
    {
        return new Vector3(index * SubjectSpacing, 0.0f, 0.0f);
    }
}
