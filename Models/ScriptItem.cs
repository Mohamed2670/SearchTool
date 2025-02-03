using ServerSide.Models;

namespace SearchTool_ServerSide.Models
{
    public class ScriptItem
    {
        public int Id { get; set; }
        public decimal InsurancePay { get; set; }
        public decimal Net {get; set; }
        public decimal Discount { get; set; }
        public decimal PatientPay { get; set; }
        public int Quantity { get; set; }
        public required string NDCCode { get; set; }
        public int DrugInsuranceId { get; set; }
        public DrugInsurance? DrugInsurance{ get; set; }
        public int ScriptId { get; set; }
        public Script? Script { get; set; }
    }
}