using AuthService.DTOs;
using AuthService.Helpers;
using AuthService.Repositories;

namespace AuthService.Services;

public interface IAuthApplicationService
{
    Task<ResponseMessageDto> VerifyEmail(string token);
}

public class AuthApplicationService : IAuthApplicationService
{
    private readonly ILogger<AuthApplicationService> _logger;
    private readonly IJwtTokenHelper _jwtTokenHelper;
    private readonly IVerifyEmailRepository _verifyEmailRepository;
    private readonly IUserRepository _userRepository;
    public AuthApplicationService(IUserRepository userRepository, ILogger<AuthApplicationService> logger, IJwtTokenHelper jwtTokenHelper, IVerifyEmailRepository verifyEmailRepository)
    {
        _logger = logger;
        _jwtTokenHelper = jwtTokenHelper;
        _verifyEmailRepository = verifyEmailRepository;
        _userRepository = userRepository;
    }
    public async Task<ResponseMessageDto> VerifyEmail(string token)
    {
        if (token == null)
        {
            this._logger.LogError("Token is null");
            return new ResponseMessageDto("Invalid token");
        }
        try
        {
            var existingToken = await this._verifyEmailRepository.SelectVerifyByToken(token);
            if (!this._jwtTokenHelper.ValidateJwtEmailVeifyToken(token))
            {
                this._logger.LogError("Token is invalid");
                return new ResponseMessageDto("Invalid token or expired");
            }
            if (existingToken.ExpiresAt < DateTime.UtcNow)
            {
                this._logger.LogError("Token is expired");
                return new ResponseMessageDto("Token is expired");
            }
            await this._userRepository.UpdateVerifyEmail(existingToken.UserId);
            await this._verifyEmailRepository.DeleteVerifyEmailAsync(existingToken.Id);
            return new ResponseMessageDto("Email verified successfully");
        }
        catch
        {
            this._logger.LogError("Error occurred while verifying email");
            throw;
        }
    }
};