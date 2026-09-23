using System.Runtime.InteropServices;

namespace TUnit.Patterns.Extensions;

/// <summary>Conditional skip decided at registration time.</summary>
public sealed class SkipOnAttribute(string platform) : SkipAttribute(string.Format(null, ReasonFormat, platform))
{
    private const string ReasonFormat = "Not supported on {0}";

    public override Task<bool> ShouldSkip(TestRegisteredContext context) =>
        Task.FromResult(RuntimeInformation.IsOSPlatform(OSPlatform.Create(platform)));
}
