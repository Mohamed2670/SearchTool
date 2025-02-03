using SearchTool_ServerSide.Models;

namespace ServerSide.Models
{
    public class Script
    {
        public int Id { get; set; }
        public required string ScriptCode { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public decimal TotalPrice { get; set; }
        public required int UserId { get; set; } = 1;
        public User? User { get; set; }
        public decimal InsurancePay { get; set; }
        public decimal Net {get; set; }
        public decimal Discount { get; set; }
        public decimal PatientPay { get; set; }
        public decimal Quantity { get; set; }
        public required string NDCCode { get; set; }
        public int DrugInsuranceId { get; set; }
        public DrugInsurance? DrugInsurance{ get; set; }
        
        
    }
}