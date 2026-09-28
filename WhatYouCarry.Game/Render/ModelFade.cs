using Godot;

namespace WhatYouCarry.Game.Render;

/// <summary>
/// The fade of an enemy model or of the Overseer model (D-721). A model fades when its body box crosses the capsule of
/// the wall fade (D-292), or when the drawn camera is near its body box. A faded model draws with the fade material,
/// which keeps 5 pixels in 16 of the dither pattern of the wall fade, so the model stays in the opaque pass (D-632).
/// </summary>
/// <remarks>
/// The capsule test grows the body box by the fade radius on each side, and checks the segment of the wall fade
/// against the grown box. A corner of the grown box reaches past the capsule, so a model fades a little early at a
/// corner, and never late.
/// </remarks>
public static class ModelFade
{
    /// <summary>The shader of a faded model.</summary>
    public const string ShaderPath = "res://Render/model_fade.gdshader";

    /// <summary>The uniform that holds the model atlas.</summary>
    public const string AtlasName = "atlas";

    /// <summary>The uniform that holds the part of the pixels that a faded model keeps.</summary>
    public const string KeptName = "kept";

    /// <summary>The part of the pixels that a faded model keeps: 5 of the 16 thresholds of the dither pattern (D-721).</summary>
    public const float Kept = 5.0f / 16.0f;

    // The name pattern of the node search that matches every node. The type filter selects the meshes.
    private const string EveryName = "*";

    /// <summary>The straight distance from the drawn camera to the nearest point of a body box at which the model fades, in meters (D-721).</summary>
    public const float CameraReach = 1.0f;

    /// <summary>The material of a faded model: the fade shader with the model atlas, nearest filtering, and mipmaps (D-85, D-677).</summary>
    public static ShaderMaterial CreateMaterial(Texture2D atlas)
    {
        ShaderMaterial material = new()
        {
            Shader = GD.Load<Shader>(ShaderPath),
        };
        material.SetShaderParameter(AtlasName, atlas);
        material.SetShaderParameter(KeptName, Kept);
        return material;
    }

    /// <summary>The body box of a model at its feet: the box of D-165, which every enemy and the Overseer use.</summary>
    public static Aabb BodyBox(Vector3 feet)
    {
        const float halfWidth = Core.Entities.PlayerBody.HalfWidth;
        return new Aabb(
            feet - new Vector3(halfWidth, 0.0f, halfWidth),
            new Vector3(2.0f * halfWidth, Core.Entities.PlayerBody.Height, 2.0f * halfWidth));
    }

    /// <summary>Answers whether a model fades for one frame (D-721).</summary>
    /// <param name="camera">The drawn camera, where the segment of the wall fade starts.</param>
    /// <param name="fadeEnd">The player end of the segment of the wall fade, from <see cref="World.WorldMaterial.FadeEnd"/>.</param>
    /// <param name="box">The body box of the model.</param>
    public static bool Fades(Vector3 camera, Vector3 fadeEnd, Aabb box)
    {
        // The straight distance from the camera to the nearest point of the box, and not the distance on each axis, so
        // a camera off a corner of the box fades the model at 1.0 meter and not at up to 1.73.
        Vector3 nearest = camera.Clamp(box.Position, box.End);
        if (nearest.DistanceSquaredTo(camera) <= CameraReach * CameraReach)
        {
            return true;
        }

        return SegmentCrossesBox(camera, fadeEnd, box.Grow(World.WorldMaterial.FadeRadius));
    }

    /// <summary>
    /// Gives each mesh of a model tree one material. The held sword hangs under the tree, so it takes the material too.
    /// A call reads every node of the tree, so the caller calls it only when the fade of the model changes.
    /// </summary>
    public static void SetMaterial(Node3D root, Material material)
    {
        foreach (Node node in root.FindChildren(EveryName, nameof(MeshInstance3D), true, false))
        {
            ((MeshInstance3D)node).MaterialOverride = material;
        }
    }

    /// <summary>
    /// Answers whether the segment from <paramref name="start"/> to <paramref name="end"/> meets the box. This is the slab
    /// test: on each axis the segment is inside the box between two parameters, and the three ranges must overlap.
    /// </summary>
    private static bool SegmentCrossesBox(Vector3 start, Vector3 end, Aabb box)
    {
        Vector3 low = box.Position;
        Vector3 high = box.End;
        Vector3 delta = end - start;
        float enter = 0.0f;
        float leave = 1.0f;
        for (int axis = 0; axis < 3; axis++)
        {
            if (delta[axis] == 0.0f)
            {
                if (start[axis] < low[axis] || start[axis] > high[axis])
                {
                    return false;
                }

                continue;
            }

            float first = (low[axis] - start[axis]) / delta[axis];
            float second = (high[axis] - start[axis]) / delta[axis];
            enter = Mathf.Max(enter, Mathf.Min(first, second));
            leave = Mathf.Min(leave, Mathf.Max(first, second));
            if (enter > leave)
            {
                return false;
            }
        }

        return true;
    }
}
