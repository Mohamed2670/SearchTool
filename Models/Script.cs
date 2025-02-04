using SearchTool_ServerSide.Models;

namespace ServerSide.Models
{
    public class Script
    {
       public int Id { get; set; }
        public string? Date { get; set; }
        public string? ScriptCode { get; set; }
        public string? RxNumber { get; set; }
        public string? DrugName { get; set; }
        public string? Insurance { get; set; }
        public string? Prescriber { get; set; }
        public decimal Quantity { get; set; }
        public decimal AcquisitionCost { get; set; }
        public string? NDCCode { get; set; }
        public int RxCui { get; set; }
        
        
    }
}