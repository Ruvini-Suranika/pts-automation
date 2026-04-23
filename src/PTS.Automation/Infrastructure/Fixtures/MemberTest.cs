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

        await using var context = await Browser.NewContextAsync(ContextOptionsFactory.Build(Settings));
        var page = await context.NewPageAsync();
        var login = new LoginPage(page, Settings.Applications.Member);

        await login.GotoAsync();
        await login.LoginAsync(creds.Username, creds.Password);
        await login.WaitForPostLoginAsync();

        await context.StorageStateAsync(new BrowserContextStorageStateOptions { Path = statePath });
        AuthStateCache.MarkPrimed(Role);

        Log.For<MemberTest>().Information("Member auth state primed at {Path}", statePath);
    }
}
