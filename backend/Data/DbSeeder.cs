

using Backend.Data;

namespace bikenance.Data;

public static class DbSeeder
{
	public static async Task SeedAsync(AppDbContext db)
	{
		if (await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AnyAsync(db.Bikes))
		{
			return;
		}

		var roadBike = new Bike
		{
			Id = Guid.NewGuid(),
			Name = "Morning Roadster",
			Brand = "Velociti",
			IconId = 1,
			Parts = new List<BikePart>
			{
				new BikePart { Id = Guid.NewGuid(), Name = "Front Wheel Tire", Position = BikePartPosition.FrontWheelTire },
				new BikePart { Id = Guid.NewGuid(), Name = "Rear Wheel Tire", Position = BikePartPosition.RearWheelTire },
				new BikePart { Id = Guid.NewGuid(), Name = "Chain", Position = BikePartPosition.Chain },
				new BikePart { Id = Guid.NewGuid(), Name = "Front Brakes", Position = BikePartPosition.FrontBrakes },
				new BikePart { Id = Guid.NewGuid(), Name = "Rear Brakes", Position = BikePartPosition.RearBrakes },
			}
		};

		foreach (var part in roadBike.Parts)
		{
			part.Bike = roadBike;
		}

		var cityBike = new Bike
		{
			Id = Guid.NewGuid(),
			Name = "City Commuter",
			Brand = "UrbanRide",
			IconId = 2,
			Parts = new List<BikePart>
			{
				new BikePart { Id = Guid.NewGuid(), Name = "Front Wheel Tire", Position = BikePartPosition.FrontWheelTire },
				new BikePart { Id = Guid.NewGuid(), Name = "Rear Wheel Tire", Position = BikePartPosition.RearWheelTire },
				new BikePart { Id = Guid.NewGuid(), Name = "Saddle", Position = BikePartPosition.Saddle },
				new BikePart { Id = Guid.NewGuid(), Name = "Pedal", Position = BikePartPosition.Pedal },
				new BikePart { Id = Guid.NewGuid(), Name = "Rear Chain Wheel", Position = BikePartPosition.RearChainWheel },
			}
		};

		foreach (var part in cityBike.Parts)
		{
			part.Bike = cityBike;
		}

		var journeys = new[]
		{
			new Journey
			{
				Id = Guid.NewGuid(),
				Bike = roadBike,
				Title = "Saturday Long Ride",
				Kilometer = 42,
			},
			new Journey
			{
				Id = Guid.NewGuid(),
				Bike = roadBike,
				Title = "Sunday Morning Sprint",
				Kilometer = 18,
			},
			new Journey
			{
				Id = Guid.NewGuid(),
				Bike = cityBike,
				Title = "Daily Commute",
				Kilometer = 12,
			},
		};

		var maintenanceTasks = new[]
		{
			new MaintenanceTask
			{
				Id = Guid.NewGuid(),
				BikePart = roadBike.Parts.First(p => p.Position == BikePartPosition.Chain),
				Description = "Lubricate chain and inspect for wear.",
				TimeInterval = TimeSpan.FromDays(14),
				Cost = 10,
				Importance = ImportanceLevel.HIGH,
				IsActive = true,
			},
			new MaintenanceTask
			{
				Id = Guid.NewGuid(),
				BikePart = roadBike.Parts.First(p => p.Position == BikePartPosition.FrontBrakes),
				Description = "Check brake pads and replace if worn.",
				TimeInterval = TimeSpan.FromDays(30),
				Cost = 25,
				Importance = ImportanceLevel.MEDIUM,
				IsActive = true,
			},
			new MaintenanceTask
			{
				Id = Guid.NewGuid(),
				BikePart = cityBike.Parts.First(p => p.Position == BikePartPosition.RearChainWheel),
				Description = "Inspect rear chain wheel and tighten bolts.",
				TimeInterval = TimeSpan.FromDays(45),
				Cost = 15,
				Importance = ImportanceLevel.MEDIUM,
				IsActive = true,
			}
		};

		var serviceEvents = new[]
		{
			new ServiceEvent
			{
				Id = Guid.NewGuid(),
				BikePart = roadBike.Parts.First(p => p.Position == BikePartPosition.RearWheelTire),
				Description = "Replaced rear tire after puncture.",
				StateAfterService = 100,
				Cost = 28,
			},
			new ServiceEvent
			{
				Id = Guid.NewGuid(),
				BikePart = cityBike.Parts.First(p => p.Position == BikePartPosition.Saddle),
				Description = "Adjusted saddle height and tightened clamps.",
				StateAfterService = 95,
				Cost = 0,
			}
		};

		db.Bikes.AddRange(roadBike, cityBike);
		db.Journeys.AddRange(journeys);
		db.MaintenanceTasks.AddRange(maintenanceTasks);
		db.ServiceEvents.AddRange(serviceEvents);

		await db.SaveChangesAsync();
	}
}