using AuthService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthApplicationService _authApplicationService;
        private readonly ILogger<AuthController> _logger;
        public AuthController(ILogger<AuthController> logger, IAuthApplicationService authApplicationService)
        {
            _authApplicationService = authApplicationService;
            _logger = logger;
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
                var response = await this._authApplicationService.VerifyEmail(token);
                this._logger.LogInformation("Email verified successfully");
                return Ok(response);
            }
            catch
            {
                this._logger.LogError("Error occurred while verifying email");
                throw;
            }
        }
    }
}