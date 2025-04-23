
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SearchTool_ServerSide.Dtos.TokenDto;
using ServerSide;
using ServerSide.Model;

namespace SearchTool_ServerSide.Authentication
{
    public class UserAccessToken(IHttpContextAccessor _httpContextAccessor, JwtOptions _jwtOptions)
    {
        public bool IsAuthenticated(int entityId)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                return false;
            }
            Console.WriteLine("sdsa : " + user.IsInRole("Admin"));

            var role = user.FindFirst(ClaimTypes.Role)?.Value;
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = user.FindFirst(ClaimTypes.Email)?.Value;
            var branchId = user.FindFirst("BranchId")?.Value;
            Console.WriteLine("sdsa : " + branchId + " : " + entityId);

            if (role == "SuperAdmin")
                return true;
            if (branchId == entityId.ToString())
            {
                return true;
            }

            return false;
        }
        public bool IsAuthenticatedUser(int userId)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                return false;
            }
            var personId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = user.FindFirst(ClaimTypes.Role)?.Value;
            if (role == "SuperAdmin")
            {
                return true;
            }
            return personId == userId.ToString();

        }
        public TokenReadDto tokenData()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                return null;
            }
            Console.WriteLine("sdsa : " + user.IsInRole("Admin"));

            var role = user.FindFirst(ClaimTypes.Role)?.Value;
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = user.FindFirst(ClaimTypes.Email)?.Value;
            var branchId = user.FindFirst("BranchId")?.Value;
            return new TokenReadDto { UserId = userId, UserRole = role, Email = email, BranchId = branchId };
        }
        public TokenReadDto ValidateRefreshToken(string refreshToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtOptions.SigningKey);
            try
            {
                var principal = tokenHandler.ValidateToken(refreshToken, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true, // Since it's a refresh token
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var role = principal.FindFirst(ClaimTypes.Role)?.Value;
                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var email = principal.FindFirst(ClaimTypes.Email)?.Value;
                var branchId = principal.FindFirst("BranchId")?.Value;
                return new TokenReadDto { UserId = userId, UserRole = role, Email = email, BranchId = branchId };
            }
            catch
            {
                return null;
            }
        }
    }
}