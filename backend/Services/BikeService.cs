using Backend.Data;
using Backend.Models;
using Backend.Dtos;
using AutoMapper;
using Backend.Repositories;
using System.Transactions;

namespace Backend.Services;

public interface IBikeService
{
    Task<BikeDto> GetByIdAsync(Guid id);
    Task<List<BikeDto>> GetAllAsync();
    Task<BikeDto?> AddAsync(BikeDto bike);
    Task<BikeDto?> UpdateAsync(Guid id, BikeDto bike);
    Task<bool> DeleteAsync(Guid id);
}

public class BikeService(IMapper mapper, IBikeRepository bikeRepository, IBikePartService bikePartService, IUserRepository userRepository) : IBikeService
{
    public async Task<BikeDto> GetByIdAsync(Guid id)
    {
        var bike = await bikeRepository.GetByIdAsync(id);

        return mapper.Map<BikeDto>(bike);
    }

    public async Task<List<BikeDto>> GetAllAsync()
    {
        var bikes = await bikeRepository.GetAllAsync();

        return mapper.Map<List<BikeDto>>(bikes);
    }

    public async Task<BikeDto?> AddAsync(BikeDto bike)
    {
        var owner = await userRepository.GetByIdAsync(bike.OwnerId);
        if (owner == null)
        {
            return null;
        }

        var createdBike = new Bike
        {
            Name = bike.Name,
            Brand = bike.Brand,
            IconId = bike.IconId,
            Price = bike.Price,
            DateOfPurchase = bike.DateOfPurchase,
            Owner = owner
        };

        bikeRepository.Add(createdBike);
        await bikeRepository.SaveChangesAsync();

        await bikePartService.AddAllByBikeIdAsync(createdBike.Id, bike.Parts);

        return mapper.Map<BikeDto>(createdBike);
    }

    public async Task<BikeDto?> UpdateAsync(Guid id, BikeDto bike)
    {
        var existingBike = await bikeRepository.GetByIdAsync(id);

        if (existingBike == null)
        {
            return null;
        }

        existingBike.Name = bike.Name;
        existingBike.Brand = bike.Brand;
        existingBike.IconId = bike.IconId;
        existingBike.Price = bike.Price;
        existingBike.DateOfPurchase = bike.DateOfPurchase;

        bikeRepository.Update(existingBike);
        await bikeRepository.SaveChangesAsync();

        await bikePartService.UpdateAllAsync(id, bike.Parts);

        return mapper.Map<BikeDto>(existingBike);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var bike = await bikeRepository.GetByIdAsync(id);
        if (bike == null)
        {
            return false;
        }

        bikeRepository.Remove(bike);
        await bikeRepository.SaveChangesAsync();

        return true;
    }
}
