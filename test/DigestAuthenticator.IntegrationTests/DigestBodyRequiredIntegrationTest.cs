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
///     Integration tests for devices that return 400 Bad Request when the handshake
///     request has no body (e.g. ONVIF cameras). Verifies that the original request's
///     parameters are forwarded to the digest challenge request.
/// </summary>
[Trait("Category", "IntegrationTests")]
[Trait("Class", nameof(DigestAuthenticator))]
public class DigestBodyRequiredIntegrationTest : IClassFixture<DigestBodyRequiredServerStub>
{
    [Fact]
    public async Task Given_BodyRequiredServer_When_PostWithBody_Then_AuthenticationSucceeds()
    {
        var loggerMock = Substitute.For<ILogger>();
        var client = DigestBodyRequiredServerStub.CreateClient(loggerMock);

        var request = new RestRequest("values", Method.Post);
        request.AddStringBody("<xml>payload</xml>", ContentType.Xml);

        var response = await client.ExecuteAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.ShouldBe("Successful authentication!");
    }

    [Fact]
    public async Task Given_BodyRequiredServer_When_PutWithBody_Then_AuthenticationSucceeds()
    {
        var loggerMock = Substitute.For<ILogger>();
        var client = DigestBodyRequiredServerStub.CreateClient(loggerMock);

        var request = new RestRequest("values", Method.Put);
        request.AddJsonBody(new { Name = "test", Value = 42 });

        var response = await client.ExecuteAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.ShouldBe("Successful authentication!");
    }

    [Fact]
    public async Task Given_BodyRequiredServer_When_PostWithStringBody_Then_AuthenticationSucceeds()
    {
        var loggerMock = Substitute.For<ILogger>();
        var client = DigestBodyRequiredServerStub.CreateClient(loggerMock);

        var request = new RestRequest("values", Method.Post);
        request.AddStringBody("{\"key\":\"value\"}", ContentType.Json);

        var response = await client.ExecuteAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Given_BodyRequiredServer_When_GetRequest_Then_AuthenticationSucceeds()
    {
        // GET requests don't require a body, so they should work normally
        var loggerMock = Substitute.For<ILogger>();
        var client = DigestBodyRequiredServerStub.CreateClient(loggerMock);

        var request = new RestRequest("values");

        var response = await client.ExecuteAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Given_BodyRequiredServer_When_PostWithBodyAndCancellationToken_Then_Succeeds()
    {
        var loggerMock = Substitute.For<ILogger>();

        var restOptions = new RestClientOptions(DigestBodyRequiredServerStub.BaseUrl)
        {
            Authenticator = new DigestAuthenticator("test-user", "test-password", logger: loggerMock)
        };
        var client = new RestClient(restOptions);

        var request = new RestRequest("values", Method.Post);
        request.AddStringBody("<soap>content</soap>", ContentType.Xml);

        using var cts = new CancellationTokenSource();
        var response = await client.ExecuteAsync(request, cts.Token);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Given_BodyRequiredServer_When_WrongCredentials_Then_ReturnsUnauthorized()
    {
        var loggerMock = Substitute.For<ILogger>();

        var restOptions = new RestClientOptions(DigestBodyRequiredServerStub.BaseUrl)
        {
            Authenticator = new DigestAuthenticator("wrong-user", "wrong-pass", logger: loggerMock)
        };
        var client = new RestClient(restOptions);

        var request = new RestRequest("values", Method.Post);
        request.AddStringBody("<xml>payload</xml>", ContentType.Xml);

        var response = await client.ExecuteAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
