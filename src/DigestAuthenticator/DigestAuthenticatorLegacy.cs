using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace RestSharp.Authenticators.Digest;

/// <summary>
///     Backward-compatible extensions and factory methods for <see cref="DigestAuthenticator" />.
///     These members preserve the v2.x API surface so that existing consumers can upgrade
///     to v3.0 (RestSharp 114+) without immediate code changes.
/// </summary>
public static class DigestAuthenticatorLegacy
{
    /// <summary>
    ///     Authenticates the request without a <see cref="CancellationToken" />,
    ///     preserving the v2.x call-site signature.
    /// </summary>
    /// <param name="authenticator">The authenticator instance.</param>
    /// <param name="client">The REST client.</param>
    /// <param name="request">The REST request.</param>
    /// <returns>A <see cref="ValueTask" /> representing the asynchronous operation.</returns>
    [Obsolete(
        "Use Authenticate(client, request, cancellationToken) instead. This overload exists for backward compatibility with RestSharp < 114.")]
    public static ValueTask AuthenticateAsync(
        this DigestAuthenticator authenticator,
        IRestClient client,
        RestRequest request)
    {
        return authenticator.Authenticate(client, request, CancellationToken.None);
    }

    /// <summary>
    ///     Creates a new <see cref="DigestAuthenticator" /> using the v1.5/v1.6 parameter order
    ///     where <paramref name="logger" /> preceded <paramref name="timeout" />.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="password">The password.</param>
    /// <param name="logger">The optional logger.</param>
    /// <param name="timeout">The request timeout in milliseconds (default 100 000).</param>
    /// <returns>A configured <see cref="DigestAuthenticator" /> instance.</returns>
    [Obsolete(
        "Use the DigestAuthenticator constructor directly. This method preserves the v1.x parameter order (logger before timeout) for backward compatibility.")]
    public static DigestAuthenticator Create(
        string username,
        string password,
        ILogger? logger,
        int timeout = 100000)
    {
        return new DigestAuthenticator(username, password, timeout, logger);
    }

    /// <summary>
    ///     Creates a new <see cref="DigestAuthenticator" /> using only username and password,
    ///     matching the original v1.0–v1.4 constructor signature.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="password">The password.</param>
    /// <returns>A configured <see cref="DigestAuthenticator" /> instance.</returns>
    [Obsolete(
        "Use the DigestAuthenticator constructor directly. This method exists for backward compatibility with v1.x.")]
    public static DigestAuthenticator Create(
        string username,
        string password)
    {
        return new DigestAuthenticator(username, password);
    }
}
