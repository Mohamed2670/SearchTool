namespace SearchTool_ServerSide.Models
{
    public class InsurancePCN
    {
        public int Id { get; set; }
        public required string PCN { get; set; }
        public int InsuranceId { get; set; }
        public Insurance? Insurance { get; set; }
        public ICollection<InsuranceRx> InsuranceRxs { get; set; }
    }
}