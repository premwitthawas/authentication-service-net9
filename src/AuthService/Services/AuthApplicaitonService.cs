using AuthService.DTOs;
using AuthService.Helpers;
using AuthService.Repositories;

namespace AuthService.Services;

public interface IAuthApplicaitonService
{
    Task<ResponseServiceDto<ReponseLoginDto>> LoginAsync(LoginDto loginDto);
}

public class AuthApplicaitonService : IAuthApplicaitonService
{
    private readonly ILogger<AuthApplicaitonService> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenHelper _jwtTokenHelper;
    private readonly IPasswordHashedHelper _passwordHashedHelper;
    public AuthApplicaitonService(ILogger<AuthApplicaitonService> logger, IUserRepository userRepository, IPasswordHashedHelper passwordHashedHelper, IJwtTokenHelper jwtTokenHelper)
    {
        _logger = logger;
        _userRepository = userRepository;
        _passwordHashedHelper = passwordHashedHelper;
        _jwtTokenHelper = jwtTokenHelper;
    }
    public async Task<ResponseServiceDto<ReponseLoginDto>> LoginAsync(LoginDto loginDto)
    {
        if (string.IsNullOrEmpty(loginDto.Username) || string.IsNullOrEmpty(loginDto.Password))
        {
            this._logger.LogError("Username or password is null or empty");
            return new ResponseServiceDto<ReponseLoginDto>(null, "Username or password is null or empty", false, 400);
        }
        try
        {
            var user = await _userRepository.SelectUserByUsernameAsync(loginDto.Username);
            _logger.LogInformation("User found for username: {Username}", user.UserName.ToString());
            if (user == null)
            {
                this._logger.LogError("User not found for username: {Username}", loginDto.Username);
                return new ResponseServiceDto<ReponseLoginDto>(null, "User not found", false, 404);
            }
            bool isMatchPass = this._passwordHashedHelper.ValidatePassword(loginDto.Password, user.Password);
            if (!isMatchPass)
            {
                this._logger.LogError("Password is incorrect for username: {Username}", loginDto.Username);
                return new ResponseServiceDto<ReponseLoginDto>(null, "Password is incorrect", false, 400);
            }
            var accessToken = this._jwtTokenHelper.GenerateJwtAccessToken(user.Id.ToString());
            var refreshToken = this._jwtTokenHelper.GenerateJwtRefeshToken(user.Id.ToString());
            this._logger.LogInformation("Login successful for username: {Username}", loginDto.Username);
            return new ResponseServiceDto<ReponseLoginDto>(new ReponseLoginDto(accessToken, refreshToken), "Login successful", true, 200);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "An error occurred while logging in");
            return new ResponseServiceDto<ReponseLoginDto>(null, "An error occurred while logging in", false, 500);
        }
    }
}