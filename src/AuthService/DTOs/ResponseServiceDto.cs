namespace AuthService.DTOs;
public record ResponseServiceDto<T>(T Data, string Message, bool Success, int StatusCode);
