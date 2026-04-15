using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public class BikePart
{
    [Key]
    [Required]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = "";

    public BikePartPosition Position { get; set; }

    [Required]
    public virtual Bike Bike { get; set; } = null!;
}