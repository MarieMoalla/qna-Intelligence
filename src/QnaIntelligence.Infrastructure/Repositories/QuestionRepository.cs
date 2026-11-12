using Microsoft.EntityFrameworkCore;
using QnaIntelligence.Application.Interfaces;
using QnaIntelligence.Domain.Entities;
using QnaIntelligence.Domain.Enums;
using QnaIntelligence.Infrastructure.Persistence;

namespace QnaIntelligence.Infrastructure.Repositories;

public class QuestionRepository(ApplicationDbContext dbContext) : IQuestionRepository
{
    public Task<List<Question>> GetAllAsync(CancellationToken cancellationToken)
    {
        return dbContext.Questions.ToListAsync(cancellationToken);
    }

    public Task<Question?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Questions
            .Include(question => question.Analysis)
            .FirstOrDefaultAsync(
            question => question.Id == id,
            cancellationToken);
    }

    public async Task AddAsync(Question question, CancellationToken cancellationToken)
    {
        await dbContext.Questions.AddAsync(question, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Question question, CancellationToken cancellationToken)
    {
        var analysis = question.Analysis;
        var isNewAnalysis = analysis is not null
            && dbContext.Entry(analysis).State == EntityState.Detached;

        dbContext.Questions.Update(question);
        if (isNewAnalysis)
        {
            dbContext.Entry(analysis!).State = EntityState.Added;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var question = await dbContext.Questions.FirstOrDefaultAsync(
            question => question.Id == id,
            cancellationToken);

        if (question is null)
        {
            return;
        }

        dbContext.Questions.Remove(question);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
