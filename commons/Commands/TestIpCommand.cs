using System.Diagnostics;
using System.Net;
using DotNetCommons.Commands;
using DotNetCommons.Services.Misc;
using DotNetCommons.SqlData;

namespace commons.Commands;

[CommandAction(["test", "ip"], "Test the IP country/city databases by looking up your own IP address", [])]
public class TestIpCommand : CommandAction<ConnectionArgs>
{
    private readonly IpifyIntegration _ipify;
    private readonly ISqlDataService _service;

    public TestIpCommand(IpifyIntegration ipify, ISqlDataService service)
    {
        _ipify   = ipify;
        _service = service;
    }

    public override async Task<int> ExecuteAsync(CancellationToken ct)
    {
        var ip = await _ipify.GetMyIpAddress();
        if (ip.IsFailure)
            throw new MessageException($"Failed to get IP address: {ip.Error}");

        var address = ip.Value!.Address;
        Console.WriteLine($"Your IP address is {address}");

        // Warm up the connections
        await _service.LookupCity(IPAddress.Loopback);

        var watch   = Stopwatch.StartNew();
        var country = await _service.LookupCountry(address);
        Console.WriteLine($"Your country is {country?.Country}, lookup done = {watch.Elapsed}");

        watch    = Stopwatch.StartNew();
        var city = await _service.LookupCity(address);
        Console.WriteLine($"Your city is {city?.Country} : {city?.State} : {city?.City}, lookup done = {watch.Elapsed}");

        return 0;
    }
}