using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace RestSharp.Authenticators.Digest.Tests;

/// <summary>
///     The integration tests for <see cref="DigestAuthenticator" />.
/// </summary>
[Trait("Category", "IntegrationTests")]
[Trait("Class", nameof(DigestAuthenticator))]
public class DigestIntegrationTest
{
    [SkippableFact]
    public async Task Given_ADigestAuthEndpoint_When_ITryToGetInfo_Then_TheAuthMustBeResolved()
    {
        Skip.IfNot(Debugger.IsAttached);

        var options = new RestClientOptions("http://localhost:46551/api")
        {
            Authenticator = new DigestAuthenticator("eddie", "starwars123")
        };

        var client = new RestClient(options);

        var request = new RestRequest("values");
        request.AddHeader("Content-Type", "application/json");

        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
