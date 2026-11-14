using System.Net;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QnaIntelligence.Application.Features.Ingestion;
using QnaIntelligence.Application.Interfaces;
using QnaIntelligence.Application.Questions.Dtos;
using QnaIntelligence.Domain.Entities;
using QnaIntelligence.Domain.Enums;
using QnaIntelligence.Infrastructure.Persistence;

namespace QnaIntelligence.Infrastructure.Services.Ingestion;

public sealed partial class MicrosoftQnaIngestionService(
    HttpClient httpClient,
    ApplicationDbContext dbContext,
    IAiAnalysisService aiAnalysisService,
    ILogger<MicrosoftQnaIngestionService> logger)
    : IQnaIngestionService
{
    private const string AzureTagPage = "https://learn.microsoft.com/en-us/answers/tags/133/azure/";

    public async Task<List<DiscoveredQuestionDto>> GetLatestQuestionsAsync(
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(AzureTagPage, cancellationToken);
        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync(cancellationToken);
        var questions = new List<DiscoveredQuestionDto>();

        foreach (Match match in QuestionLinkRegex().Matches(html))
        {
            var title = WebUtility.HtmlDecode(match.Groups["title"].Value).Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                continue;
            }

            questions.Add(new DiscoveredQuestionDto
            {
                Title = title,
                Url = new Uri(new Uri(AzureTagPage), match.Groups["url"].Value).ToString(),
                Technology = "Azure"
            });
        }

        return questions;
    }

    public async Task<ImportResultDto> ImportLatestQuestionsAsync(
        CancellationToken cancellationToken)
    {
        var discoveredQuestions = await GetLatestQuestionsAsync(cancellationToken);
        var candidates = new List<(string SourceId, DiscoveredQuestionDto Question)>();
        var discoveredSourceIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var discoveredUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var duplicates = 0;

        foreach (var question in discoveredQuestions)
        {
            var sourceId = GetSourceId(question.Url);
            if (sourceId is null
                || !discoveredSourceIds.Add(sourceId)
                || !discoveredUrls.Add(question.Url))
            {
                duplicates++;
                continue;
            }

            candidates.Add((sourceId, question));
        }

        if (candidates.Count == 0)
        {
            return new ImportResultDto { Duplicates = duplicates };
        }

        var sourceIds = candidates.Select(candidate => candidate.SourceId).ToList();
        var urls = candidates.Select(candidate => candidate.Question.Url).ToList();
        var existingSources = await dbContext.QuestionSources
            .Where(source => sourceIds.Contains(source.SourceId) || urls.Contains(source.Url))
            .Select(source => new { source.SourceId, source.Url })
            .ToListAsync(cancellationToken);
        var existingSourceIds = existingSources
            .Select(source => source.SourceId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var existingUrls = existingSources
            .Select(source => source.Url)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var imported = 0;
        var analyzed = 0;
        var failed = 0;
        var importedUtc = DateTime.UtcNow;

        foreach (var (sourceId, discoveredQuestion) in candidates)
        {
            if (existingSourceIds.Contains(sourceId) || existingUrls.Contains(discoveredQuestion.Url))
            {
                duplicates++;
                continue;
            }

            var question = new Question
            {
                Id = Guid.NewGuid(),
                Title = discoveredQuestion.Title,
                Url = discoveredQuestion.Url,
                Technology = discoveredQuestion.Technology,
                CreatedUtc = discoveredQuestion.DiscoveredUtc ?? importedUtc
            };

            try
            {
                var questionSource = new QuestionSource
                {
                    Id = Guid.NewGuid(),
                    SourceId = sourceId,
                    Url = discoveredQuestion.Url,
                    ImportedUtc = importedUtc,
                    Question = question
                };

                dbContext.Questions.Add(question);
                dbContext.QuestionSources.Add(questionSource);
                await dbContext.SaveChangesAsync(cancellationToken);
                imported++;
                existingSourceIds.Add(sourceId);
                existingUrls.Add(discoveredQuestion.Url);
            }
            catch (DbUpdateException)
            {
                dbContext.ChangeTracker.Clear();

                var wasImportedConcurrently = await dbContext.QuestionSources
                    .AnyAsync(source => source.SourceId == sourceId
                        || source.Url == discoveredQuestion.Url, cancellationToken);

                if (!wasImportedConcurrently)
                {
                    throw;
                }

                duplicates++;
                continue;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                dbContext.ChangeTracker.Clear();
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Failed to import Q&A question {SourceId} at {QuestionUrl}; continuing with remaining questions.",
                    sourceId,
                    discoveredQuestion.Url);
                dbContext.ChangeTracker.Clear();
                failed++;
                continue;
            }

            await using var analysisTransaction = await dbContext.Database
                .BeginTransactionAsync(cancellationToken);
            try
            {
                var analysisDto = await aiAnalysisService.AnalyzeAsync(
                    question.Id,
                    question.Title,
                    question.Content,
                    cancellationToken);

                var analysis = ToAnalysis(question, analysisDto);
                dbContext.Analyses.Add(analysis);
                await dbContext.SaveChangesAsync(cancellationToken);

                question.Analysis = analysis;
                question.Status = QuestionStatus.Analyzed;
                await dbContext.SaveChangesAsync(cancellationToken);
                await analysisTransaction.CommitAsync(cancellationToken);
                analyzed++;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                await analysisTransaction.RollbackAsync(CancellationToken.None);
                dbContext.ChangeTracker.Clear();
                throw;
            }
            catch (Exception exception)
            {
                await analysisTransaction.RollbackAsync(CancellationToken.None);
                logger.LogError(
                    exception,
                    "Question {QuestionId} from source {SourceId} was imported but analysis failed; keeping status New and continuing.",
                    question.Id,
                    sourceId);

                dbContext.ChangeTracker.Clear();
                failed++;
            }
        }

        return new ImportResultDto
        {
            Imported = imported,
            Analyzed = analyzed,
            Duplicates = duplicates,
            Failed = failed
        };
    }

    private static string DescribeEntries(DbUpdateConcurrencyException exception)
    {
        return string.Join(
            "; ",
            exception.Entries.Select(entry =>
            {
                var key = entry.Metadata.FindPrimaryKey();
                var keyValues = key is null
                    ? "<no primary key>"
                    : string.Join(
                        ", ",
                        key.Properties.Select(property =>
                            $"{property.Name}={Convert.ToString(entry.Property(property.Name).CurrentValue, CultureInfo.InvariantCulture) ?? "<null>"}"));

                return $"Entity={entry.Metadata.ClrType.Name}, State={entry.State}, Key=[{keyValues}]";
            }));
    }

    private static string? GetSourceId(string questionUrl)
    {
        if (!Uri.TryCreate(questionUrl, UriKind.Absolute, out var uri))
        {
            return null;
        }

        var match = SourceIdRegex.Match(uri.AbsolutePath);
        return match.Success ? match.Groups["id"].Value : null;
    }

    private static QuestionAnalysis ToAnalysis(Question question, QuestionAnalysisDto analysisDto)
    {
        return new QuestionAnalysis
        {
            Id = Guid.NewGuid(),
            QuestionId = question.Id,
            Question = question,
            Summary = analysisDto.Summary,
            Technologies = analysisDto.Technologies,
            RootCauses = analysisDto.RootCauses,
            MissingInformation = analysisDto.MissingInformation,
            TroubleshootingSteps = analysisDto.TroubleshootingSteps,
            ProposedAnswer = analysisDto.ProposedAnswer,
            ConfidenceScore = analysisDto.ConfidenceScore
        };
    }

    [GeneratedRegex("<h2\\b[^>]*>\\s*<a\\s+href=\"(?<url>/en-us/answers/questions/[^\"]+)\"[^>]*>(?<title>.*?)</a>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex QuestionLinkRegex();

    private static readonly Regex SourceIdRegex = new(
        @"/answers/questions/(?<id>\d+)(?:/|$)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
}