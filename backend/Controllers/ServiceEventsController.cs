using Backend.Services;
using Backend.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServiceEventsController(IServiceEventService serviceEventService) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetServiceEvent(Guid id)
    {
        var serviceEvent = await serviceEventService.GetByIdAsync(id);
        if (serviceEvent is null)
        {
            return NotFound();
        }

        return Ok(serviceEvent);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllServiceEvents([FromQuery] Guid? bikePartId, [FromQuery] Guid? bikeId)
    {
        if (bikePartId.HasValue)
        {
            var serviceEvents = await serviceEventService.GetAllByBikePartIdAsync(bikePartId.Value);
            if (serviceEvents is null)
            {
                return NotFound();
            }

            return Ok(serviceEvents);
        }

        if (bikeId.HasValue)
        {
            var serviceEvents = await serviceEventService.GetAllByBikeIdAsync(bikeId.Value);
            if (serviceEvents is null)
            {
                return NotFound();
            }

            return Ok(serviceEvents);
        }

        return BadRequest("Either bikePartId or bikeId must be provided.");
    }

    [HttpPost]
    public async Task<IActionResult> AddServiceEvent([FromQuery] Guid bikePartId, [FromBody] ServiceEventDto serviceEvent)
    {
        var createdServiceEvent = await serviceEventService.AddAsync(bikePartId, serviceEvent);
        if (createdServiceEvent is null)
        {
            return NotFound();
        }

        return CreatedAtAction(nameof(GetServiceEvent), new { id = createdServiceEvent.Id }, createdServiceEvent);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateServiceEvent(Guid id, [FromBody] ServiceEventDto serviceEvent)
    {
        var updatedServiceEvent = await serviceEventService.UpdateAsync(id, serviceEvent);
        if (updatedServiceEvent is null)
        {
            return NotFound();
        }

        return Ok(updatedServiceEvent);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteServiceEvent(Guid id)
    {
        var deleted = await serviceEventService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteAllServiceEvents([FromQuery] Guid? bikePartId, [FromQuery] Guid? bikeId)
    {
        if (bikePartId.HasValue)
        {
            var deleted = await serviceEventService.DeleteAllByBikePartIdAsync(bikePartId.Value);
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }

        if (bikeId.HasValue)
        {
            var deleted = await serviceEventService.DeleteAllByBikeIdAsync(bikeId.Value);
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }

        return BadRequest("Either bikePartId or bikeId must be provided.");
    }
}
