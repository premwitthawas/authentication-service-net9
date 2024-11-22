namespace AuthService.DTOs
{
    public record ResponseCreateUserDto(Guid Id, string Username, string Email);
}