using System.Buffers.Binary;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.RegularExpressions;
using DotNetCommons.Net;
using DotNetCommons.Services.Rdap.Entities;
using DotNetCommons.Sys;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DotNetCommons.Services.Rdap;

public enum RdapAction
{
    OnlyLocal,
    QueueIfNotFound,
    FetchIfNotFound
}

public class RdapIntegration
{
    private static string? _curlVersion;
    private static readonly Regex CurlRegex = new(@"^curl\s+(.+?)\s");

    private readonly RdapDbContext _context;
    private readonly ILogger<RdapIntegration> _logger;
    private readonly HttpClient _client;

    public RdapIntegration(RdapDbContext context, ILogger<RdapIntegration> logger)
    {
        _context = context;
        _logger  = logger;

        _client = new HttpClient();
        _client.DefaultRequestHeaders.UserAgent.Clear();
        _client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("curl", GetCurlVersion().Result.NullIfEmpty() ?? "8.19.0"));
    }

    private static bool AddressToUInt64(IPAddress address, out ulong a1, out ulong a2)
    {
        a1 = a2 = 0;
        if (!IsValidAddress(address))
            return false;

        if (address.AddressFamily == AddressFamily.InterNetwork)
            address = address.MapToIPv6();

        Span<byte> bytes = stackalloc byte[16];
        address.TryWriteBytes(bytes, out _);

        a1 = BinaryPrimitives.ReadUInt64BigEndian(bytes[..8]);
        a2 = BinaryPrimitives.ReadUInt64BigEndian(bytes[8..]);

        return true;
    }

    private static async Task<string?> GetCurlVersion()
    {
        if (_curlVersion != null)
            return _curlVersion;

        var version = await Spawn.Eval("curl", "--version");
        var match   = CurlRegex.Match(version.Output);
        return _curlVersion = match.Success ? match.Groups[1].Value : "";
    }

    public async Task<List<RdapResult>> FetchRdap(IPAddress ip, CancellationToken ct = default)
    {
        if (!IsValidAddress(ip))
            return [];

        _logger.LogDebug("rdap.org lookup for {ip}", ip);

        var url     = $"https://rdap.org/ip/{ip}";
        var results = new List<RdapResult>();

        try
        {
            for (int i = 0; i < 5; i++)
            {
                _logger.LogDebug("Calling url {url}", url);

                var data = await _client.GetStringAsync(url, ct);
                if (data.IsEmpty())
                    break;

                var json = JsonSerializer.Deserialize<RdapResponse>(data);
                if (json == null)
                    break;

                var result = new RdapResult
                {
                    Name         = json.Name,
                    Network      = json.Handle,
                    StartAddress = json.StartAddress,
                    EndAddress   = json.EndAddress,
                    IpVerson     = json.IpVersion
                };
                results.Add(result);

                RecurseEntities(json.Entities, result);

                var up = json.Links.FirstOrDefault(l => l.Rel == "up")?.Href;
                if (up == url || up == null)
                    break;

                url = up;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while doing RDAP lookup for url: {url}", url);
        }

        return results;

        static void RecurseEntities(List<RdapEntity> entities, RdapResult result)
        {
            foreach (var entity in entities)
            {
                result.VCards.AddRange(VCard.ParseVCardArray(entity.VcardArray));
                RecurseEntities(entity.Entities, result);
            }
        }
    }

    public async Task<List<RdapCache>> LookupRdap(IPAddress ip, RdapAction action, CancellationToken ct = default)
    {
        if (!AddressToUInt64(ip, out var a1, out var a2))
            return [];

        var result = await _context.RdapCache
            .Where(x =>
                (x.Start1 < a1 || (x.Start1 == a1 && x.Start2 <= a2)) &&
                (x.End1 > a1 || (x.End1 == a1 && x.End2 >= a2))
            )
            .OrderBy(x => x.Start1)
            .ThenBy(x => x.Start2)
            .ToListAsync(ct);

        if (result.IsEmpty())
        {
            if (action == RdapAction.QueueIfNotFound)
                await QueueAddress(ip);
            else if (action == RdapAction.FetchIfNotFound)
            {
                // Fetch new information and write it
                await SaveRdapInfo(await FetchRdap(ip, ct));
            }
        }

        return result;
    }

    public async Task ProcessQueue(int maxAddresses, CancellationToken ct = default)
    {
        for (var i = 0; i < maxAddresses; i++)
        {
            if (ct.IsCancellationRequested)
                return;

            var item = await _context.RdapQueue.OrderBy(x => x.Address).Take(1).FirstOrDefaultAsync(ct);
            if (item == null)
                return;

            if (IPAddress.TryParse(item.Address, out var address))
            {
                await LookupRdap(address, RdapAction.FetchIfNotFound, ct);
            }

            _context.RdapQueue.Remove(item);
            await _context.SaveChangesAsync(CancellationToken.None);
        }
    }

    private async Task SaveRdapInfo(List<RdapResult> rdapRecords)
    {
        foreach (var info in rdapRecords)
        {
            if (!IPAddress.TryParse(info.StartAddress, out var start))
                continue;
            if (!IPAddress.TryParse(info.EndAddress, out var end))
                continue;

            if (!AddressToUInt64(start, out var s1, out var s2))
                continue;
            if (!AddressToUInt64(end, out var e1, out var e2))
                continue;

            var exists = await _context.RdapCache.AnyAsync(x => x.Start1 == s1 && x.Start2 == s2 && x.End1 == e1 && x.End2 == e2);
            if (exists)
                continue;

            var s = Math.ScaleB(s1, 64) + s2;
            var e = Math.ScaleB(e1, 64) + e2;

            _context.RdapCache.Add(new RdapCache
            {
                Start1 = s1,
                Start2 = s2,
                End1 = e1,
                End2 = e2,
                Size = e - s,
                StartAddress = start.ToString(),
                EndAddress = end.ToString(),
                UpdatedZ = DateTime.UtcNow,
                Organization = info.PrimaryVCard?.FormattedName,
                Country = info.PrimaryVCard?.Country
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task<bool> QueueAddress(IPAddress address)
    {
        if (!IsValidAddress(address))
            return false;

        if (address.AddressFamily != AddressFamily.InterNetwork && address.AddressFamily != AddressFamily.InterNetworkV6)
            return false;

        var s = address.ToString();
        var exists = await _context.RdapQueue.AnyAsync(x => x.Address == s);
        if (exists)
            return false;

        try
        {
            _context.RdapQueue.Add(new RdapQueue { Address = s });
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogInformation(e, "Exception while trying to save IP address {Address} to RDAP queue", s);
            return false;
        }
    }

    private static bool IsValidAddress(IPAddress address)
    {
        if (address.AddressFamily != AddressFamily.InterNetwork && address.AddressFamily != AddressFamily.InterNetworkV6)
            return false;
        
        if (address.IsIPv6LinkLocal || address.IsIPv6Multicast || address.IsIPv6Teredo || address.IsIPv6UniqueLocal)
            return false;

        if (IPAddress.IsLoopback(address))
            return false;

        if (address.Equals(IPAddress.Broadcast) || address.Equals(IPAddress.Any) || address.Equals(IPAddress.IPv6Any) || address.Equals(IPAddress.None))
            return false;
        
        return true;
    }
}
