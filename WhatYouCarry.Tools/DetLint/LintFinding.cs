namespace WhatYouCarry.Tools.DetLint;

/// <summary>
/// One banned symbol that the lint tool found: the file, the position, the rule, the symbol, and why the rule
/// bans it (T-2).
/// </summary>
public sealed record LintFinding(string Path, int Line, int Column, string Rule, string Symbol, string Detail)
{
    public override string ToString()
    {
        return $"{this.Path}({this.Line},{this.Column}): {this.Rule}: {this.Symbol}. {this.Detail}";
    }
}
