using ServerSide.Model;

namespace SearchTool_ServerSide.Models
{
    public class ClassInsurance : IEntity
    {
        public int InsuranceId { get; set; }
        public int ClassInfoId { get; set; }
        public DateTime Date { get; set; }
        public int BranchId { get; set; }

        public string InsuranceName { get; set; }
        public decimal BestNet { get; set; } = 0;
        public decimal BestACQ { get; set; } = 0;
        public decimal BestInsurancePayment { get; set; } = 0;
        public decimal BestPatientPayment { get; set; } = 0;
        public decimal Qty { get; set; } = 1;
        public int DrugId { get; set; }
        public string ScriptCode { get; set; }
        public DateTime ScriptDateTime { get; set; }
        public Branch Branch { get; set; }
        public Drug Drug { get; set; }
        public ClassInfo ClassInfo { get; set; }
        public InsuranceRx Insurance { get; set; }
        public int Id { get; set; }
    }
}