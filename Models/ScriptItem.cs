using ServerSide.Model;
using ServerSide.Models;

namespace SearchTool_ServerSide.Models
{
    public class ScriptItem : IEntity
    {
        public int Id { get; set; }
        public int ScriptId { get; set; }
        public Script Script { get; set; }

        // Foreign Keys
        public int DrugId { get; set; }
        public Drug Drug { get; set; }

        public string RxNumber { get; set; }
        public int InsuranceId { get; set; }
        public InsuranceRx Insurance { get; set; }

        public int DrugClassId { get; set; }
        public DrugClass DrugClass { get; set; }

        public string UserEmail { get; set; }
        public User Prescriber { get; set; }

        // Script Item Details
        public string PF { get; set; }
        public string Quantity { get; set; }
        public decimal AcquisitionCost { get; set; }
        public decimal Discount { get; set; }
        public decimal InsurancePayment { get; set; }
        public decimal PatientPayment { get; set; }
        public decimal NetProfit => PatientPayment + InsurancePayment - AcquisitionCost;
        public string NDCCode { get; set; }
    }
}