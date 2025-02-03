namespace SearchTool_ServerSide.Dtos.DrugDtos
{
    public class ScriptAddDto
    {
        public string ScriptCode { get; set; }
        public string Date { get; set; }
        public decimal TotalPrice { get; set; }
        public int UserId { get; set; }
        public decimal InsurancePay { get; set; }
        public decimal Net { get; set; }
        public decimal Discount { get; set; }
        public decimal PatientPay { get; set; }
        public decimal Quantity { get; set; }
        public string NDCCode { get; set; }
        public int DrugInsuranceId { get; set; }
    }
}