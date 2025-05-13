using AutoMapper;
using SearchTool_ServerSide.Dtos.OrderDtos;
using SearchTool_ServerSide.Dtos.OrderItemDtos;
using SearchTool_ServerSide.Dtos.SearchLogDtos;
using SearchTool_ServerSide.Models;

namespace SearchTool_ServerSide.Profiles
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<Order, OrderAddDto>().ReverseMap();
            CreateMap<OrderItem,OrderItemReadDto>().ReverseMap();
            CreateMap<Order, OrderHistoryReadDto>()
                .ForMember(dest => dest.OrderItemReadDtos, opt => opt.MapFrom(src => src.OrderItems));
            CreateMap<SearchLog, SearchLogReadDto>().ReverseMap();
        }

    }
}