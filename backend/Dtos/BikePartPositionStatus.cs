using Backend.Models;

namespace Backend.Dtos;

public record BikePartPositionStatus
{
    public Guid BikePartId { get; init; }
    public BikePartPosition Position { get; init; }
    public Status Status { get; init; }
}

public enum Status
{
    NONE,
    OK,
    WARNING,
    CRITICAL
}