using System.Text;
using DotNetCommons.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.IO;

[TestClass]
public class MemoryBlockTest
{
    private static byte[] MakeLoopingTestBuffer(int size)
    {
        var  buffer = new byte[size];
        byte value  = 0;
        for (var i = 0; i < buffer.Length; i++)
            buffer[i] = value++;
        return buffer;
    }

    private static MemoryBlock MakeTestBlock(int repetitions = 1024)
    {
        var result = new MemoryBlock();
        var buffer = "abcdefghijklmnopqrstuvwxyz123456"u8.ToArray();

        for (var i = 0; i < repetitions; i++)
            result.Write(buffer, (ulong)i * 32);
        
        return result;
    }

    [TestMethod]
    public void Test()
    {
        var mem = new MemoryBlock();
        mem.Capacity.Should().Be(0);
        mem.Length.Should().Be(0);

        var buffer = new byte[1024];
        mem.Read(buffer, 0, 1024).Should().Be(0);

        buffer = new byte[1048576];
        mem.Read(buffer, 1048576, 1024).Should().Be(0);

        buffer = "Hello, world"u8.ToArray(); 
        mem.Write(buffer, 0, buffer.Length);
        mem.Capacity.Should().Be(1024);
        mem.Length.Should().Be(12);

        mem.Write([], 1048576, 0);
        mem.Length.Should().Be(1048576);

        buffer = new byte[10];
        mem.Read(buffer, 10_000, 10).Should().Be(10);
        buffer.Should().BeEquivalentTo(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
        
        buffer = MakeLoopingTestBuffer(80_000_000);
        
        mem.Write(buffer, 12, buffer.Length);
        mem.Capacity.Should().Be(90698752ul);
        mem.Length.Should().Be(80_000_012);

        buffer = new byte[15];
        mem.Read(buffer, 0, 14).Should().Be(14);
        buffer.Should().BeEquivalentTo("Hello, world\x00\x01\x00"u8.ToArray());

        buffer = new byte[60_000_000];
        mem.Read(buffer, 8_000_000, 60_000_000).Should().Be(60_000_000);
        byte value = 244;
        for (var i = 0; i < buffer.Length; i++)
        {
            if (buffer[i] != value)
                Assert.Fail($"Large buffer mismatch in position {i}: Got {buffer[i]}, expected {value}");
            value++;
        }
        
        mem.Read(buffer, 79_000_000, 10_000_000).Should().Be(1_000_012);
        value = 180;
        for (var i = 0; i < 1_000_012; i++)
        {
            if (buffer[i] != value)
                Assert.Fail($"Large buffer mismatch in position {i}: Got {buffer[i]}, expected {value}");
            value++;
        }
    }

    // This test uses 20 GB of memory. Only run when necessary.
    //[TestMethod]          
    public void HugeTest()
    {
        var mem = new MemoryBlock();
        mem.Capacity.Should().Be(0);
        mem.Length.Should().Be(0);

        var buffer = MakeLoopingTestBuffer(1048576);

        ulong position = 0;
        for (var i = 0; i < 20_000; i++)
        {
            mem.Write(buffer, position, buffer.Length);
            position += (uint)buffer.Length;
        }
        
        mem.Length.Should().Be(20_000 * 1048576ul);

        buffer = new byte[2 * 1048576];
        mem.Read(buffer, 19_999 * 1048576ul, 2 * 1048576).Should().Be(1048576);
        byte value = 0;
        for (var i = 0; i < buffer.Length; i++)
        {
            if (buffer[i] != value)
                Assert.Fail($"Large buffer mismatch in position {i}: Got {buffer[i]}, expected {value}");
            value++;
        }
        
        Console.WriteLine(GC.GetTotalMemory(true).ToString("N0") + " bytes allocated");
        
        mem.Clear();

        Console.WriteLine(GC.GetTotalMemory(true).ToString("N0") + " bytes allocated");
    }

    [TestMethod]
    public void Clone_Works()
    {
        var mem = MakeTestBlock();

        var clone = mem.Clone();
        clone.Capacity.Should().Be(mem.Capacity);
        clone.Length.Should().Be(mem.Length);

        var memBuffer   = mem.ToArray();
        var cloneBuffer = mem.ToArray();
        memBuffer.Length.Should().Be(cloneBuffer.Length);

        for (int i = 0; i < memBuffer.Length; i++)
            if (memBuffer[i] != cloneBuffer[i])
                Assert.Fail($"Buffer mismatch in position {i}: Got {memBuffer[i]}, expected {cloneBuffer[i]}");
    }

    [TestMethod]
    public void InternalMapPosition_Test()
    {
        var mem = MakeTestBlock(64);
        mem.Capacity.Should().Be(4096);
        mem.Length.Should().Be(2048);
        mem.InternalMapPosition(0).Should().Be((0, 0));
        mem.InternalMapPosition(999).Should().Be((0, 999));
        mem.InternalMapPosition(1023).Should().Be((0, 1023));
        mem.InternalMapPosition(1024).Should().Be((1, 0));
        mem.InternalMapPosition(2047).Should().Be((1, 1023));
        mem.InternalMapPosition(2048).Should().Be((1, 1024));
        mem.InternalMapPosition(2049).Should().Be((1, 1025));
        mem.InternalMapPosition(5000).Should().Be((-1, 0));
    }
    
    [TestMethod]
    public void LoadFrom_Append_Works()
    {
        var stream1 = new MemoryStream("abc"u8.ToArray());
        var stream2 = new MemoryStream("xyz"u8.ToArray());
        var stream3 = new MemoryStream("123456789"u8.ToArray());

        var mem = new MemoryBlock();
        mem.LoadFrom(stream1, true);
        mem.LoadFrom(stream2, true);
        mem.LoadFrom(stream3, true);

        var str = Encoding.UTF8.GetString(mem.ToArray());
        str.Should().Be("abcxyz123456789");
    }

    [TestMethod]
    public void LoadFrom_SmallFile_Works()
    {
        var buffer = MakeLoopingTestBuffer(32768);
        var stream = new MemoryStream(buffer);

        var mem = new MemoryBlock();
        mem.LoadFrom(stream, false);

        var  buffer2 = mem.ToArray();
        byte value   = 0;
        for (var i = 0; i < buffer2.Length; i++)
        {
            if (buffer2[i] != value)
                Assert.Fail($"Large buffer mismatch in position {i}: Got {buffer2[i]}, expected {value}");
            value++;
        }
    }
    
    [TestMethod]
    public void LoadFrom_LargeFile_Works()
    {
        var buffer = MakeLoopingTestBuffer(100_000_000);
        var stream = new MemoryStream(buffer);

        var mem = new MemoryBlock();
        mem.LoadFrom(stream, false);

        buffer = mem.ToArray();
        byte value = 0;
        for (var i = 0; i < buffer.Length; i++)
        {
            if (buffer[i] != value)
                Assert.Fail($"Large buffer mismatch in position {i}: Got {buffer[i]}, expected {value}");
            value++;
        }
    }
    
    [TestMethod]
    public void Sha256_WriteClearsStatus()
    {
        var mem    = new MemoryBlock();
        var buffer = "abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq"u8.ToArray(); 
        mem.Write(buffer, 0);

        var hash = mem.GetSha256();
        hash.Should().BeEquivalentTo(new byte[] {
            0x24, 0x8d, 0x6a, 0x61, 0xd2, 0x06, 0x38, 0xb8, 0xe5, 0xc0, 0x26, 0x93, 0x0c, 0x3e, 0x60, 0x39, 
            0xa3, 0x3c, 0xe4, 0x59, 0x64, 0xff, 0x21, 0x67, 0xf6, 0xec, 0xed, 0xd4, 0x19, 0xdb, 0x06, 0xc1
        });

        buffer = "z"u8.ToArray();
        mem.Write(buffer, 0);
        
        var hash2 = mem.GetSha256();
        hash2.Should().NotBeEquivalentTo(hash);
    }

    [TestMethod]
    public void Sha256_SingleBlock_Works()
    {
        var mem = new MemoryBlock("abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq"u8);

        var hash = mem.GetSha256();
        hash.Should().BeEquivalentTo(new byte[] {
            0x24, 0x8d, 0x6a, 0x61, 0xd2, 0x06, 0x38, 0xb8, 0xe5, 0xc0, 0x26, 0x93, 0x0c, 0x3e, 0x60, 0x39, 
            0xa3, 0x3c, 0xe4, 0x59, 0x64, 0xff, 0x21, 0x67, 0xf6, 0xec, 0xed, 0xd4, 0x19, 0xdb, 0x06, 0xc1
        });

        var hash2 = mem.GetSha256();
        hash.Should().BeEquivalentTo(hash2);
    }

    [TestMethod]
    public void Sha256_MultipleBlock_Works()
    {
        var mem = MakeTestBlock();
        
        var hash = mem.GetSha256();
        hash.Should().BeEquivalentTo(new byte[] {
            0x3c, 0xab, 0xc3, 0x3e, 0xdb, 0xd8, 0x13, 0xcc, 0xd2, 0x69, 0x7a, 0xfc, 0xb9, 0x67, 0xee, 0x74, 
            0xe0, 0x04, 0xfe, 0x3e, 0x3a, 0xa1, 0x92, 0xfa, 0x88, 0x7a, 0x7b, 0x05, 0x7c, 0x64, 0x57, 0x26
        });

        var hash2 = mem.GetSha256();
        hash.Should().BeEquivalentTo(hash2);
    }

    [TestMethod]
    public void SaveTo_Works()
    {
        var mem    = MakeTestBlock();
        var stream = new MemoryStream();
        mem.SaveTo(stream);

        stream.Length.Should().Be(32768);
        var str = Encoding.UTF8.GetString(stream.ToArray());
        str.Should().StartWith("abcdefghijklmnopqrstuvwxyz123456abcdefghijklmnopqrstuvwxyz123456");
        str.Should().EndWith("abcdefghijklmnopqrstuvwxyz123456abcdefghijklmnopqrstuvwxyz123456");
    }

    [TestMethod]
    public void ToArray_Works()
    {
        var mem    = MakeTestBlock();
        var buffer = mem.ToArray();

        buffer.Length.Should().Be(32768);
        var str = Encoding.UTF8.GetString(buffer);
        str.Should().StartWith("abcdefghijklmnopqrstuvwxyz123456abcdefghijklmnopqrstuvwxyz123456");
        str.Should().EndWith("abcdefghijklmnopqrstuvwxyz123456abcdefghijklmnopqrstuvwxyz123456");
    }
    
    [TestMethod]
    public void UpdateLength_Decrease_Works()
    {
        var mem    = MakeTestBlock(32768);
        mem.Length = 10000;
        mem.Length.Should().Be(10000);
        mem.Capacity.Should().Be(13312);
    }
    
    [TestMethod]
    public void UpdateLength_Increase_Works()
    {
        var mem    = MakeTestBlock();
        mem.Length = 1048576;
        mem.Length.Should().Be(1048576);
        mem.Capacity.Should().Be(1119232);
    }
}