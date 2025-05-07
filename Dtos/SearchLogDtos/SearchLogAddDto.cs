using SearchTool_ServerSide.Models;

namespace SearchTool_ServerSide.Dtos.SearchLogDtos
{
    public class SearchLogAddDto
    {
        public int RxgroupId { get; set; }
        public int BinId { get; set; }
        public int PcnId { get; set; }
        public int DrugId { get; set; }
        public int UserId { get; set; }

        public DateTime Date { get; set; }
        public string SearchType { get; set; }
    }

}