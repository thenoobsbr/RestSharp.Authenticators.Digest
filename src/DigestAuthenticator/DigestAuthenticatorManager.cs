using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace RestSharp.Authenticators.Digest
{
    /// <summary>
    ///     DigestAuthenticatorManager class.
    /// </summary>
    internal class DigestAuthenticatorManager
    {
        private readonly Uri _host;

        private readonly string _password;

        private readonly int _timeout;

        private readonly string _username;

        /// <summary>
        ///     The cnounce that is generated randomly by the application.
        /// </summary>
        private string _cnonce;

        /// <summary>
        ///     The nonce that is returned by the first digest request (without the data).
        /// </summary>
        private string _nonce;

        /// <summary>
        ///     The qop that is returned by the first digest request (without the data).
        /// </summary>
        private string _qop;

        /// <summary>
        ///     The Realm that is returned by the first digest request (without the data).
        /// </summary>
        private string _realm;

        /// <summary>
        ///     Creates a new instance of <see cref="DigestAuthenticatorManager" /> class.
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
        ///     Gets the digest auth header.
        /// </summary>
        /// <param name="path">The request path.</param>
        /// <param name="method">The request method.</param>
        public void GetDigestAuthHeader(string path, Method method)
        {
            var uri = new Uri(_host, path);
            var request = (HttpWebRequest) WebRequest.Create(uri);
            request.Method = method.ToString();
            request.ContentLength = 0;
            request.Timeout = _timeout;

            try
            {
                var response = (HttpWebResponse) request.GetResponse();
                Debug.WriteLine(response);
            }
            catch (WebException ex)
            {
                GetDigestDataFromException(ex);
            }
        }

        /// <summary>
        ///     Gets the digest header.
        /// </summary>
        /// <param name="digestUri">The digest uri.</param>
        /// <param name="method">The method.</param>
        /// <returns>The digest header.</returns>
        public string GetDigestHeader(string digestUri, Method method)
        {
            var hash1 = GenerateMD5($"{_username}:{_realm}:{_password}");
            var hash2 = GenerateMD5($"{method}:{digestUri}");
            var digestResponse =
                GenerateMD5($"{hash1}:{_nonce}:{DigestHeader.NONCE_COUNT:00000000}:{_cnonce}:{_qop}:{hash2}");
            return $"Digest username=\"{_username}\"," +
                   $"realm=\"{_realm}\"," +
                   $"nonce=\"{_nonce}\"," +
                   $"uri=\"{digestUri}\"," +
                   "algorithm=MD5," +
                   $"response=\"{digestResponse}\"," +
                   $"qop={_qop}," +
                   $"nc={DigestHeader.NONCE_COUNT:00000000}," +
                   $"cnonce=\"{_cnonce}\"";
        }

        /// <summary>
        ///     Generate the MD5 Hash.
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

        private void GetDigestDataFromException(WebException ex)
        {
            if (ex.Response == null || ((HttpWebResponse) ex.Response).StatusCode != HttpStatusCode.Unauthorized)
            {
                throw ex;
            }

            var digestHeader = new DigestHeader(ex.Response.Headers["WWW-Authenticate"]);

            _cnonce = new Random()
                .Next(123400, 9999999)
                .ToString(CultureInfo.InvariantCulture);

            _realm = digestHeader.Realm;
            _nonce = digestHeader.Nonce;
            _qop = digestHeader.Qop;
        }
    }
}
