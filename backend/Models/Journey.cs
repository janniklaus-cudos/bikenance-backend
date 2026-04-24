using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public class Journey
{
    [Key]
    [Required]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string? ExternalId { get; set; }

    [Required]
    public required virtual Bike Bike { get; set; }

    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = String.Empty;

    [Required]
    [Range(0, int.MaxValue)]
    public int Distance { get; set; } = 0;

    [Required]
    public DateOnly JourneyDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);

    [Required]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}