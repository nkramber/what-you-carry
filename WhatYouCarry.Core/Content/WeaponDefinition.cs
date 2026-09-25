using System.Collections.Generic;
using System.Globalization;

namespace WhatYouCarry.Core.Content;

/// <summary>
/// One weapon definition (D-87, D-168, D-334): the tier, the handedness, the three phases of a swing in ticks, the
/// damage, the arc of the blade, and the paths of the weapon model and of its swing clip. Until the loadout of PR-30,
/// the loop swings the weapon with the id `sword-basic` (D-320, D-422).
/// </summary>
/// <remarks>
/// <para>
/// Every number is a whole number, as in the projectile type (D-266): the reach and the two heights in centimeters,
/// and the arc in hundredths of a degree (D-325). The two heights count from the feet of the body that swings.
/// </para>
/// <para>
/// Core never reads the two paths. The Game layer loads the model and the swing clip from them, a test matches the
/// phases of the clip to the ticks here (D-87), and the file case check of the asset QA reads both paths (D-302).
/// </para>
/// </remarks>
/// <param name="Id">The weapon id, unique in the content set.</param>
/// <param name="Tier">The tier, from zero (D-48, D-153).</param>
/// <param name="Handedness">One of <see cref="OneHanded"/> and <see cref="TwoHanded"/> (D-26, D-29).</param>
/// <param name="WindupTicks">The ticks before the blade is live.</param>
/// <param name="ActiveTicks">The ticks with the blade live.</param>
/// <param name="RecoveryTicks">The ticks after the blade, before the swing ends.</param>
/// <param name="Damage">The damage of one hit.</param>
/// <param name="ReachCentimetres">The length of the blade line from the body center, in centimeters.</param>
/// <param name="ArcHundredths">The turn of the blade across the active ticks, in hundredths of a degree.</param>
/// <param name="LowCentimetres">The bottom of the blade band over the feet, in centimeters.</param>
/// <param name="HighCentimetres">The top of the blade band over the feet, in centimeters.</param>
/// <param name="Model">The content path of the weapon model.</param>
/// <param name="Animation">The content path of the swing clip of the body.</param>
public sealed record WeaponDefinition(
    string Id,
    long Tier,
    string Handedness,
    long WindupTicks,
    long ActiveTicks,
    long RecoveryTicks,
    long Damage,
    long ReachCentimetres,
    int ArcHundredths,
    long LowCentimetres,
    long HighCentimetres,
    string Model,
    string Animation)
{
    /// <summary>The handedness of a weapon that a shield can pair with, and that a hit can interrupt (D-26, D-321).</summary>
    public const string OneHanded = "one";

    /// <summary>The handedness of a weapon whose swing no hit interrupts (D-29).</summary>
    public const string TwoHanded = "two";

    /// <summary>The largest arc, in hundredths of a degree: a half turn, so one step of the arc never passes a half turn.</summary>
    public const int LargestArc = 18000;

    /// <summary>The directory that holds the model and the swing clip (D-298).</summary>
    public const string AssetDirectory = ContentLoader.ModelDirectory;

    /// <summary>The names that a weapon definition must carry (D-334).</summary>
    public static readonly IReadOnlyList<string> Required =
    [
        "id",
        "tier",
        "handedness",
        "windupTicks",
        "activeTicks",
        "recoveryTicks",
        "damage",
        "reachCentimetres",
        "arcHundredths",
        "lowCentimetres",
        "highCentimetres",
        "model",
        "animation",
    ];

    /// <summary>A weapon definition carries no optional name.</summary>
    public static readonly IReadOnlyList<string> Optional = [];

    /// <summary>The count of ticks of one whole swing: the windup, the active ticks, and the recovery.</summary>
    public long SwingTicks => this.WindupTicks + this.ActiveTicks + this.RecoveryTicks;

    /// <summary>Answers whether no hit interrupts a swing of this weapon (D-29).</summary>
    public bool IsTwoHanded => this.Handedness == TwoHanded;

    /// <summary>One definition from one validated object.</summary>
    /// <exception cref="Logging.ContextException">A field is absent, unknown, of another kind, or outside its bounds.</exception>
    public static WeaponDefinition FromMembers(string path, IReadOnlyList<JsonMember> members)
    {
        ContentValidator.Check(path, members, Required, Optional);

        long tier = Number(path, members, "tier");
        if (tier < 0)
        {
            throw ContentError.Make(path, "tier", $"is {tier}, and a tier is a whole number from zero");
        }

        string handedness = ContentValidator.Value(path, members, "handedness", JsonMemberKind.Text);
        if (handedness != OneHanded && handedness != TwoHanded)
        {
            throw ContentError.Make(path, "handedness", $"is '{handedness}', and a weapon is '{OneHanded}' or '{TwoHanded}' (D-26, D-29)");
        }

        long windup = Ticks(path, members, "windupTicks");
        long active = Ticks(path, members, "activeTicks");
        long recovery = Ticks(path, members, "recoveryTicks");

        long damage = Number(path, members, "damage");
        if (damage < 1)
        {
            throw ContentError.Make(path, "damage", "is below one, and a hit deals damage");
        }

        long reach = Number(path, members, "reachCentimetres");
        if (reach < 1)
        {
            throw ContentError.Make(path, "reachCentimetres", "is below one, and the blade has a length");
        }

        long arc = Number(path, members, "arcHundredths");
        if (arc < 1 || arc > LargestArc)
        {
            throw ContentError.Make(path, "arcHundredths", $"is {arc}, and the arc is from 1 to {LargestArc} hundredths of a degree (D-325)");
        }

        // Each active tick turns the blade by one step of the arc. A step of no hundredth sweeps a wedge of no width, so the
        // arc holds at least one hundredth of a degree for each active tick (D-325).
        if (arc < active)
        {
            throw ContentError.Make(path, "arcHundredths", $"is {arc}, and the arc holds at least one hundredth of a degree for each of the {active} active ticks (D-325)");
        }

        long low = Number(path, members, "lowCentimetres");
        if (low < 0)
        {
            throw ContentError.Make(path, "lowCentimetres", "is below zero, and the blade band starts at the feet or over them");
        }

        long high = Number(path, members, "highCentimetres");
        if (high <= low)
        {
            throw ContentError.Make(path, "highCentimetres", "is not over 'lowCentimetres', and the blade band has a height");
        }

        return new WeaponDefinition(
            ContentValidator.Value(path, members, "id", JsonMemberKind.Text),
            tier,
            handedness,
            windup,
            active,
            recovery,
            damage,
            reach,
            (int)arc,
            low,
            high,
            AssetPath(path, members, "model", ContentLoader.ModelExtension),
            AssetPath(path, members, "animation", ContentLoader.AnimationExtension));
    }

    /// <summary>The ticks of one phase: at least one tick, and no more than the tick counter of the loop holds (D-162).</summary>
    private static long Ticks(string path, IReadOnlyList<JsonMember> members, string name)
    {
        long value = Number(path, members, name);
        if (value < 1 || value > uint.MaxValue)
        {
            throw ContentError.Make(path, name, $"is {value}, and a phase lasts from one tick to the count that the tick counter holds");
        }

        return value;
    }

    /// <summary>
    /// A path to an asset of one kind: under the model directory, with a forward slash between segments, no empty, '.',
    /// or '..' segment, and a file name that ends in the extension of its kind after a name (D-219, D-298, D-302). A
    /// path that leaves the model directory, or that names another kind of file, fails at the weapon file and not at the
    /// boot (PR #62 review P2-1).
    /// </summary>
    private static string AssetPath(string path, IReadOnlyList<JsonMember> members, string name, string extension)
    {
        string value = ContentValidator.Value(path, members, name, JsonMemberKind.Text);
        if (!value.StartsWith(AssetDirectory, System.StringComparison.Ordinal))
        {
            throw ContentError.Make(path, name, $"is '{value}', and the path starts with '{AssetDirectory}' (D-298)");
        }

        // The loop reads one segment at each slash and at the end. The last segment it reads is the file name.
        int segmentStart = 0;
        int fileNameStart = 0;
        for (int index = 0; index <= value.Length; index++)
        {
            if (index < value.Length && value[index] == '\\')
            {
                throw ContentError.Make(path, name, $"is '{value}', and a content path separates its segments with a forward slash alone (D-219)");
            }

            if (index < value.Length && value[index] != '/')
            {
                continue;
            }

            int segmentLength = index - segmentStart;
            bool dot = segmentLength == 1 && value[segmentStart] == '.';
            bool dotDot = segmentLength == 2 && value[segmentStart] == '.' && value[segmentStart + 1] == '.';
            if (segmentLength == 0 || dot || dotDot)
            {
                throw ContentError.Make(path, name, $"is '{value}', and a content path holds no empty, '.', or '..' segment (D-298)");
            }

            fileNameStart = segmentStart;
            segmentStart = index + 1;
        }

        bool endsInExtension = value.Length - fileNameStart > extension.Length;
        for (int offset = 0; endsInExtension && offset < extension.Length; offset++)
        {
            endsInExtension = value[value.Length - extension.Length + offset] == extension[offset];
        }

        if (!endsInExtension)
        {
            throw ContentError.Make(path, name, $"is '{value}', and the file name ends in '{extension}' after a name (D-298, D-334)");
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
