using SearchTool_ServerSide.Models;

namespace SearchTool_ServerSide.Dtos.UserDtos
{
    public class UserReadDto
    {
         public required string Email { get; set; }
        public required string Name { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; }
        public string RoleName { get; set; }
        public Role Role { get; set; } = Role.Pharmacist;
    }
}