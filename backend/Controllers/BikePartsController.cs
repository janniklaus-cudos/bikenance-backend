using Backend.Dtos;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BikePartsController(IBikePartService bikePartService) : ControllerBase
{

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var part = await bikePartService.GetByIdAsync(id);
        if (part is null)
        {
            return NotFound();
        }

        return Ok(part);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllByBikeId([FromQuery] Guid bikeId)
    {
        var parts = await bikePartService.GetAllByBikeIdAsync(bikeId);
        if (parts is null)
        {
            return NotFound();
        }

        return Ok(parts);
    }

    [HttpPost]
    public async Task<IActionResult> AddBikeParts([FromQuery] Guid bikeId, [FromBody] List<BikePartCreateDto> bikeParts)
    {
        var createdParts = await bikePartService.AddAllByBikeIdAsync(bikeId, bikeParts);
        if (createdParts is null)
        {
            return NotFound();
        }

        return CreatedAtAction(nameof(GetAllByBikeId), new { bikeId }, createdParts);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateBikePart([FromBody] List<BikePartDto> bikeParts)
    {
        var updatedPart = await bikePartService.UpdateAllAsync(bikeParts);
        if (updatedPart is null)
        {
            return NotFound();
        }

        return Ok(updatedPart);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBikePart(Guid id)
    {
        var deleted = await bikePartService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteAllByBikeId([FromQuery] Guid bikeId)
    {
        var deleted = await bikePartService.DeleteAllByBikeIdAsync(bikeId);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
