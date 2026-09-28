using Godot;

namespace WhatYouCarry.Game.World;

/// <summary>
/// The one material of every world chunk (D-85, D-292): the world shader with the block atlas (D-677). The game sets the
/// two ends of the fade segment on every frame, from the camera to the player.
/// </summary>
/// <remarks>
/// The fade radius and the dither are the answer of D-632. The shader file holds the default of the radius, and this class
/// holds the same number, so a test reads one place and the shader reads the other.
/// </remarks>
public static class WorldMaterial
{
    /// <summary>The shader of every world chunk.</summary>
    public const string ShaderPath = "res://World/world.gdshader";

    /// <summary>The radius of the fade capsule, in meters (D-632).</summary>
    public const float FadeRadius = 0.75f;

    /// <summary>The uniform that holds the block atlas.</summary>
    public const string AtlasName = "atlas";

    /// <summary>The uniform that holds the side of one block canvas as a fraction of the block atlas.</summary>
    public const string TileSizeName = "tile_size";

    /// <summary>The uniform that holds the camera end of the fade segment.</summary>
    public const string FadeStartName = "fade_start";

    /// <summary>The uniform that holds the player end of the fade segment.</summary>
    public const string FadeEndName = "fade_end";

    /// <summary>The uniform that holds the fade radius.</summary>
    public const string FadeRadiusName = "fade_radius";

    /// <summary>The material with the shader, the block atlas of <see cref="BlockAtlas"/>, and the tile size.</summary>
    public static ShaderMaterial Create(Texture2D blockAtlas)
    {
        ShaderMaterial material = new()
        {
            Shader = GD.Load<Shader>(ShaderPath),
        };
        material.SetShaderParameter(AtlasName, blockAtlas);
        material.SetShaderParameter(TileSizeName, new Vector2(BlockTiles.Size, BlockTiles.Size));
        material.SetShaderParameter(FadeRadiusName, FadeRadius);
        return material;
    }

    /// <summary>Sets the fade segment for one frame, from the drawn camera to the end that <see cref="FadeEnd"/> gives.</summary>
    public static void SetFade(ShaderMaterial material, Vector3 camera, Vector3 player)
    {
        material.SetShaderParameter(FadeStartName, camera);
        material.SetShaderParameter(FadeEndName, FadeEnd(camera, player));
    }

    /// <summary>
    /// The player end of the fade segment. It pulls back from the player by the radius, so the capsule ends at the
    /// player and the floor under the feet stays. The model fade reads the same segment (D-721).
    /// </summary>
    public static Vector3 FadeEnd(Vector3 camera, Vector3 player)
    {
        Vector3 toPlayer = player - camera;
        float length = toPlayer.Length();
        return length > FadeRadius ? player - (toPlayer * (FadeRadius / length)) : camera;
    }
}
