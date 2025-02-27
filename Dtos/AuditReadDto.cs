namespace SearchTool_ServerSide.Dtos
{
    public class AuditReadDto
    {
        public DateTime Date { get; set; }
        public string ScriptCode { get; set; }
        public string RxNumber { get; set; }
        public string? User { get; set; }
        public string DrugName { get; set; }
        public int DrugId { get; set; }
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
        public string HighstDrugNDC { get; set; }
        public string HighstDrugName { get; set; }
        public int HighstDrugId { get; set; }
        public decimal HighstNet { get; set; }
        public string HighstScriptCode { get; set; }
        public string BranchCode { get; set; }
        public DateTime HighstScriptDate { get; set; }

    }
}