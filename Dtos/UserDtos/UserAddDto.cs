using SearchTool_ServerSide.Models;

namespace SearchTool_ServerSide.Dtos.UserDtos
{
    public class UserAddDto
    {
        public required string Email { get; set; }
        public required string ShortName { get; set; }
        public required string Name { get; set; }
        public required string Password { get; set; }
        public int BranchId { get; set; }
        public Role Role { get; set; } = Role.Pharmacist;
    }
}