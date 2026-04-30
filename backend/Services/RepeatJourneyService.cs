using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Backend.Dtos;
using Backend.Mapping;
using Backend.Models;
using Backend.Repositories;
using Microsoft.AspNetCore.Authentication;

namespace Backend.Services;

public interface IRepeatJourneyService
{
    Task<IEnumerable<RepeatJourneyDto>> GetAllByBikeIdAsync(Guid bikeId);
    Task<RepeatJourneyDto> AddAsync(Guid bikeId, RepeatJourneyDto journey);
    Task<RepeatJourneyDto> UpdateAsync(Guid id, RepeatJourneyDto journey);
    Task DeleteAsync(Guid id);
    Task GenerateAllForToday();

}

public class RepeatJourneyService(IMapper mapper, IRepeatJourneyRepository repeatJourneyRepository, IBikeRepository bikeRepository, IJourneyRepository journeyRepository) : IRepeatJourneyService
{

    const int MONTH_INTERVAL_WEEK_COUNT = 4;

    public async Task<IEnumerable<RepeatJourneyDto>> GetAllByBikeIdAsync(Guid bikeId)
    {
        var repeatJourneys = await repeatJourneyRepository.GetAllByBikeIdAsync(bikeId) ?? throw new ValidationException("bike id was not found");

        return mapper.Map<List<RepeatJourneyDto>>(repeatJourneys);
    }


    public async Task<RepeatJourneyDto> AddAsync(Guid bikeId, RepeatJourneyDto journey)
    {
        var bike = await bikeRepository.GetByIdAsync(bikeId) ?? throw new ValidationException("bikeId was not found");
        if (journey.StartDate.CompareTo(journey.EndDate) >= 0) throw new ValidationException("start date needs to be smaller than end date");

        var mappedJourney = mapper.Map<RepeatJourney>(journey);
        var createdRepeatJourney = new RepeatJourney
        {
            Bike = bike,
            Title = mappedJourney.Title,
            Distance = mappedJourney.Distance,
            RepeatDays = mappedJourney.RepeatDays,
            RepeatType = mappedJourney.RepeatType,
            StartDate = mappedJourney.StartDate,
            EndDate = mappedJourney.EndDate,
        };

        repeatJourneyRepository.Add(createdRepeatJourney);
        await repeatJourneyRepository.SaveChangesAsync();

        await AddJourneysStartToNow(createdRepeatJourney);

        return mapper.Map<RepeatJourneyDto>(createdRepeatJourney);
    }


    public async Task<RepeatJourneyDto> UpdateAsync(Guid id, RepeatJourneyDto journey)
    {
        var existingJourney = await repeatJourneyRepository.GetByIdAsync(id) ?? throw new ValidationException("id was not found");
        if (journey.StartDate.CompareTo(journey.EndDate) >= 0) throw new ValidationException("start date needs to be smaller than end date");

        var updatedRepeatJourney = mapper.Map<RepeatJourney>(journey);

        existingJourney.Title = updatedRepeatJourney.Title;
        existingJourney.Distance = updatedRepeatJourney.Distance;
        existingJourney.RepeatDays = updatedRepeatJourney.RepeatDays;
        existingJourney.RepeatType = updatedRepeatJourney.RepeatType;
        existingJourney.StartDate = updatedRepeatJourney.StartDate;
        existingJourney.EndDate = updatedRepeatJourney.EndDate;

        repeatJourneyRepository.Update(existingJourney);
        await repeatJourneyRepository.SaveChangesAsync();

        await RemoveConnectedJourneys(existingJourney);
        await AddJourneysStartToNow(existingJourney);

        return mapper.Map<RepeatJourneyDto>(existingJourney);
    }

    public async Task DeleteAsync(Guid id)
    {
        var repeatJourney = await repeatJourneyRepository.GetByIdAsync(id) ?? throw new ValidationException("id was not found");

        await RemoveConnectedJourneys(repeatJourney);

        repeatJourneyRepository.Remove(repeatJourney);
        await repeatJourneyRepository.SaveChangesAsync();
    }

    public async Task GenerateAllForToday()
    {
        var repeatJourneys = await repeatJourneyRepository.GetAllAsync();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var journeysOfToday = await journeyRepository.GetAllByDate(today);
        foreach (var repeatJourney in repeatJourneys)
        {
            var journeysToAdd = CreateJourneysBetween(repeatJourney, today, today);
            if (journeysOfToday.Count > 0)
            {
                // if the task was aborted during the execution, we want to make sure that there are no duplicates.
                // so if there are already tasks created today, we check each new task if there is already one with the same repeat journey id => this would be a duplicate and we wont add it
                journeysToAdd = [.. journeysToAdd.Where(newJourney => journeysOfToday.Find(existingJourney => existingJourney.RepeatJourney == newJourney.RepeatJourney) == null)];
            }
            journeyRepository.AddRange(journeysToAdd);
        }
        await journeyRepository.SaveChangesAsync();
    }

    private async Task AddJourneysStartToNow(RepeatJourney repeatJourney)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var journeysToAdd = CreateJourneysBetween(repeatJourney, repeatJourney.StartDate, today);
        journeyRepository.AddRange(journeysToAdd);
        await journeyRepository.SaveChangesAsync();
    }

    private async Task RemoveConnectedJourneys(RepeatJourney repeatJourney)
    {
        var journeysToDelete = await journeyRepository.GetAllConnectedToRepeatedJourney(repeatJourney, true);
        journeyRepository.RemoveRange(journeysToDelete);

        var journeysToRemoveConnection = await journeyRepository.GetAllConnectedToRepeatedJourney(repeatJourney, false);
        journeysToRemoveConnection.ForEach(journey => journey.RepeatJourney = null);

        await journeyRepository.SaveChangesAsync();
    }

    private static List<Journey> CreateJourneysBetween(RepeatJourney baseJourney, DateOnly startDate, DateOnly endDate)
    {
        var journeys = new List<Journey>();

        var repeatDays = WeekdayMaskConverter.ToDaysOfWeek(baseJourney.RepeatDays);

        for (var day = startDate; day <= endDate; day = day.AddDays(1))
        {
            if (!repeatDays.Contains(day.DayOfWeek)) continue;

            if (baseJourney.RepeatType == RepeatType.Monthly)
            {
                var weeksSinceStart = FullWeeksBetween(startDate, day);
                if (weeksSinceStart % MONTH_INTERVAL_WEEK_COUNT != 0) continue;
            }

            journeys.Add(new Journey
            {
                Bike = baseJourney.Bike,
                Title = baseJourney.Title,
                Distance = baseJourney.Distance,
                JourneyDate = day,
                RepeatJourney = baseJourney,
                IsConnectedToRepeatJourney = true,
            });

        }
        return journeys;
    }

    private static int FullWeeksBetween(DateOnly start, DateOnly end)
    {
        return (end.DayNumber - start.DayNumber) / 7;
    }
}