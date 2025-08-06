using DotNetCommons;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommonTests;

[TestClass]
public class CommonStringExtensionsTest_Case
{
    [TestMethod]
    public void ToCase_CamelCase()
    {
        "iAmAString".ToCase(CaseType.CamelCase).Should().Be("iAmAString");
        "i-am-a-string".ToCase(CaseType.CamelCase).Should().Be("iAmAString");
        "IAmAString".ToCase(CaseType.CamelCase).Should().Be("iAmAString");
        "i am a string".ToCase(CaseType.CamelCase).Should().Be("iAmAString");
        "i_am_a_string".ToCase(CaseType.CamelCase).Should().Be("iAmAString");
        "I am a String".ToCase(CaseType.CamelCase).Should().Be("iAmAString");
    }

    [TestMethod]
    public void ToCase_KebabCase()
    {
        "iAmAString".ToCase(CaseType.KebabCase).Should().Be("i-am-a-string");
        "i-am-a-string".ToCase(CaseType.KebabCase).Should().Be("i-am-a-string");
        "IAmAString".ToCase(CaseType.KebabCase).Should().Be("i-am-a-string");
        "i am a string".ToCase(CaseType.KebabCase).Should().Be("i-am-a-string");
        "i_am_a_string".ToCase(CaseType.KebabCase).Should().Be("i-am-a-string");
        "I am a String".ToCase(CaseType.KebabCase).Should().Be("i-am-a-string");
    }

    [TestMethod]
    public void ToCase_PascalCase()
    {
        "iAmAString".ToCase(CaseType.PascalCase).Should().Be("IAmAString");
        "i-am-a-string".ToCase(CaseType.PascalCase).Should().Be("IAmAString");
        "IAmAString".ToCase(CaseType.PascalCase).Should().Be("IAmAString");
        "i am a string".ToCase(CaseType.PascalCase).Should().Be("IAmAString");
        "i_am_a_string".ToCase(CaseType.PascalCase).Should().Be("IAmAString");
        "I am a String".ToCase(CaseType.PascalCase).Should().Be("IAmAString");
    }

    [TestMethod]
    public void ToCase_SentenceCase()
    {
        "iAmAString".ToCase(CaseType.SentenceCase).Should().Be("i am a string");
        "i-am-a-string".ToCase(CaseType.SentenceCase).Should().Be("i am a string");
        "IAmAString".ToCase(CaseType.SentenceCase).Should().Be("i am a string");
        "i am a string".ToCase(CaseType.SentenceCase).Should().Be("i am a string");
        "i_am_a_string".ToCase(CaseType.SentenceCase).Should().Be("i am a string");
        "I am a String".ToCase(CaseType.SentenceCase).Should().Be("i am a string");
    }

    [TestMethod]
    public void ToCase_SnakeCase()
    {
        "iAmAString".ToCase(CaseType.SnakeCase).Should().Be("i_am_a_string");
        "i-am-a-string".ToCase(CaseType.SnakeCase).Should().Be("i_am_a_string");
        "IAmAString".ToCase(CaseType.SnakeCase).Should().Be("i_am_a_string");
        "i am a string".ToCase(CaseType.SnakeCase).Should().Be("i_am_a_string");
        "i_am_a_string".ToCase(CaseType.SnakeCase).Should().Be("i_am_a_string");
        "I am a String".ToCase(CaseType.SnakeCase).Should().Be("i_am_a_string");
    }
}