using System.Collections.Generic;

namespace WhatYouCarry.Core.Content;

/// <summary>
/// One item definition (D-47, D-168, D-749, D-763): the id, the slot, the id of the weapon file of a weapon, and the
/// id of the armor file of an armor piece. A roll of
/// <see cref="Items.LootRoller"/> gives an item its rarity and its affixes (D-748, D-750).
/// </summary>
/// <remarks>
/// <para>
/// A weapon item names a weapon file of D-334, and that file holds the tier, the model, and the combat numbers, so
/// the numbers of a sword live in one place (D-749). The loader checks that the weapon exists.
/// </para>
/// <para>
/// A ring has no base stats, no model, and no tier field, because a ring is a pure affix carrier (D-18, D-55). A
/// rolled ring keeps the rolled tier, and that tier changes nothing (D-749).
/// </para>
/// <para>
/// An armor item names an armor file, which holds the tier, the slot, the reduction, the weight, and the model
/// (D-763). The slot of the item and the slot of its armor file must match, and the loader checks both.
/// </para>
/// </remarks>
/// <param name="Id">The item id, unique among the items.</param>
/// <param name="Slot"><see cref="WeaponSlot"/>, <see cref="RingSlot"/>, or one of the armor slots of <see cref="ArmorDefinition.Slots"/>.</param>
/// <param name="Weapon">The id of the weapon file of a weapon item, or null for another item.</param>
/// <param name="Armor">The id of the armor file of an armor item, or null for another item.</param>
public sealed record ItemDefinition(string Id, string Slot, string? Weapon, string? Armor)
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

    /// <summary>The weapon id, which a weapon item alone carries, and the armor id, which an armor item alone carries (D-749, D-763).</summary>
    public static readonly IReadOnlyList<string> Optional =
    [
        "weapon",
        "armor",
    ];

    /// <summary>One definition from one validated object.</summary>
    /// <exception cref="Logging.ContextException">A field is absent, unknown, of another kind, or not allowed for the slot.</exception>
    public static ItemDefinition FromMembers(string path, IReadOnlyList<JsonMember> members)
    {
        ContentValidator.Check(path, members, Required, Optional);

        string id = ContentValidator.Value(path, members, "id", JsonMemberKind.Text);
        string slot = ContentValidator.Value(path, members, "slot", JsonMemberKind.Text);
        bool hasWeapon = HasMember(members, "weapon");
        bool hasArmor = HasMember(members, "armor");

        if (slot == WeaponSlot)
        {
            if (!hasWeapon)
            {
                throw ContentError.Make(path, "weapon", $"is absent, and an item of the slot '{WeaponSlot}' names its weapon file (D-749)");
            }

            if (hasArmor)
            {
                throw ContentError.Make(path, "armor", $"is present, and an item of the slot '{WeaponSlot}' has no armor file (D-763)");
            }

            return new ItemDefinition(id, slot, ContentValidator.Value(path, members, "weapon", JsonMemberKind.Text), null);
        }

        if (slot == RingSlot)
        {
            if (hasWeapon)
            {
                throw ContentError.Make(path, "weapon", $"is present, and an item of the slot '{RingSlot}' has no weapon (D-55, D-749)");
            }

            if (hasArmor)
            {
                throw ContentError.Make(path, "armor", $"is present, and an item of the slot '{RingSlot}' has no armor file (D-55, D-763)");
            }

            return new ItemDefinition(id, slot, null, null);
        }

        if (ArmorDefinition.IsArmorSlot(slot))
        {
            if (!hasArmor)
            {
                throw ContentError.Make(path, "armor", $"is absent, and an item of the slot '{slot}' names its armor file (D-763)");
            }

            if (hasWeapon)
            {
                throw ContentError.Make(path, "weapon", $"is present, and an item of the slot '{slot}' has no weapon (D-763)");
            }

            return new ItemDefinition(id, slot, null, ContentValidator.Value(path, members, "armor", JsonMemberKind.Text));
        }

        throw ContentError.Make(path, "slot", $"is '{slot}', and an item is of the slot '{WeaponSlot}', '{RingSlot}', head, chest, legs, feet, or shield (D-749, D-763)");
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
