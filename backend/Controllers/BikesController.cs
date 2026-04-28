using Backend.Services;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Backend.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BikesController(IBikeService bikeService) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var bike = await bikeService.GetByIdAsync(id);
        if (bike == null)
        {
            return NotFound();
        }

        return Ok(bike);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var bikes = await bikeService.GetAllAsync();

        return Ok(bikes);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] BikeDto bike)
    {
        var createdBike = await bikeService.AddAsync(bike);

        return CreatedAtAction(nameof(GetAll), new { }, createdBike);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] BikeDto bike)
    {
        var updatedBike = await bikeService.UpdateAsync(id, bike);
        if (updatedBike is null)
        {
            return NotFound();
        }

        return Ok(updatedBike);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        // todo implement cascade to delete everything else
        var deleted = await bikeService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}