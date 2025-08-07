using DotNetCommons.IO;
using FluentAssertions;

namespace DotNetCommonTests.IO;

[TestClass]
public class FileAccessorToolsTest
{
    private IFileAccessor _fileAccessor = null!;
    private IFileItem _testPath = null!;

    [TestInitialize]
    public void Setup()
    {
        _fileAccessor = new InMemoryFileAccessor(TimeProvider.System);
        _testPath     = _fileAccessor.GetDirectory("/w/prj/cpp/snipes", true) ?? throw new Exception("Failed to create test path");

        _fileAccessor.Touch(_testPath.FullName + "/config.h");
        _fileAccessor.Touch(_testPath.FullName + "/console.h");
        _fileAccessor.Touch(_testPath.FullName + "/macros.h");
        _fileAccessor.Touch(_testPath.FullName + "/snipes.h");
    }

    [TestCleanup]
    public void Teardown()
    {
        _fileAccessor.DeleteDirectory(_testPath.FullName, true);
    }

    [TestMethod]
    public void Glob_FileSystemAccessor_Works()
    {
        var files = _fileAccessor.Glob("/w/./prj/../prj/*/Snipes/*.h")
            .Select(x => x.FullName.ToLower())
            .ToList();
        
        files.Should().BeEquivalentTo(
            "/w/prj/cpp/snipes/config.h",
            "/w/prj/cpp/snipes/console.h",
            "/w/prj/cpp/snipes/macros.h",
            "/w/prj/cpp/snipes/snipes.h"
        );
    }
}