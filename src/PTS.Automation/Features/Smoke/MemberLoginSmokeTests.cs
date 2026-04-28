using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Member.Auth;

namespace PTS.Automation.Features.Smoke;

/// <summary>
/// End-to-end smoke proving the whole framework plumbing works:
///   config → Playwright → fixture → POM → assertions → artifacts.
///
/// The first two tests require ZERO credentials — they validate the login
/// page itself renders correctly on the target environment. The third test
/// actually logs in and requires Member credentials to be configured via
/// user-secrets or the PTS_Users__Member__* environment variables.
/// </summary>
[TestFixture]
[Category(Categories.Smoke)]
[Category(Categories.Member)]
[Category(Categories.Authentication)]
public class MemberLoginSmokeTests : BaseTest
{
    [Test]
    [Category(Categories.UI)]
    [Description("Member login page loads at the configured base URL and the login form is interactive.")]
    public async Task Login_page_loads_with_expected_form()
    {
        var login = new LoginPage(Page, Settings.Applications.Member);
        await login.GotoAsync();

        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(@"/Account/Login", System.Text.RegularExpressions.RegexOptions.IgnoreCase));
        await Expect(Page.Locator("#Username")).ToBeVisibleAsync();
        await Expect(Page.Locator("#Password")).ToBeVisibleAsync();
        await Expect(Page.Locator("#loginBtn")).ToBeEnabledAsync();

        var heading = await login.GetHeadingTextAsync();
        Assert.That(heading, Does.Match("Welcome|Sign in"),
            $"Unexpected login heading '{heading}'");
    }

    [Test]
    [Category(Categories.UI)]
    [Description("Forgot-password link is present (indicates layout integrity even when creds unavailable).")]
    public async Task Login_page_shows_forgot_password_link()
    {
        var login = new LoginPage(Page, Settings.Applications.Member);
        await login.GotoAsync();

        Assert.That(await login.IsForgotPasswordLinkVisibleAsync(), Is.True,
            "Forgot password link must be visible on the login page.");
    }

    [Test]
    [Category(Categories.UI)]
    [Description("Valid Member credentials log in successfully and land outside the login endpoint.")]
    public async Task Member_logs_in_with_valid_credentials()
    {
        var creds = Settings.Users.Member;
        if (!creds.IsConfigured)
        {
            Assert.Ignore(
                "Member credentials are not configured. Configure via:\n" +
                "  dotnet user-secrets set \"Users:Member:Username\" \"<user>\"\n" +
                "  dotnet user-secrets set \"Users:Member:Password\" \"<pwd>\"\n" +
                "or environment variables PTS_Users__Member__Username / PTS_Users__Member__Password.");
        }

        var login = new LoginPage(Page, Settings.Applications.Member);
        await login.GotoAsync();

        Logger.Information("Attempting Member login as '{User}' against {Base}",
            creds.Username, Settings.Applications.Member.BaseUrl);

        await login.LoginAsync(creds.Username, creds.Password);
        await login.WaitForPostLoginAsync();

        // Diagnostic: record where the member actually lands. Useful for planning
        // the next slices (knowing the real member dashboard / landing URL).
        Logger.Information("Post-login URL: {Url}", Page.Url);
        Logger.Information("Post-login title: {Title}", await Page.TitleAsync());
        TestContext.Out.WriteLine($"[LOGIN OK] Landed at: {Page.Url}");

        Assert.That(Page.Url, Does.Not.Contain("/Account/Login").IgnoreCase,
            "After a successful login we should no longer be on /Account/Login.");

        // Sanity: there should be no error toast visible post-login.
        var err = await login.GetErrorMessageAsync();
        Assert.That(err, Is.Null.Or.Empty,
            $"Login page is showing an error message after submit: '{err}'");
    }
}
