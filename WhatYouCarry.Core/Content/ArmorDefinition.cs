using System.Collections.Generic;
using System.Globalization;

namespace WhatYouCarry.Core.Content;

/// <summary>
/// One armor definition (D-23, D-754, D-763): the tier, the slot, the flat damage reduction, the weight, and the path
/// of the overlay model. An armor item names an armor file, as a weapon item names a weapon file (D-749).
/// </summary>
/// <remarks>
/// <para>
/// Every number is a whole number. The reduction comes off each hit that the wearer takes, never below zero, and
/// plain damage lands in full (D-759). The weight slows the walk and the sprint, extends the dodge cooldown, and
/// at 24 or more resists stagger (D-755 to D-757).
/// </para>
/// <para>
/// Core never reads the model path. The Game layer hangs the overlay from it, and the asset QA checks it (D-300,
/// D-302).
/// </para>
/// </remarks>
/// <param name="Id">The armor id, unique in the content set.</param>
/// <param name="Tier">The tier, from zero (D-48, D-760).</param>
/// <param name="Slot">One of the armor slots of <see cref="Slots"/> (D-18).</param>
/// <param name="Reduction">The damage that each hit loses, from zero (D-754).</param>
/// <param name="Weight">The weight, from zero (D-754).</param>
/// <param name="Model">The content path of the overlay model (D-300).</param>
public sealed record ArmorDefinition(string Id, long Tier, string Slot, long Reduction, long Weight, string Model)
{
    /// <summary>The slot of a head piece (D-18).</summary>
    public const string HeadSlot = "head";

    /// <summary>The slot of a chest piece (D-18).</summary>
    public const string ChestSlot = "chest";

    /// <summary>The slot of a leg piece (D-18).</summary>
    public const string LegsSlot = "legs";

    /// <summary>The slot of a feet piece (D-18).</summary>
    public const string FeetSlot = "feet";

    /// <summary>The slot of a shield, which needs a one-handed main weapon (D-18, D-26).</summary>
    public const string ShieldSlot = "shield";

    /// <summary>
    /// The largest reduction or weight of one piece. Twelve pieces of D-754 stay far below it, and the sum of the worn
    /// pieces then stays below the weight at which the walk would stop (D-757).
    /// </summary>
    public const long LargestNumber = 30;

    /// <summary>The directory of the overlay models (D-300).</summary>
    public const string ArmorModelDirectory = ContentLoader.ModelDirectory + "armor/";

    /// <summary>The slots that an armor file can name (D-18, D-761). The amulet comes with PR-28.</summary>
    public static readonly IReadOnlyList<string> Slots =
    [
        HeadSlot,
        ChestSlot,
        LegsSlot,
        FeetSlot,
        ShieldSlot,
    ];

    /// <summary>The names that an armor definition must carry (D-763).</summary>
    public static readonly IReadOnlyList<string> Required =
    [
        "id",
        "tier",
        "slot",
        "reduction",
        "weight",
        "model",
    ];

    /// <summary>An armor definition carries no optional name.</summary>
    public static readonly IReadOnlyList<string> Optional = [];

    /// <summary>Answers whether a slot is one of the armor slots.</summary>
    public static bool IsArmorSlot(string slot)
    {
        foreach (string armorSlot in Slots)
        {
            if (armorSlot == slot)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>One definition from one validated object.</summary>
    /// <exception cref="Logging.ContextException">A field is absent, unknown, of another kind, or outside its bounds.</exception>
    public static ArmorDefinition FromMembers(string path, IReadOnlyList<JsonMember> members)
    {
        ContentValidator.Check(path, members, Required, Optional);

        long tier = Number(path, members, "tier");
        if (tier < 0)
        {
            throw ContentError.Make(path, "tier", $"is {tier}, and a tier is a whole number from zero");
        }

        string slot = ContentValidator.Value(path, members, "slot", JsonMemberKind.Text);
        if (!IsArmorSlot(slot))
        {
            throw ContentError.Make(path, "slot", $"is '{slot}', and an armor piece is of the slot head, chest, legs, feet, or shield (D-18, D-763)");
        }

        return new ArmorDefinition(
            ContentValidator.Value(path, members, "id", JsonMemberKind.Text),
            tier,
            slot,
            Bounded(path, members, "reduction"),
            Bounded(path, members, "weight"),
            OverlayPath(path, members));
    }

    /// <summary>The model path, which names an overlay under the armor directory of the models (D-300).</summary>
    private static string OverlayPath(string path, IReadOnlyList<JsonMember> members)
    {
        string model = ContentValidator.AssetPath(path, members, "model", ContentLoader.ModelExtension);
        if (!model.StartsWith(ArmorModelDirectory, System.StringComparison.Ordinal))
        {
            throw ContentError.Make(path, "model", $"is '{model}', and an armor model is an overlay under '{ArmorModelDirectory}' (D-300)");
        }

        return model;
    }

    /// <summary>A reduction or a weight: a whole number from zero to <see cref="LargestNumber"/>.</summary>
    private static long Bounded(string path, IReadOnlyList<JsonMember> members, string name)
    {
        long value = Number(path, members, name);
        if (value < 0 || value > LargestNumber)
        {
            throw ContentError.Make(path, name, $"is {value}, and it is a whole number from 0 to {LargestNumber} (D-754)");
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
