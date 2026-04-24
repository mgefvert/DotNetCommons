using DotNetCommons.Services.Old.Models;
using Microsoft.EntityFrameworkCore;

namespace DotNetCommons.Services.Old;

public class ServiceContext : DbContext
{
    public DbSet<IdpUser> IdpUsers { get; set; }
    public DbSet<IdpUserRight> IdpUserRight { get; set; }
}