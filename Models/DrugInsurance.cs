namespace SearchTool_ServerSide.Models
{
    public class DrugInsurance
    {
        public int Id { get; set; }
        public required int InsuranceId { get; set; }
        public required int DrugId { get; set; }
        public required string NDCCode{ get; set; }
        public Insurance? Insurance { get; set; }
        public Drug? Drug { get; set; }
    }
}