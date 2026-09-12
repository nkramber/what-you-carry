using Godot;

namespace WhatYouCarry.Game.World;

/// <summary>
/// The one material of every world chunk (D-85, D-292): the world shader with the atlas. The game sets the
/// two ends of the fade segment on every frame, from the camera to the player.
/// </summary>
/// <remarks>
/// The fade radius and the dither are OQ-160. The shader file holds the default of the radius, and this class
/// holds the same number, so a test reads one place and the shader reads the other.
/// </remarks>
public static class WorldMaterial
{
    /// <summary>The shader of every world chunk.</summary>
    public const string ShaderPath = "res://World/world.gdshader";

    /// <summary>The radius of the fade capsule, in meters (OQ-160).</summary>
    public const float FadeRadius = 0.75f;

    /// <summary>The uniform that holds the atlas.</summary>
    public const string AtlasName = "atlas";

    /// <summary>The uniform that holds the side of one tile as a fraction of the atlas.</summary>
    public const string TileSizeName = "tile_size";

    /// <summary>The uniform that holds the camera end of the fade segment.</summary>
    public const string FadeStartName = "fade_start";

    /// <summary>The uniform that holds the player end of the fade segment.</summary>
    public const string FadeEndName = "fade_end";

    /// <summary>The uniform that holds the fade radius.</summary>
    public const string FadeRadiusName = "fade_radius";

    /// <summary>The material with the shader, the atlas, and the tile size.</summary>
    public static ShaderMaterial Create(Texture2D atlas)
    {
        ShaderMaterial material = new()
        {
            Shader = GD.Load<Shader>(ShaderPath),
        };
        material.SetShaderParameter(AtlasName, atlas);
        material.SetShaderParameter(TileSizeName, new Vector2(AtlasLayout.TileSize, AtlasLayout.TileSize));
        material.SetShaderParameter(FadeRadiusName, FadeRadius);
        return material;
    }

    /// <summary>
    /// Sets the fade segment for one frame. The player end pulls back by the radius, so the capsule ends at the
    /// player and the floor under the feet stays.
    /// </summary>
    public static void SetFade(ShaderMaterial material, Vector3 camera, Vector3 player)
    {
        Vector3 toPlayer = player - camera;
        float length = toPlayer.Length();
        Vector3 end = length > FadeRadius ? player - (toPlayer * (FadeRadius / length)) : camera;
        material.SetShaderParameter(FadeStartName, camera);
        material.SetShaderParameter(FadeEndName, end);
    }
}
