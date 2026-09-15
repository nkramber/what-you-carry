using System;
using System.Collections.Generic;
using System.Globalization;
using Godot;
using WhatYouCarry.Core.Camera;
using WhatYouCarry.Core.Entities;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.World;
using WhatYouCarry.Game.Render;

namespace WhatYouCarry.Game.Review;

/// <summary>
/// One shot of the contact sheet: its cell, whether it shows the body or a block, the block, the yaw of the body,
/// the origin of the subject in the scene, and the point that the camera looks at. A ramp shot names the low end of
/// its ramp as the block.
/// </summary>
public readonly record struct SheetShot(int Index, bool IsBody, BlockId Block, float BodyYawDegrees, Vector3 Origin, Vector3 Target);

/// <summary>
/// The contact sheet of PR-14 (D-83, D-306): every block material, the body, and a ramp of each slope (PR-65) at game
/// zoom, in one PNG file for the review of the owner. <c>Main</c> starts it on the flag, renders one shot per subject,
/// and quits.
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
/// <para>
/// A ramp is longer than the middle square shows at game zoom, so a ramp shot keeps the whole render, and the ramps
/// fill one row under the other cells. The camera looks at the foot of the ramp, as a player at the foot looks up the
/// slope, and the slope rises away from it. A ramp scene is larger than a block, so the ramps stand far apart along
/// minus X.
/// </para>
/// </remarks>
public static class ContactSheet
{
    /// <summary>The user argument that starts the sheet. The next argument is the path of the PNG file.</summary>
    public const string Flag = "--contact-sheet";

    /// <summary>The rows of the Steam Deck screen, and the side of each render (D-15).</summary>
    public const int RenderPixels = 800;

    /// <summary>The side of the cell of a block or a body: the middle square of a render.</summary>
    public const int CellPixels = 400;

    /// <summary>The side of the cell of a ramp: the whole render.</summary>
    public const int RampCellPixels = RenderPixels;

    /// <summary>The count of cells of the blocks and the bodies in one row of the sheet.</summary>
    public const int Columns = 3;

    /// <summary>The distance from the camera to the subject, in meters: the boom length of the orbit camera.</summary>
    public const float Distance = OrbitCamera.BoomLength;

    /// <summary>The turn of the camera about the subject, in degrees from the south toward the east.</summary>
    public const float CameraYawDegrees = 35.0f;

    /// <summary>The height of the camera over the subject, in degrees, so the up faces show.</summary>
    public const float CameraPitchDegrees = 25.0f;

    /// <summary>The meters between two subjects along X, so no cell shows a neighbor.</summary>
    public const float SubjectSpacing = 10.0f;

    /// <summary>The meters along minus X between two ramps, and between the first ramp and the first block.</summary>
    public const float RampSpacing = 100.0f;

    /// <summary>The yaw of the second body, in degrees, which shows the faces that the first body hides.</summary>
    public const float TurnedBodyYawDegrees = 180.0f;

    /// <summary>The count of body shots.</summary>
    public const int BodyShots = 2;

    /// <summary>The size of a ramp scene along X, in blocks: the wall, the ramp, and a column of air.</summary>
    public const int RampSceneWidth = 4;

    /// <summary>The height of a ramp scene, in blocks: the floor, the ramp, and the wall over it.</summary>
    public const int RampSceneHeight = 3;

    /// <summary>The first column of the ramp in a ramp scene. The wall stands in the column before it.</summary>
    public const int RampFirstColumn = 1;

    /// <summary>The count of columns of the ramp in a ramp scene.</summary>
    public const int RampColumns = 2;

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

    /// <summary>The ramps of the sheet: the low end of a ramp of each slope of D-346, from 1:2 to 1:4. Each rises toward minus Z, away from the camera.</summary>
    public static readonly BlockId[] Ramps =
    [
        new Ramp(RampRise.MinusZ, 2, 0).Id,
        new Ramp(RampRise.MinusZ, 3, 0).Id,
        new Ramp(RampRise.MinusZ, 4, 0).Id,
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

    /// <summary>Every shot, in cell order: one per block, then the body, then the turned body, then one per ramp.</summary>
    public static IReadOnlyList<SheetShot> Shots()
    {
        List<SheetShot> shots = [];
        foreach (BlockId block in Blocks)
        {
            Vector3 origin = new(shots.Count * SubjectSpacing, 0.0f, 0.0f);
            shots.Add(new SheetShot(shots.Count, IsBody: false, block, 0.0f, origin, origin + new Vector3(0.5f, 0.5f, 0.5f)));
        }

        float[] bodyYaws = [0.0f, TurnedBodyYawDegrees];
        foreach (float yaw in bodyYaws)
        {
            Vector3 origin = new(shots.Count * SubjectSpacing, 0.0f, 0.0f);
            shots.Add(new SheetShot(shots.Count, IsBody: true, BlockId.Air, yaw, origin, origin + new Vector3(0.0f, PlayerBody.Height / 2.0f, 0.0f)));
        }

        for (int ramp = 0; ramp < Ramps.Length; ramp++)
        {
            // The scene grid starts at the origin. The foot is the middle of the ramp columns, on the floor, at the low
            // face of place 0.
            int run = Ramp.FromId(Ramps[ramp]).Run;
            Vector3 origin = new(-(ramp + 1) * RampSpacing, 0.0f, 0.0f);
            Vector3 foot = new(RampFirstColumn + (RampColumns / 2.0f), 1.0f, run + 1.0f);
            shots.Add(new SheetShot(shots.Count, IsBody: false, Ramps[ramp], 0.0f, origin, origin + foot));
        }

        return shots;
    }

    /// <summary>
    /// The grid of the shot of a ramp of one run. Row 0 is a stone floor. In row 1, a ramp of the run, two cells wide,
    /// rises toward minus Z onto a stone landing at Z = 0, and its foot is at Z = run + 1. A hewn stone wall two blocks
    /// high stands on the minus X side, and air on the plus X side, so the camera sees the slope, a side, and the
    /// crease at the wall.
    /// </summary>
    /// <exception cref="ContextException">The run is not a run of D-346.</exception>
    public static VoxelGrid RampScene(int run)
    {
        if (run < Ramp.SteepestRun || run > Ramp.ShallowestRun)
        {
            ContextException error = new($"A ramp scene takes a run from {Ramp.SteepestRun} to {Ramp.ShallowestRun} (D-346), and the run is {run}.");
            error.AddContext(nameof(run), run.ToString(CultureInfo.InvariantCulture));
            throw error;
        }

        VoxelGrid grid = new(RampSceneWidth, RampSceneHeight, run + 2);
        for (int z = 0; z < run + 2; z++)
        {
            for (int x = 0; x < RampSceneWidth; x++)
            {
                grid.Set(x, 0, z, BlockId.RawStone);
            }

            grid.Set(0, 1, z, BlockId.HewnStone);
            grid.Set(0, 2, z, BlockId.HewnStone);
            for (int x = RampFirstColumn; x < RampFirstColumn + RampColumns; x++)
            {
                if (z == 0)
                {
                    grid.Set(x, 1, z, BlockId.RawStone);
                }
                else if (z <= run)
                {
                    grid.Set(x, 1, z, new Ramp(RampRise.MinusZ, run, run - z).Id);
                }
            }
        }

        return grid;
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

    /// <summary>The part of the render of a shot that becomes its cell: the middle square for a block or a body, and the whole render for a ramp.</summary>
    public static Rect2I CropRect(SheetShot shot)
    {
        if (Ramp.IsRamp(shot.Block))
        {
            return new Rect2I(0, 0, RampCellPixels, RampCellPixels);
        }

        int margin = (RenderPixels - CellPixels) / 2;
        return new Rect2I(margin, margin, CellPixels, CellPixels);
    }

    /// <summary>The top left pixel of the cell of one shot on the sheet: the blocks and the bodies in rows of three, then the ramps in one row under them.</summary>
    public static Vector2I CellOrigin(SheetShot shot)
    {
        if (Ramp.IsRamp(shot.Block))
        {
            int firstRamp = Blocks.Length + BodyShots;
            return new Vector2I((shot.Index - firstRamp) * RampCellPixels, SquareRows() * CellPixels);
        }

        return new Vector2I((shot.Index % Columns) * CellPixels, (shot.Index / Columns) * CellPixels);
    }

    /// <summary>The width of the sheet, in pixels: the wider of the rows of three cells and the row of ramps.</summary>
    public static int SheetPixelsWide()
    {
        return Math.Max(Columns * CellPixels, Ramps.Length * RampCellPixels);
    }

    /// <summary>The height of the sheet, in pixels: the rows of the blocks and the bodies, then the row of ramps.</summary>
    public static int SheetPixelsHigh()
    {
        return (SquareRows() * CellPixels) + RampCellPixels;
    }

    /// <summary>The count of rows that the cells of the blocks and the bodies fill.</summary>
    private static int SquareRows()
    {
        int shots = Blocks.Length + BodyShots;
        return (shots + Columns - 1) / Columns;
    }
}
