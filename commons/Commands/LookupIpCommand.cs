using System.Net;
using DotNetCommons.Commands;
using DotNetCommons.SqlData;
using DotNetCommons.Sys;

namespace commons.Commands;

[CommandAction(["lookup", "ip"], "Lookup an IP address in the country/city databases", [])]
public class LookupIpCommand : CommandAction<LookupIpCommand.LookupIpArgs>
{
    public class LookupIpArgs : ConnectionArgs
    {
        [CommandLineOption('m', "mode")]
        public string? Mode { get; set; } = "country";

        [CommandLineRemaining]
        public List<string> IPs { get; set; } = [];
    }

    private readonly ISqlDataService _service;

    public LookupIpCommand(ISqlDataService service)
    {
        _service = service;
    }

    public override async Task<int> ExecuteAsync(CancellationToken ct)
    {
        foreach (var ip in Args.IPs)
        {
            var address = IPAddress.Parse(ip);
            switch (Args.Mode)
            {
                case "country":
                {
                    var location = await _service.LookupCountry(address);
                    Console.WriteLine($"{address} = {location?.Country}");
                    break;
                }

                case "city":
                {
                    var location = await _service.LookupCity(address);
                    Console.WriteLine($"{address} = {location?.City}, {location?.State}, {location?.Country}");
                    break;
                }
                default:
                    throw new Exception("Invalid mode given, 'city' or 'country' required");
            }
        }

        return 0;
    }
}