using Microsoft.AspNetCore.Mvc;
using QnaIntelligence.Application.Interfaces;
using QnaIntelligence.Application.Questions.Dtos;
using QnaIntelligence.Domain.Entities;

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
            question.Title,
            question.Content,
            cancellationToken);

        var analysis = new QuestionAnalysis
        {
            Id = Guid.NewGuid(),
            QuestionId = question.Id,
            Summary = analysisDto.Summary,
            RootCauses = analysisDto.RootCauses,
            MissingInformation = analysisDto.MissingInformation,
            TroubleshootingSteps = analysisDto.TroubleshootingSteps,
            ProposedAnswer = analysisDto.ProposedAnswer,
            ConfidenceScore = analysisDto.ConfidenceScore,
            Question = question
        };

        await questionRepository.SaveAnalysisAsync(question, analysis, cancellationToken);

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
            PriorityScore = question.PriorityScore,
            Status = question.Status.ToString()
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
            PriorityScore = question.PriorityScore,
            Status = question.Status.ToString(),
            CreatedUtc = question.CreatedUtc,
            UpdatedUtc = null,
            Analysis = question.Analysis is null
                ? null
                : new QuestionAnalysisDto
                {
                    Summary = question.Analysis.Summary,
                    RootCauses = question.Analysis.RootCauses,
                    MissingInformation = question.Analysis.MissingInformation,
                    TroubleshootingSteps = question.Analysis.TroubleshootingSteps,
                    ProposedAnswer = question.Analysis.ProposedAnswer,
                    ConfidenceScore = question.Analysis.ConfidenceScore
                }
        };
    }
}
