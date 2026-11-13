using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using QnaIntelligence.Application.Features.Ingestion;

namespace QnaIntelligence.Api.Controllers;

[ApiController]
[Route("api/ingestion")]
public sealed class IngestionController(
    IQnaIngestionService ingestionService,
    IHostEnvironment hostEnvironment) : ControllerBase
{
    [HttpPost("test")]
    public async Task<ActionResult<List<DiscoveredQuestionDto>>> Test(
        CancellationToken cancellationToken)
    {
        var questions = await ingestionService.GetLatestQuestionsAsync(cancellationToken);
        return Ok(questions);
    }

    [HttpPost("import")]
    public async Task<ActionResult<QuestionImportResultDto>> Import(
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await ingestionService.ImportLatestQuestionsAsync(cancellationToken);
            return Ok(result);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "A concurrency conflict occurred while importing questions.",
                Detail = hostEnvironment.IsDevelopment()
                    ? $"{exception.Message} Affected entries: {DescribeEntries(exception)}"
                    : "The import conflicted with another database operation. Retry the request."
            };

            return Conflict(problem);
        }
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
}