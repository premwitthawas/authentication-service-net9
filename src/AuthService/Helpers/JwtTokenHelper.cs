using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Helpers;

public interface IJwtTokenHelper
{
    string GenerateJwtEmailVeifyToken(string email);
    bool ValidateJwtEmailVeifyToken(string token);
}

public class JwtTokenHelper : IJwtTokenHelper
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtTokenHelper> _logger;
    public JwtTokenHelper(IConfiguration configuration, ILogger<JwtTokenHelper> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }
    public string GenerateJwtEmailVeifyToken(string email)
    {
        string secret = _configuration["Jwt:VerifyEmailSecret"].ToString();
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            claims: [new Claim(ClaimTypes.Email, email)],
            expires: DateTime.Now.AddMinutes(15),
            signingCredentials: creds
        );
        this._logger.LogInformation($"Generated JWT token for email verification");
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool ValidateJwtEmailVeifyToken(string token)
    {
        string secret = _configuration["Jwt:VerifyEmailSecret"].ToString();
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = securityKey,
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);
            this._logger.LogInformation($"Validated JWT token for email verification");
            return true;
        }
        catch (Exception ex)
        {
            this._logger.LogError($"Error validating JWT token for email verification: {ex.Message}");
            return false;
        }
    }
}
