using Backend.Dtos;
using Backend.Integrations.Strava.Dtos;

namespace Backend.Integrations.Strava;

public class StravaClient(HttpClient http, IConfiguration cfg)
{
    public string BuildAuthorizeUrl(string state)
    {
        var clientId = cfg["Strava:ClientId"];
        var redirectUri = Uri.EscapeDataString(cfg["Strava:RedirectUri"]!);
        var scope = Uri.EscapeDataString("activity:read_all");
        return $"https://www.strava.com/oauth/authorize" +
               $"?client_id={clientId}&response_type=code&redirect_uri={redirectUri}" +
               $"&approval_prompt=auto&scope={scope}&state={state}";
    }

    public async Task<StravaTokenResponse> ExchangeCodeAsync(string code, CancellationToken ct)
    {
        var clientId = cfg["Strava:ClientId"]!;
        var clientSecret = cfg["Strava:ClientSecret"]!;

        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["code"] = code,
            ["grant_type"] = "authorization_code"
        });

        var res = await http.PostAsync("https://www.strava.com/oauth/token", form, ct);
        res.EnsureSuccessStatusCode();
        return (await res.Content.ReadFromJsonAsync<StravaTokenResponse>(cancellationToken: ct))!;
    }

    readonly string[] acceptedSportTypes = [
        "EBikeRide",
        "EMountainBikeRide",
        "GravelRide",
        "MountainBikeRide",
        "Ride",
    ];

    public async Task<List<StravaActivity>> GetActivitiesAsync(string accessToken, int perPage, int page, CancellationToken ct)
    {
        using var req = new HttpRequestMessage(
            HttpMethod.Get,
            $"https://www.strava.com/api/v3/athlete/activities?per_page={perPage}&page={page}");

        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var res = await http.SendAsync(req, ct);
        res.EnsureSuccessStatusCode();
        var activities = await res.Content.ReadFromJsonAsync<List<StravaActivity>>(cancellationToken: ct);

        if (activities == null) return [];

        activities = [.. activities.Where(activity => acceptedSportTypes.Contains(activity.sport_type))];
        return activities;
    }

}