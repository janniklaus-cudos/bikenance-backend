using System.ComponentModel.DataAnnotations;

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
	public Bike Bike { get; set; } = null!;
}