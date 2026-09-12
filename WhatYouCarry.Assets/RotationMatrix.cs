using System;
using WhatYouCarry.Core.Physics;

namespace WhatYouCarry.Assets;

/// <summary>
/// A rotation of model space, as a three by three matrix. The asset QA poses a bone with one, and the Game
/// layer sets the same angles on a bone node (D-298).
/// </summary>
/// <remarks>
/// <para>
/// The euler order is the order of Blockbench (D-298): the matrix is the product Rz times Ry times Rx, so a
/// vector turns about X first, then about Y, then about Z, each about the fixed model axes. That is the matrix
/// that the editor of Blockbench builds for a group, and a keyframe copied from the editor poses the same way.
/// </para>
/// <para>
/// The sine and the cosine come from the runtime, and not from DetMath. This project is not Core, and the
/// simulation never reads a pose (OQ-159). The asset QA runs on one platform, and a difference in the last bit
/// of a pose changes no finding, because the clip rule has a tolerance for float noise (D-301).
/// </para>
/// </remarks>
public readonly record struct RotationMatrix(float M00, float M01, float M02, float M10, float M11, float M12, float M20, float M21, float M22)
{
    /// <summary>The rotation that changes nothing.</summary>
    public static readonly RotationMatrix Identity = new(1.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 1.0f);

    private const float DegreesToRadians = MathF.PI / 180.0f;

    /// <summary>The rotation of three euler angles in degrees, in the order of Blockbench (D-298).</summary>
    /// <exception cref="ArgumentOutOfRangeException">An angle is not finite.</exception>
    public static RotationMatrix FromEulerDegrees(Vector3 degrees)
    {
        if (!float.IsFinite(degrees.X) || !float.IsFinite(degrees.Y) || !float.IsFinite(degrees.Z))
        {
            throw new ArgumentOutOfRangeException(nameof(degrees), degrees, "An euler angle is not finite.");
        }

        RotationMatrix aboutX = AboutX(degrees.X * DegreesToRadians);
        RotationMatrix aboutY = AboutY(degrees.Y * DegreesToRadians);
        RotationMatrix aboutZ = AboutZ(degrees.Z * DegreesToRadians);
        return Multiply(aboutZ, Multiply(aboutY, aboutX));
    }

    /// <summary>The product of two rotations: the right one turns the vector first.</summary>
    public static RotationMatrix Multiply(RotationMatrix left, RotationMatrix right)
    {
        return new RotationMatrix(
            (left.M00 * right.M00) + (left.M01 * right.M10) + (left.M02 * right.M20),
            (left.M00 * right.M01) + (left.M01 * right.M11) + (left.M02 * right.M21),
            (left.M00 * right.M02) + (left.M01 * right.M12) + (left.M02 * right.M22),
            (left.M10 * right.M00) + (left.M11 * right.M10) + (left.M12 * right.M20),
            (left.M10 * right.M01) + (left.M11 * right.M11) + (left.M12 * right.M21),
            (left.M10 * right.M02) + (left.M11 * right.M12) + (left.M12 * right.M22),
            (left.M20 * right.M00) + (left.M21 * right.M10) + (left.M22 * right.M20),
            (left.M20 * right.M01) + (left.M21 * right.M11) + (left.M22 * right.M21),
            (left.M20 * right.M02) + (left.M21 * right.M12) + (left.M22 * right.M22));
    }

    /// <summary>The vector after the rotation.</summary>
    public Vector3 Apply(Vector3 vector)
    {
        return new Vector3(
            (this.M00 * vector.X) + (this.M01 * vector.Y) + (this.M02 * vector.Z),
            (this.M10 * vector.X) + (this.M11 * vector.Y) + (this.M12 * vector.Z),
            (this.M20 * vector.X) + (this.M21 * vector.Y) + (this.M22 * vector.Z));
    }

    /// <summary>The image of the X axis: the first column.</summary>
    public Vector3 AxisX()
    {
        return new Vector3(this.M00, this.M10, this.M20);
    }

    /// <summary>The image of the Y axis: the second column.</summary>
    public Vector3 AxisY()
    {
        return new Vector3(this.M01, this.M11, this.M21);
    }

    /// <summary>The image of the Z axis: the third column.</summary>
    public Vector3 AxisZ()
    {
        return new Vector3(this.M02, this.M12, this.M22);
    }

    private static RotationMatrix AboutX(float radians)
    {
        float cos = MathF.Cos(radians);
        float sin = MathF.Sin(radians);
        return new RotationMatrix(1.0f, 0.0f, 0.0f, 0.0f, cos, -sin, 0.0f, sin, cos);
    }

    private static RotationMatrix AboutY(float radians)
    {
        float cos = MathF.Cos(radians);
        float sin = MathF.Sin(radians);
        return new RotationMatrix(cos, 0.0f, sin, 0.0f, 1.0f, 0.0f, -sin, 0.0f, cos);
    }

    private static RotationMatrix AboutZ(float radians)
    {
        float cos = MathF.Cos(radians);
        float sin = MathF.Sin(radians);
        return new RotationMatrix(cos, -sin, 0.0f, sin, cos, 0.0f, 0.0f, 0.0f, 1.0f);
    }
}
