using SearchTool_ServerSide.Models;

namespace SearchTool_ServerSide.Dtos.UserDtos
{
    public class UserUpdateDto
    {
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? Password { get; set; }
        public Role? Role { get; set; } // Nullable to prevent overwriting with default
    }
}