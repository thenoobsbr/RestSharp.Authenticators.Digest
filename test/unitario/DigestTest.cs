using System.Diagnostics;
using System.Net;
using FluentAssertions;
using Xunit;

namespace RestSharp.Authenticators.Digest.Tests;

/// <summary>
///     The integration tests for <see cref="DigestAuthenticator" />.
/// </summary>
[Trait("Category", "IntegrationTests")]
[Trait("Class", nameof(DigestAuthenticator))]
public class DigestTest
{
    [SkippableFact]
    public void Given_ADigestAuthEndpoint_When_ITryToGetInfo_Then_TheAuthMustBeResolved()
    {
        Skip.IfNot(Debugger.IsAttached);

        var client = new RestClient("http://localhost:49896/api")
        {
            Authenticator = new DigestAuthenticator("eddie", "starwars123")
        };

        var request = new RestRequest("values", Method.GET);
        request.AddHeader("Content-Type", "application/json");

        var response = client.Execute(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData(
        "Digest realm=\"test - realm\", nonce=\"2021 - 12 - 21 13:40:54.513311Z f152e55bf3d14e28b90f47db5dbd8afb\", qop=\"auth\", algorithm=MD5")]
    [InlineData(
        "realm=\"test - realm\", nonce=\"2021 - 12 - 21 13:40:54.513311Z f152e55bf3d14e28b90f47db5dbd8afb\", qop=\"auth\", algorithm=MD5")]
    public void Given_ADigestAuthenticateHeader_When_ITryCreateObject_Then_AllPropsMustBeFilled(string header)
    {
        var digestHeader = new DigestHeader(header);
        digestHeader.Nonce.Should().Be("2021 - 12 - 21 13:40:54.513311Z f152e55bf3d14e28b90f47db5dbd8afb");
        digestHeader.Qop.Should().Be("auth");
        digestHeader.Realm.Should().Be("test - realm");
    }
}
