using AutoMapper;
using AutoMapper.QueryableExtensions;
using Backend.Data;
using Backend.Dtos;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class MaintenanceTaskService(IMapper mapper, IRepository<MaintenanceTask> maintenanceTaskRepository, IRepository<BikePart> bikePartRepository) : IMaintenanceTaskService
{
    public async Task<List<MaintenanceTaskDto>?> GetAllByBikePartIdAsync(Guid bikePartId)
    {
        if (!await maintenanceTaskRepository.Query().AnyAsync(bp => bp.Id == bikePartId))
        {
            return null;
        }

        return await maintenanceTaskRepository.Query()
            .Where(mt => mt.BikePart.Id == bikePartId)
            .ProjectTo<MaintenanceTaskDto>(mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<MaintenanceTaskDto?> AddAsync(Guid bikePartId, MaintenanceTaskDto maintenanceTask)
    {
        var bikePart = await bikePartRepository.GetByIdAsync(bikePartId);
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

        maintenanceTaskRepository.Add(createdMaintenanceTask);
        await maintenanceTaskRepository.SaveChangesAsync();

        return mapper.Map<MaintenanceTaskDto>(createdMaintenanceTask);
    }

    public async Task<MaintenanceTaskDto?> UpdateAsync(Guid id, MaintenanceTaskDto maintenanceTask)
    {
        var existingTask = await maintenanceTaskRepository.GetByIdAsync(id);

        if (existingTask == null)
        {
            return null;
        }

        existingTask.Description = maintenanceTask.Description;
        existingTask.TimeInterval = maintenanceTask.TimeInterval;
        existingTask.Cost = maintenanceTask.Cost;
        existingTask.Importance = maintenanceTask.Importance;
        existingTask.IsActive = maintenanceTask.IsActive;

        maintenanceTaskRepository.Update(existingTask);
        await maintenanceTaskRepository.SaveChangesAsync();

        return mapper.Map<MaintenanceTaskDto>(existingTask);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var task = await maintenanceTaskRepository.GetByIdAsync(id);
        if (task == null)
        {
            return false;
        }

        maintenanceTaskRepository.Remove(task);
        await maintenanceTaskRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAllByBikePartIdAsync(Guid bikePartId)
    {
        if (!await bikePartRepository.Query().AnyAsync(bp => bp.Id == bikePartId))
        {
            return false;
        }

        var tasks = await maintenanceTaskRepository.Query()
            .Where(mt => mt.BikePart.Id == bikePartId)
            .ToListAsync();

        if (tasks.Count == 0)
        {
            return true;
        }

        maintenanceTaskRepository.RemoveRange(tasks);
        await maintenanceTaskRepository.SaveChangesAsync();

        return true;
    }
}
