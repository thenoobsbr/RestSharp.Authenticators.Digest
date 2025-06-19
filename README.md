# RestSharp.Authenticators.Digest

A lightweight and extensible **Digest authentication plugin** for [RestSharp](https://github.com/restsharp/RestSharp),
implemented as an `IAuthenticator`.

[![NuGet](https://img.shields.io/nuget/v/RestSharp.Authenticators.Digest.svg)](https://www.nuget.org/packages/RestSharp.Authenticators.Digest/)
[![License](https://img.shields.io/github/license/thenoobsbr/RestSharp.Authenticators.Digest)](LICENSE)
![.NET Standard](https://img.shields.io/badge/.NET-Standard%202.0-blue)
[![CodeQL - C#](https://github.com/thenoobsbr/RestSharp.Authenticators.Digest/actions/workflows/codeql.yml/badge.svg)](https://github.com/thenoobsbr/RestSharp.Authenticators.Digest/actions/workflows/codeql.yml)

---

## 📦 Installation

Install via [NuGet](https://www.nuget.org/packages/RestSharp.Authenticators.Digest):

```bash
dotnet add package RestSharp.Authenticators.Digest
````

---

## ✅ Compatibility

* .NET Standard 2.0
* .NET Core 3.1+
* .NET 5, 6, 7, 8, 9+
* Fully compatible with `RestClient` from [RestSharp](https://github.com/restsharp/RestSharp)

---

## 🚀 Quick Usage

```csharp
namespace Example
{
    using RestSharp;
    using RestSharp.Authenticators.Digest;
    using System;

    public class Program
    {
        public static void Main(string[] args)
        {
            var restOptions = new RestClientOptions("https://api.myhost.com/api/v1")
            {
                Authenticator = new DigestAuthenticator(USERNAME, PASSWORD)
            };

            var client = new RestClient(restOptions);
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

---

## ✨ Features

* Implements HTTP Digest Authentication (RFC 7616)
* Nonce and realm support
* Compatible with servers that require `WWW-Authenticate: Digest`
* Stateless and thread-safe
* Works with `RestClient` and `IRestRequest` out of the box

---

## 🧪 Testing

Digest-authenticated servers can be mocked for integration testing using test frameworks or by hitting known
Digest-protected endpoints.

Tests for multiple target frameworks (`net6.0`, `net8.0`, `net9.0`) are included.

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).

---

## 🤝 Contributing

Contributions are welcome!

If you find a bug or have an idea for improvement:

* Open an [issue](https://github.com/thenoobsbr/RestSharp.Authenticators.Digest/issues)
* Or submit a [pull request](https://github.com/thenoobsbr/RestSharp.Authenticators.Digest/pulls)

Please follow the [contribution guidelines](CONTRIBUTING.md) if available.

---

## 📬 Contact

Maintained by [@thenoobsbr](https://github.com/thenoobsbr)
NuGet Package: [RestSharp.Authenticators.Digest](https://www.nuget.org/packages/RestSharp.Authenticators.Digest)
