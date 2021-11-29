using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace RestSharp.Authenticators.Digest.Tests
{
    /// <summary>
    /// The integration tests for <see cref="DigestAuthenticator"/>.
    /// </summary>
    [Trait("Category", "IntegrationTests")]
    [Trait("Class", nameof(DigestAuthenticator))]
    public class DigestTest
    {
        /// <summary>
        /// Given an digest auth endpoint, When I try to get info, Then the auth must be resolved.
        /// </summary>
        [Fact]
        public void Given_AnDigestAuthEndpoint_When_ITryToGetInfo_Then_TheAuthMustBeResolved()
        {
            var client = new RestClient("http://localhost:49896/api")
            {
                Authenticator = new DigestAuthenticator("eddie", "starwars123")
            };

            var request = new RestRequest("values", Method.GET);
            request.AddHeader("Content-Type", "application/json");

            var response = client.Execute(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
