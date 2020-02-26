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
        private const string REALM = "Digest realm";

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
        private Uri host;

        /// <summary>
        /// The user.
        /// </summary>
        private string username;

        /// <summary>
        /// The password.
        /// </summary>
        private string password;

        /// <summary>
        /// The Realm that is returned by the first digest request (without the data).
        /// </summary>
        private string realm;

        /// <summary>
        /// The nonce that is returned by the first digest request (without the data).
        /// </summary>
        private string nonce;

        /// <summary>
        /// The qop that is returned by the first digest request (without the data).
        /// </summary>
        private string qop;

        /// <summary>
        /// The cnounce that is generated randomly by the application.
        /// </summary>
        private string cnonce;

        /// <summary>
        /// The nounce count (usualy 000001)
        /// </summary>
        private const int NONCE_COUNT = 1;

        /// <summary>
        /// Creates a new instance of <see cref="DigestAuthenticatorManager"/> class.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        public DigestAuthenticatorManager(Uri host, string username, string password)
        {
            this.host = host;
            this.username = username;
            this.password = password;
        }

        /// <summary>
        /// Generate the MD5 Hash.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>The MD5.</returns>
        private string GenerateMD5(string input)
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
            var hash1 = GenerateMD5($"{this.username}:{this.realm}:{this.password}");
            var hash2 = GenerateMD5($"{method}:{digestUri}");
            var digestResponse = GenerateMD5($"{hash1}:{this.nonce}:{NONCE_COUNT:00000000}:{this.cnonce}:{this.qop}:{hash2}");
            return $"Digest username=\"{this.username}\"," +
                $"realm=\"{this.realm}\"," +
                $"nonce=\"{this.nonce}\"," +
                $"uri=\"{digestUri}\"," +
                $"algorithm=MD5," +
                $"response=\"{digestResponse}\"," +
                $"qop={this.qop}," +
                $"nc={NONCE_COUNT:00000000}," +
                $"cnonce=\"{this.cnonce}\"";
        }

        /// <summary>
        /// Gets the digest auth header.
        /// </summary>
        /// <param name="path">The request path.</param>
        /// <param name="method">The request method.</param>
        public void GetDigestAuthHeader(string path, Method method)
        {
            var uri = new Uri(host, path);
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = method.ToString();
            request.ContentLength = 0;

            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                this.GetDigestDataFromException(ex);
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

            this.cnonce = new Random()
                .Next(123400, 9999999)
                .ToString(CultureInfo.InvariantCulture);

            this.realm = wwwAuthenticateHeader.GetHeader(REALM);
            this.nonce = wwwAuthenticateHeader.GetHeader(NONCE);
            this.qop = wwwAuthenticateHeader.GetHeader(QOP);
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
    internal static class IDictionaryHeaderExtension
    {
        internal static string GetHeader(this IDictionary<string, string> header, string key)
        {
            if (header.TryGetValue(key, out var value))
            {
                return value;
            }

            throw new ApplicationException(string.Format("Header not found: {0}", key));
        }
    }
}