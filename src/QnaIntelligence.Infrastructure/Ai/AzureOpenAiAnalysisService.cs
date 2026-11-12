using System.Text.Json;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using QnaIntelligence.Application.Interfaces;
using QnaIntelligence.Application.Questions.Dtos;
using QnaIntelligence.Infrastructure.Options;

namespace QnaIntelligence.Infrastructure.Ai;

public sealed class AzureOpenAiAnalysisService : IAiAnalysisService
{
    private readonly ChatClient _chatClient;

    public AzureOpenAiAnalysisService(IOptions<AzureOpenAiOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.Endpoint))
        {
            throw new InvalidOperationException("Azure OpenAI endpoint is not configured.");
        }

        if (string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            throw new InvalidOperationException("Azure OpenAI API key is not configured.");
        }

        if (string.IsNullOrWhiteSpace(settings.DeploymentName))
        {
            throw new InvalidOperationException("Azure OpenAI deployment name is not configured.");
        }

        var client = new AzureOpenAIClient(
            new Uri(settings.Endpoint, UriKind.Absolute),
            new AzureKeyCredential(settings.ApiKey));
        _chatClient = client.GetChatClient(settings.DeploymentName);
    }

    public async Task<QuestionAnalysisDto> AnalyzeAsync(
        string title,
        string content,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(content);
        cancellationToken.ThrowIfCancellationRequested();

        var prompt = $$"""
            You are a Microsoft MVP helping answer Microsoft Q&A questions.

            Analyze the question and return JSON only.

            {
            "summary":"",
            "rootCauses":"",
            "missingInformation":"",
            "troubleshootingSteps":"",
            "proposedAnswer":"",
            "confidenceScore":0
            }

            Question Title:
            {{title}}

            Question Content:
            {{content}}
            """;

        var completionResult = await _chatClient.CompleteChatAsync(
            [new UserChatMessage(prompt)],
            new ChatCompletionOptions
            {
                ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
            },
            cancellationToken);

        var responseJson = string.Concat(completionResult.Value.Content.Select(part => part.Text));
        return ParseAnalysis(responseJson);
    }

    public async Task<string> CompletePromptAsync(string prompt, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prompt);
        cancellationToken.ThrowIfCancellationRequested();

        var completionResult = await _chatClient.CompleteChatAsync(
            [new UserChatMessage(prompt)],
            cancellationToken: cancellationToken);

        return string.Concat(completionResult.Value.Content.Select(part => part.Text));
    }

    private static QuestionAnalysisDto ParseAnalysis(string responseJson)
    {
        try
        {
            using var document = JsonDocument.Parse(responseJson);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new JsonException("The AI response must be a JSON object.");
            }

            var root = document.RootElement;
            return new QuestionAnalysisDto
            {
                Summary = ReadString(root, "summary"),
                RootCauses = ReadString(root, "rootCauses"),
                MissingInformation = ReadString(root, "missingInformation"),
                TroubleshootingSteps = ReadString(root, "troubleshootingSteps"),
                ProposedAnswer = ReadString(root, "proposedAnswer"),
                ConfidenceScore = ReadConfidenceScore(root)
            };
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException("Azure OpenAI returned an invalid analysis JSON response.", exception);
        }
    }

    private static string ReadString(JsonElement root, string propertyName)
    {
        if (!TryGetProperty(root, propertyName, out var property) || property.ValueKind == JsonValueKind.Null)
        {
            return string.Empty;
        }

        if (property.ValueKind == JsonValueKind.String)
        {
            return property.GetString() ?? string.Empty;
        }

        if (property.ValueKind == JsonValueKind.Array)
        {
            var values = new List<string>();
            foreach (var item in property.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.String)
                {
                    throw new JsonException($"The '{propertyName}' array must contain only strings.");
                }

                values.Add(item.GetString() ?? string.Empty);
            }

            return string.Join(Environment.NewLine, values);
        }

        throw new JsonException($"The '{propertyName}' field must be a string or an array of strings.");
    }

    private static double ReadConfidenceScore(JsonElement root)
    {
        if (!TryGetProperty(root, "confidenceScore", out var property))
        {
            return 0;
        }

        if (property.ValueKind != JsonValueKind.Number || !property.TryGetDouble(out var score))
        {
            throw new JsonException("The 'confidenceScore' field must be a number.");
        }

        return Math.Clamp(score, 0, 1);
    }

    private static bool TryGetProperty(JsonElement root, string propertyName, out JsonElement property)
    {
        foreach (var candidate in root.EnumerateObject())
        {
            if (string.Equals(candidate.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                property = candidate.Value;
                return true;
            }
        }

        property = default;
        return false;
    }
}