using Allure.NUnit.Attributes;
using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Admin.Auth;

namespace PTS.Automation.Features.Smoke;

/// <summary>
/// Admin-portal login smoke: shared <c>/Account/Login</c> surface with Member,
/// but navigation and post-login expectations use <see cref="AppUrl.Admin"/>.
/// Credential test uses <see cref="UsersSettings.PtsAdmin"/>.
/// </summary>
[TestFixture]
[AllureSuite(Categories.Smoke)]
[AllureFeature("Admin login")]
[AllureTag(Categories.Smoke)]
[AllureTag(Categories.Admin)]
[AllureTag(Categories.Authentication)]
[Category(Categories.Smoke)]
[Category(Categories.Admin)]
[Category(Categories.Authentication)]
public class AdminLoginSmokeTests : BaseTest
{
    [Test]
    [Category(Categories.UI)]
    [Description("Admin login entry (shared Account/Login) loads and the form is interactive.")]
    public async Task Login_page_loads_with_expected_form()
    {
        var login = new AdminLoginPage(Page, Settings.Applications.Admin);
        await StepAsync("Open Admin login page", () => login.GotoAsync());

        await StepAsync("Verify login URL and form controls", async () =>
        {
            await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(@"/Account/Login", System.Text.RegularExpressions.RegexOptions.IgnoreCase));
            await Expect(Page.Locator("#Username")).ToBeVisibleAsync();
            await Expect(Page.Locator("#Password")).ToBeVisibleAsync();
            await Expect(Page.Locator("#loginBtn")).ToBeEnabledAsync();
        });

        await StepAsync("Verify welcome or sign-in heading", async () =>
        {
            var heading = await login.GetHeadingTextAsync();
            Assert.That(heading, Does.Match("Welcome|Sign in"),
                $"Unexpected login heading '{heading}'");
        });
    }

    [Test]
    [Category(Categories.UI)]
    [Description("Forgot-password link is present on the shared login view.")]
    public async Task Login_page_shows_forgot_password_link()
    {
        var login = new AdminLoginPage(Page, Settings.Applications.Admin);
        await StepAsync("Open Admin login page", () => login.GotoAsync());

        await StepAsync("Verify forgot password link is visible", async () =>
        {
            Assert.That(await login.IsForgotPasswordLinkVisibleAsync(), Is.True,
                "Forgot password link must be visible on the login page.");
        });
    }

    [Test]
    [Category(Categories.UI)]
    [Description("Valid PtsAdmin credentials reach the Admin area (not Member login).")]
    public async Task Admin_logs_in_with_valid_credentials()
    {
        var creds = Settings.Users.PtsAdmin;
        if (!creds.IsConfigured)
        {
            Assert.Ignore(
                "PtsAdmin credentials are not configured. Configure via:\n" +
                "  dotnet user-secrets set \"Users:PtsAdmin:Username\" \"<user>\"\n" +
                "  dotnet user-secrets set \"Users:PtsAdmin:Password\" \"<pwd>\"\n" +
                "or environment variables PTS_Users__PtsAdmin__Username / PTS_Users__PtsAdmin__Password.");
        }

        var login = new AdminLoginPage(Page, Settings.Applications.Admin);
        await StepAsync("Open Admin login page", () => login.GotoAsync());

        Logger.Information("Attempting PtsAdmin login as '{User}' against {Base}",
            creds.Username, Settings.Applications.Admin.BaseUrl);

        await StepAsync("Submit PtsAdmin credentials", async () =>
        {
            await login.LoginAsync(creds.Username, creds.Password);
            await login.WaitForPostLoginAsync();
        });

        await StepAsync("Log post-login URL and title", async () =>
        {
            Logger.Information("Post-login URL: {Url}", Page.Url);
            Logger.Information("Post-login title: {Title}", await Page.TitleAsync());
            TestContext.Out.WriteLine($"[ADMIN LOGIN OK] Landed at: {Page.Url}");
        });

        await StepAsync("Verify navigation away from login page", async () =>
        {
            Assert.That(Page.Url, Does.Not.Contain("/Account/Login").IgnoreCase,
                "After a successful login we should no longer be on /Account/Login.");
        });

        await StepAsync("Verify URL is in Admin application area", async () =>
        {
            Assert.That(Page.Url, Does.Contain("/Admin/").IgnoreCase,
                "Superuser / PtsAdmin login should land in the Admin application area.");
        });

        await StepAsync("Verify no login error message after submit", async () =>
        {
            var err = await login.GetErrorMessageAsync();
            Assert.That(err, Is.Null.Or.Empty,
                $"Login page is showing an error message after submit: '{err}'");
        });
    }
}
