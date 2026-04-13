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
	public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

	[Required]
	public List<BikePart> Parts { get; set; } = [];
}