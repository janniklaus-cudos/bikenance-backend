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

public class BikePartServiceTests
{
    private readonly IMapper _mapper;
    private readonly Mock<IBikePartRepository> _bikePartRepoMock;
    private readonly Mock<IBikeRepository> _bikeRepoMock;

    public BikePartServiceTests()
    {
        var loggerFactory = LoggerFactory.Create(b => { });
        var cfg = new MapperConfiguration(c =>
        {
            c.AddProfile(new BikePartProfile());
        }, loggerFactory);
        cfg.AssertConfigurationIsValid();
        _mapper = cfg.CreateMapper();

        _bikePartRepoMock = new Mock<IBikePartRepository>();
        _bikeRepoMock = new Mock<IBikeRepository>();
    }

    [Fact]
    public async Task GetByIdAsync()
    {
        // Arrange
        var input = new BikePart { Id = Guid.NewGuid(), Name = "Chain", Position = BikePartPosition.Chain, Bike = new Bike { Id = Guid.NewGuid() } };
        _bikePartRepoMock.Setup(r => r.GetByIdAsync(input.Id, It.IsAny<CancellationToken>())).ReturnsAsync(input);
        var sut = new BikePartService(_mapper, _bikePartRepoMock.Object, _bikeRepoMock.Object);

        // Act
        var result = await sut.GetByIdAsync(input.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(input.Id);
        result.Name.Should().Be(input.Name);
        result.Position.Should().Be(input.Position);
        result.BikeId.Should().Be(input.Bike.Id);
    }

    [Fact]
    public async Task AddAllByBikeIdAsync()
    {
        // Arrange
        var bike = new Bike { Id = Guid.NewGuid() };
        _bikeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(bike);
        var sut = new BikePartService(_mapper, _bikePartRepoMock.Object, _bikeRepoMock.Object);
        var input = new List<BikePartDto>
        {
            new() { Name = "Chain", Position = BikePartPosition.Chain},
            new() { Name = "Custom" }
        };

        // Act
        var result = await sut.AddAllByBikeIdAsync(bike.Id, input);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(input.Count);
        result[0].Name.Should().Be(input[0].Name);
        result[1].Position.Should().Be(BikePartPosition.NONE);
    }

    [Fact]
    public async Task AddAllByBikeIdAsync_ReturnsNull_WhenBikeNotFound()
    {
        // Arrange
        _bikeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Bike?)null);
        var sut = new BikePartService(_mapper, _bikePartRepoMock.Object, _bikeRepoMock.Object);

        // Act
        var result = await sut.AddAllByBikeIdAsync(Guid.NewGuid(), []);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAllAsync_UpdatesParts_WhenBikeExists()
    {
        // Arrange
        var bike = new Bike { Id = Guid.NewGuid() };
        var existingParts = new List<BikePart>
        {
            new BikePart { Id = Guid.NewGuid(), Name = "Chain", Position = BikePartPosition.Chain, Bike = bike },
            new BikePart { Id = Guid.NewGuid(), Name = "Tire", Position = BikePartPosition.FrontWheelTire, Bike = bike }
        };
        var updateDtos = new List<BikePartDto>
        {
            new BikePartDto { Id = existingParts[0].Id, Name = "Chain Updated", Position = BikePartPosition.Chain, BikeId = bike.Id },
            new BikePartDto { Id = existingParts[1].Id, Name = "Tire Updated", Position = BikePartPosition.FrontWheelTire, BikeId = bike.Id }
        };

        _bikeRepoMock.Setup(r => r.GetByIdAsync(bike.Id, It.IsAny<CancellationToken>())).ReturnsAsync(bike);
        _bikePartRepoMock.Setup(r => r.GetAllByBikeIdAsync(bike.Id, It.IsAny<CancellationToken>())).ReturnsAsync(existingParts);
        _bikePartRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = new BikePartService(_mapper, _bikePartRepoMock.Object, _bikeRepoMock.Object);

        // Act
        var result = await sut.UpdateAllAsync(bike.Id, updateDtos);

        // Assert
        result.Should().NotBeNull();
        result[0].Name.Should().Be("Chain Updated");
        _bikePartRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task UpdateAllAsync_RemovesParts_WhenBikeExists()
    {
        // Arrange
        var bike = new Bike { Id = Guid.NewGuid() };
        var existingParts = new List<BikePart>
        {
            new BikePart { Id = Guid.NewGuid(), Name = "Chain", Position = BikePartPosition.Chain, Bike = bike },
            new BikePart { Id = Guid.NewGuid(), Name = "Tire", Position = BikePartPosition.FrontWheelTire, Bike = bike }
        };
        var updatedDtos = new List<BikePartDto>
        {
            new BikePartDto { Id = existingParts[1].Id, Name = "Tire Updated", Position = BikePartPosition.FrontWheelTire, BikeId = bike.Id }
        };

        _bikeRepoMock.Setup(r => r.GetByIdAsync(bike.Id, It.IsAny<CancellationToken>())).ReturnsAsync(bike);
        _bikePartRepoMock.Setup(r => r.GetAllByBikeIdAsync(bike.Id, It.IsAny<CancellationToken>())).ReturnsAsync(existingParts);
        _bikePartRepoMock.Setup(r => r.Remove(It.IsAny<BikePart>()));
        _bikePartRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = new BikePartService(_mapper, _bikePartRepoMock.Object, _bikeRepoMock.Object);

        // Act
        var result = await sut.UpdateAllAsync(bike.Id, updatedDtos);

        // Assert
        result.Should().NotBeNull();
        _bikePartRepoMock.Verify(r => r.Remove(existingParts[0]), Times.Once);
        _bikePartRepoMock.Verify(r => r.UpdateRange(existingParts), Times.Never);
        _bikePartRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task UpdateAllAsync_AddsNewParts_WhenBikeExists()
    {
        // Arrange
        var bike = new Bike { Id = Guid.NewGuid() };
        var existingParts = new List<BikePart>
        {
            new() { Id = Guid.NewGuid(), Name = "Tire", Position = BikePartPosition.FrontWheelTire, Bike = bike }
        };
        var updatedDtos = new List<BikePartDto>
        {
            new() { Id = existingParts[0].Id, Name = "Tire Updated", Position = BikePartPosition.FrontWheelTire, BikeId = bike.Id },
            new() { Name = "Custom" }

        };

        _bikeRepoMock.Setup(r => r.GetByIdAsync(bike.Id, It.IsAny<CancellationToken>())).ReturnsAsync(bike);
        _bikePartRepoMock.Setup(r => r.GetAllByBikeIdAsync(bike.Id, It.IsAny<CancellationToken>())).ReturnsAsync(existingParts);
        _bikePartRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = new BikePartService(_mapper, _bikePartRepoMock.Object, _bikeRepoMock.Object);

        // Act
        var result = await sut.UpdateAllAsync(bike.Id, updatedDtos);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(2);
        result[1].Name.Should().Be(updatedDtos[1].Name);
        _bikePartRepoMock.Verify(r => r.AddRange(It.IsAny<IEnumerable<BikePart>>()), Times.Once);
        _bikePartRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task UpdateAllAsync_ReturnsEmptyList_WhenBikeNotFound()
    {
        // Arrange
        _bikeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Bike?)null);
        var sut = new BikePartService(_mapper, _bikePartRepoMock.Object, _bikeRepoMock.Object);

        // Act
        var result = await sut.UpdateAllAsync(new Guid(), new List<BikePartDto>());

        // Assert
        result.Should().BeNull();
        _bikePartRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenPartNotFound()
    {
        // Arrange
        _bikePartRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((BikePart?)null);
        var sut = new BikePartService(_mapper, _bikePartRepoMock.Object, _bikeRepoMock.Object);

        // Act
        var result = await sut.DeleteAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
        _bikePartRepoMock.Verify(r => r.Remove(It.IsAny<BikePart>()), Times.Never);
        _bikePartRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_RemovesAndReturnsTrue_WhenPartFound()
    {
        // Arrange
        var part = new BikePart { Id = Guid.NewGuid(), Name = "X", Position = BikePartPosition.Chain, Bike = new Bike { Id = Guid.NewGuid() } };
        _bikePartRepoMock.Setup(r => r.GetByIdAsync(part.Id, It.IsAny<CancellationToken>())).ReturnsAsync(part);
        _bikePartRepoMock.Setup(r => r.Remove(part));
        _bikePartRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = new BikePartService(_mapper, _bikePartRepoMock.Object, _bikeRepoMock.Object);

        // Act
        var result = await sut.DeleteAsync(part.Id);

        // Assert
        result.Should().BeTrue();
        _bikePartRepoMock.Verify(r => r.Remove(part), Times.Once);
        _bikePartRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
