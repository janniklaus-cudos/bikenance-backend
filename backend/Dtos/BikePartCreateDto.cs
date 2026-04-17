
using Backend.Models;

namespace Backend.Dtos;

public record BikePartCreateDto
{
    public string Name { get; init; } = string.Empty;
    public BikePartPosition Position { get; init; } = BikePartPosition.NONE;
}