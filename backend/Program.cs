using Backend.Data;
using Backend.Services;
using Backend.Mapping;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Backend.Repositories;
using AutoMapper;
using Backend.Integrations.Strava;

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
    // servicees
    .FromAssemblyOf<BikeService>()
    .AddClasses(classes => classes.Where(t => t.Name.EndsWith("Service")))
    .AsImplementedInterfaces()
    .WithScopedLifetime()
    // repositories
    .FromAssemblyOf<IBikeRepository>()
    .AddClasses(classes => classes.Where(t => t.Name.EndsWith("Repository")))
    .AsImplementedInterfaces()
    .WithScopedLifetime()
);
builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

// strava api integration
builder.Services.AddHttpClient<StravaClient>();
builder.Services.AddSingleton<StravaTokenStore>();

// CORS configuration
const string CORS_VITE_DEV = "ViteDev";
builder.Services.AddCors(o => o.AddPolicy(CORS_VITE_DEV, p =>
    p.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod()));

// Auto Mapper configuration
builder.Services.AddSingleton(provider =>
{
    var loggerFactory = provider.GetRequiredService<ILoggerFactory>();

    var config = new MapperConfiguration(cfg =>
    {
        cfg.AddMaps(typeof(BikeProfile).Assembly);
    }, loggerFactory);

    // only during DEV is this recommended
    config.AssertConfigurationIsValid();

    return config;
});

builder.Services.AddScoped<IMapper>(provider =>
{
    var config = provider.GetRequiredService<MapperConfiguration>();
    return config.CreateMapper(provider.GetService);
});

var app = builder.Build();

app.UseCors(CORS_VITE_DEV);

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
