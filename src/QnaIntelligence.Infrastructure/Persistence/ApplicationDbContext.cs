using Microsoft.EntityFrameworkCore;
using QnaIntelligence.Domain.Entities;

namespace QnaIntelligence.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Question> Questions => Set<Question>();

    public DbSet<QuestionAnalysis> Analyses => Set<QuestionAnalysis>();
}
