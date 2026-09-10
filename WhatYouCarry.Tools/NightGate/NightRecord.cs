using System;
using System.Globalization;
using System.Text.Json;
using WhatYouCarry.Tools.BotRunner;

namespace WhatYouCarry.Tools.NightGate;

/// <summary>The record of one night, as the night job publishes it (D-273): the commit it tested, its end time in UTC, and its status.</summary>
public sealed record NightRecord(string Commit, DateTimeOffset EndedAt, string Status);

/// <summary>Reads the text of <c>night.json</c> into a <see cref="NightRecord"/>. Every defect names the field (T-2).</summary>
public static class NightRecordParser
{
    /// <summary>The exact form of the end time, as <see cref="NightRecordCommand.Build"/> writes it.</summary>
    public const string TimeFormat = "yyyy-MM-dd'T'HH:mm:ss'Z'";

    /// <summary>Returns the record, or null with the reason in <paramref name="error"/> when the text is not a record.</summary>
    public static NightRecord? TryParse(string text, out string error)
    {
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(text);
        }
        catch (JsonException exception)
        {
            error = $"the text is not JSON: {exception.Message}";
            return null;
        }

        using (document)
        {
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                error = $"the JSON is a {document.RootElement.ValueKind}, and a record is an object";
                return null;
            }

            string? commit = ReadString(document.RootElement, NightRecordCommand.CommitName, out error);
            if (commit is null)
            {
                return null;
            }

            if (!NightRecordCommand.IsHash(commit))
            {
                error = $"the field '{NightRecordCommand.CommitName}' holds '{commit}', and a commit is 40 lowercase hexadecimal digits";
                return null;
            }

            string? time = ReadString(document.RootElement, NightRecordCommand.EndedAtName, out error);
            if (time is null)
            {
                return null;
            }

            if (!DateTimeOffset.TryParseExact(time, TimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTimeOffset endedAt))
            {
                error = $"the field '{NightRecordCommand.EndedAtName}' holds '{time}', and the form is {TimeFormat}";
                return null;
            }

            string? status = ReadString(document.RootElement, NightRecordCommand.StatusName, out error);
            if (status is null)
            {
                return null;
            }

            if (Array.IndexOf(NightRecordCommand.Statuses, status) < 0)
            {
                error = $"the field '{NightRecordCommand.StatusName}' holds '{status}', and the statuses are {string.Join(", ", NightRecordCommand.Statuses)}";
                return null;
            }

            error = string.Empty;
            return new NightRecord(commit, endedAt, status);
        }
    }

    private static string? ReadString(JsonElement record, string name, out string error)
    {
        if (!record.TryGetProperty(name, out JsonElement element))
        {
            error = $"the field '{name}' is absent";
            return null;
        }

        if (element.ValueKind != JsonValueKind.String)
        {
            error = $"the field '{name}' is a {element.ValueKind}, and the record holds it as a string";
            return null;
        }

        error = string.Empty;
        return element.GetString();
    }
}
