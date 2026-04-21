using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EvaluationController(IEvaluationService evaluationService) : ControllerBase
{
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

}