using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Backend.Dtos;
using Backend.Models;
using Backend.Repositories;
using Backend.Services;
using Google.Apis.Auth;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Services;

public class AuthService(IConfiguration _config, IUserRepository userRepository, IMapper _mapper) : IAuthService
{
    public async Task<UserDto?> ValidateGoogleToken(string token)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = [_config["Auth:Google:ClientId"]]
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(token, settings);

            var user = await userRepository.GetByEmailAsync(payload.Email);

            if (user == null)
            {
                user = new User
                {
                    Email = payload.Email,
                    Name = payload.Name,
                };
                userRepository.Add(user);
                await userRepository.SaveChangesAsync();
            }

            GenerateJwtToken(user);

            return _mapper.Map<UserDto>(user);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private void GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Auth:Jwt:Key"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        };

        var token = new JwtSecurityToken(
            issuer: _config["Auth:Jwt:Issuer"],
            audience: _config["Auth:Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: credentials);


        user.JwtToken = new JwtSecurityTokenHandler().WriteToken(token);
    }

}