using System.Collections.Generic;
using System.Globalization;

namespace WhatYouCarry.Core.Content;

/// <summary>
/// One projectile definition (D-42, G-6). PR-10 reads these to fire a projectile.
/// </summary>
public sealed record ProjectileDefinition(string Id, long SpeedCentimetres, long GravityScalePercent, long LifetimeTicks, long Damage, long AreaCentimetres)
{
    /// <summary>The names that a projectile definition must carry.</summary>
    public static readonly IReadOnlyList<string> Required =
    [
        "id",
        "speedCentimetres",
        "gravityScalePercent",
        "lifetimeTicks",
        "damage",
    ];

    /// <summary>The names that a projectile definition can carry.</summary>
    public static readonly IReadOnlyList<string> Optional = ["areaCentimetres"];

    /// <summary>One definition from one validated object.</summary>
    /// <exception cref="Logging.ContextException">A field is absent, unknown, or of another kind.</exception>
    public static ProjectileDefinition FromMembers(string path, IReadOnlyList<JsonMember> members)
    {
        ContentValidator.Check(path, members, Required, Optional);

        long lifetime = Number(path, members, "lifetimeTicks");
        if (lifetime < 1)
        {
            throw ContentError.Make(path, "lifetimeTicks", "is below one tick, and a projectile lives at least one tick");
        }

        long area = 0;
        foreach (JsonMember member in members)
        {
            if (member.Name == "areaCentimetres")
            {
                area = Number(path, members, "areaCentimetres");
            }
        }

        return new ProjectileDefinition(
            ContentValidator.Value(path, members, "id", JsonMemberKind.Text),
            Number(path, members, "speedCentimetres"),
            Number(path, members, "gravityScalePercent"),
            lifetime,
            Number(path, members, "damage"),
            area);
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
