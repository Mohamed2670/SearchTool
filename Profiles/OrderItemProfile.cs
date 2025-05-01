using AutoMapper;
using SearchTool_ServerSide.Dtos.OrderItemDtos;
using SearchTool_ServerSide.Models;

namespace SearchTool_ServerSide.Profiles
{
    public class OrderItemProfile : Profile
    {
        public OrderItemProfile()
        {
            CreateMap<OrderItem, OrderItemAddDto>().ReverseMap();
        }

    }
}