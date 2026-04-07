using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace RestSharp.Authenticators.Digest.IntegrationTests.Fixtures;

/// <summary>
///     A digest server stub that implements RFC 2069 (no qop in WWW-Authenticate challenge).
/// </summary>
public partial class DigestRfc2069ServerStub : IAsyncDisposable
{
    private const string PASSWORD = "test-password";

#if NET8_0
    private const int PORT = 8098;
#else
    private const int PORT = 8099;
#endif

    private const string REALM = "test-realm-rfc2069";
    private const string USERNAME = "test-user";
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Task _serverTask;

    public static string BaseUrl => $"http://localhost:{PORT}";

    public DigestRfc2069ServerStub()
    {
        var nonce = GenerateNonce();

        _serverTask = StartServer(REALM, USERNAME, PASSWORD, nonce, PORT);
        Console.WriteLine($"RFC 2069 Server started! port: {PORT}.");
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        Console.WriteLine("Shutting down the RFC 2069 server...");
#if NET8_0_OR_GREATER
        await _cancellationTokenSource.CancelAsync();
#else
        _cancellationTokenSource.Cancel();
#endif
        _cancellationTokenSource.Dispose();
        await _serverTask;
    }

    public static IRestClient CreateClient(ILogger logger)
    {
        var restOptions = new RestClientOptions($"http://localhost:{PORT}")
        {
            Authenticator = new DigestAuthenticator(USERNAME, PASSWORD, logger: logger)
        };

        return new RestClient(restOptions);
    }

    private static string CalculateMD5Hash(string input)
    {
        var inputBytes = Encoding.ASCII.GetBytes(input);
        var hash = MD5.HashData(inputBytes);
        var stringBuilder = new StringBuilder();
        hash.ToList().ForEach(b => stringBuilder.Append(b.ToString("x2")));
        return stringBuilder.ToString();
    }

    private static string GenerateNonce()
    {
        var nonceBytes = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(nonceBytes);
        }

        return Convert.ToBase64String(nonceBytes);
    }

    /// <summary>
    ///     Validates digest auth using RFC 2069 format: response = MD5(hash1:nonce:hash2), no qop/nc/cnonce.
    /// </summary>
    private static bool IsDigestAuthenticated(HttpListenerRequest request, string realm, string username,
        string password, string nonce)
    {
        var authorizationHeader = request.Headers["Authorization"];

        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Digest"))
        {
            return false;
        }

        var matches = MyRegex().Matches(authorizationHeader);
        var authValues = new Dictionary<string, string>();
        foreach (Match match in matches)
        {
            authValues[match.Groups[1].Value] = match.Groups[2].Value;
        }

        if (!authValues.TryGetValue("username", out var receivedUsername) ||
            !authValues.TryGetValue("realm", out var receivedRealm) ||
            !authValues.TryGetValue("nonce", out var receivedNonce) ||
            !authValues.TryGetValue("uri", out var uri) ||
            !authValues.TryGetValue("response", out var receivedResponse))
        {
            return false;
        }

        // RFC 2069 should NOT have qop, nc, or cnonce
        if (authValues.ContainsKey("qop") || authValues.ContainsKey("nc") || authValues.ContainsKey("cnonce"))
        {
            return false;
        }

        if (realm != receivedRealm || nonce != receivedNonce || username != receivedUsername)
        {
            return false;
        }

        var hash1 = CalculateMD5Hash($"{username}:{realm}:{password}");
        var hash2 = CalculateMD5Hash($"{request.HttpMethod.ToUpperInvariant()}:{uri}");

        // RFC 2069 format: response = MD5(hash1:nonce:hash2)
        var expectedResponse = CalculateMD5Hash($"{hash1}:{receivedNonce}:{hash2}");

        return expectedResponse == receivedResponse;
    }

    /// <summary>
    ///     Sends a 401 challenge WITHOUT qop (RFC 2069 style).
    /// </summary>
    private static void SendDigestAuthenticationChallenge(HttpListenerResponse response, string realm, string nonce)
    {
        response.StatusCode = 401;
        response.Headers.Add("WWW-Authenticate", $"Digest realm=\"{realm}\", nonce=\"{nonce}\"");
        response.OutputStream.Close();
    }

    private async Task StartServer(string realm, string username, string password, string nonce, int port)
    {
        using var listener = new HttpListener();
        listener.Prefixes.Add($"http://localhost:{port}/");
        listener.Start();

        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                var context = await listener.GetContextAsync();
                var request = context.Request;
                var response = context.Response;

                if (!IsDigestAuthenticated(request, realm, username, password, nonce))
                {
                    SendDigestAuthenticationChallenge(response, realm, nonce);
                    continue;
                }

                const string RESPONSE_DATA = "Successful authentication!";
                var buffer = Encoding.UTF8.GetBytes(RESPONSE_DATA);

                response.ContentType = "text/plain";
                response.ContentLength64 = buffer.Length;
                response.StatusCode = 200;
                await response.OutputStream.WriteAsync(buffer);
                response.OutputStream.Close();
            }
            catch (HttpListenerException)
            {
                // Ignoring an exception that occurred due to server shutdown
            }
        }

        listener.Stop();
    }

    [GeneratedRegex(@"(\w+)=""?([^"",\s]+)""?")]
    private static partial Regex MyRegex();
}
