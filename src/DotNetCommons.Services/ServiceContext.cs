using DotNetCommons.Services.Models;
using Microsoft.EntityFrameworkCore;

namespace DotNetCommons.Services;

public class ServiceContext : DbContext
{
    public DbSet<IdpUser> IdpUsers { get; set; }
    public DbSet<IdpUserRight> IdpUserRight { get; set; }
}