using System.Net;

namespace RestSharp.Authenticators.Digest
{
    /// <summary>
    ///     Digest middleware for <see cref="IRestClient" />.
    /// </summary>
    public class DigestAuthenticator : IAuthenticator
    {
        private const int DEFAULT_TIMEOUT = 100000;
        private readonly string _password;

        private readonly string _username;

        /// <summary>
        ///     Creates a new instance of <see cref="DigestAuthenticator" /> class.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        public DigestAuthenticator(string username, string password)
        {
            _username = username;
            _password = password;
            Timeout = DEFAULT_TIMEOUT;
        }

        /// <summary>
        ///     The web request timeout (default 100000).
        /// </summary>
        public int Timeout { get; set; }

        /// <inheritdoc cref="IAuthenticator" />
        public void Authenticate(IRestClient client, IRestRequest request)
        {
            // TODO: We must check this to use the new way.
#pragma warning disable CS0618
            request.Credentials = new NetworkCredential(_username, _password);
#pragma warning restore CS0618

            var uri = client.BuildUri(request);
            var manager = new DigestAuthenticatorManager(client.BaseUrl, _username, _password, Timeout);
            manager.GetDigestAuthHeader(uri.PathAndQuery, request.Method);
            var digestHeader = manager.GetDigestHeader(uri.PathAndQuery, request.Method);
            request.AddParameter("Authorization", digestHeader, ParameterType.HttpHeader);
        }
    }
}
