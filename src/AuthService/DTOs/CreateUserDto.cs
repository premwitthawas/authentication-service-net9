namespace AuthService.DTOs
{
    public record CreateUserDto(string Username, string Email, string Password);
    public record ResponseCreateUserDto(Guid Id, string Username, string Email);

}