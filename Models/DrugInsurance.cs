namespace SearchTool_ServerSide.Models
{
    public class DrugInsurance
    {
        public required int InsuranceId { get; set; }
        public required int DrugId { get; set; }
        public required string NDCCode { get; set; }
        public required string DrugName { get; set; }
        public int ClassId { get; set; }
        public string insuranceName { get; set; }
        public decimal Net { get; set; }
        public string date { get; set; }
        public string Prescriber { get; set; }
        public decimal Quantity { get; set; }
        public decimal AcquisitionCost { get; set; }
        public decimal? RxCui { get; set; }
        public decimal Discount { get; set; }
        public decimal InsurancePayment { get; set; }
        public decimal PatientPayment { get; set; }
        public string DrugClass { get; set; }
        public Insurance? Insurance { get; set; }
        public Drug? Drug { get; set; }
    }
}