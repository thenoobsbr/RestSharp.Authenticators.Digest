namespace RestSharp.Authenticators.Digest
{
    using System.Net;

    using RestSharp.Authenticators;
    using RestSharp;

    /// <summary>
    /// Midleware de autenticação Digest para <see cref="IRestClient" />.
    /// </summary>
    public class DigestAuthenticator : IAuthenticator
    {
        /// <summary>
        /// O usuário da autenticação.
        /// </summary>
        private readonly string usuario;

        /// <summary>
        /// A senha de autenticação.
        /// </summary>
        private readonly string senha; 

        /// <summary>
        /// Inicializa uma nova instância de <see cref="DigestAuthenticator" />.
        /// </summary>
        /// <param name="usuario">O usuário de autenticação.</param>
        /// <param name="senha">A senha de autenticação.</param>
        public DigestAuthenticator(string usuario, string senha)
        {
            this.usuario = usuario;
            this.senha = senha;
        } 

        /// <inheritdoc cref="IAuthenticator" />.
        public void Authenticate(IRestClient client, IRestRequest request)
        {
            request.Credentials = new NetworkCredential(usuario, senha); 

            var uri = client.BuildUri(request);
            var manager = new DigestAuthenticatorManager(client.BaseUrl, usuario, senha);
            manager.ObterDadosAutenticacaoDigest(uri.PathAndQuery, request.Method);
            var digestHeader = manager.ObterDigestHeader(uri.PathAndQuery, request.Method);
            request.AddParameter("Authorization", digestHeader, ParameterType.HttpHeader);
        }
    } 
}