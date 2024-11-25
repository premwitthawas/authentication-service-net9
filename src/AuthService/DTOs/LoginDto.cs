namespace AuthService.DTOs;

public record LoginDto(string Username, string Password);

public record ReponseLoginDto(string AccessToken, string RefreshToken);