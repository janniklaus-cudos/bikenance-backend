using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public class Journey
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
    public int Kilometer { get; set; } = 0;

    [Required]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}