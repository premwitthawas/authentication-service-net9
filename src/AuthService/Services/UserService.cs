using AuthService.DTOs;
using AuthService.Helpers;
using AuthService.Models;
using AuthService.Repositories;

namespace AuthService.Services;


public interface IUserService
{
    Task<ResponseCreateUserDto> RegisterUser(CreateUserDto createUserDto);
}

public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHashedHelper _passwordHashedHelper;
    public UserService(IUserRepository userRepository, ILogger<UserService> logger, IPasswordHashedHelper passwordHashedHelper)
    {
        _userRepository = userRepository;
        _logger = logger;
        _passwordHashedHelper = passwordHashedHelper;
    }

    public async Task<ResponseCreateUserDto> RegisterUser(CreateUserDto createUserDto)
    {
        if (createUserDto == null)
        {
            throw new ArgumentNullException(nameof(createUserDto), "CreateUserDto cannot be null.");
        }
        if (string.IsNullOrWhiteSpace(createUserDto.Username))
        {
            throw new ArgumentNullException(nameof(createUserDto.Username), "Username cannot be null or empty.");
        }
        if (string.IsNullOrWhiteSpace(createUserDto.Email))
        {
            throw new ArgumentNullException(nameof(createUserDto.Email), "Email cannot be null or empty.");
        }
        if (string.IsNullOrWhiteSpace(createUserDto.Password))
        {
            throw new ArgumentNullException(nameof(createUserDto.Password), "Password cannot be null or empty.");
        }
        try
        {
            var existingUser = await this._userRepository.SelectUserByUsernameAsync(createUserDto.Username);
            if (existingUser != null)
            {
                throw new InvalidOperationException($"Username: {createUserDto.Username} already exists.");
            }
            string hashedPassword = this._passwordHashedHelper.HashPassword(createUserDto.Password);
            User user = new()
            {
                UserName = createUserDto.Username,
                Email = createUserDto.Email,
                Password = hashedPassword,
                RoleId = 2
            };
            var result = await this._userRepository.InsertUserAsync(user);
            return new ResponseCreateUserDto(result.Id, result.UserName, result.Email);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error occurred while registering user.");
            throw;
        }
    }
}