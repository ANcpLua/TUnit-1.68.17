using TUnit.Playwright;

namespace TUnit.Playwright_1._68._17;

/// <summary>
/// [RecordVideo] records the browser context of one test, renames Playwright's page@hash.webm
/// after the test and attaches it to the test result as an artifact.
/// </summary>
public class VideoRecordingTests : PageTest
{
    public const string VideoDirectory = "playwright-videos";
    public const string ArtifactDescription = "Playwright video recording";
    private const int Width = 640;
    private const int Height = 360;

    [Test]
    [RecordVideo(VideoDirectory, Width, Height)]
    public async Task Recorded_page_uses_the_recording_viewport()
    {
        await Page.SetContentAsync("<h1>recorded</h1>");

        await Assert.That(Page.Video).IsNotNull();
        await Assert.That(Page.ViewportSize!.Width).IsEqualTo(Width);
        await Assert.That(Page.ViewportSize.Height).IsEqualTo(Height);
    }

    // The video is finalised after teardown, so only a later test can see the renamed file and the artifact.
    // An existing file is never overwritten: a second run of the same test in the same directory gets "-2", "-3", ...
    [Test]
    [DependsOn(nameof(Recorded_page_uses_the_recording_viewport))]
    public async Task Recording_is_named_after_the_test_and_attached()
    {
        var recorded = TestContext.Current!.Dependencies.GetTests(nameof(Recorded_page_uses_the_recording_viewport)).Single();
        var video = recorded.Output.Artifacts.Single();

        await Assert.That(video.File.Name).Matches($@"^{nameof(Recorded_page_uses_the_recording_viewport)}(-\d+)?\.webm$");
        await Assert.That(video.File.Directory!.Name).IsEqualTo(VideoDirectory);
        await Assert.That(video.File.Exists).IsTrue();
        await Assert.That(video.Description).IsEqualTo(ArtifactDescription);
    }

    [Test]
    public async Task Tests_without_the_attribute_do_not_record()
    {
        await Assert.That(Page.Video).IsNull();
    }
}

/// <summary>The same attribute on a per-test PageFixture (composition instead of inheritance).</summary>
public class FixtureVideoRecordingTests
{
    [ClassDataSource<PageFixture>] public required PageFixture BrowserPage { get; init; }

    [Test]
    [RecordVideo(VideoRecordingTests.VideoDirectory)]
    public async Task Fixture_page_is_recorded_at_the_default_size()
    {
        await BrowserPage.Page.SetContentAsync("<h1>recorded through a fixture</h1>");

        await Assert.That(BrowserPage.Page.Video).IsNotNull();
        await Assert.That(BrowserPage.Page.ViewportSize!.Width).IsEqualTo(1280);
        await Assert.That(BrowserPage.Page.ViewportSize.Height).IsEqualTo(1400);
    }
}
