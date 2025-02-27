
using System.Security.Claims;
using ServerSide.Model;

namespace SearchTool_ServerSide.Authentication
{
    public class UserAccessToken(IHttpContextAccessor _httpContextAccessor)
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
    }
}