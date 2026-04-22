namespace Backend.Dtos;

public record JourneyDto
{
    public Guid Id { get; init; }
    public Guid BikeId { get; init; }
    public string Title { get; init; } = String.Empty;
    public int Kilometer { get; init; }
    public DateTime CreatedAtUtc { get; init; }

}