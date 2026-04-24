using Microsoft.EntityFrameworkCore;

namespace DotNetCommons.EF.DataSeeding;

/// <summary>
/// Defines a contract for seeding test data into a database context of type <typeparamref name="TContext"/>.
/// Typically used to populate test data required for unit or integration tests.
/// </summary>
public interface ITestSeeder<in TContext>
    where TContext : DbContext
{
    void SeedTestData(TContext context);
}