namespace Backend.Dtos;

public record BikePartEvaluationDto
{
    public int DaysSinceLastService { get; init; }
    public int DistanceSinceLastService { get; init; }
    public int? AverageCostPerService { get; init; }
    public int? TotalCost { get; init; } = 0;
    public DateOnly? NextServiceDueDate { get; init; }
}