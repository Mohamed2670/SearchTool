namespace SearchTool_ServerSide.Models
{
    public class InsuranceRx
    {
        public int Id { get; set; }
        public required string RxGroup { get; set; }
        public int InsurancePCNId { get; set; }
        public InsurancePCN? InsurancePCN { get; set; }

    }
}