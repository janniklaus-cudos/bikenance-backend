using AutoMapper;
using Backend.Dtos;
using Backend.Models;
using Backend.Repositories;

namespace Backend.Services;

public interface IJourneyService
{
    Task<JourneyDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<JourneyDto>?> GetAllByBikeIdAsync(Guid bikeId);
    Task<IEnumerable<RepeatJourneyDto>?> GetAllRepeatByBikeIdAsync(Guid bikeId);
    Task<JourneyDto?> AddAsync(Guid bikeId, JourneyDto journey);
    Task<IEnumerable<JourneyDto>?> AddAllAsync(Guid bikeId, IEnumerable<JourneyDto> journeys);
    Task<JourneyDto?> UpdateAsync(Guid id, JourneyDto journey);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> DeleteAllByBikeIdAsync(Guid bikeId);
}

public class JourneyService(IMapper mapper, IJourneyRepository journeyRepository, IBikeRepository bikeRepository, IRepeatJourneyRepository repeatJourneyRepository) : IJourneyService
{
    public async Task<JourneyDto?> GetByIdAsync(Guid id)
    {
        var journey = await journeyRepository.GetByIdAsync(id);
        if (journey == null)
        {
            return null;
        }

        return mapper.Map<JourneyDto>(journey);
    }

    public async Task<IEnumerable<JourneyDto>?> GetAllByBikeIdAsync(Guid bikeId)
    {
        var journeys = await journeyRepository.GetAllByBikeIdAsync(bikeId);
        if (journeys == null)
        {
            return null;
        }

        return mapper.Map<List<JourneyDto>>(journeys);
    }

    public async Task<IEnumerable<RepeatJourneyDto>?> GetAllRepeatByBikeIdAsync(Guid bikeId)
    {
        var repeatJourneys = await repeatJourneyRepository.GetAllByBikeIdAsync(bikeId);
        if (repeatJourneys == null)
        {
            return null;
        }

        return mapper.Map<List<RepeatJourneyDto>>(repeatJourneys);
    }


    public async Task<JourneyDto?> AddAsync(Guid bikeId, JourneyDto journey)
    {
        var bike = await bikeRepository.GetByIdAsync(bikeId);
        if (bike == null)
        {
            return null;
        }

        var mappedJourney = mapper.Map<Journey>(journey);
        var createdJourney = new Journey
        {
            Bike = bike,
            Title = mappedJourney.Title,
            Distance = mappedJourney.Distance,
            JourneyDate = mappedJourney.JourneyDate,
        };

        journeyRepository.Add(createdJourney);
        await journeyRepository.SaveChangesAsync();

        return mapper.Map<JourneyDto>(createdJourney);
    }

    public async Task<IEnumerable<JourneyDto>?> AddAllAsync(Guid bikeId, IEnumerable<JourneyDto> journeys)
    {
        var bike = await bikeRepository.GetByIdAsync(bikeId);
        if (bike == null)
        {
            return null;
        }

        var createdJourneys = mapper.Map<List<Journey>>(journeys);
        createdJourneys.ForEach(journey =>
        {
            var createdJourney = new Journey
            {
                Bike = bike,
                Title = journey.Title,
                Distance = journey.Distance,
                JourneyDate = journey.JourneyDate,
            };
            journeyRepository.Add(createdJourney);
        });

        journeyRepository.AddRange(createdJourneys);
        await journeyRepository.SaveChangesAsync();

        return mapper.Map<List<JourneyDto>>(createdJourneys);
    }

    public async Task<JourneyDto?> UpdateAsync(Guid id, JourneyDto journey)
    {
        var existingJourney = await journeyRepository.GetByIdAsync(id);
        if (existingJourney == null)
        {
            return null;
        }

        existingJourney.Title = journey.Title;
        existingJourney.Distance = journey.Distance;
        existingJourney.JourneyDate = journey.JourneyDate;
        existingJourney.ExternalId = journey.ExternalId;
        existingJourney.IsConnectedToRepeatJourney = journey.IsConnectedToRepeatJourney;

        journeyRepository.Update(existingJourney);
        await journeyRepository.SaveChangesAsync();

        return mapper.Map<JourneyDto>(existingJourney);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var task = await journeyRepository.GetByIdAsync(id);
        if (task == null)
        {
            return false;
        }

        journeyRepository.Remove(task);
        await journeyRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAllByBikeIdAsync(Guid bikeId)
    {
        var bike = await bikeRepository.GetByIdAsync(bikeId);
        if (bike == null)
        {
            return false;
        }

        var journeys = await journeyRepository.GetAllByBikeIdAsync(bikeId);
        if (journeys.Count == 0)
        {
            return true;
        }

        journeyRepository.RemoveRange(journeys);
        await journeyRepository.SaveChangesAsync();

        return true;
    }
}