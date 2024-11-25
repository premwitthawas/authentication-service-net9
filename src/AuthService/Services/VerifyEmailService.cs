using AuthService.DTOs;
using AuthService.Helpers;
using AuthService.Models;
using AuthService.Repositories;

namespace AuthService.Services;
public interface IVerifyEmailService
{
    Task<ResponseMessageDto> CreateTokenVerifyEmail(string email);
    Task<ResponseMessageDto> VerifyEmail(string token);
}
public class VerifyEmailService : IVerifyEmailService
{
    private readonly IVerifyEmailRepository _verifyEmailRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<VerifyEmailService> _logger;
    private readonly ISendMail _sendMail;
    private readonly IJwtTokenHelper _jwtTokenHelper;
    public VerifyEmailService(IVerifyEmailRepository verifyEmailRepository, ILogger<VerifyEmailService> logger, IUserRepository userRepository, ISendMail sendMail, IJwtTokenHelper jwtTokenHelper)
    {
        _verifyEmailRepository = verifyEmailRepository;
        _logger = logger;
        _userRepository = userRepository;
        _sendMail = sendMail;
        _jwtTokenHelper = jwtTokenHelper;
    }
    public async Task<ResponseMessageDto> CreateTokenVerifyEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            this._logger.LogError("Email is null or empty");
            return new ResponseMessageDto("Invalid email", 400, false);
        }

        try
        {
            var user = await this._userRepository.SelectUserByEmailAsync(email);
            if (user == null)
            {
                this._logger.LogError("User not found for email: {Email}", email);
                return new ResponseMessageDto("User not found", 404, false);
            }

            if (user.IsVerified)
            {
                this._logger.LogError("Email already verified for user ID: {UserId}", user.Id);
                return new ResponseMessageDto("Email already verified", 400, false);
            }

            var existingToken = await this._verifyEmailRepository.SelectVerifyByUserId(user.Id);
            if (existingToken != null)
            {
                await this._verifyEmailRepository.DeleteVerifyEmailAsync(existingToken.Id);
            }

            string jwtToken = this._jwtTokenHelper.GenerateJwtEmailVeifyToken(email);

            var token = new VerifyEmail
            {
                Email = user.Email,
                UserId = user.Id,
                Token = jwtToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };

            await this._verifyEmailRepository.InsertVerifyEmail(token);
            this._logger.LogInformation("New token created for user ID: {UserId}", user.Id);

            await this._sendMail.SendVerifyEmailAsync(email, token.Token);
            this._logger.LogInformation("Verification email sent to {Email}", email);

            return new ResponseMessageDto("Token created", 201, true);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "An error occurred while creating token for email: {Email}", email);
            throw new Exception("Failed to create verification token", ex);
        }
    }
    public async Task<ResponseMessageDto> VerifyEmail(string token)
    {
        if (token == null)
        {
            this._logger.LogError("Token is null");
            return new ResponseMessageDto("Invalid token", 400, false);
        }
        try
        {
            var existingToken = await this._verifyEmailRepository.SelectVerifyByToken(token);
            if (!this._jwtTokenHelper.ValidateJwtEmailVeifyToken(token))
            {
                this._logger.LogError("Token is invalid");
                return new ResponseMessageDto("Invalid token or expired", 400, false);
            }
            if (existingToken.ExpiresAt < DateTime.UtcNow)
            {
                this._logger.LogError("Token is expired");
                return new ResponseMessageDto("Token is expired", 400, false);
            }
            await this._userRepository.UpdateVerifyEmailAsync(existingToken.UserId);
            await this._verifyEmailRepository.DeleteVerifyEmailAsync(existingToken.Id);
            return new ResponseMessageDto("Email verified successfully", 200, true);
        }
        catch
        {
            this._logger.LogError("Error occurred while verifying email");
            throw;
        }
    }
}