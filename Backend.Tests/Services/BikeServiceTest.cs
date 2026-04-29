using System.ComponentModel;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Backend.Data;
using Backend.Dtos;
using Backend.Models;
using Backend.Repositories;
using Backend.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Services;

public class BikeServiceTests
{
    private readonly IMapper _mapper;
    private readonly Mock<IBikeRepository> _bikeRepoMock;
    private readonly Mock<IBikePartService> _partServiceMock;
    private readonly Mock<IUserRepository> _userRepoMock;



    public BikeServiceTests()
    {
        var loggerFactory = LoggerFactory.Create(b => { });
        var cfg = new MapperConfiguration(c =>
        {
            c.CreateMap<Bike, BikeDto>()
                .ForMember(d => d.Parts, o => o.MapFrom(_ => new List<BikePartDto>()))
                .ForMember(d => d.OwnerId, o => o.MapFrom(s => s.Owner.Id));
            c.CreateMap<BikeDto, Bike>()
                .ForMember(m => m.Owner, o => o.Ignore())
                .ForMember(m => m.Parts, o => o.Ignore());
        }, loggerFactory);

        cfg.AssertConfigurationIsValid();
        _mapper = cfg.CreateMapper();


        _bikeRepoMock = new Mock<IBikeRepository>();
        _partServiceMock = new Mock<IBikePartService>();
        _userRepoMock = new Mock<IUserRepository>();
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
        var sut = new BikeService(_mapper, _bikeRepoMock.Object, _partServiceMock.Object, _userRepoMock.Object);

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
        var ownerId = Guid.NewGuid();
        var owner = new User { Id = ownerId };

        var input = new BikeDto
        {
            OwnerId = ownerId,
            Name = "My Bike",
            Brand = "Brand",
            IconId = 2,
            Price = 0,
            DateOfPurchase = DateOnly.FromDateTime(DateTime.Today),
            Parts = [new BikePartDto { Name = "Chain", Position = BikePartPosition.Chain }]
        };

        _userRepoMock
            .Setup(r => r.GetByIdAsync(ownerId))
            .ReturnsAsync(owner);

        // ensure SaveChangesAsync succeeds
        _bikeRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // capture the Bike that is added
        Bike? addedBike = null;
        _bikeRepoMock
            .Setup(r => r.Add(It.IsAny<Bike>()))
            .Callback<Bike>(b => addedBike = b);

        _partServiceMock
            .Setup(s => s.AddAllByBikeIdAsync(It.IsAny<Guid>(), It.IsAny<List<BikePartDto>>()))
            .ReturnsAsync([]);

        var sut = new BikeService(_mapper, _bikeRepoMock.Object, _partServiceMock.Object, _userRepoMock.Object);

        // Act
        var result = await sut.AddAsync(input);

        // Assert
        result.Should().NotBeNull();

        _userRepoMock.Verify(r => r.GetByIdAsync(ownerId), Times.Once);
        _bikeRepoMock.Verify(r => r.Add(It.IsAny<Bike>()), Times.Once);
        _bikeRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);

        addedBike.Should().NotBeNull();
        addedBike!.Name.Should().Be(input.Name);
        addedBike.Brand.Should().Be(input.Brand);
        addedBike.IconId.Should().Be(input.IconId);
        addedBike.Owner.Should().BeSameAs(owner);

        _partServiceMock.Verify(s => s.AddAllByBikeIdAsync(
            addedBike.Id,
            It.Is<List<BikePartDto>>(p => p.Count == input.Parts.Count)
        ), Times.Once);

        result!.Name.Should().Be(input.Name);
        result.OwnerId.Should().Be(ownerId);
        result.Id.Should().Be(addedBike.Id);
        result.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UpdateAsync_WhenBikeDoesNotExist_ReturnsNull_AndDoesNotSave()
    {
        // Arrange
        _bikeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Bike?)null);
        var sut = new BikeService(_mapper, _bikeRepoMock.Object, _partServiceMock.Object, _userRepoMock.Object);
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
        var sut = new BikeService(_mapper, _bikeRepoMock.Object, _partServiceMock.Object, _userRepoMock.Object);
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
        var sut = new BikeService(_mapper, _bikeRepoMock.Object, _partServiceMock.Object, _userRepoMock.Object);

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
        var sut = new BikeService(_mapper, _bikeRepoMock.Object, _partServiceMock.Object, _userRepoMock.Object);

        // Act
        var ok = await sut.DeleteAsync(existing.Id);

        // Assert
        ok.Should().BeTrue();
        _bikeRepoMock.Verify(r => r.Remove(existing), Times.Once);
        _bikeRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

}