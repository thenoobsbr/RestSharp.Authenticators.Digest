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
///     The integration tests for <see cref="DigestAuthenticator" />.
/// </summary>
[Trait("Category", "IntegrationTests")]
[Trait("Class", nameof(DigestAuthenticator))]
public class DigestIntegrationTest : IClassFixture<DigestServerStub>
{
    [Fact]
    public async Task Given_ADigestAuthEndpoint_When_ITryToGetInfo_Then_TheAuthMustBeResolved()
    {
        var loggerMock = Substitute.For<ILogger>();
        loggerMock.BeginScope("DigestServerStub");

        var request = new RestRequest("values");
        request.AddHeader("Content-Type", "application/json");

        var client = DigestServerStub.CreateClient(loggerMock);
        var response = await client.ExecuteAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        loggerMock.ReceivedWithAnyArgs().LogDebug("NONONO");
    }

    [Fact]
    public async Task Given_ADigestAuthEndpoint_When_ITryToInjectOwnClient_Then_TheAuthMustBeResolved()
    {
        var proxyCalled = false;

        var loggerMock = Substitute.For<ILogger>();
        loggerMock.BeginScope("DigestServerStub");

        var request = new RestRequest("values");
        request.AddHeader("Content-Type", "application/json");

        var options = new RestClientOptions
        {
            Proxy = new TestProxy(() => proxyCalled = true)
        };

        var client = DigestServerStub.CreateInjectedOptionClient(loggerMock, options);
        var response = await client.ExecuteAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        loggerMock.ReceivedWithAnyArgs().LogDebug("NONONO");

        Assert.True(proxyCalled, "Injected RestClientOptions.Proxy should be used in digest handshake.");
    }

    [Fact]
    public async Task Given_CancellationToken_When_Authenticating_Then_Succeeds()
    {
        var loggerMock = Substitute.For<ILogger>();

        var restOptions = new RestClientOptions(DigestServerStub.BaseUrl)
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
    public async Task Given_LegacyAuthenticateAsync_When_Called_Then_AuthenticationSucceeds()
    {
        var loggerMock = Substitute.For<ILogger>();

        var restOptions = new RestClientOptions(DigestServerStub.BaseUrl)
        {
            Authenticator = new DigestAuthenticator("test-user", "test-password", logger: loggerMock)
        };
        var client = new RestClient(restOptions);

        var request = new RestRequest("values");

#pragma warning disable CS0618
        await restOptions.Authenticator.ShouldBeOfType<DigestAuthenticator>()
            .AuthenticateAsync(client, request);
#pragma warning restore CS0618

        var response = await client.ExecuteAsync(request);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
