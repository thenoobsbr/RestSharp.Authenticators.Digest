using Microsoft.Extensions.Logging.Abstractions;

using Shouldly;

using Xunit;

namespace RestSharp.Authenticators.Digest.Tests;

/// <summary>
///     The integration tests for <see cref="DigestAuthenticator" />.
/// </summary>
[Trait("Category", "UnitTest")]
[Trait("Class", nameof(DigestAuthenticator))]
public class DigestUnitTest
{
    [Theory]
    [InlineData(
        "Digest realm=\"test - realm\", nonce=\"2021 - 12 - 21 13:40:54.513311Z f152e55bf3d14e28b90f47db5dbd8afb\", qop=\"auth\", algorithm=MD5")]
    [InlineData(
        "realm=\"test - realm\", nonce=\"2021 - 12 - 21 13:40:54.513311Z f152e55bf3d14e28b90f47db5dbd8afb\", qop=\"auth\", algorithm=MD5")]
    [InlineData(
        "realm=\"test - realm\", nonce=\"2021 - 12 - 21 13:40:54.513311Z f152e55bf3d14e28b90f47db5dbd8afb\", qop=auth, algorithm=MD5")]
    public void Given_ADigestAuthenticateHeader_When_ITryCreateObject_Then_AllPropsMustBeFilled(string header)
    {
        var digestHeader = new DigestHeader(header, NullLogger.Instance);
        digestHeader.Nonce.ShouldBe("2021 - 12 - 21 13:40:54.513311Z f152e55bf3d14e28b90f47db5dbd8afb");
        digestHeader.Qop.ShouldBe("auth");
        digestHeader.Realm.ShouldBe("test - realm");
    }
}
