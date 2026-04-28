using Backend.Data;
using Backend.Models;

namespace Backend.Repositories;

public class UserRepository(AppDbContext db) : EfRepository<User>(db), IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email) =>
         _db.Users.FirstOrDefault<User>(user => user.Email == email);

}