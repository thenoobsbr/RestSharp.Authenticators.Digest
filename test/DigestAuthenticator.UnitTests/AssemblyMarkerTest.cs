using System.Reflection;

using Shouldly;

using Xunit;

namespace RestSharp.Authenticators.Digest.UnitTests;

[Trait("Category", "UnitTest")]
[Trait("Class", nameof(AssemblyMarker))]
public class AssemblyMarkerTest
{
    [Fact]
    public void Given_AssemblyMarker_When_AccessingSelf_Then_ReturnsNonNullAssembly()
    {
        var assembly = AssemblyMarker.Self;

        assembly.ShouldNotBeNull();
        assembly.ShouldBeAssignableTo<Assembly>();
    }

    [Fact]
    public void Given_AssemblyMarker_When_AccessingSelf_Then_AssemblyNameContainsDigest()
    {
        var assembly = AssemblyMarker.Self;

        assembly.GetName().Name.ShouldBe("RestSharp.Authenticators.Digest");
    }

    [Fact]
    public void Given_AssemblyMarker_When_AccessingVersion_Then_ReturnsNonNullVersion()
    {
        var version = AssemblyMarker.Self.GetName().Version;

        version.ShouldNotBeNull();
    }
}
