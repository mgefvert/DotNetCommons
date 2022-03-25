using System;
using DotNetCommons.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.Security;

[TestClass]
public class Crc64Test
{
    [TestMethod]
    public void Test()
    { 
        var calc = Crc64.ComputeChecksum("123456789");
        Console.WriteLine($"Calculated = {calc:x16}");

        var expect = 0xe9c6d914c4b8d9caul;
        Console.WriteLine($"Expected = {expect:x16}");

        Assert.AreEqual(expect, calc);
    }
}