using System.ComponentModel.DataAnnotations;
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
        try
        {
            var repeatJourneys = await repeatJourneyService.GetAllByBikeIdAsync(bikeId);

            return Ok(repeatJourneys);
        }
        catch (ValidationException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromQuery] Guid bikeId, [FromBody] RepeatJourneyDto journey)
    {
        try
        {
            var createdRepeatJourney = await repeatJourneyService.AddAsync(bikeId, journey);

            return CreatedAtAction(nameof(GetAllByBikeId), new { bikeId }, createdRepeatJourney);
        }
        catch (ValidationException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] RepeatJourneyDto journey)
    {
        try
        {
            var updatedRepeatJourney = await repeatJourneyService.UpdateAsync(id, journey);

            return Ok(updatedRepeatJourney);
        }
        catch (ValidationException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await repeatJourneyService.DeleteAsync(id);

            return NoContent();
        }
        catch (ValidationException e)
        {
            return BadRequest(e.Message);
        }
    }
}