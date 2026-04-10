using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Bike> Bikes => Set<Bike>();
    // später ergänzen:
    // public DbSet<MaintenanceTask> MaintenanceTasks => Set<MaintenanceTask>();
    // public DbSet<ServiceEvent> ServiceEvents => Set<ServiceEvent>();
    // public DbSet<OdometerEntry> OdometerEntries => Set<OdometerEntry>();

}