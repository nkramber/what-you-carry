namespace WhatYouCarry.Tools.AssetQa;

/// <summary>One finding of the asset QA: the content path of the file at fault, and what is wrong with it (D-135).</summary>
/// <param name="Path">The path relative to the content directory, with forward slashes.</param>
/// <param name="Message">The finding, with the box, the bone, the tick, or the reference at fault.</param>
public sealed record AssetFinding(string Path, string Message)
{
    /// <summary>The finding as one line of the report.</summary>
    public string Line()
    {
        return $"{this.Path}: {this.Message}";
    }
}
