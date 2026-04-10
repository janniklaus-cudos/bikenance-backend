using System.ComponentModel.DataAnnotations;

public class Bike
{
	[Key]
	public Guid Id { get; set; }

	[Required]
	[MaxLength(100)]
	public string Name { get; set; } = "New Bike";

	[Required]
	[MaxLength(100)]
	public string Brand { get; set; } = String.Empty;

	[Required]
	public int IconId { get; set; } = 0;

	[Required]
	public DateTime CreatedAtUtc { get; } = DateTime.UtcNow;

	[Required]
	public List<BikePart> Parts { get; set; } = [];
}