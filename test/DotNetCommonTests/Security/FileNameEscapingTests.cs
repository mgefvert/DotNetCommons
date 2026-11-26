using DotNetCommons.Security;

namespace DotNetCommonTests.Security;

[TestClass]
public class FileNameEscapingTests
{
    [TestMethod]
    [DataRow("orders-2026.sql", "orders-2026.sql")]
    [DataRow(" caf\u00e9 \u6771\U0001f600.sql", " caf\u00e9 \u6771\U0001f600.sql")]
    [DataRow(".notes", ".notes")]
    [DataRow(".", "%2E")]
    [DataRow("..", "%2E%2E")]
    [DataRow(" \t. ", " %09%2E%20")]
    [DataRow("name . .", "name%20%2E%20%2E")]
    [DataRow("../elsewhere", "..%2Felsewhere")]
    [DataRow(@"\\server\share", "%5C%5Cserver%5Cshare")]
    [DataRow(@"C:\file:stream", "C%3A%5Cfile%3Astream")]
    [DataRow("%2F", "%252F")]
    [DataRow("NuL.backup.sql", "%4EuL.backup.sql")]
    [DataRow("COM0.log", "%43OM0.log")]
    [DataRow("CONIN$.txt", "%43ONIN$.txt")]
    [DataRow("conout$", "%63onout$")]
    [DataRow("COM\u00b9.sql", "%43OM\u00b9.sql")]
    [DataRow("LPT\u00b2 .txt", "%4CPT\u00b2 .txt")]
    [DataRow("lpt\u00b3", "%6Cpt\u00b3")]
    [DataRow("LPT0", "%4CPT0")]
    [DataRow("COM10", "COM10")]
    [DataRow("COM\u2074", "COM\u2074")]
    [DataRow("company.sql", "company.sql")]
    public void EscapesFilenameComponents(string name, string expected)
    {
        var escaped = Sanitizer.EscapeFileName(name);

        Assert.AreEqual(expected, escaped);
        Assert.AreEqual(name, Uri.UnescapeDataString(escaped));
        Assert.AreEqual(escaped, Path.GetFileName(escaped));
    }

    [TestMethod]
    public void AllAsciiCharactersRoundTripWithoutCollisions()
    {
        const string invalidCharacters = "\"<>:/\\|?*";
        var escapedNames = new HashSet<string>(StringComparer.Ordinal);
        for (var code = 0; code <= 127; code++)
        {
            var name = ((char)code).ToString();
            var escaped = Sanitizer.EscapeFileName(name);

            Assert.IsTrue(escapedNames.Add(escaped), $"Collision for ASCII {code}.");
            Assert.IsFalse(escaped.Any(c => char.IsControl(c) || invalidCharacters.Contains(c)));
            Assert.IsFalse(escaped.EndsWith(' ') || escaped.EndsWith('.'));
            Assert.AreEqual(name, Uri.UnescapeDataString(escaped));
        }
    }

    [TestMethod]
    public void EscapesEveryReservedPortNumberWithOrWithoutExtension()
    {
        foreach (var stem in new[] { "CON", "PRN", "AUX", "NUL" }
                     .Concat("0123456789\u00b9\u00b2\u00b3".SelectMany(digit => new[] { $"COM{digit}", $"LPT{digit}" })))
        {
            foreach (var ending in new[] { "", ".first.second", " .sql" })
            {
                var name = stem.ToLowerInvariant() + ending;
                var escaped = Sanitizer.EscapeFileName(name);
                Assert.StartsWith("%", escaped);
                Assert.AreEqual(name, Uri.UnescapeDataString(escaped));
            }
        }
    }

    [TestMethod]
    public void PreservesLongNames()
    {
        var name = new string('a', 210) + ".log";
        Assert.AreEqual(name, Sanitizer.EscapeFileName(name));
    }

    [TestMethod]
    public void RejectsMissingNamesAndMalformedUnicode()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => Sanitizer.EscapeFileName(null!));
        Assert.ThrowsExactly<ArgumentException>(() => Sanitizer.EscapeFileName(""));
        Assert.ThrowsExactly<System.Text.EncoderFallbackException>(() => Sanitizer.EscapeFileName("\ud800"));
        Assert.ThrowsExactly<System.Text.EncoderFallbackException>(() => Sanitizer.EscapeFileName("\udc00"));
    }
}
