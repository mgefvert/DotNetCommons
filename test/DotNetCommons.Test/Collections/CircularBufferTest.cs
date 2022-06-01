using DotNetCommons.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace DotNetCommons.Test.Collections;

[TestClass]
public class CircularBufferTest
{
    private static readonly Random Rnd = new();

    [TestMethod]
    public void StressTest1()
    {
        var buffer = new CircularBuffer<int>(4);

        for (int i = 0; i < 1000; i++)
        {
            var n = Rnd.Next(1, 999);
            buffer.Write(n);
            Assert.AreEqual(n, buffer.Read());
        }
    }

    [TestMethod]
    public void StressTest2()
    {
        var buffer = new CircularBuffer<int>(4);

        var source = new List<int>();
        for (int i = 0; i < 999; i++)
            source.Add(Rnd.Next(1, 999));

        var result = new List<int>();

        for (int i = 0; i < 333; i++)
        {
            buffer.Write(source[i * 3]);
            buffer.Write(source[i * 3 + 1]);
            buffer.Write(source[i * 3 + 2]);

            result.Add(buffer.Read());
            result.Add(buffer.Read());
            result.Add(buffer.Read());
        }

        CollectionAssert.AreEqual(source, result);
    }

    [TestMethod]
    public void TestContains()
    {
        var buffer = new CircularBuffer<int>(4);
        Assert.IsFalse(buffer.Contains(10));

        buffer = new CircularBuffer<int>(4);
        buffer.Write(10);
        buffer.Write(20);

        Assert.IsTrue(buffer.Contains(10));
        Assert.IsTrue(buffer.Contains(20));
        Assert.IsFalse(buffer.Contains(30));

        buffer = new CircularBuffer<int>(4);
        buffer.Write(10);
        buffer.Write(20);
        buffer.Write(30);
        buffer.Write(40);

        Assert.IsTrue(buffer.Contains(30));
        Assert.IsTrue(buffer.Contains(40));
        Assert.IsFalse(buffer.Contains(50));
    }

    [TestMethod, ExpectedException(typeof(CircularBufferFullException))]
    public void TestOverflow()
    {
        var buffer = new CircularBuffer<int>(4);
        buffer.Write(10);
        buffer.Write(20);
        buffer.Write(30);
        buffer.Write(40);
        buffer.Write(50);
    }

    [TestMethod]
    public void TestReadAndWrite()
    {
        var buffer = new CircularBuffer<int>(4);
        Assert.IsTrue(buffer.Empty);
        Assert.IsFalse(buffer.Full);
        Assert.AreEqual(0, buffer.Count);
        Assert.AreEqual(4, buffer.Size);
        Assert.AreEqual("", string.Join(",", buffer.Values()));

        buffer.Write(10);
        Assert.AreEqual(1, buffer.Count);
        Assert.AreEqual(10, buffer.First);
        Assert.AreEqual(10, buffer.Last);
        Assert.IsFalse(buffer.Empty);
        Assert.IsFalse(buffer.Full);
        Assert.AreEqual("10", string.Join(",", buffer.Values()));

        buffer.Write(20);
        Assert.AreEqual(2, buffer.Count);
        Assert.AreEqual(10, buffer.First);
        Assert.AreEqual(20, buffer.Last);
        Assert.IsFalse(buffer.Empty);
        Assert.IsFalse(buffer.Full);
        Assert.AreEqual("10,20", string.Join(",", buffer.Values()));

        buffer.Write(30);
        Assert.AreEqual(3, buffer.Count);
        Assert.AreEqual(10, buffer.First);
        Assert.AreEqual(30, buffer.Last);
        Assert.IsFalse(buffer.Empty);
        Assert.IsFalse(buffer.Full);
        Assert.AreEqual("10,20,30", string.Join(",", buffer.Values()));

        buffer.Write(40);
        Assert.AreEqual(4, buffer.Count);
        Assert.AreEqual(10, buffer.First);
        Assert.AreEqual(40, buffer.Last);
        Assert.IsFalse(buffer.Empty);
        Assert.IsTrue(buffer.Full);
        Assert.AreEqual("10,20,30,40", string.Join(",", buffer.Values()));

        var n = buffer.Read();
        Assert.AreEqual(10, n);
        Assert.AreEqual(3, buffer.Count);
        Assert.AreEqual(20, buffer.First);
        Assert.AreEqual(40, buffer.Last);
        Assert.IsFalse(buffer.Empty);
        Assert.IsFalse(buffer.Full);
        Assert.AreEqual("20,30,40", string.Join(",", buffer.Values()));

        buffer.Write(50);
        Assert.AreEqual(4, buffer.Count);
        Assert.AreEqual(20, buffer.First);
        Assert.AreEqual(50, buffer.Last);
        Assert.IsFalse(buffer.Empty);
        Assert.IsTrue(buffer.Full);
        Assert.AreEqual("20,30,40,50", string.Join(",", buffer.Values()));

        n = buffer.Read();
        Assert.AreEqual(20, n);
        Assert.AreEqual(3, buffer.Count);
        Assert.AreEqual(30, buffer.First);
        Assert.AreEqual(50, buffer.Last);
        Assert.IsFalse(buffer.Empty);
        Assert.IsFalse(buffer.Full);
        Assert.AreEqual("30,40,50", string.Join(",", buffer.Values()));

        buffer.Write(60);
        Assert.AreEqual(4, buffer.Count);
        Assert.AreEqual(30, buffer.First);
        Assert.AreEqual(60, buffer.Last);
        Assert.IsFalse(buffer.Empty);
        Assert.IsTrue(buffer.Full);
        Assert.AreEqual("30,40,50,60", string.Join(",", buffer.Values()));

        n = buffer.Read();
        Assert.AreEqual(30, n);
        Assert.AreEqual(3, buffer.Count);
        Assert.AreEqual(40, buffer.First);
        Assert.AreEqual(60, buffer.Last);
        Assert.IsFalse(buffer.Empty);
        Assert.IsFalse(buffer.Full);
        Assert.AreEqual("40,50,60", string.Join(",", buffer.Values()));

        n = buffer.Read();
        Assert.AreEqual(40, n);
        Assert.AreEqual(2, buffer.Count);
        Assert.AreEqual(50, buffer.First);
        Assert.AreEqual(60, buffer.Last);
        Assert.IsFalse(buffer.Empty);
        Assert.IsFalse(buffer.Full);
        Assert.AreEqual("50,60", string.Join(",", buffer.Values()));

        n = buffer.Read();
        Assert.AreEqual(50, n);
        Assert.AreEqual(1, buffer.Count);
        Assert.AreEqual(60, buffer.First);
        Assert.AreEqual(60, buffer.Last);
        Assert.IsFalse(buffer.Empty);
        Assert.IsFalse(buffer.Full);
        Assert.AreEqual("60", string.Join(",", buffer.Values()));

        n = buffer.Read();
        Assert.AreEqual(60, n);
        Assert.AreEqual(0, buffer.Count);
        Assert.IsTrue(buffer.Empty);
        Assert.IsFalse(buffer.Full);
        Assert.AreEqual("", string.Join(",", buffer.Values()));
    }

    [TestMethod]
    public void TestSingle()
    {
        var buffer = new CircularBuffer<int>(1);
        Assert.AreEqual(0, buffer.Count);
        Assert.IsTrue(buffer.Empty);
        Assert.IsFalse(buffer.Full);
        Assert.AreEqual("", string.Join(",", buffer.Values()));

        buffer.Write(10);
        Assert.AreEqual(1, buffer.Count);
        Assert.AreEqual(10, buffer.First);
        Assert.AreEqual(10, buffer.Last);
        Assert.IsFalse(buffer.Empty);
        Assert.IsTrue(buffer.Full);
        Assert.AreEqual("10", string.Join(",", buffer.Values()));

        var n = buffer.Read();
        Assert.AreEqual(10, n);
        Assert.AreEqual(0, buffer.Count);
        Assert.IsTrue(buffer.Empty);
        Assert.IsFalse(buffer.Full);
        Assert.AreEqual("", string.Join(",", buffer.Values()));

        buffer.Write(20);
        Assert.AreEqual(1, buffer.Count);
        Assert.AreEqual(20, buffer.First);
        Assert.AreEqual(20, buffer.Last);
        Assert.IsFalse(buffer.Empty);
        Assert.IsTrue(buffer.Full);
        Assert.AreEqual("20", string.Join(",", buffer.Values()));

        n = buffer.Read();
        Assert.AreEqual(20, n);
        Assert.AreEqual(0, buffer.Count);
        Assert.IsTrue(buffer.Empty);
        Assert.IsFalse(buffer.Full);
        Assert.AreEqual("", string.Join(",", buffer.Values()));
    }

    [TestMethod, ExpectedException(typeof(CircularBufferEmptyException))]
    public void TestUnderflow_Empty()
    {
        var buffer = new CircularBuffer<int>(4);
        buffer.Read();
    }

    [TestMethod, ExpectedException(typeof(CircularBufferEmptyException))]
    public void TestUnderflow_NotEmpty()
    {
        var buffer = new CircularBuffer<int>(4);
        buffer.Write(10);
        buffer.Read();
        buffer.Read();
    }
}