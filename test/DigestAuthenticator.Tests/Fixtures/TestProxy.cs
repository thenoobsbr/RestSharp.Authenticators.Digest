using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RestSharp.Authenticators.Digest.Tests.Fixtures;
internal class TestProxy : IWebProxy
{
    private readonly Action _onProxyCalled;

    public TestProxy(Action onProxyCalled)
    {
        _onProxyCalled = onProxyCalled;
    }

    public Uri GetProxy(Uri destination)
    {
        _onProxyCalled();
        return destination;
    }

    public bool IsBypassed(Uri host) => false;

    public ICredentials? Credentials { get; set; }
}
