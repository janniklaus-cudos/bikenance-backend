using Backend.Models;

namespace Backend.Dtos;

public record MaintenanceTaskDto
{
    public Guid Id { get; init; }
    public Guid BikePartId { get; init; }
    public string Description { get; init; } = string.Empty;
    public TimeSpan TimeInterval { get; init; }
    public int Cost { get; init; }
    public ImportanceLevel Importance { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}