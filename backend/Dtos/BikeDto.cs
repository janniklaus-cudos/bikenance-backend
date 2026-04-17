namespace Backend.Dtos;

public record BikeDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = "";
    public string Brand { get; init; } = "";
    public int IconId { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public List<BikePartDto> Parts { get; init; } = [];
}