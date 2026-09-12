using Godot;

namespace WhatYouCarry.Game.Render;

/// <summary>
/// The camera and the light of the scene. PR-12 held a flat floor and a player box here too, and PR-13 replaced
/// them with the chunk meshes and the player model. The light stays until the lighting of D-81 arrives.
/// </summary>
public static class PlaceholderScene
{
    /// <summary>The vertical view of the camera, in degrees: the default of the engine, named here so the contact sheet reads the same view (D-306).</summary>
    public const float ViewDegrees = 75.0f;

    /// <summary>The tilt of the light, in degrees about each axis.</summary>
    public static readonly Vector3 LightRotationDegrees = new(-55.0f, 35.0f, 0.0f);

    /// <summary>The one camera of the scene, with the view of <see cref="ViewDegrees"/>. Main places it from the Core pose on each frame (D-245).</summary>
    public static Camera3D Camera()
    {
        return new Camera3D { Current = true, Fov = ViewDegrees };
    }

    /// <summary>One directional light, so the chunks and the model have shade.</summary>
    public static DirectionalLight3D Light()
    {
        return new DirectionalLight3D { RotationDegrees = LightRotationDegrees };
    }
}
