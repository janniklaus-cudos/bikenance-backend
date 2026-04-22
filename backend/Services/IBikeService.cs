
using Backend.Dtos;
using Backend.Models;

namespace Backend.Services;

public interface IBikeService
{
    Task<BikeDto> GetByIdAsync();
    Task<List<BikeDto>> GetAllAsync();
    Task<BikeDto> AddAsync(BikeDto bike);
    Task<BikeDto?> UpdateAsync(Guid id, BikeDto bike);
    Task<bool> DeleteAsync(Guid id);
}
