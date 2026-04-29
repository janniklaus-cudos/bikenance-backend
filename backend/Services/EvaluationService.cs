using AutoMapper;
using Backend.Data;
using Backend.Dtos;
using Backend.Models;
using Backend.Repositories;

namespace Backend.Services;

public interface IEvaluationService
{
    Task<BikeEvaluationDto?> EvaluateBikeAsync(Guid bikeId);
    Task<BikePartEvaluationDto?> EvaluateBikePartAsync(Guid bikePartId);
    Task<List<BikePartPositionStatus>?> EvaluateBikePartPositionStatusAsync(Guid bikeId);

}

public class EvaluationService(IBikePartRepository bikePartRepository, IJourneyRepository journeyRepository, IBikeRepository bikeRepository, IServiceEventRepository serviceEventRepository) : IEvaluationService
{

    public async Task<BikeEvaluationDto?> EvaluateBikeAsync(Guid bikeId)
    {
        var bike = await bikeRepository.GetByIdAsync(bikeId);
        if (bike == null)
        {
            return null;
        }

        var serviceEvents = await serviceEventRepository.GetAllByBikeIdAsync(bikeId);
        var totalCosts = serviceEvents.Sum(se => se.Cost);
        var totalDistance = await journeyRepository.GetDistanceAfterDateByBikeId(bikeId, DateOnly.FromDateTime(bike.CreatedAtUtc));
        double costPerDistance = 0;
        if (totalDistance != 0)
        {
            costPerDistance = Math.Round((double)totalCosts / totalDistance, 2, MidpointRounding.AwayFromZero);
        }

        var bikeParts = await bikePartRepository.GetAllByBikeIdAsync(bikeId);
        var bikePartSummaries = new List<BikePartSummary>();
        bikeParts.ForEach(bikePart =>
        {
            var bikePartServiceEventsCount = bikePart.ServiceEvents.Count;
            var daysSinceBikeCreation = (DateTime.Now.Date - bike.CreatedAtUtc.Date).Days;
            int? averageDaysServiceIntervals = bikePartServiceEventsCount != 0
                                                ? (int)Math.Round((double)daysSinceBikeCreation / bikePartServiceEventsCount, 0, MidpointRounding.AwayFromZero)
                                                : null;

            bikePartSummaries.Add(
                new BikePartSummary
                {
                    Name = bikePart.Name,
                    TotalServices = bikePartServiceEventsCount,
                    TotalCost = bikePart.ServiceEvents.Sum(bp => bp.Cost),
                    AverageDaysServiceInterval = averageDaysServiceIntervals
                });
        });


        var evaluation = new BikeEvaluationDto
        {
            BikeId = bike.Id,
            BikeName = bike.Name,
            TotalServiceEvents = serviceEvents.Count,
            TotalCost = totalCosts,
            CostPerDistance = costPerDistance,
            BikePartSummaries = bikePartSummaries
        };

        return evaluation;
    }

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
        int? totalCost = null;
        if (serviceEvents?.Count > 0)
        {
            totalCost = serviceEvents.Sum(se => se.Cost);
            averageCostPerService = (int)Math.Round((float)totalCost / serviceEvents.Count, 0, MidpointRounding.AwayFromZero);
        }


        return new BikePartEvaluationDto
        {
            DaysSinceLastService = daysSinceLastService,
            DistanceSinceLastService = distanceSinceLastService,
            AverageCostPerService = averageCostPerService,
            TotalCost = totalCost,
        };
    }

    private static DateOnly? CalculateNextServiceDueDate(BikePart bikePart, MaintenanceTask maintenanceTask, BikePartEvaluationDto evaluationDto)
    {
        var latestKnownDate = CalculateLatestServiceEventDate(bikePart);

        var daysIntervalDueDate = latestKnownDate.AddDays(maintenanceTask.DaysInterval);

        var latestService = GetLatestServiceEvent(bikePart);
        var stateAfterLastService = latestService != null ? (double)latestService.StateAfterService / 100 : 1;
        var distancePercentageUntilNextService = (double)evaluationDto.DistanceSinceLastService / maintenanceTask.DistanceInterval + (1 - stateAfterLastService);
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
        var latestServiceEvent = GetLatestServiceEvent(bikePart);
        return latestServiceEvent?.DateOfService ?? DateOnly.FromDateTime(bikePart.Bike.CreatedAtUtc);
    }

    private static ServiceEvent? GetLatestServiceEvent(BikePart bikePart)
    {
        var serviceEvents = bikePart.ServiceEvents;
        if (serviceEvents != null && serviceEvents.Count != 0) return serviceEvents.OrderByDescending(se => se.DateOfService).FirstOrDefault();
        return null;
    }

    private static BikePartPositionStatus GenerateStatus(BikePart bikePart, Status status)
    {
        return new BikePartPositionStatus { BikePartId = bikePart.Id, Position = bikePart.Position, Status = status };
    }
}