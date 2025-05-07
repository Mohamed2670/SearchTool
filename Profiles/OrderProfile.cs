using AutoMapper;
using SearchTool_ServerSide.Dtos.OrderDtos;
using SearchTool_ServerSide.Models;

namespace SearchTool_ServerSide.Profiles
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<Order, OrderAddDto>().ReverseMap();
        }

    }
}