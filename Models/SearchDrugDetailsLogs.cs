using ServerSide.Model;

namespace SearchTool_ServerSide.Models
{
    public class SearchDrugDetailsLogs : IEntity
    {
        public int Id { get; set; }
        public string NDC { get; set; }
        public string UserEmail { get; set; }
        public string Action { get; set; } // like user use search by *** and use This fields with this values
        public User User { get; set; }
        public Drug Drug { get; set; }
    }
}