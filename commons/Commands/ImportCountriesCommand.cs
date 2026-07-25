using System.Globalization;
using CsvHelper.Configuration;
using DotNetCommons.Commands;
using DotNetCommons.EF;
using DotNetCommons.SqlData;
using DotNetCommons.SqlData.Entities;

namespace commons.Commands;

[CommandAction(["import", "countries"], "Download and import country data", [])]
public class ImportCountriesCommand : CommandAction<ConnectionArgs>
{
    private readonly SqlDataContext _context;
    private static readonly Uri Url = new("https://datahub.io/core/country-codes/r/country-codes.csv");

    private class CountryCodeClassMap : ClassMap<DbGeoCountry>
    {
        public CountryCodeClassMap()
        {
            Map(m => m.Id).Ignore();

            Map(m => m.Iso2).Name("ISO3166-1-Alpha-2");
            Map(m => m.Iso3).Name("ISO3166-1-Alpha-3");
            Map(m => m.Name).Name("official_name_en");
            Map(m => m.Capital).Name("Capital");
            Map(m => m.TelCode).Name("Dial");
            Map(m => m.Currency).Name("ISO4217-currency_alphabetic_code");
            Map(m => m.Continent).Name("Continent");
            Map(m => m.Region).Name("Region Name");
            Map(m => m.Subregion).Name("Sub-region Name");
        }
    }

    public ImportCountriesCommand(SqlDataContext context)
    {
        _context = context;
    }

    public override int Execute()
    {
        var data = Helper.DownloadOrCacheString("countrycodes", Url);

        using var reader = new StringReader(data);
        using var csv    = Helper.GetCsvReader<CountryCodeClassMap>(reader, CultureInfo.InvariantCulture, true, ",");

        var records = csv.GetRecords<DbGeoCountry>().Where(x => x.IsValid).ToList();
        var existing = _context.GeoCountries.ToList();

        new Patch().Update(PatchMode.AllowAll, x => x.Iso2!, existing, records);
        var n = _context.SaveChanges();
        Console.WriteLine($"Saved {n} updated rows for 'countries' import");

        return 0;
    }
}