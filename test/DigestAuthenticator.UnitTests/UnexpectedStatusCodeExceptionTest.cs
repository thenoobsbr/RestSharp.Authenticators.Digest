using System;

using RestSharp.Authenticators.Digest.Exceptions;

using Shouldly;

using Xunit;

namespace RestSharp.Authenticators.Digest.UnitTests;

[Trait("Category", "UnitTest")]
[Trait("Class", nameof(UnexpectedStatusCodeException))]
public class UnexpectedStatusCodeExceptionTest
{
    [Fact]
    public void Given_Exception_When_Checked_Then_IsException()
    {
        var ex = new UnexpectedStatusCodeException("test");

        ex.ShouldBeAssignableTo<Exception>();
    }

    [Fact]
    public void Given_Message_When_Creating_Then_MessageIsSet()
    {
        var ex = new UnexpectedStatusCodeException("test message");

        ex.Message.ShouldBe("test message");
    }

    [Fact]
    public void Given_NullMessage_When_Creating_Then_DefaultMessage()
    {
        var ex = new UnexpectedStatusCodeException(null);

        ex.Message.ShouldNotBeNullOrEmpty();
    }
}
