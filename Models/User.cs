using Microsoft.AspNetCore.Identity;
using ServerSide.Model;

namespace SearchTool_ServerSide.Models
{
    public class User : IEntity
    {
        public int Id { get; set; }
        public required string Email { get; set; }
        public required string ShortName { get; set; }
        public required string Name { get; set; }
        public required string Password { get; set; }
        public int BranchId { get; set; }
        public ICollection<Log> Logs { get; set; }
        public Branch Branch { get; set; }
        public Role Role { get; set; } = Role.Pharmacist;
    }
}