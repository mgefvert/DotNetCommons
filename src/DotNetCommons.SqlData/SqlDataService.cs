using System.Net;
using System.Net.Sockets;
using Dapper;
using DotNetCommons;
using Microsoft.EntityFrameworkCore;

namespace DotNetCommons.SqlData;

public record IpCity(byte[] Bytes, string? Country, string? State, string? City)
{
    public IPAddress NetworkAddress { get; } = new(Bytes);
}

public record IpCountry(byte[] Bytes, string? Country)
{
    public IPAddress NetworkAddress { get; } = new(Bytes);
}

public class SqlDataService : ISqlDataService
{
    private readonly SqlDataContext _context;

    public SqlDataService(SqlDataContext context)
    {
        _context = context;
    }

    public async Task<IpCity?> LookupCity(IPAddress ip)
    {
        if (ip.AddressFamily == AddressFamily.InterNetwork)
            ip = ip.MapToIPv6();

        if (ip.AddressFamily != AddressFamily.InterNetworkV6)
            return null;

        var bytes      = ip.GetAddressBytes().ToHex();
        var connection = _context.Database.GetDbConnection();

        var result = await connection.QueryAsync<IpCity>(
            $"""
            select ipc.ip as bytes, ipl1.name as country, ipl2.name as state, ipl3.name as city
            from ip_city ipc
                left join ip_lookup ipl1 on ipl1.id = ipc.country
                left join ip_lookup ipl2 on ipl2.id = ipc.state
                left join ip_lookup ipl3 on ipl3.id = ipc.city
            where ip <= X'{bytes}' 
            order by ip desc 
            limit 1
            """);

        return result.FirstOrDefault();
    }

    public async Task<IpCountry?> LookupCountry(IPAddress ip)
    {
        if (ip.AddressFamily == AddressFamily.InterNetwork)
            ip = ip.MapToIPv6();

        if (ip.AddressFamily != AddressFamily.InterNetworkV6)
            return null;

        var bytes      = ip.GetAddressBytes().ToHex();
        var connection = _context.Database.GetDbConnection();

        var result = await connection.QueryAsync<IpCountry>(
            $"""
             select ipc.ip as bytes, ipl1.`name` as country
             from ip_country ipc
                 left join ip_lookup ipl1 on ipl1.id = ipc.country
             where ip <= X'{bytes}' 
             order by ip desc 
             limit 1
             """);

        return result.FirstOrDefault();
    }
}