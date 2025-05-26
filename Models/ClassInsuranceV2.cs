using ServerSide.Model;

namespace SearchTool_ServerSide.Models
{
    public class ClassInsuranceV2 : IEntity 
    {
        public int InsuranceId { get; set; }
        public int ClassV2Id { get; set; }
        public DateTime Date { get; set; }
        public int BranchId { get; set;}
        
        public string ClassName { get; set; }
        public string InsuranceName { get; set; }
        public decimal BestNet { get; set; } = 0;
        public int DrugId { get; set; }
        public string ScriptCode { get; set; }
        public DateTime ScriptDateTime { get; set; }
        public Branch Branch { get; set; }
        public Drug Drug { get; set; }
        public DrugClassV2 DrugClassV2 { get; set; }
        public InsuranceRx Insurance { get; set; }
        public int Id { get; set; }
    }
}