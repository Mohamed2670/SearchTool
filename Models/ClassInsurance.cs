namespace SearchTool_ServerSide.Models
{
    public class ClassInsurance
    {
        public int InsuranceId { get; set;}
        public int ClassId { get; set;}
        public string ClassName { get; set;}
        public string InsuranceName { get; set;}
        public decimal BestNet { get; set; } = 0;
        public int DrugId { get; set; }
        public DateTime Date { get; set; }
        public Drug Drug { get; set; }
        public DrugClass DrugClass { get; set; }
        public Insurance Insurance { get; set;}
    }
}