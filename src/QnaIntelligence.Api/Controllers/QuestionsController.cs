using Microsoft.AspNetCore.Mvc;
using QnaIntelligence.Application.Interfaces;
using QnaIntelligence.Application.Questions.Dtos;
using QnaIntelligence.Domain.Entities;

namespace QnaIntelligence.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuestionsController(IQuestionRepository questionRepository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<QuestionDto>>> GetAll(CancellationToken cancellationToken)
    {
        var questions = await questionRepository.GetAllAsync(cancellationToken);
        return questions.Select(ToDto).ToList();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<QuestionDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var question = await questionRepository.GetByIdAsync(id, cancellationToken);
        if (question is null)
        {
            return NotFound();
        }

        return ToDto(question);
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
            PriorityScore = 0,
            Status = question.Status.ToString()
        };
    }
}
