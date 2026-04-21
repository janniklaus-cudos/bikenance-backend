using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public class MaintenanceTask
{
    [Key]
    [Required]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public required virtual BikePart BikePart { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = String.Empty;

    [Required]
    public bool IsDaysIntervalActive { get; set; } = true;

    [Required]
    [Range(0, int.MaxValue)]
    public int DaysInterval { get; set; } = 90;

    [Required]
    public bool IsDistanceIntervalActive { get; set; } = true;

    [Required]
    [Range(0, int.MaxValue)]
    public int DistanceInterval { get; set; } = 1000;

    [Required]
    [Range(0, int.MaxValue)]
    public int Cost { get; set; } = 0;

    [Required]
    public ImportanceLevel Importance { get; set; } = ImportanceLevel.MEDIUM;

    [Required]
    public bool IsActive { get; set; } = true;

    [Required]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}