# Release notes

## v3.0.0

- Updates RestSharp dependency to version 114.0.0;
- Adds CancellationToken support to the Authenticate method (IAuthenticator breaking change in RestSharp 114);
- Threads CancellationToken through the digest handshake pipeline;
- Adds backward compatibility layer (DigestAuthenticatorLegacy) for consumers upgrading from v2.x;
- Drops net6.0 test target (EOL);
- Adds comprehensive unit tests for DigestAuthenticator, DigestHeader, DigestAuthenticatorManager, and
  DigestAuthenticatorLegacy;
- Adds integration tests for error scenarios, wrong credentials, multiple HTTP methods, and concurrent requests;

## v1.4.0

- Fixies the authentication when the qop value comes without `"`;
- Updates packages;
- Adds a real integration tests;

## v1.5.0

- Adjusts the nuget package;
- Adds optional ILogger to the DigestAuthenticator constructor;
- Adds optional request timeout to the DigestAuthenticator constructor;

## v1.6.0

- GetDigestAuthHeader should inherit proxy;

## v2.0.0

- Changes the compatibility to the RestSharp version `111.2.0` or greater.

## v2.1.0

- Add ability to change client options from digest RestClient object
