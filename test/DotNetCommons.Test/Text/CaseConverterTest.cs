using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.Text;

[TestClass]
public class CaseConverterTest
{
    [TestMethod]
    public void CamelCase_Works() => Assert.AreEqual("thisShouldBeCamelCase", "This should be CAMEL-Case!".ToCase(CaseType.CamelCase));

    [TestMethod]
    public void CamelCase_NoStartWithDigit() => Assert.AreEqual("camelCases", "4 Camel Cases".ToCase(CaseType.CamelCase));

    [TestMethod]
    public void CamelCase_NoStartWithInvalid() => Assert.AreEqual("camelCases", "-- Camel Cases".ToCase(CaseType.CamelCase));

    [TestMethod]
    public void KebabCase_Works() => Assert.AreEqual("this-should-be-kebab-case", "This should be KEBAB-Case!".ToCase(CaseType.KebabCase));

    [TestMethod]
    public void KebabCase_StartWithDigit() => Assert.AreEqual("4-kebab-cases", "4 Kebab Cases".ToCase(CaseType.KebabCase));

    [TestMethod]
    public void PascalCase_Works() => Assert.AreEqual("ThisShouldBePascalCase", "This should be PASCAL-Case!".ToCase(CaseType.PascalCase));

    [TestMethod]
    public void PascalCase_NoStartWithDigit() => Assert.AreEqual("PascalCases", "4 Pascal Cases".ToCase(CaseType.PascalCase));

    [TestMethod]
    public void PascalCase_NoStartWithInvalid() => Assert.AreEqual("PascalCases", "-- Pascal Cases".ToCase(CaseType.PascalCase));

    [TestMethod]
    public void SnakeCase_Works() => Assert.AreEqual("this_should_be_snake_case", "This should be SNAKE-Case!".ToCase(CaseType.SnakeCase));

    [TestMethod]
    public void SnakeCase_StartWithDigit() => Assert.AreEqual("4_snake_cases", "4 Snake Cases".ToCase(CaseType.SnakeCase));

    [TestMethod]
    public void SentenceCase_Works() => Assert.AreEqual("this should be sentence case", "This should be SENTENCE-Case!".ToCase(CaseType.SentenceCase));

    [TestMethod]
    public void SentenceCase_StartWithDigit() => Assert.AreEqual("4 sentence cases", "4 Sentence Cases".ToCase(CaseType.SentenceCase));
}
