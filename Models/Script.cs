using SearchTool_ServerSide.Models;

namespace ServerSide.Models
{
    public class Script
    {
        public int Id { get; set; }
        public required string ScriptCode { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public decimal TotalPrice { get; set; }
        public required int UserId { get; set; }
        public User? User { get; set; }
        
        
    }
}