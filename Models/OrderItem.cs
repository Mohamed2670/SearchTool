using SearchTool_ServerSide.Dtos.SearchLogDtos;
using ServerSide.Model;

namespace SearchTool_ServerSide.Models
{
    public class OrderItem : IEntity
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string DrugName { get; set; }
        public string DrugNDC { get; set; }
        public decimal NetPrice { get; set; }
        public decimal PatientPay { get; set; }
        public decimal InsurancePay { get; set; }
        public decimal AcquisitionCost { get; set; }
        public decimal AddtionalCost { get; set; }
        public int? InsuranceRxId { get; set; }
        public int Amount { get; set; }
        public SearchLogReadDto SearchLogReadDto { get; set; } 
        public InsuranceRx? InsuranceRx { get; set; }
        // public Order Order { get; set; }
        public Drug Drug { get; set; }
    }
}