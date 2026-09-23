using System.Globalization;
using TUnit.Core.Interfaces;

namespace TUnit.Patterns.Extensions;

/// <summary>ITestExecutor: wraps the test body in an ambient scope and restores it afterwards, whatever the outcome.</summary>
public sealed class ScopedCultureExecutor : ITestExecutor
{
    public const string CultureName = "de-AT";

    public async ValueTask ExecuteTest(TestContext context, Func<ValueTask> action)
    {
        var original = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(CultureName);

        try
        {
            await action();
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }
}
