using System.Collections.Generic;

namespace WhatYouCarry.Core.Items;

/// <summary>
/// One item of the loadout of a run record header (D-151, D-766): the item id and the ids of its affixes. The tier
/// comes from the item, and the rarity comes from the count of affixes (D-748).
/// </summary>
/// <param name="Item">The item id.</param>
/// <param name="Affixes">The affix ids of the item, in roll order. One item never carries an affix twice (D-750).</param>
public sealed record LoadoutEntry(string Item, IReadOnlyList<string> Affixes);
