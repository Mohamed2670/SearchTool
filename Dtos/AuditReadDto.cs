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

        // 🧾 Insurance Info
        public int InsuranceId { get; set; }
        public string InsuranceRx { get; set; }
        public string BINCode { get; set; }
        public string BINName { get; set; }
        public string PCNName { get; set; }

        // 🧾 Linking IDs
        public int? RxGroupId { get; set; }
        public int? BinId { get; set; }
        public int? PcnId { get; set; }

        // 🧾 Highest Alternative Info
        public string HighestInsuranceRx { get; set; }
        public string HighestBINCode { get; set; }
        public string HighestBINName { get; set; }
        public string HighestPCNName { get; set; }

        // 🧾 Highest Alternative Linking IDs
        public int? HighestRxGroupId { get; set; }
        public int? HighestBinId { get; set; }
        public int? HighestPcnId { get; set; }

        public string PF { get; set; }

        public decimal Quantity { get; set; }
        public decimal HighestQuantity { get; set; }

        public decimal AcquisitionCost { get; set; }
        public decimal Discount { get; set; }
        public decimal InsurancePayment { get; set; }
        public decimal PatientPayment { get; set; }
        public decimal NetProfit { get; set; }
        public decimal NetProfitPerItem { get; set; }
        public decimal HighestNetProfitPerItem { get; set; }

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
