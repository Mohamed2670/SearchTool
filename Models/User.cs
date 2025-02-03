using Microsoft.AspNetCore.Identity;

namespace SearchTool_ServerSide.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Email { get; set; }
        public required string Name { get; set; }
        public required string Password { get; set; }
        public Role Role = Role.Staff;
    }
}