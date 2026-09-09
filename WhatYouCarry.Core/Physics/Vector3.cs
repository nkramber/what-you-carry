namespace WhatYouCarry.Core.Physics;

/// <summary>
/// A point or a displacement in the frame of Godot: right-handed, Y up, meters (D-234). Forward at yaw zero is
/// minus Z.
/// </summary>
/// <remarks>
/// This is a Core type and not <c>System.Numerics.Vector3</c>, which may use SIMD and gives another result
/// width on another platform (G-2). The lint tool reads symbols, so the shared name is not a finding (F-64).
/// </remarks>
public readonly record struct Vector3(float X, float Y, float Z)
{
    /// <summary>The sum of two vectors, component by component.</summary>
    public static Vector3 operator +(Vector3 first, Vector3 second)
    {
        return new Vector3(first.X + second.X, first.Y + second.Y, first.Z + second.Z);
    }
}
