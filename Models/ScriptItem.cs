using ServerSide.Models;

namespace SearchTool_ServerSide.Models
{
    public class ScriptItem
    {
        public int Id { get; set; }
        public int ScriptId { get; set; }
        public Script Script { get; set; } // Navigation property
        public string DrugName { get; set; }
        public string Insurance { get; set; }
        public string PF { get; set; }
        public string Prescriber { get; set; }
        public decimal Quantity { get; set; }
        public decimal AcquisitionCost { get; set; }
        public decimal Discount { get; set; }
        public decimal InsurancePayment { get; set; }
        public decimal PatientPayment { get; set; }
        public string NDCCode { get; set; }
        public decimal NetProfit { get; set; }
        public string DrugClass { get; set; }
    }
}