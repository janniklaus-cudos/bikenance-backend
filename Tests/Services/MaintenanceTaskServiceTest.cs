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

public class MaintenanceTaskServiceTests
{
    private readonly IMapper _mapper;

    public MaintenanceTaskServiceTests()
    {
        var cfg = new MapperConfiguration(c =>
        {
            c.CreateMap<MaintenanceTask, MaintenanceTaskDto>();
        });
        cfg.AssertConfigurationIsValid();
        _mapper = cfg.CreateMapper();
    }

    [Fact, Description("as GetAllAsync relies on .ProjectTo we cannot test this in a unit test")]
    public async Task GetAllByBikePartIdAsync()
    {

    }

    [Fact]
    public async Task AddAsync_ReturnsNull_WhenBikePartNotFound()
    {
        // Arrange
        var repo = new Mock<IRepository<MaintenanceTask>>();
        var bikePartRepo = new Mock<IRepository<BikePart>>();
        bikePartRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((BikePart?)null);
        var sut = new MaintenanceTaskService(_mapper, repo.Object, bikePartRepo.Object);

        // Act
        var result = await sut.AddAsync(Guid.NewGuid(), new MaintenanceTaskDto());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenTaskNotFound()
    {
        // Arrange
        var repo = new Mock<IRepository<MaintenanceTask>>();
        var bikePartRepo = new Mock<IRepository<BikePart>>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((MaintenanceTask?)null);
        var sut = new MaintenanceTaskService(_mapper, repo.Object, bikePartRepo.Object);

        // Act
        var result = await sut.UpdateAsync(Guid.NewGuid(), new MaintenanceTaskDto());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenTaskNotFound()
    {
        // Arrange
        var repo = new Mock<IRepository<MaintenanceTask>>();
        var bikePartRepo = new Mock<IRepository<BikePart>>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((MaintenanceTask?)null);
        var sut = new MaintenanceTaskService(_mapper, repo.Object, bikePartRepo.Object);

        // Act
        var result = await sut.DeleteAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }
}
