using AuthService.DTOs;
using AuthService.Helpers;
using AuthService.Repositories;

namespace AuthService.Services;

public interface IResetPasswordService
{
    Task<ResponseMessageDto> CreateResetPasswordTokenAsync(Guid userId);
    Task<ResponseMessageDto> UpdatePasswordByTokenAsync(ResetPasswordDto resetPasswordDto);
}

public class ResetPasswordService : IResetPasswordService
{
    private readonly ILogger<ResetPasswordService> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IResetPasswordRepository _resetPasswordRepository;
    private readonly ISendMail _sendMail;
    private readonly IJwtTokenHelper _jwtTokenHelper;
    private readonly IPasswordHashedHelper _passwordHasher;
    public ResetPasswordService(IPasswordHashedHelper passwordHasher, ILogger<ResetPasswordService> logger, IResetPasswordRepository resetPasswordRepository, ISendMail sendMail, IUserRepository userRepository, IJwtTokenHelper jwtTokenHelper)
    {
        _passwordHasher = passwordHasher;
        _logger = logger;
        _resetPasswordRepository = resetPasswordRepository;
        _sendMail = sendMail;
        _userRepository = userRepository;
        _jwtTokenHelper = jwtTokenHelper;
    }
    public async Task<ResponseMessageDto> CreateResetPasswordTokenAsync(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            _logger.LogError("UserId is null or empty ON CreateResetPasswordToken Service");
            throw new ArgumentNullException(nameof(userId), "UserId is null or empty");
        }
        try
        {
            var user = await _userRepository.SelectUserByIdAsync(userId);
            if (user == null)
            {
                _logger.LogError("User not found ON CreateResetPasswordToken Service");
                return new ResponseMessageDto("User not found");
            }
            var existingToken = await _resetPasswordRepository.SelectResetPasswordTokenByUserIdAsync(user.Id);
            if (existingToken != null)
            {
                await _resetPasswordRepository.DeleteResetPasswordTokenByTokenIdAsync(existingToken.Id);
            }
            var token = _jwtTokenHelper.GenerateJwtResetPasswordToken(user.Email);
            var resetPasswordToken = new ResetPasswordToken
            {
                UserId = user.Id,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };
            await _resetPasswordRepository.InsertResetPasswordTokenAsync(resetPasswordToken);
            await _sendMail.SendResetPasswordEmailAsync(user.Email, token);
            return new ResponseMessageDto("Reset Password Token Created");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error on CreateResetPasswordToken Service");
            throw;
        }
    }
    public async Task<ResponseMessageDto> UpdatePasswordByTokenAsync(ResetPasswordDto resetPasswordDto)
    {
        if (resetPasswordDto == null)
        {
            _logger.LogError("ResetPasswordDto is null ON UpdatePassword Service");
            throw new ArgumentNullException(nameof(resetPasswordDto), "ResetPasswordDto is null");
        }
        try
        {
            if (resetPasswordDto.Password != resetPasswordDto.ConfirmPassword)
            {
                _logger.LogError("Password and ConfirmPassword do not match ON UpdatePassword Service");
                return new ResponseMessageDto("Password and ConfirmPassword do not match");
            }
            var resetPasswordToken = await _resetPasswordRepository.SelectResetPasswordTokenByTokenAsync(resetPasswordDto.Token);
            if (resetPasswordToken == null)
            {
                _logger.LogError("Reset Password Token not found ON UpdatePassword Service");
                return new ResponseMessageDto("Reset Password Token not found");
            }
            if (resetPasswordToken.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogError("Reset Password Token has expired ON UpdatePassword Service");
                return new ResponseMessageDto("Reset Password Token has expired");
            }
            var email = _jwtTokenHelper.ValidateJwtResetPasswordToken(resetPasswordDto.Token);
            if (email == null)
            {
                _logger.LogError("Invalid Reset Password Token ON UpdatePassword Service");
                return new ResponseMessageDto("Invalid Reset Password Token");
            }
            var user = await _userRepository.SelectUserByEmailAsync(email);
            if (user == null)
            {
                _logger.LogError("User not found ON UpdatePassword Service");
                return new ResponseMessageDto("User not found");
            }
            var passwordHash = this._passwordHasher.HashPassword(resetPasswordDto.Password);
            await _userRepository.UpdatePasswordByEmailAsync(user.Email, passwordHash);
            return new ResponseMessageDto("Password Updated");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error on UpdatePassword Service");
            throw;
        }

    }
}