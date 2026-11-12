using Microsoft.EntityFrameworkCore;
using QnaIntelligence.Domain.Entities;
using QnaIntelligence.Domain.Services;

namespace QnaIntelligence.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        if (await dbContext.Questions.AnyAsync(cancellationToken))
        {
            return;
        }

        var createdUtc = DateTime.UtcNow;
        var questions = new[]
        {
            new Question
            {
                Id = Guid.NewGuid(),
                Title = "Azure App Service deployment fails",
                Content = "An application deployment to Azure App Service fails.",
                Technology = "Azure App Service",
                Tag = "deployment",
                PriorityScore = PriorityCalculator.Calculate(false, 0, 0),
                CreatedUtc = createdUtc
            },
            new Question
            {
                Id = Guid.NewGuid(),
                Title = "ASP.NET Core startup exception",
                Content = "An exception occurs while an ASP.NET Core application is starting.",
                Technology = "ASP.NET Core",
                Tag = "startup",
                PriorityScore = PriorityCalculator.Calculate(false, 0, 0),
                CreatedUtc = createdUtc
            },
            new Question
            {
                Id = Guid.NewGuid(),
                Title = "Windows Update error 0x80070057",
                Content = "Windows Update fails with error code 0x80070057.",
                Technology = "Windows Update",
                Tag = "update",
                PriorityScore = PriorityCalculator.Calculate(false, 0, 0),
                CreatedUtc = createdUtc
            }
        };

        await dbContext.Questions.AddRangeAsync(questions, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
