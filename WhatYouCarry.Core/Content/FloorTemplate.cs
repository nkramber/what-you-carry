using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Core.Content;

/// <summary>
/// One floor template (D-6, D-46, D-166, D-167, D-210, D-252, D-341 to D-344). The floor generator reads one to dig a floor.
/// </summary>
/// <remarks>
/// <para>
/// The size of the floor is in blocks (D-252, D-343). The validator rejects a size past the grid limit of D-164, and a
/// size below the smallest floor that the dig plan can carve.
/// </para>
/// <para>
/// The dig sizes name the gallery, the drifts, and the chamber heights (D-341, D-342). Each is at least the three blocks
/// of D-166. A tunnel width is odd, because the brush of a walker is centered on the cell it stands on, and it fits
/// inside the rock shell of the floor. A dig height leaves three rows of the floor as rock and floor: the base row, one
/// floor row, and the top row (D-352).
/// </para>
/// </remarks>
public sealed record FloorTemplate(string Id, long MinDepth, long MaxDepth, long RoomCountMin, long RoomCountMax, long DifficultyBudget, string Band, int SizeX, int SizeY, int SizeZ, int GalleryWidth, int GalleryHeight, int DriftWidth, int DriftHeight, int ChamberHeightMin, int ChamberHeightMax)
{
    /// <summary>The smallest size along X or Z that the dig plan can carve, in blocks.</summary>
    public const int MinSizeXZ = 24;

    /// <summary>The smallest dig size, in blocks: the tunnel cross-section of D-166.</summary>
    public const int SmallestDigSize = 3;

    /// <summary>The rows of a floor that no dig height takes: the base row of the shell, one floor row, and the top row of the shell.</summary>
    public const int RowsOutsideDig = 3;

    /// <summary>The columns of a floor that no tunnel width takes on one axis: the shell on each side.</summary>
    public const int ColumnsOutsideDig = 2;

    /// <summary>The smallest size along Y that the dig plan can carve, in blocks: the smallest dig height and the rows outside it.</summary>
    public const int MinSizeY = SmallestDigSize + RowsOutsideDig;

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
        "galleryWidth",
        "galleryHeight",
        "driftWidth",
        "driftHeight",
        "chamberHeightMin",
        "chamberHeightMax",
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

        int galleryWidth = TunnelWidth(path, members, "galleryWidth", sizeX, sizeZ);
        int galleryHeight = DigHeight(path, members, "galleryHeight", sizeY);
        int driftWidth = TunnelWidth(path, members, "driftWidth", sizeX, sizeZ);
        int driftHeight = DigHeight(path, members, "driftHeight", sizeY);
        int chamberHeightMin = DigHeight(path, members, "chamberHeightMin", sizeY);
        int chamberHeightMax = DigHeight(path, members, "chamberHeightMax", sizeY);
        if (chamberHeightMin > chamberHeightMax)
        {
            throw ContentError.Make(path, "chamberHeightMin", "is above chamberHeightMax, and no chamber height fits then");
        }

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
            sizeZ,
            galleryWidth,
            galleryHeight,
            driftWidth,
            driftHeight,
            chamberHeightMin,
            chamberHeightMax);
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

    /// <summary>One tunnel width: from the cross-section of D-166 to the floor inside its shell, and odd (D-342, D-352).</summary>
    private static int TunnelWidth(string path, IReadOnlyList<JsonMember> members, string name, int sizeX, int sizeZ)
    {
        long width = Number(path, members, name);
        int narrowerSide = sizeX < sizeZ ? sizeX : sizeZ;
        int widest = narrowerSide - ColumnsOutsideDig;
        if (width < SmallestDigSize || width > widest)
        {
            throw ContentError.Make(path, name, $"is {width}, and a tunnel width on a floor of {sizeX} by {sizeZ} blocks is from {SmallestDigSize} to {widest} (D-166, D-352)");
        }

        if (width % 2 == 0)
        {
            throw ContentError.Make(path, name, $"is {width}, and a tunnel width is odd, so the brush of a walker is centered on its cell (D-352)");
        }

        return (int)width;
    }

    /// <summary>One dig height: from the cross-section of D-166 to the rows of the floor past the base row, one floor row, and the top row (D-342, D-352).</summary>
    private static int DigHeight(string path, IReadOnlyList<JsonMember> members, string name, int sizeY)
    {
        long height = Number(path, members, name);
        int tallest = sizeY - RowsOutsideDig;
        if (height < SmallestDigSize || height > tallest)
        {
            throw ContentError.Make(path, name, $"is {height}, and a dig height on a floor of {sizeY} rows is from {SmallestDigSize} to {tallest}, because the base row, one floor row, and the top row stay (D-166, D-352)");
        }

        return (int)height;
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
