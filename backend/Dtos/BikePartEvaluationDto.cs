namespace Backend.Dtos;

public record BikePartEvaluationDto
{
    public int DaysSinceLastService { get; init; }
    public int DistanceSinceLastService { get; init; }
    public int? AverageCostPerService { get; init; }
    public DateOnly? NextServiceDueDate { get; init; }

}