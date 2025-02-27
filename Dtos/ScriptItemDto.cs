namespace SearchTool_ServerSide.Dtos
{
    public class ScriptItemDto
    {
        public int Id { get; set; }
        // Foreign Keys
        public string DrugName { get; set; }

        public string InsuranceName { get; set; }

        public string DrugClassName { get; set; }

        public string PrescriberName { get; set; }
        public string UserName { get; set; }

        // Script Item Details
        public string PF { get; set; }
        public decimal Quantity { get; set; }
        public decimal AcquisitionCost { get; set; }
        public decimal Discount { get; set; }
        public decimal InsurancePayment { get; set; }
        public decimal PatientPayment { get; set; }
        public decimal NetProfit => PatientPayment + InsurancePayment - AcquisitionCost;
        public string NDCCode { get; set; }
        public string BranchName { get; set; }
    }
}