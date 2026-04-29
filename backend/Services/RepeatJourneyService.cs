using AutoMapper;
using Backend.Dtos;
using Backend.Models;
using Backend.Repositories;

namespace Backend.Services;

public interface IRepeatJourneyService
{
    Task<IEnumerable<RepeatJourneyDto>?> GetAllByBikeIdAsync(Guid bikeId);
    Task<RepeatJourneyDto?> AddAsync(Guid bikeId, RepeatJourneyDto journey);
    Task<RepeatJourneyDto?> UpdateAsync(Guid id, RepeatJourneyDto journey);
    Task<bool> DeleteAsync(Guid id);
}

public class RepeatJourneyService(IMapper mapper, IRepeatJourneyRepository repeatJourneyRepository, IBikeRepository bikeRepository) : IRepeatJourneyService
{

    public async Task<IEnumerable<RepeatJourneyDto>?> GetAllByBikeIdAsync(Guid bikeId)
    {
        var repeatJourneys = await repeatJourneyRepository.GetAllByBikeIdAsync(bikeId);
        if (repeatJourneys == null)
        {
            return null;
        }

        return mapper.Map<List<RepeatJourneyDto>>(repeatJourneys);
    }


    public async Task<RepeatJourneyDto?> AddAsync(Guid bikeId, RepeatJourneyDto journey)
    {
        var bike = await bikeRepository.GetByIdAsync(bikeId);
        if (bike == null)
        {
            return null;
        }

        var createdRepeatJourney = mapper.Map<RepeatJourney>(journey);
        createdRepeatJourney.Bike = bike;

        repeatJourneyRepository.Add(createdRepeatJourney);
        await repeatJourneyRepository.SaveChangesAsync();

        return mapper.Map<RepeatJourneyDto>(createdRepeatJourney);
    }


    public async Task<RepeatJourneyDto?> UpdateAsync(Guid id, RepeatJourneyDto journey)
    {
        var existingJourney = await repeatJourneyRepository.GetByIdAsync(id);
        if (existingJourney == null)
        {
            return null;
        }

        var updatedRepeatJourney = mapper.Map<RepeatJourney>(journey);

        existingJourney.Title = updatedRepeatJourney.Title;
        existingJourney.Distance = updatedRepeatJourney.Distance;
        existingJourney.RepeatDays = updatedRepeatJourney.RepeatDays;
        existingJourney.RepeatType = updatedRepeatJourney.RepeatType;
        existingJourney.StartDate = updatedRepeatJourney.StartDate;
        existingJourney.EndDate = updatedRepeatJourney.EndDate;

        repeatJourneyRepository.Update(existingJourney);
        await repeatJourneyRepository.SaveChangesAsync();

        return mapper.Map<RepeatJourneyDto>(existingJourney);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var task = await repeatJourneyRepository.GetByIdAsync(id);
        if (task == null)
        {
            return false;
        }

        repeatJourneyRepository.Remove(task);
        await repeatJourneyRepository.SaveChangesAsync();

        return true;
    }
}