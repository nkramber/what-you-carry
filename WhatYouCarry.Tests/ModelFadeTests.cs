using System;
using Godot;
using WhatYouCarry.Game.Render;
using WhatYouCarry.Game.World;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The fade of an enemy model and of the Overseer model (PR-97, D-721, F-196).</summary>
public sealed class ModelFadeTests
{
    /// <summary>The center of the player body, with the feet at the origin.</summary>
    private static readonly Vector3 PlayerCenter = new(0.0f, 0.9f, 0.0f);

    /// <summary>The camera at rest behind the right shoulder: 0.9 right, 2.2 up, and 3 back (D-719).</summary>
    private static readonly Vector3 Camera = new(0.9f, 2.2f, 3.0f);

    private static readonly Vector3 FadeEnd = WorldMaterial.FadeEnd(Camera, PlayerCenter);

    /// <summary>The constants of D-721 hold: 5 pixels in 16, and a reach of 1.0 meter from the drawn camera.</summary>
    [Fact]
    public void TheConstantsHold()
    {
        Assert.Equal(5.0f / 16.0f, ModelFade.Kept);
        Assert.Equal(1.0f, ModelFade.CameraReach);
    }

    /// <summary>The body box of a model is the box of D-165 over its feet.</summary>
    [Fact]
    public void TheBodyBoxStandsOnTheFeet()
    {
        Aabb box = ModelFade.BodyBox(new Vector3(2.0f, 1.0f, 3.0f));

        Assert.Equal(new Vector3(1.7f, 1.0f, 2.7f), box.Position);
        Assert.Equal(new Vector3(0.6f, 1.8f, 0.6f), box.Size);
    }

    /// <summary>The fade segment ends one fade radius before the player, and at the camera when the camera is nearer than that.</summary>
    [Fact]
    public void TheFadeSegmentEndsBeforeThePlayer()
    {
        Vector3 end = WorldMaterial.FadeEnd(new Vector3(0.0f, 0.9f, 3.0f), PlayerCenter);
        Assert.Equal(new Vector3(0.0f, 0.9f, WorldMaterial.FadeRadius), end);

        Vector3 near = new(0.0f, 0.9f, 0.5f);
        Assert.Equal(near, WorldMaterial.FadeEnd(near, PlayerCenter));
    }

    /// <summary>PR-97 regression: a scavenger between the camera and the player fades (F-196).</summary>
    [Fact]
    public void AModelBetweenTheCameraAndThePlayerFades()
    {
        Assert.True(ModelFade.Fades(Camera, FadeEnd, ModelFade.BodyBox(new Vector3(0.45f, 0.0f, 1.5f))));
    }

    /// <summary>A scavenger in front of the player at melee range stays whole, because the player fights it (F-195).</summary>
    [Fact]
    public void AModelInFrontOfThePlayerStaysWhole()
    {
        Assert.False(ModelFade.Fades(Camera, FadeEnd, ModelFade.BodyBox(new Vector3(0.0f, 0.0f, -1.2f))));
        Assert.False(ModelFade.Fades(Camera, FadeEnd, ModelFade.BodyBox(new Vector3(-3.0f, 0.0f, 1.0f))));
    }

    /// <summary>A model near the drawn camera fades, also when it stands off the fade segment (D-721).</summary>
    [Fact]
    public void AModelNearTheCameraFades()
    {
        // The box spans x = 1.75 to 2.35: 0.85 right of the camera, past the fade radius and inside the reach. The
        // segment runs from x = 0.9 toward the player at x = 0, so it never meets the grown box.
        Aabb box = ModelFade.BodyBox(new Vector3(2.05f, 0.0f, 3.0f));
        Assert.False(box.Grow(WorldMaterial.FadeRadius).HasPoint(Camera));
        Assert.True(ModelFade.Fades(Camera, FadeEnd, box));

        Aabb far = ModelFade.BodyBox(new Vector3(2.35f, 0.0f, 3.0f));
        Assert.False(ModelFade.Fades(Camera, FadeEnd, far));
    }

    /// <summary>
    /// PR-97 regression: the reach is the straight distance to the nearest point of the body box, and not the distance on
    /// each axis. A box 0.9 meters off the camera on X and on Z, and 0.4 under it, is 1.33 meters away at its corner, so it stays whole. A box
    /// grown by the reach on each axis holds the camera, and fades it.
    /// </summary>
    [Fact]
    public void TheReachIsAStraightDistance()
    {
        // The box spans x = 1.8 to 2.4 and z = 3.9 to 4.5: 0.9 past the camera on each axis, and away from the segment.
        Aabb corner = ModelFade.BodyBox(new Vector3(2.1f, 0.0f, 4.2f));
        Assert.True(corner.Grow(ModelFade.CameraReach).HasPoint(Camera));
        Assert.False(ModelFade.Fades(Camera, FadeEnd, corner));

        // At 0.6 on X and on Z the corner is 0.94 away, inside the reach.
        Aabb near = ModelFade.BodyBox(new Vector3(1.8f, 0.0f, 3.9f));
        Assert.True(ModelFade.Fades(Camera, FadeEnd, near));
    }

    /// <summary>
    /// The fade shader keeps the pixels of the Bayer pattern of the wall fade at or under the kept part, and samples the
    /// model atlas as the model material does (D-85, D-632, D-677, D-721).
    /// </summary>
    [Fact]
    public void TheFadeShaderKeepsFivePixelsInSixteen()
    {
        string shader = RepositoryRoot.ReadFile("WhatYouCarry.Game/Render/model_fade.gdshader");
        string world = RepositoryRoot.ReadFile("WhatYouCarry.Game/World/world.gdshader");

        Assert.Contains($"uniform sampler2D {ModelFade.AtlasName} : source_color, filter_nearest_mipmap;", shader, StringComparison.Ordinal);
        Assert.Contains($"uniform float {ModelFade.KeptName} = 0.3125;", shader, StringComparison.Ordinal);
        Assert.Contains("if (dither_threshold(FRAGCOORD.xy) > kept) {", shader, StringComparison.Ordinal);
        Assert.Contains("render_mode cull_back;", shader, StringComparison.Ordinal);
        Assert.DoesNotContain("ALPHA", shader, StringComparison.Ordinal);
        const string pattern = "int pattern[16] = {0, 8, 2, 10, 12, 4, 14, 6, 3, 11, 1, 9, 15, 7, 13, 5};";
        Assert.Contains(pattern, shader, StringComparison.Ordinal);
        Assert.Contains(pattern, world, StringComparison.Ordinal);
        Assert.Equal("res://Render/model_fade.gdshader", ModelFade.ShaderPath);
    }

    /// <summary>
    /// PR-97 regression: the background is the dark stone of unlit rock, so a view line from the drawn camera that meets no
    /// face inside rock does not read as a pit (D-720, D-722). The near black of the color before fails it.
    /// </summary>
    [Fact]
    public void TheBackgroundIsDarkStone()
    {
        Color background = SceneLight.BackgroundColor;

        Assert.Equal(new Color(0.055f, 0.05f, 0.065f), background);
        Assert.True(background.R8 >= 12 && background.G8 >= 12 && background.B8 >= 14, $"The background {background} is darker than unlit rock.");
    }

    /// <summary>PR-97 regression: the camera draws from the view of the pose, and the enemy models read the fade segment (D-720, D-721).</summary>
    [Fact]
    public void TheSceneDrawsFromTheViewAndFadesTheModels()
    {
        string main = RepositoryRoot.ReadFile("WhatYouCarry.Game/Main.cs");

        Assert.Contains("Vector3 cameraPosition = RenderInterpolation.ToGodot(pose.View);", main, StringComparison.Ordinal);
        Assert.Contains("this.enemyNodes?.Draw(this.loop.Enemies, this.loop.Hunter, fraction, cameraPosition, WorldMaterial.FadeEnd(cameraPosition, playerCenter));", main, StringComparison.Ordinal);
        Assert.Contains("ModelFade.CreateMaterial(atlas)", main, StringComparison.Ordinal);
        Assert.DoesNotContain("ToGodot(pose.Position)", main, StringComparison.Ordinal);
    }
}
