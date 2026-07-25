using DotNetCommons.SqlData.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotNetCommons.SqlData;

public class SqlDataContext : DbContext
{
    public DbSet<DbGeoAirport> GeoAirports { get; set; } = null!;
    public DbSet<DbGeoAreaCode> GeoAreaCodes { get; set; } = null!;
    public DbSet<DbGeoCountry> GeoCountries { get; set; } = null!;
    public DbSet<DbGeoZipCode> GeoZipCodes { get; set; } = null!;
    public DbSet<DbIpCity> IpCities { get; set; } = null!;
    public DbSet<DbIpCountry> IpCountries { get; set; } = null!;
    public DbSet<DbIpLookup> IpLookup { get; set; } = null!;

    public SqlDataContext()
    {
    }
    
    public SqlDataContext(DbContextOptions<SqlDataContext> options) : base(options)
    {
    }
}