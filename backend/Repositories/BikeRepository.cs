using Backend.Data;
using Backend.Models;

namespace Backend.Repositories;

public class BikeRepository(AppDbContext db) : EfRepository<Bike>(db), IBikeRepository
{
}