using System;

using Microsoft.Extensions.Logging.Abstractions;

using Shouldly;

using Xunit;

namespace RestSharp.Authenticators.Digest.UnitTests;

[Trait("Category", "UnitTest")]
[Trait("Class", nameof(DigestHeader))]
public class DigestHeaderTest
{
    [Fact]
    public void Given_EmptyHeader_When_Parsing_Then_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => new DigestHeader("", NullLogger.Instance));
    }

    [Fact]
    public void Given_GarbageHeader_When_Parsing_Then_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => new DigestHeader("not-a-valid-header", NullLogger.Instance));
    }

    [Theory]
    [InlineData("Digest realm=\"r\", nonce=\"n\", qop=\"auth\"")]
    [InlineData("digest realm=\"r\", nonce=\"n\", qop=\"auth\"")]
    [InlineData("DIGEST realm=\"r\", nonce=\"n\", qop=\"auth\"")]
    public void Given_HeaderWithDifferentCasing_When_Parsing_Then_Succeeds(string header)
    {
        var digestHeader = new DigestHeader(header, NullLogger.Instance);

        digestHeader.Realm.ShouldBe("r");
        digestHeader.Nonce.ShouldBe("n");
        digestHeader.Qop.ShouldBe("auth");
    }

    [Fact]
    public void Given_HeaderWithDomain_When_Parsing_Then_CoreFieldsExtracted()
    {
        const string HEADER = "Digest realm=\"test\", nonce=\"abc\", qop=\"auth\", domain=\"/api\"";
        var digestHeader = new DigestHeader(HEADER, NullLogger.Instance);

        digestHeader.Realm.ShouldBe("test");
        digestHeader.Nonce.ShouldBe("abc");
        digestHeader.Qop.ShouldBe("auth");
    }

    [Fact]
    public void Given_HeaderWithLongNonce_When_Parsing_Then_NonceIsExtracted()
    {
        const string LONG_NONCE = "2021-12-21T13:40:54.513311Z+f152e55bf3d14e28b90f47db5dbd8afb+extra-data";
        const string HEADER = $"Digest realm=\"test\", nonce=\"{LONG_NONCE}\", qop=\"auth\"";
        var digestHeader = new DigestHeader(HEADER, NullLogger.Instance);

        digestHeader.Nonce.ShouldBe(LONG_NONCE);
    }

    [Fact]
    public void Given_HeaderWithMultipleQopValues_When_Parsing_Then_QopIsExtracted()
    {
        const string HEADER = "Digest realm=\"test-realm\", nonce=\"abc123\", qop=\"auth,auth-int\"";
        var digestHeader = new DigestHeader(HEADER, NullLogger.Instance);

        digestHeader.Qop.ShouldBe("auth,auth-int");
    }

    [Fact]
    public void Given_HeaderWithOpaque_When_Parsing_Then_OpaqueIsExtracted()
    {
        const string HEADER = "Digest realm=\"test-realm\", nonce=\"abc123\", qop=\"auth\", opaque=\"opaque-value\"";
        var digestHeader = new DigestHeader(HEADER, NullLogger.Instance);

        digestHeader.Opaque.ShouldBe("opaque-value");
        digestHeader.Realm.ShouldBe("test-realm");
        digestHeader.Nonce.ShouldBe("abc123");
        digestHeader.Qop.ShouldBe("auth");
    }

    [Fact]
    public void Given_HeaderWithoutOpaque_When_Parsing_Then_OpaqueIsNull()
    {
        const string HEADER = "Digest realm=\"test-realm\", nonce=\"abc123\", qop=\"auth\"";
        var digestHeader = new DigestHeader(HEADER, NullLogger.Instance);

        digestHeader.Opaque.ShouldBeNull();
    }

    [Fact]
    public void Given_HeaderWithSpecialCharsInRealm_When_Parsing_Then_RealmIsExtracted()
    {
        const string HEADER = "Digest realm=\"My App - Production (v2)\", nonce=\"abc123\", qop=\"auth\"";
        var digestHeader = new DigestHeader(HEADER, NullLogger.Instance);

        digestHeader.Realm.ShouldBe("My App - Production (v2)");
    }

    [Fact]
    public void Given_HeaderWithStale_When_Parsing_Then_CoreFieldsExtracted()
    {
        const string HEADER = "Digest realm=\"test\", nonce=\"abc\", qop=\"auth\", stale=\"false\"";
        var digestHeader = new DigestHeader(HEADER, NullLogger.Instance);

        digestHeader.Realm.ShouldBe("test");
        digestHeader.Nonce.ShouldBe("abc");
    }

    [Fact]
    public void Given_HeaderWithUnquotedQop_When_Parsing_Then_QopIsExtracted()
    {
        const string HEADER = "Digest realm=\"test-realm\", nonce=\"abc123\", qop=auth";
        var digestHeader = new DigestHeader(HEADER, NullLogger.Instance);

        digestHeader.Qop.ShouldBe("auth");
    }

    [Fact]
    public void Given_MissingNonce_When_Parsing_Then_ThrowsArgumentException()
    {
        const string HEADER = "Digest realm=\"test-realm\", qop=\"auth\"";

        Should.Throw<ArgumentException>(() => new DigestHeader(HEADER, NullLogger.Instance));
    }

    [Fact]
    public void Given_MissingQop_When_Parsing_Then_SucceedsBecauseQopIsOptionalPerRfc2069()
    {
        const string HEADER = "Digest realm=\"test-realm\", nonce=\"abc123\"";
        var digestHeader = new DigestHeader(HEADER, NullLogger.Instance);

        digestHeader.Realm.ShouldBe("test-realm");
        digestHeader.Nonce.ShouldBe("abc123");
        digestHeader.Qop.ShouldBeNull();
    }

    [Fact]
    public void Given_MissingRealm_When_Parsing_Then_ThrowsArgumentException()
    {
        const string HEADER = "Digest nonce=\"abc123\", qop=\"auth\"";

        Should.Throw<ArgumentException>(() => new DigestHeader(HEADER, NullLogger.Instance));
    }

    [Fact]
    public void Given_ValidHeader_When_CallingToString_Then_ReturnsFormattedString()
    {
        const string HEADER = "Digest realm=\"test-realm\", nonce=\"abc123\", qop=\"auth\", opaque=\"op123\"";
        var digestHeader = new DigestHeader(HEADER, NullLogger.Instance);

        var result = digestHeader.ToString();

        result.ShouldContain("Realm=\"test-realm\"");
        result.ShouldContain("Nonce=\"abc123\"");
        result.ShouldContain("Qop=\"auth\"");
        result.ShouldContain("Opaque=\"op123\"");
    }
}
