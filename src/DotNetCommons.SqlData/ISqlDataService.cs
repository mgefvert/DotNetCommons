using System.Net;

namespace DotNetCommons.SqlData;

public interface ISqlDataService
{
    Task<IpCity?> LookupCity(IPAddress ip);
    Task<IpCountry?> LookupCountry(IPAddress ip);
}