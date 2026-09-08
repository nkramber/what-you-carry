using System;
using System.Collections.Generic;

namespace WhatYouCarry.Tools.SteCheck;

/// <summary>
/// The STE rules that a sentence can break (D-139). Each rule returns the finding detail, or null when the sentence passes.
/// The rules read the normalized words. An opaque word (code, a quote, or a parenthesis) is one word that no grammar rule reads.
/// </summary>
public static class SteRules
{
    public const int ProceduralWordLimit = 20;
    public const int DescriptiveWordLimit = 25;

    public const string RuleProceduralLength = "STE 5.1";
    public const string RuleDescriptiveLength = "STE 6.3";
    public const string RuleSemicolon = "STE 8.1";
    public const string RuleContraction = "STE 4.2";
    public const string RulePassive = "STE 3.6";
    public const string RuleHelperVerb = "STE 3.4";
    public const string RuleIngForm = "STE 3.5";

    private static readonly HashSet<string> Auxiliaries = new(StringComparer.Ordinal) { "is", "are", "was", "were", "be", "been", "being" };
    private static readonly HashSet<string> PerfectHelpers = new(StringComparer.Ordinal) { "has", "have", "had" };
    private static readonly HashSet<string> ModalHelpers = new(StringComparer.Ordinal) { "should", "would", "could", "might", "may", "shall", "ought" };

    /// <summary>An adverb can stand between the auxiliary and the participle: "is not merged", "was never read".</summary>
    private static readonly HashSet<string> Adverbs = new(StringComparer.Ordinal)
    {
        "not", "never", "also", "then", "now", "still", "always", "already", "only", "first", "later", "both", "all", "each", "well", "again", "once", "ever",
    };

    /// <summary>Past participles that do not end in "ed".</summary>
    private static readonly HashSet<string> IrregularParticiples = new(StringComparer.Ordinal)
    {
        "begun", "bent", "bound", "born", "bought", "bred", "broken", "brought", "built", "caught", "chosen", "cut", "dealt", "done", "drawn", "driven",
        "eaten", "fallen", "fed", "felt", "fought", "forbidden", "forgotten", "found", "frozen", "given", "gone", "ground", "grown", "heard", "held",
        "hidden", "hit", "hung", "hurt", "kept", "known", "laid", "led", "left", "lent", "let", "lit", "lost", "made", "meant", "met", "paid", "put",
        "read", "ridden", "risen", "run", "said", "seen", "sent", "set", "shown", "shut", "sold", "spent", "split", "spoken", "spread", "spun", "stood",
        "struck", "stuck", "sung", "sunk", "swept", "sworn", "taken", "taught", "thought", "thrown", "told", "torn", "understood", "woken", "won", "worn",
        "written", "wound",
    };

    /// <summary>Words that end in "ed" and are not participles.</summary>
    private static readonly HashSet<string> NotParticiples = new(StringComparer.Ordinal)
    {
        "red", "bed", "shed", "wed", "sled", "indeed", "hundred", "naked", "wicked", "sacred", "wretched", "rugged", "ragged", "crooked", "jagged", "dogged",
        "beloved", "aged", "need", "seed", "speed", "feed", "deed", "greed", "heed", "reed", "weed", "breed", "bleed", "exceed", "proceed", "succeed", "embed",
    };

    /// <summary>Words that end in "ing" and are nouns, prepositions, or technical names. The ste-writing skill lists the technical names.</summary>
    private static readonly HashSet<string> NotIngForms = new(StringComparer.Ordinal)
    {
        "thing", "nothing", "something", "anything", "everything", "during", "morning", "evening", "string", "warning", "heading", "ending",
        "lighting", "meshing", "pathfinding", "finding", "sibling", "ceiling", "spring", "ring", "king", "wing", "ping", "sing", "bring", "sling", "swing",
    };

    private static readonly HashSet<string> Prepositions = new(StringComparer.Ordinal)
    {
        "about", "above", "across", "after", "against", "along", "among", "around", "at", "before", "behind", "below", "beside", "besides", "between",
        "beyond", "by", "despite", "except", "for", "from", "in", "inside", "into", "near", "of", "off", "on", "onto", "outside", "over", "per", "since",
        "through", "throughout", "to", "toward", "towards", "under", "until", "upon", "via", "with", "within", "without",
    };

    private static readonly string[] ContractionEndings = ["n't", "'re", "'ve", "'ll", "'d", "'m", "'s"];
    private static readonly HashSet<string> ContractionBases = new(StringComparer.Ordinal)
    {
        "i", "you", "we", "they", "he", "she", "it", "who", "what", "that", "there", "here", "let", "how", "where", "when", "why",
    };

    public static string? CheckLength(Sentence sentence, out string rule)
    {
        int limit = sentence.IsProceduralStep ? ProceduralWordLimit : DescriptiveWordLimit;
        rule = sentence.IsProceduralStep ? RuleProceduralLength : RuleDescriptiveLength;
        if (sentence.WordCount <= limit)
        {
            return null;
        }

        return $"{sentence.WordCount} words, the limit is {limit}";
    }

    public static string? CheckSemicolon(Sentence sentence)
    {
        return sentence.Text.Contains(';') && !IsInsideOpaqueWord(sentence, ';') ? "a semicolon" : null;
    }

    public static string? CheckContraction(Sentence sentence)
    {
        foreach (string word in sentence.Words)
        {
            if (SentenceText.IsOpaque(word))
            {
                continue;
            }

            string normalized = SentenceText.Normalize(word).Replace('’', '\'');
            if (IsContraction(normalized))
            {
                return $"the contraction '{normalized}'";
            }
        }

        return null;
    }

    /// <summary>An auxiliary, an optional adverb, and a past participle: "is written", "was never merged".</summary>
    public static string? CheckPassive(Sentence sentence)
    {
        List<string> words = GrammarWords(sentence);
        for (int index = 0; index + 1 < words.Count; index++)
        {
            if (!Auxiliaries.Contains(words[index]))
            {
                continue;
            }

            int next = SkipAdverb(words, index + 1);
            if (next < words.Count && IsParticiple(words[next]))
            {
                return $"the passive form '{words[index]} {words[next]}'";
            }
        }

        return null;
    }

    /// <summary>A modal verb, a perfect tense ("has merged"), or a progressive tense ("is merging").</summary>
    public static string? CheckHelperVerb(Sentence sentence)
    {
        List<string> words = GrammarWords(sentence);
        for (int index = 0; index < words.Count; index++)
        {
            string word = words[index];
            if (ModalHelpers.Contains(word))
            {
                return $"the helper verb '{word}'";
            }

            if (index + 1 >= words.Count)
            {
                continue;
            }

            int next = SkipAdverb(words, index + 1);
            if (next >= words.Count)
            {
                continue;
            }

            if (PerfectHelpers.Contains(word) && IsParticiple(words[next]))
            {
                return $"the complex tense '{word} {words[next]}'";
            }

            if (Auxiliaries.Contains(word) && IsIngForm(words[next]))
            {
                return $"the complex tense '{word} {words[next]}'";
            }
        }

        return null;
    }

    /// <summary>An "-ing" form as the first word, or as the word after a preposition.</summary>
    public static string? CheckIngForm(Sentence sentence)
    {
        List<string> words = GrammarWords(sentence);
        if (words.Count > 0 && IsIngForm(words[0]))
        {
            return $"the sentence starts with '{words[0]}'";
        }

        for (int index = 0; index + 1 < words.Count; index++)
        {
            if (Prepositions.Contains(words[index]) && IsIngForm(words[index + 1]))
            {
                return $"the -ing form after a preposition '{words[index]} {words[index + 1]}'";
            }
        }

        return null;
    }

    public static bool IsParticiple(string word)
    {
        if (IrregularParticiples.Contains(word))
        {
            return true;
        }

        if (word.Length < 4 || !word.EndsWith("ed", StringComparison.Ordinal) || NotParticiples.Contains(word))
        {
            return false;
        }

        bool endsInEed = word.EndsWith("eed", StringComparison.Ordinal);
        return !endsInEed || word is "agreed" or "freed" or "guaranteed";
    }

    public static bool IsIngForm(string word)
    {
        return word.Length >= 5 && word.EndsWith("ing", StringComparison.Ordinal) && !word.Contains('-') && !NotIngForms.Contains(word);
    }

    private static bool IsContraction(string word)
    {
        foreach (string ending in ContractionEndings)
        {
            if (!word.EndsWith(ending, StringComparison.Ordinal) || word.Length == ending.Length)
            {
                continue;
            }

            string stem = word[..^ending.Length];
            if (ending == "n't" || ContractionBases.Contains(stem))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>The normalized words that the grammar rules read. Opaque words are out, because a rule must not read inside a quote.</summary>
    private static List<string> GrammarWords(Sentence sentence)
    {
        var words = new List<string>(sentence.Words.Count);
        foreach (string word in sentence.Words)
        {
            if (!SentenceText.IsOpaque(word))
            {
                words.Add(SentenceText.Normalize(word));
            }
        }

        return words;
    }

    private static int SkipAdverb(List<string> words, int index)
    {
        if (index < words.Count && (Adverbs.Contains(words[index]) || words[index].EndsWith("ly", StringComparison.Ordinal)))
        {
            return index + 1;
        }

        return index;
    }

    /// <summary>The sentence text is unmasked, so this reads the words again: a semicolon inside an opaque word is not a finding.</summary>
    private static bool IsInsideOpaqueWord(Sentence sentence, char punctuation)
    {
        foreach (string word in sentence.Words)
        {
            if (!SentenceText.IsOpaque(word) && word.Contains(punctuation))
            {
                return false;
            }
        }

        return true;
    }
}
