namespace Backend.Integrations.Strava.Dtos;

public record StravaTokenResponse(
    string access_token,
    string refresh_token,
    long expires_at,
    Athlete athlete);