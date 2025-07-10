namespace SearchTool_ServerSide.Dtos.DrugDtos
{
    public class DrugInsuranceReadDto
    {
        public required int InsuranceId { get; set; }
        public required int DrugId { get; set; }
        public required int BranchId { get; set; }

        public required string NDCCode { get; set; }
        public int DrugClassId { get; set; }
        public string  DrugClass { get; set; }
        public decimal Net { get; set; }
        public DateTime date { get; set; }
        public string Prescriber { get; set; }
        public decimal Quantity { get; set; }
        public decimal AcquisitionCost { get; set; }
        public decimal Discount { get; set; }
        public decimal InsurancePayment { get; set; }
        public decimal PatientPayment { get; set; }
        public string? Insurance { get; set; }
        public string? Drug { get; set; }
        public string? Branch { get; set; }
        public int Id { get; set; }
    }
}