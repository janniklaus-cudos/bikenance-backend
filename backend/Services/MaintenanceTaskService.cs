using AutoMapper;
using AutoMapper.QueryableExtensions;
using Backend.Data;
using Backend.Dtos;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class MaintenanceTaskService(AppDbContext db, IMapper mapper) : IMaintenanceTaskService
{
    public async Task<List<MaintenanceTaskDto>?> GetAllByBikePartIdAsync(Guid bikePartId)
    {
        if (!await db.BikeParts.AnyAsync(bp => bp.Id == bikePartId))
        {
            return null;
        }

        return await db.MaintenanceTasks
            .Where(mt => mt.BikePart.Id == bikePartId)
            .ProjectTo<MaintenanceTaskDto>(mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<MaintenanceTaskDto?> AddAsync(Guid bikePartId, MaintenanceTaskDto maintenanceTask)
    {
        var bikePart = await db.BikeParts.FindAsync(bikePartId);
        if (bikePart == null)
        {
            return null;
        }

        var createdMaintenanceTask = new MaintenanceTask
        {
            BikePart = bikePart,
            Description = maintenanceTask.Description,
            TimeInterval = maintenanceTask.TimeInterval,
            Cost = maintenanceTask.Cost,
            Importance = maintenanceTask.Importance,
            IsActive = maintenanceTask.IsActive
        };

        db.MaintenanceTasks.Add(createdMaintenanceTask);
        await db.SaveChangesAsync();

        return mapper.Map<MaintenanceTaskDto>(createdMaintenanceTask);
    }

    public async Task<MaintenanceTaskDto?> UpdateAsync(Guid id, MaintenanceTaskDto maintenanceTask)
    {
        var existingTask = await db.MaintenanceTasks
            .Include(mt => mt.BikePart)
            .FirstOrDefaultAsync(mt => mt.Id == id);

        if (existingTask == null)
        {
            return null;
        }

        existingTask.Description = maintenanceTask.Description;
        existingTask.TimeInterval = maintenanceTask.TimeInterval;
        existingTask.Cost = maintenanceTask.Cost;
        existingTask.Importance = maintenanceTask.Importance;
        existingTask.IsActive = maintenanceTask.IsActive;

        await db.SaveChangesAsync();

        return mapper.Map<MaintenanceTaskDto>(existingTask);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var task = await db.MaintenanceTasks.FindAsync(id);
        if (task == null)
        {
            return false;
        }

        db.MaintenanceTasks.Remove(task);
        await db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAllByBikePartIdAsync(Guid bikePartId)
    {
        if (!await db.BikeParts.AnyAsync(bp => bp.Id == bikePartId))
        {
            return false;
        }

        var tasks = await db.MaintenanceTasks
            .Where(mt => mt.BikePart.Id == bikePartId)
            .ToListAsync();

        if (tasks.Count == 0)
        {
            return true;
        }

        db.MaintenanceTasks.RemoveRange(tasks);
        await db.SaveChangesAsync();

        return true;
    }
}
