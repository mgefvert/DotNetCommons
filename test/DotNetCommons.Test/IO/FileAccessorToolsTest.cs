using DotNetCommons.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.IO;

[TestClass]
public class FileAccessorToolsTest
{
    [TestMethod]
    public void Glob_FileSystemAccessor_DriveRoot_Works()
    {
        var fileAccessor = new FileSystemAccessor();
        
        var files = fileAccessor.Glob(@"c:\w\.\prj\..\prj\*\Snipes\*.h")
            .Select(x => x.FullName.ToLower())
            .ToList();
        
        files.Should().BeEquivalentTo(
            @"c:\w\prj\cpp\snipes\config-sample.h",
            @"c:\w\prj\cpp\snipes\config.h",
            @"c:\w\prj\cpp\snipes\console.h",
            @"c:\w\prj\cpp\snipes\keyboard.h",
            @"c:\w\prj\cpp\snipes\macros.h",
            @"c:\w\prj\cpp\snipes\platform.h",
            @"c:\w\prj\cpp\snipes\snipes.h",
            @"c:\w\prj\cpp\snipes\sound.h",
            @"c:\w\prj\cpp\snipes\timer.h",
            @"c:\w\prj\cpp\snipes\types.h"
        );
    }

    [TestMethod]
    public void Glob_FileSystemAccessor_Root_Works()
    {
        var fileAccessor = new FileSystemAccessor();
        
        var files = fileAccessor.Glob(@"\w\prj\*\Snipes\*.h")
            .Select(x => x.FullName.ToLower())
            .ToList();
        
        files.Should().BeEquivalentTo(
            @"c:\w\prj\cpp\snipes\config-sample.h",
            @"c:\w\prj\cpp\snipes\config.h",
            @"c:\w\prj\cpp\snipes\console.h",
            @"c:\w\prj\cpp\snipes\keyboard.h",
            @"c:\w\prj\cpp\snipes\macros.h",
            @"c:\w\prj\cpp\snipes\platform.h",
            @"c:\w\prj\cpp\snipes\snipes.h",
            @"c:\w\prj\cpp\snipes\sound.h",
            @"c:\w\prj\cpp\snipes\timer.h",
            @"c:\w\prj\cpp\snipes\types.h"
        );
    }
}