using DotNetCommons.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Net;

/// <summary>
/// Defines an IP-address based access list that can grant or revoke access based on either
/// IPv4 or IPv6 network ranges.
/// </summary>
public class IPAccessList
{
    /// <summary>
    /// List of addresses approved for access.
    /// </summary>
    public List<IPAddress> Addresses { get; } = new();

    /// <summary>
    /// List of networks approved for access.
    /// </summary>
    public List<IPNetwork> Ranges { get; } = new();

    /// <summary>
    /// Parse a comma-separated string of IP addresses and networks.
    /// </summary>
    public static IPAccessList Parse(string list)
    {
        var result = new IPAccessList();
        foreach (var item in list.Split(',').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)))
            result.Add(item);

        return result;
    }

    /// <summary>
    /// Add an approved IP address.
    /// </summary>
    /// <param name="address"></param>
    public void Add(IPAddress address)
    {
        Addresses.Add(address);
    }

    /// <summary>
    /// Add a range of approved IP addresses.
    /// </summary>
    /// <param name="addresses"></param>
    public void Add(IEnumerable<IPAddress> addresses)
    {
        Addresses.AddRange(addresses);
    }

    /// <summary>
    /// Add an approved IP network.
    /// </summary>
    /// <param name="range"></param>
    public void Add(IPNetwork range)
    {
        Ranges.Add(range);
    }

    /// <summary>
    /// Add a range of approved IP networks.
    /// </summary>
    /// <param name="ranges"></param>
    public void Add(IEnumerable<IPNetwork> ranges)
    {
        Ranges.AddRange(ranges);
    }

    /// <summary>
    /// Add an IP address or network represented as a string. Accepts host addresses
    /// and tries to look them up.
    /// </summary>
    public void Add(string address)
    {
        if (IPAddress.TryParse(address, out var ip))
        {
            Add(ip);
            return;
        }

        if (IPNetwork.TryParse(address, out var range) && range != null)
        {
            Add(range);
            return;
        }

        Addresses.AddRangeIfNotNull(Dns.GetHostAddresses(address));
    }

    /// <summary>
    /// Determine if a given IP address is included in the access list or not.
    /// </summary>
    public bool Contains(IPAddress address)
    {
        return Addresses.Any(ip => ip.Equals(address)) || Ranges.Any(ip => ip.Contains(address));
    }

    /// <summary>
    /// List the IP addresses and networks.
    /// </summary>
    public override string ToString()
    {
        var result = new List<string>();

        result.AddRange(Addresses.Select(x => x.ToString()));
        result.AddRange(Ranges.Select(x => x.ToString()));

        result.Sort();

        return string.Join(", ", result);
    }
}