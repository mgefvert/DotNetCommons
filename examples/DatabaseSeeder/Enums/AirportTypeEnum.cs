using System.ComponentModel;

namespace DatabaseSeeder.Enums;

public enum AirportTypeEnum
{
    [Description("Small regional airport")]
    SmallAirport = 1,
    [Description("Medium or national airport")]
    MediumAirport = 2,
    [Description("Large or international airport")]
    LargeAirport = 3,
    [Description("Port for balloons")]
    BalloonPort = 4,
    [Description("Base for seaplanes")]
    SeaplaneBase = 6,
    [Description("Helicopter airport")]
    Heliport = 7,
    [Description("Closed airport")]
    Closed = 255,
}