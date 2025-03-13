using ServerSide.Model;

namespace SearchTool_ServerSide.Models
{
    public class Insurance : IEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public required string Bin { get; set; }
        public string? HelpDeskNumber { get; set; }
        public ICollection<InsurancePCN> InsurancePCNs { get; set; }
    }

}