using Backend.Models;
using Backend.Repositories;
using Backend.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace Tests.Services;

public class EvaluationServiceTests
{
    private readonly Mock<IBikePartRepository> _bikePartRepoMock;

    public EvaluationServiceTests()
    {
        _bikePartRepoMock = new Mock<IBikePartRepository>();
    }

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
        // Arrange
        _bikePartRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BikePart)null!);

        var sut = new EvaluationService(_bikePartRepoMock.Object);

        // Act
        var result = await sut.EvaluateBikePartAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task EvaluateBikePartAsync_ReturnsSinceLastService_WhenNoMaintenanceTask()
    {
        // Arrange
        var now = DateTime.Today;

        var bike = new Bike { Id = Guid.NewGuid(), CreatedAtUtc = now.AddDays(-10) };
        var part = new BikePart
        {
            Id = Guid.NewGuid(),
            Bike = bike
        };

        var daysSinceLastService = 2;
        part.ServiceEvents =
        [
            CreateServiceEvent(part, 10, DateOnly.FromDateTime(now).AddDays(-(2*daysSinceLastService))),
            CreateServiceEvent(part, 20, DateOnly.FromDateTime(now).AddDays(-daysSinceLastService)), // latest
        ];

        _bikePartRepoMock
            .Setup(r => r.GetByIdAsync(part.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(part);

        var sut = new EvaluationService(_bikePartRepoMock.Object);

        // Act
        var result = await sut.EvaluateBikePartAsync(part.Id);

        // Assert
        result.Should().NotBeNull();
        result.DaysSinceLastService.Should().Be(daysSinceLastService);
        result.DistanceSinceLastService.Should().Be(250);
        result.CostTotal.Should().Be(30);
        result.NextServiceDueDate.Should().BeNull();
    }

    [Fact]
    public async Task EvaluateBikePartAsync_SetsNextServiceDueDate_WhenDaysIntervalActive()
    {
        // Arrange
        var now = DateTime.Today;

        var bike = new Bike { Id = Guid.NewGuid(), CreatedAtUtc = now.AddDays(-100) };
        var part = new BikePart
        {
            Id = Guid.NewGuid(),
            Bike = bike,
        };

        part.MaintenanceTask = new MaintenanceTask
        {
            BikePart = part,
            DaysInterval = 30,
            IsDaysIntervalActive = true,
            IsDistanceIntervalActive = false,
        };

        var serviceEventDate = -10;
        part.ServiceEvents =
        [
            CreateServiceEvent(part, 5, DateOnly.FromDateTime(now).AddDays(serviceEventDate)),
        ];

        _bikePartRepoMock
            .Setup(r => r.GetByIdAsync(part.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(part);

        var sut = new EvaluationService(_bikePartRepoMock.Object);

        // Act
        var result = await sut.EvaluateBikePartAsync(part.Id);

        // Assert
        result.Should().NotBeNull();
        result.NextServiceDueDate.Should().Be(now.AddDays(serviceEventDate + part.MaintenanceTask.DaysInterval));
    }

    [Fact]
    public async Task EvaluateBikePartAsync_UsesBikeCreatedAt_WhenNoServiceEvents()
    {
        // Arrange
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

        _bikePartRepoMock
            .Setup(r => r.GetByIdAsync(part.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(part);

        var sut = new EvaluationService(_bikePartRepoMock.Object);

        // Act
        var result = await sut.EvaluateBikePartAsync(part.Id);

        // Assert
        result.Should().NotBeNull();
        result.NextServiceDueDate.Should().Be(bike.CreatedAtUtc.AddDays(part.MaintenanceTask.DaysInterval));
    }

    [Fact]
    public async Task EvaluateBikePartAsync_ReturnsNullNextServiceDueDate_WhenNoActiveIntervals()
    {
        // Arrange
        var now = DateTime.Today;

        var bike = new Bike { Id = Guid.NewGuid(), CreatedAtUtc = now.AddDays(-10) };
        var part = new BikePart
        {
            Id = Guid.NewGuid(),
            Bike = bike,
        };

        part.MaintenanceTask = new MaintenanceTask
        {
            BikePart = part,
            DaysInterval = 10,
            IsDaysIntervalActive = false,
            IsDistanceIntervalActive = false,
        };

        part.ServiceEvents =
        [
            CreateServiceEvent(part, 1, DateOnly.FromDateTime(now).AddDays(-1)),
        ];

        _bikePartRepoMock
            .Setup(r => r.GetByIdAsync(part.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(part);

        var sut = new EvaluationService(_bikePartRepoMock.Object);

        // Act
        var result = await sut.EvaluateBikePartAsync(part.Id);

        // Assert
        result.Should().NotBeNull();
        result.NextServiceDueDate.Should().BeNull();
    }
}