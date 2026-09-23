namespace TUnit.Patterns.Context;

/// <summary>Session-level artifact attached once for the whole run, next to per-test artifacts in the results directory.</summary>
public static class SessionArtifacts
{
    public const string FileName = "session-info.txt";
    public const string DisplayName = "Session info";
    private const string Description = "Environment captured before the first test";

    [Before(TestSession)]
    public static async Task AttachSessionInfo()
    {
        var path = Path.Combine(TestContext.ResultsDirectory, FileName);

        await File.WriteAllTextAsync(path, Environment.Version.ToString());
        TestSessionContext.Current!.AddArtifact(new Artifact
        {
            File = new FileInfo(path),
            DisplayName = DisplayName,
            Description = Description,
        });
    }
}

public sealed class TestBuilderContextTests
{
    private const string GeneratedAtKey = "GeneratedAt";

    // Runs during discovery; the StateBag is copied forward into the TestContext of every test it produces.
    public static IEnumerable<int> Cases()
    {
        TestBuilderContext.Current!.StateBag[GeneratedAtKey] = TestBuilderContext.Current.TestMetadata.Name;
        yield return 1;
    }

    [Test]
    [MethodDataSource(nameof(Cases))]
    public async Task State_flows_from_discovery_to_execution(int @case)
    {
        await Assert.That(@case).IsEqualTo(1);
        await Assert.That(TestContext.Current!.StateBag[GeneratedAtKey]).IsEqualTo(nameof(State_flows_from_discovery_to_execution));
    }
}
