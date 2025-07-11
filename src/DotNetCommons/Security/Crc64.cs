﻿using System.Text;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Security;

/// <summary>
/// CRC-64 using the Redis implementation with the Jones polynomial.
/// </summary>
public static class Crc64
{
    private static readonly ulong[] Data =
    [
        0x0000000000000000ul, 0x7ad870c830358979ul, 0xf5b0e190606b12f2ul, 0x8f689158505e9b8bul,
        0xc038e5739841b68ful, 0xbae095bba8743ff6ul, 0x358804e3f82aa47dul, 0x4f50742bc81f2d04ul,
        0xab28ecb46814fe75ul, 0xd1f09c7c5821770cul, 0x5e980d24087fec87ul, 0x24407dec384a65feul,
        0x6b1009c7f05548faul, 0x11c8790fc060c183ul, 0x9ea0e857903e5a08ul, 0xe478989fa00bd371ul,
        0x7d08ff3b88be6f81ul, 0x07d08ff3b88be6f8ul, 0x88b81eabe8d57d73ul, 0xf2606e63d8e0f40aul,
        0xbd301a4810ffd90eul, 0xc7e86a8020ca5077ul, 0x4880fbd87094cbfcul, 0x32588b1040a14285ul,
        0xd620138fe0aa91f4ul, 0xacf86347d09f188dul, 0x2390f21f80c18306ul, 0x594882d7b0f40a7ful,
        0x1618f6fc78eb277bul, 0x6cc0863448deae02ul, 0xe3a8176c18803589ul, 0x997067a428b5bcf0ul,
        0xfa11fe77117cdf02ul, 0x80c98ebf2149567bul, 0x0fa11fe77117cdf0ul, 0x75796f2f41224489ul,
        0x3a291b04893d698dul, 0x40f16bccb908e0f4ul, 0xcf99fa94e9567b7ful, 0xb5418a5cd963f206ul,
        0x513912c379682177ul, 0x2be1620b495da80eul, 0xa489f35319033385ul, 0xde51839b2936bafcul,
        0x9101f7b0e12997f8ul, 0xebd98778d11c1e81ul, 0x64b116208142850aul, 0x1e6966e8b1770c73ul,
        0x8719014c99c2b083ul, 0xfdc17184a9f739faul, 0x72a9e0dcf9a9a271ul, 0x08719014c99c2b08ul,
        0x4721e43f0183060cul, 0x3df994f731b68f75ul, 0xb29105af61e814feul, 0xc849756751dd9d87ul,
        0x2c31edf8f1d64ef6ul, 0x56e99d30c1e3c78ful, 0xd9810c6891bd5c04ul, 0xa3597ca0a188d57dul,
        0xec09088b6997f879ul, 0x96d1784359a27100ul, 0x19b9e91b09fcea8bul, 0x636199d339c963f2ul,
        0xdf7adabd7a6e2d6ful, 0xa5a2aa754a5ba416ul, 0x2aca3b2d1a053f9dul, 0x50124be52a30b6e4ul,
        0x1f423fcee22f9be0ul, 0x659a4f06d21a1299ul, 0xeaf2de5e82448912ul, 0x902aae96b271006bul,
        0x74523609127ad31aul, 0x0e8a46c1224f5a63ul, 0x81e2d7997211c1e8ul, 0xfb3aa75142244891ul,
        0xb46ad37a8a3b6595ul, 0xceb2a3b2ba0eececul, 0x41da32eaea507767ul, 0x3b024222da65fe1eul,
        0xa2722586f2d042eeul, 0xd8aa554ec2e5cb97ul, 0x57c2c41692bb501cul, 0x2d1ab4dea28ed965ul,
        0x624ac0f56a91f461ul, 0x1892b03d5aa47d18ul, 0x97fa21650afae693ul, 0xed2251ad3acf6feaul,
        0x095ac9329ac4bc9bul, 0x7382b9faaaf135e2ul, 0xfcea28a2faafae69ul, 0x8632586aca9a2710ul,
        0xc9622c4102850a14ul, 0xb3ba5c8932b0836dul, 0x3cd2cdd162ee18e6ul, 0x460abd1952db919ful,
        0x256b24ca6b12f26dul, 0x5fb354025b277b14ul, 0xd0dbc55a0b79e09ful, 0xaa03b5923b4c69e6ul,
        0xe553c1b9f35344e2ul, 0x9f8bb171c366cd9bul, 0x10e3202993385610ul, 0x6a3b50e1a30ddf69ul,
        0x8e43c87e03060c18ul, 0xf49bb8b633338561ul, 0x7bf329ee636d1eeaul, 0x012b592653589793ul,
        0x4e7b2d0d9b47ba97ul, 0x34a35dc5ab7233eeul, 0xbbcbcc9dfb2ca865ul, 0xc113bc55cb19211cul,
        0x5863dbf1e3ac9decul, 0x22bbab39d3991495ul, 0xadd33a6183c78f1eul, 0xd70b4aa9b3f20667ul,
        0x985b3e827bed2b63ul, 0xe2834e4a4bd8a21aul, 0x6debdf121b863991ul, 0x1733afda2bb3b0e8ul,
        0xf34b37458bb86399ul, 0x8993478dbb8deae0ul, 0x06fbd6d5ebd3716bul, 0x7c23a61ddbe6f812ul,
        0x3373d23613f9d516ul, 0x49aba2fe23cc5c6ful, 0xc6c333a67392c7e4ul, 0xbc1b436e43a74e9dul,
        0x95ac9329ac4bc9b5ul, 0xef74e3e19c7e40ccul, 0x601c72b9cc20db47ul, 0x1ac40271fc15523eul,
        0x5594765a340a7f3aul, 0x2f4c0692043ff643ul, 0xa02497ca54616dc8ul, 0xdafce7026454e4b1ul,
        0x3e847f9dc45f37c0ul, 0x445c0f55f46abeb9ul, 0xcb349e0da4342532ul, 0xb1eceec59401ac4bul,
        0xfebc9aee5c1e814ful, 0x8464ea266c2b0836ul, 0x0b0c7b7e3c7593bdul, 0x71d40bb60c401ac4ul,
        0xe8a46c1224f5a634ul, 0x927c1cda14c02f4dul, 0x1d148d82449eb4c6ul, 0x67ccfd4a74ab3dbful,
        0x289c8961bcb410bbul, 0x5244f9a98c8199c2ul, 0xdd2c68f1dcdf0249ul, 0xa7f41839ecea8b30ul,
        0x438c80a64ce15841ul, 0x3954f06e7cd4d138ul, 0xb63c61362c8a4ab3ul, 0xcce411fe1cbfc3caul,
        0x83b465d5d4a0eeceul, 0xf96c151de49567b7ul, 0x76048445b4cbfc3cul, 0x0cdcf48d84fe7545ul,
        0x6fbd6d5ebd3716b7ul, 0x15651d968d029fceul, 0x9a0d8ccedd5c0445ul, 0xe0d5fc06ed698d3cul,
        0xaf85882d2576a038ul, 0xd55df8e515432941ul, 0x5a3569bd451db2caul, 0x20ed197575283bb3ul,
        0xc49581ead523e8c2ul, 0xbe4df122e51661bbul, 0x3125607ab548fa30ul, 0x4bfd10b2857d7349ul,
        0x04ad64994d625e4dul, 0x7e7514517d57d734ul, 0xf11d85092d094cbful, 0x8bc5f5c11d3cc5c6ul,
        0x12b5926535897936ul, 0x686de2ad05bcf04ful, 0xe70573f555e26bc4ul, 0x9ddd033d65d7e2bdul,
        0xd28d7716adc8cfb9ul, 0xa85507de9dfd46c0ul, 0x273d9686cda3dd4bul, 0x5de5e64efd965432ul,
        0xb99d7ed15d9d8743ul, 0xc3450e196da80e3aul, 0x4c2d9f413df695b1ul, 0x36f5ef890dc31cc8ul,
        0x79a59ba2c5dc31ccul, 0x037deb6af5e9b8b5ul, 0x8c157a32a5b7233eul, 0xf6cd0afa9582aa47ul,
        0x4ad64994d625e4daul, 0x300e395ce6106da3ul, 0xbf66a804b64ef628ul, 0xc5bed8cc867b7f51ul,
        0x8aeeace74e645255ul, 0xf036dc2f7e51db2cul, 0x7f5e4d772e0f40a7ul, 0x05863dbf1e3ac9deul,
        0xe1fea520be311aaful, 0x9b26d5e88e0493d6ul, 0x144e44b0de5a085dul, 0x6e963478ee6f8124ul,
        0x21c640532670ac20ul, 0x5b1e309b16452559ul, 0xd476a1c3461bbed2ul, 0xaeaed10b762e37abul,
        0x37deb6af5e9b8b5bul, 0x4d06c6676eae0222ul, 0xc26e573f3ef099a9ul, 0xb8b627f70ec510d0ul,
        0xf7e653dcc6da3dd4ul, 0x8d3e2314f6efb4adul, 0x0256b24ca6b12f26ul, 0x788ec2849684a65ful,
        0x9cf65a1b368f752eul, 0xe62e2ad306bafc57ul, 0x6946bb8b56e467dcul, 0x139ecb4366d1eea5ul,
        0x5ccebf68aecec3a1ul, 0x2616cfa09efb4ad8ul, 0xa97e5ef8cea5d153ul, 0xd3a62e30fe90582aul,
        0xb0c7b7e3c7593bd8ul, 0xca1fc72bf76cb2a1ul, 0x45775673a732292aul, 0x3faf26bb9707a053ul,
        0x70ff52905f188d57ul, 0x0a2722586f2d042eul, 0x854fb3003f739fa5ul, 0xff97c3c80f4616dcul,
        0x1bef5b57af4dc5adul, 0x61372b9f9f784cd4ul, 0xee5fbac7cf26d75ful, 0x9487ca0fff135e26ul,
        0xdbd7be24370c7322ul, 0xa10fceec0739fa5bul, 0x2e675fb4576761d0ul, 0x54bf2f7c6752e8a9ul,
        0xcdcf48d84fe75459ul, 0xb71738107fd2dd20ul, 0x387fa9482f8c46abul, 0x42a7d9801fb9cfd2ul,
        0x0df7adabd7a6e2d6ul, 0x772fdd63e7936baful, 0xf8474c3bb7cdf024ul, 0x829f3cf387f8795dul,
        0x66e7a46c27f3aa2cul, 0x1c3fd4a417c62355ul, 0x935745fc4798b8deul, 0xe98f353477ad31a7ul,
        0xa6df411fbfb21ca3ul, 0xdc0731d78f8795daul, 0x536fa08fdfd90e51ul, 0x29b7d047efec8728ul
    ];

    /// <summary>
    /// Calculate a CRC-64 value from a byte buffer, expressed as eight bytes.
    /// </summary>
    public static byte[] ComputeChecksumBytes(byte[] bytes)
    {
        return BitConverter.GetBytes(ComputeChecksum(bytes));
    }

    /// <summary>
    /// Calculate a CRC-64 value from a byte buffer, expressed as an unsigned int64.
    /// </summary>
    public static ulong ComputeChecksum(byte[] bytes)
    {
        return bytes.Aggregate(0ul, (current, b) => (current >> 8) ^ Data[(byte)current ^ b]);
    }

    /// <summary>
    /// Calculate a CRC-64 value from a string buffer, expressed as an unsigned int64.
    /// </summary>
    public static ulong ComputeChecksum(string data, Encoding? encoding = null)
    {
        return ComputeChecksum((encoding ?? Encoding.UTF8).GetBytes(data));
    }
}