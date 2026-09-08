using System;
using System.IO;

namespace WhatYouCarry.Tests;

/// <summary>Finds the checkout root from the test assembly location. The solution file marks the root.</summary>
public static class RepositoryRoot
{
    public const string SolutionFileName = "WhatYouCarry.slnx";

    public static string Find()
    {
        string? directory = AppContext.BaseDirectory;
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory, SolutionFileName)))
            {
                return directory;
            }

            directory = Path.GetDirectoryName(directory);
        }

        throw new InvalidOperationException(
            $"No directory above '{AppContext.BaseDirectory}' holds '{SolutionFileName}'.");
    }

    public static string ReadFile(string relativePath)
    {
        string path = Path.Combine(Find(), relativePath);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"The repository has no file '{relativePath}'.", path);
        }

        return File.ReadAllText(path);
    }
}
