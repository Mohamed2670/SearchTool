namespace SearchTool_ServerSide.Models
{
    public class InsuranceStatus
    {
        public string SourceDrugNDC { get; set; }
        public string TargetDrugNDC { get; set; }
        public Drug SourceDrug { get; set; }
        public Drug TargetDrug { get; set; }
        public int InsuranceRxId { get; set; }
        public InsuranceRx InsuranceRx { get; set; }
        public ICollection<Report> Reports { get; set; } = new List<Report>();
        public string ApprovedStatus { get; set; } = "NA";
        public string PriorAuthorizationStatus { get; set; } = "NA";
    }


    public class Report
    {
        public int Id { get; set; }  // simple PK for the history row

        // Composite FK back to InsuranceStatus
        public string SourceDrugNDC { get; set; }
        public string TargetDrugNDC { get; set; }
        public int InsuranceRxId { get; set; }
        public InsuranceStatus InsuranceStatus { get; set; }

        public string Status { get; set; } = "Approved";
        public string StatusDescription { get; set; } = "The drug is approved for use.";
        public string AdditionalInfo { get; set; } = "No additional information available.";
        public DateTime StatusDate { get; set; } = DateTime.UtcNow;

        public string UserEmail { get; set; }
        public User User { get; set; }
    }


}