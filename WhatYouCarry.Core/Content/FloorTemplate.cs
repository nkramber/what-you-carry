using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Core.Content;

/// <summary>
/// One floor template (D-6, D-46, D-166, D-167, D-210, D-252). The floor generator reads one to dig a floor.
/// </summary>
/// <remarks>
/// The size is one per band, in blocks (D-252). The validator rejects a size past the grid limit of D-164, and a
/// size below the smallest floor that the dig plan can carve: the plan keeps a shell of rock around the floor,
/// and it needs six rows for one tunnel floor row, four air rows, and the shell.
/// </remarks>
public sealed record FloorTemplate(string Id, long MinDepth, long MaxDepth, long RoomCountMin, long RoomCountMax, long DifficultyBudget, string Band, int SizeX, int SizeY, int SizeZ)
{
    /// <summary>The smallest size along X or Z that the dig plan can carve, in blocks.</summary>
    public const int MinSizeXZ = 24;

    /// <summary>The smallest size along Y that the dig plan can carve, in blocks: the base row, one floor row, four air rows, and the top row.</summary>
    public const int MinSizeY = 7;

    /// <summary>The names that a floor template must carry.</summary>
    public static readonly IReadOnlyList<string> Required =
    [
        "id",
        "minDepth",
        "maxDepth",
        "roomCountMin",
        "roomCountMax",
        "difficultyBudget",
        "band",
        "sizeX",
        "sizeY",
        "sizeZ",
    ];

    /// <summary>The names that a floor template can carry.</summary>
    public static readonly IReadOnlyList<string> Optional = [];

    /// <summary>One template from one validated object.</summary>
    /// <exception cref="Logging.ContextException">A field is absent, unknown, or of another kind, or a size is outside its bounds.</exception>
    public static FloorTemplate FromMembers(string path, IReadOnlyList<JsonMember> members)
    {
        ContentValidator.Check(path, members, Required, Optional);

        long minDepth = Number(path, members, "minDepth");
        long maxDepth = Number(path, members, "maxDepth");
        long roomMin = Number(path, members, "roomCountMin");
        long roomMax = Number(path, members, "roomCountMax");
        long budget = Number(path, members, "difficultyBudget");

        if (minDepth > maxDepth)
        {
            throw ContentError.Make(path, "minDepth", "is above maxDepth, and a band covers no floor then");
        }

        if (roomMin > roomMax)
        {
            throw ContentError.Make(path, "roomCountMin", "is above roomCountMax, and no room count fits then");
        }

        if (roomMin < 1)
        {
            throw ContentError.Make(path, "roomCountMin", "is below one, and every floor holds a room");
        }

        if (budget < 10)
        {
            throw ContentError.Make(path, "difficultyBudget", "is below ten, and the budget window of D-167 is one tenth of the budget");
        }

        int sizeX = Size(path, members, "sizeX", MinSizeXZ, VoxelGrid.MaxSizeX);
        int sizeY = Size(path, members, "sizeY", MinSizeY, VoxelGrid.MaxSizeY);
        int sizeZ = Size(path, members, "sizeZ", MinSizeXZ, VoxelGrid.MaxSizeZ);

        return new FloorTemplate(
            ContentValidator.Value(path, members, "id", JsonMemberKind.Text),
            minDepth,
            maxDepth,
            roomMin,
            roomMax,
            budget,
            ContentValidator.Value(path, members, "band", JsonMemberKind.Text),
            sizeX,
            sizeY,
            sizeZ);
    }

    /// <summary>One size field, inside its bounds (D-164, D-252).</summary>
    private static int Size(string path, IReadOnlyList<JsonMember> members, string name, int minimum, int maximum)
    {
        long size = Number(path, members, name);
        if (size < minimum || size > maximum)
        {
            throw ContentError.Make(path, name, $"is {size}, and a floor size on this axis is from {minimum} to {maximum} blocks (D-164, D-252)");
        }

        return (int)size;
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
