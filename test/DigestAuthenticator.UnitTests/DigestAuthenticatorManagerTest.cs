using System;
using System.Reflection;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using NSubstitute;

using Shouldly;

using Xunit;

namespace RestSharp.Authenticators.Digest.UnitTests;

[Trait("Category", "UnitTest")]
[Trait("Class", nameof(DigestAuthenticatorManager))]
public class DigestAuthenticatorManagerTest
{
    [Theory]
    [InlineData(Method.Get)]
    [InlineData(Method.Post)]
    [InlineData(Method.Put)]
    [InlineData(Method.Delete)]
    public void Given_DifferentMethods_When_GetDigestHeader_Then_ContainsUsername(Method method)
    {
        var manager = new DigestAuthenticatorManager(
            new Uri("http://localhost"),
            "user",
            "pass",
            TimeSpan.FromSeconds(30),
            null,
            NullLogger.Instance);

        var header = manager.GetDigestHeader("/test", method);

        header.ShouldContain("username=\"user\"");
    }

    [Fact]
    public void Given_DigestData_When_GetDigestHeader_Then_ReturnsValidFormat()
    {
        var manager = new DigestAuthenticatorManager(
            new Uri("http://localhost"),
            "testuser",
            "testpass",
            TimeSpan.FromSeconds(30),
            null,
            NullLogger.Instance);

        var header = manager.GetDigestHeader("/api/values", Method.Get);

        header.ShouldStartWith("Digest ");
        header.ShouldContain("username=\"testuser\"");
        header.ShouldContain("algorithm=MD5");
        header.ShouldContain("uri=\"/api/values\"");
    }

    [Fact]
    public void Given_Rfc2617DigestData_When_GetDigestHeader_Then_ReturnsValidFormatWithQop()
    {
        var manager = new DigestAuthenticatorManager(
            new Uri("http://localhost"),
            "testuser",
            "testpass",
            TimeSpan.FromSeconds(30),
            null,
            NullLogger.Instance);

        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        var type = typeof(DigestAuthenticatorManager);
        type.GetField("_qop", flags)!.SetValue(manager, "auth");
        type.GetField("_nonce", flags)!.SetValue(manager, "test-nonce");
        type.GetField("_realm", flags)!.SetValue(manager, "test-realm");
        type.GetField("_cnonce", flags)!.SetValue(manager, "12345");

        var header = manager.GetDigestHeader("/api/values", Method.Get);

        header.ShouldStartWith("Digest ");
        header.ShouldContain("username=\"testuser\"");
        header.ShouldContain("algorithm=MD5");
        header.ShouldContain("uri=\"/api/values\"");
        header.ShouldContain("nc=00000001");
        header.ShouldContain("qop=auth");
        header.ShouldContain("cnonce=\"12345\"");
    }

    [Fact]
    public void Given_EmptyPassword_When_Creating_Then_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() =>
            new DigestAuthenticatorManager(new Uri("http://localhost"), "user", "", TimeSpan.FromSeconds(10),
                null, NullLogger.Instance));
    }

    [Fact]
    public void Given_EmptyUsername_When_Creating_Then_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() =>
            new DigestAuthenticatorManager(new Uri("http://localhost"), "", "pass", TimeSpan.FromSeconds(10),
                null, NullLogger.Instance));
    }

    [Fact]
    public void Given_LoggerProvided_When_Creating_Then_LoggerIsUsed()
    {
        var logger = Substitute.For<ILogger>();
        var manager = new DigestAuthenticatorManager(
            new Uri("http://localhost"),
            "user",
            "pass",
            TimeSpan.FromSeconds(30),
            null,
            logger);

        manager.ShouldNotBeNull();
    }

    [Fact]
    public void Given_NegativeTimeout_When_Creating_Then_ThrowsArgumentOutOfRangeException()
    {
        Should.Throw<ArgumentOutOfRangeException>(() =>
            new DigestAuthenticatorManager(new Uri("http://localhost"), "user", "pass",
                TimeSpan.FromSeconds(-1), null, NullLogger.Instance));
    }

    [Fact]
    public void Given_NullHost_When_Creating_Then_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() =>
            new DigestAuthenticatorManager(null!, "user", "pass", TimeSpan.FromSeconds(10), null,
                NullLogger.Instance));
    }

    [Fact]
    public void Given_NullPassword_When_Creating_Then_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() =>
            new DigestAuthenticatorManager(new Uri("http://localhost"), "user", null!, TimeSpan.FromSeconds(10),
                null, NullLogger.Instance));
    }

    [Fact]
    public void Given_NullUsername_When_Creating_Then_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() =>
            new DigestAuthenticatorManager(new Uri("http://localhost"), null!, "pass", TimeSpan.FromSeconds(10),
                null, NullLogger.Instance));
    }

    [Fact]
    public void Given_ValidParameters_When_Creating_Then_Succeeds()
    {
        var manager = new DigestAuthenticatorManager(
            new Uri("http://localhost"),
            "user",
            "pass",
            TimeSpan.FromSeconds(30),
            null,
            NullLogger.Instance);

        manager.ShouldNotBeNull();
    }

    [Fact]
    public void Given_WithRestClientOptions_When_Creating_Then_Succeeds()
    {
        var options = new RestClientOptions();
        var manager = new DigestAuthenticatorManager(
            new Uri("http://localhost"),
            "user",
            "pass",
            TimeSpan.FromSeconds(30),
            options,
            NullLogger.Instance);

        manager.ShouldNotBeNull();
    }

    [Fact]
    public void Given_ZeroTimeout_When_Creating_Then_ThrowsArgumentOutOfRangeException()
    {
        Should.Throw<ArgumentOutOfRangeException>(() =>
            new DigestAuthenticatorManager(new Uri("http://localhost"), "user", "pass", TimeSpan.Zero, null,
                NullLogger.Instance));
    }
}
