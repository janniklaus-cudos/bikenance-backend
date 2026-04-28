using Backend.Data;
using Backend.Services;
using Backend.Mapping;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Backend.Repositories;
using AutoMapper;
using Backend.Integrations.Strava;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;

var builder = WebApplication.CreateBuilder(args);

// add authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Auth:Jwt:Issuer"],
        ValidAudience = builder.Configuration["Auth:Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Auth:Jwt:Key"]))
    };
});

// ASP.NET Core services
builder.Services.AddControllers(options =>
    {
        var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        options.Filters.Add(new AuthorizeFilter(policy));
    })
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
