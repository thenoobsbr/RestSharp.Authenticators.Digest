using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NSubstitute;

using RestSharp.Authenticators.Digest.IntegrationTests.Fixtures;

using Shouldly;

using Xunit;

namespace RestSharp.Authenticators.Digest.IntegrationTests;

/// <summary>
///     Integration tests for RFC 2069 digest authentication (no qop in server challenge).
/// </summary>
[Trait("Category", "IntegrationTests")]
[Trait("Class", nameof(DigestAuthenticator))]
public class DigestRfc2069IntegrationTest : IClassFixture<DigestRfc2069ServerStub>
{
    [Fact]
    public async Task Given_Rfc2069Server_When_Authenticating_Then_Succeeds()
    {
        var loggerMock = Substitute.For<ILogger>();
        var client = DigestRfc2069ServerStub.CreateClient(loggerMock);

        var request = new RestRequest("values");
        var response = await client.ExecuteAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.ShouldBe("Successful authentication!");
    }

    [Fact]
    public async Task Given_Rfc2069Server_When_PostRequest_Then_Succeeds()
    {
        var loggerMock = Substitute.For<ILogger>();
        var client = DigestRfc2069ServerStub.CreateClient(loggerMock);

        var request = new RestRequest("values", Method.Post);
        var response = await client.ExecuteAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Given_Rfc2069Server_When_MultipleRequests_Then_AllSucceed()
    {
        var loggerMock = Substitute.For<ILogger>();
        var client = DigestRfc2069ServerStub.CreateClient(loggerMock);

        for (var i = 0; i < 3; i++)
        {
            var request = new RestRequest("values");
            var response = await client.ExecuteAsync(request);
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task Given_Rfc2069Server_WithCancellationToken_When_Authenticating_Then_Succeeds()
    {
        var loggerMock = Substitute.For<ILogger>();

        var restOptions = new RestClientOptions(DigestRfc2069ServerStub.BaseUrl)
        {
            Authenticator = new DigestAuthenticator("test-user", "test-password", logger: loggerMock)
        };
        var client = new RestClient(restOptions);

        var request = new RestRequest("values");
        using var cts = new CancellationTokenSource();
        var response = await client.ExecuteAsync(request, cts.Token);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Given_Rfc2069Server_WithWrongCredentials_When_Authenticating_Then_ReturnsUnauthorized()
    {
        var loggerMock = Substitute.For<ILogger>();

        var restOptions = new RestClientOptions(DigestRfc2069ServerStub.BaseUrl)
        {
            Authenticator = new DigestAuthenticator("wrong-user", "wrong-pass", logger: loggerMock)
        };
        var client = new RestClient(restOptions);

        var request = new RestRequest("values");
        var response = await client.ExecuteAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
