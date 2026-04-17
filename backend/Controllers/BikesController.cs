using Backend.Services;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Backend.Dtos;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BikesController(IBikeService bikeService) : ControllerBase
{

    [HttpGet]
    public async Task<IActionResult> GetAllBikes()
    {
        var bikes = await bikeService.GetAllAsync();
        return Ok(bikes);
    }

    [HttpPost]
    public async Task<IActionResult> AddBike([FromBody] BikeCreateDto bike)
    {
        var createdBike = await bikeService.AddAsync(bike);
        return CreatedAtAction(nameof(GetAllBikes), new { }, createdBike);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBike(Guid id, [FromBody] BikeDto bike)
    {
        var updatedBike = await bikeService.UpdateAsync(id, bike);
        if (updatedBike is null)
        {
            return NotFound();
        }

        return Ok(updatedBike);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBike(Guid id)
    {
        var deleted = await bikeService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}