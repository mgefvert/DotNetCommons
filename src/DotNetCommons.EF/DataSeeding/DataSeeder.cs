using Microsoft.EntityFrameworkCore;

namespace DotNetCommons.EF.DataSeeding;

/// Provides methods to seed reference and test data into a database context.
/// This class is designed to work with Entity Framework Core DbContexts and
/// facilitates seeding of initial data, whether for production or testing purposes.
public static class DataSeeder
{
    /// <summary>
    /// Seeds reference data into a database by discovering and invoking all implementations of <see cref="IReferenceSeeder{TContext}"/>
    /// within the application. This method is typically used to populate static data required for the correct operation of the application.
    /// </summary>
    /// <param name="serviceProvider">
    /// An instance of <see cref="IServiceProvider"/> used to resolve the DbContext and seeder instances required for seeding.
    /// </param>
    public static void SeedReferenceData(IServiceProvider serviceProvider)
    {
        FindAndCallSeederWithContextDiscovery(serviceProvider, typeof(IReferenceSeeder<>), nameof(IReferenceSeeder<>.SeedReferenceData));
    }

    /// <summary>
    /// Seeds reference data into a database by invoking all implementations of <see cref="IReferenceSeeder{TContext}"/>
    /// for a specific <see cref="DbContext"/> instance. This method is typically used to populate static data required
    /// for the application when the DbContext instance is explicitly provided.
    /// </summary>
    public static void SeedReferenceData<TContext>(TContext dbContext)
        where TContext : DbContext
    {
        FindAndCallSeederWithContextGiven(dbContext, typeof(IReferenceSeeder<>), nameof(IReferenceSeeder<>.SeedReferenceData));
    }

    /// <summary>
    /// Seeds test data into a database by discovering and invoking all implementations of <see cref="ITestSeeder{TContext}"/>
    /// within the application. This method is typically used to populate test data required for unit testing.
    /// </summary>
    /// <param name="serviceProvider">
    /// An instance of <see cref="IServiceProvider"/> used to resolve the DbContext and seeder instances required for seeding.
    /// </param>
    public static void SeedTestData(IServiceProvider serviceProvider)
    {
        FindAndCallSeederWithContextDiscovery(serviceProvider, typeof(ITestSeeder<>), nameof(ITestSeeder<>.SeedTestData));
    }

    /// <summary>
    /// Seeds test data into a database by invoking all implementations of <see cref="ITestSeeder{TContext}"/>
    /// for a specific <see cref="DbContext"/> instance. This method is typically used to populate test data required
    /// for unit testing when the DbContext instance is explicitly provided.
    /// </summary>
    public static void SeedTestData<TContext>(TContext dbContext)
        where TContext : DbContext
    {
        FindAndCallSeederWithContextGiven(dbContext, typeof(ITestSeeder<>), nameof(ITestSeeder<>.SeedTestData));
    }

    private static void FindAndCallSeederWithContextGiven<TContext>(TContext context, Type seedInterfaceType, string methodName)
        where TContext : DbContext
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract || type.IsInterface)
                    continue;

                var seedInterface = type.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == seedInterfaceType);
                if (seedInterface == null)
                    continue;

                var seedMethod = seedInterface.GetMethod(methodName);
                var seedObject = Activator.CreateInstance(type);
                seedMethod?.Invoke(seedObject, [context]);
            }
        }
    }

    private static void FindAndCallSeederWithContextDiscovery(IServiceProvider serviceProvider, Type seedInterfaceType, string methodName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract || type.IsInterface)
                    continue;

                var seedInterface = type.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == seedInterfaceType);
                if (seedInterface == null)
                    continue;

                var contextType = seedInterface.GetGenericArguments()[0];
                var context     = serviceProvider.GetService(contextType);
                if (context == null)
                    continue;

                var seedMethod = seedInterface.GetMethod(methodName);
                var seedObject = Activator.CreateInstance(type);
                seedMethod?.Invoke(seedObject, [context]);
            }
        }
    }
}