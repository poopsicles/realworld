using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Realworld.Models;
using Realworld.Services;

namespace realworld_unit_tests;

public class TestTokenService : ITokenService
{
    private readonly string _key = "SUPERSECRETKEY!!";
    private readonly string _issuer = "TestIssuer";
    private readonly string _audience = "TestAudience";

    /// <summary>
    /// Creates a JWT that contains a user's claims
    /// </summary>
    /// <param name="user"></param>
    /// <returns>The JWT in Compact Serialization Format.</returns>
    public string CreateToken(UserModel user)
    {
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
             {
                new Claim("Id", user.ID.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
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

    /// <summary>
    /// Checks if a token was signed with the username and email provided
    /// </summary>
    /// <param name="token"></param>
    /// <param name="username"></param>
    /// <param name="email"></param>
    /// <returns>true if the token is valid, false if not</returns>
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

    public IEnumerable<Claim> GetTokenClaims(string token) {
        var tokenHandler = new JwtSecurityTokenHandler();

        var jwt = tokenHandler.ReadJwtToken(token);

        return jwt.Claims;
    }
}