using AutoMapper;
using OrderProto;
using OrderServiceApp.Domain;
using shared.Events;

namespace OrderServiceApp.Mappers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Request -> Domain
            CreateMap<CreateOrderRequest, Order>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => OrderStatus.Pending));

            // Domain -> Response
            CreateMap<Order, CreateOrderResponse>()
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<Order, GetOrderResponse>()
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            // Domain -> Event
            CreateMap<Order, OrderCreatedEvent>()
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.Id));
        }
    }
}
