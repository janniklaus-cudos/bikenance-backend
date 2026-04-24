namespace Backend.Integrations.Strava.Dtos;

public record StravaActivity(
    long id,
    string name,
    DateTime start_date,
    double distance,
    string sport_type
    );

