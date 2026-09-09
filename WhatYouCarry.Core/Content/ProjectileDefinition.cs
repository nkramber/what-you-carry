using System.Collections.Generic;
using System.Globalization;

namespace WhatYouCarry.Core.Content;

/// <summary>
/// One projectile definition (D-42, D-266, G-6). The projectile simulation fires these.
/// </summary>
/// <remarks>
/// The speed is in centimeters per second, the gravity scale in percent of the gravity of D-231, the lifetime
/// in ticks, and the spread is the half angle of the spread cone in hundredths of a degree (D-266). Every
/// number is a whole number, so the file reads the same on every platform (D-77).
/// </remarks>
public sealed record ProjectileDefinition(string Id, long SpeedCentimetres, long GravityScalePercent, long LifetimeTicks, long Damage, long AreaCentimetres, int SpreadHundredths)
{
    /// <summary>The largest spread half angle, in hundredths of a degree: a half turn.</summary>
    public const int LargestSpread = 18000;

    /// <summary>The names that a projectile definition must carry.</summary>
    public static readonly IReadOnlyList<string> Required =
    [
        "id",
        "speedCentimetres",
        "gravityScalePercent",
        "lifetimeTicks",
        "damage",
        "spreadHundredths",
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

        long speed = Number(path, members, "speedCentimetres");
        if (speed < 1)
        {
            throw ContentError.Make(path, "speedCentimetres", "is below one, and a projectile moves");
        }

        long gravityScale = Number(path, members, "gravityScalePercent");
        if (gravityScale < 0)
        {
            throw ContentError.Make(path, "gravityScalePercent", "is below zero, and gravity never lifts a projectile");
        }

        long spread = Number(path, members, "spreadHundredths");
        if (spread < 0 || spread > LargestSpread)
        {
            throw ContentError.Make(path, "spreadHundredths", $"is {spread}, and the spread half angle is from 0 to {LargestSpread} hundredths of a degree (D-266)");
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
            speed,
            gravityScale,
            lifetime,
            Number(path, members, "damage"),
            area,
            (int)spread);
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
