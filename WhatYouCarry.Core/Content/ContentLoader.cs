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

    /// <summary>
    /// The directory of the models and the animations, which Core never reads (OQ-159, D-298). Every content
    /// source skips it, so an animation file there is not a content file of this loader.
    /// </summary>
    public const string ModelDirectory = "models/";

    /// <summary>The file extension of a model under the model directory: the project file that Blockbench writes (OQ-159).</summary>
    public const string ModelExtension = ".bbmodel";

    /// <summary>The file extension of an animation under the model directory (D-298).</summary>
    public const string AnimationExtension = ".json";

    /// <summary>
    /// The directory of the palette, the texture rules, and the atlas, which Core never reads (D-305). Every content
    /// source skips it, so a palette change leaves the content hash of D-163 as it was.
    /// </summary>
    public const string TextureDirectory = "textures/";

    /// <summary>Answers whether a content path lies in a directory that every content source skips: the models or the textures (D-298, D-305).</summary>
    public static bool IsAssetPath(string contentPath)
    {
        return contentPath.StartsWith(ModelDirectory, System.StringComparison.Ordinal)
            || contentPath.StartsWith(TextureDirectory, System.StringComparison.Ordinal);
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
        List<HunterDefinition> hunters = [];
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
                floors.Add(FloorTemplate.FromMembers(file.Path, members));
            }
            else if (file.Path.StartsWith(ChamberDirectory, System.StringComparison.Ordinal))
            {
                chambers.Add(ChamberKind.FromMembers(file.Path, members));
            }
            else if (file.Path.StartsWith(ProjectileDirectory, System.StringComparison.Ordinal))
            {
                projectiles.Add(ProjectileDefinition.FromMembers(file.Path, members));
            }
            else if (file.Path.StartsWith(WeaponDirectory, System.StringComparison.Ordinal))
            {
                weapons.Add(WeaponDefinition.FromMembers(file.Path, members));
            }
            else if (file.Path.StartsWith(EnemyDirectory, System.StringComparison.Ordinal))
            {
                enemies.Add(EnemyDefinition.FromMembers(file.Path, members));
            }
            else if (file.Path.StartsWith(HunterDirectory, System.StringComparison.Ordinal))
            {
                hunters.Add(HunterDefinition.FromMembers(file.Path, members));
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

        CheckUniqueIds(floors, chambers, projectiles, weapons, enemies);
        CheckEnemyWeapons(enemies, weapons);
        HunterDefinition hunter = OneHunter(hunters, weapons);
        return new ContentSet(hash, floors, chambers, projectiles, weapons, enemies, hunter, strings);
    }

    /// <summary>Two records of one type must not share an id, because a lookup would then take either one.</summary>
    private static void CheckUniqueIds(List<FloorTemplate> floors, List<ChamberKind> chambers, List<ProjectileDefinition> projectiles, List<WeaponDefinition> weapons, List<EnemyDefinition> enemies)
    {
        for (int index = 0; index < floors.Count; index++)
        {
            for (int other = index + 1; other < floors.Count; other++)
            {
                if (floors[index].Id == floors[other].Id)
                {
                    throw ContentError.Make(ContentLoader.FloorDirectory, floors[index].Id, "is the id of two floor templates");
                }
            }
        }

        for (int index = 0; index < chambers.Count; index++)
        {
            for (int other = index + 1; other < chambers.Count; other++)
            {
                if (chambers[index].Id == chambers[other].Id)
                {
                    throw ContentError.Make(ContentLoader.ChamberDirectory, chambers[index].Id, "is the id of two chamber kinds");
                }
            }
        }

        for (int index = 0; index < projectiles.Count; index++)
        {
            for (int other = index + 1; other < projectiles.Count; other++)
            {
                if (projectiles[index].Id == projectiles[other].Id)
                {
                    throw ContentError.Make(ContentLoader.ProjectileDirectory, projectiles[index].Id, "is the id of two projectile definitions");
                }
            }
        }

        for (int index = 0; index < weapons.Count; index++)
        {
            for (int other = index + 1; other < weapons.Count; other++)
            {
                if (weapons[index].Id == weapons[other].Id)
                {
                    throw ContentError.Make(ContentLoader.WeaponDirectory, weapons[index].Id, "is the id of two weapon definitions");
                }
            }
        }

        for (int index = 0; index < enemies.Count; index++)
        {
            for (int other = index + 1; other < enemies.Count; other++)
            {
                if (enemies[index].Id == enemies[other].Id)
                {
                    throw ContentError.Make(ContentLoader.EnemyDirectory, enemies[index].Id, "is the id of two enemy families");
                }
            }
        }
    }

    /// <summary>
    /// The one hunter of the set (D-56). A set with no hunter or with two is a fault, because the timer spawns one
    /// hunter at expiry (D-45). The hunter names a weapon of the set (D-413).
    /// </summary>
    private static HunterDefinition OneHunter(List<HunterDefinition> hunters, List<WeaponDefinition> weapons)
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

        throw ContentError.Make(HunterDirectory + hunter.Id, "weapon", $"names '{hunter.Weapon}', and the content set holds no weapon of that id (D-413)");
    }

    /// <summary>
    /// Every enemy family names a weapon of the set (D-31, D-397). An id that no weapon carries is a fault of the
    /// content set, and a spawn with no weapon would swing nothing (T-2).
    /// </summary>
    private static void CheckEnemyWeapons(List<EnemyDefinition> enemies, List<WeaponDefinition> weapons)
    {
        foreach (EnemyDefinition enemy in enemies)
        {
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
                throw ContentError.Make(ContentLoader.EnemyDirectory + enemy.Id, "weapon", $"names '{enemy.Weapon}', and the content set holds no weapon of that id (D-397)");
            }
        }
    }
}

/// <summary>Every record of one content set, and the hash that the run record header carries (D-151, D-163).</summary>
public sealed record ContentSet(string Hash, IReadOnlyList<FloorTemplate> Floors, IReadOnlyList<ChamberKind> Chambers, IReadOnlyList<ProjectileDefinition> Projectiles, IReadOnlyList<WeaponDefinition> Weapons, IReadOnlyList<EnemyDefinition> Enemies, HunterDefinition Hunter, Strings Strings);
