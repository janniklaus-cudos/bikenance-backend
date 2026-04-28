using Backend.Services;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{

    [AllowAnonymous]
    [HttpPost("google")]
    public async Task<IActionResult> LoginGoogle([FromBody] string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest();
        }

        var user = await authService.ValidateGoogleToken(token);
        if (user == null)
        {
            return Unauthorized();
        }

        return Ok(user);
    }
}