public class StravaTokenStore
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public long ExpiresAt { get; set; }
}