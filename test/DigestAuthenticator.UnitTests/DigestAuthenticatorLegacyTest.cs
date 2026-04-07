using System;

using Microsoft.Extensions.Logging;

using NSubstitute;

using Shouldly;

using Xunit;

namespace RestSharp.Authenticators.Digest.UnitTests;

[Trait("Category", "UnitTest")]
[Trait("Class", nameof(DigestAuthenticatorLegacy))]
public class DigestAuthenticatorLegacyTest
{
    [Fact]
    public void Given_LegacyCreateSimple_When_Called_Then_ReturnsAuthenticator()
    {
#pragma warning disable CS0618
        var authenticator = DigestAuthenticatorLegacy.Create("user", "pass");
#pragma warning restore CS0618

        authenticator.ShouldNotBeNull();
        authenticator.ShouldBeAssignableTo<IAuthenticator>();
    }

    [Fact]
    public void Given_LegacyCreateWithLogger_When_Called_Then_ReturnsAuthenticator()
    {
#pragma warning disable CS0618
        var logger = Substitute.For<ILogger>();
        var authenticator = DigestAuthenticatorLegacy.Create("user", "pass", logger);
#pragma warning restore CS0618

        authenticator.ShouldNotBeNull();
        authenticator.ShouldBeAssignableTo<IAuthenticator>();
    }

    [Fact]
    public void Given_LegacyCreateWithLoggerAndTimeout_When_Called_Then_ReturnsAuthenticator()
    {
#pragma warning disable CS0618
        var logger = Substitute.For<ILogger>();
        var authenticator = DigestAuthenticatorLegacy.Create("user", "pass", logger, 5000);
#pragma warning restore CS0618

        authenticator.ShouldNotBeNull();
    }

    [Fact]
    public void Given_LegacyCreateWithNullLogger_When_Called_Then_ReturnsAuthenticator()
    {
#pragma warning disable CS0618
        var authenticator = DigestAuthenticatorLegacy.Create("user", "pass", null);
#pragma warning restore CS0618

        authenticator.ShouldNotBeNull();
    }

    [Fact]
    public void Given_LegacyCreateWithNullPassword_When_Called_Then_Throws()
    {
#pragma warning disable CS0618
        Should.Throw<ArgumentException>(() => DigestAuthenticatorLegacy.Create("user", null!));
#pragma warning restore CS0618
    }

    [Fact]
    public void Given_LegacyCreateWithNullUsername_When_Called_Then_Throws()
    {
#pragma warning disable CS0618
        Should.Throw<ArgumentException>(() => DigestAuthenticatorLegacy.Create(null!, "pass"));
#pragma warning restore CS0618
    }
}
