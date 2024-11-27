namespace AuthService.DTOs;
public record CreateSessionDto(string Provider, Guid UserId, string AccessToken, string RefreshToken, DateTime AccessExpiresAt, DateTime RefreshExpiresAt);
public record ResponseCreateSessionDto(string SessionId);