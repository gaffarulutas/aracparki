namespace AracParki.Application.Corporate.Dtos;

public sealed class CorporateModerationCountsDto
{
    public int Pending { get; init; }
    public int Approved { get; init; }
    public int Rejected { get; init; }
}
