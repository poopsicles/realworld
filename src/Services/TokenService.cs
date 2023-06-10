using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Realworld.Models;

namespace Realworld.Services;

public interface ITokenService
{
    string CreateToken(UserModel user);
}

/// <summary>
/// Generates tokens
/// </summary>
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private const int ExpirationMinutes = 360;
    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Creates a JWT that contains a user's claims
    /// </summary>
    /// <param name="user"></param>
    /// <returns>The JWT in Compact Serialization Format.</returns>
    public string CreateToken(UserModel user)
    {
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];
        var key = _configuration["Jwt:Key"];

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
             {
                new Claim("Id", user.ID.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
             }),
            Expires = DateTime.UtcNow.AddMinutes(ExpirationMinutes),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key!)), 
                SecurityAlgorithms.HmacSha512)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}