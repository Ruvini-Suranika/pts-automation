namespace PTS.Automation.Infrastructure.Playwright;

/// <summary>
/// Playwright-NUnit launches the per-worker browser via its own <c>BrowserService</c>
/// which reads a fixed set of env vars (<c>HEADED</c>, <c>BROWSER</c>, <c>PWDEBUG</c>).
/// There is no supported override point on <c>PageTest</c> for browser launch options.
///
/// To keep <see cref="BrowserSettings"/> from <c>appsettings.json</c> meaningful we
/// translate its values into the env vars Playwright-NUnit understands, at the
/// earliest assembly-level hook (see <c>GlobalTestHooks.BeforeAllTests</c>), before
/// any test has forced the browser to launch.
///
/// This class is the single place that encodes that translation.
/// </summary>
public static class BrowserLaunchOptionsFactory
{
    public const string HeadedEnvVar  = "HEADED";
    public const string BrowserEnvVar = "BROWSER";

    /// <summary>
    /// Returns <c>true</c> if <c>HEADED</c> env var is set to a truthy value
    /// (1, true, on, yes), <c>false</c> if explicitly disabled (0, false, off, no),
    /// or <c>null</c> if the env var is absent (caller should fall back to appsettings).
    /// </summary>
    public static bool? HeadedFromEnv()
    {
        var v = System.Environment.GetEnvironmentVariable(HeadedEnvVar);
        if (string.IsNullOrWhiteSpace(v)) return null;
        v = v.Trim().ToLowerInvariant();
        return v switch
        {
            "1" or "true"  or "on"  or "yes" => true,
            "0" or "false" or "off" or "no"  => false,
            _ => null
        };
    }

    /// <summary>
    /// Makes <c>Browser.Headless</c> from <see cref="TestSettings"/> effective by
    /// setting <c>HEADED=1</c> when the config asks for headed mode and no
    /// env-var override has already been supplied. Env var always wins.
    /// </summary>
    public static void ApplyConfigToEnv(TestSettings s)
    {
        // Only bridge if the user hasn't already spoken via env var.
        if (HeadedFromEnv() is null && !s.Browser.Headless)
        {
            System.Environment.SetEnvironmentVariable(HeadedEnvVar, "1");
        }
    }

    /// <summary>
    /// The effective mode after env-var override vs appsettings.
    /// Used for the run banner.
    /// </summary>
    public static bool EffectiveHeaded(TestSettings s) =>
        HeadedFromEnv() ?? !s.Browser.Headless;
}
