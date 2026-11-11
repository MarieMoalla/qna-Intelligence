using QnaIntelligence.Domain.Entities;

namespace QnaIntelligence.Application.Interfaces;

public interface IQuestionRepository
{
    Task<List<Question>> GetAllAsync(CancellationToken cancellationToken);

    Task<Question?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(Question question, CancellationToken cancellationToken);

    Task UpdateAsync(Question question, CancellationToken cancellationToken);

    Task SaveAnalysisAsync(
        Question question,
        QuestionAnalysis analysis,
        CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
