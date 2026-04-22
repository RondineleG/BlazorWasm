using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace BlazorTerminal.Api.Services;

public class JwtService(IConfiguration configuration)
{
    private readonly string _secretKey = configuration["Jwt:Secret"] ?? "MrBurTerminalSecretKey2026!@#$%";
    private readonly string _issuer = configuration["Jwt:Issuer"] ?? "MrBurTerminal";
    private readonly int _expiryHours = int.Parse(configuration["Jwt:ExpiryHours"] ?? "24");

    public string GenerateToken(string username, int userId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, username),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _issuer,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_expiryHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public int GetExpirySeconds() => _expiryHours * 3600;
}