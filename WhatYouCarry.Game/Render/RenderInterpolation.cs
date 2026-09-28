using System;
using WhatYouCarry.Core.Camera;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Core.World;
using CoreVector3 = WhatYouCarry.Core.Physics.Vector3;

namespace WhatYouCarry.Game.Render;

/// <summary>
/// The render state between two ticks (D-73, D-245). Core moves in whole ticks, and a frame falls between two
/// of them, so the Game layer draws each thing at the point between its last two tick positions. The camera is the
/// exception: its look interpolates, and the orbit camera places the pose from it (D-724).
/// </summary>
public static class RenderInterpolation
{
    /// <summary>The point at a fraction of the way from the previous tick position to the current one.</summary>
    public static CoreVector3 Between(CoreVector3 previous, CoreVector3 current, float fraction)
    {
        return previous + ((current - previous) * fraction);
    }

    /// <summary>
    /// The camera pose of a frame between two ticks (D-724). The feet are the interpolated feet of the frame. The look
    /// interpolates between the two tick looks, and the orbit camera places the pose from it, so the drawn camera stays
    /// on the boom circle through a fast turn. An interpolation of the two tick poses cut across that circle and drew
    /// a frame near the head (F-202).
    /// </summary>
    public static CameraPose Camera(VoxelGrid grid, CoreVector3 feet, TickLook previous, TickLook current, float fraction)
    {
        int yaw = Yaw(previous.Yaw, current.Yaw, fraction);
        int pitch = previous.Pitch + (int)MathF.Round((current.Pitch - previous.Pitch) * fraction);
        return OrbitCamera.Place(grid, feet, yaw, pitch);
    }

    /// <summary>
    /// The yaw sum at a fraction of the way between two tick yaws, in hundredths of a degree, from 0 up to but not
    /// including a full turn (D-227). The yaw turns the short way across the wrap. A turn of exactly half a circle
    /// turns counterclockwise, seen from above (D-234).
    /// </summary>
    public static int Yaw(int previous, int current, float fraction)
    {
        int turn = current - previous;
        if (turn > SimulationLoop.FullTurn / 2)
        {
            turn -= SimulationLoop.FullTurn;
        }
        else if (turn <= -SimulationLoop.FullTurn / 2)
        {
            turn += SimulationLoop.FullTurn;
        }

        int yaw = previous + (int)MathF.Round(turn * fraction);
        return ((yaw % SimulationLoop.FullTurn) + SimulationLoop.FullTurn) % SimulationLoop.FullTurn;
    }

    /// <summary>A Core vector as an engine vector. The two share one frame, so no component changes (D-234).</summary>
    public static Godot.Vector3 ToGodot(CoreVector3 vector)
    {
        return new Godot.Vector3(vector.X, vector.Y, vector.Z);
    }
}
