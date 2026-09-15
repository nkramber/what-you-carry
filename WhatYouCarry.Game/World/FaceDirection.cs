using WhatYouCarry.Core.World;

namespace WhatYouCarry.Game.World;

/// <summary>
/// The six face directions of the world mesh, as the numbers 0 to 5: plus X, minus X, up, down, plus Z, and minus Z.
/// The axis of a direction is the number over two, and an even number points along the positive axis.
/// </summary>
public static class FaceDirection
{
    /// <summary>The count of face directions.</summary>
    public const int Count = 6;

    /// <summary>The direction toward plus X.</summary>
    public const int PlusX = 0;

    /// <summary>The direction toward minus X.</summary>
    public const int MinusX = 1;

    /// <summary>The direction toward plus Y.</summary>
    public const int Up = 2;

    /// <summary>The direction toward minus Y.</summary>
    public const int Down = 3;

    /// <summary>The direction toward plus Z.</summary>
    public const int PlusZ = 4;

    /// <summary>The direction toward minus Z.</summary>
    public const int MinusZ = 5;

    /// <summary>The axis of a direction: 0 for X, 1 for Y, and 2 for Z.</summary>
    public static int Axis(int direction)
    {
        return direction / 2;
    }

    /// <summary>The sign of a direction along its axis: 1 or -1.</summary>
    public static int Sign(int direction)
    {
        return direction % 2 == 0 ? 1 : -1;
    }

    /// <summary>The direction that points the other way along the same axis.</summary>
    public static int Opposite(int direction)
    {
        return direction % 2 == 0 ? direction + 1 : direction - 1;
    }

    /// <summary>The direction toward which the floor of a ramp rises (D-367).</summary>
    public static int Uphill(RampRise rise)
    {
        switch (rise)
        {
            case RampRise.PlusX:
                return PlusX;
            case RampRise.MinusX:
                return MinusX;
            case RampRise.PlusZ:
                return PlusZ;
            default:
                return MinusZ;
        }
    }
}
