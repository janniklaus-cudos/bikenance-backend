
using Backend.Models;

namespace Backend.Dtos;

public record BikePartDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public BikePartPosition Position { get; init; }
    public Guid BikeId { get; init; }
}