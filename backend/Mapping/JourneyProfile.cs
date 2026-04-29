using AutoMapper;
using Backend.Dtos;
using Backend.Integrations.Strava.Dtos;
using Backend.Models;

namespace Backend.Mapping;

public class JourneyProfile : Profile
{
    public JourneyProfile()
    {
        CreateMap<Journey, JourneyDto>()
            .ForMember(dto => dto.BikeId, opt => opt.MapFrom(model => model.Bike.Id))
            .ForMember(dto => dto.RepeatJourneyId, opt =>
            {
                opt.AllowNull();
                opt.MapFrom<Guid?>(model => model.RepeatJourney != null ? model.RepeatJourney.Id : null);
            });

        CreateMap<JourneyDto, Journey>()
            .ForMember(model => model.Bike, opt => opt.Ignore())
            .ForMember(model => model.RepeatJourney, opt => opt.Ignore());

        CreateMap<StravaActivity, JourneyDto>()
            .ForMember(journey => journey.Id, opt => opt.Ignore())
            .ForMember(journey => journey.BikeId, opt => opt.Ignore())
            .ForMember(journey => journey.RepeatJourneyId, opt => opt.Ignore())
            .ForMember(journey => journey.IsConnectedToRepeatJourney, opt => opt.Ignore())
            .ForMember(journey => journey.CreatedAtUtc, opt => opt.Ignore())
            .ForMember(journey => journey.ExternalId, opt => opt.MapFrom(activity => activity.id))
            .ForMember(journey => journey.Title, opt => opt.MapFrom(activity => activity.name))
            .ForMember(journey => journey.Distance, opt => opt.MapFrom(activity => Math.Round(activity.distance / 1000, 0, MidpointRounding.AwayFromZero)))
            .ForMember(journey => journey.JourneyDate, opt => opt.MapFrom(activity => DateOnly.FromDateTime(activity.start_date)));


    }
}
