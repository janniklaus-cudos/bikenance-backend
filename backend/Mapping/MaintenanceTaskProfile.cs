using AutoMapper;
using Backend.Dtos;
using Backend.Models;

namespace Backend.Mapping;

public class MaintenanceTaskProfile : Profile
{
    public MaintenanceTaskProfile()
    {
        CreateMap<MaintenanceTask, MaintenanceTaskDto>()
            .ForMember(dto => dto.BikePartId, opt => opt.MapFrom(model => model.BikePart.Id));

        CreateMap<MaintenanceTaskDto, MaintenanceTask>()
            .ForMember(model => model.BikePart, opt => opt.Ignore());
    }
}
