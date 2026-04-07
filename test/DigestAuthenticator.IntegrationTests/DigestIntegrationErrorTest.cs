using System.Net;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NSubstitute;

using RestSharp.Authenticators.Digest.Exceptions;
using RestSharp.Authenticators.Digest.IntegrationTests.Fixtures;

using Shouldly;

using Xunit;

namespace RestSharp.Authenticators.Digest.IntegrationTests;

[Trait("Category", "IntegrationTests")]
[Trait("Class", nameof(DigestAuthenticator))]
public class DigestIntegrationErrorTest(DigestServerStub digestServerStub) : IClassFixture<DigestServerStub>
{
    private readonly DigestServerStub _digestServerStub = digestServerStub;

    [Fact]
    public async Task Given_CustomTimeout_When_Authenticating_Then_Succeeds()
    {
        var loggerMock = Substitute.For<ILogger>();

        var restOptions = new RestClientOptions(DigestServerStub.BaseUrl)
        {
            Authenticator = new DigestAuthenticator("test-user", "test-password", 30000, loggerMock)
        };
        var client = new RestClient(restOptions);

        var request = new RestRequest("values");
        var response = await client.ExecuteAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Given_DeleteMethod_When_Authenticating_Then_Succeeds()
    {
        var loggerMock = Substitute.For<ILogger>();
        var client = DigestServerStub.CreateClient(loggerMock);

        var request = new RestRequest("values", Method.Delete);
        var response = await client.ExecuteAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Given_NonExistentHost_When_Authenticating_Then_ThrowsUnexpectedStatusCodeException()
    {
        var loggerMock = Substitute.For<ILogger>();

        var restOptions = new RestClientOptions("http://localhost:19999")
        {
            Authenticator = new DigestAuthenticator("user", "pass", 3000, loggerMock)
        };
        var client = new RestClient(restOptions);

        var request = new RestRequest("values");

        await Should.ThrowAsync<UnexpectedStatusCodeException>(async () =>
            await client.ExecuteAsync(request));
    }

    [Fact]
    public async Task Given_PostMethod_When_Authenticating_Then_Succeeds()
    {
        var loggerMock = Substitute.For<ILogger>();
        var client = DigestServerStub.CreateClient(loggerMock);

        var request = new RestRequest("values", Method.Post);
        var response = await client.ExecuteAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Given_PutMethod_When_Authenticating_Then_Succeeds()
    {
        var loggerMock = Substitute.For<ILogger>();
        var client = DigestServerStub.CreateClient(loggerMock);

        var request = new RestRequest("values", Method.Put);
        var response = await client.ExecuteAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Given_ValidAuth_When_DifferentPaths_Then_AllSucceed()
    {
        var loggerMock = Substitute.For<ILogger>();
        var client = DigestServerStub.CreateClient(loggerMock);

        var paths = new[] { "values", "api/test", "data" };
        foreach (var path in paths)
        {
            var request = new RestRequest(path);
            var response = await client.ExecuteAsync(request);
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task Given_ValidAuth_When_GetRequest_Then_ResponseContentIsCorrect()
    {
        var loggerMock = Substitute.For<ILogger>();
        var client = DigestServerStub.CreateClient(loggerMock);

        var request = new RestRequest("values");
        var response = await client.ExecuteAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.ShouldBe("Successful authentication!");
    }

    [Fact]
    public async Task Given_ValidAuth_When_MultipleRequests_Then_AllSucceed()
    {
        var loggerMock = Substitute.For<ILogger>();
        var client = DigestServerStub.CreateClient(loggerMock);

        for (var i = 0; i < 3; i++)
        {
            var request = new RestRequest("values");
            var response = await client.ExecuteAsync(request);
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task Given_WrongCredentials_When_Authenticating_Then_ReturnsUnauthorized()
    {
        var loggerMock = Substitute.For<ILogger>();

        var restOptions = new RestClientOptions(DigestServerStub.BaseUrl)
        {
            Authenticator = new DigestAuthenticator("wrong-user", "wrong-pass", logger: loggerMock)
        };
        var client = new RestClient(restOptions);

        var request = new RestRequest("values");
        var response = await client.ExecuteAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Given_WrongPassword_When_Authenticating_Then_ReturnsUnauthorized()
    {
        var loggerMock = Substitute.For<ILogger>();

        var restOptions = new RestClientOptions(DigestServerStub.BaseUrl)
        {
            Authenticator = new DigestAuthenticator("test-user", "wrong-pass", logger: loggerMock)
        };
        var client = new RestClient(restOptions);

        var request = new RestRequest("values");
        var response = await client.ExecuteAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
