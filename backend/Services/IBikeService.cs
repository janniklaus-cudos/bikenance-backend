
using Backend.Dtos;
using Backend.Models;

namespace Backend.Services;

public interface IBikeService
{
    Task<List<BikeDto>> GetAllAsync();
    Task<BikeDto> AddAsync(Bike bike);
    Task<BikeDto?> UpdateAsync(Guid id, Bike bike);
    Task<bool> DeleteAsync(Guid id);
}
