using PTS.Automation.Infrastructure.Playwright;
using PTS.Automation.Pages.Admin.Auth;

namespace PTS.Automation.Infrastructure.Fixtures;

/// <summary>
/// Base fixture for tests that require an authenticated PTS Admin user.
/// See <see cref="MemberTest"/> for the auth-state caching mechanism.
/// </summary>
public abstract class AdminTest : BaseTest
{
    private const string Role = "admin";

    public override BrowserNewContextOptions ContextOptions()
    {
        var statePath = AuthStateCache.PathFor(Settings, Role);
        return ContextOptionsFactory.Build(Settings,
            storageStatePath: AuthStateCache.IsPrimed(Role) ? statePath : null);
    }

    /// <summary>
    /// Auto-skip tests that require an authenticated Admin when credentials
    /// are not configured. Runs after <see cref="BaseTest.BaseSetUp"/> so tracing
    /// and logging are still initialised for the ignore result.
    /// </summary>
    [SetUp]
    public void SkipIfAdminNotAuthenticated()
    {
        if (!AuthStateCache.IsPrimed(Role) && !Settings.Users.PtsAdmin.IsConfigured)
        {
            Assert.Ignore(
                "PtsAdmin credentials are not configured. Configure via:\n" +
                "  cd src/PTS.Automation\n" +
                "  dotnet user-secrets set \"Users:PtsAdmin:Username\" \"<user>\"\n" +
                "  dotnet user-secrets set \"Users:PtsAdmin:Password\" \"<pwd>\"\n" +
                "or set PTS_Users__PtsAdmin__Username / PTS_Users__PtsAdmin__Password.");
        }
    }

    [OneTimeSetUp]
    public async Task PrimeAdminAuthState()
    {
        if (AuthStateCache.IsPrimed(Role)) return;

        var statePath = AuthStateCache.PathFor(Settings, Role);
        if (File.Exists(statePath))
        {
            AuthStateCache.MarkPrimed(Role);
            return;
        }

        var creds = Settings.Users.PtsAdmin;
        if (!creds.IsConfigured)
        {
            Log.For<AdminTest>().Warning(
                "PtsAdmin credentials not configured. Set via `dotnet user-secrets set \"Users:PtsAdmin:Username\" <value>` " +
                "and `dotnet user-secrets set \"Users:PtsAdmin:Password\" <value>`, or export PTS_Users__PtsAdmin__Username/Password.");
            return;
        }

        // See MemberTest.PrimeMemberAuthState for why we own the browser
        // lifecycle here instead of reusing PageTest.Browser.
        using var pw = await Microsoft.Playwright.Playwright.CreateAsync();
        var browserType = ResolveBrowserType(pw);
        await using var browser = await browserType.LaunchAsync(
            BrowserLaunchOptionsFactory.Build(Settings));

        await using var context = await browser.NewContextAsync(ContextOptionsFactory.Build(Settings));
        var page = await context.NewPageAsync();
        var login = new AdminLoginPage(page, Settings.Applications.Admin);

        await login.GotoAsync();
        await login.LoginAsync(creds.Username, creds.Password);
        await login.WaitForPostLoginAsync();

        await context.StorageStateAsync(new BrowserContextStorageStateOptions { Path = statePath });
        AuthStateCache.MarkPrimed(Role);

        Log.For<AdminTest>().Information("Admin auth state primed at {Path}", statePath);
    }

    private static IBrowserType ResolveBrowserType(IPlaywright pw)
    {
        var name = System.Environment.GetEnvironmentVariable("BROWSER")
                   ?? ConfigFactory.Settings.Browser.Name
                   ?? "chromium";

        return name.ToLowerInvariant() switch
        {
            "firefox" => pw.Firefox,
            "webkit"  => pw.Webkit,
            _         => pw.Chromium
        };
    }
}
