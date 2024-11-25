using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Helpers;

public interface IJwtTokenHelper
{
    string GenerateJwtEmailVeifyToken(string email);
    bool ValidateJwtEmailVeifyToken(string token);
    string GenerateJwtResetPasswordToken(string email);
    string ValidateJwtResetPasswordToken(string token);
    string GenerateJwtAccessToken(string userId);
    string GenerateJwtRefeshToken(string userId);
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

    public string GenerateJwtAccessToken(string userId)
    {
        string acess_key = _configuration["Jwt:accessTokenSecret"].ToString();
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(acess_key));
        var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            claims: [new Claim(ClaimTypes.NameIdentifier, userId)],
            expires: DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:accessTokenExpiration"])),
            signingCredentials: creds,
            audience: _configuration["Jwt:Audience"].ToString(),
            issuer: _configuration["Jwt:Issuer"].ToString()
        );
        this._logger.LogInformation($"Generated Access JWT token for user {userId}");
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    public string GenerateJwtEmailVeifyToken(string email)
    {
        string secret = _configuration["Jwt:VerifyEmailSecret"].ToString();
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            claims: [new Claim(ClaimTypes.Email, email)],
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds
        );
        this._logger.LogInformation($"Generated JWT token for email verification");
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    public string GenerateJwtRefeshToken(string userId)
    {
        string acess_key = _configuration["Jwt:refeshTokenSecret"].ToString();
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(acess_key));
        var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            claims: [new Claim(ClaimTypes.NameIdentifier, userId)],
            expires: DateTime.UtcNow.AddDays(double.Parse(_configuration["Jwt:refreshTokenExpiration"])),
            signingCredentials: creds,
            audience: _configuration["Jwt:Audience"].ToString(),
            issuer: _configuration["Jwt:Issuer"].ToString()
        );
        this._logger.LogInformation($"Generated Refesh JWT token for user {userId}");
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    public string GenerateJwtResetPasswordToken(string email)
    {
        string secret = _configuration["Jwt:ResetPasswordSecret"].ToString();
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            claims: [new Claim(ClaimTypes.Email, email)],
            expires: DateTime.UtcNow.AddMinutes(15),
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

    public string ValidateJwtResetPasswordToken(string token)
    {
        string secret = _configuration["Jwt:ResetPasswordSecret"].ToString();
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var tokenParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = securityKey,
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
            tokenHandler.ValidateToken(token, tokenParameters, out SecurityToken validatedToken);
            var jwtToken = validatedToken as JwtSecurityToken;
            this._logger.LogInformation($"Validated JWT token for reset password verification");
            return jwtToken.Claims.First(claim => claim.Type == ClaimTypes.Email).Value;
        }
        catch (Exception ex)
        {
            this._logger.LogError($"Error validating JWT token for reset password verification: {ex.Message}");
            return null;
        }
    }
}
