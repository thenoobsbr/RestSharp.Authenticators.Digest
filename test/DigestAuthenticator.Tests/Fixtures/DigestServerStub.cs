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

namespace RestSharp.Authenticators.Digest.Tests.Fixtures;

public class DigestServerStub : IAsyncDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Task _serverTask;
    
    private const string REALM = "test-realm";
    private const string USERNAME = "test-user";
    private const string PASSWORD = "test-password";
    private const int PORT = 8080;

    public DigestServerStub()
    {
        var nonce = GenerateNonce();

        _cancellationTokenSource = new CancellationTokenSource();
        
        _serverTask = StartServer(REALM, USERNAME, PASSWORD, nonce, PORT);
        Console.WriteLine($"Server started! port: {PORT}.");
    }

    public IRestClient CreateClient(ILogger logger)
    {
        var restOptions = new RestClientOptions($"http://localhost:{PORT}")
        {
            Authenticator = new DigestAuthenticator(USERNAME, PASSWORD, logger: logger)
        };

        return new RestClient(restOptions);
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        Console.WriteLine("Shutting down the server...");
        _cancellationTokenSource.Cancel();
        await _serverTask;
    }

    private static string CalculateMD5Hash(string input)
    {
        var inputBytes = Encoding.ASCII.GetBytes(input);
        var hash = MD5.Create().ComputeHash(inputBytes);
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

    private static bool IsDigestAuthenticated(HttpListenerRequest request, string realm, string username, string password, string nonce)
    {
        var authorizationHeader = request.Headers["Authorization"];

        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Digest"))
        {
            return false;
        }

        var matches = Regex.Matches(authorizationHeader, @"(\w+)=""?([^"",\s]+)""?");
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
            CalculateMD5Hash($"{hash1}:{receivedNonce}:{DigestHeader.NONCE_COUNT:00000000}:{receivedCNonce}:{receivedQop}:{hash2}");

        return expectedResponse == receivedResponse;
    }

    private static void SendDigestAuthenticationChallenge(HttpListenerResponse response, string realm, string nonce)
    {
        response.StatusCode = 401;
        response.Headers.Add("WWW-Authenticate", $"Digest realm=\"{realm}\", nonce=\"{nonce}\", qop=\"auth\"");
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
}
