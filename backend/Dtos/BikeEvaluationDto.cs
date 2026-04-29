namespace Backend.Dtos;

public record BikeEvaluationDto
{
    public Guid BikeId { get; init; }
    public string BikeName { get; init; } = string.Empty;
    public int TotalCost { get; init; }
    public int TotalServiceEvents { get; init; }
    public double CostPerDistance { get; init; }
    public List<BikePartSummary> BikePartSummaries { get; init; } = [];

}

public record BikePartSummary
{
    public string Name { get; init; } = string.Empty;
    public int TotalServices { get; init; }
    public int TotalCost { get; init; }
    public int? AverageDaysServiceInterval { get; init; }
}