namespace SearchTool_ServerSide.Dtos.DrugDtos
{
    public class DrugBestAlternativeReadDto
    {
        public int? ClassId { get; set; }
        public DateTime? Date { get; set; }
        public int? BranchId { get; set; }

        public string? ClassName { get; set; }
        public decimal? BestNet { get; set; } = 0;
        public int DrugId { get; set; }
        public string? ScriptCode { get; set; }
        public DateTime? ScriptDateTime { get; set; }

        public string? DrugName { get; set; }
        public string? DrugClass { get; set; }
        public string? BranchName { get; set; }
        public string? NDC { get; set; }
        public int BinId { get; set; }
        public int PcnId { get; set; }
        public int RxGroupId { get; set; }
        public string BinFullName { get; set; }
        public string Bin { get; set; }
        public string Pcn { get; set; }
        public string? Rxgroup { get; set; }

    }
}