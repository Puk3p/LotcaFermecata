using Microsoft.Extensions.Options;
using SP25.WebApi;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

public class JwtTokenService
{
    private readonly JwtOptions _jwtOptions;

    public JwtTokenService(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    public string GenerateToken(string userId, string username)
    {
        var issuedAt = DateTime.UtcNow;
        var expires = issuedAt.Add(_jwtOptions.ValidFor);

        var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, userId),
        new Claim(ClaimTypes.Name, username),
        new Claim(JwtRegisteredClaimNames.Jti, JwtOptions.JtiGenerator),
        new Claim(JwtRegisteredClaimNames.Iat,
            ((DateTimeOffset)issuedAt).ToUnixTimeSeconds().ToString(),
            ClaimValueTypes.Integer64)
    };

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            notBefore: issuedAt,
            expires: expires,
            signingCredentials: _jwtOptions.SigningCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}