using CoffeeShopBot.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShopBot.Services
{
    public class AdminSeeder
    {
        private readonly ApplicationContext _db;
        private readonly IConfiguration _configuration;
        public AdminSeeder(ApplicationContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public async Task SeedAsync()
        {
            if (await _db.admins.AnyAsync())
                return;

            var password = _configuration["Admin:Password"];

            if (string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException(
                    "Admin password is not configured.");

            var admin = new Admin
            {
                Name = "admin"
            };

            var hasher = new PasswordHasher<Admin>();

            admin.PasswordHash =
                hasher.HashPassword(admin, password);

            _db.admins.Add(admin);

            await _db.SaveChangesAsync();
        }
    }
}
