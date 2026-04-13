using AutoMapper;
using AutoMapper.QueryableExtensions;
using Backend.Data;
using Backend.Dtos;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class BikePartService(IMapper mapper, IRepository<BikePart> bikePartRepository, IRepository<Bike> bikeRepository) : IBikePartService
{

    public async Task<BikePartDto?> GetByIdAsync(Guid id)
    {
        var bikePart = await bikePartRepository.Query()
            .Where(bp => bp.Id == id)
            .ProjectTo<BikePartDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        return bikePart;
    }

    public async Task<List<BikePartDto>?> GetAllByBikeIdAsync(Guid bikeId)
    {
        if (!await bikePartRepository.Query().AnyAsync(b => b.Id == bikeId))
        {
            return null;
        }

        return await bikePartRepository.Query()
            .Where(bp => bp.Bike.Id == bikeId)
            .ProjectTo<BikePartDto>(mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<BikePartDto?> AddAsync(Guid bikeId, BikePartDto bikePart)
    {
        var bike = await bikeRepository.GetByIdAsync(bikeId);
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

        bikePartRepository.Add(createdBikePart);
        await bikePartRepository.SaveChangesAsync();

        return mapper.Map<BikePartDto>(createdBikePart);
    }

    public async Task<List<BikePartDto>?> AddAllByBikeIdAsync(Guid bikeId, List<BikePartDto> bikeParts)
    {
        var bike = await bikeRepository.GetByIdAsync(bikeId);
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

        bikePartRepository.AddRange(createdBikeParts);
        await bikePartRepository.SaveChangesAsync();

        return mapper.Map<List<BikePartDto>>(createdBikeParts);
    }

    public async Task<List<BikePartDto>?> UpdateAllAsync(Guid id, List<BikePartDto> bikeParts)
    {
        var existingBikeParts = await bikePartRepository.Query()
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

        bikePartRepository.UpdateRange(existingBikeParts);
        await bikePartRepository.SaveChangesAsync();

        return mapper.Map<List<BikePartDto>>(existingBikeParts);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var part = await bikePartRepository.GetByIdAsync(id);
        if (part == null)
        {
            return false;
        }

        bikePartRepository.Remove(part);
        await bikePartRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAllByBikeIdAsync(Guid bikeId)
    {
        if (!await bikeRepository.Query().AnyAsync(b => b.Id == bikeId))
        {
            return false;
        }

        var parts = await bikePartRepository.Query()
            .Where(bp => bp.Bike.Id == bikeId)
            .ToListAsync();

        if (parts.Count == 0)
        {
            return true;
        }

        bikePartRepository.RemoveRange(parts);
        await bikePartRepository.SaveChangesAsync();

        return true;
    }
}
