namespace RestSharp.Authenticators.Digest
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text;
    using System;

    using RestSharp;

    /// <summary>
    /// DigestAuthenticatorManager class.
    /// </summary>
    internal class DigestAuthenticatorManager
    {
        /// <summary>
        /// Header Realm.
        /// </summary>
        private const string DIGEST_REALM = "Digest realm";

        /// <summary>
        /// Header Realm.
        /// </summary>
        private const string REALM = "realm";

        /// <summary>
        /// Header nonce.
        /// </summary>
        private const string NONCE = "nonce";

        /// <summary>
        /// Header qop.
        /// </summary>
        private const string QOP = "qop";

        /// <summary>
        /// The host.
        /// </summary>
        private readonly Uri _host;

        /// <summary>
        /// The user.
        /// </summary>
        private readonly string _username;

        /// <summary>
        /// The password.
        /// </summary>
        private readonly string _password;

        /// <summary>
        /// The timeout.
        /// </summary>
        private readonly int _timeout;

        /// <summary>
        /// The Realm that is returned by the first digest request (without the data).
        /// </summary>
        private string _realm;

        /// <summary>
        /// The nonce that is returned by the first digest request (without the data).
        /// </summary>
        private string _nonce;

        /// <summary>
        /// The qop that is returned by the first digest request (without the data).
        /// </summary>
        private string _qop;

        /// <summary>
        /// The cnounce that is generated randomly by the application.
        /// </summary>
        private string _cnonce;

        /// <summary>
        /// The nounce count (usually 000001)
        /// </summary>
        private const int NONCE_COUNT = 1;

        /// <summary>
        /// Creates a new instance of <see cref="DigestAuthenticatorManager"/> class.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="timeout">The timeout.</param>
        public DigestAuthenticatorManager(Uri host, string username, string password, int timeout)
        {
            _host = host;
            _username = username;
            _password = password;
            _timeout = timeout;
        }

        /// <summary>
        /// Generate the MD5 Hash.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>The MD5.</returns>
        private static string GenerateMD5(string input)
        {
            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hash = MD5.Create().ComputeHash(inputBytes);
            var stringBuilder = new StringBuilder();
            hash.ToList().ForEach(b => stringBuilder.Append(b.ToString("x2")));
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Gets the digest header.
        /// </summary>
        /// <param name="digestUri">The digest uri.</param>
        /// <param name="method">The method.</param>
        /// <returns>The digest header.</returns>
        public string GetDigestHeader(string digestUri, Method method)
        {
            var hash1 = GenerateMD5($"{_username}:{_realm}:{_password}");
            var hash2 = GenerateMD5($"{method}:{digestUri}");
            var digestResponse = GenerateMD5($"{hash1}:{_nonce}:{NONCE_COUNT:00000000}:{_cnonce}:{_qop}:{hash2}");
            return $"Digest username=\"{_username}\"," +
                $"realm=\"{_realm}\"," +
                $"nonce=\"{_nonce}\"," +
                $"uri=\"{digestUri}\"," +
                "algorithm=MD5," +
                $"response=\"{digestResponse}\"," +
                $"qop={_qop}," +
                $"nc={NONCE_COUNT:00000000}," +
                $"cnonce=\"{_cnonce}\"";
        }

        /// <summary>
        /// Gets the digest auth header.
        /// </summary>
        /// <param name="path">The request path.</param>
        /// <param name="method">The request method.</param>
        public void GetDigestAuthHeader(string path, Method method)
        {
            var uri = new Uri(_host, path);
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = method.ToString();
            request.ContentLength = 0;
            request.Timeout = _timeout;

            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                System.Diagnostics.Debug.WriteLine(response);
            }
            catch (WebException ex)
            {
                GetDigestDataFromException(ex);
            }
        }

        /// <summary>
        /// Gets the digest data from exception.
        /// </summary>
        /// <param name="ex">The exception.</param>
        private void GetDigestDataFromException(WebException ex)
        {
            if (ex.Response == null || ((HttpWebResponse)ex.Response).StatusCode != HttpStatusCode.Unauthorized)
            {
                throw ex;
            }

            var wwwAuthenticateHeader = TransformHeaderToDictionary(
                ex.Response.Headers["WWW-Authenticate"]
            );

            _cnonce = new Random()
                .Next(123400, 9999999)
                .ToString(CultureInfo.InvariantCulture);

            _realm = wwwAuthenticateHeader.GetFirstHeader(DIGEST_REALM, REALM);
            _nonce = wwwAuthenticateHeader.GetHeader(NONCE);
            _qop = wwwAuthenticateHeader.GetHeader(QOP);
        }

        /// <summary>
        /// Transform the header to dictionary.
        /// </summary>
        /// <param name="wwwAuthenticateHeader">The header</param>
        /// <returns>A instance of <see cref="IDictionary{K,V}"/>.</returns>
        private static IDictionary<string, string> TransformHeaderToDictionary(string wwwAuthenticateHeader)
        {
            return wwwAuthenticateHeader
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Split('='))
                .ToDictionary(
                    split => split[0].Trim(),
                    split => split[1].Replace('"', ' ').Trim());
        }
    }

    /// <summary>
    /// Dictionary extension.
    /// </summary>
    internal static class DictionaryHeaderExtension
    {
        internal static string GetHeader(this IDictionary<string, string> header, string key)
        {
            if (header.TryGetValue(key, out var value))
            {
                return value;
            }

            throw new ApplicationException($"Header not found: {key}");
        }

        internal static string GetFirstHeader(this IDictionary<string, string> header, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (header.TryGetValue(key, out var value))
                {
                    return value;
                }
            }

            throw new ApplicationException($"No Headers found with following keys: {string.Join(",", keys)}");
        }
    }
}
