using AuthService.DTOs;
using AuthService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IVerifyEmailService _verifyEmailService;
        public UserController(IUserService userService, IVerifyEmailService verifyEmailService)
        {
            _userService = userService;
            _verifyEmailService = verifyEmailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] CreateUserDto createUserDto)
        {
            try
            {
                var response = await this._userService.RegisterUser(createUserDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
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
    }
}