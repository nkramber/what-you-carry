namespace WhatYouCarry.Tools.SteCheck;

/// <summary>One line of checker output: the file, the line, the rule, what the rule saw, and the text (T-2).</summary>
public sealed record Finding(string Path, int Line, string Rule, string Detail, string Text)
{
    public override string ToString()
    {
        return $"{Path}:{Line}: {Rule}: {Detail}. {Text}";
    }
}
