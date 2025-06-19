using ServerSide.Model;

namespace SearchTool_ServerSide.Models
{
    public class DrugBranch : IEntity
    {
        public string DrugNDC { get; set; }
        public int BranchId { get; set; }
        public int? Stock { get; set; } = 100;
        public Drug Drug { get; set; }
        public Branch Branch { get; set; }
        public int Id { get; set; }
    }
}