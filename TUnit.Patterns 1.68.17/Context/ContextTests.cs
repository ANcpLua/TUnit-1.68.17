namespace TUnit.Patterns.Context;

public sealed class ContextTests
{
    private const string Owner = "Owner";
    private const string Team = "platform";
    private const string ArtifactName = "context.txt";
    private const string ArtifactDisplayName = "Context dump";
    private const string TableBaseName = "todos";
    private const string Separator = "-";

    [Test]
    [Property(Owner, Team)]
    public async Task Custom_properties_are_readable_at_run_time()
    {
        var properties = TestContext.Current!.Metadata.TestDetails.CustomProperties;

        await Assert.That(properties[Owner]).Contains(Team);
    }

    [Test]
    public async Task Isolation_helpers_produce_unique_resource_names()
    {
        var isolation = TestContext.Current!.Isolation;

        await Assert.That(isolation.GetIsolatedName(TableBaseName)).IsEqualTo($"test_{isolation.UniqueId}_{TableBaseName}");
        await Assert.That(isolation.GetIsolatedPrefix(Separator)).IsEqualTo($"test{Separator}{isolation.UniqueId}{Separator}");
    }

    // Attached artifacts use the platform results directory, honouring --results-directory.
    [Test]
    public async Task Artifacts_land_in_the_results_directory()
    {
        var context = TestContext.Current!;
        var path = Path.Combine(TestContext.ResultsDirectory, context.Isolation.GetIsolatedName(ArtifactName));

        await File.WriteAllTextAsync(path, context.Metadata.DisplayName);
        context.Output.AttachArtifact(path, displayName: ArtifactDisplayName);

        await Assert.That(File.Exists(path)).IsTrue();
        await Assert.That(Path.IsPathRooted(TestContext.ResultsDirectory)).IsTrue();
    }
}
