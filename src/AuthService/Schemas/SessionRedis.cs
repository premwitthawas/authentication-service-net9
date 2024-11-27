using System.Text.Json.Serialization;

namespace AuthService.Schemas;


public class SessionRedis
{
    public string SessionId { get; set; }
    public string UserId { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime AccessTokenExpires { get; set; }
    public DateTime RefreshTokenExpires { get; set; }
    public bool IsAccessTokenExpired()
    {
        return DateTime.UtcNow > AccessTokenExpires;
    }
    public bool IsRefreshTokenExpired()
    {
        return DateTime.UtcNow > RefreshTokenExpires;
    }
};
