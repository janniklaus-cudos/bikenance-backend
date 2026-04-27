using System.ComponentModel;
using AutoMapper;
using Backend.Data;
using Backend.Dtos;
using Backend.Mapping;
using Backend.Models;
using Backend.Repositories;
using Backend.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tests.Services;

public class ServiceEventServiceTests
{
    private readonly IMapper _mapper;

    private readonly Mock<IServiceEventRepository> _serviceEventRepo;
    private readonly Mock<IBikePartRepository> _bikePartRepo;
    private readonly Mock<IBikeRepository> _bikeRepo;

    public ServiceEventServiceTests()
    {
        var loggerFactory = LoggerFactory.Create(b => { });
        var cfg = new MapperConfiguration(c =>
        {
            c.AddProfile(new ServiceEventProfile());
        }, loggerFactory);
        cfg.AssertConfigurationIsValid();
        _mapper = cfg.CreateMapper();

        _serviceEventRepo = new Mock<IServiceEventRepository>();
        _bikePartRepo = new Mock<IBikePartRepository>();
        _bikeRepo = new Mock<IBikeRepository>();
    }

    [Fact]
    public async Task GetByIdAsync()
    {
        // Arrange
        var input = new ServiceEvent
        {
            Id = Guid.NewGuid(),
            BikePart = new BikePart { Id = Guid.NewGuid() },
            Description = "Test Service Event",
            StateAfterService = 80,
            Cost = 20,
            CreatedAtUtc = DateTime.UtcNow
        };
        _serviceEventRepo.Setup(r => r.GetByIdAsync(input.Id)).ReturnsAsync(input);
        var sut = new ServiceEventService(_mapper, _serviceEventRepo.Object, _bikePartRepo.Object, _bikeRepo.Object);

        // Act
        var result = await sut.GetByIdAsync(input.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(input.Id);
        result.BikePartId.Should().Be(input.BikePart.Id);
        result.Description.Should().Be(input.Description);
        result.StateAfterService.Should().Be(input.StateAfterService);
        result.Cost.Should().Be(input.Cost);
    }

    [Fact]
    public async Task GetAllByBikePartIdAsync_ReturnsServiceEvents()
    {
        // Arrange
        var bikePart = new BikePart() { Id = Guid.NewGuid() };
        var input = new List<ServiceEvent>
        {
            new() {
                Id = Guid.NewGuid(),
                BikePart = bikePart,
                Description = "Test Service Event 1",
                StateAfterService = 80,
                Cost = 20,
                CreatedAtUtc = DateTime.UtcNow
            },
            new() {
                Id = Guid.NewGuid(),
                BikePart = bikePart,
                Description = "Test Service Event 2",
                StateAfterService = 60,
                Cost = 50,
                CreatedAtUtc = DateTime.UtcNow
            }
        };
        _serviceEventRepo.Setup(r => r.GetAllByBikePartIdAsync(bikePart.Id, It.IsAny<CancellationToken>())).ReturnsAsync(input);
        var sut = new ServiceEventService(_mapper, _serviceEventRepo.Object, _bikePartRepo.Object, _bikeRepo.Object);

        // Act
        var result = await sut.GetAllByBikePartIdAsync(bikePart.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().Id.Should().Be(input[0].Id);
        result.First().BikePartId.Should().Be(input[0].BikePart.Id);
        result.Last().Id.Should().Be(input[1].Id);
    }

    [Fact]
    public async Task GetAllByBikeIdAsync_ReturnsServiceEvents()
    {
        // Arrange
        var bike = new Bike() { Id = Guid.NewGuid() };
        var input = new List<ServiceEvent>
        {
            new() {
                Id = Guid.NewGuid(),
                BikePart = new BikePart { Id = Guid.NewGuid(), Bike = bike },
                Description = "Test Service Event 1",
                StateAfterService = 80,
                Cost = 20,
                CreatedAtUtc = DateTime.UtcNow
            },
            new() {
                Id = Guid.NewGuid(),
                BikePart = new BikePart { Id = Guid.NewGuid(), Bike = bike },
                Description = "Test Service Event 2",
                StateAfterService = 60,
                Cost = 50,
                CreatedAtUtc = DateTime.UtcNow
            }
        };
        _serviceEventRepo.Setup(r => r.GetAllByBikeIdAsync(bike.Id, It.IsAny<CancellationToken>())).ReturnsAsync(input);
        var sut = new ServiceEventService(_mapper, _serviceEventRepo.Object, _bikePartRepo.Object, _bikeRepo.Object);

        // Act
        var result = await sut.GetAllByBikeIdAsync(bike.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().Id.Should().Be(input[0].Id);
        result.Last().Id.Should().Be(input[1].Id);
    }

    [Fact]
    public async Task AddAsync_ReturnsNull_WhenBikePartNotFound()
    {
        // Arrange
        _bikePartRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((BikePart?)null);
        var sut = new ServiceEventService(_mapper, _serviceEventRepo.Object, _bikePartRepo.Object, _bikeRepo.Object);

        // Act
        var result = await sut.AddAsync(Guid.NewGuid(), new ServiceEventDto());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenServiceEventNotFound()
    {
        // Arrange
        _serviceEventRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((ServiceEvent?)null);
        var sut = new ServiceEventService(_mapper, _serviceEventRepo.Object, _bikePartRepo.Object, _bikeRepo.Object);

        // Act
        var result = await sut.UpdateAsync(Guid.NewGuid(), new ServiceEventDto());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenServiceEventNotFound()
    {
        // Arrange
        _serviceEventRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((ServiceEvent?)null);
        var sut = new ServiceEventService(_mapper, _serviceEventRepo.Object, _bikePartRepo.Object, _bikeRepo.Object);

        // Act
        var result = await sut.DeleteAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAllByBikePartIdAsync_ReturnsFalse_WhenBikePartNotFound()
    {
        // Arrange
        _bikePartRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((BikePart?)null);
        var sut = new ServiceEventService(_mapper, _serviceEventRepo.Object, _bikePartRepo.Object, _bikeRepo.Object);

        // Act
        var result = await sut.DeleteAllByBikePartIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAllByBikeIdAsync_ReturnsFalse_WhenBikeNotFound()
    {
        // Arrange
        _bikeRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Bike?)null);
        var sut = new ServiceEventService(_mapper, _serviceEventRepo.Object, _bikePartRepo.Object, _bikeRepo.Object);

        // Act
        var result = await sut.DeleteAllByBikeIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }
}
