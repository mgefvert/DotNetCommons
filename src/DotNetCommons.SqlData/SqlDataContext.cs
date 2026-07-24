using DotNetCommons.SqlData.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotNetCommons.SqlData;

public class SqlDataContext : DbContext
{
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