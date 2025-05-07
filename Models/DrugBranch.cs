using ServerSide.Model;

namespace SearchTool_ServerSide.Models
{
    public class DrugBranch : IEntity
    {
        public int DrugId { get; set; }
        public int BranchId { get; set; }
        public Drug Drug { get; set; }
        public Branch Branch { get; set; }
        public int Id { get; set; }
    }
}