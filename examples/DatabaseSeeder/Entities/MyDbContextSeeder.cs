using DatabaseSeeder.Enums;
using DotNetCommons.EF;
using DotNetCommons.EF.DataSeeding;

namespace DatabaseSeeder.Entities;

public class MyDbContextSeeder : IReferenceSeeder<MyDbContext>, ITestSeeder<MyDbContext>
{
    public void SeedReferenceData(MyDbContext context)
    {
        context.AirportTypes.Seed(s =>
        {
            s.EnsureEnumsCreated<AirportTypeEnum>(x => x.Id, x => x.Name, x => x.Description);
        });

        context.SaveChanges();
    }

    public void SeedTestData(MyDbContext context)
    {
        context.Airports.Seed(s =>
        {
            s.EnsureCreated(x => x.Ident, [
                new Airport
                {
                    Ident     = "SC62",
                    Type      = (int)AirportTypeEnum.Heliport,
                    Name      = "Hampton Regional Medical Center Heliport",
                    Latitude  = 32.8524017334,
                    Longitude = -81.0886993408,
                    Elevation = 86
                },
                new Airport
                {
                    Ident     = "US-10288",
                    Type      = (int)AirportTypeEnum.Closed,
                    Name      = "Moccasin Creek Airport",
                    Latitude  = 33.1422,
                    Longitude = -80.955597,
                    Elevation = 142
                },
                new Airport
                {
                    Ident     = "KLQK",
                    Type      = (int)AirportTypeEnum.SmallAirport,
                    Name      = "Pickens County Airport",
                    Latitude  = 34.8100013733,
                    Longitude = -82.70290374759999,
                    Elevation = 1013
                },
                new Airport
                {
                    Ident     = "KGSP",
                    Type      = (int)AirportTypeEnum.MediumAirport,
                    Name      = "Greenville Spartanburg International Airport",
                    Latitude  = 34.895699,
                    Longitude = -82.218903,
                    Elevation = 964
                }
            ]);
        });

        context.SaveChanges();
    }
}