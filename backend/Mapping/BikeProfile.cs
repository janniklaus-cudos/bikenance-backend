using AutoMapper;
using Backend.Dtos;
using Backend.Models;

namespace Backend.Mapping;

public class BikeProfile : Profile
{
	public BikeProfile()
	{
		CreateMap<Bike, BikeDto>()
			.ForMember(dto => dto.Parts, opt => opt.MapFrom(model => model.Parts));

		CreateMap<BikeDto, Bike>()
			.ForMember(model => model.Parts, opt => opt.Ignore());
	}
}