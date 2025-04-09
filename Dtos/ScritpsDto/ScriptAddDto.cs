namespace SearchTool_ServerSide.Dtos.ScritpsDto
{
    public class ScriptAddDto
    {
        public string Date { get; set; }
        public string Script { get; set; }
        public string RxGroup { get; set; }
        public string Bin { get; set; }
        public string PCN { get; set; }
        public string RxNumber { get; set; }
        public string? User { get; set; }
        public string DrugName { get; set; }
        public string Insurance { get; set; }
        public string PF { get; set; }
        public string Prescriber { get; set; }
        public string Quantity { get; set; }
        public decimal AcquisitionCost { get; set; }
        public decimal Discount { get; set; }
        public decimal InsurancePayment { get; set; }
        public decimal PatientPayment { get; set; }
        public string Branch { get; set; }
        public string NDCCode { get; set; }
        

    }
  
}