using Godot;
using WhatYouCarry.Core.Entities;

namespace WhatYouCarry.Game.Render;

/// <summary>
/// The nodes of the PR-12 scene: a flat colored floor, a box for the player, the camera, and one light. PR-13
/// replaces the floor and the box with the mesher and the model loader.
/// </summary>
public static class PlaceholderScene
{
    /// <summary>The color of the floor plane.</summary>
    public static readonly Color FloorColor = new(0.36f, 0.42f, 0.30f);

    /// <summary>The color of the player box.</summary>
    public static readonly Color PlayerColor = new(0.85f, 0.65f, 0.30f);

    /// <summary>The tilt of the light, in degrees about each axis.</summary>
    public static readonly Vector3 LightRotationDegrees = new(-55.0f, 35.0f, 0.0f);

    /// <summary>A flat plane over the footprint of the grid, with its face at the given height.</summary>
    public static MeshInstance3D Floor(int sizeX, int sizeZ, float top)
    {
        PlaneMesh mesh = new()
        {
            Size = new Vector2(sizeX, sizeZ),
            Material = Material(FloorColor),
        };

        return new MeshInstance3D
        {
            Mesh = mesh,
            Position = new Vector3(sizeX / 2.0f, top, sizeZ / 2.0f),
        };
    }

    /// <summary>The box of the player body (D-165), with its origin at the box center. The caller places it at the feet plus half the height.</summary>
    public static MeshInstance3D PlayerBox()
    {
        BoxMesh mesh = new()
        {
            Size = new Vector3(2.0f * PlayerBody.HalfWidth, PlayerBody.Height, 2.0f * PlayerBody.HalfWidth),
            Material = Material(PlayerColor),
        };

        return new MeshInstance3D { Mesh = mesh };
    }

    /// <summary>The one camera of the scene. Main places it from the Core pose on each frame (D-245).</summary>
    public static Camera3D Camera()
    {
        return new Camera3D { Current = true };
    }

    /// <summary>One directional light, so the box and the floor have shade.</summary>
    public static DirectionalLight3D Light()
    {
        return new DirectionalLight3D { RotationDegrees = LightRotationDegrees };
    }

    /// <summary>A flat material of one color.</summary>
    private static StandardMaterial3D Material(Color color)
    {
        return new StandardMaterial3D { AlbedoColor = color };
    }
}
