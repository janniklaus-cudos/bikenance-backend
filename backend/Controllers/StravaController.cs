
using AutoMapper;
using Backend.Dtos;
using Backend.Integrations.Strava;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StravaController(StravaClient strava, StravaTokenStore store, IMapper mapper) : ControllerBase
{
    [HttpGet("connect")]
    public IActionResult Connect()
    {
        // state gegen CSRF (eigentlich pro User in Session/DB speichern)
        var state = Guid.NewGuid().ToString("N");
        var url = strava.BuildAuthorizeUrl(state);
        return Redirect(url);
    }

    // redirect URI: Strava calls this after authentication
    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string? state, CancellationToken ct)
    {
        var token = await strava.ExchangeCodeAsync(code, ct);

        // TODO: hier in DB speichern, verknüpft mit deinem eingeloggten User
        store.AccessToken = token.access_token;
        store.RefreshToken = token.refresh_token;
        store.ExpiresAt = token.expires_at;

        return Redirect("http://localhost:5173/journeys/strava-redirect");
    }

    [HttpGet("activities")]
    public async Task<IActionResult> GetActivities([FromQuery] int page = 1, [FromQuery] int perPage = 30, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(store.AccessToken))
            return Unauthorized("Not connected to Strava.");

        var activities = await strava.GetActivitiesAsync(store.AccessToken, perPage, page, ct);
        return Ok(mapper.Map<List<JourneyDto>>(activities));
    }
}