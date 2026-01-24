using Microsoft.EntityFrameworkCore;

namespace DatabaseSeeder.Entities;

public class MyDbContext : DbContext
{
    public DbSet<AirportType> AirportTypes { get; set; }
    public DbSet<Airport> Airports { get; set; }

    public MyDbContext(DbContextOptions options) : base(options)
    {
    }
}