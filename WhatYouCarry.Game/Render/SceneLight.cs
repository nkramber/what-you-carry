using Godot;

namespace WhatYouCarry.Game.Render;

/// <summary>
/// The light of play and of the contact sheet (D-59, D-81, D-678, D-679): a dark, cool ambient light, and one warm
/// omni light, the lantern, that the player carries. No light casts a shadow, and the scene has no directional light.
/// </summary>
/// <remarks>
/// The contact sheet approval of PR-77 exit test 3 fixes the color, the energy, the range, and the offset (D-679,
/// D-680, D-681).
/// </remarks>
public static class SceneLight
{
    /// <summary>
    /// The brightness of the ambient light (D-681). Every face that the lantern does not reach reads this light alone.
    /// The engine reads <see cref="AmbientColor"/> in sRGB, so the energy 1 gives about 2 percent in linear light.
    /// </summary>
    public const float AmbientEnergy = 4.0f;

    /// <summary>The brightness of the lantern at its middle.</summary>
    public const float LanternEnergy = 1.6f;

    /// <summary>The distance in meters at which the lantern light ends.</summary>
    public const float LanternRange = 7.0f;

    /// <summary>The height of the lantern above the root of the player model, in meters: above the head (D-680).</summary>
    public const float LanternHeight = 2.1f;

    /// <summary>
    /// The distance of the lantern behind the center of the player, in meters (D-680). The body faces minus Z at yaw
    /// zero, and the play camera stands behind it, so the lantern lights the back that the camera sees.
    /// </summary>
    public const float LanternBack = 0.5f;

    /// <summary>The color of the ambient light: a dark, cool gray blue (D-678).</summary>
    public static readonly Color AmbientColor = new(0.16f, 0.18f, 0.24f);

    /// <summary>The color behind every face: near black, so the mine has no sky.</summary>
    public static readonly Color BackgroundColor = new(0.01f, 0.01f, 0.015f);

    /// <summary>The color of the lantern: a warm flame orange (D-59).</summary>
    public static readonly Color LanternColor = new(1.0f, 0.72f, 0.45f);

    /// <summary>The place of the lantern in the frame of the player model root: above and behind the head (D-680).</summary>
    public static readonly Vector3 LanternOffset = new(0.0f, LanternHeight, LanternBack);

    /// <summary>The environment node that holds the background and the ambient light of one world.</summary>
    public static WorldEnvironment Environment()
    {
        Environment environment = new()
        {
            BackgroundMode = Godot.Environment.BGMode.Color,
            BackgroundColor = BackgroundColor,
            AmbientLightSource = Godot.Environment.AmbientSource.Color,
            AmbientLightColor = AmbientColor,
            AmbientLightEnergy = AmbientEnergy,
        };
        return new WorldEnvironment { Environment = environment };
    }

    /// <summary>
    /// The lantern: one omni light with no shadow (D-81). It gives no specular highlight, so a block face and a model
    /// face read as matte, as the texels paint them.
    /// </summary>
    public static OmniLight3D Lantern()
    {
        return new OmniLight3D
        {
            LightColor = LanternColor,
            LightEnergy = LanternEnergy,
            LightSpecular = 0.0f,
            OmniRange = LanternRange,
            ShadowEnabled = false,
        };
    }
}
