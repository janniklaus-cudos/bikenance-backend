using AutoMapper.QueryableExtensions;
using Backend.Data;
using Backend.Models;
using Backend.Dtos;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace Backend.Services;

public class BikeService(AppDbContext db, IMapper mapper) : IBikeService
{
    public async Task<List<BikeDto>> GetAllAsync()
    {
        return await db.Bikes
        .ProjectTo<BikeDto>(mapper.ConfigurationProvider)
        .ToListAsync();
    }

    public async Task<Bike> AddAsync(Bike bike)
    {
        if (bike.Id == Guid.Empty)
        {
            bike.Id = Guid.NewGuid();
        }

        foreach (var part in bike.Parts)
        {
            part.Bike = bike;
        }

        db.Bikes.Add(bike);
        await db.SaveChangesAsync();

        return bike;
    }

    public async Task<Bike?> UpdateAsync(Guid id, Bike bike)
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

        existingBike.Parts = bike.Parts ?? new List<BikePart>();
        foreach (var part in existingBike.Parts)
        {
            part.Bike = existingBike;
        }

        await db.SaveChangesAsync();

        return existingBike;
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
