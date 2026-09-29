using System.Collections.Generic;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Core.Content;

/// <summary>
/// Reads a content set, validates each file against the validator for its type, and returns typed records
/// (D-91, D-92, D-168, D-219). A failure names the file, the field, and the reason (T-2).
/// </summary>
/// <remarks>
/// <para>
/// The loader opens no file. It takes the bytes from an <see cref="IContentSource"/>, so the Game layer owns
/// the disk and Core owns the shape (D-211, D-219).
/// </para>
/// <para>
/// The loader sorts the files by path before it reads them, in the ordinal order of the content hash, so every
/// typed list has one order on every platform. The floor generator draws chamber kinds by index, so a list in
/// the order of a directory walk would dig another floor from one seed on another file system (D-159, G-9).
/// </para>
/// </remarks>
public sealed class ContentLoader
{
    /// <summary>The directory that holds every floor template.</summary>
    public const string FloorDirectory = "floors/";

    /// <summary>The directory that holds every chamber kind (D-255).</summary>
    public const string ChamberDirectory = "chambers/";

    /// <summary>The directory that holds every projectile definition.</summary>
    public const string ProjectileDirectory = "projectiles/";

    /// <summary>The directory that holds every weapon definition (D-334).</summary>
    public const string WeaponDirectory = "weapons/";

    /// <summary>The directory that holds every enemy family (D-395).</summary>
    public const string EnemyDirectory = "enemies/";

    /// <summary>The directory that holds the one hunter of the timer (D-45, D-409).</summary>
    public const string HunterDirectory = "hunter/";

    /// <summary>The directory that holds every item definition (D-749).</summary>
    public const string ItemDirectory = "items/";

    /// <summary>The directory that holds every affix definition (D-747).</summary>
    public const string AffixDirectory = "affixes/";

    /// <summary>The id of the weapon that the attack bit swings, until the loadout of PR-30. A content set without it fails to load (D-422).</summary>
    public const string MainWeaponId = "sword-basic";

    /// <summary>
    /// The directory of the models and the animations, which Core never reads (D-631, D-298). Every content
    /// source skips it, so an animation file there is not a content file of this loader.
    /// </summary>
    public const string ModelDirectory = "models/";

    /// <summary>The file extension of a model under the model directory: the project file that Blockbench writes (D-631).</summary>
    public const string ModelExtension = ".bbmodel";

    /// <summary>The file extension of an animation under the model directory (D-298).</summary>
    public const string AnimationExtension = ".json";

    /// <summary>
    /// The directory of the palette, the recipes, the atlas, and the layout, which Core never reads (D-305, D-505).
    /// Every content source skips it, so a palette change leaves the content hash of D-163 as it was.
    /// </summary>
    public const string TextureDirectory = "textures/";

    /// <summary>
    /// The directory of the sound parameter files and the rendered sounds, which Core never reads (D-453). Every
    /// content source skips it, so a sound change leaves the content hash of D-163 as it was.
    /// </summary>
    public const string AudioDirectory = "audio/";

    /// <summary>Answers whether a content path lies in a directory that every content source skips: the models, the textures, or the audio (D-298, D-305, D-453).</summary>
    public static bool IsAssetPath(string contentPath)
    {
        return contentPath.StartsWith(ModelDirectory, System.StringComparison.Ordinal)
            || contentPath.StartsWith(TextureDirectory, System.StringComparison.Ordinal)
            || contentPath.StartsWith(AudioDirectory, System.StringComparison.Ordinal);
    }

    private readonly IContentSource source;

    /// <summary>A loader that reads one source.</summary>
    public ContentLoader(IContentSource source)
    {
        this.source = source;
    }

    /// <summary>Every content record of one source, and the hash of the set.</summary>
    /// <exception cref="ContextException">A file is not valid, or the set has no string table.</exception>
    public ContentSet Load()
    {
        List<ContentFile> files = [];
        foreach (ContentFile file in this.source.Read())
        {
            files.Add(file);
        }

        files.Sort(static (first, second) => string.CompareOrdinal(first.Path, second.Path));
        string hash = ContentHash.Of(files);

        List<FloorTemplate> floors = [];
        List<ChamberKind> chambers = [];
        List<ProjectileDefinition> projectiles = [];
        List<WeaponDefinition> weapons = [];
        List<EnemyDefinition> enemies = [];
        List<string> enemyPaths = [];
        List<HunterDefinition> hunters = [];
        List<ItemDefinition> items = [];
        List<string> itemPaths = [];
        List<AffixDefinition> affixes = [];
        List<string> hunterPaths = [];
        List<RecordId> ids = [];
        Strings? strings = null;

        foreach (ContentFile file in files)
        {
            IReadOnlyList<JsonMember> members = JsonObjectReader.Read(file.Path, file.Bytes);

            if (file.Path == Strings.FilePath)
            {
                strings = Strings.FromMembers(file.Path, members);
            }
            else if (file.Path.StartsWith(FloorDirectory, System.StringComparison.Ordinal))
            {
                FloorTemplate floor = FloorTemplate.FromMembers(file.Path, members);
                floors.Add(floor);
                ids.Add(new RecordId("floor templates", file.Path, floor.Id));
            }
            else if (file.Path.StartsWith(ChamberDirectory, System.StringComparison.Ordinal))
            {
                ChamberKind chamber = ChamberKind.FromMembers(file.Path, members);
                chambers.Add(chamber);
                ids.Add(new RecordId("chamber kinds", file.Path, chamber.Id));
            }
            else if (file.Path.StartsWith(ProjectileDirectory, System.StringComparison.Ordinal))
            {
                ProjectileDefinition projectile = ProjectileDefinition.FromMembers(file.Path, members);
                projectiles.Add(projectile);
                ids.Add(new RecordId("projectile definitions", file.Path, projectile.Id));
            }
            else if (file.Path.StartsWith(WeaponDirectory, System.StringComparison.Ordinal))
            {
                WeaponDefinition weapon = WeaponDefinition.FromMembers(file.Path, members);
                weapons.Add(weapon);
                ids.Add(new RecordId("weapon definitions", file.Path, weapon.Id));
            }
            else if (file.Path.StartsWith(EnemyDirectory, System.StringComparison.Ordinal))
            {
                EnemyDefinition enemy = EnemyDefinition.FromMembers(file.Path, members);
                enemies.Add(enemy);
                enemyPaths.Add(file.Path);
                ids.Add(new RecordId("enemy families", file.Path, enemy.Id));
            }
            else if (file.Path.StartsWith(HunterDirectory, System.StringComparison.Ordinal))
            {
                hunters.Add(HunterDefinition.FromMembers(file.Path, members));
                hunterPaths.Add(file.Path);
            }
            else if (file.Path.StartsWith(ItemDirectory, System.StringComparison.Ordinal))
            {
                ItemDefinition item = ItemDefinition.FromMembers(file.Path, members);
                items.Add(item);
                itemPaths.Add(file.Path);
                ids.Add(new RecordId("item definitions", file.Path, item.Id));
            }
            else if (file.Path.StartsWith(AffixDirectory, System.StringComparison.Ordinal))
            {
                AffixDefinition affix = AffixDefinition.FromMembers(file.Path, members);
                affixes.Add(affix);
                ids.Add(new RecordId("affix definitions", file.Path, affix.Id));
            }
            else
            {
                // A file that no type claims is a defect of the content set, and never a file to step over.
                throw ContentError.MakeForFile(file.Path, "no content type claims this path");
            }
        }

        if (strings is null)
        {
            ContextException error = new($"The content set holds no string table at '{Strings.FilePath}'.");
            error.AddContext("file", Strings.FilePath);
            throw error;
        }

        CheckUniqueIds(ids);
        CheckEnemyWeapons(enemies, enemyPaths, weapons);
        CheckItemWeapons(items, itemPaths, weapons);
        HunterDefinition hunter = OneHunter(hunters, hunterPaths, weapons);
        CheckMainWeapon(weapons);
        return new ContentSet(hash, floors, chambers, projectiles, weapons, enemies, hunter, items, affixes, strings);
    }

    /// <summary>
    /// Two records of one type must not share an id, because a lookup would then take either one. The error names
    /// the file of each record, so the author finds both (T-2, F-150).
    /// </summary>
    private static void CheckUniqueIds(List<RecordId> ids)
    {
        for (int index = 0; index < ids.Count; index++)
        {
            for (int other = index + 1; other < ids.Count; other++)
            {
                if (ids[index].Plural == ids[other].Plural && ids[index].Id == ids[other].Id)
                {
                    throw ContentError.Make(ids[other].Path, "id", $"is '{ids[other].Id}', the id of two {ids[other].Plural}: this file and '{ids[index].Path}'");
                }
            }
        }
    }

    /// <summary>
    /// The one hunter of the set (D-56). A set with no hunter or with two is a fault, because the timer spawns one
    /// hunter at expiry (D-45). The hunter names a weapon of the set (D-413).
    /// </summary>
    private static HunterDefinition OneHunter(List<HunterDefinition> hunters, List<string> hunterPaths, List<WeaponDefinition> weapons)
    {
        if (hunters.Count != 1)
        {
            ContextException error = new($"The content set holds {hunters.Count} hunter files under '{HunterDirectory}', and it holds exactly one, because the timer spawns one hunter at expiry (D-45, D-56).");
            error.AddContext("directory", HunterDirectory);
            error.AddContext("hunters", ((long)hunters.Count).ToString(System.Globalization.CultureInfo.InvariantCulture));
            throw error;
        }

        HunterDefinition hunter = hunters[0];
        foreach (WeaponDefinition weapon in weapons)
        {
            if (weapon.Id == hunter.Weapon)
            {
                return hunter;
            }
        }

        throw ContentError.Make(hunterPaths[0], "weapon", $"names '{hunter.Weapon}', and the content set holds no weapon of that id (D-413)");
    }

    /// <summary>
    /// The set holds the main weapon, because the attack bit swings it (D-422). A set without it fails here, at
    /// the load, and not at the first loop that reads it (T-2).
    /// </summary>
    private static void CheckMainWeapon(List<WeaponDefinition> weapons)
    {
        foreach (WeaponDefinition weapon in weapons)
        {
            if (weapon.Id == MainWeaponId)
            {
                return;
            }
        }

        ContextException error = new($"The content set holds no weapon with the id '{MainWeaponId}' under '{WeaponDirectory}', and the attack bit swings it (D-422).");
        error.AddContext("directory", WeaponDirectory);
        error.AddContext("weapon", MainWeaponId);
        error.AddContext("weapons", ((long)weapons.Count).ToString(System.Globalization.CultureInfo.InvariantCulture));
        throw error;
    }

    /// <summary>
    /// Every enemy family names a weapon of the set (D-31, D-397). An id that no weapon carries is a fault of the
    /// content set, and a spawn with no weapon would swing nothing (T-2).
    /// </summary>
    private static void CheckEnemyWeapons(List<EnemyDefinition> enemies, List<string> enemyPaths, List<WeaponDefinition> weapons)
    {
        for (int index = 0; index < enemies.Count; index++)
        {
            EnemyDefinition enemy = enemies[index];
            bool found = false;
            foreach (WeaponDefinition weapon in weapons)
            {
                if (weapon.Id == enemy.Weapon)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                throw ContentError.Make(enemyPaths[index], "weapon", $"names '{enemy.Weapon}', and the content set holds no weapon of that id (D-397)");
            }
        }
    }

    /// <summary>
    /// Every weapon item names a weapon of the set, which holds its tier and its numbers (D-749, D-752). An id that no
    /// weapon carries is a fault of the content set, and a roll of the item would find no tier (T-2).
    /// </summary>
    private static void CheckItemWeapons(List<ItemDefinition> items, List<string> itemPaths, List<WeaponDefinition> weapons)
    {
        for (int index = 0; index < items.Count; index++)
        {
            ItemDefinition item = items[index];
            if (item.Weapon is null)
            {
                continue;
            }

            bool found = false;
            foreach (WeaponDefinition weapon in weapons)
            {
                if (weapon.Id == item.Weapon)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                throw ContentError.Make(itemPaths[index], "weapon", $"names '{item.Weapon}', and the content set holds no weapon of that id (D-749)");
            }
        }
    }
}

/// <summary>The id of one loaded record, the file that holds it, and the plural name of its type for the error text.</summary>
internal sealed record RecordId(string Plural, string Path, string Id);

/// <summary>Every record of one content set, and the hash that the run record header carries (D-151, D-163).</summary>
public sealed record ContentSet(string Hash, IReadOnlyList<FloorTemplate> Floors, IReadOnlyList<ChamberKind> Chambers, IReadOnlyList<ProjectileDefinition> Projectiles, IReadOnlyList<WeaponDefinition> Weapons, IReadOnlyList<EnemyDefinition> Enemies, HunterDefinition Hunter, IReadOnlyList<ItemDefinition> Items, IReadOnlyList<AffixDefinition> Affixes, Strings Strings);
