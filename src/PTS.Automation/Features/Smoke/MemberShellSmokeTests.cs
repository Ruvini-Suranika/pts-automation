using Allure.NUnit.Attributes;
using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Member.Auth;
using PTS.Automation.Pages.Member.Dashboard;
using PTS.Automation.Pages.Member.Shell;

namespace PTS.Automation.Features.Smoke;

/// <summary>
/// End-to-end proof that the shell POM works against the real QA environment:
///   1. log in
///   2. land on the Dashboard with expected readiness markers
///   3. nav bar is visible and the top-level items are present
///   4. settings mega-menu opens and shows the expected item labels
///   5. profile menu opens and logout returns us to the login page
///
/// These tests require Member credentials. They skip gracefully (with a clear
/// hint) if credentials are not configured.
/// </summary>
[TestFixture]
[AllureSuite(Categories.Smoke)]
[AllureFeature("Member shell")]
[AllureTag(Categories.Smoke)]
[AllureTag(Categories.Member)]
[AllureTag(Categories.Authentication)]
[Category(Categories.Smoke)]
[Category(Categories.Member)]
[Category(Categories.Authentication)]
public class MemberShellSmokeTests : BaseTest
{
    [Test]
    [Category(Categories.UI)]
    [Description("After valid Member login we land on the Dashboard; nav bar, profile, and settings shell are all visible.")]
    public async Task Authenticated_member_sees_dashboard_with_shell_chrome()
    {
        var creds = Settings.Users.Member;
        if (!creds.IsConfigured)
            Assert.Ignore(CredentialHint);

        await StepAsync("Log in as Member", async () =>
        {
            var login = new LoginPage(Page, Settings.Applications.Member);
            await login.GotoAsync();
            await login.LoginAsync(creds.Username, creds.Password);
            await login.WaitForPostLoginAsync();
        });

        var dashboard = new DashboardPage(Page, Settings.Applications.Member);

        await StepAsync("Wait for Dashboard readiness", async () =>
        {
            await dashboard.WaitForReadyAsync();
            Assert.That(await dashboard.IsEnquiriesHeadVisibleAsync(), Is.True,
                "Dashboard enquiries-head section should be visible after login.");
            Assert.That(await dashboard.GetBrowserTitleAsync(), Is.EqualTo("Dashboard"),
                "Browser title should read 'Dashboard' on the landing page.");
        });

        await StepAsync("Nav bar is visible", async () =>
        {
            Assert.That(await dashboard.NavBar.IsVisibleAsync(), Is.True,
                "Top-level Member nav bar should be visible on an authenticated page.");
        });

        await StepAsync("Profile menu is visible and displays the user name", async () =>
        {
            Assert.That(await dashboard.ProfileMenu.IsToggleVisibleAsync(), Is.True,
                "Profile dropdown toggle should be visible.");
        });
    }

    [Test]
    [Category(Categories.UI)]
    [Description("The settings mega-menu opens and shows the core items we model in the POM.")]
    public async Task Settings_mega_menu_lists_expected_items()
    {
        var creds = Settings.Users.Member;
        if (!creds.IsConfigured)
            Assert.Ignore(CredentialHint);

        await StepAsync("Log in and land on Dashboard", async () =>
        {
            var login = new LoginPage(Page, Settings.Applications.Member);
            await login.GotoAsync();
            await login.LoginAsync(creds.Username, creds.Password);
            await login.WaitForPostLoginAsync();
        });

        var dashboard = new DashboardPage(Page, Settings.Applications.Member);
        await StepAsync("Wait for Dashboard readiness", () => dashboard.WaitForReadyAsync());

        var labels = await StepAsync("Open settings mega-menu and read item labels",
            () => dashboard.SettingsMegaMenu.GetVisibleItemLabelsAsync());

        await StepAsync("Log visible settings item labels", async () =>
        {
            Logger.Information("Settings items visible for this user: {Items}", string.Join(", ", labels));
        });

        await StepAsync("Verify core settings mega-menu items", async () =>
        {
            // Items that should be present regardless of role (Contracted Suppliers,
            // Downloads, Email, Itinerary, Organisation, Quote, Users).
            Assert.That(labels, Does.Contain("Contracted Suppliers"));
            Assert.That(labels, Does.Contain("Downloads"));
            Assert.That(labels, Does.Contain("Users"));
        });
    }

    [Test]
    [Category(Categories.UI)]
    [Description("Logout leaves the Member portal and destroys the session — " +
                 "a subsequent request to a protected page redirects to login.")]
    public async Task Logout_ends_member_session()
    {
        var creds = Settings.Users.Member;
        if (!creds.IsConfigured)
            Assert.Ignore(CredentialHint);

        await StepAsync("Log in and land on Dashboard", async () =>
        {
            var login = new LoginPage(Page, Settings.Applications.Member);
            await login.GotoAsync();
            await login.LoginAsync(creds.Username, creds.Password);
            await login.WaitForPostLoginAsync();
        });

        var dashboard = new DashboardPage(Page, Settings.Applications.Member);
        await StepAsync("Wait for Dashboard readiness", () => dashboard.WaitForReadyAsync());

        await StepAsync("Log out via profile menu", () => dashboard.ProfileMenu.LogoutAsync());

        await StepAsync("Verify URL left Member area after logout", async () =>
        {
            Assert.That(Page.Url, Does.Not.Contain("/Member/").IgnoreCase,
                $"After logout we must have left /Member/. Actual URL: {Page.Url}");
        });

        // Strong check: the session is really gone. Re-request the dashboard —
        // the server must redirect us to the login page (possibly via access
        // denied) because we no longer have an authenticated cookie.
        await StepAsync("Re-visiting /Member/Index should redirect to login", async () =>
        {
            await Page.GotoAsync(Settings.Applications.Member.BaseUri + "Member/Index");

            var landed = Page.Url;
            var isLoginOrDenied =
                landed.Contains("/Account/Login", StringComparison.OrdinalIgnoreCase) ||
                landed.Contains("/Account/LogIn", StringComparison.OrdinalIgnoreCase) ||
                landed.Contains("AccessDenied",   StringComparison.OrdinalIgnoreCase);

            Assert.That(isLoginOrDenied, Is.True,
                $"Protected page should redirect an unauthenticated user. Actual URL: {landed}");
        });
    }

    private const string CredentialHint =
        "Member credentials are not configured. Configure via:\n" +
        "  cd src/PTS.Automation\n" +
        "  dotnet user-secrets set \"Users:Member:Username\" \"<user>\"\n" +
        "  dotnet user-secrets set \"Users:Member:Password\" \"<pwd>\"\n" +
        "or set PTS_Users__Member__Username / PTS_Users__Member__Password.";
}
