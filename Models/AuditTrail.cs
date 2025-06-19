using ServerSide.Model;

namespace SearchTool_ServerSide.Models
{
    public class AuditTrail : IEntity
    {
        public int Id { get; set; }
        public string TableName { get; set; }
        public string ActionType { get; set; } // Insert, Update, Delete
        public string PrimaryKey { get; set; }
        public string OldValues { get; set; }
        public string NewValues { get; set; }
        public string PerformedBy { get; set; }
        public DateTime Timestamp { get; set; }
    }

}