using System.Collections.Generic;
using System.Globalization;

namespace WhatYouCarry.Core.Content;

/// <summary>
/// The hunter of the timer (D-45, D-409): the Overseer. It names its weapon, the distance that starts a swing, the
/// cadence of its swings, and its speed curve after expiry.
/// </summary>
/// <remarks>
/// <para>
/// Every number is a whole number, as in the enemy type (D-266): the distance and the speeds in centimeters, and the
/// delays in ticks. The weapon is a file of the weapon directory, and its swing runs by the player melee rules
/// (D-31, D-413).
/// </para>
/// <para>
/// The speed on a tick is the start speed plus the gain for each <c>speedGainTicks</c> ticks after expiry, and it
/// rises on every tick with no cap (D-408, D-423). One content set holds exactly one hunter (D-56).
/// </para>
/// </remarks>
/// <param name="Id">The hunter id. The cause of a death that the hunter deals carries it (D-411).</param>
/// <param name="Weapon">The weapon id that the hunter swings (D-413).</param>
/// <param name="AttackRangeCentimetres">How near the two feet centers come before a swing starts (D-423).</param>
/// <param name="AttackCooldownTicks">The ticks from the end of one swing to the tick that can start the next (D-413).</param>
/// <param name="StartSpeedCentimetresPerSecond">The speed on the tick of the spawn (D-408).</param>
/// <param name="SpeedGainCentimetresPerSecond">The speed that each <paramref name="SpeedGainTicks"/> ticks after expiry add (D-408).</param>
/// <param name="SpeedGainTicks">The ticks over which the speed rises by one gain (D-408).</param>
public sealed record HunterDefinition(
    string Id,
    string Weapon,
    long AttackRangeCentimetres,
    long AttackCooldownTicks,
    long StartSpeedCentimetresPerSecond,
    long SpeedGainCentimetresPerSecond,
    long SpeedGainTicks)
{
    /// <summary>The names that a hunter must carry.</summary>
    public static readonly IReadOnlyList<string> Required =
    [
        "id",
        "weapon",
        "attackRangeCentimetres",
        "attackCooldownTicks",
        "startSpeedCentimetresPerSecond",
        "speedGainCentimetresPerSecond",
        "speedGainTicks",
    ];

    /// <summary>A hunter carries no optional name.</summary>
    public static readonly IReadOnlyList<string> Optional = [];

    /// <summary>How near the two feet centers come before a swing starts, in meters.</summary>
    public float AttackRangeMetres => this.AttackRangeCentimetres / 100.0f;

    /// <summary>
    /// The speed of the hunter, in meters per second, a count of ticks after expiry (D-408, D-423). The count is
    /// whole ticks, so the curve gives one speed on every platform.
    /// </summary>
    public float SpeedMetresPerSecond(long ticksAfterExpiry)
    {
        float start = this.StartSpeedCentimetresPerSecond / 100.0f;
        float gainPerTick = this.SpeedGainCentimetresPerSecond / 100.0f / this.SpeedGainTicks;
        return start + (gainPerTick * ticksAfterExpiry);
    }

    /// <summary>One hunter from one validated object.</summary>
    /// <exception cref="Logging.ContextException">A field is absent, unknown, of another kind, or outside its bounds.</exception>
    public static HunterDefinition FromMembers(string path, IReadOnlyList<JsonMember> members)
    {
        ContentValidator.Check(path, members, Required, Optional);

        long gain = Number(path, members, "speedGainCentimetresPerSecond");
        if (gain < 1)
        {
            throw ContentError.Make(path, "speedGainCentimetresPerSecond", $"is {gain}, and the hunter speeds up until escape is impossible (D-45, D-408)");
        }

        return new HunterDefinition(
            ContentValidator.Value(path, members, "id", JsonMemberKind.Text),
            ContentValidator.Value(path, members, "weapon", JsonMemberKind.Text),
            AtLeastOne(path, members, "attackRangeCentimetres"),
            AtLeastOne(path, members, "attackCooldownTicks"),
            AtLeastOne(path, members, "startSpeedCentimetresPerSecond"),
            gain,
            AtLeastOne(path, members, "speedGainTicks"));
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
