using System;
using System.Collections.Generic;
using System.IO;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Tools.AssetQa;

/// <summary>
/// Every model, overlay, and animation under the content directory, read once for the three checks (D-135).
/// A file that does not load is a finding and not a stop, so one run reports every file at fault (T-2).
/// </summary>
/// <param name="Bodies">The models at the top of the model directory, in path order.</param>
/// <param name="Overlays">The armor overlays under the armor directory, in path order (D-300).</param>
/// <param name="Animations">The animation files next to the models, in path order (D-298).</param>
/// <param name="LoadFindings">One finding per file that did not load, with the loader message.</param>
public sealed record AssetSet(IReadOnlyList<LoadedModel> Bodies, IReadOnlyList<LoadedModel> Overlays, IReadOnlyList<LoadedAnimation> Animations, IReadOnlyList<AssetFinding> LoadFindings)
{
    private const string ModelPattern = "*" + AssetPaths.ModelExtension;
    private const string AnimationPattern = "*" + AssetPaths.AnimationExtension;

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
        List<AssetFinding> findings = [];

        string modelDirectory = Path.Combine(contentRoot, AssetPaths.ModelDirectory);
        if (!Directory.Exists(modelDirectory))
        {
            return new AssetSet(bodies, overlays, animations, findings);
        }

        foreach (string file in SortedFiles(modelDirectory, ModelPattern))
        {
            ReadModel(contentRoot, file, bodies, findings);
        }

        foreach (string file in SortedFiles(modelDirectory, AnimationPattern))
        {
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

        return new AssetSet(bodies, overlays, animations, findings);
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

    /// <summary>The files of one directory with a pattern, not its subdirectories, in ordinal path order.</summary>
    private static string[] SortedFiles(string directory, string pattern)
    {
        string[] files = Directory.GetFiles(directory, pattern, SearchOption.TopDirectoryOnly);
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
