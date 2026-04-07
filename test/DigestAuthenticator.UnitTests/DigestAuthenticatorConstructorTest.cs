using System;

using Microsoft.Extensions.Logging;

using NSubstitute;

using Shouldly;

using Xunit;

namespace RestSharp.Authenticators.Digest.UnitTests;

[Trait("Category", "UnitTest")]
[Trait("Class", nameof(DigestAuthenticator))]
public class DigestAuthenticatorConstructorTest
{
    [Fact]
    public void Given_AllParameters_When_Creating_Then_Succeeds()
    {
        var logger = Substitute.For<ILogger>();
        var options = new RestClientOptions();
        var authenticator = new DigestAuthenticator("user", "password", 30000, logger, options);
        authenticator.ShouldNotBeNull();
    }

    [Fact]
    public void Given_EmptyPassword_When_Creating_Then_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => new DigestAuthenticator("user", ""))
            .ParamName.ShouldBe("password");
    }

    [Fact]
    public void Given_EmptyUsername_When_Creating_Then_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => new DigestAuthenticator("", "password"))
            .ParamName.ShouldBe("username");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Given_InvalidTimeout_When_Creating_Then_ThrowsArgumentException(int timeout)
    {
        Should.Throw<ArgumentException>(() => new DigestAuthenticator("user", "password", timeout));
    }

    [Fact]
    public void Given_NullLogger_When_Creating_Then_FallsBackToNullLogger()
    {
        var authenticator = new DigestAuthenticator("user", "password", logger: null);
        authenticator.ShouldNotBeNull();
    }

    [Fact]
    public void Given_NullPassword_When_Creating_Then_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => new DigestAuthenticator("user", null!))
            .ParamName.ShouldBe("password");
    }

    [Fact]
    public void Given_NullUsername_When_Creating_Then_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => new DigestAuthenticator(null!, "password"))
            .ParamName.ShouldBe("username");
    }

    [Fact]
    public void Given_ValidParameters_When_Creating_Then_Succeeds()
    {
        var authenticator = new DigestAuthenticator("user", "password");
        authenticator.ShouldNotBeNull();
        authenticator.ShouldBeAssignableTo<IAuthenticator>();
    }

    [Fact]
    public void Given_ValidParametersWithLogger_When_Creating_Then_Succeeds()
    {
        var logger = Substitute.For<ILogger>();
        var authenticator = new DigestAuthenticator("user", "password", logger: logger);
        authenticator.ShouldNotBeNull();
    }

    [Fact]
    public void Given_ValidParametersWithOptions_When_Creating_Then_Succeeds()
    {
        var options = new RestClientOptions { Proxy = null };
        var authenticator = new DigestAuthenticator("user", "password", restClientOptions: options);
        authenticator.ShouldNotBeNull();
    }

    [Fact]
    public void Given_ValidParametersWithTimeout_When_Creating_Then_Succeeds()
    {
        var authenticator = new DigestAuthenticator("user", "password", 5000);
        authenticator.ShouldNotBeNull();
    }

    [Fact]
    public void Given_WhitespacePassword_When_Creating_Then_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => new DigestAuthenticator("user", "   "))
            .ParamName.ShouldBe("password");
    }

    [Fact]
    public void Given_WhitespaceUsername_When_Creating_Then_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => new DigestAuthenticator("   ", "password"))
            .ParamName.ShouldBe("username");
    }
}
