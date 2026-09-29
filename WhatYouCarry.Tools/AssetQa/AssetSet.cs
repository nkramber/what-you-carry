using System;
using System.Collections.Generic;
using System.IO;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Tools.AssetQa;

/// <summary>
/// Every model, overlay, and animation under the content directory, read once for the three checks (D-135).
/// A file that does not load is a finding and not a stop, so one run reports every file at fault (T-2).
/// </summary>
/// <param name="Bodies">The models under the model directory at any depth, outside the armor directory, in path order. The content loader accepts a model in a subdirectory, so the gate reads it too (D-135, F-119).</param>
/// <param name="Overlays">The armor overlays under the armor directory at any depth, in path order (D-300).</param>
/// <param name="Animations">The animation files under the model directory at any depth, in path order (D-298). A paint file of D-508 is not an animation.</param>
/// <param name="Wielded">The swing clip of each enemy family and of the hunter, in path order: the model that the file names, and the clip that its weapon names (D-742).</param>
/// <param name="LoadFindings">One finding per file that did not load, with the loader message.</param>
public sealed record AssetSet(IReadOnlyList<LoadedModel> Bodies, IReadOnlyList<LoadedModel> Overlays, IReadOnlyList<LoadedAnimation> Animations, IReadOnlyList<WieldedClip> Wielded, IReadOnlyList<AssetFinding> LoadFindings)
{
    private const string ModelPattern = "*" + AssetPaths.ModelExtension;
    private const string AnimationPattern = "*" + AssetPaths.AnimationExtension;
    private const string ContentPattern = "*.json";

    /// <summary>The set of one content directory on disk. An absent model directory is an empty set.</summary>
    /// <exception cref="ContextException">The content directory does not exist.</exception>
    public static AssetSet Read(string contentRoot)
    {
        if (!Directory.Exists(contentRoot))
        {
            ContextException error = new("The content directory does not exist.");
            error.AddContext("directory", contentRoot);
            throw error;
        }

        List<LoadedModel> bodies = [];
        List<LoadedModel> overlays = [];
        List<LoadedAnimation> animations = [];
        List<WieldedClip> wielded = [];
        List<AssetFinding> findings = [];

        string modelDirectory = Path.Combine(contentRoot, AssetPaths.ModelDirectory);
        if (!Directory.Exists(modelDirectory))
        {
            return new AssetSet(bodies, overlays, animations, wielded, findings);
        }

        foreach (string file in SortedFiles(modelDirectory, ModelPattern))
        {
            // The armor directory holds the overlays, which the loop below reads (D-300).
            if (ContentPath(contentRoot, file).StartsWith(AssetPaths.ArmorDirectory, StringComparison.Ordinal))
            {
                continue;
            }

            ReadModel(contentRoot, file, bodies, findings);
        }

        foreach (string file in SortedFiles(modelDirectory, AnimationPattern))
        {
            // A paint file names the recipes of a model, and the texture generator reads it (D-508).
            if (file.EndsWith(AssetPaths.PaintExtension, StringComparison.Ordinal))
            {
                continue;
            }

            ReadAnimation(contentRoot, file, animations, findings);
        }

        string armorDirectory = Path.Combine(contentRoot, AssetPaths.ArmorDirectory);
        if (Directory.Exists(armorDirectory))
        {
            foreach (string file in SortedFiles(armorDirectory, ModelPattern))
            {
                ReadModel(contentRoot, file, overlays, findings);
            }
        }

        ReadWielded(contentRoot, wielded, findings);
        return new AssetSet(bodies, overlays, animations, wielded, findings);
    }

    /// <summary>Answers whether a model path is a body of the set, or a body file that did not load and has its own load finding.</summary>
    public bool ReadsBody(string modelPath)
    {
        foreach (LoadedModel body in this.Bodies)
        {
            if (body.Path == modelPath)
            {
                return true;
            }
        }

        foreach (AssetFinding finding in this.LoadFindings)
        {
            if (finding.Path == modelPath)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>The body of the set at a path, or null when no body loaded from it.</summary>
    public LoadedModel? Body(string modelPath)
    {
        foreach (LoadedModel body in this.Bodies)
        {
            if (body.Path == modelPath)
            {
                return body;
            }
        }

        return null;
    }

    /// <summary>The animation of the set at a path, or null when no animation loaded from it.</summary>
    public LoadedAnimation? Animation(string animationPath)
    {
        foreach (LoadedAnimation animation in this.Animations)
        {
            if (animation.Path == animationPath)
            {
                return animation;
            }
        }

        return null;
    }

    /// <summary>Answers whether a file of the set did not load and has its own load finding.</summary>
    public bool FailedToLoad(string path)
    {
        foreach (AssetFinding finding in this.LoadFindings)
        {
            if (finding.Path == path)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>The animations of one model, in path order.</summary>
    public IReadOnlyList<LoadedAnimation> AnimationsOf(LoadedModel model)
    {
        List<LoadedAnimation> result = [];
        foreach (LoadedAnimation animation in this.Animations)
        {
            if (animation.Clip.Model == model.Path)
            {
                result.Add(animation);
            }
        }

        return result;
    }

    /// <summary>The files of one directory and its subdirectories with a pattern, in ordinal path order.</summary>
    private static string[] SortedFiles(string directory, string pattern)
    {
        string[] files = Directory.GetFiles(directory, pattern, SearchOption.AllDirectories);
        Array.Sort(files, StringComparer.Ordinal);
        return files;
    }

    private static void ReadModel(string contentRoot, string file, List<LoadedModel> models, List<AssetFinding> findings)
    {
        string path = ContentPath(contentRoot, file);
        try
        {
            models.Add(new LoadedModel(path, BlockbenchLoader.Parse(path, File.ReadAllBytes(file))));
        }
        catch (ContextException error)
        {
            findings.Add(new AssetFinding(path, error.Message));
        }
        catch (Exception error)
        {
            findings.Add(LoaderFault(path, error));
        }
    }

    private static void ReadAnimation(string contentRoot, string file, List<LoadedAnimation> animations, List<AssetFinding> findings)
    {
        string path = ContentPath(contentRoot, file);
        try
        {
            animations.Add(new LoadedAnimation(path, AnimationLoader.Parse(path, File.ReadAllBytes(file))));
        }
        catch (ContextException error)
        {
            findings.Add(new AssetFinding(path, error.Message));
        }
        catch (Exception error)
        {
            findings.Add(LoaderFault(path, error));
        }
    }

    /// <summary>
    /// The swing clip of each enemy family and of the hunter (D-742). Each file names a model and a weapon, and the
    /// weapon file names the clip. A file that does not load, and a weapon id that no weapon file holds, is a finding.
    /// </summary>
    private static void ReadWielded(string contentRoot, List<WieldedClip> wielded, List<AssetFinding> findings)
    {
        Dictionary<string, string> clipOfWeapon = new(StringComparer.Ordinal);
        foreach (string file in ContentFiles(contentRoot, ContentLoader.WeaponDirectory))
        {
            string path = ContentPath(contentRoot, file);
            WeaponDefinition? weapon = ReadContent(path, file, WeaponDefinition.FromMembers, findings);
            if (weapon is not null && !clipOfWeapon.TryAdd(weapon.Id, weapon.Animation))
            {
                findings.Add(new AssetFinding(path, $"holds the weapon id '{weapon.Id}' of another weapon file, so the swing clip of that weapon is not one clip (D-742)"));
            }
        }

        List<(string Path, string Model, string Weapon)> holders = [];
        foreach (string file in ContentFiles(contentRoot, ContentLoader.EnemyDirectory))
        {
            string path = ContentPath(contentRoot, file);
            EnemyDefinition? family = ReadContent(path, file, EnemyDefinition.FromMembers, findings);
            if (family is not null)
            {
                holders.Add((path, family.Model, family.Weapon));
            }
        }

        foreach (string file in ContentFiles(contentRoot, ContentLoader.HunterDirectory))
        {
            string path = ContentPath(contentRoot, file);
            HunterDefinition? hunter = ReadContent(path, file, HunterDefinition.FromMembers, findings);
            if (hunter is not null)
            {
                holders.Add((path, hunter.Model, hunter.Weapon));
            }
        }

        foreach ((string path, string model, string weapon) in holders)
        {
            if (clipOfWeapon.TryGetValue(weapon, out string? clip))
            {
                wielded.Add(new WieldedClip(path, model, clip));
                continue;
            }

            findings.Add(new AssetFinding(path, $"names the weapon '{weapon}', and no weapon file holds that id, so no pose reads its swing clip (D-742)"));
        }
    }

    /// <summary>The JSON files of one content directory at any depth, in ordinal path order. An absent directory has none.</summary>
    private static string[] ContentFiles(string contentRoot, string directory)
    {
        string full = Path.Combine(contentRoot, directory);
        return Directory.Exists(full) ? SortedFiles(full, ContentPattern) : [];
    }

    /// <summary>One content record of a file, or null with a finding when the file does not load.</summary>
    private static T? ReadContent<T>(string path, string file, Func<string, IReadOnlyList<JsonMember>, T> parse, List<AssetFinding> findings)
        where T : class
    {
        try
        {
            return parse(path, JsonObjectReader.Read(path, File.ReadAllBytes(file)));
        }
        catch (ContextException error)
        {
            findings.Add(new AssetFinding(path, error.Message));
            return null;
        }
        catch (Exception error)
        {
            findings.Add(LoaderFault(path, error));
            return null;
        }
    }

    /// <summary>
    /// A finding for a loader fault that is not a content error. The run still names the file and reads every other
    /// file, where the fault once ended the command with no file named (T-2, F-118).
    /// </summary>
    private static AssetFinding LoaderFault(string path, Exception error)
    {
        return new AssetFinding(path, $"the loader failed with {error.GetType().Name}: {error.Message}");
    }

    /// <summary>The path of a file relative to the content directory, with forward slashes.</summary>
    public static string ContentPath(string contentRoot, string file)
    {
        return Path.GetRelativePath(contentRoot, file).Replace('\\', '/');
    }
}

/// <summary>One model file that loaded, with its content path.</summary>
public sealed record LoadedModel(string Path, BlockbenchModel Model);

/// <summary>One animation file that loaded, with its content path.</summary>
public sealed record LoadedAnimation(string Path, AnimationClip Clip);

/// <summary>The swing clip of one enemy family or of the hunter (D-742).</summary>
/// <param name="Source">The content path of the family file or of the hunter file.</param>
/// <param name="ModelPath">The content path of the model that the file names.</param>
/// <param name="AnimationPath">The content path of the swing clip that the weapon of the file names.</param>
public sealed record WieldedClip(string Source, string ModelPath, string AnimationPath);
