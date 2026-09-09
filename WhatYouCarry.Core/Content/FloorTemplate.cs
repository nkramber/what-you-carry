using System.Collections.Generic;
using System.Globalization;

namespace WhatYouCarry.Core.Content;

/// <summary>
/// One floor template (D-6, D-46, D-166, D-167, D-210). PR-9 reads these to generate a floor.
/// </summary>
public sealed record FloorTemplate(string Id, long MinDepth, long MaxDepth, long RoomCountMin, long RoomCountMax, long DifficultyBudget, string Band)
{
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
    ];

    /// <summary>The names that a floor template can carry.</summary>
    public static readonly IReadOnlyList<string> Optional = [];

    /// <summary>One template from one validated object.</summary>
    /// <exception cref="Logging.ContextException">A field is absent, unknown, or of another kind.</exception>
    public static FloorTemplate FromMembers(string path, IReadOnlyList<JsonMember> members)
    {
        ContentValidator.Check(path, members, Required, Optional);

        long minDepth = Number(path, members, "minDepth");
        long maxDepth = Number(path, members, "maxDepth");
        long roomMin = Number(path, members, "roomCountMin");
        long roomMax = Number(path, members, "roomCountMax");

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

        return new FloorTemplate(
            ContentValidator.Value(path, members, "id", JsonMemberKind.Text),
            minDepth,
            maxDepth,
            roomMin,
            roomMax,
            Number(path, members, "difficultyBudget"),
            ContentValidator.Value(path, members, "band", JsonMemberKind.Text));
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
