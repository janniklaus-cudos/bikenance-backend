using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public class Bike
{
    [Key]
    [Required]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = "New Bike";

    [Required]
    [MaxLength(100)]
    public string Brand { get; set; } = String.Empty;

    [Required]
    public int IconId { get; set; } = 0;

    [Required]
    public int Price { get; set; } = 0;

    [Required]
    public DateOnly DateOfPurchase { get; set; } = DateOnly.FromDateTime(DateTime.Now);

    [Required]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [Required]
    public virtual List<BikePart> Parts { get; set; } = [];

    [Required]
    public virtual User Owner { get; set; } = null!;
}