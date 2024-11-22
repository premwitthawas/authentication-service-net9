using AuthService.DTOs;
using AuthService.Helpers;
using AuthService.Models;
using AuthService.Repositories;
using AuthService.Services;
using Microsoft.Extensions.Logging;
using Moq;
namespace AuthService.Tests.Services;

public class UserServiceTest
{
    private readonly Mock<IUserRepository> _userRepository;
    private readonly Mock<ILogger<UserService>> _logger;
    private readonly Mock<IPasswordHashedHelper> _passwordHashedHelper;
    private readonly UserService _userService;

    public UserServiceTest()
    {
        _userRepository = new Mock<IUserRepository>();
        _logger = new Mock<ILogger<UserService>>();
        _passwordHashedHelper = new Mock<IPasswordHashedHelper>();
        _userService = new UserService(_userRepository.Object, _logger.Object, _passwordHashedHelper.Object);
    }

    [Fact]
    public async Task RegisterUser_ShouldThrowArgumentNullException_WhenCreateUserDtoIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _userService.RegisterUser(null));
    }

    [Fact]
    public async Task RegisterUser_ShouldThrowArgumentException_WhenUsernameIsEmpty()
    {
        var createUserDto = new CreateUserDto("", "email@example.com", "password123");
        await Assert.ThrowsAsync<ArgumentNullException>(() => _userService.RegisterUser(createUserDto));
    }

    [Fact]
    public async Task RegisterUser_ShouldThrowArgumentException_WhenPasswordIsEmpty()
    {
        var createUserDto = new CreateUserDto("username", "email@example.com", "");
        await Assert.ThrowsAsync<ArgumentNullException>(() => _userService.RegisterUser(createUserDto));
    }

    [Fact]
    public async Task RegisterUser_ShouldCallRepository_WhenValidCreateUserDtoIsProvided()
    {
        var createUserDto = new CreateUserDto("username", "email@example.com", "password123");
        _passwordHashedHelper.Setup(p => p.HashPassword(It.IsAny<string>())).Returns("hashedPassword");
        _userRepository.Setup(r => r.InsertUser(It.IsAny<User>())).ReturnsAsync(new User());
        await _userService.RegisterUser(createUserDto);
        _userRepository.Verify(repo => repo.InsertUser(It.Is<User>(u => u.UserName == "username" && u.Email == "email@example.com" && u.Password == "hashedPassword")), Times.Once);
        _passwordHashedHelper.Verify(p => p.HashPassword(It.IsAny<string>()), Times.Once);
    }
}