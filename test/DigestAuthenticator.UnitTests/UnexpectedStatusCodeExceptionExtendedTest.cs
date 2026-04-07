using System;

using RestSharp.Authenticators.Digest.Exceptions;

using Shouldly;

using Xunit;

namespace RestSharp.Authenticators.Digest.UnitTests;

[Trait("Category", "UnitTest")]
[Trait("Class", nameof(UnexpectedStatusCodeException))]
public class UnexpectedStatusCodeExceptionExtendedTest
{
    [Fact]
    public void Given_EmptyMessage_When_Creating_Then_MessageIsEmpty()
    {
        var ex = new UnexpectedStatusCodeException("");

        ex.Message.ShouldBe("");
    }

    [Fact]
    public void Given_LongMessage_When_Creating_Then_MessageIsPreserved()
    {
        var longMessage = new string('a', 10000);
        var ex = new UnexpectedStatusCodeException(longMessage);

        ex.Message.ShouldBe(longMessage);
    }

    [Fact]
    public void Given_SpecialCharsMessage_When_Creating_Then_MessageIsPreserved()
    {
        const string MESSAGE = "Error: status=500 (Internal Server Error) @ /api/v1/resource?id=123";
        var ex = new UnexpectedStatusCodeException(MESSAGE);

        ex.Message.ShouldBe(MESSAGE);
    }

    [Fact]
    public void Given_Exception_When_CaughtAsException_Then_MessageIsAccessible()
    {
        Exception ex = new UnexpectedStatusCodeException("caught");

        ex.Message.ShouldBe("caught");
    }

    [Fact]
    public void Given_Exception_When_Checked_Then_InnerExceptionIsNull()
    {
        var ex = new UnexpectedStatusCodeException("test");

        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void Given_Exception_When_Thrown_Then_CanBeCaught()
    {
        Should.Throw<UnexpectedStatusCodeException>(() =>
            throw new UnexpectedStatusCodeException("thrown"));
    }
}
