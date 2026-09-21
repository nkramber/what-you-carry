using System.Collections.Generic;
using System.Globalization;

namespace WhatYouCarry.Core.Content;

/// <summary>
/// One enemy family (D-168, D-395): the floors that hold it, its weight against the floor budget, its health, the
/// weapon that every spawn of the family carries, and the numbers of its senses and its cadence.
/// </summary>
/// <remarks>
/// <para>
/// Every number is a whole number, as in the weapon type (D-266): the two distances in centimeters, and the two
/// delays in ticks. A humanoid family carries a weapon of the player gear pool, and one weapon serves every spawn
/// of the family (D-31, D-397). The family holds no model path, because the Game layer draws every enemy with the
/// body model of the player until PR-62 (D-401).
/// </para>
/// <para>
/// The weight maps the room weights of D-167 to spawns: the generator fills each chamber past the first with
/// enemies for the weight of that chamber (D-398). The depth range works like the range of a floor template
/// (D-252), and a floor that no family covers holds no enemy until PR-36 to PR-42 add the other seven families.
/// </para>
/// </remarks>
/// <param name="Id">The family id, unique in the content set.</param>
/// <param name="MinDepth">The first floor that holds the family (D-395).</param>
/// <param name="MaxDepth">The last floor that holds the family (D-395).</param>
/// <param name="Weight">The weight of one spawn against the floor budget (D-167, D-399).</param>
/// <param name="Health">The health of one spawn (D-399).</param>
/// <param name="Weapon">The weapon id that every spawn carries (D-397).</param>
/// <param name="SightCentimetres">How far the family sees the player, between the two eye points (D-400).</param>
/// <param name="GiveUpTicks">The ticks with no line of sight after which a hunt ends (D-400).</param>
/// <param name="AttackRangeCentimetres">How near the two feet centers come before a swing starts (D-402).</param>
/// <param name="AttackCooldownTicks">The ticks from the end of one swing to the tick that can start the next (D-402).</param>
/// <param name="SpeedCentimetresPerSecond">How fast the family moves, in centimeters per second (D-402).</param>
public sealed record EnemyDefinition(
    string Id,
    long MinDepth,
    long MaxDepth,
    long Weight,
    long Health,
    string Weapon,
    long SightCentimetres,
    long GiveUpTicks,
    long AttackRangeCentimetres,
    long AttackCooldownTicks,
    long SpeedCentimetresPerSecond)
{
    /// <summary>The names that an enemy family must carry.</summary>
    public static readonly IReadOnlyList<string> Required =
    [
        "id",
        "minDepth",
        "maxDepth",
        "weight",
        "health",
        "weapon",
        "sightCentimetres",
        "giveUpTicks",
        "attackRangeCentimetres",
        "attackCooldownTicks",
        "speedCentimetresPerSecond",
    ];

    /// <summary>An enemy family carries no optional name.</summary>
    public static readonly IReadOnlyList<string> Optional = [];

    /// <summary>How far the family sees the player, in meters.</summary>
    public float SightMetres => this.SightCentimetres / 100.0f;

    /// <summary>How near the two feet centers come before a swing starts, in meters.</summary>
    public float AttackRangeMetres => this.AttackRangeCentimetres / 100.0f;

    /// <summary>How fast the family moves, in meters per second.</summary>
    public float SpeedMetresPerSecond => this.SpeedCentimetresPerSecond / 100.0f;

    /// <summary>Answers whether the family covers one floor (D-395).</summary>
    public bool CoversFloor(int floor)
    {
        return this.MinDepth <= floor && floor <= this.MaxDepth;
    }

    /// <summary>One family from one validated object.</summary>
    /// <exception cref="Logging.ContextException">A field is absent, unknown, of another kind, or outside its bounds.</exception>
    public static EnemyDefinition FromMembers(string path, IReadOnlyList<JsonMember> members)
    {
        ContentValidator.Check(path, members, Required, Optional);

        long minDepth = Number(path, members, "minDepth");
        long maxDepth = Number(path, members, "maxDepth");
        if (minDepth < 1)
        {
            throw ContentError.Make(path, "minDepth", $"is {minDepth}, and every run starts at floor 1 (D-3)");
        }

        if (minDepth > maxDepth)
        {
            throw ContentError.Make(path, "maxDepth", "is below minDepth, and the family covers no floor then");
        }

        long weight = Number(path, members, "weight");
        if (weight < 1)
        {
            throw ContentError.Make(path, "weight", "is below one, and every spawn counts against the floor budget (D-167)");
        }

        long health = Number(path, members, "health");
        if (health < 1)
        {
            throw ContentError.Make(path, "health", "is below one, and a spawn that starts dead is no spawn (D-322)");
        }

        return new EnemyDefinition(
            ContentValidator.Value(path, members, "id", JsonMemberKind.Text),
            minDepth,
            maxDepth,
            weight,
            health,
            ContentValidator.Value(path, members, "weapon", JsonMemberKind.Text),
            AtLeastOne(path, members, "sightCentimetres"),
            AtLeastOne(path, members, "giveUpTicks"),
            AtLeastOne(path, members, "attackRangeCentimetres"),
            AtLeastOne(path, members, "attackCooldownTicks"),
            AtLeastOne(path, members, "speedCentimetresPerSecond"));
    }

    /// <summary>One whole-number field of one or more. Each field here names a distance, a delay, or a speed, and none of the three is zero.</summary>
    private static long AtLeastOne(string path, IReadOnlyList<JsonMember> members, string name)
    {
        long value = Number(path, members, name);
        if (value < 1)
        {
            throw ContentError.Make(path, name, $"is {value}, and this field is one or more");
        }

        return value;
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
