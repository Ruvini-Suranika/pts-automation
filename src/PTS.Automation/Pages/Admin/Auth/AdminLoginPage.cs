namespace PTS.Automation.Pages.Admin.Auth;

/// <summary>
/// Admin portal login page.
///
/// NOTE: In the current deployment, Member and Admin share the same login
/// endpoint at <c>https://qa.pts.cloud/Account/Login</c> — the landing page
/// after login is determined by the user's role claims. This class exists
/// because (a) assertions about post-login landing differ (Admin lands on an
/// admin dashboard), and (b) when/if Admin gets a dedicated login surface,
/// only this file changes.
/// </summary>
public sealed class AdminLoginPage : BasePage
{
    public AdminLoginPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => App.LoginPath;

    private ILocator UsernameInput      => Page.Locator("#Username");
    private ILocator PasswordInput      => Page.Locator("#Password");
    private ILocator LoginButton        => Page.Locator("#loginBtn");
    private ILocator ForgotPasswordLink => Page.GetByRole(AriaRole.Link, new() { Name = "Forgot password?" });
    private ILocator Heading            => Page.Locator(".login-form__heading, .login-box h1").First;
    private ILocator ErrorMessage       => Page.Locator(".login-form__text.fw-bold, .error").First;

    protected override ILocator ReadinessIndicator => UsernameInput;

    public async Task LoginAsync(string username, string password)
    {
        await UsernameInput.FillAsync(username);
        await PasswordInput.FillAsync(password);
        await LoginButton.ClickAsync();
    }

    /// <summary>
    /// Admin logins go through the same <c>AccountController.LoginCheck</c>
    /// endpoint and the same client-side <c>loginSuccess</c> JS. Superuser
    /// accounts whose <c>isMemberAndAdminUser</c> flag is false are gated
    /// behind the same "mandatory 2FA" SweetAlert popup — we dismiss it if
    /// present and then wait for the redirect.
    /// </summary>
    public async Task WaitForPostLoginAsync()
    {
        var popup = Page.Locator("div.swal2-container div.swal2-popup");
        var confirm = Page.Locator("div.swal2-container button.swal2-confirm");

        var popupTask = popup.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = Settings.Timeouts.DefaultMs
        }).ContinueWith(t => t.IsCompletedSuccessfully);

        var urlTask = Page.WaitForURLAsync(
            url => !url.Contains("/Account/Login", StringComparison.OrdinalIgnoreCase),
            new PageWaitForURLOptions { Timeout = Settings.Timeouts.DefaultMs })
            .ContinueWith(t => t.IsCompletedSuccessfully);

        var winner = await Task.WhenAny(popupTask, urlTask);

        if (winner == popupTask && popupTask.Result)
        {
            await confirm.ClickAsync();
        }

        await Page.WaitForURLAsync(
            url => !url.Contains("/Account/Login", StringComparison.OrdinalIgnoreCase),
            new PageWaitForURLOptions { Timeout = Settings.Timeouts.NavigationMs });

        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle,
            new PageWaitForLoadStateOptions { Timeout = Settings.Timeouts.NavigationMs });
    }

    /// <summary>Same shared <c>Login.cshtml</c> as the Member portal — exposed for smoke assertions.</summary>
    public Task<string> GetHeadingTextAsync() => Heading.InnerTextAsync();

    public Task<bool> IsForgotPasswordLinkVisibleAsync() => ForgotPasswordLink.IsVisibleAsync();

    public async Task<string?> GetErrorMessageAsync() =>
        await ErrorMessage.CountAsync() > 0 && await ErrorMessage.IsVisibleAsync()
            ? (await ErrorMessage.InnerTextAsync()).Trim()
            : null;
}
