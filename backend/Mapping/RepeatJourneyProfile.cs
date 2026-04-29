using AutoMapper;
using Backend.Dtos;
using Backend.Models;

namespace Backend.Mapping;

public class RepeatJourneyProfile : Profile
{
    public RepeatJourneyProfile()
    {
        CreateMap<RepeatJourney, RepeatJourneyDto>()
            .ForMember(dto => dto.BikeId, opt => opt.MapFrom(model => model.Bike.Id))
            .ForMember(dto => dto.RepeatDays, opt => opt.ConvertUsing(new MaskToWeekdayCodesConverter(), model => model.RepeatDays));

        CreateMap<RepeatJourneyDto, RepeatJourney>()
            .ForMember(model => model.Bike, opt => opt.Ignore())
            .ForMember(model => model.RepeatDays, opt => opt.ConvertUsing(new WeekdayCodesToMaskConverter(), dto => dto.RepeatDays));

    }
}
