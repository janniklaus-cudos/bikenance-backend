using AutoMapper.QueryableExtensions;
using Backend.Data;
using Backend.Models;
using Backend.Dtos;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace Backend.Services;

public class BikeService(AppDbContext db, IMapper mapper, IBikePartService bikePartService) : IBikeService
{
    public async Task<List<BikeDto>> GetAllAsync()
    {
        return await db.Bikes
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

        db.Bikes.Add(createdBike);
        await db.SaveChangesAsync();

        return mapper.Map<BikeDto>(createdBike);
    }

    public async Task<BikeDto?> UpdateAsync(Guid id, BikeDto bike)
    {
        var existingBike = await db.Bikes
            .Include(b => b.Parts)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (existingBike == null)
        {
            return null;
        }

        existingBike.Name = bike.Name;
        existingBike.Brand = bike.Brand;
        existingBike.IconId = bike.IconId;

        await bikePartService.UpdateAllAsync(id, bike.Parts);

        db.Bikes.Update(existingBike);
        await db.SaveChangesAsync();

        return mapper.Map<BikeDto>(existingBike);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var bike = await db.Bikes.FindAsync(id);
        if (bike == null)
        {
            return false;
        }

        db.Bikes.Remove(bike);
        await db.SaveChangesAsync();

        return true;
    }
}
