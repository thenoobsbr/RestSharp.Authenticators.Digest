using System;

using Microsoft.Extensions.Logging;

using NSubstitute;

using Shouldly;

using Xunit;

namespace RestSharp.Authenticators.Digest.UnitTests;

[Trait("Category", "UnitTest")]
[Trait("Class", nameof(DigestAuthenticatorLegacy))]
public class DigestAuthenticatorLegacyExtendedTest
{
    [Fact]
    public void Given_LegacyCreateWithLoggerAndDefaultTimeout_When_Called_Then_ReturnsAuthenticator()
    {
#pragma warning disable CS0618
        var logger = Substitute.For<ILogger>();
        var authenticator = DigestAuthenticatorLegacy.Create("user", "pass", logger);
#pragma warning restore CS0618

        authenticator.ShouldNotBeNull();
    }

    [Fact]
    public void Given_LegacyCreateWithInvalidTimeout_When_Called_Then_Throws()
    {
#pragma warning disable CS0618
        Should.Throw<ArgumentException>(() =>
            DigestAuthenticatorLegacy.Create("user", "pass", null, 0));
#pragma warning restore CS0618
    }

    [Fact]
    public void Given_LegacyCreateWithNegativeTimeout_When_Called_Then_Throws()
    {
#pragma warning disable CS0618
        Should.Throw<ArgumentException>(() =>
            DigestAuthenticatorLegacy.Create("user", "pass", null, -1));
#pragma warning restore CS0618
    }

    [Fact]
    public void Given_LegacyCreateWithEmptyUsername_When_Called_Then_Throws()
    {
#pragma warning disable CS0618
        Should.Throw<ArgumentException>(() =>
            DigestAuthenticatorLegacy.Create("", "pass"));
#pragma warning restore CS0618
    }

    [Fact]
    public void Given_LegacyCreateWithEmptyPassword_When_Called_Then_Throws()
    {
#pragma warning disable CS0618
        Should.Throw<ArgumentException>(() =>
            DigestAuthenticatorLegacy.Create("user", ""));
#pragma warning restore CS0618
    }

    [Fact]
    public void Given_LegacyCreateWithWhitespaceUsername_When_Called_Then_Throws()
    {
#pragma warning disable CS0618
        Should.Throw<ArgumentException>(() =>
            DigestAuthenticatorLegacy.Create("   ", "pass"));
#pragma warning restore CS0618
    }

    [Fact]
    public void Given_LegacyCreateWithWhitespacePassword_When_Called_Then_Throws()
    {
#pragma warning disable CS0618
        Should.Throw<ArgumentException>(() =>
            DigestAuthenticatorLegacy.Create("user", "   "));
#pragma warning restore CS0618
    }

    [Fact]
    public void Given_LegacyCreateSimple_When_Called_Then_ReturnsIAuthenticator()
    {
#pragma warning disable CS0618
        var authenticator = DigestAuthenticatorLegacy.Create("user", "pass");
#pragma warning restore CS0618

        authenticator.ShouldBeAssignableTo<IAuthenticator>();
    }

    [Fact]
    public void Given_LegacyCreateWithCustomTimeout_When_Called_Then_Succeeds()
    {
#pragma warning disable CS0618
        var authenticator = DigestAuthenticatorLegacy.Create("user", "pass", null, 5000);
#pragma warning restore CS0618

        authenticator.ShouldNotBeNull();
    }
}
