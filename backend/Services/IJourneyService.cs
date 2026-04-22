using Backend.Dtos;

namespace Backend.Services;

public interface IJourneyService
{
    Task<JourneyDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<JourneyDto>?> GetAllByBikeIdAsync(Guid bikeId);
    Task<JourneyDto?> AddAsync(Guid bikeId, JourneyDto journey);
    Task<IEnumerable<JourneyDto>?> AddAllAsync(Guid bikeId, IEnumerable<JourneyDto> journeys);
    Task<JourneyDto?> UpdateAsync(Guid id, JourneyDto journey);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> DeleteAllByBikeIdAsync(Guid bikeId);
}