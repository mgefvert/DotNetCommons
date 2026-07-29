using DotNetCommons.Security;
using FluentAssertions;

namespace DotNetCommonTests.Security;

[TestClass]
public class SanitizerTests
{
    public static IEnumerable<object[]> InvalidEmails()
    {
        var emails = new[]
        {
            // Null input
            null!,
            "",
            "   ",
            "\t\n\r",

            // XSS attacks
            "<script>alert('xss')</script>@example.com",
            "user<script>@example.com",
            "user@<script>example.com",
            "user@example<script>.com",
            "user@example.com<img src=x onerror=alert(1)>",
            "user@example.com<svg onload=alert(1)>",
            "'\"><script>alert(String.fromCharCode(88,83,83))</script>@example.com",
            "user@example.com' onmouseover='alert(1)",
            "user@example.com\" onerror=\"alert(1)",

            // SQL injection attacks
            "user@example.com' OR '1'='1",
            "user@example.com'; DROP TABLE users--",
            "user@example.com' UNION SELECT * FROM users--",
            "user@example.com'--",
            "user@example.com'/*",
            "user@example.com'; DELETE FROM users; --",
            "user@example.com' AND SLEEP(5)--",
            "user@example.com' AND WAITFOR DELAY '00:00:05'--",

            // Command injection attacks
            "user@example.com; ls -la",
            "user@example.com | cat /etc/passwd",
            "user@example.com && rm -rf /",
            "user@example.com `whoami`",
            "user@example.com $(whoami)",
            "user@example.com & dir",
            "user@example.com && type C:\\Windows\\System32\\config\\sam",
            "user@example.com > /dev/null",
            "user@example.com < /etc/passwd",

            // Path traversal attacks
            "user@example.com../../etc/passwd",
            "user@example.com..\\..\\windows\\system32",
            "user@example.com....//....//etc/passwd",
            "user@example.com%2e%2e%2f",
            "user@example.com%252e%252e%252f",
            "user@example.com/etc/passwd",
            "user@example.comC:\\Windows\\System32",

            // JavaScript protocol injection
            "javascript:alert(1)@example.com",
            "user@javascript:alert(1).com",
            "data:text/html,<script>alert(1)</script>@example.com",
            "vbscript:msgbox(1)@example.com",
            "file:///etc/passwd@example.com",

            // HTML injection attacks
            "<b>user</b>@example.com",
            "user@<iframe src='evil.com'></iframe>example.com",
            "<a href='http://evil.com'>user@example.com</a>",
            "user@example.com&lt;script&gt;",
            "user@example.com&#60;script&#62;",
            "user@example.com<style>body{background:red}</style>",

            // LDAP injection attacks
            "user@example.com*)(uid=*))(|(uid=*",
            "user@example.com)(cn=*))(&(cn=*",
            "user@example.com*",
            "user@example.com(",
            "user@example.com)",
            "user@example.com\\",

            // Malformed email addresses
            "user@@example.com",
            "user@example@com",
            "@example.com",
            "user@",
            "example.com",
            "user@example.com\x00",
            "user\u0001@example.com",
            "user@example.com\u001f",

            // URL encoding
            "user%40example.com",
            "user@example.com%3Cscript%3E",

            // Double encoding
            "user%2540example.com",

            // HTML entity encoding
            "user&#64;example.com",
            "user&#x40;example.com",

            // Unicode with malicious intent
            "user@example.com\u202e", // Right-to-left override
            "user\u200b@example.com", // Zero-width space
            "user@example.com\ufeff", // Zero-width no-break space
        };

        foreach (var email in emails)
            yield return [email];

        // Email addresses exceeding reasonable length (320 characters is RFC 5321 limit)
        yield return [new string('a', 65) + "@example.com"]; // Local part > 64 chars
        yield return ["user@" + new string('a', 256) + ".com"]; // Domain > 255 chars
        yield return [new string('a', 1000) + "@" + new string('b', 1000) + ".com"];

        // Buffer overflow attempt patterns
        yield return ["user@example.com" + new string('A', 10000)];
    }

    [TestMethod]
    public void EmailAddress_ValidEmails_ShouldPassThrough()
    {
        // Valid standard email addresses
        Sanitizer.EmailAddress("user@example.com").Should().Be("user@example.com");
        Sanitizer.EmailAddress("test.user@example.com").Should().Be("test.user@example.com");
        Sanitizer.EmailAddress("user+tag@example.com").Should().Be("user+tag@example.com");
        Sanitizer.EmailAddress("user_name@example.co.uk").Should().Be("user_name@example.co.uk");
        Sanitizer.EmailAddress("123@example.com").Should().Be("123@example.com");
        Sanitizer.EmailAddress("user@subdomain.example.com").Should().Be("user@subdomain.example.com");

        // Valid internationalized email addresses
        Sanitizer.EmailAddress("用户@例え.jp").Should().Be("用户@例え.jp");
        Sanitizer.EmailAddress("user@münchen.de").Should().Be("user@münchen.de");
        Sanitizer.EmailAddress("josé@españa.es").Should().Be("josé@españa.es");
    }

    [TestMethod]
    [DynamicData(nameof(InvalidEmails))]
    public void EmailAddress_InvalidEmails_ShowThrow(string email)
    {
        Sanitizer.IsValidEmailAddress(email).Should().BeFalse();
        Sanitizer.EmailAddress(email).Should().BeNull();
    }

    [TestMethod]
    [DataRow("user\r\n@example.com", "user@example.com")]
    [DataRow("user@example.com\r\n", "user@example.com")]
    public void EmailAddress_ParseableEmailAddresses_ShouldSanitize(string email, string expected)
    {
        Sanitizer.EmailAddress(email).Should().BeNull();
    }

    [TestMethod]
    public void IsValidEmailUser_ShouldValidateLocalPartRules()
    {
        Sanitizer.IsValidEmailUser("user.name+tag").Should().BeTrue();
        Sanitizer.IsValidEmailUser("josé").Should().BeTrue();
        Sanitizer.IsValidEmailUser(new string('a', 64)).Should().BeTrue();

        Sanitizer.IsValidEmailUser("").Should().BeFalse();
        Sanitizer.IsValidEmailUser(".user").Should().BeFalse();
        Sanitizer.IsValidEmailUser("user.").Should().BeFalse();
        Sanitizer.IsValidEmailUser("user..name").Should().BeFalse();
        Sanitizer.IsValidEmailUser("user name").Should().BeFalse();
        Sanitizer.IsValidEmailUser(new string('a', 65)).Should().BeFalse();
    }

    [TestMethod]
    public void IsValidHostName_ShouldValidateDnsRules()
    {
        Sanitizer.IsValidHostName("subdomain.example.com").Should().BeTrue();
        Sanitizer.IsValidHostName("münchen.de").Should().BeTrue();
        Sanitizer.IsValidHostName(new string('a', 63) + ".com").Should().BeTrue();

        Sanitizer.IsValidHostName("").Should().BeFalse();
        Sanitizer.IsValidHostName(".example.com").Should().BeFalse();
        Sanitizer.IsValidHostName("example.com-").Should().BeFalse();
        Sanitizer.IsValidHostName("example..com").Should().BeFalse();
        Sanitizer.IsValidHostName("exam_ple.com").Should().BeFalse();
        Sanitizer.IsValidHostName("example-.com").Should().BeFalse();
        Sanitizer.IsValidHostName(new string('a', 64) + ".com").Should().BeFalse();
        Sanitizer.IsValidHostName(new string('a', 250) + ".com").Should().BeFalse();
    }

    [TestMethod]
    public void FileName_ShouldTrimRemoveInvalidCharactersAndLimitLength()
    {
        Sanitizer.FileName("  report.txt  ").Should().Be("report.txt");
        Sanitizer.FileName("reports/annual.txt").Should().Be("reportsannual.txt");
        Sanitizer.FileName(new string('a', 201)).Should().Be(new string('a', 200));
    }

    [TestMethod]
    [DataRow("   ")]
    [DataRow("///")]
    [DataRow("CON")]
    [DataRow("con.txt")]
    [DataRow("LPT9.log")]
    public void FileName_InvalidOrReservedName_ShouldThrow(string fileName)
    {
        var action = () => Sanitizer.FileName(fileName);

        action.Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public void PhoneNumberToItuNumber_ShouldNormalizeCommonFormats()
    {
        Sanitizer.PhoneNumberToItuNumber("+1 (212) 555-0199").Should().Be("+12125550199");
        Sanitizer.PhoneNumberToItuNumber("+1 (212) 555-0199", "+1").Should().Be("+12125550199");
        Sanitizer.PhoneNumberToItuNumber("+1 (212) 555-0199", "+46").Should().Be("+12125550199");

        Sanitizer.PhoneNumberToItuNumber("0046 70 123 45 67").Should().Be("+46701234567");
        Sanitizer.PhoneNumberToItuNumber("0046 70 123 45 67", "+1").Should().Be("+46701234567");
        Sanitizer.PhoneNumberToItuNumber("0046 70 123 45 67", "+46").Should().Be("+46701234567");

        Sanitizer.PhoneNumberToItuNumber("070-123 45 67").Should().Be("0701234567");
        Sanitizer.PhoneNumberToItuNumber("070-123 45 67", "+1").Should().Be("+1701234567");
        Sanitizer.PhoneNumberToItuNumber("070-123 45 67", "+46").Should().Be("+46701234567");

        Sanitizer.PhoneNumberToItuNumber("212-555-0199").Should().Be("2125550199");
        Sanitizer.PhoneNumberToItuNumber("212-555-0199", "+1").Should().Be("+12125550199");
        Sanitizer.PhoneNumberToItuNumber("212-555-0199", "+46").Should().Be("+462125550199");
    }

    [TestMethod]
    public void PhoneNumberToItuNumber_EmptyOrWithoutDigits_ShouldReturnNull()
    {
        Sanitizer.PhoneNumberToItuNumber(null).Should().BeNull();
        Sanitizer.PhoneNumberToItuNumber("   ").Should().BeNull();
        Sanitizer.PhoneNumberToItuNumber("not a phone number").Should().BeNull();
        Sanitizer.PhoneNumberToItuNumber("+").Should().BeNull();
    }

    [TestMethod]
    public void StripHtmlTags_ShouldRemoveTagsAndPreserveText()
    {
        Sanitizer.StripHtmlTags("Plain text").Should().Be("Plain text");
        Sanitizer.StripHtmlTags("<b>Hello</b> <i>world</i>").Should().Be("Hello world");
        Sanitizer.StripHtmlTags("before<br>after").Should().Be("beforeafter");
        Sanitizer.StripHtmlTags("<a title=\"1 > 0\">link</a>").Should().Be("link");
        Sanitizer.StripHtmlTags("before <unfinished").Should().Be("before ");
    }

    [TestMethod]
    public void StripHtmlTags_NullOrEmpty_ShouldPassThrough()
    {
        Sanitizer.StripHtmlTags(null).Should().BeNull();
        Sanitizer.StripHtmlTags("").Should().BeEmpty();
    }
}
