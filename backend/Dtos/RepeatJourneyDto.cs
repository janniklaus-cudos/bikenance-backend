using Backend.Models;

namespace Backend.Dtos;

public record RepeatJourneyDto
{
    public Guid Id { get; init; }
    public Guid BikeId { get; init; }
    public string Title { get; init; } = String.Empty;
    public int Distance { get; init; }
    public string[] RepeatDays { get; init; } = [];
    public RepeatType RepeatType { get; set; } = RepeatType.Weekly;
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public DateTime CreatedAtUtc { get; init; }

}