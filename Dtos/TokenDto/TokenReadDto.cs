namespace SearchTool_ServerSide.Dtos.TokenDto
{

    // var role = user.FindFirst(ClaimTypes.Role)?.Value;
    // var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    // var email = user.FindFirst(ClaimTypes.Email)?.Value;
    // var branchId = user.FindFirst("BranchId")?.Value;
    public class TokenReadDto
    {
        public string UserId { get; set; }
        public string UserRole { get; set; }
        public string Email { get; set; }
        public string BranchId { get; set; }
    }
}