namespace RestSharp.Authenticators.Digest
{
    using System.Net;

    using RestSharp.Authenticators;
    using RestSharp;

    /// <summary>
    /// Digest midleware for <see cref="IRestClient"/>.
    /// </summary>
    public class DigestAuthenticator : IAuthenticator
    {
        /// <summary>
        /// The username.
        /// </summary>
        private readonly string username;

        /// <summary>
        /// The password.
        /// </summary>
        private readonly string password; 

        /// <summary>
        /// Creates a new instance of <see cref="DigestAuthenticator"/> class.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        public DigestAuthenticator(string username, string password)
        {
            this.username = username;
            this.password = password;
        } 

        /// <inheritdoc cref="IAuthenticator" />.
        public void Authenticate(IRestClient client, IRestRequest request)
        {
            request.Credentials = new NetworkCredential(username, this.password); 

            var uri = client.BuildUri(request);
            var manager = new DigestAuthenticatorManager(client.BaseUrl, this.username, this.password);
            manager.GetDigestAuthHeader(uri.PathAndQuery, request.Method);
            var digestHeader = manager.GetDigestHeader(uri.PathAndQuery, request.Method);
            request.AddParameter("Authorization", digestHeader, ParameterType.HttpHeader);
        }
    } 
}