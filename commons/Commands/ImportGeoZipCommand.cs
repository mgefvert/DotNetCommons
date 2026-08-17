using System.Globalization;
using System.IO.Compression;
using CsvHelper.Configuration;
using DotNetCommons;
using DotNetCommons.Commands;
using DotNetCommons.EF;
using DotNetCommons.SqlData;
using DotNetCommons.SqlData.Entities;

namespace commons.Commands;

[CommandAction(["import", "geo", "zip"], "Download and import zip codes", [])]
public class ImportGeoZipCommand : CommandAction<ConnectionArgs>
{
    private readonly SqlDataContext _context;
    private static readonly Uri Url = new("https://download.geonames.org/export/zip/US.zip");

    public class ZipCodeClassMap : ClassMap<DbGeoZipCode>
    {
        public ZipCodeClassMap()
        {
            Map(m => m.Id).Ignore();

            Map(m => m.Code).Index(1);
            Map(m => m.City).Index(2);
            Map(m => m.State).Index(4);
            Map(m => m.County).Index(5);
            Map(m => m.Latitude).Index(9);
            Map(m => m.Longitude).Index(10);
        }
    }

    public ImportGeoZipCommand(SqlDataContext context)
    {
        _context = context;
    }

    public override int Execute()
    {
        var data = Helper.DownloadOrCache("zipcodes", Url);

        using var mem = new MemoryStream(data);
        using var zip = new ZipArchive(mem, ZipArchiveMode.Read);

        var entry  = zip.Entries.FirstOrDefault(x => x.Name.EqualsInsensitive("US.txt")) ?? throw new Exception("CSV not found in zip file");

        using var stream = entry.Open();
        using var reader = new StreamReader(stream);
        using var csv    = Helper.GetCsvReader<ZipCodeClassMap>(reader, CultureInfo.InvariantCulture, false, "\t");

        var records = csv.GetRecords<DbGeoZipCode>().Where(x => x.IsValid).ToList();
        var existing = _context.GeoZipCodes.ToList();

        new Patch(context: _context).Update(PatchMode.AllowAll, x => x.Code!, existing, records);
        var n = _context.SaveChanges();
        Console.WriteLine($"zipcodes: saved {n} updated rows");

        return 0;
    }
}