namespace SearchTool_ServerSide.Models
{
    public class DrugBranch
    {
        public int DrugId { get; set; }
        public int BranchId { get; set; }
        public Drug Drug { get; set; }
        public Branch Branch { get; set; }
        
    }
}