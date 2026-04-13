
using Backend.Dtos;
using Backend.Models;

namespace Backend.Services;

public interface IBikeService
{
	Task<List<BikeDto>> GetAllAsync();
	Task<Bike> AddAsync(Bike bike);
	Task<Bike?> UpdateAsync(Guid id, Bike bike);
	Task<bool> DeleteAsync(Guid id);
}
