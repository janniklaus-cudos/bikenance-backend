using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public class ServiceEvent
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
    [Range(0, 100)]
    public int StateAfterService { get; set; } = 100;

    [Required]
    [Range(0, int.MaxValue)]
    public int Cost { get; set; } = 0;

    [Required]
    public DateTime DateOfService { get; set; } = DateTime.Now;

    [Required]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

}