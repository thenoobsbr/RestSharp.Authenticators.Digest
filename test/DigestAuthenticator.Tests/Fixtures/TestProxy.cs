using System;
using System.Net;

namespace RestSharp.Authenticators.Digest.Tests.Fixtures;

internal class TestProxy : IWebProxy
{
    private readonly Action _onProxyCalled;

    public TestProxy(Action onProxyCalled)
    {
        _onProxyCalled = onProxyCalled;
    }

    public ICredentials? Credentials { get; set; }

    public Uri GetProxy(Uri destination)
    {
        _onProxyCalled();
        return destination;
    }

    public bool IsBypassed(Uri host)
    {
        return false;
    }
}
