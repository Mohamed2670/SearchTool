using ServerSide.Model;

namespace SearchTool_ServerSide.Models
{
    public class DrugClass : IEntity
    {
        public int DrugId { get; set; }
        public int ClassId { get; set; }
        public Drug Drug { get; set; }
        public ClassInfo ClassInfo { get; set; } 
        public int Id { get; set; }
    }
}