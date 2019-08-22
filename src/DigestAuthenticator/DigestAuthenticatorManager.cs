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
    /// Classe de gerenciamento da autenticação Digest.
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
        /// O host de conexão.
        /// </summary>
        private Uri host;

        /// <summary>
        /// O usuário de autenticação.
        /// </summary>
        private string usuario;

        /// <summary>
        /// A senha de autenticação.
        /// </summary>
        private string senha;

        /// <summary>
        /// O Realm que é retornado pela primeira requisição digest (sem os dados).
        /// </summary>
        private string realm;

        /// <summary>
        /// O nonce que é retornado pela primeira requisição digest (sem os dados).
        /// </summary>
        private string nonce;

        /// <summary>
        /// O qoq que é retornado pela primeira requisição digest (sem os dados).
        /// </summary>
        private string qop;

        /// <summary>
        /// O cnounce que é gerado randômincamente pela aplicação.
        /// </summary>
        private string cnonce;

        /// <summary>
        /// O nounce count (normalmente 000001)
        /// </summary>
        private const int NONCE_COUNT = 1;

        /// <summary>
        /// Inicializa uma nova instância de <see cref="DigestAuthenticatorManager" />.
        /// </summary>
        /// <param name="host">O host da autenticação.</param>
        /// <param name="usuario">O usuário da autenticação.</param>
        /// <param name="senha">A senha da autenticação.</param>
        public DigestAuthenticatorManager(Uri host, string usuario, string senha)
        {
            this.host = host;
            this.usuario = usuario;
            this.senha = senha;
        } 

        /// <summary>
        /// Calcula o Hash MD5.
        /// </summary>
        /// <param name="input">O dado que terá o hash calculado.</param>
        /// <returns>Retorna a string representado o hash MD5.</returns>
        private string CalcularMD5(string input)
        {
            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hash = MD5.Create().ComputeHash(inputBytes);
            var stringBuilder = new StringBuilder();
            hash.ToList().ForEach(b => stringBuilder.Append(b.ToString("x2")));
            return stringBuilder.ToString();
        } 

        public string ObterDigestHeader(string digestUri, Method metodo)
        {
            var hash1 = CalcularMD5($"{this.usuario}:{this.realm}:{this.senha}");
            var hash2 = CalcularMD5($"{metodo}:{digestUri}");
            var digestResponse = CalcularMD5($"{hash1}:{this.nonce}:{NONCE_COUNT:00000000}:{this.cnonce}:{this.qop}:{hash2}"); 
            return $"Digest username=\"{this.usuario}\"," +
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
        /// Obtém os dados da autenticação Digest.
        /// </summary>
        /// <param name="path">O path da requisição.</param>
        /// <param name="metodo">O método da requisição.</param>
        public void ObterDadosAutenticacaoDigest(string path, Method metodo)
        {
            var uri = new Uri($"{host}{path}"); 
            var request = (HttpWebRequest) WebRequest.Create(uri); 
            request.Method = metodo.ToString();

            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse) request.GetResponse();
            }
            catch (WebException ex)
            {
                this.CapturarExcecaoEObterDadosDigest(ex);
            }
        }

        /// <summary>
        /// Captura a exceção e obtém os dados do digest.
        /// </summary>
        /// <param name="ex">A exceção.</param>
        private void CapturarExcecaoEObterDadosDigest(WebException ex)
        {
            if (ex.Response == null || ((HttpWebResponse) ex.Response).StatusCode != HttpStatusCode.Unauthorized)
            {
                throw ex;
            }

            var wwwAuthenticateHeader = TransformarHeaderEmDicionario(
                ex.Response.Headers["WWW-Authenticate"]
            );

            this.cnonce = new Random()
                .Next(123400, 9999999)
                .ToString(CultureInfo.InvariantCulture);

            this.realm = wwwAuthenticateHeader.ObterHeader(REALM);
            this.nonce = wwwAuthenticateHeader.ObterHeader(NONCE);
            this.qop = wwwAuthenticateHeader.ObterHeader(QOP);
        }

        /// <summary>
        /// Transforma o header do Digest em dicionário.
        /// </summary>
        /// <param name="wwwAuthenticateHeader">O header</param>
        /// <returns>O dicionário com os dados do header.</returns>
        private static IDictionary<string, string> TransformarHeaderEmDicionario(string wwwAuthenticateHeader)
        {
            return wwwAuthenticateHeader
                .Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Split('='))
                .ToDictionary(
                    split => split[0].Trim(),
                    split => split[1].Replace('"', ' ').Trim());
        }
    }

    /// <summary>
    /// Extensão para dicionário para obter dados.
    /// </summary>
    internal static class IDictionaryHeaderExtension
    {
        internal static string ObterHeader(this IDictionary<string, string> header, string chave)
        {
            if (header.TryGetValue(chave, out var valor))
            {
                return valor;
            }

            throw new ApplicationException(string.Format("Header {0} não encontrado", chave));
        } 
    }
}