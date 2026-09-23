using System;
using System.Globalization;

namespace WhatYouCarry.Tools.CodexReview;

/// <summary>
/// The version that <c>codex --version</c> prints, such as <c>codex-cli 0.156.1</c> or
/// <c>codex-cli 0.155.0-alpha.9.2</c>. A prerelease of a version is older than the version itself, as in semantic
/// versioning, so a prerelease of the minimum does not pass (D-512).
/// </summary>
public sealed record CodexVersion(int Major, int Minor, int Patch, string Prerelease)
{
    public const string OutputPrefix = "codex-cli ";

    /// <summary>Reads the first line of the version output. Throws with the text when the line has another form (T-2).</summary>
    public static CodexVersion Parse(string versionOutput)
    {
        string line = versionOutput.Split('\n')[0].Trim();
        if (!line.StartsWith(OutputPrefix, StringComparison.Ordinal))
        {
            throw new FormatException($"The Codex version output '{line}' does not start with '{OutputPrefix}'.");
        }

        string version = line[OutputPrefix.Length..].Trim();
        int dash = version.IndexOf('-');
        string core = dash < 0 ? version : version[..dash];
        string prerelease = dash < 0 ? string.Empty : version[(dash + 1)..];
        string[] parts = core.Split('.');
        if (parts.Length != 3)
        {
            throw new FormatException($"The Codex version '{version}' does not have the form <major>.<minor>.<patch>.");
        }

        return new CodexVersion(ParsePart(parts[0], version), ParsePart(parts[1], version), ParsePart(parts[2], version), prerelease);
    }

    /// <summary>True when this version is the minimum or newer. The prerelease text itself is not compared.</summary>
    public bool IsAtLeast(CodexVersion minimum)
    {
        int byNumber = CompareNumbers(minimum);
        if (byNumber != 0)
        {
            return byNumber > 0;
        }

        bool thisIsRelease = Prerelease.Length == 0;
        bool minimumIsRelease = minimum.Prerelease.Length == 0;
        return thisIsRelease || !minimumIsRelease;
    }

    public override string ToString()
    {
        string core = $"{Major}.{Minor}.{Patch}";
        return Prerelease.Length == 0 ? core : $"{core}-{Prerelease}";
    }

    private int CompareNumbers(CodexVersion other)
    {
        if (Major != other.Major)
        {
            return Major.CompareTo(other.Major);
        }

        if (Minor != other.Minor)
        {
            return Minor.CompareTo(other.Minor);
        }

        return Patch.CompareTo(other.Patch);
    }

    private static int ParsePart(string part, string version)
    {
        if (!int.TryParse(part, NumberStyles.None, CultureInfo.InvariantCulture, out int value))
        {
            throw new FormatException($"The part '{part}' of the Codex version '{version}' is not a whole number.");
        }

        return value;
    }
}
