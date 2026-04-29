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
    private readonly Mock<IServiceEventRepository> _serviceEventRepoMock = new();

    private EvaluationService CreateSut() =>
        new(_bikePartRepoMock.Object, _journeyRepoMock.Object, _bikeRepoMock.Object, _serviceEventRepoMock.Object);

    private static Bike CreateBike(DateOnly purchaseDate, int price = 0)
        => new()
        {
            Id = Guid.NewGuid(),
            Name = "Bike",
            Brand = "Brand",
            IconId = 1,
            Price = price,
            DateOfPurchase = purchaseDate,
            CreatedAtUtc = DateTime.UtcNow,
            Owner = new User() // keep required nav non-null
        };

    private static BikePart CreatePart(Bike bike, string name = "Part", BikePartPosition pos = BikePartPosition.NONE)
        => new()
        {
            Id = Guid.NewGuid(),
            Bike = bike,
            Name = name,
            Position = pos,
            ServiceEvents = [],
            MaintenanceTask = null
        };

    private static ServiceEvent CreateServiceEvent(BikePart part, int cost, DateOnly dateOfService, int stateAfterService = 100)
        => new()
        {
            BikePart = part,
            Cost = cost,
            DateOfService = dateOfService,
            StateAfterService = stateAfterService,
            Description = "desc"
        };

    // -------------------- EvaluateBikeAsync --------------------

    [Fact]
    public async Task EvaluateBikeAsync_ReturnsNull_WhenBikeNotFound()
    {
        _bikeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Bike?)null);

        var sut = CreateSut();
        var result = await sut.EvaluateBikeAsync(Guid.NewGuid());

        result.Should().BeNull();
        _serviceEventRepoMock.VerifyNoOtherCalls();
        _bikePartRepoMock.VerifyNoOtherCalls();
        _journeyRepoMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task EvaluateBikeAsync_ComputesTotals_AndAddsPartSummaries()
    {
        // Arrange
        var purchase = DateOnly.FromDateTime(DateTime.Today.AddDays(-100));
        var bike = CreateBike(purchase, price: 1000);

        _bikeRepoMock.Setup(r => r.GetByIdAsync(bike.Id)).ReturnsAsync(bike);

        // BikeEvaluation: ServiceEvents come from serviceEventRepository
        _serviceEventRepoMock
            .Setup(r => r.GetAllByBikeIdAsync(bike.Id))
            .ReturnsAsync([
                new ServiceEvent { BikePart = CreatePart(bike), Cost = 100, DateOfService = purchase.AddDays(10), Description = "x" },
                new ServiceEvent { BikePart = CreatePart(bike), Cost = 50,  DateOfService = purchase.AddDays(20), Description = "x" },
            ]);

        _journeyRepoMock
            .Setup(r => r.GetDistanceAfterDateByBikeId(bike.Id, bike.DateOfPurchase))
            .ReturnsAsync(1000);

        // Part summaries: BikeParts come from bikePartRepository
        var chain = CreatePart(bike, "Chain");
        chain.ServiceEvents.Add(CreateServiceEvent(chain, 10, purchase.AddDays(30)));
        chain.ServiceEvents.Add(CreateServiceEvent(chain, 20, purchase.AddDays(40))); // latest

        var tire = CreatePart(bike, "Tire"); // 0 service events

        _bikePartRepoMock
            .Setup(r => r.GetAllByBikeIdAsync(bike.Id))
            .ReturnsAsync([chain, tire]);

        // distance at last service date (purchase + 40)
        _journeyRepoMock
            .Setup(r => r.GetDistanceBeforeDateByBikeId(bike.Id, purchase.AddDays(40)))
            .ReturnsAsync(200);

        var sut = CreateSut();

        // Act
        var result = await sut.EvaluateBikeAsync(bike.Id);

        // Assert
        result.Should().NotBeNull();
        result!.BikeId.Should().Be(bike.Id);
        result.TotalServiceEvents.Should().Be(2);
        result.TotalCost.Should().Be(150);
        result.CostPerDistance.Should().Be(1.15); // (150 + 1000) / 1000

        result.BikePartSummaries.Should().HaveCount(2);

        var chainSummary = result.BikePartSummaries.Single(x => x.Name == "Chain");
        chainSummary.TotalServices.Should().Be(2);
        chainSummary.TotalCost.Should().Be(30);
        chainSummary.AverageDaysServiceInterval.Should().Be(20);     // (40 days)/2, AwayFromZero
        chainSummary.AverageDistanceServiceInterval.Should().Be(100); // 200/2, AwayFromZero

        var tireSummary = result.BikePartSummaries.Single(x => x.Name == "Tire");
        tireSummary.TotalServices.Should().Be(0);
        tireSummary.TotalCost.Should().Be(0);
        tireSummary.AverageDaysServiceInterval.Should().BeNull();
        tireSummary.AverageDistanceServiceInterval.Should().BeNull();
    }

    [Fact]
    public async Task EvaluateBikeAsync_CostPerDistance_IsZero_WhenTotalDistanceIsZero()
    {
        var purchase = DateOnly.FromDateTime(DateTime.Today.AddDays(-10));
        var bike = CreateBike(purchase, price: 500);

        _bikeRepoMock.Setup(r => r.GetByIdAsync(bike.Id)).ReturnsAsync(bike);
        _serviceEventRepoMock.Setup(r => r.GetAllByBikeIdAsync(bike.Id)).ReturnsAsync([]);
        _journeyRepoMock.Setup(r => r.GetDistanceAfterDateByBikeId(bike.Id, bike.DateOfPurchase)).ReturnsAsync(0);
        _bikePartRepoMock.Setup(r => r.GetAllByBikeIdAsync(bike.Id)).ReturnsAsync([]);

        var sut = CreateSut();
        var result = await sut.EvaluateBikeAsync(bike.Id);

        result!.CostPerDistance.Should().Be(0);
    }

    // -------------------- EvaluateBikePartAsync --------------------

    [Fact]
    public async Task EvaluateBikePartAsync_ReturnsNull_WhenBikePartNotFound()
    {
        _bikePartRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((BikePart?)null);

        var sut = CreateSut();
        var result = await sut.EvaluateBikePartAsync(Guid.NewGuid());

        result.Should().BeNull();
        _journeyRepoMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task EvaluateBikePartAsync_WhenNoServiceEvents_SetsCostsNull_AndUsesPurchaseDate()
    {
        var purchase = DateOnly.FromDateTime(DateTime.Today.AddDays(-10));
        var bike = CreateBike(purchase);
        var part = CreatePart(bike, "Chain");

        _bikePartRepoMock.Setup(r => r.GetByIdAsync(part.Id)).ReturnsAsync(part);
        _journeyRepoMock.Setup(r => r.GetDistanceAfterDateByBikeId(bike.Id, purchase)).ReturnsAsync(123);

        var sut = CreateSut();
        var result = await sut.EvaluateBikePartAsync(part.Id);

        result.Should().NotBeNull();
        result!.TotalCost.Should().BeNull();
        result.AverageCostPerService.Should().BeNull();
        result.DistanceSinceLastService.Should().Be(123);
        result.NextServiceDueDate.Should().BeNull();
    }

    [Fact]
    public async Task EvaluateBikePartAsync_WhenTaskMissingOrInactive_DoesNotSetNextServiceDueDate()
    {
        var purchase = DateOnly.FromDateTime(DateTime.Today.AddDays(-20));
        var bike = CreateBike(purchase);
        var part = CreatePart(bike, "Chain");
        var latest = purchase.AddDays(10);
        part.ServiceEvents.Add(CreateServiceEvent(part, 10, latest));

        _bikePartRepoMock.Setup(r => r.GetByIdAsync(part.Id)).ReturnsAsync(part);
        _journeyRepoMock.Setup(r => r.GetDistanceAfterDateByBikeId(bike.Id, latest)).ReturnsAsync(50);

        var sut = CreateSut();

        part.MaintenanceTask = null;
        (await sut.EvaluateBikePartAsync(part.Id))!.NextServiceDueDate.Should().BeNull();

        part.MaintenanceTask = new MaintenanceTask
        {
            BikePart = part,
            IsActive = false,
            IsDaysIntervalActive = true,
            DaysInterval = 30,
            IsDistanceIntervalActive = false,
            DistanceInterval = 1000
        };
        (await sut.EvaluateBikePartAsync(part.Id))!.NextServiceDueDate.Should().BeNull();
    }

    [Fact]
    public async Task EvaluateBikePartAsync_SetsNextServiceDueDate_DaysOnly()
    {
        var purchase = DateOnly.FromDateTime(DateTime.Today.AddDays(-100));
        var bike = CreateBike(purchase);
        var part = CreatePart(bike, "Chain");
        var latest = DateOnly.FromDateTime(DateTime.Today.AddDays(-10));
        part.ServiceEvents.Add(CreateServiceEvent(part, 10, latest));

        part.MaintenanceTask = new MaintenanceTask
        {
            BikePart = part,
            IsActive = true,
            IsDaysIntervalActive = true,
            DaysInterval = 30,
            IsDistanceIntervalActive = false,
            DistanceInterval = 1000
        };

        _bikePartRepoMock.Setup(r => r.GetByIdAsync(part.Id)).ReturnsAsync(part);
        _journeyRepoMock.Setup(r => r.GetDistanceAfterDateByBikeId(bike.Id, latest)).ReturnsAsync(123);

        var sut = CreateSut();
        var result = await sut.EvaluateBikePartAsync(part.Id);

        result!.NextServiceDueDate.Should().Be(latest.AddDays(30));
    }

    [Fact]
    public async Task EvaluateBikePartAsync_DistanceOnly_WithZeroDistance_ButStateAfterService50_PredictsDueDate()
    {
        // Covers: stateAfterService influences distancePercentageUntilNextService
        var purchase = DateOnly.FromDateTime(DateTime.Today.AddDays(-100));
        var bike = CreateBike(purchase);
        var part = CreatePart(bike, "Chain");
        var latest = DateOnly.FromDateTime(DateTime.Today.AddDays(-10));
        part.ServiceEvents.Add(CreateServiceEvent(part, 10, latest, stateAfterService: 50)); // => +0.5

        part.MaintenanceTask = new MaintenanceTask
        {
            BikePart = part,
            IsActive = true,
            IsDaysIntervalActive = false,
            IsDistanceIntervalActive = true,
            DistanceInterval = 1000,
            DaysInterval = 999
        };

        _bikePartRepoMock.Setup(r => r.GetByIdAsync(part.Id)).ReturnsAsync(part);
        _journeyRepoMock.Setup(r => r.GetDistanceAfterDateByBikeId(bike.Id, latest)).ReturnsAsync(0);

        // distancePercentage = 0/1000 + (1 - 0.5) = 0.5
        // daysSince = 10 => predicted = 10*(1/0.5)=20 => latest+20
        var sut = CreateSut();
        var result = await sut.EvaluateBikePartAsync(part.Id);

        result!.NextServiceDueDate.Should().Be(latest.AddDays(20));
    }

    [Fact]
    public async Task EvaluateBikePartAsync_CalculatesTotalAndAverageCost_AwayFromZero()
    {
        var purchase = DateOnly.FromDateTime(DateTime.Today.AddDays(-10));
        var bike = CreateBike(purchase);
        var part = CreatePart(bike, "Chain");

        var d1 = purchase.AddDays(1);
        var d2 = purchase.AddDays(2); // latest
        part.ServiceEvents.Add(CreateServiceEvent(part, 10, d1));
        part.ServiceEvents.Add(CreateServiceEvent(part, 11, d2));

        _bikePartRepoMock.Setup(r => r.GetByIdAsync(part.Id)).ReturnsAsync(part);
        _journeyRepoMock.Setup(r => r.GetDistanceAfterDateByBikeId(bike.Id, d2)).ReturnsAsync(0);

        var sut = CreateSut();
        var result = await sut.EvaluateBikePartAsync(part.Id);

        result!.TotalCost.Should().Be(21);
        result.AverageCostPerService.Should().Be(11); // 10.5 AwayFromZero => 11
    }

    // -------------------- EvaluateBikePartPositionStatusAsync --------------------

    [Fact]
    public async Task EvaluateBikePartPositionStatusAsync_ReturnsNull_WhenBikeNotFound()
    {
        _bikeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Bike?)null);

        var sut = CreateSut();
        var result = await sut.EvaluateBikePartPositionStatusAsync(Guid.NewGuid());

        result.Should().BeNull();
        _journeyRepoMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task EvaluateBikePartPositionStatusAsync_SkipsParts_WhenNoTaskOrInactive()
    {
        var purchase = DateOnly.FromDateTime(DateTime.Today.AddDays(-100));
        var bike = CreateBike(purchase);

        var p1 = CreatePart(bike, "NoTask", BikePartPosition.NONE);
        p1.MaintenanceTask = null;

        var p2 = CreatePart(bike, "Inactive", BikePartPosition.NONE);
        p2.MaintenanceTask = new MaintenanceTask { BikePart = p2, IsActive = false };

        bike.Parts = [p1, p2];

        _bikeRepoMock.Setup(r => r.GetByIdAsync(bike.Id)).ReturnsAsync(bike);

        var sut = CreateSut();
        var result = await sut.EvaluateBikePartPositionStatusAsync(bike.Id);

        result!.Should().BeEmpty();
        _journeyRepoMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task EvaluateBikePartPositionStatusAsync_ReturnsCritical_WhenDueDatePassed()
    {
        var purchase = DateOnly.FromDateTime(DateTime.Today.AddDays(-200));
        var bike = CreateBike(purchase);

        var part = CreatePart(bike, "Chain", BikePartPosition.NONE);
        part.MaintenanceTask = new MaintenanceTask
        {
            BikePart = part,
            IsActive = true,
            IsDaysIntervalActive = true,
            IsDistanceIntervalActive = false,
            DaysInterval = 10,
            DistanceInterval = 1000
        };

        var latest = DateOnly.FromDateTime(DateTime.Today.AddDays(-40)); // due = -30 => passed
        part.ServiceEvents.Add(CreateServiceEvent(part, 10, latest));

        bike.Parts = [part];

        _bikeRepoMock.Setup(r => r.GetByIdAsync(bike.Id)).ReturnsAsync(bike);
        _journeyRepoMock.Setup(r => r.GetDistanceAfterDateByBikeId(bike.Id, latest)).ReturnsAsync(0);

        var sut = CreateSut();
        var result = await sut.EvaluateBikePartPositionStatusAsync(bike.Id);

        result.Should().ContainSingle();
        result![0].BikePartId.Should().Be(part.Id);
        result[0].Status.Should().Be(Status.CRITICAL);
    }

    [Fact]
    public async Task EvaluateBikePartPositionStatusAsync_ReturnsOk_WhenDueDateAfterOneMonth()
    {
        var purchase = DateOnly.FromDateTime(DateTime.Today.AddDays(-200));
        var bike = CreateBike(purchase);

        var part = CreatePart(bike, "Chain", BikePartPosition.NONE);
        part.MaintenanceTask = new MaintenanceTask
        {
            BikePart = part,
            IsActive = true,
            IsDaysIntervalActive = true,
            IsDistanceIntervalActive = false,
            DaysInterval = 60,
            DistanceInterval = 1000
        };

        var latest = DateOnly.FromDateTime(DateTime.Today.AddDays(-5)); // due in 55 days > 30
        part.ServiceEvents.Add(CreateServiceEvent(part, 10, latest));

        bike.Parts = [part];

        _bikeRepoMock.Setup(r => r.GetByIdAsync(bike.Id)).ReturnsAsync(bike);
        _journeyRepoMock.Setup(r => r.GetDistanceAfterDateByBikeId(bike.Id, latest)).ReturnsAsync(0);

        var sut = CreateSut();
        var result = await sut.EvaluateBikePartPositionStatusAsync(bike.Id);

        result!.Should().ContainSingle(x => x.Status == Status.OK);
    }

    [Fact]
    public async Task EvaluateBikePartPositionStatusAsync_ReturnsWarning_WhenDueDateWithinOneMonth()
    {
        var purchase = DateOnly.FromDateTime(DateTime.Today.AddDays(-200));
        var bike = CreateBike(purchase);

        var part = CreatePart(bike, "Chain", BikePartPosition.NONE);
        part.MaintenanceTask = new MaintenanceTask
        {
            BikePart = part,
            IsActive = true,
            IsDaysIntervalActive = true,
            IsDistanceIntervalActive = false,
            DaysInterval = 10,
            DistanceInterval = 1000
        };

        var latest = DateOnly.FromDateTime(DateTime.Today.AddDays(-5)); // due in 5 days <= 30
        part.ServiceEvents.Add(CreateServiceEvent(part, 10, latest));

        bike.Parts = [part];

        _bikeRepoMock.Setup(r => r.GetByIdAsync(bike.Id)).ReturnsAsync(bike);
        _journeyRepoMock.Setup(r => r.GetDistanceAfterDateByBikeId(bike.Id, latest)).ReturnsAsync(0);

        var sut = CreateSut();
        var result = await sut.EvaluateBikePartPositionStatusAsync(bike.Id);

        result!.Should().ContainSingle(x => x.Status == Status.WARNING);
    }
}