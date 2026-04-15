namespace Backend.Dtos;

public record ServiceEventDto
{
    public Guid Id { get; init; }
    public Guid BikePartId { get; init; }
    public string Description { get; init; } = string.Empty;
    public int StateAfterService { get; init; }
    public int Cost { get; init; } = 0;
    public DateTime CreatedAtUtc { get; init; }
}
