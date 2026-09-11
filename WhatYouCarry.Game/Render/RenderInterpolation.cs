using WhatYouCarry.Core.Camera;
using CoreVector3 = WhatYouCarry.Core.Physics.Vector3;

namespace WhatYouCarry.Game.Render;

/// <summary>
/// The render state between two ticks (D-73, D-245). Core moves in whole ticks, and a frame falls between two
/// of them, so the Game layer draws each thing at the point between its last two tick positions.
/// </summary>
public static class RenderInterpolation
{
    /// <summary>The point at a fraction of the way from the previous tick position to the current one.</summary>
    public static CoreVector3 Between(CoreVector3 previous, CoreVector3 current, float fraction)
    {
        return previous + ((current - previous) * fraction);
    }

    /// <summary>
    /// The pose at a fraction of the way between two tick poses. Each direction interpolates on its own, so a
    /// small turn per tick stays near unit length.
    /// </summary>
    public static CameraPose Between(CameraPose previous, CameraPose current, float fraction)
    {
        return new CameraPose(
            Between(previous.Position, current.Position, fraction),
            Between(previous.Forward, current.Forward, fraction),
            Between(previous.Right, current.Right, fraction),
            Between(previous.Up, current.Up, fraction),
            Between(previous.Shoulder, current.Shoulder, fraction));
    }

    /// <summary>A Core vector as an engine vector. The two share one frame, so no component changes (D-234).</summary>
    public static Godot.Vector3 ToGodot(CoreVector3 vector)
    {
        return new Godot.Vector3(vector.X, vector.Y, vector.Z);
    }
}
