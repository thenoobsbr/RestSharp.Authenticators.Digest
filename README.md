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
* .NET Framework 4.7.1+ (limited by RestSharp 114.0.0)
* .NET Core 2.0+
* .NET 5, 6, 7, 8, 9, 10+
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

* Implements HTTP Digest Authentication with support for both modern and legacy protocols:
  * **RFC 2617 / RFC 7616** — Full digest authentication with `qop=auth`, `cnonce`, and nonce counting
  * **RFC 2069** — Legacy digest authentication without `qop` (for older servers)
* Automatic protocol detection — the library reads the server's `WWW-Authenticate` challenge and selects the correct hash format
* Nonce, realm, opaque, and qop support
* Compatible with servers that require `WWW-Authenticate: Digest`
* Stateless and thread-safe
* Works with `RestClient` and `IRestRequest` out of the box

---

## 🔐 Protocol Support

The library **automatically detects** which protocol the server uses based on the `WWW-Authenticate` response header. No configuration is needed — the same code works for both protocols.

### RFC 2617 / RFC 7616 (modern servers)

When the server includes `qop` in its challenge (e.g., `qop="auth"` or `qop="auth,auth-int"`), the library uses the full digest scheme:

```
response = MD5(MD5(username:realm:password):nonce:nc:cnonce:qop:MD5(method:uri))
```

The generated `Authorization` header includes `qop`, `nc`, and `cnonce` fields.

### RFC 2069 (legacy servers)

When the server **does not** include `qop` in its challenge, the library falls back to the simplified RFC 2069 scheme:

```
response = MD5(MD5(username:realm:password):nonce:MD5(method:uri))
```

The generated `Authorization` header omits `qop`, `nc`, and `cnonce` fields, as required by the legacy protocol.

### Usage

The usage is identical for both protocols — just create the authenticator and let the library handle the rest:

```csharp
var restOptions = new RestClientOptions("https://api.myhost.com/api/v1")
{
    Authenticator = new DigestAuthenticator("username", "password")
};

var client = new RestClient(restOptions);
var request = new RestRequest("resource");
var response = await client.ExecuteAsync(request);
```

This works transparently whether the server uses RFC 2069 or RFC 2617/7616.

---

## 🧪 Testing

The project includes comprehensive unit and integration tests covering both RFC 2069 and RFC 2617/7616 protocols.

Integration tests use embedded HTTP listeners that simulate digest servers for both protocols.

Tests target multiple frameworks (`net8.0`, `net10.0`).

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
