using System.Text;
using DotNetCommons.IO;
using DotNetCommons.Temporal;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.IO;

[TestClass]
public class InMemoryFileAccessorTests
{
    private readonly InMemoryFileAccessor _accessor;
    private readonly TestClock _clock = new();

    public InMemoryFileAccessorTests()
    {
        _accessor = new InMemoryFileAccessor(_clock);
        
        _accessor.GetDirectory("/bin", true);
        _accessor.GetDirectory("/etc", true);
        _accessor.GetDirectory("/home", true);
        _accessor.GetDirectory("/home/jim", true);
        _accessor.GetDirectory("/home/pam", true);
        _accessor.GetDirectory("/sbin", true);
        _accessor.GetDirectory("/var", true);
        _accessor.GetDirectory("/usr", true);
        _accessor.GetDirectory("/usr/bin", true);
        _accessor.GetDirectory("/usr/sbin", true);
        
        _accessor.WriteAllBytes("/bin/bash", TestFile(0xCD, 4296));
        _accessor.WriteAllBytes("/sbin/sudo", TestFile(0x41, 996));
        
        _accessor.WriteAllBytes("/bin/old", TestFile(0x42, 10));
        _accessor.SetFileTime("/bin/old", new DateTime(1996, 12, 31));
    }

    private static byte[] TestFile(byte value, int length)
    {
        var result = new byte[length];
        Array.Fill(result, value);
        return result;
    }
    
    [TestMethod]
    public void ChangeDirectory_Works()
    {
        _accessor.CurrentDirectory.Should().Be("/");
        _accessor.ChangeDirectory("usr");
        _accessor.CurrentDirectory.Should().Be("/usr");
        _accessor.ChangeDirectory("bin");
        _accessor.CurrentDirectory.Should().Be("/usr/bin");
        _accessor.ChangeDirectory("..");
        _accessor.CurrentDirectory.Should().Be("/usr");
        _accessor.ChangeDirectory("/sbin");
        _accessor.CurrentDirectory.Should().Be("/sbin");
        _accessor.ChangeDirectory("");
        _accessor.CurrentDirectory.Should().Be("/sbin");
        _accessor.ChangeDirectory("../home/pam");
        _accessor.CurrentDirectory.Should().Be("/home/pam");
        _accessor.ChangeDirectory("../..");
        _accessor.CurrentDirectory.Should().Be("/");
        _accessor.ChangeDirectory("usr/./bin/../../var");
        _accessor.CurrentDirectory.Should().Be("/var");
        _accessor.ChangeDirectory("/");
        _accessor.CurrentDirectory.Should().Be("/");
    }

    [TestMethod]
    public void CopyFile_Works()
    {
        _accessor.FileExists("/bin/bash").Should().BeTrue();
        _accessor.FileExists("/sbin/bash").Should().BeFalse();
        _accessor.CopyFile("/bin/bash", "/sbin/bash", false);
        _accessor.FileExists("/bin/bash").Should().BeTrue();
        _accessor.FileExists("/sbin/bash").Should().BeTrue();
    }
    
    [TestMethod]
    public void CopyFile_CanOverwrite_Works()
    {
        _accessor.Touch("/sbin/bash");
        _accessor.FileExists("/bin/bash").Should().BeTrue();
        _accessor.FileExists("/sbin/bash").Should().BeTrue();
        _accessor.CopyFile("/bin/bash", "/sbin/bash", true);
        _accessor.FileExists("/bin/bash").Should().BeTrue();
        _accessor.FileExists("/sbin/bash").Should().BeTrue();
        _accessor.GetFileSize("/sbin/bash").Should().Be(4296);
    }
    
    [TestMethod, ExpectedException(typeof(IOException))]
    public void CopyFile_CantOverwrite_Fails()
    {
        _accessor.Touch("/sbin/bash");
        _accessor.FileExists("/bin/bash").Should().BeTrue();
        _accessor.FileExists("/sbin/bash").Should().BeTrue();
        _accessor.CopyFile("/bin/bash", "/sbin/bash", false);
    }
    
    [TestMethod]
    public void CreateDirectory_Works()
    {
        _accessor.DirectoryExists("/home/jim/test/xyzzy").Should().BeFalse();
        _accessor.GetDirectory("/home/jim/test/xyzzy", true);
        _accessor.DirectoryExists("/home/jim/test/xyzzy").Should().BeTrue();
    }
    
    [TestMethod]
    public void CreateFile_AbsolutePath_Works()
    {
        _accessor.FileExists("/home/jim/foobar").Should().BeFalse();
        using (var stream = _accessor.OpenFile("/home/jim/foobar", FileMode.Create, FileAccess.Write))
        {
            stream.Write("Hello, world!"u8);
        }
        
        _accessor.FileExists("/home/jim/foobar").Should().BeTrue();
        _accessor.GetFileSize("/home/jim/foobar").Should().Be(13);
        _accessor.GetFileTime("/home/jim/foobar").Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(2));
    }

    [TestMethod]
    public void CreateFile_RelativePath_Works()
    {
        _accessor.ChangeDirectory("/home/pam");
        
        _accessor.FileExists("foobar").Should().BeFalse();
        using (var stream = _accessor.OpenFile("foobar", FileMode.Create, FileAccess.Write))
        {
            stream.Write("Hello, world!"u8);
        }
        
        _accessor.FileExists("/home/pam/foobar").Should().BeTrue();
        _accessor.GetFileSize("/home/pam/foobar").Should().Be(13);
        _accessor.GetFileTime("/home/pam/foobar").Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(2));
    }

    [TestMethod]
    public void CreateFile_OverExisting_Works()
    {
        _accessor.FileExists("/bin/bash").Should().BeTrue();
        using (var file = _accessor.OpenFile("/bin/bash", FileMode.Create, FileAccess.Write))
        {
            var buffer = "Hello, world!"u8.ToArray(); 
            file.Write(buffer, 0, buffer.Length);
        }
        
        _accessor.FileExists("/bin/bash").Should().BeTrue();
        _accessor.GetFileSize("/bin/bash").Should().Be(13);
        _accessor.GetFileTime("/bin/bash").Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(2));
    }

    [TestMethod]
    public void DeleteDirectory_Works()
    {
        _accessor.DirectoryExists("/var").Should().BeTrue();
        _accessor.DeleteDirectory("/var", false);
        _accessor.DirectoryExists("/var").Should().BeFalse();
    }

    [TestMethod, ExpectedException(typeof(IOException))]
    public void DeleteDirectory_NonRecursive_FailsOnNotEmpty()
    {
        _accessor.DirectoryExists("/bin").Should().BeTrue();
        _accessor.DeleteDirectory("/bin", false);
    }
    
    [TestMethod]
    public void DeleteDirectory_Recursive_Works()
    {
        _accessor.DirectoryExists("/bin").Should().BeTrue();
        _accessor.DeleteDirectory("/bin", true);
        _accessor.DirectoryExists("/bin").Should().BeFalse();
    }
    
    [TestMethod]
    public void DeleteFile_Works()
    {
        _accessor.FileExists("/bin/bash").Should().BeTrue();
        _accessor.DeleteFile("/bin/bash");
        _accessor.FileExists("/bin").Should().BeFalse();
    }
    
    [TestMethod]
    public void DeleteFile_NotExisting_ShouldBeSilent()
    {
        _accessor.DeleteFile("/bin/xyzzy");
    }
    
    [TestMethod]
    public void DirectoryExists_Works()
    {
        _accessor.DirectoryExists("/").Should().BeTrue();
        _accessor.DirectoryExists(".").Should().BeTrue();
        _accessor.DirectoryExists("/bin").Should().BeTrue();
        _accessor.DirectoryExists("bin").Should().BeTrue();
        _accessor.DirectoryExists("/sbin").Should().BeTrue();
        _accessor.DirectoryExists("sbin").Should().BeTrue();
        _accessor.DirectoryExists("/home/jim").Should().BeTrue();
        _accessor.DirectoryExists("home/jim").Should().BeTrue();
        
        _accessor.DirectoryExists("/bin/bash").Should().BeFalse();
        _accessor.DirectoryExists("/xyzzy").Should().BeFalse();
        _accessor.DirectoryExists("/xyzzy/foo").Should().BeFalse();
        _accessor.DirectoryExists("/xyzzy/foo/bar").Should().BeFalse();
        _accessor.DirectoryExists("/xyzzy/.").Should().BeFalse();
    }
    
    [TestMethod]
    public void FileExists_Works()
    {
        _accessor.FileExists("/bin/bash").Should().BeTrue();
        _accessor.FileExists("bin/bash").Should().BeTrue();
        _accessor.ChangeDirectory("/bin");
        _accessor.FileExists("bash").Should().BeTrue();
        _accessor.FileExists("./bash").Should().BeTrue();
        _accessor.ChangeDirectory("/");
        
        _accessor.FileExists("/").Should().BeFalse();
        _accessor.FileExists(".").Should().BeFalse();
        _accessor.FileExists("/bin").Should().BeFalse();
        _accessor.FileExists("bin").Should().BeFalse();
        _accessor.FileExists("/xyzzy").Should().BeFalse();
        _accessor.FileExists("/xyzzy/foo").Should().BeFalse();
        _accessor.FileExists("/xyzzy/foo/bar").Should().BeFalse();
        _accessor.FileExists("/xyzzy/.").Should().BeFalse();
    }
    
    [TestMethod]
    public void GetFiles_NonRecursive_Works()
    {
        _accessor.Touch("/foo");
        _accessor.Touch("/foobar");
        _accessor.Touch("/bar");
        _accessor.Touch("/xyzzy");
        
        var files = _accessor.GetFiles("/", "*", false).Select(x => x.FullName).OrderBy(x => x).ToList();
        files.Should().BeEquivalentTo(
            "/bar",
            "/foo", 
            "/foobar", 
            "/xyzzy" 
        );
        
        files = _accessor.GetFiles("/bin", "*", false).Select(x => x.FullName).OrderBy(x => x).ToList();
        files.Should().BeEquivalentTo(
            "/bin/bash",
            "/bin/old"
        );
        
        files = _accessor.GetFiles("/", "foo*", false).Select(x => x.FullName).OrderBy(x => x).ToList();
        files.Should().BeEquivalentTo(
            "/foo",
            "/foobar"
        );
    }
    
    [TestMethod]
    public void GetFiles_Recursive_Works()
    {
        _accessor.Touch("/foo");
        _accessor.Touch("/foobar");
        _accessor.Touch("/bar");
        _accessor.Touch("/xyzzy");
        _accessor.Touch("/sbin/foodie");
        
        var files = _accessor.GetFiles("/", "*", true).Select(x => x.FullName).OrderBy(x => x).ToList();
        files.Should().BeEquivalentTo(
            "/bar",
            "/foo", 
            "/foobar", 
            "/xyzzy",
            "/bin/bash",
            "/bin/old",
            "/sbin/sudo",
            "/sbin/foodie"
        );
        
        files = _accessor.GetFiles("/bin", "*", true).Select(x => x.FullName).OrderBy(x => x).ToList();
        files.Should().BeEquivalentTo(
            "/bin/bash",
            "/bin/old"
        );
        
        files = _accessor.GetFiles("/", "foo*", true).Select(x => x.FullName).OrderBy(x => x).ToList();
        files.Should().BeEquivalentTo(
            "/foo",
            "/foobar",
            "/sbin/foodie"
        );
    }
    
    [TestMethod]
    public void GetFileSize_Works()
    {
        _accessor.GetFileSize("/bin/bash").Should().Be(4296);
        _accessor.GetFileSize("/sbin/sudo").Should().Be(996);
    }
    
    [TestMethod, ExpectedException(typeof(FileNotFoundException))]
    public void GetFileSize_NotFound_ThrowsException()
    {
        _accessor.GetFileSize("/xyzzy/foo/bar").Should().Be(100);
    }
    
    [TestMethod]
    public void GetFileTime_Works()
    {
        _accessor.GetFileTime("/bin/bash").Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        _accessor.GetFileTime("/sbin/sudo").Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
    }
    
    [TestMethod, ExpectedException(typeof(FileNotFoundException))]
    public void GetFileTime_NotFound_ThrowsException()
    {
        _accessor.GetFileTime("/bin/xyzzy");
    }

    [TestMethod]
    public void ListFiles_Works()
    {
        _accessor.Touch("/usr/foo");
        _accessor.WriteAllText("/usr/foobar", "This is the end, beautiful friend");
        var now = _clock.Now.ToString("s");

        var files = _accessor.ListFiles("/usr")
            .Select(f => $"{f.Name},{f.Directory},{f.Size},{f.LastWriteTime:s}")
            .ToList();
        
        files.Should().BeEquivalentTo(
            $"bin,True,0,{now}",
            $"sbin,True,0,{now}",
            $"foo,False,0,{now}",
            $"foobar,False,33,{now}"
        );
        
        _accessor.ChangeDirectory("usr");
        var files2 = _accessor.ListFiles()
            .Select(f => $"{f.Name},{f.Directory},{f.Size},{f.LastWriteTime:s}")
            .ToList();
        files2.Should().BeEquivalentTo(files);
    }
    
    [TestMethod]
    public void MoveFile_Works()
    {
        _accessor.FileExists("/bin/bash").Should().BeTrue();
        _accessor.FileExists("/sbin/bash").Should().BeFalse();
        _accessor.MoveFile("/bin/bash", "/sbin/bash", false);
        _accessor.FileExists("/bin/bash").Should().BeFalse();
        _accessor.FileExists("/sbin/bash").Should().BeTrue();
    }
    
    [TestMethod]
    public void MoveFile_CanOverwrite_Works()
    {
        _accessor.Touch("/sbin/bash");
        _accessor.FileExists("/bin/bash").Should().BeTrue();
        _accessor.FileExists("/sbin/bash").Should().BeTrue();
        _accessor.MoveFile("/bin/bash", "/sbin/bash", true);
        _accessor.FileExists("/bin/bash").Should().BeFalse();
        _accessor.FileExists("/sbin/bash").Should().BeTrue();
        _accessor.GetFileSize("/sbin/bash").Should().Be(4296);
    }
    
    [TestMethod, ExpectedException(typeof(IOException))]
    public void MoveFile_CantOverwrite_Fails()
    {
        _accessor.Touch("/sbin/bash");
        _accessor.FileExists("/bin/bash").Should().BeTrue();
        _accessor.FileExists("/sbin/bash").Should().BeTrue();
        _accessor.MoveFile("/bin/bash", "/sbin/bash", false);
    }
    
    [TestMethod, ExpectedException(typeof(FileNotFoundException))]
    public void OpenFile_NotFound_Fails()
    {
        _ = _accessor.OpenFile("/bin/xyzzy", FileMode.Open, FileAccess.Read);
    }
    
    [TestMethod]
    public void OpenFile_Read_Works()
    {
        using var file = _accessor.OpenFile("/sbin/sudo", FileMode.Open, FileAccess.Read);
        file.Length.Should().Be(996);
        file.Position.Should().Be(0);
        file.CanRead.Should().BeTrue();
        file.CanWrite.Should().BeFalse();

        using var reader  = new StreamReader(file);
        
        var content = reader.ReadToEnd();
        content.Should().Be(new string('A', 996));
    }
    
    [TestMethod]
    public void ReadAllBytes_Works()
    {
        var content = _accessor.ReadAllBytes("/sbin/sudo");
        content.Length.Should().Be(996);
        Encoding.UTF8.GetString(content).Should().Be(new string('A', 996));
    }
    
    [TestMethod, ExpectedException(typeof(FileNotFoundException))]
    public void ReadAllBytes_NotFound_Fails()
    {
        _ = _accessor.ReadAllBytes("/sbin/xyzzy");
    }
    
    [TestMethod]
    public async Task ReadAllBytesAsync_Works()
    {
        var content = await _accessor.ReadAllBytesAsync("/sbin/sudo");
        content.Length.Should().Be(996);
        Encoding.UTF8.GetString(content).Should().Be(new string('A', 996));
    }
    
    [TestMethod, ExpectedException(typeof(FileNotFoundException))]
    public async Task ReadAllBytesAsync_NotFound_Fails()
    {
        _ = await _accessor.ReadAllBytesAsync("/sbin/xyzzy");
    }
    
    [TestMethod]
    public void ReadAllText_Works()
    {
        var content = _accessor.ReadAllText("/sbin/sudo");
        content.Length.Should().Be(996);
        content.Should().Be(new string('A', 996));
    }
    
    [TestMethod, ExpectedException(typeof(FileNotFoundException))]
    public void ReadAllText_NotFound_Fails()
    {
        _ = _accessor.ReadAllText("/sbin/xyzzy");
    }
    
    [TestMethod]
    public async Task ReadAllTextAsync_Works()
    {
        var content = await _accessor.ReadAllTextAsync("/sbin/sudo");
        content.Length.Should().Be(996);
        content.Should().Be(new string('A', 996));
    }
    
    [TestMethod, ExpectedException(typeof(FileNotFoundException))]
    public async Task ReadAllTextAsync_NotFound_Fails()
    {
        _ = await _accessor.ReadAllTextAsync("/sbin/xyzzy");
    }
    
    [TestMethod]
    public void Touch_Existing_Works()
    {
        _accessor.GetFileTime("/bin/old").Should().BeCloseTo(new DateTime(1996, 12, 31), TimeSpan.Zero);
        _accessor.GetFileSize("/bin/old").Should().Be(10);
        _accessor.Touch("/bin/old");
        _accessor.GetFileTime("/bin/old").Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        _accessor.GetFileSize("/bin/old").Should().Be(10);
    }
    
    [TestMethod]
    public void Touch_New_Works()
    {
        _accessor.FileExists("/bin/xyzzy").Should().BeFalse();
        _accessor.Touch("/bin/xyzzy");
        _accessor.FileExists("/bin/xyzzy").Should().BeTrue();
        _accessor.GetFileTime("/bin/xyzzy").Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        _accessor.GetFileSize("/bin/xyzzy").Should().Be(0);
    }
    
    [TestMethod]
    public void WriteAllBytes_Existing_Works()
    {
        _accessor.FileExists("/bin/bash").Should().BeTrue();
        _accessor.GetFileSize("/bin/bash").Should().Be(4296);
        _accessor.WriteAllBytes("/bin/bash", TestFile(0x61, 100));
        _accessor.FileExists("/bin/bash").Should().BeTrue();
        _accessor.GetFileSize("/bin/bash").Should().Be(100);
        _accessor.GetFileTime("/bin/bash").Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
    }
    
    [TestMethod]
    public void WriteAllBytes_NewFile_Works()
    {
        _accessor.FileExists("/bin/xyzzy").Should().BeFalse();
        _accessor.WriteAllBytes("/bin/xyzzy", TestFile(0x61, 100));
        _accessor.FileExists("/bin/xyzzy").Should().BeTrue();
        _accessor.GetFileSize("/bin/xyzzy").Should().Be(100);
        _accessor.GetFileTime("/bin/xyzzy").Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
    }
    
    [TestMethod]
    public async Task WriteAllBytesAsync_Works()
    {
        _accessor.FileExists("/bin/xyzzy").Should().BeFalse();
        await _accessor.WriteAllBytesAsync("/bin/xyzzy", TestFile(0x61, 100));
        _accessor.FileExists("/bin/xyzzy").Should().BeTrue();
        _accessor.GetFileSize("/bin/xyzzy").Should().Be(100);
        _accessor.GetFileTime("/bin/xyzzy").Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
    }
    
    [TestMethod]
    public void WriteAllText_Works()
    {
        _accessor.FileExists("/bin/xyzzy").Should().BeFalse();
        _accessor.WriteAllText("/bin/xyzzy", "abcdefgh");
        _accessor.FileExists("/bin/xyzzy").Should().BeTrue();
        _accessor.GetFileSize("/bin/xyzzy").Should().Be(8);
        _accessor.GetFileTime("/bin/xyzzy").Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
    }
    
    [TestMethod]
    public async Task WriteAllTextAsync()
    {
        _accessor.FileExists("/bin/xyzzy").Should().BeFalse();
        await _accessor.WriteAllTextAsync("/bin/xyzzy", "abcdefgh");
        _accessor.FileExists("/bin/xyzzy").Should().BeTrue();
        _accessor.GetFileSize("/bin/xyzzy").Should().Be(8);
        _accessor.GetFileTime("/bin/xyzzy").Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
    }
}