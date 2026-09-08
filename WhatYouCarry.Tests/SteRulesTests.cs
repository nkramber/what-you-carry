using System;
using System.Collections.Generic;
using System.Linq;
using WhatYouCarry.Tools.SteCheck;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>One test per STE rule, each with a sentence that passes and a sentence that fails (PR-2 exit test 1).</summary>
public sealed class SteRulesTests
{
    [Fact]
    public void DescriptiveSentenceLimitIs25Words()
    {
        string twentyFive = Words(25);
        string twentySix = Words(26);
        Assert.Empty(Rules(twentyFive));
        Finding finding = Assert.Single(Rules(twentySix));
        Assert.Equal(SteRules.RuleDescriptiveLength, finding.Rule);
        Assert.Contains("26 words, the limit is 25", finding.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void ProceduralStepLimitIs20WordsUnderAProcedureHeading()
    {
        string twentyOne = Words(21);
        Assert.Empty(SteChecker.Check("f.md", $"## Procedure\n\n1. {Words(20)}\n"));
        Finding finding = Assert.Single(SteChecker.Check("f.md", $"## Procedure\n\n1. {twentyOne}\n"));
        Assert.Equal(SteRules.RuleProceduralLength, finding.Rule);
        Assert.Equal(3, finding.Line);

        // The same item outside a Sequence or Procedure section has the 25-word limit.
        Assert.Empty(SteChecker.Check("f.md", $"## Scope\n\n1. {twentyOne}\n"));
        // A bullet inside the section is not a step.
        Assert.Empty(SteChecker.Check("f.md", $"## 5. Sequence\n\n- {twentyOne}\n"));
        // The nearest heading decides, at any level.
        Assert.Empty(SteChecker.Check("f.md", $"## Procedure\n\n### Notes\n\n1. {twentyOne}\n"));
    }

    [Fact]
    public void SemicolonIsAFinding()
    {
        Assert.Empty(Rules("The tool runs, and it reports each finding."));
        Finding finding = Assert.Single(Rules("The tool runs; it reports each finding."));
        Assert.Equal(SteRules.RuleSemicolon, finding.Rule);
        // A semicolon inside a code span is part of an identifier.
        Assert.Empty(Rules("The line `a; b` is code."));
    }

    [Fact]
    public void ContractionIsAFinding()
    {
        Assert.Empty(Rules("The tool does not run, and the project's memory is the doc."));
        Finding finding = Assert.Single(Rules("The tool doesn't run."));
        Assert.Equal(SteRules.RuleContraction, finding.Rule);
        Assert.Equal(SteRules.RuleContraction, Assert.Single(Rules("It's red.")).Rule);
        Assert.Equal(SteRules.RuleContraction, Assert.Single(Rules("We'll merge it.")).Rule);
        // A possessive is not a contraction.
        Assert.Empty(Rules("The owner's account holds the token."));
    }

    [Fact]
    public void PassiveVoiceIsAFinding()
    {
        Assert.Empty(Rules("The tool wrote the file."));
        Finding finding = Assert.Single(Rules("The file was written by the tool."));
        Assert.Equal(SteRules.RulePassive, finding.Rule);
        Assert.Contains("'was written'", finding.Detail, StringComparison.Ordinal);
        // An adverb can stand between the auxiliary and the participle.
        Assert.Equal(SteRules.RulePassive, Assert.Single(Rules("The PR was never merged.")).Rule);
        Assert.Equal(SteRules.RulePassive, Assert.Single(Rules("The PR must be reviewed.")).Rule);
        // Words that end in "eed" are not participles, and a quote is opaque.
        Assert.Empty(Rules("Property tests are seed loops."));
        Assert.Empty(Rules("Not \"The file should be loaded.\" (5.3)."));
    }

    [Fact]
    public void HelperVerbIsAFinding()
    {
        Assert.Empty(Rules("The owner merges the PR. The owner must merge it. The owner will merge it."));
        Finding finding = Assert.Single(Rules("The owner should merge the PR."));
        Assert.Equal(SteRules.RuleHelperVerb, finding.Rule);
        Assert.Contains("'should'", finding.Detail, StringComparison.Ordinal);
        Assert.Equal(SteRules.RuleHelperVerb, Assert.Single(Rules("The owner may merge the PR.")).Rule);
        Assert.Equal(SteRules.RuleHelperVerb, Assert.Single(Rules("The tool has merged the PR.")).Rule);
        Assert.Equal(SteRules.RuleHelperVerb, Assert.Single(Rules("The tool is merging the PR.")).Rule);
    }

    [Fact]
    public void IngFormIsAFinding()
    {
        Assert.Empty(Rules("The run takes time. Check the file before the merge."));
        Finding finding = Assert.Single(Rules("Running the tool takes time."));
        Assert.Equal(SteRules.RuleIngForm, finding.Rule);
        Assert.Equal(SteRules.RuleIngForm, Assert.Single(Rules("Check the file before merging.")).Rule);
        // A noun, a technical name, and a hyphenated identifier are not -ing forms.
        Assert.Empty(Rules("Nothing runs. One line per finding. The cost of lighting is low. name: ste-writing"));
    }

    [Fact]
    public void OpaqueSpansCountAsOneWord()
    {
        // Rules 8.5 and 8.6: parentheses, code, and quoted text count as one word each.
        List<Sentence> sentences = Sentences("The tool (the checker) reads `docs/design.md` and the \"Ready for owner merge\" verdict.");
        Sentence sentence = Assert.Single(sentences);
        Assert.Equal(9, sentence.WordCount);
        // Rule 8.7: a hyphenated word is one word. A number is one word. An emoji is not a word.
        Assert.Equal(5, Assert.Single(Sentences("The review-gate job runs 2026-09-07 ✅")).WordCount);
    }

    [Fact]
    public void SentenceEndsAtAColonAndInsidePunctuationDoesNotEnd()
    {
        List<Sentence> sentences = Sentences("Options: the first one. Read `a.b` and \"x. y\" (see D-1.) now.");
        Assert.Equal(["Options:", "the first one.", "Read `a.b` and \"x. y\" (see D-1.) now."], sentences.Select(s => s.Text).ToArray());
    }

    [Fact]
    public void ColonInsideAWordDoesNotEndASentence()
    {
        // Review P2-1: a colon ends a sentence only when a space or the line end follows it. A time, a ratio, and a URL stay whole.
        List<Sentence> sentences = Sentences("Read https://www.asd-ste100.org/ at 10:30 in 16:9 today.");
        Assert.Equal(7, Assert.Single(sentences).WordCount);
        Assert.Equal(2, Sentences("Options: the value.").Count);
        Assert.Single(Sentences("Options:the value."));
    }

    [Fact]
    public void NestedParenthesesAreOneOpaqueWord()
    {
        // Review P2-2: rule 8.5, the complete outer span is one word, and no grammar rule reads inside it.
        Sentence sentence = Assert.Single(Sentences("Read (the (short) name) now."));
        Assert.Equal(3, sentence.WordCount);
        Assert.Equal(["Read", "(the (short) name)", "now."], sentence.Words.Select(SentenceText.Unmask).ToArray());
        Assert.Empty(Rules("The name (it was written (by hand) once) is short."));
        // An unclosed span stays plain text.
        Assert.Equal(5, Assert.Single(Sentences("Read (the (short name now.")).WordCount);
    }

    [Fact]
    public void HeadingsTablesAndFencesAreNotChecked()
    {
        string text = "# The heading was written by hand\n\n| a | b |\n|---|---|\n| was written | it's |\n\n```\nThe code was written; it's fine.\n```\n\n---\n\nThe prose is clean.\n";
        Assert.Empty(SteChecker.Check("f.md", text));
    }

    [Fact]
    public void ListMarkersQuotesAndLinksAreStripped()
    {
        List<Sentence> sentences = Sentences("- [ ] **Read** the [design](docs/design.md) file.");
        Assert.Equal("Read the design file.", Assert.Single(sentences).Text);
        Assert.Equal("In plain English: the tool reads.", Sentences("> *In plain English:* the tool reads.").Select(s => s.Text).Aggregate((a, b) => a + " " + b));
    }

    [Fact]
    public void FindingNamesFileLineRuleDetailAndSentence()
    {
        Finding finding = Assert.Single(SteChecker.Check("docs/x.md", "Clean line.\n\nThe file was written by the tool.\n"));
        Assert.Equal("docs/x.md:3: STE 3.6: the passive form 'was written'. The file was written by the tool.", finding.ToString());
    }

    private static List<Finding> Rules(string paragraph)
    {
        return SteChecker.Check("f.md", paragraph + "\n");
    }

    private static List<Sentence> Sentences(string line)
    {
        var sentences = new List<Sentence>();
        foreach (TextLine textLine in MarkdownText.Read(line + "\n"))
        {
            sentences.AddRange(SentenceText.Split(textLine));
        }

        return sentences;
    }

    /// <summary>A clean sentence of exactly <paramref name="count"/> words: "The tool reads word word ... word."</summary>
    private static string Words(int count)
    {
        var words = new List<string> { "The", "tool", "reads" };
        while (words.Count < count)
        {
            words.Add("word");
        }

        return string.Join(' ', words) + ".";
    }
}
