using AutoMapper;
using AutoMapper.QueryableExtensions;
using Backend.Data;
using Backend.Dtos;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class BikePartService(AppDbContext db, IMapper mapper) : IBikePartService
{

    public async Task<BikePartDto?> GetByIdAsync(Guid id)
    {
        var bikePart = await db.BikeParts
            .Where(bp => bp.Id == id)
            .ProjectTo<BikePartDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        return bikePart;
    }

    public async Task<List<BikePartDto>?> GetAllByBikeIdAsync(Guid bikeId)
    {
        if (!await db.Bikes.AnyAsync(b => b.Id == bikeId))
        {
            return null;
        }

        return await db.BikeParts
            .Where(bp => bp.Bike.Id == bikeId)
            .ProjectTo<BikePartDto>(mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<BikePart?> AddAsync(Guid bikeId, BikePart bikePart)
    {
        var bike = await db.Bikes.FindAsync(bikeId);
        if (bike == null)
        {
            return null;
        }

        if (bikePart.Id == Guid.Empty)
        {
            bikePart.Id = Guid.NewGuid();
        }

        bikePart.Bike = bike;
        db.BikeParts.Add(bikePart);
        await db.SaveChangesAsync();

        return bikePart;
    }

    public async Task<List<BikePart>?> AddAllByBikeIdAsync(Guid bikeId, List<BikePart> bikeParts)
    {
        var bike = await db.Bikes.FindAsync(bikeId);
        if (bike == null)
        {
            return null;
        }

        foreach (var part in bikeParts)
        {
            if (part.Id == Guid.Empty)
            {
                part.Id = Guid.NewGuid();
            }

            part.Bike = bike;
        }

        db.BikeParts.AddRange(bikeParts);
        await db.SaveChangesAsync();

        return bikeParts;
    }

    public async Task<BikePart?> UpdateAsync(Guid id, BikePart bikePart)
    {
        var existingPart = await db.BikeParts
            .Include(bp => bp.Bike)
            .FirstOrDefaultAsync(bp => bp.Id == id);

        if (existingPart == null)
        {
            return null;
        }

        existingPart.Name = bikePart.Name;
        existingPart.Position = bikePart.Position;

        await db.SaveChangesAsync();

        return existingPart;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var part = await db.BikeParts.FindAsync(id);
        if (part == null)
        {
            return false;
        }

        db.BikeParts.Remove(part);
        await db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAllByBikeIdAsync(Guid bikeId)
    {
        if (!await db.Bikes.AnyAsync(b => b.Id == bikeId))
        {
            return false;
        }

        var parts = await db.BikeParts
            .Where(bp => bp.Bike.Id == bikeId)
            .ToListAsync();

        if (parts.Count == 0)
        {
            return true;
        }

        db.BikeParts.RemoveRange(parts);
        await db.SaveChangesAsync();

        return true;
    }
}
