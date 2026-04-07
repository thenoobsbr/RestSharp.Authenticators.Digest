using System;
using System.Collections.Generic;
using System.IO;
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
///     A digest server stub that requires a request body for POST/PUT requests.
///     Returns 400 Bad Request (instead of 401) when a POST/PUT arrives without a body,
///     simulating devices like ONVIF cameras that validate the payload before authentication.
/// </summary>
public partial class DigestBodyRequiredServerStub : IAsyncDisposable
{
    private const string PASSWORD = "test-password";

#if NET8_0
    private const int PORT = 8078;
#else
    private const int PORT = 8079;
#endif

    private const string REALM = "test-realm-body-required";
    private const string USERNAME = "test-user";
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Task _serverTask;

    public static string BaseUrl => $"http://localhost:{PORT}";

    public DigestBodyRequiredServerStub()
    {
        var nonce = GenerateNonce();

        _serverTask = StartServer(REALM, USERNAME, PASSWORD, nonce, PORT);
        Console.WriteLine($"Body-required server started! port: {PORT}.");
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        Console.WriteLine("Shutting down the body-required server...");
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
            !authValues.TryGetValue("cnonce", out var receivedCNonce) ||
            !authValues.TryGetValue("qop", out var receivedQop) ||
            !authValues.TryGetValue("uri", out var uri) ||
            !authValues.TryGetValue("response", out var receivedResponse))
        {
            return false;
        }

        if (realm != receivedRealm || nonce != receivedNonce || username != receivedUsername)
        {
            return false;
        }

        var hash1 = CalculateMD5Hash($"{username}:{realm}:{password}");
        var hash2 = CalculateMD5Hash($"{request.HttpMethod.ToUpperInvariant()}:{uri}");

        var expectedResponse =
            CalculateMD5Hash(
                $"{hash1}:{receivedNonce}:{DigestHeader.NONCE_COUNT:00000000}:{receivedCNonce}:{receivedQop}:{hash2}");

        return expectedResponse == receivedResponse;
    }

    private static bool IsBodyRequired(string httpMethod)
    {
        return httpMethod is "POST" or "PUT" or "PATCH";
    }

    private static void SendDigestAuthenticationChallenge(HttpListenerResponse response, string realm, string nonce)
    {
        response.StatusCode = 401;
        response.Headers.Add("WWW-Authenticate", $"Digest realm=\"{realm}\", nonce=\"{nonce}\", qop=\"auth,auth-int\"");
        response.OutputStream.Close();
    }

    private static void SendBadRequest(HttpListenerResponse response)
    {
        response.StatusCode = 400;
        var buffer = Encoding.UTF8.GetBytes("Bad Request: body is required");
        response.ContentType = "text/plain";
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }

    private static bool HasBody(HttpListenerRequest request)
    {
        if (!request.HasEntityBody)
        {
            return false;
        }

        using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
        var body = reader.ReadToEnd();
        return !string.IsNullOrWhiteSpace(body);
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

                // Simulate devices that return 400 when body is missing on POST/PUT
                if (IsBodyRequired(request.HttpMethod) && !HasBody(request))
                {
                    SendBadRequest(response);
                    continue;
                }

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
