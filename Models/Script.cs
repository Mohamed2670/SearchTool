using SearchTool_ServerSide.Models;

namespace ServerSide.Models
{
    public class Script
    {
        public int Id { get; set; }
        public string Date { get; set; }
        public string ScriptCode { get; set; }
        public string RxNumber { get; set; }
        public string DrugName { get; set; }
        public string Insurance { get; set; }
        public string Prescriber { get; set; }
        public decimal Quantity { get; set; }
        public decimal AcquisitionCost { get; set; }
        public string NDCCode { get; set; }
        public decimal RxCui { get; set; }
        public decimal Discount { get; set; }
        public decimal InsurancePayment { get; set; }
        public decimal PatientPayment { get; set; }
        public string DrugClass { get; set; }
        public decimal NetProfit { get; set; }


    }
}