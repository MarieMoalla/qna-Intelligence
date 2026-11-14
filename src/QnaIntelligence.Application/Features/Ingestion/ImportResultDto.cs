namespace QnaIntelligence.Application.Features.Ingestion;

public sealed class ImportResultDto
{
    public int Imported { get; set; }

    public int Duplicates { get; set; }

    public int Analyzed { get; set; }

    public int Failed { get; set; }
}
