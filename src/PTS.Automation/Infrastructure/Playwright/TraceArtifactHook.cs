namespace PTS.Automation.Infrastructure.Playwright;

/// <summary>
/// Helpers for capturing diagnostic artifacts (traces, screenshots) when a test fails
/// and attaching them to the NUnit <see cref="TestContext"/> so they appear in test
/// reports and CI artifacts.
/// </summary>
public static class TraceArtifactHook
{
    public static async Task StartTracingAsync(IBrowserContext context)
    {
        await context.Tracing.StartAsync(new TracingStartOptions
        {
            Screenshots = true,
            Snapshots   = true,
            Sources     = true,
            Title       = TestContext.CurrentContext.Test.FullName
        });
    }

    public static async Task StopTracingAsync(IBrowserContext context, TestSettings settings, bool testFailed)
    {
        var retain = string.Equals(settings.Browser.Trace, "on", StringComparison.OrdinalIgnoreCase)
                  || (string.Equals(settings.Browser.Trace, "retain-on-failure", StringComparison.OrdinalIgnoreCase) && testFailed);

        if (!retain)
        {
            await context.Tracing.StopAsync();
            return;
        }

        Directory.CreateDirectory(settings.Paths.TraceDir);
        var safeName = SafeName(TestContext.CurrentContext.Test.FullName);
        var tracePath = Path.Combine(settings.Paths.TraceDir, $"{safeName}.zip");

        await context.Tracing.StopAsync(new TracingStopOptions { Path = tracePath });
        TestContext.AddTestAttachment(tracePath, "Playwright trace (open with: playwright show-trace)");
    }

    public static async Task CaptureScreenshotOnFailureAsync(IPage page, TestSettings settings)
    {
        if (TestContext.CurrentContext.Result.Outcome.Status != NUnit.Framework.Interfaces.TestStatus.Failed)
            return;

        Directory.CreateDirectory(settings.Paths.ScreenshotDir);
        var safeName = SafeName(TestContext.CurrentContext.Test.FullName);
        var path = Path.Combine(settings.Paths.ScreenshotDir, $"{safeName}.png");

        await page.ScreenshotAsync(new PageScreenshotOptions { Path = path, FullPage = true });
        TestContext.AddTestAttachment(path, "Failure screenshot");
    }

    private static string SafeName(string input)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            input = input.Replace(c, '_');
        return input.Length > 150 ? input[..150] : input;
    }
}
