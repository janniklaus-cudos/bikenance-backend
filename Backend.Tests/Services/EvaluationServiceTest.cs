using Backend.Dtos;
using Backend.Models;
using Backend.Repositories;
using Backend.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace Tests.Services;

public class EvaluationServiceTests
{
    private readonly Mock<IBikePartRepository> _bikePartRepoMock = new();
    private readonly Mock<IJourneyRepository> _journeyRepoMock = new();
    private readonly Mock<IBikeRepository> _bikeRepoMock = new();

    private EvaluationService CreateSut() =>
        new(_bikePartRepoMock.Object, _journeyRepoMock.Object, _bikeRepoMock.Object);

    private static ServiceEvent CreateServiceEvent(BikePart part, int cost, DateOnly dateOfService) =>
        new()
        {
            BikePart = part,
            Cost = cost,
            DateOfService = dateOfService,
        };

    [Fact]
    public async Task EvaluateBikePartAsync_ReturnsNull_WhenBikePartNotFound()
    {
        _bikePartRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((BikePart?)null);

        var sut = CreateSut();

        var result = await sut.EvaluateBikePartAsync(Guid.NewGuid());

        result.Should().BeNull();
        _journeyRepoMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task EvaluateBikePartAsync_ReturnsSinceLastService_WhenNoMaintenanceTask()
    {
        var now = DateTime.Today;
        var bike = new Bike { Id = Guid.NewGuid(), CreatedAtUtc = now.AddDays(-10) };
        var part = new BikePart { Id = Guid.NewGuid(), Bike = bike };

        var latestServiceDate = DateOnly.FromDateTime(now.AddDays(-2));
        part.ServiceEvents =
        [
            CreateServiceEvent(part, 10, DateOnly.FromDateTime(now.AddDays(-4))),
            CreateServiceEvent(part, 20, latestServiceDate), // latest
        ];
        part.MaintenanceTask = null;

        _bikePartRepoMock
            .Setup(r => r.GetByIdAsync(part.Id))
            .ReturnsAsync(part);

        _journeyRepoMock
            .Setup(r => r.GetDistanceAfterDateByBikeId(bike.Id, latestServiceDate))
            .ReturnsAsync(250);

        var sut = CreateSut();

        var result = await sut.EvaluateBikePartAsync(part.Id);

        result.Should().NotBeNull();
        result!.DaysSinceLastService.Should().Be(2);
        result.DistanceSinceLastService.Should().Be(250);
        result.AverageCostPerService.Should().Be(15); // (10+20)/2
        result.NextServiceDueDate.Should().BeNull();

        _journeyRepoMock.Verify(r => r.GetDistanceAfterDateByBikeId(bike.Id, latestServiceDate), Times.Once);
    }

    [Fact]
    public async Task EvaluateBikePartAsync_UsesBikeCreatedAt_WhenNoServiceEvents()
    {
        var now = DateTime.Today;
        var bike = new Bike { Id = Guid.NewGuid(), CreatedAtUtc = now.AddDays(-40) };
        var part = new BikePart
        {
            Id = Guid.NewGuid(),
            Bike = bike,
            ServiceEvents = []
        };

        part.MaintenanceTask = new MaintenanceTask
        {
            BikePart = part,
            DaysInterval = 10,
            IsDaysIntervalActive = true,
            IsDistanceIntervalActive = false,
        };

        var latestKnownDate = DateOnly.FromDateTime(bike.CreatedAtUtc);

        _bikePartRepoMock
            .Setup(r => r.GetByIdAsync(part.Id))
            .ReturnsAsync(part);

        _journeyRepoMock
            .Setup(r => r.GetDistanceAfterDateByBikeId(bike.Id, latestKnownDate))
            .ReturnsAsync(0);

        var sut = CreateSut();

        var result = await sut.EvaluateBikePartAsync(part.Id);

        result.Should().NotBeNull();
        result.NextServiceDueDate.Should().Be(latestKnownDate.AddDays(10));
        result.AverageCostPerService.Should().BeNull();

        _journeyRepoMock.Verify(r => r.GetDistanceAfterDateByBikeId(bike.Id, latestKnownDate), Times.Once);
    }

    [Fact]
    public async Task EvaluateBikePartAsync_SetsNextServiceDueDate_WhenDaysIntervalActiveOnly()
    {
        var now = DateTime.Today;
        var bike = new Bike { Id = Guid.NewGuid(), CreatedAtUtc = now.AddDays(-100) };
        var part = new BikePart { Id = Guid.NewGuid(), Bike = bike };

        part.MaintenanceTask = new MaintenanceTask
        {
            BikePart = part,
            DaysInterval = 30,
            IsDaysIntervalActive = true,
            IsDistanceIntervalActive = false,
        };

        var latestServiceDate = DateOnly.FromDateTime(now.AddDays(-10));
        part.ServiceEvents = [CreateServiceEvent(part, 5, latestServiceDate)];

        _bikePartRepoMock
            .Setup(r => r.GetByIdAsync(part.Id))
            .ReturnsAsync(part);

        _journeyRepoMock
            .Setup(r => r.GetDistanceAfterDateByBikeId(bike.Id, latestServiceDate))
            .ReturnsAsync(123);

        var sut = CreateSut();

        var result = await sut.EvaluateBikePartAsync(part.Id);

        result.Should().NotBeNull();
        result!.NextServiceDueDate.Should().Be(latestServiceDate.AddDays(30));
    }

    [Fact]
    public async Task EvaluateBikePartAsync_SetsNextServiceDueDate_WhenDistanceIntervalActiveOnly()
    {
        var now = DateTime.Today;
        var bike = new Bike { Id = Guid.NewGuid(), CreatedAtUtc = now.AddDays(-200) };
        var part = new BikePart { Id = Guid.NewGuid(), Bike = bike };

        part.MaintenanceTask = new MaintenanceTask
        {
            BikePart = part,
            IsDaysIntervalActive = false,
            IsDistanceIntervalActive = true,
            DistanceInterval = 1000
        };

        var latestServiceDate = DateOnly.FromDateTime(now.AddDays(-10));
        part.ServiceEvents = [CreateServiceEvent(part, 10, latestServiceDate)];

        // distanceSinceLastService = 250 => percentage = 0.25 of 1000
        // daysSinceLastService = 10 => predicted due in 40 days
        _bikePartRepoMock
            .Setup(r => r.GetByIdAsync(part.Id))
            .ReturnsAsync(part);

        _journeyRepoMock
            .Setup(r => r.GetDistanceAfterDateByBikeId(bike.Id, latestServiceDate))
            .ReturnsAsync(250);

        var sut = CreateSut();

        var result = await sut.EvaluateBikePartAsync(part.Id);

        result.Should().NotBeNull();
        result.NextServiceDueDate.Should().Be(latestServiceDate.AddDays(40));
    }

    [Fact]
    public async Task EvaluateBikePartAsync_WhenBothIntervalsActive_ChoosesEarlierDueDate()
    {
        var now = DateTime.Today;
        var bike = new Bike { Id = Guid.NewGuid(), CreatedAtUtc = now.AddDays(-200) };
        var part = new BikePart { Id = Guid.NewGuid(), Bike = bike };

        part.MaintenanceTask = new MaintenanceTask
        {
            BikePart = part,
            IsDaysIntervalActive = true,
            IsDistanceIntervalActive = true,
            DaysInterval = 30,
            DistanceInterval = 1000
        };

        var latestServiceDate = DateOnly.FromDateTime(now.AddDays(-10));
        part.ServiceEvents = [CreateServiceEvent(part, 10, latestServiceDate)];

        // distanceSinceLastService = 500 => percentage 0.5
        // daysSinceLastService = 10 => distance due in 20 days (earlier than 30)
        _bikePartRepoMock
            .Setup(r => r.GetByIdAsync(part.Id))
            .ReturnsAsync(part);

        _journeyRepoMock
            .Setup(r => r.GetDistanceAfterDateByBikeId(bike.Id, latestServiceDate))
            .ReturnsAsync(500);

        var sut = CreateSut();

        var result = await sut.EvaluateBikePartAsync(part.Id);

        result.Should().NotBeNull();
        result!.NextServiceDueDate.Should().Be(latestServiceDate.AddDays(20));
    }

    [Fact]
    public async Task EvaluateBikePartAsync_ReturnsNullNextServiceDueDate_WhenNoActiveIntervals()
    {
        var now = DateTime.Today;
        var bike = new Bike { Id = Guid.NewGuid(), CreatedAtUtc = now.AddDays(-10) };
        var part = new BikePart { Id = Guid.NewGuid(), Bike = bike };

        part.MaintenanceTask = new MaintenanceTask
        {
            BikePart = part,
            DaysInterval = 10,
            DistanceInterval = 500,
            IsDaysIntervalActive = false,
            IsDistanceIntervalActive = false,
        };

        var latestServiceDate = DateOnly.FromDateTime(now.AddDays(-1));
        part.ServiceEvents = [CreateServiceEvent(part, 1, latestServiceDate)];

        _bikePartRepoMock
            .Setup(r => r.GetByIdAsync(part.Id))
            .ReturnsAsync(part);

        _journeyRepoMock
            .Setup(r => r.GetDistanceAfterDateByBikeId(bike.Id, latestServiceDate))
            .ReturnsAsync(200);

        var sut = CreateSut();

        var result = await sut.EvaluateBikePartAsync(part.Id);

        result.Should().NotBeNull();
        result!.NextServiceDueDate.Should().BeNull();
    }

    [Fact]
    public async Task EvaluateBikePartAsync_WhenDistanceIsZero_AndOnlyDistanceIntervalActive_ReturnsMaxValueDueDate()
    {
        var now = DateTime.Today;
        var bike = new Bike { Id = Guid.NewGuid(), CreatedAtUtc = now.AddDays(-10) };
        var part = new BikePart { Id = Guid.NewGuid(), Bike = bike };

        part.MaintenanceTask = new MaintenanceTask
        {
            BikePart = part,
            IsDaysIntervalActive = false,
            IsDistanceIntervalActive = true,
            DistanceInterval = 1000
        };

        var latestServiceDate = DateOnly.FromDateTime(now.AddDays(-3));
        part.ServiceEvents = [CreateServiceEvent(part, 10, latestServiceDate)];

        _bikePartRepoMock
            .Setup(r => r.GetByIdAsync(part.Id))
            .ReturnsAsync(part);

        _journeyRepoMock
            .Setup(r => r.GetDistanceAfterDateByBikeId(bike.Id, latestServiceDate))
            .ReturnsAsync(0);

        var sut = CreateSut();

        var result = await sut.EvaluateBikePartAsync(part.Id);

        result.Should().NotBeNull();
        result.NextServiceDueDate.Should().Be(DateOnly.MaxValue);
    }

    [Fact]
    public async Task EvaluateBikePartAsync_RoundsAverageCostPerService_ToNearestInt()
    {
        var now = DateTime.Today;
        var bike = new Bike { Id = Guid.NewGuid(), CreatedAtUtc = now.AddDays(-10) };
        var part = new BikePart { Id = Guid.NewGuid(), Bike = bike };

        var latestServiceDate = DateOnly.FromDateTime(now.AddDays(-1));
        part.ServiceEvents =
        [
            CreateServiceEvent(part, 10, latestServiceDate),
            CreateServiceEvent(part, 5, DateOnly.FromDateTime(now.AddDays(-2))),
            CreateServiceEvent(part, 2, DateOnly.FromDateTime(now.AddDays(-2))),
        ];

        _bikePartRepoMock
            .Setup(r => r.GetByIdAsync(part.Id))
            .ReturnsAsync(part);

        _journeyRepoMock
            .Setup(r => r.GetDistanceAfterDateByBikeId(bike.Id, latestServiceDate))
            .ReturnsAsync(0);

        var sut = CreateSut();

        var result = await sut.EvaluateBikePartAsync(part.Id);

        result.Should().NotBeNull();
        // average => 10 + 5 + 2 => 17 / 3 = 5.66 => 6 
        result.AverageCostPerService.Should().Be(6);
    }


    [Fact]
    public async Task EvaluateBikePartPositionStatusAsync_ReturnsNull_WhenBikeNotFound()
    {
        _bikeRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Bike?)null);

        var sut = CreateSut();

        var result = await sut.EvaluateBikePartPositionStatusAsync(Guid.NewGuid());

        result.Should().BeNull();
        _journeyRepoMock.VerifyNoOtherCalls();
        _bikePartRepoMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task EvaluateBikePartPositionStatusAsync_SkipsParts_WhenNoTaskOrNotActiveOrPositionNone()
    {
        var bike = new Bike { Id = Guid.NewGuid(), CreatedAtUtc = DateTime.Today.AddDays(-30) };

        var partNoTask = new BikePart { Id = Guid.NewGuid(), Bike = bike, Position = BikePartPosition.RearBrakes };
        partNoTask.MaintenanceTask = null;

        var partInactiveTask = new BikePart { Id = Guid.NewGuid(), Bike = bike, Position = BikePartPosition.RearWheelRim };
        partInactiveTask.MaintenanceTask = new MaintenanceTask { BikePart = partInactiveTask, IsActive = false };

        var partPositionNone = new BikePart { Id = Guid.NewGuid(), Bike = bike, Position = BikePartPosition.NONE };
        partPositionNone.MaintenanceTask = new MaintenanceTask { BikePart = partPositionNone, IsActive = true };

        bike.Parts = [partNoTask, partInactiveTask, partPositionNone];

        _bikeRepoMock
            .Setup(r => r.GetByIdAsync(bike.Id))
            .ReturnsAsync(bike);

        var sut = CreateSut();

        var result = await sut.EvaluateBikePartPositionStatusAsync(bike.Id);

        result.Should().NotBeNull();
        result!.Should().BeEmpty();
        _journeyRepoMock.VerifyNoOtherCalls(); // should not calculate distances for skipped parts
    }

    [Fact]
    public async Task EvaluateBikePartPositionStatusAsync_ReturnsOk_WhenDueDateIsAfterOneMonth()
    {
        // Arrange
        var bike = new Bike { Id = Guid.NewGuid(), CreatedAtUtc = DateTime.Today.AddDays(-200) };
        var latestServiceDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-5));

        // dueDate = latestServiceDate + 60 days -> definitely > today+30
        var part = CreateActivePart(bike, BikePartPosition.FrontWheelRim, latestServiceDate, daysInterval: 60);
        bike.Parts = [part];

        _bikeRepoMock
            .Setup(r => r.GetByIdAsync(bike.Id))
            .ReturnsAsync(bike);

        _journeyRepoMock
            .Setup(r => r.GetDistanceAfterDateByBikeId(bike.Id, latestServiceDate))
            .ReturnsAsync(100);
    }

    private static BikePart CreateActivePart(
        Bike bike,
        BikePartPosition position,
        DateOnly latestServiceDate,
        int daysInterval)
    {
        var part = new BikePart
        {
            Id = Guid.NewGuid(),
            Bike = bike,
            Position = position,
            ServiceEvents = [],
        };

        part.MaintenanceTask = new MaintenanceTask
        {
            BikePart = part,
            IsActive = true,

            // Make CalculateNextServiceDueDate deterministic: only days interval active
            IsDaysIntervalActive = true,
            IsDistanceIntervalActive = false,
            DaysInterval = daysInterval
        };

        part.ServiceEvents.Add(CreateServiceEvent(part, cost: 10, dateOfService: latestServiceDate));
        return part;
    }
}