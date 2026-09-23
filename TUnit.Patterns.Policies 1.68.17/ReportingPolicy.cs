using TUnit.Core.Settings;

namespace TUnit.Patterns.Policies;

/// <summary>
/// context.Settings.Reporting switches the built-in reports from code, next to the other TUnitSettings.
/// Precedence: TUNIT_DISABLE_HTML_REPORTER / TUNIT_DISABLE_JSON_REPORT / TUNIT_DISABLE_ARTIFACT_UPLOAD → settings → default.
/// This project keeps the HTML report but drops the JSON sidecar (only report aggregation reads it) and never uploads
/// the report as a CI artifact. After a run, TestResults/ holds "{assembly}-report.html" and no ".tunit-report.json";
/// a sidecar left over from an earlier run is deleted.
/// </summary>
public static class ReportingPolicy
{
    public static ReportingSettings? Applied { get; private set; }

    [Before(TestDiscovery)]
    public static void Configure(BeforeTestDiscoveryContext context)
    {
        context.Settings.Reporting.HtmlReportEnabled = true;
        context.Settings.Reporting.JsonReportEnabled = false;
        context.Settings.Reporting.ArtifactUploadEnabled = false;

        Applied = context.Settings.Reporting;
    }
}

public sealed class ReportingPolicyTests
{
    [Test]
    public async Task Discovery_hook_configured_the_reports()
    {
        var reporting = ReportingPolicy.Applied!;

        await Assert.That(reporting.HtmlReportEnabled).IsTrue();
        await Assert.That(reporting.JsonReportEnabled).IsFalse();
        await Assert.That(reporting.ArtifactUploadEnabled).IsFalse();
    }
}
