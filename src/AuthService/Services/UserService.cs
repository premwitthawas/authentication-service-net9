using AuthService.DTOs;
using AuthService.Helpers;
using AuthService.Models;
using AuthService.Repositories;

namespace AuthService.Services;


public interface IUserService
{
    Task<ResponseServiceDto<ResponseCreateUserDto>> RegisterUserAsync(CreateUserDto createUserDto);
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

    public async Task<ResponseServiceDto<ResponseCreateUserDto>> RegisterUserAsync(CreateUserDto createUserDto)
    {
        if (createUserDto == null)
        {
            return new ResponseServiceDto<ResponseCreateUserDto>(null, "CreateUserDto cannot be null.", false, 400);
        }
        if (string.IsNullOrWhiteSpace(createUserDto.Username))
        {
            return new ResponseServiceDto<ResponseCreateUserDto>(null, "Username cannot be null or empty.", false, 400);
        }
        if (string.IsNullOrWhiteSpace(createUserDto.Email))
        {
            return new ResponseServiceDto<ResponseCreateUserDto>(null, "Email cannot be null or empty.", false, 400);
        }
        if (string.IsNullOrWhiteSpace(createUserDto.Password))
        {
            return new ResponseServiceDto<ResponseCreateUserDto>(null, "Password cannot be null or empty.", false, 400);
        }
        try
        {
            var existingUser = await this._userRepository.SelectUserByUsernameAsync(createUserDto.Username);
            if (existingUser != null)
            {
                return new ResponseServiceDto<ResponseCreateUserDto>(null, "Username already exists.", false, 400);
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
            return new ResponseServiceDto<ResponseCreateUserDto>(new ResponseCreateUserDto(result.Id, result.UserName, result.Email), "User registered successfully.", true, 201);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error occurred while registering user.");
            return new ResponseServiceDto<ResponseCreateUserDto>(null, "Error occurred while registering user.", false, 500);
        }
    }
}