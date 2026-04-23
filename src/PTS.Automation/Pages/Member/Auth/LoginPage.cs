namespace PTS.Automation.Pages.Member.Auth;

/// <summary>
/// Member portal login page.
/// Selectors derived from <c>PTS.WebUI.Web.PTSWeb/Views/Account/Login.cshtml</c>:
///   - <c>#Username</c>  (text input, name="UserName")
///   - <c>#Password</c>  (password input, name="Password")
///   - <c>#loginBtn</c>  ("Log in" button)
/// </summary>
public sealed class LoginPage : BasePage
{
    public LoginPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => App.LoginPath;

    // ── Locators ────────────────────────────────────────────────────────────
    private ILocator UsernameInput      => Page.Locator("#Username");
    private ILocator PasswordInput      => Page.Locator("#Password");
    private ILocator LoginButton        => Page.Locator("#loginBtn");
    private ILocator ForgotPasswordLink => Page.GetByRole(AriaRole.Link, new() { Name = "Forgot password?" });
    private ILocator Heading            => Page.Locator(".login-form__heading, .login-box h1").First;
    private ILocator ErrorMessage       => Page.Locator(".login-form__text.fw-bold, .error").First;

    protected override ILocator ReadinessIndicator => UsernameInput;

    // ── Actions ─────────────────────────────────────────────────────────────
    public async Task LoginAsync(string username, string password)
    {
        await UsernameInput.FillAsync(username);
        await PasswordInput.FillAsync(password);
        await LoginButton.ClickAsync();
    }

    // ── Mandatory 2FA-notice popup (SweetAlert2) ────────────────────────
    // For TravelMemberAdmin / TravelSalesGroup / Superuser / SubAdmin accounts
    // whose `isMemberAndAdminUser` flag is false, accountScript.js shows a
    // SweetAlert popup titled "Important Security Update Required: Two-Factor
    // Authentication ... is now mandatory." before redirecting. The user MUST
    // click the confirm button to trigger `redirectBasedOnRole`.
    //
    // See PTSWeb/Scripts/PTSApp/accountScript.js :: loginSuccess / show2FANoticePopup.
    private ILocator TwoFactorNoticePopup       => Page.Locator("div.swal2-container div.swal2-popup");
    private ILocator TwoFactorNoticeConfirmBtn  => Page.Locator("div.swal2-container button.swal2-confirm");

    /// <summary>
    /// Waits for the login flow to complete. The app performs an AJAX POST to
    /// <c>/Account/LoginCheck</c> (returning JSON with roles + JWT), then the
    /// client-side JS redirects based on role.
    ///
    /// For member-admin users the redirect is gated behind a SweetAlert
    /// "mandatory 2FA" notice popup — this method dismisses that popup if it
    /// appears, then waits for the URL to leave <c>/Account/Login</c>.
    /// </summary>
    public async Task WaitForPostLoginAsync()
    {
        // Race: either the popup appears, or the URL changes immediately
        // (for users not gated by the 2FA notice).
        var popupTask = TwoFactorNoticePopup
            .WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = Settings.Timeouts.DefaultMs
            })
            .ContinueWith(t => t.IsCompletedSuccessfully);

        var urlTask = Page.WaitForURLAsync(
            url => !url.Contains("/Account/Login", StringComparison.OrdinalIgnoreCase),
            new PageWaitForURLOptions { Timeout = Settings.Timeouts.DefaultMs })
            .ContinueWith(t => t.IsCompletedSuccessfully);

        var winner = await Task.WhenAny(popupTask, urlTask);

        if (winner == popupTask && popupTask.Result)
        {
            // Popup appeared — dismiss it so the JS can proceed to redirect.
            await TwoFactorNoticeConfirmBtn.ClickAsync();
        }

        // In both paths we still need to wait for the final redirect to land.
        await Page.WaitForURLAsync(
            url => !url.Contains("/Account/Login", StringComparison.OrdinalIgnoreCase),
            new PageWaitForURLOptions { Timeout = Settings.Timeouts.NavigationMs });

        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle,
            new PageWaitForLoadStateOptions { Timeout = Settings.Timeouts.NavigationMs });
    }

    // ── Queries (for assertions) ────────────────────────────────────────────
    public Task<string> GetHeadingTextAsync() => Heading.InnerTextAsync();
    public Task<bool>   IsForgotPasswordLinkVisibleAsync() => ForgotPasswordLink.IsVisibleAsync();
    public Task<bool>   IsLoginButtonEnabledAsync() => LoginButton.IsEnabledAsync();
    public async Task<string?> GetErrorMessageAsync() =>
        await ErrorMessage.CountAsync() > 0 && await ErrorMessage.IsVisibleAsync()
            ? (await ErrorMessage.InnerTextAsync()).Trim()
            : null;
}
