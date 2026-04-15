using AutoMapper;
using Backend.Dtos;
using Backend.Models;

namespace Backend.Mapping;

public class ServiceEventProfile : Profile
{
    public ServiceEventProfile()
    {
        CreateMap<ServiceEvent, ServiceEventDto>()
            .ForMember(dto => dto.BikePartId, opt => opt.MapFrom(model => model.BikePart.Id));

        CreateMap<ServiceEventDto, ServiceEvent>()
            .ForMember(model => model.BikePart, opt => opt.Ignore());
    }
}
