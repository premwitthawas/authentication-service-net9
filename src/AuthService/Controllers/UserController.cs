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
                var response = await this._userService.RegisterUserAsync(createUserDto);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}