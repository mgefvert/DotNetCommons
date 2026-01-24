using DatabaseSeeder.Entities;
using DotNetCommons.EF.DataSeeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var connectionString = configuration.GetConnectionString("Default")
                       ?? throw new InvalidOperationException("Connection string 'Default' not found in appsettings.json");

var dbContext = new MyDbContext(new DbContextOptionsBuilder<MyDbContext>()
    .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
    .Options);

using (dbContext)
{
    dbContext.Database.EnsureCreated();
    DataSeeder.SeedReferenceData(dbContext);
    DataSeeder.SeedTestData(dbContext);
}
