using System.ComponentModel.DataAnnotations;

public class Journey
{
	[Key]
	[Required]
	public Guid Id { get; set; } = Guid.NewGuid();

	[Required]
	public required Bike Bike { get; set; }

	[Required]
	[MaxLength(100)]
	public string Title { get; set; } = String.Empty;

	[Required]
	[Range(0, int.MaxValue)]
	public int Kilometer { get; set; } = 0;

	[Required]
	public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}