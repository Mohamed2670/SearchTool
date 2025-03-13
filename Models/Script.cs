using SearchTool_ServerSide.Models;
using ServerSide.Model;

namespace ServerSide.Models
{
    public class Script : IEntity
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string ScriptCode { get; set; }
        // Foreign Keys
        public int? UserId{ get; set; }
        public User? User { get; set; }
        public int BranchId { get; set; }
        public Branch Branch { get; set; }

        // Navigation Property
        public List<ScriptItem> ScriptItems { get; set; } = new();
    }
}
