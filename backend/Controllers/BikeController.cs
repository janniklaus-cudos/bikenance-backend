using Backend.Services;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BikeController : ControllerBase
{
	private readonly IBikeService _bikeService;

	public BikeController(IBikeService bikeService)
	{
		_bikeService = bikeService;
	}

	[HttpGet]
	public async Task<IActionResult> GetAllBikes()
	{
		var bikes = await _bikeService.GetAllAsync();
		return Ok(bikes);
	}

	[HttpPost]
	public async Task<IActionResult> AddBike([FromBody] Bike bike)
	{
		var createdBike = await _bikeService.AddAsync(bike);
		return CreatedAtAction(nameof(GetAllBikes), new { }, createdBike);
	}

	[HttpPut("{id}")]
	public async Task<IActionResult> UpdateBike(Guid id, [FromBody] Bike bike)
	{
		var updatedBike = await _bikeService.UpdateAsync(id, bike);
		if (updatedBike is null)
		{
			return NotFound();
		}

		return Ok(updatedBike);
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> DeleteBike(Guid id)
	{
		var deleted = await _bikeService.DeleteAsync(id);
		if (!deleted)
		{
			return NotFound();
		}

		return NoContent();
	}
}