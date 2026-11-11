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
        dbContext.Questions.Update(question);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveAnalysisAsync(
        Question question,
        QuestionAnalysis analysis,
        CancellationToken cancellationToken)
    {
        if (question.Analysis is null)
        {
            analysis.QuestionId = question.Id;
            analysis.Question = question;
            question.Analysis = analysis;
            await dbContext.Analyses.AddAsync(analysis, cancellationToken);
        }
        else
        {
            question.Analysis.Summary = analysis.Summary;
            question.Analysis.RootCauses = analysis.RootCauses;
            question.Analysis.MissingInformation = analysis.MissingInformation;
            question.Analysis.TroubleshootingSteps = analysis.TroubleshootingSteps;
            question.Analysis.ProposedAnswer = analysis.ProposedAnswer;
            question.Analysis.ConfidenceScore = analysis.ConfidenceScore;
        }

        question.Status = QuestionStatus.Analyzed;
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
