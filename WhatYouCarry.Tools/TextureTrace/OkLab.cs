using System;

namespace WhatYouCarry.Tools.TextureTrace;

/// <summary>
/// A color in OKLab: the lightness, and the two opponent axes (D-529). A distance in OKLab follows the difference that
/// the eye sees, so the trace takes the palette shade at the least distance.
/// </summary>
/// <remarks>
/// The matrices are those of Björn Ottosson, "A perceptual color space for image processing", 2020. The trace runs in
/// Tools, and it writes a texel map that the repository holds, so the floating point of the platform never reaches the
/// atlas (D-527).
/// </remarks>
public readonly record struct OkLab(double L, double A, double B)
{
    /// <summary>The OKLab color of linear-light red, green, and blue, each from 0 to 1.</summary>
    public static OkLab FromLinear(double red, double green, double blue)
    {
        double l = Math.Cbrt((0.4122214708 * red) + (0.5363325363 * green) + (0.0514459929 * blue));
        double m = Math.Cbrt((0.2119034982 * red) + (0.6806995451 * green) + (0.1073969566 * blue));
        double s = Math.Cbrt((0.0883024619 * red) + (0.2817188376 * green) + (0.6299787005 * blue));
        return new OkLab(
            (0.2104542553 * l) + (0.7936177850 * m) - (0.0040720468 * s),
            (1.9779984951 * l) - (2.4285922050 * m) + (0.4505937099 * s),
            (0.0259040371 * l) + (0.7827717662 * m) - (0.8086757660 * s));
    }

    /// <summary>The square of the distance to another color.</summary>
    public double DistanceSquared(OkLab other)
    {
        double lightness = this.L - other.L;
        double green = this.A - other.A;
        double blue = this.B - other.B;
        return (lightness * lightness) + (green * green) + (blue * blue);
    }
}
