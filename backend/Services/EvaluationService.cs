using AutoMapper;
using Backend.Data;
using Backend.Dtos;
using Backend.Models;
using Backend.Repositories;

namespace Backend.Services;

public interface IEvaluationService
{
    Task<BikePartEvaluationDto?> EvaluateBikePartAsync(Guid bikePartId);

    Task<List<BikePartPositionStatus>?> EvaluateBikePartPositionStatusAsync(Guid bikeId);

}

public class EvaluationService(IBikePartRepository bikePartRepository, IJourneyRepository journeyRepository, IBikeRepository bikeRepository) : IEvaluationService
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
        if (maintenanceTask == null || !maintenanceTask.IsActive)
        {
            return evaluationSinceLastService;
        }

        var nextServiceDueDate = CalculateNextServiceDueDate(bikePart, maintenanceTask, evaluationSinceLastService);
        return evaluationSinceLastService with
        {
            NextServiceDueDate = nextServiceDueDate
        };

    }

    public async Task<List<BikePartPositionStatus>?> EvaluateBikePartPositionStatusAsync(Guid bikeId)
    {
        var bike = await bikeRepository.GetByIdAsync(bikeId);
        if (bike == null)
        {
            return null;
        }

        var bikePartPositionStatus = new List<BikePartPositionStatus>();

        foreach (var bikePart in bike.Parts)
        {
            if (bikePart.MaintenanceTask == null || !bikePart.MaintenanceTask.IsActive)
                continue;

            var dueDate = CalculateNextServiceDueDate(bikePart, bikePart.MaintenanceTask, await CalculateSinceLastService(bikePart));

            if (dueDate == null) continue;

            var now = DateOnly.FromDateTime(DateTime.Today);
            // due date is already passed
            if (now.CompareTo(dueDate) >= 0)
            {
                bikePartPositionStatus.Add(GenerateStatus(bikePart, Status.CRITICAL));
                continue;
            }

            var inOneMonth = DateOnly.FromDateTime(DateTime.Today.AddDays(30));
            // if inOneMonth is earlier than due date
            if (inOneMonth.CompareTo(dueDate) < 0)
                bikePartPositionStatus.Add(GenerateStatus(bikePart, Status.OK));
            else
                bikePartPositionStatus.Add(GenerateStatus(bikePart, Status.WARNING));


        }

        return bikePartPositionStatus;
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

    private static BikePartPositionStatus GenerateStatus(BikePart bikePart, Status status)
    {
        return new BikePartPositionStatus { BikePartId = bikePart.Id, Position = bikePart.Position, Status = status };
    }
}