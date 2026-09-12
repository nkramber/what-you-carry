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

    /// <summary>
    /// The directory of the models and the animations, which Core never reads (OQ-159, D-298). Every content
    /// source skips it, so an animation file there is not a content file of this loader.
    /// </summary>
    public const string ModelDirectory = "models/";

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

        CheckUniqueIds(floors, chambers, projectiles);
        return new ContentSet(hash, floors, chambers, projectiles, strings);
    }

    /// <summary>Two records of one type must not share an id, because a lookup would then take either one.</summary>
    private static void CheckUniqueIds(List<FloorTemplate> floors, List<ChamberKind> chambers, List<ProjectileDefinition> projectiles)
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
    }
}

/// <summary>Every record of one content set, and the hash that the run record header carries (D-151, D-163).</summary>
public sealed record ContentSet(string Hash, IReadOnlyList<FloorTemplate> Floors, IReadOnlyList<ChamberKind> Chambers, IReadOnlyList<ProjectileDefinition> Projectiles, Strings Strings);
