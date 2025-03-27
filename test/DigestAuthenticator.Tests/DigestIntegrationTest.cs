using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RestSharp.Authenticators.Digest.Tests.Fixtures;
using Xunit;

namespace RestSharp.Authenticators.Digest.Tests;

/// <summary>
///     The integration tests for <see cref="DigestAuthenticator" />.
/// </summary>
[Trait("Category", "IntegrationTests")]
[Trait("Class", nameof(DigestAuthenticator))]
public class DigestIntegrationTest : IClassFixture<DigestServerStub>
{
    private readonly DigestServerStub _fixture;

    public DigestIntegrationTest(DigestServerStub fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Given_ADigestAuthEndpoint_When_ITryToGetInfo_Then_TheAuthMustBeResolved()
    {
        var loggerMock = Substitute.For<ILogger>();
        loggerMock.BeginScope("DigestServerStub");

        var request = new RestRequest("values");
        request.AddHeader("Content-Type", "application/json");

        var client = _fixture.CreateClient(loggerMock);
        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        loggerMock.ReceivedWithAnyArgs().LogDebug("NONONO");
    }


    [Fact]
    public async Task Given_ADigestAuthEndpoint_When_ITryToInjectOwnClient_Then_TheAuthMustBeResolved()
    {
        var loggerMock = Substitute.For<ILogger>();
        loggerMock.BeginScope("DigestServerStub");

        var request = new RestRequest("values");
        request.AddHeader("Content-Type", "application/json");

        RestClientOptions options = new RestClientOptions()
        {
            MaxRedirects = 2
        };

        var client = _fixture.CreateInjectedOptionClient(loggerMock, options);
        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        loggerMock.ReceivedWithAnyArgs().LogDebug("NONONO");
    }
}
