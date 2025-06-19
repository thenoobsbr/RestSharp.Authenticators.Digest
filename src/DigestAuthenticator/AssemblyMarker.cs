using System.Reflection;

namespace RestSharp.Authenticators.Digest;

internal static class AssemblyMarker
{
    public static Assembly Self => typeof(AssemblyMarker).Assembly;
}
