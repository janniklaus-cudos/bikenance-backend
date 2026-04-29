using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public class RepeatJourney
{
    [Key]
    [Required]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public required virtual Bike Bike { get; set; }

    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = String.Empty;

    [Required]
    [Range(0, int.MaxValue)]
    public int Distance { get; set; } = 0;

    [Required]
    [Range(0, 128)]
    public int RepeatDays { get; set; } = 0;

    [Required]
    public RepeatType RepeatType { get; set; } = RepeatType.Weekly;

    [Required]
    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);

    [Required]
    public DateOnly EndDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);

    [Required]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public enum RepeatType
{
    Weekly, Monthly
}