using System;

namespace RestSharp.Authenticators.Digest.Exceptions;

public class UnexpectedStatusCodeException : Exception
{
    public UnexpectedStatusCodeException(string? message)
        : base(message)
    {
    }
}
