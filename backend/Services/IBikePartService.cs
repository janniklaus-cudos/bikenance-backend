using Backend.Dtos;
using Backend.Models;

namespace Backend.Services;

public interface IBikePartService
{
    Task<BikePartDto?> GetByIdAsync(Guid id);
    Task<List<BikePartDto>?> GetAllByBikeIdAsync(Guid bikeId);
    Task<List<BikePartDto>?> AddAllByBikeIdAsync(Guid bikeId, List<BikePartCreateDto> bikeParts);
    Task<List<BikePartDto>?> UpdateAllAsync(List<BikePartDto> bikePart);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> DeleteAllByBikeIdAsync(Guid bikeId);
}
