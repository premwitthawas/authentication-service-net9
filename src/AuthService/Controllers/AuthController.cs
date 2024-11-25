using AuthService.DTOs;
using AuthService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthApplicaitonService _authApplicaitonService;
        private readonly IVerifyEmailService _verifyEmailService;
        private readonly IResetPasswordService _resetPasswordService;
        private readonly ILogger<AuthController> _logger;
        public AuthController(IAuthApplicaitonService applicaitonService, ILogger<AuthController> logger, IResetPasswordService resetPasswordService, IVerifyEmailService verifyEmailService)
        {
            _logger = logger;
            _resetPasswordService = resetPasswordService;
            _verifyEmailService = verifyEmailService;
            _authApplicaitonService = applicaitonService;
        }
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginDto loginDto)
        {
            try
            {
                var response = await this._authApplicaitonService.LoginAsync(loginDto);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error occurred while logging in");
                throw;
            }
        }
        [HttpPost("send-verify-email")]
        public async Task<IActionResult> SendVerifyEmail([FromBody] SendVerifyEmailDto sendVerifyEmailDto)
        {
            try
            {
                var response = await this._verifyEmailService.CreateTokenVerifyEmail(sendVerifyEmailDto.Email);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("verify-email/{token}")]
        public async Task<IActionResult> VerifyEmail(string token)
        {
            if (token == null)
            {
                this._logger.LogError("Token is null");
                return BadRequest("Invalid token");
            }
            try
            {
                var response = await this._verifyEmailService.VerifyEmail(token);
                this._logger.LogInformation("Email verified successfully");
                return Ok(response);
            }
            catch
            {
                this._logger.LogError("Error occurred while verifying email");
                throw;
            }
        }
        [HttpPost("send-reset-password/{id}")]
        public async Task<IActionResult> SendResetPassword(string id)
        {
            try
            {
                var response = await this._resetPasswordService.CreateResetPasswordTokenAsync(Guid.Parse(id));
                this._logger.LogInformation("Password reset successfully");
                return StatusCode(response.StatusCode, response);
            }
            catch
            {
                this._logger.LogError("Error occurred while resetting password");
                throw;
            }
        }
        [HttpPut("reset-password")]
        public async Task<IActionResult> ResetPasswordByToken(ResetPasswordDto resetPasswordDto)
        {
            try
            {
                var response = await this._resetPasswordService.UpdatePasswordByTokenAsync(resetPasswordDto);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error occurred while resetting password");
                throw;
            }
        }
    }
}