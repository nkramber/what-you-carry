using WhatYouCarry.Core.Determinism;

namespace WhatYouCarry.Core.Physics;

/// <summary>
/// A point or a displacement in the frame of Godot: right-handed, Y up, meters (D-234). Forward at yaw zero is
/// minus Z.
/// </summary>
/// <remarks>
/// This is a Core type and not <c>System.Numerics.Vector3</c>, which may use SIMD and gives another result
/// width on another platform (G-2). The lint tool reads symbols, so the shared name is not a finding (F-64).
/// Every operation here is add, subtract, multiply, divide, or the IEEE square root, so it gives the same bits
/// on every platform.
/// </remarks>
public readonly record struct Vector3(float X, float Y, float Z)
{
    /// <summary>The sum of two vectors, component by component.</summary>
    public static Vector3 operator +(Vector3 first, Vector3 second)
    {
        return new Vector3(first.X + second.X, first.Y + second.Y, first.Z + second.Z);
    }

    /// <summary>The difference of two vectors, component by component.</summary>
    public static Vector3 operator -(Vector3 first, Vector3 second)
    {
        return new Vector3(first.X - second.X, first.Y - second.Y, first.Z - second.Z);
    }

    /// <summary>The vector scaled by a number.</summary>
    public static Vector3 operator *(Vector3 vector, float scale)
    {
        return new Vector3(vector.X * scale, vector.Y * scale, vector.Z * scale);
    }

    /// <summary>The dot product of two vectors.</summary>
    public static float Dot(Vector3 first, Vector3 second)
    {
        return (first.X * second.X) + (first.Y * second.Y) + (first.Z * second.Z);
    }

    /// <summary>The cross product, which is at a right angle to both vectors and follows the right-hand rule.</summary>
    public static Vector3 Cross(Vector3 first, Vector3 second)
    {
        return new Vector3(
            (first.Y * second.Z) - (first.Z * second.Y),
            (first.Z * second.X) - (first.X * second.Z),
            (first.X * second.Y) - (first.Y * second.X));
    }

    /// <summary>The length of the vector, through the IEEE square root.</summary>
    /// <exception cref="System.ArgumentOutOfRangeException">A component is not finite.</exception>
    public float Length()
    {
        return DetMath.Sqrt(Dot(this, this));
    }
}
