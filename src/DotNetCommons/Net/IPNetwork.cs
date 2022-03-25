using System;
using System.Net;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Net;

public class IPNetwork
{
    private readonly byte[] _bytes;
    private readonly int _maxLength;

    public IPAddress Address { get; }
    public int MaskLen { get; }
    public IPAddress NetMask => LengthToNetMask(MaskLen, _maxLength);

    public IPNetwork(IPAddress address, int maskLength)
    {
        Address = Mask(address, maskLength);
        MaskLen = maskLength;
        _bytes = Address.GetAddressBytes();
        _maxLength = _bytes.Length * 8;
    }

    public IPNetwork(IPAddress address) : this(address, address.GetAddressBytes().Length * 8)
    {
    }

    public IPNetwork(IPAddress address, IPAddress netMask) : this(address, NetMaskToLength(netMask))
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

    public static IPNetwork? Parse(string network)
    {
        if (TryParse(network, out var range))
            return range;

        throw new ArgumentException("Invalid IP network: " + network, nameof(network));
    }

    public static bool TryParse(string network, out IPNetwork? range)
    {
        range = null;

        if (string.IsNullOrEmpty(network))
            return false;

        var x = network.Split('/');

        IPAddress? address = null;
        var mask = 0;

        if (x.Length >= 1)
        {
            if (!IPAddress.TryParse(x[0], out address))
                return false;

            mask = address.GetAddressBytes().Length * 8;
        }

        if (x.Length == 2)
        {
            if (!int.TryParse(x[1], out mask))
                return false;
        }

        range = new IPNetwork(address!, mask);
        return true;
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

    private static byte[] GetMaskedBytes(IPAddress address, int maskLength)
    {
        var bytes = address.GetAddressBytes();
        for (var i = maskLength; i < bytes.Length * 8; i++)
            BitSet(bytes, i, false);

        return bytes;
    }

    public static IPAddress LengthToNetMask(int maskLength, int maxLength)
    {
        var bytes = new byte[maxLength / 8];
        for (var i = 0; i < maskLength; i++)
            BitSet(bytes, i, true);

        return new IPAddress(bytes);
    }

    public static IPAddress Mask(IPAddress address, int maskLength)
    {
        return new IPAddress(GetMaskedBytes(address, maskLength));
    }

    public static int NetMaskToLength(IPAddress netMask)
    {
        var bytes = netMask.GetAddressBytes();
        for (var i = 0; i < bytes.Length * 8; i++)
            if (!BitGet(bytes, i))
                return i;

        return bytes.Length * 8;
    }
}