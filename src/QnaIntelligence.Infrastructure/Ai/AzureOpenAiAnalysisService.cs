using System.Text.Json;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using QnaIntelligence.Application.Interfaces;
using QnaIntelligence.Application.Questions.Dtos;
using QnaIntelligence.Domain.Entities;
using QnaIntelligence.Infrastructure.Options;
using QnaIntelligence.Infrastructure.Persistence;

namespace QnaIntelligence.Infrastructure.Ai;

public sealed class AzureOpenAiAnalysisService : IAiAnalysisService
{
    private readonly ChatClient _chatClient;
    private readonly ApplicationDbContext _dbContext;
    private readonly string _model;

    public AzureOpenAiAnalysisService(
        IOptions<AzureOpenAiOptions> options,
        ApplicationDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(dbContext);

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
        _dbContext = dbContext;
        _model = settings.DeploymentName;
    }

    public async Task<QuestionAnalysisDto> AnalyzeAsync(
        Guid questionId,
        string title,
        string content,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(content);
        cancellationToken.ThrowIfCancellationRequested();

        var prompt = $$"""
            You are an experienced Microsoft MVP answering Microsoft Q&A.

            Your task:

            1. Identify technologies involved.
            2. Summarize the problem.
            3. List likely root causes.
            4. Identify missing information.
            5. Provide troubleshooting steps.
            6. Create a detailed answer suitable for Microsoft Q&A.

            Return JSON only.

            {
            "summary":"",
            "technologies":"",
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
        await _dbContext.AiRequestLogs.AddAsync(
            new AiRequestLog
            {
                Id = Guid.NewGuid(),
                QuestionId = questionId,
                CreatedUtc = DateTime.UtcNow,
                Model = _model,
                Prompt = prompt,
                Response = responseJson
            },
            cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

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
                Technologies = ReadString(root, "technologies"),
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