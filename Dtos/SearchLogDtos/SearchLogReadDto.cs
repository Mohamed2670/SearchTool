namespace SearchTool_ServerSide.Dtos.SearchLogDtos
{
    public class SearchLogReadDto
    {
        public int Id { get; set; }
        public int? RxgroupId { get; set; }
        public string RxgroupName { get; set; }
        public string BinName { get; set; }
        public string PcnName { get; set; }
        public int? BinId { get; set; }
        public int? PcnId { get; set; }

        public int DrugId { get; set; }
        public string DrugName { get; set; }
        public string NDC { get; set; }
        public int UserId { get; set; }
        public int OrderItemId { get; set; }
        public DateTime Date { get; set; }
        public string SearchType { get; set; }
    }
}