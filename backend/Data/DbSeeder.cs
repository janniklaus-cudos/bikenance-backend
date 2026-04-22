

using Backend.Models;
using Backend.Data;

namespace Backend.Data;

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
            Name = "Morning Roadster",
            Brand = "Velociti",
            IconId = 1,
            Parts = new List<BikePart>
            {
                new BikePart {  Name = "Front Wheel Tire", Position = BikePartPosition.FrontWheelTire },
                new BikePart {  Name = "Rear Wheel Tire", Position = BikePartPosition.RearWheelTire },
                new BikePart {  Name = "Chain", Position = BikePartPosition.Chain },
                new BikePart {  Name = "Front Brakes", Position = BikePartPosition.FrontBrakes },
                new BikePart {  Name = "Rear Brakes", Position = BikePartPosition.RearBrakes },
            },
            CreatedAtUtc = DateTime.UtcNow,
        };

        foreach (var part in roadBike.Parts)
        {
            part.Bike = roadBike;
        }

        var cityBike = new Bike
        {
            Name = "City Commuter",
            Brand = "UrbanRide",
            IconId = 2,
            Parts = new List<BikePart>
            {
                new BikePart {  Name = "Front Wheel Tire", Position = BikePartPosition.FrontWheelTire },
                new BikePart {  Name = "Rear Wheel Tire", Position = BikePartPosition.RearWheelTire },
                new BikePart {  Name = "Saddle", Position = BikePartPosition.Saddle },
                new BikePart {  Name = "Pedal", Position = BikePartPosition.Pedals },
                new BikePart {  Name = "Rear Chain Wheel", Position = BikePartPosition.RearChainWheel },
            },
            CreatedAtUtc = DateTime.UtcNow - TimeSpan.FromDays(30),
        };

        foreach (var part in cityBike.Parts)
        {
            part.Bike = cityBike;
        }

        var journeys = new[]
        {
            new Journey
            {
                Bike = roadBike,
                Title = "Saturday Long Ride",
                Kilometer = 42,
            },
            new Journey
            {
                Bike = roadBike,
                Title = "Sunday Morning Sprint",
                Kilometer = 18,
            },
            new Journey
            {
                Bike = cityBike,
                Title = "Daily Commute",
                Kilometer = 12,
            },
        };

        var maintenanceTasks = new[]
        {
            new MaintenanceTask
            {
                BikePart = roadBike.Parts.First(p => p.Position == BikePartPosition.Chain),
                Description = "Lubricate chain and inspect for wear.",
                DaysInterval = 14,
                Cost = 10,
                Importance = ImportanceLevel.HIGH,
                IsActive = true,
            },
            new MaintenanceTask
            {
                BikePart = roadBike.Parts.First(p => p.Position == BikePartPosition.FrontBrakes),
                Description = "Check brake pads and replace if worn.",
                DaysInterval = 30,
                Cost = 25,
                Importance = ImportanceLevel.MEDIUM,
                IsActive = true,
            },
            new MaintenanceTask
            {
                BikePart = cityBike.Parts.First(p => p.Position == BikePartPosition.RearChainWheel),
                Description = "Inspect rear chain wheel and tighten bolts.",
                DaysInterval = 45,
                Cost = 15,
                Importance = ImportanceLevel.MEDIUM,
                IsActive = true,
            }
        };

        var serviceEvents = new[]
        {
            new ServiceEvent
            {
                BikePart = roadBike.Parts.First(p => p.Position == BikePartPosition.RearWheelTire),
                Description = "Replaced rear tire after puncture.",
                StateAfterService = 100,
                Cost = 28,
            },
            new ServiceEvent
            {
                BikePart = cityBike.Parts.First(p => p.Position == BikePartPosition.Saddle),
                Description = "Adjusted saddle height and tightened clamps.",
                StateAfterService = 95,
                Cost = 0,
            },
            new ServiceEvent
            {
                BikePart = roadBike.Parts.First(p => p.Position == BikePartPosition.Chain),
                Description = "Cleaned and lubricated chain, replaced damaged links.",
                StateAfterService = 90,
                Cost = 15,
            },
            new ServiceEvent
            {
                BikePart = roadBike.Parts.First(p => p.Position == BikePartPosition.FrontBrakes),
                Description = "Replaced worn brake pads and adjusted brake cable tension.",
                StateAfterService = 100,
                Cost = 35,
            },
            new ServiceEvent
            {
                BikePart = cityBike.Parts.First(p => p.Position == BikePartPosition.FrontWheelTire),
                Description = "Patched puncture in front tire.",
                StateAfterService = 85,
                Cost = 8,
            },
            new ServiceEvent
            {
                BikePart = cityBike.Parts.First(p => p.Position == BikePartPosition.Pedals),
                Description = "Repaired pedal bearing and replaced cracked plastic platform.",
                StateAfterService = 80,
                Cost = 20,
            }
        };

        db.Bikes.AddRange(roadBike, cityBike);
        db.Journeys.AddRange(journeys);
        db.MaintenanceTasks.AddRange(maintenanceTasks);
        db.ServiceEvents.AddRange(serviceEvents);

        await db.SaveChangesAsync();
    }
}