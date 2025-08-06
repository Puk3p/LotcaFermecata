using AutoMapper;
using SP25.Business.ModelDTOs;
using SP25.Domain.Models;

namespace SP25.Business.ModelMapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<TestModel, TestModelDto>().ReverseMap();
            CreateMap<Order, OrderDto>().ReverseMap();
            CreateMap<OrderItem, OrderItemDto>().ReverseMap();

        }
    }
}
