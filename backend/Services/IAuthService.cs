using Backend.Dtos;

namespace Backend.Services;

public interface IAuthService
{
    Task<UserDto> ValidateGoogleToken(string token);
}
