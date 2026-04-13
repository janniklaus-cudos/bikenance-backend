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

    public async Task<BikePartDto?> AddAsync(Guid bikeId, BikePartDto bikePart)
    {
        var bike = await db.Bikes.FindAsync(bikeId);
        if (bike == null)
        {
            return null;
        }

        var createdBikePart = new BikePart
        {
            Name = bikePart.Name,
            Position = bikePart.Position,
            Bike = bike
        };

        db.BikeParts.Add(createdBikePart);
        await db.SaveChangesAsync();

        return mapper.Map<BikePartDto>(createdBikePart);
    }

    public async Task<List<BikePartDto>?> AddAllByBikeIdAsync(Guid bikeId, List<BikePartDto> bikeParts)
    {
        var bike = await db.Bikes.FindAsync(bikeId);
        if (bike == null)
        {
            return null;
        }

        var createdBikeParts = bikeParts.Select(bp => new BikePart
        {
            Name = bp.Name,
            Position = bp.Position,
            Bike = bike
        }).ToList();

        db.BikeParts.AddRange(createdBikeParts);
        await db.SaveChangesAsync();

        return mapper.Map<List<BikePartDto>>(createdBikeParts);
    }

    public async Task<List<BikePartDto>?> UpdateAllAsync(Guid id, List<BikePartDto> bikeParts)
    {
        var existingBikeParts = await db.BikeParts
            .Where(bp => bp.Bike.Id == id)
            .ToListAsync();

        foreach (var existingBikePart in existingBikeParts)
        {
            var updatedPart = bikeParts.FirstOrDefault(bp => bp.Id == existingBikePart.Id);
            if (updatedPart != null)
            {
                existingBikePart.Name = updatedPart.Name;
                existingBikePart.Position = updatedPart.Position;
            }
        }

        db.BikeParts.UpdateRange(existingBikeParts);
        await db.SaveChangesAsync();

        return mapper.Map<List<BikePartDto>>(existingBikeParts);
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
