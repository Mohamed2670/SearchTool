using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SearchTool_ServerSide.Authentication;
using SearchTool_ServerSide.Dtos.UserDtos;
using SearchTool_ServerSide.Services;

namespace SearchTool_ServerSide.Controllers
{
    [ApiController]
    [Route("user")]
    public class UserController(UserSevice _userService, UserAccessToken userAccessToken) : ControllerBase
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
            if (tokens == null)
                return Unauthorized(new { message = "Invalid email or password" });

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, // Prevent access from JavaScript
                Secure = true, // Use only on HTTPS
                SameSite = SameSiteMode.Strict, // Prevent CSRF
                Expires = DateTime.UtcNow.AddDays(1) // Expiration time
            };

            Response.Cookies.Append("refreshToken", tokens.Value.refreshToken, cookieOptions);

            return Ok(new { accessToken = tokens.Value.accessToken });
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
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, // Prevent access from JavaScript
                Secure = true, // Use only on HTTPS
                SameSite = SameSiteMode.Strict, // Prevent CSRF
                Expires = DateTime.UtcNow.AddDays(1) // Expiration time
            };
            Response.Cookies.Append("refreshToken", tokens.Value.refreshToken, cookieOptions);

            return tokens != null ? Ok(new
            {
                accessToken = tokens.Value.accessToken,

            }) : BadRequest("Invalid email or password");
        }
        [HttpGet("UserById"), Authorize(Policy = "Pharmacist")]
        public async Task<IActionResult> GetUserById()
        {
            var userData = userAccessToken.tokenData();
            if (userData == null || string.IsNullOrEmpty(userData.UserId))
            {
                return Unauthorized("Invalid or missing token data");
            }

            if (!int.TryParse(userData.UserId, out int userId))
            {
                return BadRequest("Invalid user ID format");
            }

            // Use logging instead of Console.WriteLine

            var user = await _userService.GetUserById(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }
            return Ok(user);
        }

        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser(UserUpdateDto userUpdateDto)
        {
            var userData = userAccessToken.tokenData();
             if (userData == null || string.IsNullOrEmpty(userData.UserId))
            {
                return Unauthorized("Invalid or missing token data");
            }

            if (!int.TryParse(userData.UserId, out int userId))
            {
                return BadRequest("Invalid user ID format");
            }

            if (userUpdateDto.Password != null)
            {
                userUpdateDto.Password = BCrypt.Net.BCrypt.HashPassword(userUpdateDto.Password);
            }
            var user = await _userService.UserUpdate(userId, userUpdateDto);
            return Ok(user);
        }
        [HttpDelete("id")]
        public async Task<IActionResult> DeleteUserById(int id)
        {
            var user = await _userService.DeleteUserById(id);
            if (user == null)
            {
                return BadRequest("NotFound User");
            }
            return Ok(user);
        }
        [HttpGet]
        public async Task<IActionResult> GetAllUser()
        {
            var users = await _userService.GetAllUser();
            return Ok(users);
        }

    }
}