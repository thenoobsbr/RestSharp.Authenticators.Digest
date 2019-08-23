# RestSharp.DigestAuthenticator
Extends RestSharp features for digest authentication

## Examples
```CSharp
namespace Example
{
    using RestSharp;
    using RestSharp.Authenticators.Digest;
    using System;

    public class Program
    {
        public static void Main(string[] args)
        {
            var client = new RestClient("http://api.myhost.com/api/v1");
            client.Authenticator = new DigestAuthenticator("user", "password");
            var request = new RestRequest("values", Method.GET);
            request.AddHeader("Content-Type", "application/json");

            var response = client.Execute(request);

            Console.WriteLine(response.StatusCode);
            Console.WriteLine(response.Content);
            Console.ReadKey(true);
        }
    }
}
```
