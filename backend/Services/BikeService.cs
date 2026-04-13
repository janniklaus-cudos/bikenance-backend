using AutoMapper.QueryableExtensions;
using Backend.Data;
using Backend.Models;
using Backend.Dtos;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace Backend.Services;

public class BikeService(IMapper mapper, IRepository<Bike> bikeRepository, IBikePartService bikePartService) : IBikeService
{
    public async Task<List<BikeDto>> GetAllAsync()
    {
        return await bikeRepository.Query()
            .ProjectTo<BikeDto>(mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<BikeDto> AddAsync(BikeDto bike)
    {
        var createdBike = new Bike
        {
            Name = bike.Name,
            Brand = bike.Brand,
            IconId = bike.IconId
        };

        await bikePartService.AddAllByBikeIdAsync(createdBike.Id, bike.Parts);

        bikeRepository.Add(createdBike);
        await bikeRepository.SaveChangesAsync();

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

        await bikePartService.UpdateAllAsync(id, bike.Parts);

        bikeRepository.Update(existingBike);
        await bikeRepository.SaveChangesAsync();

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
