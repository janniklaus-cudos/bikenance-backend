using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EvaluationController(IEvaluationService evaluationService) : ControllerBase
{
    [HttpGet("bike/{bikeId}")]
    public async Task<IActionResult> GetBikeEvaluation(Guid bikeId)
    {
        var bikeEvaluation = await evaluationService.EvaluateBikeAsync(bikeId);
        if (bikeEvaluation is null)
        {
            return NotFound();
        }

        return Ok(bikeEvaluation);
    }

    [HttpGet("bikePart/{bikePartId}")]
    public async Task<IActionResult> GetBikePartEvaluation(Guid bikePartId)
    {
        var bikePartEvaluation = await evaluationService.EvaluateBikePartAsync(bikePartId);
        if (bikePartEvaluation is null)
        {
            return NotFound();
        }

        return Ok(bikePartEvaluation);
    }

    [HttpGet("bikePartPositionStatus/{bikeId}")]
    public async Task<IActionResult> GetBikePartPositionStatus(Guid bikeId)
    {
        var bikePartStatusColors = await evaluationService.EvaluateBikePartPositionStatusAsync(bikeId);
        if (bikePartStatusColors == null)
        {
            return NotFound();
        }

        return Ok(bikePartStatusColors);
    }

}