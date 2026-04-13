using Backend.Models;

namespace Backend.Dtos;

public record MaintenanceTaskDto(
    Guid Id,
    Guid BikePartId,
    string Description,
    TimeSpan TimeInterval,
    int Cost,
    ImportanceLevel Importance,
    bool IsActive,
    DateTime CreatedAtUtc
);
