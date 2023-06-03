using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Realworld.Models;
using Realworld.Services;

namespace realworld_unit_tests;

public class TestTokenService : ITokenService
{
    string _key = "SUPERSECRETKEY!!";
    string _issuer = "TestIssuer";
    string _audience = "TestAudience";

    public string CreateToken(UserModel user)
    {
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
             {
                new Claim("Id", user.ID.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.username),
                new Claim(JwtRegisteredClaimNames.Email, user.email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
             }),
            Expires = DateTime.UtcNow.AddMinutes(1),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_key!)),
                SecurityAlgorithms.HmacSha512)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public bool ValidateToken(string token, string username, string email) {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidateTokenReplay = false,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_key!)),
            ValidAudience = _audience,
            ValidIssuer = _issuer,
        };

        var jwt = tokenHandler.ValidateToken(token, validationParameters, out _);
        
        if (jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value != username) {
            return false;
        }

        if (jwt.Claims.First(c => c.Type == ClaimTypes.Email).Value != email) {
            return false;
        }

        return true;
    }
}