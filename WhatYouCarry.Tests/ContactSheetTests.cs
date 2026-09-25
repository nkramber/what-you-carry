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
using CoreVector3 = WhatYouCarry.Core.Physics.Vector3;

namespace WhatYouCarry.Tests;

/// <summary>
/// The contact sheet of PR-14 (D-83, D-306): the flag, the scale of game zoom, and the place of every shot, with the
/// ramps of PR-65. The render needs a window, so these tests read the layout, and the owner reads the sheet (PR-14 exit
/// test 5, PR-65 exit test 4).
/// </summary>
public sealed class ContactSheetTests
{
    /// <summary>The radius of a sphere around a block, in meters, for the neighbor test.</summary>
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

    /// <summary>
    /// The sheet shows each block once, in id order, the body from both sides, and the low end of a ramp of each slope.
    /// Each shot has a cell of its own inside the sheet, and no two cells overlap.
    /// </summary>
    [Fact]
    public void ShotsCoverEveryBlockBothSidesOfTheBodyAndEachSlope()
    {
        IReadOnlyList<SheetShot> shots = ContactSheet.Shots();

        Assert.Equal(12, shots.Count);
        BlockId[] blocks = shots.Where(shot => !shot.IsBody && !Ramp.IsRamp(shot.Block)).Select(shot => shot.Block).ToArray();
        Assert.Equal(new[] { BlockId.RawStone, BlockId.HewnStone, BlockId.TimberBeam, BlockId.OreVein, BlockId.StillWater, BlockId.Rubble, BlockId.Plank }, blocks);
        float[] yaws = shots.Where(shot => shot.IsBody).Select(shot => shot.BodyYawDegrees).ToArray();
        Assert.Equal(new[] { 0.0f, 180.0f }, yaws);
        Ramp[] ramps = shots.Where(shot => Ramp.IsRamp(shot.Block)).Select(shot => Ramp.FromId(shot.Block)).ToArray();
        Assert.Equal(new[] { new Ramp(RampRise.MinusZ, 2, 0), new Ramp(RampRise.MinusZ, 3, 0), new Ramp(RampRise.MinusZ, 4, 0) }, ramps);

        Rect2I render = new(0, 0, ContactSheet.RenderPixels, ContactSheet.RenderPixels);
        Rect2I sheet = new(0, 0, ContactSheet.SheetPixelsWide(), ContactSheet.SheetPixelsHigh());
        List<Rect2I> cells = [];
        for (int index = 0; index < shots.Count; index++)
        {
            Assert.Equal(index, shots[index].Index);
            Rect2I crop = ContactSheet.CropRect(shots[index]);
            Rect2I cell = new(ContactSheet.CellOrigin(shots[index]), crop.Size);
            Assert.True(render.Encloses(crop), $"The crop {crop} of the shot {index} leaves the render.");
            Assert.True(sheet.Encloses(cell), $"The cell {cell} of the shot {index} leaves the sheet {sheet}.");
            foreach (Rect2I earlier in cells)
            {
                Assert.False(earlier.Intersects(cell), $"The cell {cell} of the shot {index} overlaps the cell {earlier}.");
            }

            cells.Add(cell);
        }

        Assert.Equal(new Rect2I(200, 200, 400, 400), ContactSheet.CropRect(shots[0]));
        Assert.Equal(render, ContactSheet.CropRect(shots[^1]));
    }

    /// <summary>
    /// A sphere around each block projects inside its cell, so no cell cuts its subject. Each box corner of the player
    /// model and of the sword that its right hand tilts forward projects inside the cell of each body shot (D-336, D-591).
    /// Each corner of the slope of a ramp projects inside its render.
    /// </summary>
    [Fact]
    public void EverySubjectFitsItsCell()
    {
        double blockRadius = Math.Sqrt(3.0) / 2.0;
        Assert.True(ProjectedDiameter(blockRadius) < ContactSheet.CellPixels, $"A block spans {ProjectedDiameter(blockRadius)} pixels, and a cell holds {ContactSheet.CellPixels}.");

        double bodyRadius = BodyRadius();
        Assert.True(bodyRadius > 0.9, $"The body radius is {bodyRadius} meters, and the body is 1.8 meters tall.");
        float cellHalf = ContactSheet.CellPixels / (float)ContactSheet.RenderPixels;
        foreach (SheetShot shot in ContactSheet.Shots().Where(shot => shot.IsBody))
        {
            // The scene stands the body at the origin of the shot and turns it about the up axis by the yaw of the shot.
            Basis turn = new(Vector3.Up, Mathf.DegToRad(shot.BodyYawDegrees));
            foreach (Vector3 corner in BodyCorners())
            {
                Vector2 place = ViewPlace(shot, shot.Origin + (turn * corner));
                Assert.True(Math.Abs(place.X) <= cellHalf && Math.Abs(place.Y) <= cellHalf, $"The body corner {corner} of the shot {shot.Index} is at {place} on the render, outside its cell of {cellHalf}.");
            }
        }

        foreach (SheetShot shot in ContactSheet.Shots().Where(shot => Ramp.IsRamp(shot.Block)))
        {
            int run = Ramp.FromId(shot.Block).Run;
            foreach (float x in new float[] { ContactSheet.RampFirstColumn, ContactSheet.RampFirstColumn + ContactSheet.RampColumns })
            {
                // The foot of the slope lies on the floor at Z = run + 1, and the top lies on the landing at Z = 1.
                foreach (Vector3 corner in new[] { new Vector3(x, 1.0f, run + 1.0f), new Vector3(x, 2.0f, 1.0f) })
                {
                    Vector2 place = ViewPlace(shot, shot.Origin + corner);
                    Assert.True(Math.Abs(place.X) <= 1.0f && Math.Abs(place.Y) <= 1.0f, $"The slope corner {corner} of the ramp of run {run} is at {place} on the render, outside it.");
                }
            }
        }
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
        foreach (SheetShot shot in shots)
        {
            Vector3 camera = ContactSheet.CameraPosition(shot);
            Vector3 axis = (shot.Target - camera).Normalized();
            double cellCorner = Math.Atan(Math.Tan(halfView) * ContactSheet.CropRect(shot).Size.X / ContactSheet.RenderPixels * Math.Sqrt(2.0));
            foreach (SheetShot other in shots.Where(other => other.Index != shot.Index))
            {
                Vector3 toOther = other.Target - camera;
                double angle = Math.Acos(Math.Clamp(axis.Dot(toOther.Normalized()), -1.0f, 1.0f));
                double spread = Math.Asin(Math.Min(1.0, SubjectRadius(other) / toOther.Length()));
                Assert.True(angle - spread > cellCorner, $"The cell of the shot {shot.Index} can show the subject of the shot {other.Index}: {angle - spread} radians from the view axis, inside {cellCorner}.");
            }
        }
    }

    /// <summary>Each ramp scene holds a whole run of its slope between the foot and a landing, a wall on one side, and air on the other.</summary>
    [Fact]
    public void EachRampSceneHoldsAWholeRunOntoALanding()
    {
        for (int run = Ramp.SteepestRun; run <= Ramp.ShallowestRun; run++)
        {
            VoxelGrid scene = ContactSheet.RampScene(run);
            Assert.Equal(run + 2, scene.SizeZ);
            for (int x = ContactSheet.RampFirstColumn; x < ContactSheet.RampFirstColumn + ContactSheet.RampColumns; x++)
            {
                Assert.Equal(BlockId.RawStone, scene.Get(x, 1, 0));
                for (int place = 0; place < run; place++)
                {
                    Assert.Equal(new Ramp(RampRise.MinusZ, run, place).Id, scene.Get(x, 1, run - place));
                }

                Assert.Equal(BlockId.Air, scene.Get(x, 1, run + 1));
                Assert.Equal(BlockId.RawStone, scene.Get(x, 0, run + 1));
            }

            Assert.Equal(BlockId.HewnStone, scene.Get(0, 2, 1));
            Assert.Equal(BlockId.Air, scene.Get(ContactSheet.RampSceneWidth - 1, 1, 1));
        }

        ContextException error = Assert.Throws<ContextException>(() => ContactSheet.RampScene(1));
        Assert.Contains("run is 1", error.Message, StringComparison.Ordinal);
    }

    /// <summary>The diameter in pixels of a sphere of one radius at the camera distance, on the view axis of an 800 pixel render.</summary>
    private static double ProjectedDiameter(double radius)
    {
        double halfView = PlaceholderScene.ViewDegrees * Math.PI / 360.0;
        double angle = Math.Asin(radius / ContactSheet.Distance);
        return ContactSheet.RenderPixels * Math.Tan(angle) / Math.Tan(halfView);
    }

    /// <summary>
    /// The place of a point on the square render of a shot, from -1 to 1 along the width and the height of the render,
    /// by the camera of the shot and the view of the scene camera.
    /// </summary>
    private static Vector2 ViewPlace(SheetShot shot, Vector3 point)
    {
        Vector3 camera = ContactSheet.CameraPosition(shot);
        Vector3 forward = (shot.Target - camera).Normalized();
        Vector3 right = forward.Cross(Vector3.Up).Normalized();
        Vector3 up = right.Cross(forward);
        Vector3 offset = point - camera;
        float depth = offset.Dot(forward);
        Assert.True(depth > 0.0f, $"The point {point} is behind the camera of the shot {shot.Index}.");
        double tangent = Math.Tan(PlaceholderScene.ViewDegrees * Math.PI / 360.0);
        return new Vector2((float)(offset.Dot(right) / (depth * tangent)), (float)(offset.Dot(up) / (depth * tangent)));
    }

    /// <summary>The radius of a sphere around the target of a shot that holds its subject: the farthest corner of the scene grid of a ramp, the farthest corner of the body and its sword, and one meter for a block.</summary>
    private static double SubjectRadius(SheetShot shot)
    {
        if (shot.IsBody)
        {
            return BodyRadius();
        }

        if (!Ramp.IsRamp(shot.Block))
        {
            return NeighborRadius;
        }

        VoxelGrid scene = ContactSheet.RampScene(Ramp.FromId(shot.Block).Run);
        Vector3 center = shot.Target - shot.Origin;
        double radius = 0.0;
        foreach (int x in new[] { 0, scene.SizeX })
        {
            foreach (int y in new[] { 0, scene.SizeY })
            {
                foreach (int z in new[] { 0, scene.SizeZ })
                {
                    radius = Math.Max(radius, new Vector3(x, y, z).DistanceTo(center));
                }
            }
        }

        return radius;
    }

    /// <summary>The largest distance from the target of a body shot to a box corner of the player model or of its held sword, in meters (D-336).</summary>
    private static double BodyRadius()
    {
        SheetShot shot = ContactSheet.Shots().First(candidate => candidate.IsBody);
        Vector3 center = shot.Target - shot.Origin;
        return BodyCorners().Max(corner => (double)corner.DistanceTo(center));
    }

    /// <summary>
    /// Every box corner of the player model, and of the sword at the weapon point of its right hand, in meters in model
    /// space. The sword hangs from the point at the tilt of the point, as the Game hangs it (D-330, D-591).
    /// </summary>
    private static List<Vector3> BodyCorners()
    {
        string content = Path.Combine(RepositoryRoot.Find(), "content");
        BlockbenchModel body = BlockbenchLoader.Parse(AssetPaths.BodyModel, File.ReadAllBytes(Path.Combine(content, AssetPaths.BodyModel)));
        string swordPath = SimulationLoop.MainWeapon(TestWorld.Content).Model;
        BlockbenchModel sword = BlockbenchLoader.Parse(swordPath, File.ReadAllBytes(Path.Combine(content, swordPath)));
        AttachmentPoint hand = body.Attachments.First(point => point.Slot == EquipmentSlots.Weapon);
        RotationMatrix hold = RotationMatrix.FromEulerDegrees(hand.RotationDegrees);
        List<Vector3> corners = [];
        foreach ((ModelBox box, bool held) in body.Boxes.Select(box => (box, false)).Concat(sword.Boxes.Select(box => (box, true))))
        {
            foreach (float x in new[] { box.From.X, box.To.X })
            {
                foreach (float y in new[] { box.From.Y, box.To.Y })
                {
                    foreach (float z in new[] { box.From.Z, box.To.Z })
                    {
                        CoreVector3 corner = held ? hand.Position + hold.Apply(new CoreVector3(x, y, z)) : new CoreVector3(x, y, z);
                        corners.Add(new Vector3(corner.X, corner.Y, corner.Z));
                    }
                }
            }
        }

        return corners;
    }
}
