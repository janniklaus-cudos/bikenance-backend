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
    public TimeSpan TimeInterval { get; set; } = TimeSpan.FromDays(30);

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