using AutoMapper;
using AutoMapper.QueryableExtensions;
using Backend.Data;
using Backend.Dtos;
using Backend.Models;
using Backend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public interface IServiceEventService
{
    Task<ServiceEventDto?> GetByIdAsync(Guid id);
    Task<List<ServiceEventDto>?> GetAllByBikePartIdAsync(Guid bikePartId);
    Task<List<ServiceEventDto>?> GetAllByBikeIdAsync(Guid bikeId);
    Task<ServiceEventDto?> AddAsync(Guid bikePartId, ServiceEventDto serviceEvent);
    Task<ServiceEventDto?> UpdateAsync(Guid id, ServiceEventDto serviceEvent);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> DeleteAllByBikePartIdAsync(Guid bikePartId);
    Task<bool> DeleteAllByBikeIdAsync(Guid bikeId);
}

public class ServiceEventService(
    IMapper mapper,
    IServiceEventRepository serviceEventRepository,
    IBikePartRepository bikePartRepository,
    IBikeRepository bikeRepository) : IServiceEventService
{
    public async Task<ServiceEventDto?> GetByIdAsync(Guid id)
    {
        var serviceEvent = await serviceEventRepository.GetByIdAsync(id);
        if (serviceEvent == null)
        {
            return null;
        }

        return mapper.Map<ServiceEventDto>(serviceEvent);
    }

    public async Task<List<ServiceEventDto>?> GetAllByBikePartIdAsync(Guid bikePartId)
    {
        var serviceEvents = await serviceEventRepository.GetAllByBikePartIdAsync(bikePartId);
        if (serviceEvents == null)
        {
            return null;
        }

        return mapper.Map<List<ServiceEventDto>>(serviceEvents);
    }

    public async Task<List<ServiceEventDto>?> GetAllByBikeIdAsync(Guid bikeId)
    {
        var serviceEvents = await serviceEventRepository.GetAllByBikeIdAsync(bikeId);
        if (serviceEvents == null)
        {
            return null;
        }

        return mapper.Map<List<ServiceEventDto>>(serviceEvents);
    }

    public async Task<ServiceEventDto?> AddAsync(Guid bikePartId, ServiceEventDto serviceEvent)
    {
        var bikePart = await bikePartRepository.GetByIdAsync(bikePartId);
        if (bikePart == null)
        {
            return null;
        }

        var createdServiceEvent = new ServiceEvent
        {
            BikePart = bikePart,
            Description = serviceEvent.Description,
            StateAfterService = serviceEvent.StateAfterService,
            Cost = serviceEvent.Cost,
            DateOfService = serviceEvent.DateOfService,
        };

        serviceEventRepository.Add(createdServiceEvent);
        await serviceEventRepository.SaveChangesAsync();

        return mapper.Map<ServiceEventDto>(createdServiceEvent);
    }

    public async Task<ServiceEventDto?> UpdateAsync(Guid id, ServiceEventDto serviceEvent)
    {
        var existingEvent = await serviceEventRepository.GetByIdAsync(id);

        if (existingEvent == null)
        {
            return null;
        }

        existingEvent.Description = serviceEvent.Description;
        existingEvent.StateAfterService = serviceEvent.StateAfterService;
        existingEvent.Cost = serviceEvent.Cost;
        existingEvent.DateOfService = serviceEvent.DateOfService;

        serviceEventRepository.Update(existingEvent);
        await serviceEventRepository.SaveChangesAsync();

        return mapper.Map<ServiceEventDto>(existingEvent);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var serviceEvent = await serviceEventRepository.GetByIdAsync(id);
        if (serviceEvent == null)
        {
            return false;
        }

        serviceEventRepository.Remove(serviceEvent);
        await serviceEventRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAllByBikePartIdAsync(Guid bikePartId)
    {
        var bikePart = await bikePartRepository.GetByIdAsync(bikePartId);
        if (bikePart == null)
        {
            return false;
        }

        var serviceEvents = await serviceEventRepository.GetAllByBikePartIdAsync(bikePartId);
        if (serviceEvents.Count == 0)
        {
            return true;
        }

        serviceEventRepository.RemoveRange(serviceEvents);
        await serviceEventRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAllByBikeIdAsync(Guid bikeId)
    {
        var bike = await bikeRepository.GetByIdAsync(bikeId);
        if (bike == null)
        {
            return false;
        }

        var serviceEvents = await serviceEventRepository.GetAllByBikeIdAsync(bikeId);
        if (serviceEvents.Count == 0)
        {
            return true;
        }

        serviceEventRepository.RemoveRange(serviceEvents);
        await serviceEventRepository.SaveChangesAsync();

        return true;
    }
}
