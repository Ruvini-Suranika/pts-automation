namespace PTS.Automation.Infrastructure.Config;

public sealed class BrowserSettings
{
    public string Name { get; set; } = "chromium";

    /// <summary>
    /// When <c>true</c>, the browser runs without a visible window.
    /// Bridged to the <c>HEADED</c> env var by <c>GlobalTestHooks</c> before
    /// Playwright-NUnit initialises its per-worker browser service. The env
    /// var always wins over this value.
    /// </summary>
    public bool Headless { get; set; } = true;

    public ViewportSize Viewport { get; set; } = new();

    /// <summary>
    /// Video recording policy. Values: <c>on</c>, <c>retain-on-failure</c>, <c>off</c>.
    /// Applied via <c>ContextOptionsFactory</c> at per-test context creation.
    /// </summary>
    public string Video { get; set; } = "retain-on-failure";

    /// <summary>
    /// Trace retention policy. Values: <c>on</c>, <c>retain-on-failure</c>, <c>off</c>.
    /// Applied via <c>TraceArtifactHook</c> in <c>BaseTest</c>.
    /// </summary>
    public string Trace { get; set; } = "retain-on-failure";

    /// <summary>
    /// Screenshot policy. Values: <c>only-on-failure</c>, <c>on</c>, <c>off</c>.
    /// Applied via <c>TraceArtifactHook</c> in <c>BaseTest</c>.
    /// </summary>
    public string Screenshot { get; set; } = "only-on-failure";

    public sealed class ViewportSize
    {
        public int Width { get; set; } = 1440;
        public int Height { get; set; } = 900;
    }
}
