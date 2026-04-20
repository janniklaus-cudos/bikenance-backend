using System.ComponentModel;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Backend.Data;
using Backend.Dtos;
using Backend.Models;
using Backend.Repositories;
using Backend.Services;
using FluentAssertions;
using Moq;

namespace Tests.Services;

public class BikeServiceTests
{
    private readonly IMapper _mapper;
    private readonly Mock<IBikeRepository> _bikeRepoMock;
    private readonly Mock<IBikePartService> _partServiceMock;

    public BikeServiceTests()
    {
        var cfg = new MapperConfiguration(c =>
        {
            c.CreateMap<Bike, BikeDto>()
             .ForMember(d => d.Parts, o => o.MapFrom(_ => new List<BikePartDto>()));
        });

        cfg.AssertConfigurationIsValid();
        _mapper = cfg.CreateMapper();


        _bikeRepoMock = new Mock<IBikeRepository>();
        _partServiceMock = new Mock<IBikePartService>();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedDtos()
    {
        // Arrange
        var existingBikes = new List<Bike>
        {
            new() { Id = Guid.NewGuid(), Name = "Bike1", Brand = "Brand1", IconId = 1, Parts = [] },
            new() { Id = Guid.NewGuid(), Name = "Bike2", Brand = "Brand2", IconId = 2, Parts = [] }
        };

        _bikeRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(existingBikes);
        var sut = new BikeService(_mapper, _bikeRepoMock.Object, _partServiceMock.Object);

        // Act
        var result = await sut.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().AllBeOfType<BikeDto>();
        result.First().Name.Should().Be("Bike1");
        result.Last().Name.Should().Be("Bike2");
        _bikeRepoMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddAsync_CreateAndSave()
    {
        // Arrange
        var input = new BikeDto
        {
            Name = "My Bike",
            Brand = "Brand",
            IconId = 2,
            Parts = [new BikePartDto() { Name = "Chain", Position = BikePartPosition.Chain }]
        };

        _bikeRepoMock.Setup(r => r.Add(It.IsAny<Bike>()));
        _bikeRepoMock.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);


        _partServiceMock.Setup(s => s.AddAllByBikeIdAsync(
                It.IsAny<Guid>(),
                input.Parts))
            .ReturnsAsync([]);

        var sut = new BikeService(_mapper, _bikeRepoMock.Object, _partServiceMock.Object);

        // Act
        var result = await sut.AddAsync(input);

        // Assert
        _bikeRepoMock.Verify(r => r.Add(It.Is<Bike>(b =>
            b.Name == input.Name &&
            b.Brand == input.Brand &&
            b.IconId == input.IconId
        )), Times.Once);

        _partServiceMock.Verify(s => s.AddAllByBikeIdAsync(
            result.Id,
            It.Is<List<BikePartDto>>(p => p.Count == input.Parts.Count)
        ), Times.Once);

        _bikeRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);

        result.Name.Should().Be(input.Name);
        result.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UpdateAsync_WhenBikeDoesNotExist_ReturnsNull_AndDoesNotSave()
    {
        // Arrange
        _bikeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Bike?)null);
        var sut = new BikeService(_mapper, _bikeRepoMock.Object, _partServiceMock.Object);
        var input = new BikeDto { Name = "X", Brand = "Y", IconId = 1, Parts = [] };

        // Act
        var result = await sut.UpdateAsync(Guid.NewGuid(), input);

        // Assert
        result.Should().BeNull();
        _bikeRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _bikeRepoMock.Verify(r => r.Update(It.IsAny<Bike>()), Times.Never);
        _partServiceMock.Verify(s => s.UpdateAllAsync(It.IsAny<Guid>(), It.IsAny<List<BikePartDto>>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenBikeExists_UpdatesFields_UpdatesParts_SavesAndReturnsDto()
    {
        // Arrange
        var existing = new Bike { Id = Guid.NewGuid(), Name = "Old", Brand = "OldBrand", IconId = 0, Parts = [] };
        _bikeRepoMock.Setup(r => r.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _partServiceMock.Setup(s => s.UpdateAllAsync(It.IsAny<Guid>(), It.IsAny<List<BikePartDto>>())).ReturnsAsync([]);
        _bikeRepoMock.Setup(r => r.Update(existing));
        _bikeRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = new BikeService(_mapper, _bikeRepoMock.Object, _partServiceMock.Object);
        var input = new BikeDto { Name = "New", Brand = "NewBrand", IconId = 7, Parts = [] };

        // Act
        var result = await sut.UpdateAsync(existing.Id, input);

        // Assert
        existing.Name.Should().Be("New");
        existing.Brand.Should().Be("NewBrand");
        existing.IconId.Should().Be(7);
        _partServiceMock.Verify(s => s.UpdateAllAsync(existing.Id, input.Parts), Times.Once);
        _bikeRepoMock.Verify(r => r.Update(existing), Times.Once);
        _bikeRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        result.Should().NotBeNull();
        result!.Name.Should().Be("New");
    }

    [Fact]
    public async Task DeleteAsync_WhenBikeNotFound_ReturnsFalse()
    {
        // Arrange
        _bikeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Bike?)null);
        var sut = new BikeService(_mapper, _bikeRepoMock.Object, _partServiceMock.Object);

        // Act
        var ok = await sut.DeleteAsync(Guid.NewGuid());

        // Assert
        ok.Should().BeFalse();
        _bikeRepoMock.Verify(r => r.Remove(It.IsAny<Bike>()), Times.Never);
        _bikeRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenBikeFound_RemovesAndReturnsTrue()
    {
        // Arrange
        var existing = new Bike { Id = Guid.NewGuid(), Name = "B", Brand = "X", IconId = 0, Parts = [] };
        _bikeRepoMock.Setup(r => r.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _bikeRepoMock.Setup(r => r.Remove(existing));
        _bikeRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = new BikeService(_mapper, _bikeRepoMock.Object, _partServiceMock.Object);

        // Act
        var ok = await sut.DeleteAsync(existing.Id);

        // Assert
        ok.Should().BeTrue();
        _bikeRepoMock.Verify(r => r.Remove(existing), Times.Once);
        _bikeRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

}