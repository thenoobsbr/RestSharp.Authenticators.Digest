using System.Threading.Tasks;

namespace RestSharp.Authenticators.Digest;

/// <summary>
///     Digest middleware for <see cref="RestClient" />.
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
    public async ValueTask Authenticate(RestClient client, RestRequest request)
    {
        var uri = client.BuildUri(request);
        var manager = new DigestAuthenticatorManager(client.BuildUri(new RestRequest()), _username, _password, Timeout);
        await manager.GetDigestAuthHeader(uri.PathAndQuery, request.Method);
        var digestHeader = manager.GetDigestHeader(uri.PathAndQuery, request.Method);
        request.AddOrUpdateHeader("Connection", "Keep-Alive");
        request.AddOrUpdateHeader(KnownHeaders.Authorization, digestHeader);
    }
}
