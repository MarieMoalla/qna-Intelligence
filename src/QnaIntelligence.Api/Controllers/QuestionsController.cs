using Microsoft.AspNetCore.Mvc;
using QnaIntelligence.Application.Interfaces;
using QnaIntelligence.Application.Questions.Dtos;
using QnaIntelligence.Domain.Entities;
using QnaIntelligence.Domain.Enums;
using QnaIntelligence.Domain.Services;

namespace QnaIntelligence.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuestionsController(
    IQuestionRepository questionRepository,
    IAiAnalysisService aiAnalysisService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<QuestionDto>>> GetAll(CancellationToken cancellationToken)
    {
        var questions = await questionRepository.GetAllAsync(cancellationToken);
        return questions.Select(ToDto).ToList();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetQuestionDetailsDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var question = await questionRepository.GetByIdAsync(id, cancellationToken);
        if (question is null)
        {
            return NotFound();
        }

        return ToDetailsDto(question);
    }

    [HttpPost("{id:guid}/analyze")]
    public async Task<ActionResult<QuestionAnalysisDto>> Analyze(
        Guid id,
        CancellationToken cancellationToken)
    {
        var question = await questionRepository.GetByIdAsync(id, cancellationToken);
        if (question is null)
        {
            return NotFound();
        }

        var analysisDto = await aiAnalysisService.AnalyzeAsync(
            question.Id,
            question.Title,
            question.Content,
            cancellationToken);

        var analysis = question.Analysis ?? new QuestionAnalysis
        {
            Id = Guid.NewGuid(),
            QuestionId = question.Id,
            Question = question
        };
        analysis.Summary = analysisDto.Summary;
        analysis.Technologies = analysisDto.Technologies;
        analysis.RootCauses = analysisDto.RootCauses;
        analysis.MissingInformation = analysisDto.MissingInformation;
        analysis.TroubleshootingSteps = analysisDto.TroubleshootingSteps;
        analysis.ProposedAnswer = analysisDto.ProposedAnswer;
        analysis.ConfidenceScore = analysisDto.ConfidenceScore;

        question.Analysis = analysis;
        question.Status = QuestionStatus.Analyzed;
        question.PriorityScore = CalculatePriorityScore(question);

        await questionRepository.UpdateAsync(question, cancellationToken);

        return Ok(analysisDto);
    }

    [HttpPost]
    public async Task<ActionResult<QuestionDto>> Create(
        CreateQuestionDto createQuestionDto,
        CancellationToken cancellationToken)
    {
        var question = new Question
        {
            Id = Guid.NewGuid(),
            Title = createQuestionDto.Title,
            Content = createQuestionDto.Content,
            Url = createQuestionDto.Url,
            Technology = createQuestionDto.Technology,
            Tag = createQuestionDto.Tag,
            CreatedUtc = DateTime.UtcNow
        };
        question.PriorityScore = CalculatePriorityScore(question);

        await questionRepository.AddAsync(question, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = question.Id },
            ToDto(question));
    }

    private static QuestionDto ToDto(Question question)
    {
        return new QuestionDto
        {
            Id = question.Id,
            Title = question.Title,
            Technology = question.Technology,
            Tag = question.Tag,
            PriorityScore = CalculatePriorityScore(question),
            Status = question.Status.ToString(),
            Source = question.QuestionSources
                .OrderByDescending(source => source.ImportedUtc)
                .Select(_ => "Microsoft Learn Q&A")
                .FirstOrDefault(),
            ImportedUtc = question.QuestionSources
                .OrderByDescending(source => source.ImportedUtc)
                .Select(source => (DateTime?)source.ImportedUtc)
                .FirstOrDefault()
        };
    }

    private static GetQuestionDetailsDto ToDetailsDto(Question question)
    {
        return new GetQuestionDetailsDto
        {
            Id = question.Id,
            Title = question.Title,
            Url = question.Url,
            Content = question.Content,
            Technology = question.Technology,
            Tag = question.Tag,
            HasAnswers = question.HasAnswers,
            PriorityScore = CalculatePriorityScore(question),
            Status = question.Status.ToString(),
            CreatedUtc = question.CreatedUtc,
            UpdatedUtc = null,
            Analysis = question.Analysis is null
                ? null
                : new QuestionAnalysisDto
                {
                    Summary = question.Analysis.Summary,
                    Technologies = question.Analysis.Technologies,
                    RootCauses = question.Analysis.RootCauses,
                    MissingInformation = question.Analysis.MissingInformation,
                    TroubleshootingSteps = question.Analysis.TroubleshootingSteps,
                    ProposedAnswer = question.Analysis.ProposedAnswer,
                    ConfidenceScore = question.Analysis.ConfidenceScore
                }
        };
    }

    private static int CalculatePriorityScore(Question question)
    {
        var ageHours = Math.Max(0, (int)(DateTime.UtcNow - question.CreatedUtc).TotalHours);
        var confidence = question.Analysis?.ConfidenceScore ?? 0;

        return PriorityCalculator.Calculate(question.HasAnswers, confidence, ageHours);
    }
}
