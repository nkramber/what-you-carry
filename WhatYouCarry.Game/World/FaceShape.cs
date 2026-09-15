using System.Globalization;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Game.World;

/// <summary>
/// The part of one side of a cell that the solid of the cell fills, as the mesher reads it to hide a face (D-345,
/// D-367). A side is one of the six squares that bound the cell.
/// </summary>
/// <remarks>
/// <para>
/// On a side toward X or Z, the solid rises from the bottom of the cell to a height at each end of the side, in a
/// straight line from one end to the other. The low end is the end at the lower coordinate along the side: Z for a
/// side toward X, and X for a side toward Z. On the top and the bottom, the two heights are both zero or both whole.
/// A height counts in twelfths of a block, so each end of a ramp of 1:2, 1:3, or 1:4 is a whole number, and a
/// comparison is exact.
/// </para>
/// <para>
/// A block fills every side, and air and still water fill none. A ramp fills its bottom and no top. It fills each
/// end to the height of the slope at that end, and each side from the height of the slope at one end of the cell to
/// the height at the other.
/// </para>
/// </remarks>
public readonly record struct FaceShape(int LowEnd, int HighEnd)
{
    /// <summary>The height of a whole block, in twelfths.</summary>
    public const int Whole = 12;

    /// <summary>A side that the solid fills.</summary>
    public static readonly FaceShape Full = new(Whole, Whole);

    /// <summary>A side that the solid does not touch.</summary>
    public static readonly FaceShape Empty = new(0, 0);

    /// <summary>Answers whether the solid touches no part of the side.</summary>
    public bool IsEmpty => this.LowEnd == 0 && this.HighEnd == 0;

    /// <summary>Answers whether this shape covers another shape on the same side: it is at least as high at both ends.</summary>
    public bool Covers(FaceShape other)
    {
        return this.LowEnd >= other.LowEnd && this.HighEnd >= other.HighEnd;
    }

    /// <summary>
    /// The shape of the face that a block shows toward a direction: the solid of a ramp, no face for air, and a whole
    /// face for every other block, still water included (D-258).
    /// </summary>
    /// <exception cref="ContextException">The block is a ramp, and the direction is not one of the six.</exception>
    public static FaceShape OfFace(BlockId block, int direction)
    {
        if (Ramp.IsRamp(block))
        {
            return OfRamp(Ramp.FromId(block), direction);
        }

        return block == BlockId.Air ? Empty : Full;
    }

    /// <summary>
    /// The part of the side of a block toward a direction that hides the face of the neighbor there: the solid of a
    /// ramp, the whole side for a solid block, and nothing for air and still water (D-258).
    /// </summary>
    /// <exception cref="ContextException">The block is a ramp, and the direction is not one of the six.</exception>
    public static FaceShape CoverOf(BlockId block, int direction)
    {
        if (Ramp.IsRamp(block))
        {
            return OfRamp(Ramp.FromId(block), direction);
        }

        return VoxelGrid.IsSolidBlock(block) ? Full : Empty;
    }

    /// <summary>The shape of the solid of a ramp on its side toward a direction (D-345, D-367).</summary>
    /// <exception cref="ContextException">The direction is not one of the six.</exception>
    public static FaceShape OfRamp(Ramp ramp, int direction)
    {
        if (direction < 0 || direction >= FaceDirection.Count)
        {
            ContextException error = new($"A face direction is a number from 0 to {FaceDirection.Count - 1}, and it is {direction}.");
            error.AddContext(nameof(direction), direction.ToString(CultureInfo.InvariantCulture));
            throw error;
        }

        if (direction == FaceDirection.Up)
        {
            return Empty;
        }

        if (direction == FaceDirection.Down)
        {
            return Full;
        }

        int lowHeight = Whole * ramp.Place / ramp.Run;
        int highHeight = Whole * (ramp.Place + 1) / ramp.Run;
        int uphill = FaceDirection.Uphill(ramp.Rise);
        if (direction == uphill)
        {
            return new FaceShape(highHeight, highHeight);
        }

        if (direction == FaceDirection.Opposite(uphill))
        {
            return new FaceShape(lowHeight, lowHeight);
        }

        // A side along the rise. Its low end is the low end of the slope when the ramp rises toward plus X or plus Z.
        bool risesTowardHigherCoordinate = ramp.Rise == RampRise.PlusX || ramp.Rise == RampRise.PlusZ;
        return risesTowardHigherCoordinate ? new FaceShape(lowHeight, highHeight) : new FaceShape(highHeight, lowHeight);
    }
}
