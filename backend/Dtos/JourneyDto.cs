namespace Backend.Dtos;

public record JourneyDto
{
    public Guid Id { get; init; }
    public Guid BikeId { get; init; }
    public string? ExternalId { get; init; }
    public string Title { get; init; } = String.Empty;
    public int Distance { get; init; }
    public DateOnly JourneyDate { get; init; }
    public DateTime CreatedAtUtc { get; init; }

    public Guid RepeatJourneyId { get; init; }
    public bool IsConnectedToRepeatJourney { get; set; } = true;
}