using System;

using Microsoft.Extensions.Logging;

using NSubstitute;

using Shouldly;

using Xunit;

namespace RestSharp.Authenticators.Digest.UnitTests;

[Trait("Category", "UnitTest")]
[Trait("Class", nameof(DigestAuthenticator))]
public class DigestAuthenticatorConstructorExtendedTest
{
    [Fact]
    public void Given_DefaultTimeout_When_Creating_Then_Succeeds()
    {
        var authenticator = new DigestAuthenticator("user", "password");
        authenticator.ShouldNotBeNull();
    }

    [Fact]
    public void Given_MinTimeout_When_Creating_Then_Succeeds()
    {
        var authenticator = new DigestAuthenticator("user", "password", 1);
        authenticator.ShouldNotBeNull();
    }

    [Fact]
    public void Given_LargeTimeout_When_Creating_Then_Succeeds()
    {
        var authenticator = new DigestAuthenticator("user", "password", int.MaxValue);
        authenticator.ShouldNotBeNull();
    }

    [Fact]
    public void Given_SpecialCharsInUsername_When_Creating_Then_Succeeds()
    {
        var authenticator = new DigestAuthenticator("user@domain.com", "password");
        authenticator.ShouldNotBeNull();
    }

    [Fact]
    public void Given_SpecialCharsInPassword_When_Creating_Then_Succeeds()
    {
        var authenticator = new DigestAuthenticator("user", "p@$$w0rd!#%");
        authenticator.ShouldNotBeNull();
    }

    [Fact]
    public void Given_AllParametersExplicit_When_Creating_Then_IsIAuthenticator()
    {
        var logger = Substitute.For<ILogger>();
        var options = new RestClientOptions();
        var authenticator = new DigestAuthenticator("user", "password", 5000, logger, options);

        authenticator.ShouldBeAssignableTo<IAuthenticator>();
    }

    [Fact]
    public void Given_OnlyRestClientOptions_When_Creating_Then_Succeeds()
    {
        var options = new RestClientOptions();
        var authenticator = new DigestAuthenticator("user", "password", restClientOptions: options);
        authenticator.ShouldNotBeNull();
    }

    [Fact]
    public void Given_NullRestClientOptions_When_Creating_Then_Succeeds()
    {
        var authenticator = new DigestAuthenticator("user", "password", restClientOptions: null);
        authenticator.ShouldNotBeNull();
    }

    [Theory]
    [InlineData(-100)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(int.MinValue)]
    public void Given_InvalidTimeoutValues_When_Creating_Then_ThrowsArgumentException(int timeout)
    {
        var ex = Should.Throw<ArgumentException>(() =>
            new DigestAuthenticator("user", "password", timeout));

        ex.ParamName.ShouldBe("timeout");
    }

    [Fact]
    public void Given_TabInUsername_When_Creating_Then_Succeeds()
    {
        // Tab is not whitespace-only, it has content
        var authenticator = new DigestAuthenticator("user\tname", "password");
        authenticator.ShouldNotBeNull();
    }
}
