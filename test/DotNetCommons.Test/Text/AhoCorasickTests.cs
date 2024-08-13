// Copyright (c) 2013 Pēteris Ņikiforovs
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using DotNetCommons.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.Text;

[TestClass]
public class Tests
{
    [TestMethod]
    public void HelloWorld()
    {
        const string text = "hello and welcome to this beautiful world!";

        var trie = new AhoCorasickTrie();
        trie.Add("hello");
        trie.Add("world");
        trie.Build();

        var matches = trie.Find(text).ToArray();

        Assert.AreEqual(2, matches.Length);
        Assert.AreEqual("hello", matches[0]);
        Assert.AreEqual("world", matches[1]);
    }

    [TestMethod]
    public void Contains()
    {
        const string text = "hello and welcome to this beautiful world!";

        var trie = new AhoCorasickTrie();
        trie.Add("hello");
        trie.Add("world");
        trie.Build();

        Assert.IsTrue(trie.Find(text).Any());
    }

    [TestMethod]
    public void LineNumbers()
    {
        const string text = "world, i hello you!";
        var words = new[] { "hello", "world" };

        var trie = new AhoCorasickTrie<int>();
        for (var i = 0; i < words.Length; i++)
            trie.Add(words[i], i);
        trie.Build();

        var lines = trie.Find(text).ToArray();

        Assert.AreEqual(2, lines.Length);
        Assert.AreEqual(1, lines[0]);
        Assert.AreEqual(0, lines[1]);
    }

    [TestMethod]
    public void Words()
    {
        var text = "one two three four".Split(' ');

        var trie = new AhoCorasickTrie<string, bool>();
        trie.Add(new[] { "three", "four" }, true);
        trie.Build();

        Assert.IsTrue(trie.Find(text).Any());
    }
}