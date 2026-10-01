using CoffeeShopBot.Models;
using Microsoft.EntityFrameworkCore;
namespace CoffeeShopBot.Data;

public class ApplicationContext : DbContext
{
    public DbSet<User> users {get;set;} = null!;
    public DbSet<Admin> admins {get;set;} = null!;
    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base (options)
    {
        Database.EnsureCreated();
    }
}