using Backend.Models;

namespace Backend.Dtos;

public record BikeDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = "";
    public string Brand { get; init; } = "";
    public int IconId { get; init; }
    public int Price { get; init; }
    public DateOnly DateOfPurchase { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public List<BikePartDto> Parts { get; init; } = [];
    public Guid OwnerId { get; init; }
}