using Godot;

namespace WhatYouCarry.Game.Render;

/// <summary>
/// The camera and the light of the scene. PR-12 held a flat floor and a player box here too, and PR-13 replaced
/// them with the chunk meshes and the player model. The light stays until the lighting of D-81 arrives.
/// </summary>
public static class PlaceholderScene
{
    /// <summary>The tilt of the light, in degrees about each axis.</summary>
    public static readonly Vector3 LightRotationDegrees = new(-55.0f, 35.0f, 0.0f);

    /// <summary>The one camera of the scene. Main places it from the Core pose on each frame (D-245).</summary>
    public static Camera3D Camera()
    {
        return new Camera3D { Current = true };
    }

    /// <summary>One directional light, so the chunks and the model have shade.</summary>
    public static DirectionalLight3D Light()
    {
        return new DirectionalLight3D { RotationDegrees = LightRotationDegrees };
    }
}
