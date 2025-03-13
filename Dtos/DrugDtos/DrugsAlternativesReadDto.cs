namespace SearchTool_ServerSide.Dtos.DrugDtos
{
    public class DrugsAlternativesReadDto
    {
        public required int InsuranceId { get; set; }
        public required int DrugId { get; set; }
        public required int BranchId { get; set; }
        public required string NDCCode { get; set; }
        public required string DrugName { get; set; }
        public int DrugClassId { get; set; }
        public string insuranceName { get; set; }
        public decimal Net { get; set; }
        public DateTime date { get; set; }
        public string Prescriber { get; set; }
        public string Quantity { get; set; }
        public decimal AcquisitionCost { get; set; }
        public decimal Discount { get; set; }
        public decimal InsurancePayment { get; set; }
        public decimal PatientPayment { get; set; }
        public string DrugClass { get; set; }
        public string branchName { get; set; }
        
        // Added missing properties
        public string bin { get; set; }
        public string pcn { get; set; }
        public string rxgroup { get; set; }
    }
}
