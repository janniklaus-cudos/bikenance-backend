using AutoMapper;
using Backend.Dtos;
using Backend.Models;
using Backend.Repositories;

namespace Backend.Services;

public class JourneyService(IMapper mapper, IJourneyRepository journeyRepository, IBikeRepository bikeRepository) : IJourneyService
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

    public async Task<JourneyDto?> AddAsync(Guid bikeId, JourneyDto journey)
    {
        var bike = await bikeRepository.GetByIdAsync(bikeId);
        if (bike == null)
        {
            return null;
        }

        var createdJourney = mapper.Map<Journey>(journey);
        createdJourney.Bike = bike;

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
        createdJourneys.ForEach(journey => journey.Bike = bike);

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