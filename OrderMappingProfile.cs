using AutoMapper;
using PaczkomatDatabaseAPI.Models;

namespace PaczkomatDatabaseAPI
{
    public class OrderMappingProfile : Profile
    {
        public OrderMappingProfile() 
        {
            CreateMap<CreateOrderDto, Order>()
                .ForMember(order => order.Id, opt => opt.MapFrom((src, order, destMember, context) => context.Items["Id"]))
                .ForMember(order => order.CodeInserting, opt => opt.MapFrom((src, order, destMember, context) => context.Items["CodeInserting"]))
                .ForMember(order => order.OrderDate, opt => opt.MapFrom((src, order, destMember, context) => context.Items["OrderDate"]))
                .ForMember(order => order.Status, opt => opt.MapFrom((src, order, destMember, context) => context.Items["Status"]));

    }
    }
}
