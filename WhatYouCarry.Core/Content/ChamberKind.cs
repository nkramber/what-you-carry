using System.Collections.Generic;
using System.Globalization;

namespace WhatYouCarry.Core.Content;

/// <summary>
/// One chamber kind (D-167, D-255). The floor generator draws kinds until the sum of their weights lands inside
/// the budget window of the floor, and it digs one chamber for each kind drawn.
/// </summary>
/// <remarks>
/// A chamber is a union of boxes that overlap (D-253). The kind names how many boxes, from
/// <see cref="BoxCountMin"/> to <see cref="BoxCountMax"/>, and the side of each box along X and along Z, from
/// <see cref="BoxSizeMin"/> to <see cref="BoxSizeMax"/>. A box side is at least three, so a chamber is never
/// narrower than a tunnel (D-166). PR-16 maps the weight to enemy spawns (D-167).
/// </remarks>
public sealed record ChamberKind(string Id, long Weight, int BoxCountMin, int BoxCountMax, int BoxSizeMin, int BoxSizeMax)
{
    /// <summary>The smallest box side, in blocks: the tunnel cross-section of D-166.</summary>
    public const int SmallestBoxSide = 3;

    /// <summary>The largest box side, in blocks. It bounds the work of one chamber.</summary>
    public const int LargestBoxSide = 16;

    /// <summary>The largest box count. It bounds the work of one chamber.</summary>
    public const int LargestBoxCount = 8;

    /// <summary>The names that a chamber kind must carry.</summary>
    public static readonly IReadOnlyList<string> Required =
    [
        "id",
        "weight",
        "boxCountMin",
        "boxCountMax",
        "boxSizeMin",
        "boxSizeMax",
    ];

    /// <summary>The names that a chamber kind can carry.</summary>
    public static readonly IReadOnlyList<string> Optional = [];

    /// <summary>One kind from one validated object.</summary>
    /// <exception cref="Logging.ContextException">A field is absent, unknown, or of another kind, or a range is outside its bounds.</exception>
    public static ChamberKind FromMembers(string path, IReadOnlyList<JsonMember> members)
    {
        ContentValidator.Check(path, members, Required, Optional);

        long weight = Number(path, members, "weight");
        if (weight < 1)
        {
            throw ContentError.Make(path, "weight", "is below one, and every chamber counts toward the budget (D-167)");
        }

        int countMin = Bounded(path, members, "boxCountMin", 1, LargestBoxCount);
        int countMax = Bounded(path, members, "boxCountMax", 1, LargestBoxCount);
        if (countMin > countMax)
        {
            throw ContentError.Make(path, "boxCountMin", "is above boxCountMax, and no box count fits then");
        }

        int sizeMin = Bounded(path, members, "boxSizeMin", SmallestBoxSide, LargestBoxSide);
        int sizeMax = Bounded(path, members, "boxSizeMax", SmallestBoxSide, LargestBoxSide);
        if (sizeMin > sizeMax)
        {
            throw ContentError.Make(path, "boxSizeMin", "is above boxSizeMax, and no box size fits then");
        }

        return new ChamberKind(
            ContentValidator.Value(path, members, "id", JsonMemberKind.Text),
            weight,
            countMin,
            countMax,
            sizeMin,
            sizeMax);
    }

    /// <summary>One whole-number field inside an inclusive range.</summary>
    private static int Bounded(string path, IReadOnlyList<JsonMember> members, string name, int minimum, int maximum)
    {
        long value = Number(path, members, name);
        if (value < minimum || value > maximum)
        {
            throw ContentError.Make(path, name, $"is {value}, and this field is from {minimum} to {maximum}");
        }

        return (int)value;
    }

    private static long Number(string path, IReadOnlyList<JsonMember> members, string name)
    {
        string text = ContentValidator.Value(path, members, name, JsonMemberKind.Number);
        if (!long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out long value))
        {
            throw ContentError.Make(path, name, "holds a number that does not fit a whole number");
        }

        return value;
    }
}
