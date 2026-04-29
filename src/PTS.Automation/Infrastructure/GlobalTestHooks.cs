using PTS.Automation.Infrastructure.Playwright;

namespace PTS.Automation;

/// <summary>
/// Assembly-level setup and teardown. An NUnit <see cref="SetUpFixture"/>'s
/// <c>[OneTimeSetUp]</c> / <c>[OneTimeTearDown]</c> applies to every test in
/// the SAME namespace AND all child namespaces. This file therefore lives in
/// the ROOT namespace (<c>PTS.Automation</c>) so its hooks cover every test
/// under <c>PTS.Automation.Features.*</c>.
///
/// Responsibilities:
///   1. Print a run banner (env, base URLs, headed/headless, worker count) so
///      failures can be reproduced without digging through CI logs.
///   2. Clear stale <c>.auth/</c> storage-state files from previous runs so
///      auth priming always happens fresh (no risk of expired session cookies).
///   3. Flush Serilog at the end so the last messages are not lost.
/// </summary>
[SetUpFixture]
public sealed class GlobalTestHooks
{
    [OneTimeSetUp]
    public void BeforeAllTests()
    {
        var s = ConfigFactory.Settings;

        // Bridge `Browser.Headless` from appsettings into the HEADED env var
        // BEFORE Playwright-NUnit's per-worker browser service initialises.
        // (PageTest has no BrowserOptions() override point — env vars are the API.)
        BrowserLaunchOptionsFactory.ApplyConfigToEnv(s);
        BrowserLaunchOptionsFactory.ApplyHeadedSlowMoToPlaywrightAdapter(s);

        ClearStaleAuthState(s);

        var headed = BrowserLaunchOptionsFactory.EffectiveHeaded(s);
        var browser = System.Environment.GetEnvironmentVariable("BROWSER") ?? s.Browser.Name;

        Log.Root.Information(
            "╔══════════════════ PTS.Automation run ══════════════════╗");
        Log.Root.Information("  Environment        : {Env}", ConfigFactory.CurrentEnvironment);
        Log.Root.Information("  Member base URL    : {Url}", s.Applications.Member.BaseUrl);
        Log.Root.Information("  Admin  base URL    : {Url}", s.Applications.Admin.BaseUrl);
        Log.Root.Information("  Browser            : {Browser}", browser);
        Log.Root.Information("  Mode               : {Mode}", headed ? "headed" : "headless");
        if (headed)
            Log.Root.Information("  SlowMo (headed)    : {Ms} ms", BrowserLaunchOptionsFactory.HeadedSlowMoMs);
        Log.Root.Information("  Viewport           : {W}x{H}", s.Browser.Viewport.Width, s.Browser.Viewport.Height);
        Log.Root.Information("  Trace retention    : {Trace}", s.Browser.Trace);
        Log.Root.Information("  Video retention    : {Video}", s.Browser.Video);
        Log.Root.Information("  Screenshot policy  : {Shot}", s.Browser.Screenshot);
        Log.Root.Information("  Artifacts root     : {Root}", Path.GetFullPath(s.Paths.ArtifactsRoot));
        Log.Root.Information("  Member creds set   : {Set}", s.Users.Member.IsConfigured);
        Log.Root.Information("  Admin  creds set   : {Set}", s.Users.PtsAdmin.IsConfigured);
        Log.Root.Information(
            "╚════════════════════════════════════════════════════════╝");
    }

    [OneTimeTearDown]
    public void AfterAllTests()
    {
        Log.Root.Information("===== Test run complete =====");
        Log.CloseAndFlush();
    }

    /// <summary>
    /// Removes any previously-cached auth storage state. Without this the first
    /// test of a role could silently reuse an expired cookie from an earlier
    /// run and produce confusing "intermittent" failures.
    /// </summary>
    private static void ClearStaleAuthState(TestSettings s)
    {
        if (!Directory.Exists(s.Paths.AuthStateDir)) return;

        foreach (var file in Directory.EnumerateFiles(s.Paths.AuthStateDir, "*.storage-state.json"))
        {
            try { File.Delete(file); }
            catch { /* next run will overwrite; don't fail run on cleanup */ }
        }
    }
}
