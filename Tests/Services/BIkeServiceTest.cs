using System.ComponentModel;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Backend.Data;
using Backend.Dtos;
using Backend.Models;
using Backend.Services;
using FluentAssertions;
using Moq;

namespace Tests.Services;

public class BikeServiceTests
{
    private readonly IMapper _mapper;

    public BikeServiceTests()
    {
        var cfg = new MapperConfiguration(c =>
        {
            c.CreateMap<Bike, BikeDto>()
             .ForMember(d => d.Parts, o => o.MapFrom(_ => new List<BikePartDto>()));
        });

        cfg.AssertConfigurationIsValid();
        _mapper = cfg.CreateMapper();
    }

    [Fact, Description("as GetAllAsync relies on .ProjectTo we cannot test this in a unit test")]
    public async Task GetAllAsync_ReturnsMappedDtos()
    {
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
            Parts = [new BikePartDto() { Name = "Chain", Position = BikePartPosition.Chain, BikeId = Guid.Empty }]
        };

        var repo = new Mock<IRepository<Bike>>();
        var partService = new Mock<IBikePartService>();

        repo.Setup(r => r.Add(It.IsAny<Bike>()));
        repo.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);


        partService.Setup(s => s.AddAllByBikeIdAsync(
                input.Id,
                input.Parts))
            .ReturnsAsync([]);

        var sut = new BikeService(_mapper, repo.Object, partService.Object);

        // Act
        var result = await sut.AddAsync(input);

        // Assert
        repo.Verify(r => r.Add(It.Is<Bike>(b =>
            b.Name == input.Name &&
            b.Brand == input.Brand &&
            b.IconId == input.IconId
        )), Times.Once);

        partService.Verify(s => s.AddAllByBikeIdAsync(
            result.Id,
            It.Is<List<BikePartDto>>(p => p.Count == input.Parts.Count)
        ), Times.Once);

        repo.Verify(r => r.SaveChangesAsync(), Times.Once);

        result.Name.Should().Be(input.Name);
        result.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UpdateAsync_WhenBikeDoesNotExist_ReturnsNull_AndDoesNotSave()
    {
        // Arrange
        var repo = new Mock<IRepository<Bike>>();
        var partService = new Mock<IBikePartService>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Bike?)null);
        var sut = new BikeService(_mapper, repo.Object, partService.Object);
        var input = new BikeDto { Name = "X", Brand = "Y", IconId = 1, Parts = [] };

        // Act
        var result = await sut.UpdateAsync(Guid.NewGuid(), input);

        // Assert
        result.Should().BeNull();
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        repo.Verify(r => r.Update(It.IsAny<Bike>()), Times.Never);
        partService.Verify(s => s.UpdateAllAsync(It.IsAny<Guid>(), It.IsAny<List<BikePartDto>>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenBikeExists_UpdatesFields_UpdatesParts_SavesAndReturnsDto()
    {
        // Arrange
        var existing = new Bike { Id = Guid.NewGuid(), Name = "Old", Brand = "OldBrand", IconId = 0, Parts = [] };
        var repo = new Mock<IRepository<Bike>>();
        var partService = new Mock<IBikePartService>();
        repo.Setup(r => r.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        partService.Setup(s => s.UpdateAllAsync(existing.Id, It.IsAny<List<BikePartDto>>())).ReturnsAsync([]);
        repo.Setup(r => r.Update(existing));
        repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = new BikeService(_mapper, repo.Object, partService.Object);
        var input = new BikeDto { Name = "New", Brand = "NewBrand", IconId = 7, Parts = [] };

        // Act
        var result = await sut.UpdateAsync(existing.Id, input);

        // Assert
        existing.Name.Should().Be("New");
        existing.Brand.Should().Be("NewBrand");
        existing.IconId.Should().Be(7);
        partService.Verify(s => s.UpdateAllAsync(existing.Id, input.Parts), Times.Once);
        repo.Verify(r => r.Update(existing), Times.Once);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        result.Should().NotBeNull();
        result!.Name.Should().Be("New");
    }

    [Fact]
    public async Task DeleteAsync_WhenBikeNotFound_ReturnsFalse()
    {
        // Arrange
        var repo = new Mock<IRepository<Bike>>();
        var partService = new Mock<IBikePartService>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Bike?)null);
        var sut = new BikeService(_mapper, repo.Object, partService.Object);

        // Act
        var ok = await sut.DeleteAsync(Guid.NewGuid());

        // Assert
        ok.Should().BeFalse();
        repo.Verify(r => r.Remove(It.IsAny<Bike>()), Times.Never);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenBikeFound_RemovesAndReturnsTrue()
    {
        // Arrange
        var existing = new Bike { Id = Guid.NewGuid(), Name = "B", Brand = "X", IconId = 0, Parts = [] };
        var repo = new Mock<IRepository<Bike>>();
        var partService = new Mock<IBikePartService>();
        repo.Setup(r => r.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        repo.Setup(r => r.Remove(existing));
        repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = new BikeService(_mapper, repo.Object, partService.Object);

        // Act
        var ok = await sut.DeleteAsync(existing.Id);

        // Assert
        ok.Should().BeTrue();
        repo.Verify(r => r.Remove(existing), Times.Once);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

}