using AuthService.DTOs;
using AuthService.Helpers;
using AuthService.Models;
using AuthService.Repositories;
using AuthService.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ILogger<UserService>> _loggerMock;
    private readonly Mock<IPasswordHashedHelper> _passwordHashedHelperMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<UserService>>();
        _passwordHashedHelperMock = new Mock<IPasswordHashedHelper>();
        _userService = new UserService(_userRepositoryMock.Object, _loggerMock.Object, _passwordHashedHelperMock.Object);
    }

    [Fact]
    public async Task RegisterUserAsync_CreateUserDtoIsNull_ReturnsBadRequest()
    {
        // Act
        var result = await _userService.RegisterUserAsync(null);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("CreateUserDto cannot be null.", result.Message);
    }

    [Fact]
    public async Task RegisterUserAsync_UsernameIsNullOrEmpty_ReturnsBadRequest()
    {

        var createUserDto = new CreateUserDto("", "test@example.com", "password");
        var result = await _userService.RegisterUserAsync(createUserDto);
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("Username cannot be null or empty.", result.Message);
    }

    [Fact]
    public async Task RegisterUserAsync_EmailIsNullOrEmpty_ReturnsBadRequest()
    {
        var createUserDto = new CreateUserDto("username", "", "password");
        var result = await _userService.RegisterUserAsync(createUserDto);
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("Email cannot be null or empty.", result.Message);
    }

    [Fact]
    public async Task RegisterUserAsync_PasswordIsNullOrEmpty_ReturnsBadRequest()
    {
        var createUserDto = new CreateUserDto("username", "test@example.com", "");
        var result = await _userService.RegisterUserAsync(createUserDto);
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("Password cannot be null or empty.", result.Message);
    }

    [Fact]
    public async Task RegisterUserAsync_UsernameAlreadyExists_ReturnsBadRequest()
    {
        var createUserDto = new CreateUserDto("existinguser", "test@example.com", "password");
        _userRepositoryMock.Setup(repo => repo.SelectUserByUsernameAsync(It.IsAny<string>())).ReturnsAsync(new User()
        {
            UserName = "existinguser",
            Email = "test@example.com",
            Password = "password",
            RoleId = 2
        });
        var result = await _userService.RegisterUserAsync(createUserDto);
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("Username already exists.", result.Message);
    }

    [Fact]
    public async Task RegisterUserAsync_SuccessfulRegistration_ReturnsCreated()
    {
        var createUserDto = new CreateUserDto("newuser", "test@example.com", "password");
        _userRepositoryMock.Setup(repo => repo.SelectUserByUsernameAsync(It.IsAny<string>())).ReturnsAsync(default(User));
        _passwordHashedHelperMock.Setup(helper => helper.HashPassword(It.IsAny<string>())).Returns("hashedpassword");
        _userRepositoryMock.Setup(repo => repo.InsertUserAsync(It.IsAny<User>())).ReturnsAsync(new User { Id = Guid.NewGuid(), UserName = "newuser", Email = "test@example.com" });
        var result = await _userService.RegisterUserAsync(createUserDto);
        Assert.True(result.Success);
        Assert.Equal(201, result.StatusCode);
        Assert.Equal("User registered successfully.", result.Message);
        _passwordHashedHelperMock.Verify(helper => helper.HashPassword("password"), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_ExceptionThrown_ReturnsInternalServerError()
    {
        var createUserDto = new CreateUserDto("newuser", "test@example.com", "password");
        _userRepositoryMock.Setup(repo => repo.SelectUserByUsernameAsync(It.IsAny<string>())).ThrowsAsync(new Exception("Database error"));
        var result = await _userService.RegisterUserAsync(createUserDto);
        Assert.False(result.Success);
        Assert.Equal(500, result.StatusCode);
        Assert.Equal("Error occurred while registering user.", result.Message);
    }
}