using AutoMapper;
using Backend.Data;
using Backend.Dtos;
using Backend.Models;
using Backend.Repositories;

namespace Backend.Services;

public class EvaluationService(IBikePartRepository bikePartRepository) : IEvaluationService
{
    public async Task<BikePartEvaluationDto?> EvaluateBikePartAsync(Guid bikePartId)
    {
        var bikePart = await bikePartRepository.GetByIdAsync(bikePartId);
        if (bikePart == null)
        {
            return null;
        }

        var evaluationSinceLastService = CalculateSinceLastService(bikePart);

        // if there is no maintenance task, we cannot predict the next service due date
        var maintenanceTask = bikePart.MaintenanceTask;
        if (maintenanceTask == null)
        {
            return evaluationSinceLastService;
        }

        var nextServiceDueDate = CalculateNextServiceDueDate(bikePart, maintenanceTask);

        return evaluationSinceLastService with
        {
            NextServiceDueDate = nextServiceDueDate
        };

    }

    private static BikePartEvaluationDto CalculateSinceLastService(BikePart bikePart)
    {
        var serviceEvents = bikePart.ServiceEvents;
        var latestKnownDate = CalculateLatestServiceEventDate(bikePart);

        var daysSinceLastService = latestKnownDate.Subtract(DateTime.UtcNow).Days;
        var distanceSinceLastService = 250; // todo placeholder;
        var costTotal = serviceEvents.Sum(se => se.Cost);

        return new BikePartEvaluationDto
        {
            DaysSinceLastService = daysSinceLastService,
            DistanceSinceLastService = distanceSinceLastService,
            CostTotal = costTotal,
        };
    }

    private static DateTime CalculateNextServiceDueDate(BikePart bikePart, MaintenanceTask maintenanceTask)
    {
        var latestKnownDate = CalculateLatestServiceEventDate(bikePart);

        var daysIntervalDueDate = latestKnownDate.AddDays(maintenanceTask.DaysInterval);
        var distanceIntervalDueDate = DateTime.MaxValue; // todo placeholder
        var nextServiceDueDate = DateTime.Compare(daysIntervalDueDate, distanceIntervalDueDate) < 0 ? daysIntervalDueDate : distanceIntervalDueDate;

        return nextServiceDueDate;
    }

    private static DateTime CalculateLatestServiceEventDate(BikePart bikePart)
    {
        var serviceEvents = bikePart.ServiceEvents;
        var latestServiceEvent = serviceEvents.OrderByDescending(se => se.CreatedAtUtc).FirstOrDefault();
        return latestServiceEvent?.CreatedAtUtc ?? bikePart.Bike.CreatedAtUtc;
    }
}