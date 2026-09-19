using System.Collections.Generic;

namespace WhatYouCarry.Tools.SteCheck;

/// <summary>Runs every STE rule over every sentence of one Markdown file (D-130, D-139).</summary>
public static class SteChecker
{
    public static List<Finding> Check(string relativePath, string text)
    {
        var findings = new List<Finding>();
        foreach (TextLine line in MarkdownText.Read(text))
        {
            foreach (Sentence sentence in SentenceText.Split(line))
            {
                CheckSentence(relativePath, sentence, findings);
            }
        }

        return findings;
    }

    private static void CheckSentence(string path, Sentence sentence, List<Finding> findings)
    {
        string? length = SteRules.CheckLength(sentence, out string lengthRule);
        Add(findings, path, sentence, lengthRule, length);
        if (sentence.IsFrontMatter)
        {
            // D-386: the front matter takes rule 6.3 alone. A description names the triggers of a skill, and a
            // trigger list is not a sentence that the grammar rules can read.
            return;
        }

        Add(findings, path, sentence, SteRules.RuleSemicolon, SteRules.CheckSemicolon(sentence));
        Add(findings, path, sentence, SteRules.RuleContraction, SteRules.CheckContraction(sentence));
        Add(findings, path, sentence, SteRules.RulePassive, SteRules.CheckPassive(sentence));
        Add(findings, path, sentence, SteRules.RuleHelperVerb, SteRules.CheckHelperVerb(sentence));
        Add(findings, path, sentence, SteRules.RuleIngForm, SteRules.CheckIngForm(sentence));
    }

    private static void Add(List<Finding> findings, string path, Sentence sentence, string rule, string? detail)
    {
        if (detail is not null)
        {
            findings.Add(new Finding(path, sentence.Line, rule, detail, sentence.Text));
        }
    }
}
