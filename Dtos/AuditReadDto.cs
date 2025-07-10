namespace SearchTool_ServerSide.Dtos
{
    public class AuditReadDto
    {
        public DateTime Date { get; set; }

        public int RemainingStock { get; set; } = 100;
        public int HighestRemainingStock { get; set; } = 100;

        public string ScriptCode { get; set; }
        public string RxNumber { get; set; }

        public string? User { get; set; }
        public string Prescriber { get; set; }

        public string DrugName { get; set; }
        public int DrugId { get; set; }

        public string Insurance { get; set; }
        public int InsuranceId { get; set; }

        public string PF { get; set; }

        public decimal Quantity { get; set; }
        public decimal AcquisitionCost { get; set; }
        public decimal Discount { get; set; }
        public decimal InsurancePayment { get; set; }
        public decimal PatientPayment { get; set; }
        public decimal NetProfit { get; set; }

        public string NDCCode { get; set; }
        public string DrugClass { get; set; }

        public string HighestDrugNDC { get; set; }
        public string HighestDrugName { get; set; }
        public int HighestDrugId { get; set; }
        public decimal HighestNet { get; set; }
        public string HighestScriptCode { get; set; }
        public DateTime HighestScriptDate { get; set; }

        public string BranchCode { get; set; }
    }
}
