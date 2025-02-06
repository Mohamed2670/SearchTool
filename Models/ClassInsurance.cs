namespace SearchTool_ServerSide.Models
{
    public class ClassInsurance
    {
        public int InsuranceId { get; set;}
        public int ClassId { get; set;}
        public string ClassName { get; set;}
        public string InsuranceName { get; set;}
        public decimal BestNet { get; set; } = 0;
        public DrugClass DrugClass { get; set;}
        public Insurance Insurance { get; set;}
    }
}