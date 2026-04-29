using Backend.Dtos;
using Backend.Repositories;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RepeatJourneysController(IRepeatJourneyService repeatJourneyService) : ControllerBase
{

    [HttpGet]
    public async Task<IActionResult> GetAllByBikeId([FromQuery] Guid bikeId)
    {
        var repeatJourneys = await repeatJourneyService.GetAllByBikeIdAsync(bikeId);
        if (repeatJourneys is null)
        {
            return NotFound();
        }

        return Ok(repeatJourneys);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromQuery] Guid bikeId, [FromBody] RepeatJourneyDto journey)
    {
        var createdRepeatJourney = await repeatJourneyService.AddAsync(bikeId, journey);
        if (createdRepeatJourney is null)
        {
            return NotFound();
        }

        return CreatedAtAction(nameof(GetAllByBikeId), new { bikeId }, createdRepeatJourney);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] RepeatJourneyDto journey)
    {
        var updatedRepeatJourney = await repeatJourneyService.UpdateAsync(id, journey);
        if (updatedRepeatJourney is null)
        {
            return NotFound();
        }

        return Ok(updatedRepeatJourney);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await repeatJourneyService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}