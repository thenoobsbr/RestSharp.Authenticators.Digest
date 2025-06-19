# Release notes

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

## v2.0.1

- Add ability to change client options from digest RestClient object