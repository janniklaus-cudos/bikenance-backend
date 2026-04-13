using Backend.Dtos;
using Backend.Models;

namespace Backend.Services;

public interface IBikePartService
{
    Task<BikePartDto?> GetByIdAsync(Guid id);
    Task<List<BikePartDto>?> GetAllByBikeIdAsync(Guid bikeId);
    Task<BikePart?> AddAsync(Guid bikeId, BikePart bikePart);
    Task<List<BikePart>?> AddAllByBikeIdAsync(Guid bikeId, List<BikePart> bikeParts);
    Task<BikePart?> UpdateAsync(Guid id, BikePart bikePart);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> DeleteAllByBikeIdAsync(Guid bikeId);
}
