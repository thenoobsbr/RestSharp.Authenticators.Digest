using System;
using System.Reflection;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Shouldly;

using Xunit;

namespace RestSharp.Authenticators.Digest.UnitTests;

[Trait("Category", "UnitTest")]
[Trait("Class", nameof(DigestAuthenticatorManager))]
public class DigestAuthenticatorManagerExtendedTest
{
    [Theory]
    [InlineData(Method.Get)]
    [InlineData(Method.Post)]
    [InlineData(Method.Put)]
    [InlineData(Method.Delete)]
    [InlineData(Method.Patch)]
    [InlineData(Method.Head)]
    [InlineData(Method.Options)]
    public void Given_AllHttpMethods_When_GetDigestHeader_Then_StartsWithDigest(Method method)
    {
        var manager = CreateManager();
        var header = manager.GetDigestHeader("/test", method);

        header.ShouldStartWith("Digest ");
    }

    [Fact]
    public void Given_DifferentCredentials_When_GetDigestHeader_Then_ResponseDiffers()
    {
        var manager1 = CreateManager("user1", "pass1");
        var manager2 = CreateManager("user2", "pass2");

        var header1 = manager1.GetDigestHeader("/test", Method.Get);
        var header2 = manager2.GetDigestHeader("/test", Method.Get);

        header1.ShouldContain("username=\"user1\"");
        header2.ShouldContain("username=\"user2\"");
        // Different credentials should produce different response hashes
        header1.ShouldNotBe(header2);
    }

    [Fact]
    public void Given_DifferentPaths_When_GetDigestHeader_Then_UriMatchesPath()
    {
        var manager = CreateManager();

        var header1 = manager.GetDigestHeader("/api/v1/users", Method.Get);
        var header2 = manager.GetDigestHeader("/api/v2/orders", Method.Post);

        header1.ShouldContain("uri=\"/api/v1/users\"");
        header2.ShouldContain("uri=\"/api/v2/orders\"");
    }

    [Fact]
    public void Given_Rfc2069HeaderWithoutQop_When_GetDigestHeader_Then_DoesNotContainCnonce()
    {
        // RFC 2069 does not use cnonce since there is no qop
        var manager = CreateManager();
        var header = manager.GetDigestHeader("/path", Method.Get);

        header.ShouldNotContain("cnonce=");
    }

    [Fact]
    public void Given_Rfc2069HeaderWithoutQop_When_GetDigestHeader_Then_DoesNotContainNonceCount()
    {
        // RFC 2069 does not use nonce count since there is no qop
        var manager = CreateManager();
        var header = manager.GetDigestHeader("/path", Method.Get);

        header.ShouldNotContain("nc=");
    }

    [Fact]
    public void Given_Rfc2069HeaderWithoutQop_When_GetDigestHeader_Then_DoesNotContainQop()
    {
        // RFC 2069 servers do not send qop, so the header should not include qop, nc, or cnonce
        var manager = CreateManager();
        var header = manager.GetDigestHeader("/path", Method.Get);

        header.ShouldNotContain("qop=");
        header.ShouldNotContain("nc=");
        header.ShouldNotContain("cnonce=");
    }

    [Fact]
    public void Given_Rfc2617WithQop_When_GetDigestHeader_Then_ContainsCnonce()
    {
        var manager = CreateManagerWithQop();
        var header = manager.GetDigestHeader("/path", Method.Get);

        header.ShouldContain("cnonce=\"");
    }

    [Fact]
    public void Given_Rfc2617WithQop_When_GetDigestHeader_Then_ContainsNonceCount()
    {
        var manager = CreateManagerWithQop();
        var header = manager.GetDigestHeader("/path", Method.Get);

        header.ShouldContain("nc=00000001");
    }

    [Fact]
    public void Given_Rfc2617WithQop_When_GetDigestHeader_Then_ContainsQop()
    {
        var manager = CreateManagerWithQop();
        var header = manager.GetDigestHeader("/path", Method.Get);

        header.ShouldContain("qop=auth");
    }

    [Fact]
    public void Given_DigestData_When_GetDigestHeader_Then_ContainsResponse()
    {
        var manager = CreateManager();
        var header = manager.GetDigestHeader("/path", Method.Get);

        header.ShouldContain("response=\"");
    }

    [Fact]
    public void Given_GetDigestHeader_When_Called_Then_ResponseIsHexString()
    {
        var manager = CreateManager();
        var header = manager.GetDigestHeader("/test", Method.Get);

        // Extract the response value
        var responseStart = header.IndexOf("response=\"", StringComparison.Ordinal) + 10;
        var responseEnd = header.IndexOf('"', responseStart);

        var response = header[responseStart..responseEnd];

        // MD5 produces 32 hex chars
        response.Length.ShouldBe(32);
        response.ShouldMatch("^[0-9a-f]{32}$");
    }

    [Fact]
    public void Given_HostWithPort_When_Creating_Then_Succeeds()
    {
        var manager = new DigestAuthenticatorManager(
            new Uri("http://localhost:8080"),
            "user",
            "pass",
            TimeSpan.FromSeconds(30),
            null,
            NullLogger.Instance);

        manager.ShouldNotBeNull();
    }

    [Fact]
    public void Given_HttpsHost_When_Creating_Then_Succeeds()
    {
        var manager = new DigestAuthenticatorManager(
            new Uri("https://secure.example.com"),
            "user",
            "pass",
            TimeSpan.FromSeconds(30),
            null,
            NullLogger.Instance);

        manager.ShouldNotBeNull();
    }

    [Fact]
    public void Given_LargeTimeout_When_Creating_Then_Succeeds()
    {
        var manager = new DigestAuthenticatorManager(
            new Uri("http://localhost"),
            "user",
            "pass",
            TimeSpan.FromHours(1),
            null,
            NullLogger.Instance);

        manager.ShouldNotBeNull();
    }

    [Fact]
    public void Given_PathWithQueryString_When_GetDigestHeader_Then_UriContainsFullPath()
    {
        var manager = CreateManager();
        var header = manager.GetDigestHeader("/api/values?id=123&type=test", Method.Get);

        header.ShouldContain("uri=\"/api/values?id=123&type=test\"");
    }

    [Fact]
    public void Given_RootPath_When_GetDigestHeader_Then_UriIsRoot()
    {
        var manager = CreateManager();
        var header = manager.GetDigestHeader("/", Method.Get);

        header.ShouldContain("uri=\"/\"");
    }

    [Fact]
    public void Given_SameInputs_When_GetDigestHeaderCalledTwice_Then_FormatIsConsistent()
    {
        var manager = CreateManager();

        var header1 = manager.GetDigestHeader("/test", Method.Get);
        var header2 = manager.GetDigestHeader("/test", Method.Get);

        header1.ShouldStartWith("Digest ");
        header2.ShouldStartWith("Digest ");
        // Both should have the same structure
        header1.ShouldContain("algorithm=MD5");
        header2.ShouldContain("algorithm=MD5");
    }

    [Fact]
    public void Given_SpecialCharsInCredentials_When_GetDigestHeader_Then_UsernameIsIncluded()
    {
        var manager = CreateManager("user@domain.com", "p@ss!w0rd#");
        var header = manager.GetDigestHeader("/test", Method.Get);

        header.ShouldContain("username=\"user@domain.com\"");
    }

    [Fact]
    public void Given_VerySmallTimeout_When_Creating_Then_Succeeds()
    {
        var manager = new DigestAuthenticatorManager(
            new Uri("http://localhost"),
            "user",
            "pass",
            TimeSpan.FromMilliseconds(1),
            null,
            NullLogger.Instance);

        manager.ShouldNotBeNull();
    }

    [Fact]
    public void Given_WhitespacePassword_When_Creating_Then_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() =>
            new DigestAuthenticatorManager(
                new Uri("http://localhost"),
                "user",
                "   ",
                TimeSpan.FromSeconds(10),
                null,
                NullLogger.Instance));
    }

    [Fact]
    public void Given_WhitespaceUsername_When_Creating_Then_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() =>
            new DigestAuthenticatorManager(
                new Uri("http://localhost"),
                "   ",
                "pass",
                TimeSpan.FromSeconds(10),
                null,
                NullLogger.Instance));
    }

    private static DigestAuthenticatorManager CreateManager(
        string username = "user",
        string password = "pass",
        ILogger? logger = null)
    {
        return new DigestAuthenticatorManager(
            new Uri("http://localhost"),
            username,
            password,
            TimeSpan.FromSeconds(30),
            null,
            logger ?? NullLogger.Instance);
    }

    /// <summary>
    ///     Creates a manager with RFC 2617 digest fields set (qop, nonce, realm, cnonce)
    ///     to simulate a completed handshake with a server that provides qop.
    /// </summary>
    private static DigestAuthenticatorManager CreateManagerWithQop(
        string username = "user",
        string password = "pass")
    {
        var manager = CreateManager(username, password);
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        var type = typeof(DigestAuthenticatorManager);

        type.GetField("_qop", flags)!.SetValue(manager, "auth");
        type.GetField("_nonce", flags)!.SetValue(manager, "test-nonce");
        type.GetField("_realm", flags)!.SetValue(manager, "test-realm");
        type.GetField("_cnonce", flags)!.SetValue(manager, "12345");

        return manager;
    }
}
