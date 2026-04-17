using AutoMapper;
using AutoMapper.QueryableExtensions;
using Backend.Data;
using Backend.Dtos;
using Backend.Models;
using Backend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class BikePartService(IMapper mapper, IBikePartRepository bikePartRepository, IRepository<Bike> bikeRepository) : IBikePartService
{

    public async Task<BikePartDto?> GetByIdAsync(Guid id)
    {
        var bikePart = await bikePartRepository.GetByIdAsync(id);
        if (bikePart == null)
        {
            return null;
        }

        return mapper.Map<BikePartDto>(bikePart);
    }

    public async Task<List<BikePartDto>?> GetAllByBikeIdAsync(Guid bikeId)
    {
        var bikeParts = await bikePartRepository.GetAllByBikeIdAsync(bikeId);
        if (bikeParts == null)
        {
            return null;
        }

        return mapper.Map<List<BikePartDto>>(bikeParts);
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

    public async Task<List<BikePartDto>?> UpdateAllAsync(List<BikePartDto> bikeParts)
    {
        if (bikeParts == null || bikeParts.Count == 0)
        {
            return [];
        }

        var existingBikeParts = await bikePartRepository.GetAllByBikeIdAsync(bikeParts.First().BikeId);

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
        var bike = await bikeRepository.GetByIdAsync(bikeId);
        if (bike == null)
        {
            return false;
        }

        var bikeParts = await bikePartRepository.GetAllByBikeIdAsync(bikeId);
        if (bikeParts.Count == 0)
        {
            return true;
        }

        bikePartRepository.RemoveRange(bikeParts);
        await bikePartRepository.SaveChangesAsync();

        return true;
    }
}
