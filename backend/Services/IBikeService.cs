
using Backend.Dtos;
using Backend.Models;

namespace Backend.Services;

public interface IBikeService
{
    Task<List<BikeDto>> GetAllAsync();
    Task<BikeDto> AddAsync(BikeCreateDto bike);
    Task<BikeDto?> UpdateAsync(Guid id, BikeDto bike);
    Task<bool> DeleteAsync(Guid id);
}
