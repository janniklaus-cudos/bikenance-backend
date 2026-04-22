using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

public class BikePart
{
    [Key]
    [Required]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = "";

    public BikePartPosition Position { get; set; } = BikePartPosition.NONE;

    [Required]
    public virtual Bike Bike { get; set; } = null!;

    [Required]
    public virtual List<ServiceEvent> ServiceEvents { get; set; } = [];

    [Required]
    public virtual MaintenanceTask? MaintenanceTask { get; set; } = null;
}