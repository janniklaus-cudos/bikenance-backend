using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
	public DbSet<Bike> Bikes => Set<Bike>();
	public DbSet<Journey> Journeys => Set<Journey>();
	public DbSet<MaintenanceTask> MaintenanceTasks => Set<MaintenanceTask>();
	public DbSet<ServiceEvent> ServiceEvents => Set<ServiceEvent>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<MaintenanceTask>()
			.Property(x => x.Importance)
			.HasConversion<string>();

		modelBuilder.Entity<BikePart>()
		.Property(x => x.Position)
		.HasConversion<string>();
	}
}