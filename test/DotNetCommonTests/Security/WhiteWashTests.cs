using DotNetCommons.Security;
using FluentAssertions;

namespace DotNetCommonTests.Security;

[TestClass]
public class WhiteWashTests
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
        WhiteWash.EmailAddress("user@example.com").Should().Be("user@example.com");
        WhiteWash.EmailAddress("test.user@example.com").Should().Be("test.user@example.com");
        WhiteWash.EmailAddress("user+tag@example.com").Should().Be("user+tag@example.com");
        WhiteWash.EmailAddress("user_name@example.co.uk").Should().Be("user_name@example.co.uk");
        WhiteWash.EmailAddress("123@example.com").Should().Be("123@example.com");
        WhiteWash.EmailAddress("user@subdomain.example.com").Should().Be("user@subdomain.example.com");

        // Valid internationalized email addresses
        WhiteWash.EmailAddress("用户@例え.jp").Should().Be("用户@例え.jp");
        WhiteWash.EmailAddress("user@münchen.de").Should().Be("user@münchen.de");
        WhiteWash.EmailAddress("josé@españa.es").Should().Be("josé@españa.es");
    }

    [TestMethod]
    [DynamicData(nameof(InvalidEmails))]
    public void EmailAddress_InvalidEmails_ShowThrow(string email)
    {
        WhiteWash.IsValidEmailAddress(email).Should().BeFalse();
        WhiteWash.EmailAddress(email).Should().BeNull();
    }

    [TestMethod]
    [DataRow("user\r\n@example.com", "user@example.com")]
    [DataRow("user@example.com\r\n", "user@example.com")]
    public void EmailAddress_ParseableEmailAddresses_ShouldSanitize(string email, string expected)
    {
        WhiteWash.EmailAddress(email).Should().Be(expected);
    }
}