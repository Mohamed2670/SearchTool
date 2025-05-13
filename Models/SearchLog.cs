using ServerSide.Model;

namespace SearchTool_ServerSide.Models
{
    public class SearchLog : IEntity
    {
        public int Id { get; set; } 

        public int? RxgroupId { get; set; }
        public int? BinId { get; set; }
        public int? PcnId { get; set; }
        public int DrugId { get; set; }
        public int UserId { get; set; }
        public int OrderItemId { get; set; }
        // public OrderItem OrderItem { get; set; }
        public User User { get; set; }
        public Insurance? Insurance { get; set; }
        public Drug Drug { get; set; }
        public InsuranceRx? InsuranceRx { get; set; }
        public InsurancePCN? InsurancePCN { get; set; }
        public DateTime Date { get; set; } 
        public string SearchType { get; set; }
    }
}