using Backend.Dtos;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MaintenanceTasksController(IMaintenanceTaskService maintenanceTaskService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllByBikePartId([FromQuery] Guid bikePartId)
    {
        var tasks = await maintenanceTaskService.GetAllByBikePartIdAsync(bikePartId);
        if (tasks is null)
        {
            return NotFound();
        }

        return Ok(tasks);
    }

    [HttpPost]
    public async Task<IActionResult> AddMaintenanceTask([FromQuery] Guid bikePartId, [FromBody] MaintenanceTaskDto maintenanceTask)
    {
        var createdTask = await maintenanceTaskService.AddAsync(bikePartId, maintenanceTask);
        if (createdTask is null)
        {
            return NotFound();
        }

        return CreatedAtAction(nameof(GetAllByBikePartId), new { bikePartId }, createdTask);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMaintenanceTask(Guid id, [FromBody] MaintenanceTaskDto maintenanceTask)
    {
        var updatedTask = await maintenanceTaskService.UpdateAsync(id, maintenanceTask);
        if (updatedTask is null)
        {
            return NotFound();
        }

        return Ok(updatedTask);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMaintenanceTask(Guid id)
    {
        var deleted = await maintenanceTaskService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteAllByBikePartId([FromQuery] Guid bikePartId)
    {
        var deleted = await maintenanceTaskService.DeleteAllByBikePartIdAsync(bikePartId);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
