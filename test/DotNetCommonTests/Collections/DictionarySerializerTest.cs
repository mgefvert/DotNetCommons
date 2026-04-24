using DotNetCommons.Collections;

namespace DotNetCommonTests.Collections;

[TestClass]
public class DictionarySerializerTest
{
    [TestMethod]
    public void TestSaveAndLoad()
    {
        var dict = new Dictionary<int, string>
        {
            [1] = "Adam",
            [2] = "Sandy",
            [-3] = "Bertha"
        };

        var store = new DictionarySerializer();
        var memory = new MemoryStream();

        store.Save(dict, memory);
        Assert.IsGreaterThan(10, memory.Position);

        memory.Position = 0;
        var result = store.Load<int, string>(memory);

        Assert.HasCount(3, result);
        Assert.AreEqual("Adam", result[1]);
        Assert.AreEqual("Sandy", result[2]);
        Assert.AreEqual("Bertha", result[-3]);
    }

    [TestMethod]
    public void TestSpeed()
    {
        var dict = new Dictionary<int, string>();

        for (var i = 1; i <= 100000; i++)
            dict[i] = "This is string number " + i;

        var store = new DictionarySerializer();
        var memory = new MemoryStream();

        var t0 = DateTime.Now;

        store.Save(dict, memory);
        memory.Position = 0;
        var result = store.Load<int, string>(memory);

        Console.WriteLine((DateTime.Now - t0).TotalMilliseconds + " ms");

        Assert.HasCount(100000, result);
        Assert.AreEqual("This is string number 4711", result[4711]);
    }
}