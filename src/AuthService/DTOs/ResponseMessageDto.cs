namespace AuthService.DTOs;

public record ResponseMessageDto(string Message, int StatusCode, bool Success);
