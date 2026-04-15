using System.ComponentModel;
using AutoMapper;
using Backend.Data;
using Backend.Dtos;
using Backend.Models;
using Backend.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace Tests.Services;

public class ServiceEventServiceTests
{
    private readonly IMapper _mapper;

    public ServiceEventServiceTests()
    {
        var cfg = new MapperConfiguration(c =>
        {
            c.CreateMap<ServiceEvent, ServiceEventDto>()
                .ForMember(dto => dto.BikePartId, opt => opt.MapFrom(model => model.BikePart.Id));
            c.CreateMap<ServiceEventDto, ServiceEvent>()
                .ForMember(model => model.BikePart, opt => opt.Ignore());
        });
        cfg.AssertConfigurationIsValid();
        _mapper = cfg.CreateMapper();
    }

    [Fact]
    public async Task GetByIdAsync()
    {
        // Arrange
        var serviceEventRepo = new Mock<IRepository<ServiceEvent>>();
        var bikePartRepo = new Mock<IRepository<BikePart>>();
        var bikeRepo = new Mock<IRepository<Bike>>();
        var input = new ServiceEvent
        {
            Id = Guid.NewGuid(),
            BikePart = new BikePart { Id = Guid.NewGuid() },
            Description = "Test Service Event",
            StateAfterService = 80,
            Cost = 20,
            CreatedAtUtc = DateTime.UtcNow
        };
        serviceEventRepo.Setup(r => r.GetByIdAsync(input.Id)).ReturnsAsync(input);
        var sut = new ServiceEventService(_mapper, serviceEventRepo.Object, bikePartRepo.Object, bikeRepo.Object);

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

    [Fact, Description("as GetAllByBikePartIdAsync relies on .ProjectTo we cannot test this in a unit test")]
    public async Task GetAllByBikePartIdAsync()
    {

    }

    [Fact, Description("as GetAllByBikeIdAsync relies on .ProjectTo we cannot test this in a unit test")]
    public async Task GetAllByBikeIdAsync()
    {

    }

    [Fact]
    public async Task AddAsync_ReturnsNull_WhenBikePartNotFound()
    {
        // Arrange
        var serviceEventRepo = new Mock<IRepository<ServiceEvent>>();
        var bikePartRepo = new Mock<IRepository<BikePart>>();
        var bikeRepo = new Mock<IRepository<Bike>>();
        bikePartRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((BikePart?)null);
        var sut = new ServiceEventService(_mapper, serviceEventRepo.Object, bikePartRepo.Object, bikeRepo.Object);

        // Act
        var result = await sut.AddAsync(Guid.NewGuid(), new ServiceEventDto());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenServiceEventNotFound()
    {
        // Arrange
        var serviceEventRepo = new Mock<IRepository<ServiceEvent>>();
        var bikePartRepo = new Mock<IRepository<BikePart>>();
        var bikeRepo = new Mock<IRepository<Bike>>();
        serviceEventRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((ServiceEvent?)null);
        var sut = new ServiceEventService(_mapper, serviceEventRepo.Object, bikePartRepo.Object, bikeRepo.Object);

        // Act
        var result = await sut.UpdateAsync(Guid.NewGuid(), new ServiceEventDto());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenServiceEventNotFound()
    {
        // Arrange
        var serviceEventRepo = new Mock<IRepository<ServiceEvent>>();
        var bikePartRepo = new Mock<IRepository<BikePart>>();
        var bikeRepo = new Mock<IRepository<Bike>>();
        serviceEventRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((ServiceEvent?)null);
        var sut = new ServiceEventService(_mapper, serviceEventRepo.Object, bikePartRepo.Object, bikeRepo.Object);

        // Act
        var result = await sut.DeleteAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAllByBikePartIdAsync_ReturnsFalse_WhenBikePartNotFound()
    {
        // Arrange
        var serviceEventRepo = new Mock<IRepository<ServiceEvent>>();
        var bikePartRepo = new Mock<IRepository<BikePart>>();
        var bikeRepo = new Mock<IRepository<Bike>>();
        bikePartRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((BikePart?)null);
        var sut = new ServiceEventService(_mapper, serviceEventRepo.Object, bikePartRepo.Object, bikeRepo.Object);

        // Act
        var result = await sut.DeleteAllByBikePartIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAllByBikeIdAsync_ReturnsFalse_WhenBikeNotFound()
    {
        // Arrange
        var serviceEventRepo = new Mock<IRepository<ServiceEvent>>();
        var bikePartRepo = new Mock<IRepository<BikePart>>();
        var bikeRepo = new Mock<IRepository<Bike>>();
        bikeRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Bike?)null);
        var sut = new ServiceEventService(_mapper, serviceEventRepo.Object, bikePartRepo.Object, bikeRepo.Object);

        // Act
        var result = await sut.DeleteAllByBikeIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }
}
