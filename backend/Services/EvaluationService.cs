using AutoMapper;
using Backend.Data;
using Backend.Dtos;
using Backend.Models;
using Backend.Repositories;

namespace Backend.Services;

public class EvaluationService(IBikePartRepository bikePartRepository, IJourneyRepository journeyRepository) : IEvaluationService
{
    public async Task<BikePartEvaluationDto?> EvaluateBikePartAsync(Guid bikePartId)
    {
        var bikePart = await bikePartRepository.GetByIdAsync(bikePartId);
        if (bikePart == null)
        {
            return null;
        }

        var evaluationSinceLastService = await CalculateSinceLastService(bikePart);

        // if there is no maintenance task, we cannot predict the next service due date
        var maintenanceTask = bikePart.MaintenanceTask;
        if (maintenanceTask == null)
        {
            return evaluationSinceLastService;
        }

        var nextServiceDueDate = CalculateNextServiceDueDate(bikePart, maintenanceTask, evaluationSinceLastService);

        return evaluationSinceLastService with
        {
            NextServiceDueDate = nextServiceDueDate
        };

    }

    private async Task<BikePartEvaluationDto> CalculateSinceLastService(BikePart bikePart)
    {
        var serviceEvents = bikePart.ServiceEvents;
        var latestKnownDate = CalculateLatestServiceEventDate(bikePart);

        var daysSinceLastService = DateTime.Today.Subtract(latestKnownDate.ToDateTime(TimeOnly.MinValue)).Days;
        var distanceSinceLastService = await journeyRepository.GetDistanceAfterDateByBikeId(bikePart.Bike.Id, latestKnownDate);
        int? averageCostPerService = null;
        if (serviceEvents?.Count > 0) averageCostPerService = (int)Math.Round(serviceEvents.Average(se => se.Cost), 0, MidpointRounding.AwayFromZero);

        return new BikePartEvaluationDto
        {
            DaysSinceLastService = daysSinceLastService,
            DistanceSinceLastService = distanceSinceLastService,
            AverageCostPerService = averageCostPerService,
        };
    }

    private static DateOnly? CalculateNextServiceDueDate(BikePart bikePart, MaintenanceTask maintenanceTask, BikePartEvaluationDto evaluationDto)
    {
        var latestKnownDate = CalculateLatestServiceEventDate(bikePart);

        var daysIntervalDueDate = latestKnownDate.AddDays(maintenanceTask.DaysInterval);

        var distancePercentageUntilNextService = (double)evaluationDto.DistanceSinceLastService / maintenanceTask.DistanceInterval;
        // if we have no distance since the last service, we cannot predict when the distance limit will be achieved
        // this could be changed by calculating the average km/month with all journeys, but this is a feature for later
        var distanceIntervalDueDate = DateOnly.MaxValue;
        if (distancePercentageUntilNextService != 0.0)
        {
            var daysSinceLastService = evaluationDto.DaysSinceLastService != 0 ? evaluationDto.DaysSinceLastService : 1;
            var distanceServiceInDays = daysSinceLastService * (1 / distancePercentageUntilNextService);
            distanceIntervalDueDate = latestKnownDate.AddDays((int)Math.Round(distanceServiceInDays, 0));
        }

        if (maintenanceTask.IsDaysIntervalActive && maintenanceTask.IsDistanceIntervalActive)
        {
            return daysIntervalDueDate.CompareTo(distanceIntervalDueDate) < 0 ? daysIntervalDueDate : distanceIntervalDueDate;
        }
        else if (maintenanceTask.IsDaysIntervalActive)
        {
            return daysIntervalDueDate;
        }
        else if (maintenanceTask.IsDistanceIntervalActive)
        {
            return distanceIntervalDueDate;
        }

        // should not happen, but if there is no active interval, we cannot predict the next service due date
        return null;
    }

    private static DateOnly CalculateLatestServiceEventDate(BikePart bikePart)
    {
        var serviceEvents = bikePart.ServiceEvents;
        var latestServiceEvent = serviceEvents.OrderByDescending(se => se.DateOfService).FirstOrDefault();
        return latestServiceEvent?.DateOfService ?? DateOnly.FromDateTime(bikePart.Bike.CreatedAtUtc);
    }
}