using System;
using System.Net;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT

namespace CommonNetTools.Net
{
  public class IPRange
  {
    private readonly byte[] _bytes;
    private readonly int _maxlen;

    public IPAddress Address { get; }
    public int MaskLen { get; }
    public IPAddress Netmask => LengthToNetmask(MaskLen, _maxlen);

    public IPRange(IPAddress address, int masklen)
    {
      Address = Mask(address, masklen);
      MaskLen = masklen;
      _bytes = Address.GetAddressBytes();
      _maxlen = _bytes.Length * 8;
    }

    public IPRange(IPAddress address) : this(address, address.GetAddressBytes().Length * 8)
    {
    }

    public IPRange(IPAddress address, IPAddress netmask) : this(address, NetmaskToLength(netmask))
    {
    }

    public bool Contains(IPAddress ip)
    {
      var ipBytes = GetMaskedBytes(ip, MaskLen);

      // Check for address family mismatch
      if (_bytes.Length != ipBytes.Length)
        return false;

      for (var i = 0; i < _bytes.Length; i++)
        if (_bytes[i] != ipBytes[i])
          return false;

      return true;
    }

    public static IPRange Parse(string network)
    {
      IPRange range;
      if (TryParse(network, out range))
        return range;

      throw new ArgumentException("Invalid IP network: " + network, nameof(network));
    }

    public static bool TryParse(string network, out IPRange range)
    {
      range = null;

      if (string.IsNullOrEmpty(network))
        return false;

      var x = network.Split('/');

      IPAddress address = null;
      int mask = 0;
      var success = true;

      if (x.Length >= 1)
        success &= IPAddress.TryParse(x[0], out address);

      if (x.Length == 2)
        success &= int.TryParse(x[1], out mask);

      if (success)
        range = new IPRange(address, mask);

      return success;
    }

    public override string ToString()
    {
      return Address + "/" + MaskLen;
    }

    private static int BitLen(byte[] buffer)
    {
      return buffer.Length * 8;
    }

    private static bool BitGet(byte[] buffer, int bit)
    {
      if (bit >= BitLen(buffer))
        throw new ArgumentOutOfRangeException(nameof(bit));

      var offset = bit / 8;
      var mask = 128 >> (bit % 8);

      return (buffer[offset] & mask) > 0;
    }

    private static void BitSet(byte[] buffer, int bit, bool value)
    {
      if (bit >= BitLen(buffer))
        throw new ArgumentOutOfRangeException(nameof(bit));

      var offset = bit / 8;
      var mask = 128 >> (bit % 8);

      if (value)
        buffer[offset] = (byte)(buffer[offset] | mask);
      else
        buffer[offset] = (byte)(buffer[offset] & ~mask);
    }

    private static byte[] GetMaskedBytes(IPAddress address, int masklen)
    {
      var bytes = address.GetAddressBytes();
      for (var i = masklen; i < bytes.Length * 8; i++)
        BitSet(bytes, i, false);

      return bytes;
    }

    public static IPAddress LengthToNetmask(int masklen, int maxlen)
    {
      var bytes = new byte[maxlen / 8];
      for (var i = 0; i < masklen; i++)
        BitSet(bytes, i, true);

      return new IPAddress(bytes);
    }

    public static IPAddress Mask(IPAddress address, int masklen)
    {
      return new IPAddress(GetMaskedBytes(address, masklen));
    }

    public static int NetmaskToLength(IPAddress netmask)
    {
      var bytes = netmask.GetAddressBytes();
      for (var i = 0; i < bytes.Length * 8; i++)
        if (!BitGet(bytes, i))
          return i;

      return bytes.Length * 8;
    }
  }
}
