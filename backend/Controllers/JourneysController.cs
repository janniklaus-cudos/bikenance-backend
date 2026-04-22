using Backend.Dtos;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JourneysController(IJourneyService journeyService) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var journey = await journeyService.GetByIdAsync(id);
        if (journey is null)
        {
            return NotFound();
        }

        return Ok(journey);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllByBikeId([FromQuery] Guid bikeId)
    {
        var journeys = await journeyService.GetAllByBikeIdAsync(bikeId);
        if (journeys is null)
        {
            return NotFound();
        }

        return Ok(journeys);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromQuery] Guid bikeId, [FromBody] JourneyDto journey)
    {
        var createdJourney = await journeyService.AddAsync(bikeId, journey);
        if (createdJourney is null)
        {
            return NotFound();
        }

        return CreatedAtAction(nameof(GetAllByBikeId), new { bikeId }, createdJourney);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] JourneyDto journey)
    {
        var updatedJourney = await journeyService.UpdateAsync(id, journey);
        if (updatedJourney is null)
        {
            return NotFound();
        }

        return Ok(updatedJourney);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await journeyService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteAllByBikeId([FromQuery] Guid bikeId)
    {
        var deleted = await journeyService.DeleteAllByBikeIdAsync(bikeId);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}