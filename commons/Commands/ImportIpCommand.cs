using System.Net;
using System.Net.Sockets;
using System.Text;
using Dapper;
using DotNetCommons;
using DotNetCommons.Commands;
using DotNetCommons.IO;
using DotNetCommons.Security;
using DotNetCommons.SqlData.Entities;
using MySql.Data.MySqlClient;

namespace commons.Commands;

[CommandAction(["import", "ip"], "Import the IP country/city databases from github.com/sapics", [])]
public class ImportIpCommand : CommandAction<ConnectionArgs>
{
    private readonly MySqlCnfReader _mysqlCnfReader;
    private readonly HttpClient _client = new();
    private static readonly Uri Root = new("https://github.com/sapics/ip-location-db/releases/download/latest/");
    private static readonly Uri IpV4CityDatabase = new(Root, "dbip-city-ipv4.csv.gz");
    private static readonly Uri IpV6CityDatabase = new(Root, "dbip-city-ipv6.csv.gz");
    private static readonly Uri IpV4CountryDatabase = new(Root, "dbip-country-ipv4.csv");
    private static readonly Uri IpV6CountryDatabase = new(Root, "dbip-country-ipv6.csv");

    private readonly Dictionary<string, int> _lookup = new(StringComparer.CurrentCulture);
    private int _lookupId;

    public ImportIpCommand(MySqlCnfReader mysqlCnfReader)
    {
        _mysqlCnfReader = mysqlCnfReader;
    }

    public override async Task<int> ExecuteAsync(CancellationToken ct)
    {
        Console.WriteLine("Downloading databases...");
        var countryIpV4B = Helper.DownloadOrCache("ipv4-country", IpV4CountryDatabase);
        var countryIpV6B = Helper.DownloadOrCache("ipv6-country", IpV6CountryDatabase);
        var cityIpV4Z    = Helper.DownloadOrCache("ipv4-city", IpV4CityDatabase);
        var cityIpV6Z    = Helper.DownloadOrCache("ipv6-city", IpV6CityDatabase);

        Console.WriteLine("Loading data...");
        var countryIpV4 = Encoding.ASCII.GetString(countryIpV4B);
        var countryIpV6 = Encoding.ASCII.GetString(countryIpV6B);
        var cityIpV4    = Encoding.ASCII.GetString(Compression.Decompress(cityIpV4Z, CompressionMethod.GZip));
        var cityIpV6    = Encoding.ASCII.GetString(Compression.Decompress(cityIpV6Z, CompressionMethod.GZip));

        Console.WriteLine("Parsing...");
        var countries = ParseCountry(countryIpV4).Concat(ParseCountry(countryIpV6)).ToArray();
        var cities    = ParseCity(cityIpV4).Concat(ParseCity(cityIpV6)).ToArray();

        var connectionString = _mysqlCnfReader.RequireConnectionString(Args.Connection, "sqldata");
        var connection       = new MySqlConnection(connectionString);

        Console.WriteLine("Removing previous entries");
        await connection.ExecuteAsync("TRUNCATE ip_city");
        await connection.ExecuteAsync("TRUNCATE ip_country");
        await connection.ExecuteAsync("TRUNCATE ip_lookup");

        Console.WriteLine("Writing lookups");
        foreach (var batch in _lookup.Batch(10000))
        {
            var sql = BuildScript(batch);
            await connection.ExecuteAsync(sql);
        }

        Console.Write("Saving countries ");
        foreach (var batch in countries.Batch(30000))
        {
            var sql = BuildScript(batch);
            await connection.ExecuteAsync(sql);
            Console.Write('.');
        }
        Console.WriteLine();

        Console.Write("Saving cities ");
        foreach (var batch in cities.Batch(30000))
        {
            var sql = BuildScript(batch);
            await connection.ExecuteAsync(sql);
            Console.Write('.');
        }
        Console.WriteLine();

        Console.WriteLine("Completed");

        return 0;
    }

    private string BuildScript(KeyValuePair<string, int>[] lookups)
    {
        var result = new StringBuilder();
        result.Append("INSERT INTO ip_lookup (id, name) VALUES ");
        foreach (var lookup in lookups.WithIndex())
        {
            if (!lookup.IsFirst)
                result.Append(',');

            var (name, id) = lookup.Item;
            name = MySqlHelper.EscapeString(name);

            result.Append($"({id}, '{name}')");
        }

        return result.ToString();
    }

    private string BuildScript(ICollection<DbIpCity> cities)
    {
        var result = new StringBuilder();
        result.Append("INSERT INTO ip_city (ip, country, state, city) VALUES ");
        foreach (var c in cities.WithIndex())
        {
            if (!c.IsFirst)
                result.Append(',');

            var ci = c.Item;
            result.Append($"(X'{ci.Ip.ToHex()}', {ci.Country?.ToString() ?? "null"}, {ci.State?.ToString() ?? "null"}, {ci.City?.ToString() ?? "null"})");
        }

        return result.ToString();
    }

    private string BuildScript(ICollection<DbIpCountry> countries)
    {
        var result = new StringBuilder();
        result.Append("INSERT INTO ip_country (ip, country) VALUES ");
        foreach (var c in countries.WithIndex())
        {
            if (!c.IsFirst)
                result.Append(',');

            var ci = c.Item;
            result.Append($"(X'{ci.Ip.ToHex()}', {ci.Country?.ToString() ?? "null"})");
        }

        return result.ToString();
    }

    private IEnumerable<DbIpCity> ParseCity(string data)
    {
        foreach (var row in data.Split("\n"))
        {
            if (string.IsNullOrWhiteSpace(row))
                continue;

            var x       = row.Split(",", StringSplitOptions.TrimEntries);
            var start   = IPAddress.Parse(x[0]);
            var country = x[2];
            var state   = x[3];
            var city    = x[5];

            if (start.AddressFamily == AddressFamily.InterNetwork)
                start = start.MapToIPv6();

            int? countryId = null;
            int? stateId = null;
            int? cityId = null;

            if (country.IsSet())
                countryId = _lookup.TryGetValue(country, out var value) ? value : _lookup[country] = ++_lookupId;

            if (state.IsSet())
                stateId = _lookup.TryGetValue(state, out var value) ? value : _lookup[state] = ++_lookupId;

            if (city.IsSet())
                cityId = _lookup.TryGetValue(city, out var value) ? value : _lookup[city] = ++_lookupId;

            yield return new DbIpCity
            {
                Ip      = start.GetAddressBytes(),
                Country = countryId,
                State   = stateId,
                City    = cityId,
            };
        }
    }

    private IEnumerable<DbIpCountry> ParseCountry(string data)
    {
        foreach (var row in data.Split("\n"))
        {
            if (string.IsNullOrWhiteSpace(row))
                continue;

            var x = row.Split(",", StringSplitOptions.TrimEntries);
            var start = IPAddress.Parse(x[0]);
            var country = x[2];

            if (start.AddressFamily == AddressFamily.InterNetwork)
                start = start.MapToIPv6();

            int? countryId = null;
            if (country.IsSet())
                countryId = _lookup.TryGetValue(country, out var value) ? value : _lookup[country] = ++_lookupId;

            yield return new DbIpCountry
            {
                Ip      = start.GetAddressBytes(),
                Country = countryId
            };
        }
    }
}