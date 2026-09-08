using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WhatYouCarry.Tools.ReviewGate;

/// <summary>The body of a Checks API request. The workflow posts it as it is (D-181).</summary>
public sealed class CheckRunPayload
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("head_sha")]
    public required string HeadSha { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("conclusion")]
    public required string Conclusion { get; init; }

    [JsonPropertyName("output")]
    public required CheckRunOutput Output { get; init; }
}

public sealed class CheckRunOutput
{
    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("summary")]
    public required string Summary { get; init; }
}

/// <summary>
/// <c>review-gate --input request.json --output check-run.json</c>.
/// Reads the request, gathers the facts from git, applies the rules, and writes the check-run body.
/// Exit 0 means the body was written, with any conclusion. A nonzero exit means the tool itself failed.
/// </summary>
public static class ReviewGateCommand
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    public static int Run(string[] args)
    {
        string? inputPath = null;
        string? outputPath = null;
        for (int i = 0; i + 1 < args.Length; i += 2)
        {
            switch (args[i])
            {
                case "--input":
                    inputPath = args[i + 1];
                    break;
                case "--output":
                    outputPath = args[i + 1];
                    break;
                default:
                    Console.Error.WriteLine($"Unknown option '{args[i]}'. Options: --input <path> --output <path>.");
                    return 2;
            }
        }

        if (inputPath is null || outputPath is null)
        {
            Console.Error.WriteLine("Both options are required: --input <path> --output <path>.");
            return 2;
        }

        CheckRunPayload payload = Evaluate(inputPath);
        File.WriteAllText(outputPath, JsonSerializer.Serialize(payload, JsonOptions));
        Console.WriteLine($"{ReviewGateRules.CheckName}: {payload.Conclusion}. {payload.Output.Title}.");
        return 0;
    }

    public static CheckRunPayload Evaluate(string inputPath)
    {
        string json = File.ReadAllText(inputPath);
        ReviewGateRequest request = JsonSerializer.Deserialize<ReviewGateRequest>(json, JsonOptions)
            ?? throw new InvalidOperationException($"The request file '{inputPath}' holds JSON null.");
        ReviewGateFacts facts = ReviewGateFacts.Gather(request);
        ReviewGateResult result = ReviewGateRules.Evaluate(facts);
        return new CheckRunPayload
        {
            Name = ReviewGateRules.CheckName,
            HeadSha = request.HeadSha,
            Status = "completed",
            Conclusion = result.Conclusion,
            Output = new CheckRunOutput { Title = result.Title, Summary = result.Summary },
        };
    }
}
