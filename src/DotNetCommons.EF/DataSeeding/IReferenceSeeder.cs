using Microsoft.EntityFrameworkCore;

namespace DotNetCommons.EF.DataSeeding;

/// <summary>
/// Defines a contract for seeding reference data into a database context. Implementations of this interface
/// are responsible for providing static or reference data required for the application to function correctly.
/// This data is often populated during application initialization or as part of deployment scripts.
/// </summary>
public interface IReferenceSeeder<in TContext>
    where TContext : DbContext
{
    void SeedReferenceData(TContext context);
}