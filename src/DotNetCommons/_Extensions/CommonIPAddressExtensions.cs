using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;

namespace DotNetCommons;

public static class CommonIPAddressExtensions
{
    extension(IPAddress address)
    {
        public static IPAddress FromUInt32(uint value)
        {
            Span<byte> bytes = stackalloc byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(bytes, value);
            return new IPAddress(bytes);
        }

        public static IPAddress FromUInt64(ulong value1, ulong value2)
        {
            Span<byte> bytes = stackalloc byte[16];
            BinaryPrimitives.WriteUInt64BigEndian(bytes[..8], value1);
            BinaryPrimitives.WriteUInt64BigEndian(bytes[8..], value2);
            return new IPAddress(bytes);
        }

        public static IPAddress FromUInt128(UInt128 value)
        {
            Span<byte> bytes = stackalloc byte[16];
            BinaryPrimitives.WriteUInt128BigEndian(bytes, value);
            return new IPAddress(bytes);
        }

        public bool ToUInt32(out uint a)
        {
            a = 0;
            if (address.AddressFamily != AddressFamily.InterNetwork)
                return false;

            Span<byte> bytes = stackalloc byte[4];
            address.TryWriteBytes(bytes, out _);

            a = BinaryPrimitives.ReadUInt32BigEndian(bytes);
            return true;
        }

        public bool ToUInt64(out ulong a1, out ulong a2)
        {
            a1 = a2 = 0;

            if (address.AddressFamily == AddressFamily.InterNetwork)
                address = address.MapToIPv6();
            if (address.AddressFamily != AddressFamily.InterNetworkV6)
                return false;

            Span<byte> bytes = stackalloc byte[16];
            address.TryWriteBytes(bytes, out _);

            a1 = BinaryPrimitives.ReadUInt64BigEndian(bytes[..8]);
            a2 = BinaryPrimitives.ReadUInt64BigEndian(bytes[8..]);
            return true;
        }

        public bool ToUInt128(out UInt128 a)
        {
            a = 0;

            if (address.AddressFamily == AddressFamily.InterNetwork)
                address = address.MapToIPv6();
            if (address.AddressFamily != AddressFamily.InterNetworkV6)
                return false;

            Span<byte> bytes = stackalloc byte[16];
            address.TryWriteBytes(bytes, out _);

            a = BinaryPrimitives.ReadUInt128BigEndian(bytes);
            return true;
        }
    }
}