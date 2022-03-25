using System;
using System.Linq;
using System.Text;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Security;

public static class Crc32
{
    private const uint Polynomial = 0xEDB88320;
    private static readonly uint[] Data = new uint[256];

    static Crc32()
    {
        for (uint i = 0; i < Data.Length; ++i)
        {
            var temp = i;
            for (var j = 8; j > 0; --j)
                if ((temp & 1) == 1)
                    temp = (temp >> 1) ^ Polynomial;
                else
                    temp >>= 1;

            Data[i] = temp;
        }
    }

    public static byte[] ComputeChecksumBytes(byte[] bytes)
    {
        return BitConverter.GetBytes(ComputeChecksum(bytes));
    }

    public static uint ComputeChecksum(byte[] bytes)
    {
        return ~bytes.Aggregate(0xFFFF_FFFF, (c, t) => (c >> 8) ^ Data[(byte) ((c & 0xFF) ^ t)]);
    }

    public static uint ComputeChecksum(string data, Encoding? encoding = null)
    {
        return ComputeChecksum((encoding ?? Encoding.UTF8).GetBytes(data));
    }
}