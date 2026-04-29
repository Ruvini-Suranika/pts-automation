using System.Globalization;

using Allure.Net.Commons;
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
[TestFixture]
[AllureNUnit]
public abstract class BaseTest : PageTest
{
    private const int MaxStepResultLength = 800;

    private readonly List<(string Step, long ElapsedMs, string? ResultDetail)> _instrumentedSteps = [];
    private readonly List<(string Label, string Value)> _registeredTestData = [];

    protected TestSettings Settings => ConfigFactory.Settings;
    protected ILogger Logger { get; private set; } = Log.Root;

    /// <summary>
    /// When <c>true</c>, each <see cref="StepAsync"/> completion logs a structured result line and
    /// <see cref="BaseTearDown"/> emits a per-test summary. Enabled on <see cref="MemberTest"/> and <see cref="AdminTest"/>.
    /// </summary>
    protected virtual bool LogStepResultDetailAndTestSummary => false;

    public override BrowserNewContextOptions ContextOptions() =>
        ContextOptionsFactory.Build(Settings);

    [SetUp]
    public virtual async Task BaseSetUp()
    {
        Logger = Log.For(TestContext.CurrentContext.Test.ClassName ?? GetType().FullName!);
        Logger.Information("===== START {TestName} =====", TestContext.CurrentContext.Test.FullName);

        if (LogStepResultDetailAndTestSummary)
        {
            _instrumentedSteps.Clear();
            _registeredTestData.Clear();
        }

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

        if (LogStepResultDetailAndTestSummary)
            LogTestRunSummary();

        Logger.Information("===== END {TestName} — {Status} =====",
            TestContext.CurrentContext.Test.FullName,
            TestContext.CurrentContext.Result.Outcome.Status);
    }

    /// <summary>
    /// Register a value for the end-of-test summary (booking refs, client ids, amounts, etc.).
    /// No-op when <see cref="LogStepResultDetailAndTestSummary"/> is <c>false</c>.
    /// </summary>
    protected void RegisterTestData(string label, object? value)
    {
        if (!LogStepResultDetailAndTestSummary) return;
        _registeredTestData.Add((label, FormatStepResult(value)));
    }

    private void LogTestRunSummary()
    {
        var test = TestContext.CurrentContext.Test;
        var result = TestContext.CurrentContext.Result;
        var status = result.Outcome.Status;

        Logger.Information("┌── TEST SUMMARY ─────────────────────────────────────────");
        Logger.Information("│ Test name: {TestName}", test.FullName);
        Logger.Information("│ Total steps executed: {Count}", _instrumentedSteps.Count);
        for (var i = 0; i < _instrumentedSteps.Count; i++)
        {
            var (step, ms, detail) = _instrumentedSteps[i];
            Logger.Information(
                "│   Step {Index}: {Step} ({Elapsed} ms){Detail}",
                i + 1,
                step,
                ms,
                string.IsNullOrEmpty(detail) ? "" : " → " + detail);
        }

        if (_registeredTestData.Count > 0)
        {
            Logger.Information("│ Data values used:");
            foreach (var (label, value) in _registeredTestData)
                Logger.Information("│   • {Label}: {Value}", label, value);
        }
        else
            Logger.Information("│ Data values used: (none — call RegisterTestData from tests when useful)");

        Logger.Information("│ Final outcome: {Status}", status);
        if (!string.IsNullOrWhiteSpace(result.Message))
            Logger.Information("│ Outcome detail: {Message}", Truncate(result.Message.Trim(), 2000));

        if (result.FailCount > 0)
            Logger.Information("│ Assertions failed: {FailCount}", result.FailCount);
        if (result.PassCount > 0)
            Logger.Information("│ Assertions passed: {PassCount}", result.PassCount);

        Logger.Information("└──────────────────────────────────────────────────────────");
    }

    private static string Truncate(string value, int maxLen)
    {
        if (value.Length <= maxLen) return value;
        return value[..maxLen] + "…";
    }

    private static string FormatStepResult(object? value)
    {
        if (value is null) return "(null)";

        switch (value)
        {
            case string s:
                return Truncate(s, MaxStepResultLength);
            case IResponse resp:
                return $"IResponse HTTP {(int)resp.Status} {Truncate(resp.Url, 400)}";
            case Uri u:
                return u.ToString();
            case bool b:
                return b ? "true" : "false";
            case IFormattable fmt when value is not IConvertible:
                return fmt.ToString(null, CultureInfo.InvariantCulture) ?? string.Empty;
            case byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal:
                return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
        }

        if (value is System.Collections.IEnumerable enumerable and not string and not byte[])
        {
            var parts = new List<string>();
            foreach (var item in enumerable)
            {
                parts.Add(item?.ToString() ?? "(null)");
                if (parts.Count >= 12)
                {
                    parts.Add("…");
                    break;
                }
            }

            return Truncate(string.Join(", ", parts), MaxStepResultLength);
        }

        var t = value.ToString() ?? string.Empty;
        return Truncate(t, MaxStepResultLength);
    }

    private void RecordStepResult(string name, long elapsedMs, object? result)
    {
        if (!LogStepResultDetailAndTestSummary) return;
        var detail = result is null ? null : FormatStepResult(result);
        _instrumentedSteps.Add((name, elapsedMs, detail));
        Logger.Information(
            "◆ STEP RESULT {Step} ({Elapsed} ms): {Detail}",
            name,
            elapsedMs,
            detail ?? "(completed, no return value)");
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
            await AllureApi.Step(name, () => action()).ConfigureAwait(false);
            sw.Stop();
            Logger.Information("✓ STEP  {Step} ({Elapsed}ms)", name, sw.ElapsedMilliseconds);
            RecordStepResult(name, sw.ElapsedMilliseconds, null);
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
            var result = await AllureApi.Step(name, () => action()).ConfigureAwait(false);
            sw.Stop();
            Logger.Information("✓ STEP  {Step} ({Elapsed}ms)", name, sw.ElapsedMilliseconds);
            RecordStepResult(name, sw.ElapsedMilliseconds, result);
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
