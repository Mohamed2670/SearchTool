using SearchTool_ServerSide.Dtos.OrderItemDtos;
using SearchTool_ServerSide.Models;

namespace SearchTool_ServerSide.Dtos.OrderDtos
{
    public class OrderHistoryReadDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalNet { get; set; }
        public decimal TotalPatientPay { get; set; }
        public decimal TotalInsurancePay { get; set; }
        public decimal TotalAcquisitionCost { get; set; }
        public decimal AdditionalCost { get; set; }
        public ICollection<OrderItemReadDto> OrderItemReadDtos { get; set; } = new List<OrderItemReadDto>();
        
    }
}