using System;
using System.Collections.Generic;
using System.Text;

namespace WhatYouCarry.Tools.SteCheck;

/// <summary>One sentence with its words. A word is a token that counts under rules 8.5 to 8.7.</summary>
public sealed record Sentence(int Line, string Text, IReadOnlyList<string> Words, int WordCount, bool IsProceduralStep);

/// <summary>
/// Splits a prose line into sentences and words. A sentence ends at a period, an exclamation mark, a question mark,
/// or a colon that a space or the line end follows (rule 8.4). A colon inside a word, as in a time or a URL, is not
/// a sentence end. Text in backticks, in double quotes, or in parentheses
/// is one word (rules 8.5 and 8.6), so the splitter masks its inner spaces and punctuation first. A masked word is
/// opaque: the grammar rules do not read inside it.
/// </summary>
public static class SentenceText
{
    private const char MaskedSpace = '\u00A0';
    private const char MaskedPeriod = '\uE000';
    private const char MaskedColon = '\uE001';
    private const char MaskedSemicolon = '\uE002';
    private const char MaskedApostrophe = '\uE003';
    private const char MaskedExclamation = '\uE004';
    private const char MaskedQuestion = '\uE005';

    private static readonly char[] MaskedPunctuation = [MaskedPeriod, MaskedColon, MaskedSemicolon, MaskedApostrophe, MaskedExclamation, MaskedQuestion];
    private static readonly char[] SentenceEnds = ['.', '!', '?', ':'];
    private static readonly char[] TrailingClosers = ['*', '_', '\'', '"', ')', ']'];

    public static List<Sentence> Split(TextLine line)
    {
        var sentences = new List<Sentence>();
        if (line.IsHeading)
        {
            return sentences;
        }

        string masked = Mask(line.Text);
        int start = 0;
        for (int index = 0; index < masked.Length; index++)
        {
            if (!IsSentenceEnd(masked, index))
            {
                continue;
            }

            int end = index + 1;
            while (end < masked.Length && Array.IndexOf(TrailingClosers, masked[end]) >= 0)
            {
                end++;
            }

            AddSentence(sentences, line, masked[start..end]);
            start = end;
        }

        AddSentence(sentences, line, masked[start..]);
        return sentences;
    }

    /// <summary>A token that starts a masked span, or that holds a masked character, is opaque.</summary>
    public static bool IsOpaque(string word)
    {
        return word.StartsWith('`') || word.StartsWith('"') || word.StartsWith('(') || word.Contains(MaskedSpace)
            || word.IndexOfAny(MaskedPunctuation) >= 0;
    }

    /// <summary>Lowercase, with the punctuation and emphasis marks at both ends removed. The grammar rules read this form.</summary>
    public static string Normalize(string word)
    {
        return word.Trim('*', '_', '"', '\'', '(', ')', '[', ']', '{', '}', '<', '>', ',', '.', ';', ':', '!', '?', '`').ToLowerInvariant();
    }

    public static string Unmask(string text)
    {
        return text.Replace(MaskedSpace, ' ').Replace(MaskedPeriod, '.').Replace(MaskedColon, ':').Replace(MaskedSemicolon, ';')
            .Replace(MaskedApostrophe, '\'').Replace(MaskedExclamation, '!').Replace(MaskedQuestion, '?');
    }

    private static void AddSentence(List<Sentence> sentences, TextLine line, string maskedSentence)
    {
        string[] tokens = maskedSentence.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
        var words = new List<string>();
        foreach (string token in tokens)
        {
            if (IsWord(token))
            {
                words.Add(token);
            }
        }

        if (words.Count == 0)
        {
            return;
        }

        string text = Unmask(string.Join(' ', tokens));
        sentences.Add(new Sentence(line.Line, text, words, words.Count, line.IsNumberedItem && line.InProceduralSection));
    }

    /// <summary>An opaque span is one word. Any other token is a word when it holds a letter or a digit.</summary>
    private static bool IsWord(string token)
    {
        if (IsOpaque(token))
        {
            return true;
        }

        foreach (char c in token)
        {
            if (char.IsLetterOrDigit(c))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsSentenceEnd(string text, int index)
    {
        if (Array.IndexOf(SentenceEnds, text[index]) < 0)
        {
            return false;
        }

        int next = index + 1;
        while (next < text.Length && Array.IndexOf(TrailingClosers, text[next]) >= 0)
        {
            next++;
        }

        return next >= text.Length || text[next] == ' ' || text[next] == '\t';
    }

    /// <summary>Masks code spans first, because a code span can hold a quote or a parenthesis. Then quotes, then parentheses.</summary>
    private static string Mask(string text)
    {
        string result = MaskSpans(text, '`', '`');
        result = MaskSpans(result, '"', '"');
        result = MaskSpans(result, '“', '”');
        return MaskSpans(result, '(', ')');
    }

    /// <summary>A span with different open and close characters can nest: "(the (short) name)" is one span (rule 8.5).</summary>
    private static string MaskSpans(string text, char open, char close)
    {
        var builder = new StringBuilder(text.Length);
        int index = 0;
        while (index < text.Length)
        {
            int spanStart = text.IndexOf(open, index);
            int spanEnd = spanStart < 0 ? -1 : FindSpanEnd(text, spanStart, open, close);
            if (spanStart < 0 || spanEnd < 0)
            {
                builder.Append(text, index, text.Length - index);
                break;
            }

            builder.Append(text, index, spanStart - index + 1);
            for (int inner = spanStart + 1; inner < spanEnd; inner++)
            {
                builder.Append(MaskChar(text[inner]));
            }

            builder.Append(close);
            index = spanEnd + 1;
        }

        return builder.ToString();
    }

    /// <summary>The index of the close character that matches the open character at <paramref name="spanStart"/>, or -1 when none does.</summary>
    private static int FindSpanEnd(string text, int spanStart, char open, char close)
    {
        int depth = 0;
        for (int index = spanStart; index < text.Length; index++)
        {
            if (text[index] == open && (open != close || index == spanStart))
            {
                depth++;
                continue;
            }

            if (text[index] == close)
            {
                depth--;
                if (depth == 0)
                {
                    return index;
                }
            }
        }

        return -1;
    }

    private static char MaskChar(char c)
    {
        return c switch
        {
            ' ' => MaskedSpace,
            '.' => MaskedPeriod,
            ':' => MaskedColon,
            ';' => MaskedSemicolon,
            '\'' => MaskedApostrophe,
            '!' => MaskedExclamation,
            '?' => MaskedQuestion,
            _ => c,
        };
    }
}
