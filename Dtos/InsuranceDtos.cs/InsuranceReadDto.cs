namespace SearchTool_ServerSide.Dtos.InsuranceDtos.cs
{
    public class InsuranceReadDto
    {
        public string? RxGroup { get; set; }
        public string InsurancePCN { get; set; }
        public string InsuranceBin { get; set; }
        public string InsuranceFullName { get; set; }
        public string HelpDeskNumber { get; set; }
    }
}