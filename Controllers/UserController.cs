using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SearchTool_ServerSide.Dtos.UserDtos;
using SearchTool_ServerSide.Services;

namespace SearchTool_ServerSide.Controllers
{
    [ApiController]
    [Route("user")]
    public class UserController(UserSevice _userService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] UserAddDto userAddDto)
        {
            var oldUser = await _userService.GetUserByEmail(userAddDto.Email);
            if (oldUser != null)
            {
                return BadRequest("This email is already exist");
            }
            userAddDto.Password = BCrypt.Net.BCrypt.HashPassword(userAddDto.Password);
            var user = await _userService.Register(userAddDto);
            return Ok(user);
        }
         [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto userLoginDto)
        {
            var tokens = await _userService.Login(userLoginDto);
            return tokens != null ? Ok(new
            {
                accessToken = tokens.Value.accessToken,
                refreshToken = tokens.Value.refreshToken,
                userId = tokens.Value.userId
            }) : BadRequest("Invalid email or password");
        }
        [HttpGet("token-test")]
        [Authorize]
        public IActionResult TokenTest()
        {
            return Ok("Authorized");
        }
        [HttpPost("access-token/{userId}")]
        [Authorize]
        public async Task<IActionResult> GenerateToken(int userId)
        {
            var user = await _userService.GetUserById(userId);
            if (user == null)
            {
                return Forbid();
            }
            var tokens = await _userService.Refresh(user.Email);
            return tokens != null ? Ok(new
            {
                accessToken = tokens.Value.accessToken,
                refreshToken = tokens.Value.refreshToken,
                userId = tokens.Value.userId
            }) : Unauthorized("Invalid email or password");
        }

    }
}