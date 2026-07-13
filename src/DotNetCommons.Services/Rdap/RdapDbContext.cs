using DotNetCommons.Services.Rdap.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotNetCommons.Services.Rdap;

public class RdapDbContext : DbContext
{
    public DbSet<RdapCache> RdapCache { get; set; }
    public DbSet<RdapQueue> RdapQueue { get; set; }

    protected RdapDbContext()
    {
    }

    public RdapDbContext(DbContextOptions<RdapDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RdapCache>(e =>
        {
            e.HasIndex(x => new { x.Start1, x.Start2, x.End1, x.End2 }).IsUnique();
        });
    }
}