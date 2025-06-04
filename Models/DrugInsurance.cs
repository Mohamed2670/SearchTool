using ServerSide.Model;

namespace SearchTool_ServerSide.Models
{
    public class DrugInsurance : IEntity
    {
        public required int InsuranceId { get; set; }
        public required int DrugId { get; set; }
        public required int BranchId { get; set; }

        public required string NDCCode { get; set; }
        public int DrugClassId { get; set; }
        public int DrugClassV2Id { get; set; }

        public int DrugClassV3Id { get; set; }
        public string? ScriptCode { get; set; }

        public decimal Net { get; set; }
        public DateTime date { get; set; }
        public string Prescriber { get; set; }
        public string Quantity { get; set; }
        public decimal AcquisitionCost { get; set; }
        public decimal Discount { get; set; }
        public decimal InsurancePayment { get; set; }
        public decimal PatientPayment { get; set; }
        public InsuranceRx? Insurance { get; set; }
        public Drug? Drug { get; set; }
        public Branch? Branch { get; set; }
        public int Id { get; set; }
    }
}