using System.Security.Claims;

namespace Backend.Services;

public interface ICurrentUserService
{
    string UserId { get; }
}

public class CurrentUserService(IHttpContextAccessor http) : ICurrentUserService
{
    public string UserId =>
        http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException("Missing user id claim");
}