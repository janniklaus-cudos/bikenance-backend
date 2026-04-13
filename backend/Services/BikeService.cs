using AutoMapper.QueryableExtensions;
using Backend.Data;
using Backend.Models;
using Backend.Dtos;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace Backend.Services;

public class BikeService : IBikeService
{
	private readonly AppDbContext _db;
	private readonly IMapper _mapper;

	public BikeService(AppDbContext db, IMapper mapper)
	{
		_db = db;
		_mapper = mapper;
	}

	public async Task<List<BikeDto>> GetAllAsync()
	{
		return await _db.Bikes
		.ProjectTo<BikeDto>(_mapper.ConfigurationProvider)
		.ToListAsync();
	}

	public async Task<Bike> AddAsync(Bike bike)
	{
		if (bike.Id == Guid.Empty)
		{
			bike.Id = Guid.NewGuid();
		}

		foreach (var part in bike.Parts)
		{
			part.Bike = bike;
		}

		_db.Bikes.Add(bike);
		await _db.SaveChangesAsync();

		return bike;
	}

	public async Task<Bike?> UpdateAsync(Guid id, Bike bike)
	{
		var existingBike = await _db.Bikes
			.Include(b => b.Parts)
			.FirstOrDefaultAsync(b => b.Id == id);

		if (existingBike == null)
		{
			return null;
		}

		existingBike.Name = bike.Name;
		existingBike.Brand = bike.Brand;
		existingBike.IconId = bike.IconId;

		existingBike.Parts = bike.Parts ?? new List<BikePart>();
		foreach (var part in existingBike.Parts)
		{
			part.Bike = existingBike;
		}

		await _db.SaveChangesAsync();

		return existingBike;
	}

	public async Task<bool> DeleteAsync(Guid id)
	{
		var bike = await _db.Bikes.FindAsync(id);
		if (bike == null)
		{
			return false;
		}

		_db.Bikes.Remove(bike);
		await _db.SaveChangesAsync();

		return true;
	}
}
