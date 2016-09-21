using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT

namespace CommonNetTools.Net
{
  public class IPAccessList
  {
    public List<IPAddress> Addresses { get; } = new List<IPAddress>();
    public List<IPRange> Ranges { get; } = new List<IPRange>();

    public static IPAccessList Parse(string list)
    {
      var result = new IPAccessList();
      foreach (var item in list.Split(',').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)))
        result.Add(item);

      return result;
    }

    public void Add(IPAddress address)
    {
      Addresses.Add(address);
    }

    public void Add(IEnumerable<IPAddress> addresses)
    {
      Addresses.AddRange(addresses);
    }

    public void Add(IPRange range)
    {
      Ranges.Add(range);
    }

    public void Add(IEnumerable<IPRange> ranges)
    {
      Ranges.AddRange(ranges);
    }

    public void Add(string address)
    {
      IPAddress ip;
      if (IPAddress.TryParse(address, out ip))
      {
        Add(ip);
        return;
      }

      IPRange range;
      if (IPRange.TryParse(address, out range))
      {
        Add(range);
        return;
      }

      Addresses.AddRangeIfNotNull(Dns.GetHostAddresses(address));
    }

    public bool Contains(IPAddress address)
    {
      return Addresses.Any(ip => ip.Equals(address)) || Ranges.Any(ip => ip.Contains(address));
    }

    public override string ToString()
    {
      var result = new List<string>();

      result.AddRange(Addresses.Select(x => x.ToString()));
      result.AddRange(Ranges.Select(x => x.ToString()));

      result.Sort();

      return string.Join(", ", result);
    }
  }
}
