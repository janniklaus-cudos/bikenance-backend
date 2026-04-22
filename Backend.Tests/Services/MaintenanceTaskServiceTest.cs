using AutoMapper;
using Backend.Dtos;
using Backend.Models;
using Backend.Repositories;
using Backend.Services;
using FluentAssertions;
using Moq;

namespace Tests.Services;

public class MaintenanceTaskServiceTests
{
    private readonly IMapper _mapper;
    private readonly Mock<IMaintenanceTaskRepository> _maintenanceTaskRepoMock;
    private readonly Mock<IBikePartRepository> _bikePartRepoMock;

    public MaintenanceTaskServiceTests()
    {
        var cfg = new MapperConfiguration(c =>
        {
            c.CreateMap<MaintenanceTask, MaintenanceTaskDto>();
        });
        cfg.AssertConfigurationIsValid();
        _mapper = cfg.CreateMapper();

        _maintenanceTaskRepoMock = new Mock<IMaintenanceTaskRepository>();
        _bikePartRepoMock = new Mock<IBikePartRepository>();
    }

    [Fact]
    public async Task GetAllByBikePartIdAsync_ReturnsTasks_WhenBikePartExists()
    {
        // Arrange
        var bikePartId = Guid.NewGuid();
        var tasks = new List<MaintenanceTask>
        {
            new() { Id = Guid.NewGuid(), Description = "Task 1", BikePart = null! },
            new() { Id = Guid.NewGuid(), Description = "Task 2", BikePart = null! }
        };
        _maintenanceTaskRepoMock.Setup(r => r.GetAllByBikePartIdAsync(bikePartId, It.IsAny<CancellationToken>())).ReturnsAsync(tasks);
        var sut = new MaintenanceTaskService(_mapper, _maintenanceTaskRepoMock.Object, _bikePartRepoMock.Object);

        // Act
        var result = await sut.GetAllByBikePartIdAsync(bikePartId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllByBikePartIdAsync_ReturnsEmptyList_WhenNoTasksExist()
    {
        // Arrange
        _maintenanceTaskRepoMock.Setup(r => r.GetAllByBikePartIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<MaintenanceTask>());
        var sut = new MaintenanceTaskService(_mapper, _maintenanceTaskRepoMock.Object, _bikePartRepoMock.Object);

        // Act
        var result = await sut.GetAllByBikePartIdAsync(Guid.NewGuid());

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AddAsync_ReturnsNull_WhenBikePartNotFound()
    {
        // Arrange
        _bikePartRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((BikePart?)null);
        var sut = new MaintenanceTaskService(_mapper, _maintenanceTaskRepoMock.Object, _bikePartRepoMock.Object);

        // Act
        var result = await sut.AddAsync(Guid.NewGuid(), new MaintenanceTaskDto());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenTaskNotFound()
    {
        // Arrange
        _maintenanceTaskRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((MaintenanceTask?)null);
        var sut = new MaintenanceTaskService(_mapper, _maintenanceTaskRepoMock.Object, _bikePartRepoMock.Object);

        // Act
        var result = await sut.UpdateAsync(Guid.NewGuid(), new MaintenanceTaskDto());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenTaskNotFound()
    {
        // Arrange
        _maintenanceTaskRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((MaintenanceTask?)null);
        var sut = new MaintenanceTaskService(_mapper, _maintenanceTaskRepoMock.Object, _bikePartRepoMock.Object);

        // Act
        var result = await sut.DeleteAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }
}
