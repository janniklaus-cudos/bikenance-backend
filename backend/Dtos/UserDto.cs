namespace Backend.Dtos;

public record UserDto
{
    public Guid Id { get; init; }
    public string ExternalId { get; init; } = String.Empty;
    public string Name { get; init; } = String.Empty;
    public string JwtToken { get; init; } = String.Empty;
    public DateTime JwtTokenUpdatedAt { get; init; }
    public DateTime StravaTokenUpdatedAt { get; init; }
}