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
            var user = await _userService.GetUserById(int.Parse(tokens.Value.userId));
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, // Prevent access from JavaScript
                Secure = true, // Use only on HTTPS
                SameSite = SameSiteMode.Strict, // Prevent CSRF
                Expires = DateTime.UtcNow.AddDays(1) // Expiration time
            };

            Response.Cookies.Append("refreshToken", tokens.Value.refreshToken, cookieOptions);

            return Ok(new { accessToken = tokens.Value.accessToken,role = user.Role.ToString()});
        }

        [HttpGet("token-test")]
        [Authorize]
        public IActionResult TokenTest()
        {
            return Ok("Authorized");
        }
        [HttpPost("access-token")]
        public async Task<IActionResult> GenerateToken()
        {
            // Get refresh token from cookies
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized("No refresh token found");
            }

            // Validate and extract user from refresh token
            var user =  userAccessToken.ValidateRefreshToken(refreshToken);
            if (user == null)
            {
                return Unauthorized("Invalid refresh token");
            }

            // Generate new access & refresh tokens
            var tokens = await _userService.Refresh(user.Email);
            if (tokens == null)
            {
                return BadRequest("Failed to refresh token");
            }

            // Set new refresh token in secure cookies
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, // Prevent JavaScript access
                Secure = true, // HTTPS only
                SameSite = SameSiteMode.Strict, // Prevent CSRF
                Expires = DateTime.UtcNow.AddDays(1) // Expiration time
            };
            Response.Cookies.Append("refreshToken", tokens.Value.refreshToken, cookieOptions);

            return Ok(new
            {
                accessToken = tokens.Value.accessToken
            });
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