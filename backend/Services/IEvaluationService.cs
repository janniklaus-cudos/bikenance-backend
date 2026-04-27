using Backend.Dtos;

namespace Backend.Services;

public interface IEvaluationService
{
    Task<BikePartEvaluationDto?> EvaluateBikePartAsync(Guid bikePartId);

    Task<List<BikePartPositionStatus>?> EvaluateBikePartPositionStatusAsync(Guid bikeId);

}