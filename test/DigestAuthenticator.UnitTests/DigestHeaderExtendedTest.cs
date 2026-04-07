using System;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using NSubstitute;

using Shouldly;

using Xunit;

namespace RestSharp.Authenticators.Digest.UnitTests;

[Trait("Category", "UnitTest")]
[Trait("Class", nameof(DigestHeader))]
public class DigestHeaderExtendedTest
{
    [Fact]
    public void Given_ValidHeader_When_CallingToString_Without_Opaque_Then_OpaqueIsEmpty()
    {
        const string HEADER = "Digest realm=\"test\", nonce=\"abc\", qop=\"auth\"";
        var digestHeader = new DigestHeader(HEADER, NullLogger.Instance);

        var result = digestHeader.ToString();

        result.ShouldContain("Realm=\"test\"");
        result.ShouldContain("Nonce=\"abc\"");
        result.ShouldContain("Qop=\"auth\"");
        result.ShouldContain("Opaque=\"\"");
    }

    [Fact]
    public void Given_NonceCountConstant_Then_ValueIsOne()
    {
        DigestHeader.NONCE_COUNT.ShouldBe(1);
    }

    [Fact]
    public void Given_NonceConstant_Then_ValueIsNonce()
    {
        DigestHeader.NONCE.ShouldBe("nonce");
    }

    [Fact]
    public void Given_QopConstant_Then_ValueIsQop()
    {
        DigestHeader.QOP.ShouldBe("qop");
    }

    [Fact]
    public void Given_RealmConstant_Then_ValueIsRealm()
    {
        DigestHeader.REALM.ShouldBe("realm");
    }

    [Fact]
    public void Given_OpaqueConstant_Then_ValueIsOpaque()
    {
        DigestHeader.OPAQUE.ShouldBe("opaque");
    }

    [Fact]
    public void Given_RegexPatternConstant_Then_IsNotEmpty()
    {
        DigestHeader.REGEX_PATTERN.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Given_HeaderWithAllFields_When_Parsing_Then_AllFieldsArePopulated()
    {
        const string HEADER =
            "Digest realm=\"full-realm\", nonce=\"full-nonce\", qop=\"auth\", opaque=\"full-opaque\", domain=\"/api\", stale=\"false\"";
        var digestHeader = new DigestHeader(HEADER, NullLogger.Instance);

        digestHeader.Realm.ShouldBe("full-realm");
        digestHeader.Nonce.ShouldBe("full-nonce");
        digestHeader.Qop.ShouldBe("auth");
        digestHeader.Opaque.ShouldBe("full-opaque");
    }

    [Fact]
    public void Given_InvalidHeader_When_Parsing_Then_LoggerReceivesErrorCall()
    {
        var logger = Substitute.For<ILogger>();

        Should.Throw<ArgumentException>(() => new DigestHeader("invalid", logger));

        logger.ReceivedWithAnyArgs().LogError("test");
    }

    [Fact]
    public void Given_OnlyNonceAndQop_When_Parsing_Then_ThrowsBecauseRealmMissing()
    {
        const string HEADER = "Digest nonce=\"n\", qop=\"auth\"";

        var ex = Should.Throw<ArgumentException>(() => new DigestHeader(HEADER, NullLogger.Instance));

        ex.Message.ShouldContain("Cannot load all required data");
    }

    [Fact]
    public void Given_OnlyRealmAndNonce_When_Parsing_Then_SucceedsBecauseQopIsOptionalPerRfc2069()
    {
        const string HEADER = "Digest realm=\"r\", nonce=\"n\"";

        var digestHeader = new DigestHeader(HEADER, NullLogger.Instance);

        digestHeader.Realm.ShouldBe("r");
        digestHeader.Nonce.ShouldBe("n");
        digestHeader.Qop.ShouldBeNull();
    }

    [Fact]
    public void Given_OnlyRealmAndQop_When_Parsing_Then_ThrowsBecauseNonceMissing()
    {
        const string HEADER = "Digest realm=\"r\", qop=\"auth\"";

        var ex = Should.Throw<ArgumentException>(() => new DigestHeader(HEADER, NullLogger.Instance));

        ex.Message.ShouldContain("Cannot load all required data");
    }

    [Theory]
    [InlineData("auth-int")]
    [InlineData("auth")]
    public void Given_SingleQopValue_When_Parsing_Then_QopIsExact(string qop)
    {
        var header = $"Digest realm=\"r\", nonce=\"n\", qop=\"{qop}\"";
        var digestHeader = new DigestHeader(header, NullLogger.Instance);

        digestHeader.Qop.ShouldBe(qop);
    }

    [Fact]
    public void Given_HeaderWithEmptyRealm_When_Parsing_Then_ThrowsArgumentException()
    {
        const string HEADER = "Digest realm=\"\", nonce=\"n\", qop=\"auth\"";

        Should.Throw<ArgumentException>(() => new DigestHeader(HEADER, NullLogger.Instance));
    }

    [Fact]
    public void Given_HeaderWithUnicodeRealm_When_Parsing_Then_RealmIsExtracted()
    {
        const string HEADER = "Digest realm=\"meu-servidor-br\", nonce=\"abc\", qop=\"auth\"";
        var digestHeader = new DigestHeader(HEADER, NullLogger.Instance);

        digestHeader.Realm.ShouldBe("meu-servidor-br");
    }

    [Fact]
    public void Given_HeaderWithNumbersInNonce_When_Parsing_Then_NonceIsExtracted()
    {
        const string HEADER = "Digest realm=\"r\", nonce=\"1234567890\", qop=\"auth\"";
        var digestHeader = new DigestHeader(HEADER, NullLogger.Instance);

        digestHeader.Nonce.ShouldBe("1234567890");
    }
}
