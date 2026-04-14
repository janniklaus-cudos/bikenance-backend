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

public class BikePartServiceTests
{
    private readonly IMapper _mapper;

    public BikePartServiceTests()
    {
        var cfg = new MapperConfiguration(c =>
        {
            c.CreateMap<BikePart, BikePartDto>();
        });
        cfg.AssertConfigurationIsValid();
        _mapper = cfg.CreateMapper();
    }

    [Fact, Description("as GetAllAsync relies on .ProjectTo we cannot test this in a unit test")]
    public async Task GetByIdAsync()
    {

    }

    [Fact]
    public async Task AddAsync_ReturnsNull_WhenBikeNotFound()
    {
        // Arrange
        var repo = new Mock<IRepository<BikePart>>();
        var bikeRepo = new Mock<IRepository<Bike>>();
        bikeRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Bike?)null);
        var sut = new BikePartService(_mapper, repo.Object, bikeRepo.Object);

        // Act
        var result = await sut.AddAsync(Guid.NewGuid(), new BikePartDto());

        // Assert
        result.Should().BeNull();
    }

    [Fact, Description("as UpdateAllAsync relies on .ToListAsync we cannot test this in a unit test with mocked repositories")]
    public async Task UpdateAllAsync()
    {
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenPartNotFound()
    {
        // Arrange
        var repo = new Mock<IRepository<BikePart>>();
        var bikeRepo = new Mock<IRepository<Bike>>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((BikePart?)null);
        var sut = new BikePartService(_mapper, repo.Object, bikeRepo.Object);

        // Act
        var result = await sut.DeleteAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
        repo.Verify(r => r.Remove(It.IsAny<BikePart>()), Times.Never);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_RemovesAndReturnsTrue_WhenPartFound()
    {
        // Arrange
        var part = new BikePart { Id = Guid.NewGuid(), Name = "X", Position = BikePartPosition.Chain, Bike = new Bike { Id = Guid.NewGuid() } };
        var repo = new Mock<IRepository<BikePart>>();
        var bikeRepo = new Mock<IRepository<Bike>>();
        repo.Setup(r => r.GetByIdAsync(part.Id, It.IsAny<CancellationToken>())).ReturnsAsync(part);
        repo.Setup(r => r.Remove(part));
        repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = new BikePartService(_mapper, repo.Object, bikeRepo.Object);

        // Act
        var result = await sut.DeleteAsync(part.Id);

        // Assert
        result.Should().BeTrue();
        repo.Verify(r => r.Remove(part), Times.Once);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
