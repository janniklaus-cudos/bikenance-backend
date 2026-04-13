using AutoMapper;
using Backend.Dtos;
using Backend.Models;

namespace Backend.Mapping;

public class BikePartProfile : Profile
{
    public BikePartProfile()
    {
        CreateMap<BikePart, BikePartDto>()
            .ForMember(dto => dto.BikeId, opt => opt.MapFrom(model => model.Bike.Id));

        CreateMap<BikePartDto, BikePart>()
            .ForMember(model => model.Bike, opt => opt.Ignore());
    }
}