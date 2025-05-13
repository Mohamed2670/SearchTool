using SearchTool_ServerSide.Dtos.SearchLogDtos;

namespace SearchTool_ServerSide.Dtos.OrderItemDtos
{
    public class OrderItemReadDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string DrugName { get; set; }
        public int DrugId { get; set; }
        public string NDC { get; set; }
        public decimal NetPrice { get; set; }
        public decimal PatientPay { get; set; }
        public decimal InsurancePay { get; set; }
        public decimal AcquisitionCost { get; set; }
        public decimal AddtionalCost { get; set; }
        public int? InsuranceRxId { get; set; }
        public string InsuranceRxName { get; set; }
        public int Amount { get; set; }
        public SearchLogReadDto? SearchLogReadDto { get; set; }
    }
}