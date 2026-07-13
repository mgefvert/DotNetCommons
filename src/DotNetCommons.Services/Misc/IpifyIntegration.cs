using System.Net;
using Microsoft.Extensions.Logging;

namespace DotNetCommons.Services.Misc;

public class IpifyResponse(IPAddress address)
{
    public IPAddress Address { get; } = address;
}

public class IpifyIntegration
{
    private readonly ILogger<IpifyIntegration> _logger;
    private readonly HttpClient _client;

    private readonly Error _error = new("IpifyError", "Error while looking up IP address on api.ipify.org");

    public IpifyIntegration(ILogger<IpifyIntegration> logger)
    {
        _logger = logger;
        _client = new HttpClient();
    }

    public async Task<Result<IpifyResponse>> GetMyIpAddress()
    {
        try
        {
            var data = await _client.GetStringAsync("https://api.ipify.org/");
            return IPAddress.TryParse(data, out var ip)
                ? new IpifyResponse(ip)
                : Result<IpifyResponse>.Fail(_error);
        }
        catch (Exception e)
        {
            _logger.LogError(e, _error.Description);
            return Result<IpifyResponse>.Fail(_error);
        }
    }
}