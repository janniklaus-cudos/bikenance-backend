using AutoMapper;
using Backend.Dtos;
using Backend.Models;
using Backend.Repositories;
using Backend.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tests.Services;

public class JourneyServiceTests
{
    private readonly IMapper _mapper;
    private readonly Mock<IJourneyRepository> _journeyRepoMock;
    private readonly Mock<IBikeRepository> _bikeRepoMock;

    public JourneyServiceTests()
    {
        var loggerFactory = LoggerFactory.Create(b => { });
        var cfg = new MapperConfiguration(c =>
        {
            c.CreateMap<Journey, JourneyDto>()
                .ForMember(d => d.BikeId, o => o.MapFrom(s => s.Bike.Id));
        }, loggerFactory);
        cfg.AssertConfigurationIsValid();

        _mapper = cfg.CreateMapper();
        _journeyRepoMock = new Mock<IJourneyRepository>();
        _bikeRepoMock = new Mock<IBikeRepository>();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDto_WhenFound()
    {
        // Arrange
        var bike = new Bike { Id = Guid.NewGuid() };
        var journey = new Journey { Id = Guid.NewGuid(), Title = "J1", Kilometer = 12, Bike = bike };

        _journeyRepoMock
            .Setup(r => r.GetByIdAsync(journey.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(journey);

        var sut = new JourneyService(_mapper, _journeyRepoMock.Object, _bikeRepoMock.Object);

        // Act
        var result = await sut.GetByIdAsync(journey.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(journey.Id);
        result.Title.Should().Be(journey.Title);
        result.Kilometer.Should().Be(journey.Kilometer);
        result.BikeId.Should().Be(bike.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        _journeyRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Journey?)null);

        var sut = new JourneyService(_mapper, _journeyRepoMock.Object, _bikeRepoMock.Object);

        // Act
        var result = await sut.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllByBikeIdAsync_ReturnsDtos_WhenRepoReturnsList()
    {
        // Arrange
        var bikeId = Guid.NewGuid();
        var bike = new Bike { Id = bikeId };
        var journeys = new List<Journey>
        {
            new() { Id = Guid.NewGuid(), Title = "A", Kilometer = 10, Bike = bike },
            new() { Id = Guid.NewGuid(), Title = "B", Kilometer = 20, Bike = bike },
        };

        _journeyRepoMock
            .Setup(r => r.GetAllByBikeIdAsync(bikeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(journeys);

        var sut = new JourneyService(_mapper, _journeyRepoMock.Object, _bikeRepoMock.Object);

        // Act
        var result = await sut.GetAllByBikeIdAsync(bikeId);

        // Assert
        result.Should().NotBeNull();
        result!.Should().HaveCount(2);
        result.First().BikeId.Should().Be(bikeId);
    }

    [Fact]
    public async Task GetAllByBikeIdAsync_ReturnsNull_WhenRepoReturnsNull()
    {
        // Arrange
        var bikeId = Guid.NewGuid();
        _journeyRepoMock
            .Setup(r => r.GetAllByBikeIdAsync(bikeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<Journey>?)null!);

        var sut = new JourneyService(_mapper, _journeyRepoMock.Object, _bikeRepoMock.Object);

        // Act
        var result = await sut.GetAllByBikeIdAsync(bikeId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_CreatesJourney_WhenBikeExists()
    {
        // Arrange
        var bike = new Bike { Id = Guid.NewGuid() };
        var dto = new JourneyDto { Title = "New", Kilometer = 42, BikeId = bike.Id };

        _bikeRepoMock
            .Setup(r => r.GetByIdAsync(bike.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bike);

        _journeyRepoMock.Setup(r => r.Add(It.IsAny<Journey>()));
        _journeyRepoMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = new JourneyService(_mapper, _journeyRepoMock.Object, _bikeRepoMock.Object);

        // Act
        var result = await sut.AddAsync(bike.Id, dto);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be(dto.Title);
        result.Kilometer.Should().Be(dto.Kilometer);
        result.BikeId.Should().Be(bike.Id);

        _journeyRepoMock.Verify(r => r.Add(It.Is<Journey>(j =>
            j.Bike == bike &&
            j.Title == dto.Title &&
            j.Kilometer == dto.Kilometer
        )), Times.Once);

        _journeyRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddAsync_ReturnsNull_WhenBikeNotFound()
    {
        // Arrange
        _bikeRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Bike?)null);

        var sut = new JourneyService(_mapper, _journeyRepoMock.Object, _bikeRepoMock.Object);

        // Act
        var result = await sut.AddAsync(Guid.NewGuid(), new JourneyDto { Title = "X", Kilometer = 1 });

        // Assert
        result.Should().BeNull();
        _journeyRepoMock.Verify(r => r.Add(It.IsAny<Journey>()), Times.Never);
        _journeyRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddAllAsync_CreatesJourneys_WhenBikeExists()
    {
        // Arrange
        var bike = new Bike { Id = Guid.NewGuid() };
        var dtos = new List<JourneyDto>
        {
            new() { Title = "A", Kilometer = 10 },
            new() { Title = "B", Kilometer = 20 },
        };

        _bikeRepoMock
            .Setup(r => r.GetByIdAsync(bike.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bike);

        _journeyRepoMock.Setup(r => r.AddRange(It.IsAny<IEnumerable<Journey>>()));
        _journeyRepoMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = new JourneyService(_mapper, _journeyRepoMock.Object, _bikeRepoMock.Object);

        // Act
        var result = await sut.AddAllAsync(bike.Id, dtos);

        // Assert
        result.Should().NotBeNull();
        result!.Should().HaveCount(2);
        result.First().BikeId.Should().Be(bike.Id);

        _journeyRepoMock.Verify(r => r.AddRange(It.Is<IEnumerable<Journey>>(list =>
            list.Count() == 2 &&
            list.All(j => j.Bike == bike)
        )), Times.Once);

        _journeyRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddAllAsync_ReturnsNull_WhenBikeNotFound()
    {
        // Arrange
        _bikeRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Bike?)null);

        var sut = new JourneyService(_mapper, _journeyRepoMock.Object, _bikeRepoMock.Object);

        // Act
        var result = await sut.AddAllAsync(Guid.NewGuid(), new List<JourneyDto>());

        // Assert
        result.Should().BeNull();
        _journeyRepoMock.Verify(r => r.AddRange(It.IsAny<IEnumerable<Journey>>()), Times.Never);
        _journeyRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesAndReturnsDto_WhenFound()
    {
        // Arrange
        var bike = new Bike { Id = Guid.NewGuid() };
        var existing = new Journey { Id = Guid.NewGuid(), Title = "Old", Kilometer = 1, Bike = bike };
        var update = new JourneyDto { Id = existing.Id, Title = "New", Kilometer = 99, BikeId = bike.Id };

        _journeyRepoMock
            .Setup(r => r.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _journeyRepoMock.Setup(r => r.Update(existing));
        _journeyRepoMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = new JourneyService(_mapper, _journeyRepoMock.Object, _bikeRepoMock.Object);

        // Act
        var result = await sut.UpdateAsync(existing.Id, update);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("New");
        result.Kilometer.Should().Be(99);

        _journeyRepoMock.Verify(r => r.Update(It.Is<Journey>(j =>
            j.Id == existing.Id &&
            j.Title == "New" &&
            j.Kilometer == 99
        )), Times.Once);

        _journeyRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        _journeyRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Journey?)null);

        var sut = new JourneyService(_mapper, _journeyRepoMock.Object, _bikeRepoMock.Object);

        // Act
        var result = await sut.UpdateAsync(Guid.NewGuid(), new JourneyDto { Title = "X", Kilometer = 1 });

        // Assert
        result.Should().BeNull();
        _journeyRepoMock.Verify(r => r.Update(It.IsAny<Journey>()), Times.Never);
        _journeyRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
    {
        // Arrange
        _journeyRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Journey?)null);

        var sut = new JourneyService(_mapper, _journeyRepoMock.Object, _bikeRepoMock.Object);

        // Act
        var result = await sut.DeleteAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
        _journeyRepoMock.Verify(r => r.Remove(It.IsAny<Journey>()), Times.Never);
        _journeyRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_RemovesAndReturnsTrue_WhenFound()
    {
        // Arrange
        var bike = new Bike { Id = Guid.NewGuid() };
        var journey = new Journey { Id = Guid.NewGuid(), Title = "X", Kilometer = 1, Bike = bike };

        _journeyRepoMock
            .Setup(r => r.GetByIdAsync(journey.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(journey);

        _journeyRepoMock.Setup(r => r.Remove(journey));
        _journeyRepoMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = new JourneyService(_mapper, _journeyRepoMock.Object, _bikeRepoMock.Object);

        // Act
        var result = await sut.DeleteAsync(journey.Id);

        // Assert
        result.Should().BeTrue();
        _journeyRepoMock.Verify(r => r.Remove(journey), Times.Once);
        _journeyRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAllByBikeIdAsync_ReturnsFalse_WhenBikeNotFound()
    {
        // Arrange
        _bikeRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Bike?)null);

        var sut = new JourneyService(_mapper, _journeyRepoMock.Object, _bikeRepoMock.Object);

        // Act
        var result = await sut.DeleteAllByBikeIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
        _journeyRepoMock.Verify(r => r.RemoveRange(It.IsAny<IEnumerable<Journey>>()), Times.Never);
        _journeyRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAllByBikeIdAsync_ReturnsTrue_WhenNoJourneys()
    {
        // Arrange
        var bike = new Bike { Id = Guid.NewGuid() };

        _bikeRepoMock
            .Setup(r => r.GetByIdAsync(bike.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bike);

        _journeyRepoMock
            .Setup(r => r.GetAllByBikeIdAsync(bike.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Journey>());

        var sut = new JourneyService(_mapper, _journeyRepoMock.Object, _bikeRepoMock.Object);

        // Act
        var result = await sut.DeleteAllByBikeIdAsync(bike.Id);

        // Assert
        result.Should().BeTrue();
        _journeyRepoMock.Verify(r => r.RemoveRange(It.IsAny<IEnumerable<Journey>>()), Times.Never);
        _journeyRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAllByBikeIdAsync_RemovesAndReturnsTrue_WhenJourneysExist()
    {
        // Arrange
        var bike = new Bike { Id = Guid.NewGuid() };
        var journeys = new List<Journey>
        {
            new() { Id = Guid.NewGuid(), Title = "A", Kilometer = 1, Bike = bike },
            new() { Id = Guid.NewGuid(), Title = "B", Kilometer = 2, Bike = bike },
        };

        _bikeRepoMock
            .Setup(r => r.GetByIdAsync(bike.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bike);

        _journeyRepoMock
            .Setup(r => r.GetAllByBikeIdAsync(bike.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(journeys);

        _journeyRepoMock.Setup(r => r.RemoveRange(journeys));
        _journeyRepoMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = new JourneyService(_mapper, _journeyRepoMock.Object, _bikeRepoMock.Object);

        // Act
        var result = await sut.DeleteAllByBikeIdAsync(bike.Id);

        // Assert
        result.Should().BeTrue();
        _journeyRepoMock.Verify(r => r.RemoveRange(journeys), Times.Once);
        _journeyRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}