using Allure.NUnit;
using PTS.Automation.Infrastructure.Playwright;

namespace PTS.Automation.Infrastructure.Fixtures;

/// <summary>
/// Root test fixture. Inherits Playwright's <see cref="PageTest"/> which gives us
/// a browser-per-worker and a fresh <see cref="IBrowserContext"/> + <see cref="IPage"/>
/// per test. This class adds:
///   - Strongly-typed access to <see cref="TestSettings"/>
///   - Per-test tracing (retain on failure)
///   - Failure screenshot capture
///   - Per-test Serilog scope
///
/// Subclasses should override <see cref="ContextOptions"/> to inject auth state.
/// </summary>
[Parallelizable(ParallelScope.Fixtures)]
[AllureNUnit]
public abstract class BaseTest : PageTest
{
    protected TestSettings Settings => ConfigFactory.Settings;
    protected ILogger Logger { get; private set; } = Log.Root;

    public override BrowserNewContextOptions ContextOptions() =>
        ContextOptionsFactory.Build(Settings);

    [SetUp]
    public virtual async Task BaseSetUp()
    {
        Logger = Log.For(TestContext.CurrentContext.Test.ClassName ?? GetType().FullName!);
        Logger.Information("===== START {TestName} =====", TestContext.CurrentContext.Test.FullName);

        Context.SetDefaultTimeout(Settings.Timeouts.DefaultMs);
        Context.SetDefaultNavigationTimeout(Settings.Timeouts.NavigationMs);

        await TraceArtifactHook.StartTracingAsync(Context);
    }

    [TearDown]
    public virtual async Task BaseTearDown()
    {
        var failed = TestContext.CurrentContext.Result.Outcome.Status
                     == NUnit.Framework.Interfaces.TestStatus.Failed;

        try
        {
            await TraceArtifactHook.CaptureScreenshotOnFailureAsync(Page, Settings);
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "Failed to capture failure screenshot");
        }

        try
        {
            await TraceArtifactHook.StopTracingAsync(Context, Settings, failed);
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "Failed to stop tracing");
        }

        Logger.Information("===== END {TestName} — {Status} =====",
            TestContext.CurrentContext.Test.FullName,
            TestContext.CurrentContext.Result.Outcome.Status);
    }

    /// <summary>Shorthand for building an absolute URL from a path relative to the member/admin base.</summary>
    protected static Uri AbsoluteUrl(AppUrl app, string relativePath) =>
        new(app.BaseUri, relativePath.TrimStart('/'));

    /// <summary>
    /// Wraps an async sub-step in timing + structured logging. Mirrors the
    /// "step" concept in Allure / ReportPortal so tests read as a sequence of
    /// human-readable actions rather than a wall of Playwright calls.
    /// </summary>
    /// <example>
    /// <code>
    ///     await StepAsync("Open add-client modal", async () => {
    ///         await listPage.OpenAddClientAsync();
    ///     });
    /// </code>
    /// </example>
    protected async Task StepAsync(string name, Func<Task> action)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        Logger.Information("▶ STEP  {Step}", name);
        try
        {
            await action().ConfigureAwait(false);
            sw.Stop();
            Logger.Information("✓ STEP  {Step} ({Elapsed}ms)", name, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            Logger.Error(ex, "✗ STEP  {Step} failed after {Elapsed}ms", name, sw.ElapsedMilliseconds);
            throw;
        }
    }

    /// <summary>Typed variant of <see cref="StepAsync(string, Func{Task})"/>.</summary>
    protected async Task<T> StepAsync<T>(string name, Func<Task<T>> action)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        Logger.Information("▶ STEP  {Step}", name);
        try
        {
            var result = await action().ConfigureAwait(false);
            sw.Stop();
            Logger.Information("✓ STEP  {Step} ({Elapsed}ms)", name, sw.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            Logger.Error(ex, "✗ STEP  {Step} failed after {Elapsed}ms", name, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
