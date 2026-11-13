namespace QnaIntelligence.Application.Features.Ingestion;

public sealed class QuestionImportResultDto
{
    public int Imported { get; set; }

    public int Duplicates { get; set; }

    public int Failed { get; set; }
}
