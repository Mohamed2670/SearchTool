using SearchTool_ServerSide.Dtos.OrderItemDtos;
using SearchTool_ServerSide.Dtos.SearchLogDtos;

namespace SearchTool_ServerSide.Dtos.OrderDtos
{
    public class CreateOrderRequestDto
    {
        public ICollection<OrderItemAddDto> OrderItems { get; set; }
        public ICollection<SearchLogAddDto> SearchLogs { get; set; }
    }
}