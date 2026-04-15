using Backend.Data;
using Backend.Services;
using Backend.Mapping;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ASP.NET Core services
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddOpenApi();

// EF Core configuration
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
       .UseLazyLoadingProxies());

// own services configuration
builder.Services.Scan(scan => scan
    .FromAssemblyOf<BikeService>()
    .AddClasses(classes => classes.Where(t => t.Name.EndsWith("Service")))
    .AsImplementedInterfaces()
    .WithScopedLifetime()
);
builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

// Auto Mapper configuration
builder.Services.AddAutoMapper(
    typeof(BikeProfile),
    typeof(BikePartProfile),
    typeof(MaintenanceTaskProfile),
    typeof(ServiceEventProfile)
    );

var app = builder.Build();

// only in development: swagger and db seeding
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db);
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
