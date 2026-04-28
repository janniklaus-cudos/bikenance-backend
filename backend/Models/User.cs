using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public class User
{
    [Key]
    [Required]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Email { get; set; } = String.Empty;

    [Required]
    public string Name { get; set; } = String.Empty;

    [Required]
    public string ExternalId { get; set; } = String.Empty;

    public string JwtToken { get; set; } = String.Empty;

    public DateTime JwtTokenUpdatedAt { get; set; }

    public string StravaToken { get; set; } = String.Empty;

    public DateTime StravaTokenUpdatedAt { get; set; }

    [Required]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}