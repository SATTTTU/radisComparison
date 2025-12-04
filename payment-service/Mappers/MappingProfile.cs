using AutoMapper;
using shared.Events;

namespace payment_service.Mappers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<OrderCreatedEvent, PaymentCompletedEvent>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Success"));
        }
    }
}
