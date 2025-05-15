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
        public int binId { get; set; }
        public int pcnId { get; set; }
        public int rxgroupId { get; set; }
        public string bin { get; set; }
        public string pcn { get; set; }
        public string rxgroup { get; set; }
        public string BinFullName { get; set; }
        public string Route { get; set; }
        public string Form { get; set; }
        public string Strength { get; set; }
        public string TECode { get; set; }
        public string ApplicationNumber { get; set; }
        public string ApplicationType { get; set; }
        public string? Ingrdient { get; set; }
        public string? StrengthUnit { get; set; }
        public string? Type { get; set; }

    }
}
