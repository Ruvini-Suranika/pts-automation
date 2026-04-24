using PTS.Automation.Infrastructure.Playwright;
using PTS.Automation.Pages.Member.Auth;

namespace PTS.Automation.Infrastructure.Fixtures;

/// <summary>
/// Base fixture for tests that require an authenticated Member user.
/// The first test of the fixture performs a real UI login and persists
/// Playwright's <c>storageState</c> to disk; subsequent tests reuse it.
/// </summary>
public abstract class MemberTest : BaseTest
{
    private const string Role = "member";

    public override BrowserNewContextOptions ContextOptions()
    {
        var statePath = AuthStateCache.PathFor(Settings, Role);
        return ContextOptionsFactory.Build(Settings,
            storageStatePath: AuthStateCache.IsPrimed(Role) ? statePath : null);
    }

    /// <summary>
    /// Auto-skip tests that require an authenticated Member when credentials
    /// are not configured. Runs after <see cref="BaseTest.BaseSetUp"/> so tracing
    /// and logging are still initialised for the ignore result.
    /// </summary>
    [SetUp]
    public void SkipIfMemberNotAuthenticated()
    {
        if (!AuthStateCache.IsPrimed(Role) && !Settings.Users.Member.IsConfigured)
        {
            Assert.Ignore(
                "Member credentials are not configured. Configure via:\n" +
                "  cd src/PTS.Automation\n" +
                "  dotnet user-secrets set \"Users:Member:Username\" \"<user>\"\n" +
                "  dotnet user-secrets set \"Users:Member:Password\" \"<pwd>\"\n" +
                "or set PTS_Users__Member__Username / PTS_Users__Member__Password.");
        }
    }

    [OneTimeSetUp]
    public async Task PrimeMemberAuthState()
    {
        // Fast-path: primed in memory already — no lock needed.
        if (AuthStateCache.IsPrimed(Role)) return;

        // Slow-path: serialise the entire prime operation so parallel
        // fixtures (e.g. TransactionsTests + BookingListTests racing in
        // separate NUnit workers) don't both try to write the same
        // storage-state file. The semaphore lives for the whole prime-flow,
        // and we re-check IsPrimed inside it so only the first waiter
        // actually performs the login.
        await AuthStateCache.PrimeLock.WaitAsync();
        try
        {
            if (AuthStateCache.IsPrimed(Role)) return;

            var statePath = AuthStateCache.PathFor(Settings, Role);
            if (File.Exists(statePath))
            {
                AuthStateCache.MarkPrimed(Role);
                return;
            }

            var creds = Settings.Users.Member;
            if (!creds.IsConfigured)
            {
                // No credentials configured — leave state un-primed. Tests that need
                // authentication will fail with a clear message; tests that only
                // exercise the login page (smoke) can still run.
                Log.For<MemberTest>().Warning(
                    "Member credentials not configured. Set via `dotnet user-secrets set \"Users:Member:Username\" <value>` " +
                    "and `dotnet user-secrets set \"Users:Member:Password\" <value>`, or export PTS_Users__Member__Username/Password.");
                return;
            }

            // NOTE: we own the Playwright / Browser / Context lifecycle here rather
            // than reusing PageTest.Browser. Playwright-NUnit's per-worker Browser
            // property isn't reliably initialised at the moment this [OneTimeSetUp]
            // runs, so we stand up a throwaway instance to do the UI login and
            // persist storage state to disk. Every subsequent test then picks up
            // the cached state via ContextOptionsFactory.Build(..., storageStatePath).
            using var pw = await Microsoft.Playwright.Playwright.CreateAsync();
            var browserType = ResolveBrowserType(pw);
            await using var browser = await browserType.LaunchAsync(
                BrowserLaunchOptionsFactory.Build(Settings));

            await using var context = await browser.NewContextAsync(ContextOptionsFactory.Build(Settings));
            var page = await context.NewPageAsync();
            var login = new LoginPage(page, Settings.Applications.Member);

            await login.GotoAsync();
            await login.LoginAsync(creds.Username, creds.Password);
            await login.WaitForPostLoginAsync();

            await context.StorageStateAsync(new BrowserContextStorageStateOptions { Path = statePath });
            AuthStateCache.MarkPrimed(Role);

            Log.For<MemberTest>().Information("Member auth state primed at {Path}", statePath);
        }
        finally
        {
            AuthStateCache.PrimeLock.Release();
        }
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
