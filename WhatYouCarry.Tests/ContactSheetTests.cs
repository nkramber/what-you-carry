using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Core.World;
using WhatYouCarry.Game;
using WhatYouCarry.Game.Render;
using WhatYouCarry.Game.Review;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// The contact sheet of PR-14 (D-83, D-306): the flag, the scale of game zoom, and the place of every shot. The
/// render needs a window, so these tests read the layout, and the owner reads the sheet (PR-14 exit test 5).
/// </summary>
public sealed class ContactSheetTests
{
    /// <summary>The radius of a sphere around any other subject, in meters, for the neighbor test.</summary>
    private const double NeighborRadius = 1.0;

    /// <summary>The flag starts the sheet, and nothing else does.</summary>
    [Fact]
    public void IsRequestedReadsTheFlag()
    {
        Assert.True(ContactSheet.IsRequested(UserArguments.Parse([ContactSheet.Flag, "sheet.png"])));
        Assert.False(ContactSheet.IsRequested(UserArguments.Parse([])));
        Assert.False(ContactSheet.IsRequested(UserArguments.Parse(["--smoke"])));
    }

    /// <summary>The path is the one word of the flag. A read with no flag is an error (T-2). The parser stops a flag with no path (D-313).</summary>
    [Fact]
    public void PathOfReadsTheArgumentAfterTheFlag()
    {
        Assert.Equal("sheet.png", ContactSheet.PathOf(UserArguments.Parse([ContactSheet.Flag, "sheet.png"])));

        ContextException absent = Assert.Throws<ContextException>(() => ContactSheet.PathOf(UserArguments.Parse([])));
        Assert.Contains(ContactSheet.Flag, absent.Message, StringComparison.Ordinal);
    }

    /// <summary>A texel on the sheet has the size of a texel in play on the Deck: about 5.4 pixels at the boom length (D-306).</summary>
    [Fact]
    public void SheetIsAtGameZoom()
    {
        Assert.Equal(75.0f, PlaceholderScene.ViewDegrees);
        Assert.Equal(3.0f, ContactSheet.Distance);
        double pixelsPerTexel = ContactSheet.PixelsPerMeter() / AtlasLayout.TexelsPerMeter;
        Assert.InRange(pixelsPerTexel, 5.4, 5.5);
    }

    /// <summary>The sheet shows each block once, in id order, and the body from both sides, each in a cell of its own.</summary>
    [Fact]
    public void ShotsCoverEveryBlockAndBothSidesOfTheBody()
    {
        IReadOnlyList<SheetShot> shots = ContactSheet.Shots();

        Assert.Equal(9, shots.Count);
        BlockId[] blocks = shots.Where(shot => !shot.IsBody).Select(shot => shot.Block).ToArray();
        Assert.Equal(new[] { BlockId.RawStone, BlockId.HewnStone, BlockId.TimberBeam, BlockId.OreVein, BlockId.StillWater, BlockId.Rubble, BlockId.Plank }, blocks);
        float[] yaws = shots.Where(shot => shot.IsBody).Select(shot => shot.BodyYawDegrees).ToArray();
        Assert.Equal(new[] { 0.0f, 180.0f }, yaws);

        HashSet<Vector2I> cells = [];
        for (int index = 0; index < shots.Count; index++)
        {
            Assert.Equal(index, shots[index].Index);
            Vector2I cell = ContactSheet.CellOrigin(index);
            Assert.True(cells.Add(cell), $"The shot {index} shares the cell {cell} with an earlier shot.");
            Assert.InRange(cell.X + ContactSheet.CellPixels, ContactSheet.CellPixels, ContactSheet.SheetPixelsWide());
            Assert.InRange(cell.Y + ContactSheet.CellPixels, ContactSheet.CellPixels, ContactSheet.SheetPixelsHigh());
        }

        Assert.Equal(new Rect2I(200, 200, 400, 400), ContactSheet.CropRect());
    }

    /// <summary>A sphere around each subject projects inside its cell, so no cell cuts its subject. The body sphere reads the box corners of the player model.</summary>
    [Fact]
    public void EverySubjectFitsItsCell()
    {
        double blockRadius = Math.Sqrt(3.0) / 2.0;
        Assert.True(ProjectedDiameter(blockRadius) < ContactSheet.CellPixels, $"A block spans {ProjectedDiameter(blockRadius)} pixels, and a cell holds {ContactSheet.CellPixels}.");

        double bodyRadius = BodyRadius();
        Assert.True(bodyRadius > 0.9, $"The body radius is {bodyRadius} meters, and the body is 1.8 meters tall.");
        Assert.True(ProjectedDiameter(bodyRadius) < ContactSheet.CellPixels, $"The body spans {ProjectedDiameter(bodyRadius)} pixels, and a cell holds {ContactSheet.CellPixels}.");
    }

    /// <summary>Every camera stands at the boom length from its target, above it.</summary>
    [Fact]
    public void CamerasStandAtTheBoomLength()
    {
        foreach (SheetShot shot in ContactSheet.Shots())
        {
            Vector3 camera = ContactSheet.CameraPosition(shot);
            Assert.InRange(camera.DistanceTo(shot.Target), ContactSheet.Distance - 0.001f, ContactSheet.Distance + 0.001f);
            Assert.True(camera.Y > shot.Target.Y, $"The camera of the shot {shot.Index} is not above its target.");
        }
    }

    /// <summary>No subject of another shot enters the cell of a shot: its sphere stays outside the cone of the cell corners.</summary>
    [Fact]
    public void NoShotSeesANeighbor()
    {
        IReadOnlyList<SheetShot> shots = ContactSheet.Shots();
        double halfView = PlaceholderScene.ViewDegrees * Math.PI / 360.0;
        double cellCorner = Math.Atan(Math.Tan(halfView) * ContactSheet.CellPixels / ContactSheet.RenderPixels * Math.Sqrt(2.0));
        foreach (SheetShot shot in shots)
        {
            Vector3 camera = ContactSheet.CameraPosition(shot);
            Vector3 axis = (shot.Target - camera).Normalized();
            foreach (SheetShot other in shots.Where(other => other.Index != shot.Index))
            {
                Vector3 toOther = other.Target - camera;
                double angle = Math.Acos(Math.Clamp(axis.Dot(toOther.Normalized()), -1.0f, 1.0f));
                double spread = Math.Asin(Math.Min(1.0, NeighborRadius / toOther.Length()));
                Assert.True(angle - spread > cellCorner, $"The cell of the shot {shot.Index} can show the subject of the shot {other.Index}: {angle - spread} radians from the view axis, inside {cellCorner}.");
            }
        }
    }

    /// <summary>The diameter in pixels of a sphere of one radius at the camera distance, on the view axis of an 800 pixel render.</summary>
    private static double ProjectedDiameter(double radius)
    {
        double halfView = PlaceholderScene.ViewDegrees * Math.PI / 360.0;
        double angle = Math.Asin(radius / ContactSheet.Distance);
        return ContactSheet.RenderPixels * Math.Tan(angle) / Math.Tan(halfView);
    }

    /// <summary>
    /// The largest distance from the target of a body shot to a box corner of the player model, or of the sword at the
    /// weapon point of its right hand, in meters (D-336).
    /// </summary>
    private static double BodyRadius()
    {
        string content = Path.Combine(RepositoryRoot.Find(), "content");
        BlockbenchModel body = BlockbenchLoader.Parse(AssetPaths.BodyModel, File.ReadAllBytes(Path.Combine(content, AssetPaths.BodyModel)));
        string swordPath = SimulationLoop.MainWeapon(TestWorld.Content).Model;
        BlockbenchModel sword = BlockbenchLoader.Parse(swordPath, File.ReadAllBytes(Path.Combine(content, swordPath)));
        AttachmentPoint hand = body.Attachments.First(point => point.Slot == EquipmentSlots.Weapon);
        SheetShot shot = ContactSheet.Shots().First(candidate => candidate.IsBody);
        Vector3 center = shot.Target - shot.Origin;
        double radius = 0.0;
        foreach ((ModelBox box, Vector3 offset) in body.Boxes.Select(box => (box, Vector3.Zero)).Concat(sword.Boxes.Select(box => (box, new Vector3(hand.Position.X, hand.Position.Y, hand.Position.Z)))))
        {
            foreach (float x in new[] { box.From.X, box.To.X })
            {
                foreach (float y in new[] { box.From.Y, box.To.Y })
                {
                    foreach (float z in new[] { box.From.Z, box.To.Z })
                    {
                        radius = Math.Max(radius, (new Vector3(x, y, z) + offset).DistanceTo(center));
                    }
                }
            }
        }

        return radius;
    }
}
