using System.Globalization;
using CsvHelper.Configuration;
using DotNetCommons.Commands;
using DotNetCommons.EF;
using DotNetCommons.SqlData;
using DotNetCommons.SqlData.Entities;

namespace commons.Commands;

[CommandAction(["import", "airports"], "Download and import airports", [])]
public class ImportAirportsCommand : CommandAction<ConnectionArgs>
{
    private readonly SqlDataContext _context;
    private static readonly Uri Url = new("https://davidmegginson.github.io/ourairports-data/airports.csv");

    public class AirportClassMap : ClassMap<DbGeoAirport>
    {
        public AirportClassMap()
        {
            Map(m => m.Id).Ignore();
            Map(m => m.Ident).Name("ident");
            Map(m => m.Type).Name("type");
            Map(m => m.IcaoCode).Name("icao_code");
            Map(m => m.IataCode).Name("iata_code");
            Map(m => m.Name).Name("name");
            Map(m => m.Latitude).Name("latitude_deg");
            Map(m => m.Longitude).Name("longitude_deg");
            Map(m => m.Elevation).Name("elevation_ft");
            Map(m => m.Continent).Name("continent");
            Map(m => m.Country).Name("iso_country");
            Map(m => m.Region).Name("iso_region");
            Map(m => m.Municipality).Name("municipality");
        }
    }

    public ImportAirportsCommand(SqlDataContext context)
    {
        _context = context;
    }

    public override int Execute()
    {
        var data = Helper.DownloadOrCacheString("airports", Url);

        using var reader = new StringReader(data);
        using var csv    = Helper.GetCsvReader<AirportClassMap>(reader, CultureInfo.InvariantCulture, true, ",");

        var records = csv.GetRecords<DbGeoAirport>().Where(x => x.IsValid).ToList();
        var existing = _context.GeoAirports.ToList();

        new Patch(context: _context).Update(PatchMode.AllowAll, x => x.Ident!, existing, records, x => _context.GeoAirports.Add(x));
        var n = _context.SaveChanges();
        Console.WriteLine($"airports: saved {n} updated rows");

        return 0;
    }
}