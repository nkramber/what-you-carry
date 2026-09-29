using System.Collections.Generic;

namespace WhatYouCarry.Core.Content;

/// <summary>
/// One item definition (D-47, D-168, D-749): the id, the slot, and for a weapon the id of its weapon file. A roll of
/// <see cref="Items.LootRoller"/> gives an item its rarity and its affixes (D-748, D-750).
/// </summary>
/// <remarks>
/// <para>
/// A weapon item names a weapon file of D-334, and that file holds the tier, the model, and the combat numbers, so
/// the numbers of a sword live in one place (D-749). The loader checks that the weapon exists.
/// </para>
/// <para>
/// A ring has no base stats, no model, and no tier field, because a ring is a pure affix carrier (D-18, D-55). A
/// rolled ring keeps the rolled tier, and that tier changes nothing (D-749). The slots of the armor come with PR-22.
/// </para>
/// </remarks>
/// <param name="Id">The item id, unique among the items.</param>
/// <param name="Slot">One of <see cref="WeaponSlot"/> and <see cref="RingSlot"/>.</param>
/// <param name="Weapon">The id of the weapon file of a weapon item, or null for a ring.</param>
public sealed record ItemDefinition(string Id, string Slot, string? Weapon)
{
    /// <summary>The slot of the main weapon (D-20).</summary>
    public const string WeaponSlot = "weapon";

    /// <summary>The slot of a ring (D-18, D-55).</summary>
    public const string RingSlot = "ring";

    /// <summary>The names that an item must carry (D-749).</summary>
    public static readonly IReadOnlyList<string> Required =
    [
        "id",
        "slot",
    ];

    /// <summary>The weapon id, which a weapon item must carry and a ring must not (D-749).</summary>
    public static readonly IReadOnlyList<string> Optional =
    [
        "weapon",
    ];

    /// <summary>One definition from one validated object.</summary>
    /// <exception cref="Logging.ContextException">A field is absent, unknown, of another kind, or not allowed for the slot.</exception>
    public static ItemDefinition FromMembers(string path, IReadOnlyList<JsonMember> members)
    {
        ContentValidator.Check(path, members, Required, Optional);

        string id = ContentValidator.Value(path, members, "id", JsonMemberKind.Text);
        string slot = ContentValidator.Value(path, members, "slot", JsonMemberKind.Text);
        bool hasWeapon = HasMember(members, "weapon");

        if (slot == WeaponSlot)
        {
            if (!hasWeapon)
            {
                throw ContentError.Make(path, "weapon", $"is absent, and an item of the slot '{WeaponSlot}' names its weapon file (D-749)");
            }

            return new ItemDefinition(id, slot, ContentValidator.Value(path, members, "weapon", JsonMemberKind.Text));
        }

        if (slot == RingSlot)
        {
            if (hasWeapon)
            {
                throw ContentError.Make(path, "weapon", $"is present, and an item of the slot '{RingSlot}' has no weapon (D-55, D-749)");
            }

            return new ItemDefinition(id, slot, null);
        }

        throw ContentError.Make(path, "slot", $"is '{slot}', and an item is of the slot '{WeaponSlot}' or '{RingSlot}' (D-749)");
    }

    /// <summary>Answers whether the object holds a member of a name.</summary>
    private static bool HasMember(IReadOnlyList<JsonMember> members, string name)
    {
        foreach (JsonMember member in members)
        {
            if (member.Name == name)
            {
                return true;
            }
        }

        return false;
    }
}
