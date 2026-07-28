using System.Globalization;
using CsvHelper.Configuration;
using DotNetCommons.Commands;
using DotNetCommons.EF;
using DotNetCommons.SqlData;
using DotNetCommons.SqlData.Entities;

namespace commons.Commands;

[CommandAction(["import", "areacodes"], "Download and import NANP area codes", [])]
public class ImportAreaCodesCommand : CommandAction<ConnectionArgs>
{
    private readonly SqlDataContext _context;
    private static readonly Uri Url = new("https://reports.nanpa.com/public/npa_report.csv");

    public class AreaCodeClassMap : ClassMap<DbGeoAreaCode>
    {
        public AreaCodeClassMap()
        {
            Map(m => m.Id).Ignore();

            Map(m => m.Code).Name("NPA_ID");
            Map(m => m.Country).Name("COUNTRY");
            Map(m => m.State).Name("LOCATION");

            // Computed property – no matching CSV column
            Map(m => m.IsValid).Ignore();
        }
    }

    public ImportAreaCodesCommand(SqlDataContext context)
    {
        _context = context;
    }

    public override int Execute()
    {
        var data = Helper.DownloadOrCacheString("areacodes", Url);

        // Filter away "File Date" header
        var lines = data.Split("\n").ToList();
        lines.RemoveAt(0);
        data = string.Join("\n", lines);

        using var reader = new StringReader(data);
        using var csv    = Helper.GetCsvReader<AreaCodeClassMap>(reader, CultureInfo.InvariantCulture, true, ",");

        var records = csv.GetRecords<DbGeoAreaCode>().Where(x => x.IsValid).ToList();
        var existing = _context.GeoAreaCodes.ToList();

        new Patch(context: _context).Update(PatchMode.AllowAll, x => x.Code!, existing, records);
        var n = _context.SaveChanges();
        Console.WriteLine($"areacodes: saved {n} updated rows");

        return 0;
    }
}