namespace Backend.Dtos;

public record BikePartEvaluationDto
{
    public int DaysSinceLastService { get; init; }
    public int DistanceSinceLastService { get; init; }
    public int CostTotal { get; init; }
    public DateTime? NextServiceDueDate { get; init; }

}