using Godot;

namespace WhatYouCarry.Game.Render;

/// <summary>
/// The camera of the scene, and the edge smoothing of every view of the world. PR-12 held a flat floor, a player box,
/// and a light here too. PR-13 replaced the floor and the box with the chunk meshes and the player model, and PR-77
/// replaced the light with <see cref="SceneLight"/>.
/// </summary>
public static class PlaceholderScene
{
    /// <summary>The vertical view of the camera, in degrees: the default of the engine, named here so the contact sheet reads the same view (D-306).</summary>
    public const float ViewDegrees = 75.0f;

    /// <summary>
    /// The antialiasing of the world (D-677): MSAA at 4x, with 2x as the fallback when the Deck frame log misses D-295.
    /// The project file sets it for the window, and each viewport that renders the world sets it for itself.
    /// </summary>
    public const Viewport.Msaa EdgeSmoothing = Viewport.Msaa.Msaa4X;

    /// <summary>The one camera of the scene, with the view of <see cref="ViewDegrees"/>. Main places it from the Core pose on each frame (D-245).</summary>
    public static Camera3D Camera()
    {
        return new Camera3D { Current = true, Fov = ViewDegrees };
    }
}
