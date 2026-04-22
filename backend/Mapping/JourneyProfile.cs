using AutoMapper;
using Backend.Dtos;
using Backend.Models;

namespace Backend.Mapping;

public class JourneyProfile : Profile
{
    public JourneyProfile()
    {
        CreateMap<Journey, JourneyDto>()
            .ForMember(dto => dto.BikeId, opt => opt.MapFrom(model => model.Bike.Id));

        CreateMap<JourneyDto, Journey>()
            .ForMember(model => model.Bike, opt => opt.Ignore());
    }
}
