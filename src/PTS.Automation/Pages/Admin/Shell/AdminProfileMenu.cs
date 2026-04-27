namespace PTS.Automation.Pages.Admin.Shell;

/// <summary>
/// Admin profile dropdown (View Profile / Logout). Source:
/// <c>Views/Admin/_AdminLayout.cshtml</c> — same duplicate
/// <c>id="dropdownMenuButton1"</c> pattern as the Member layout.
///
/// Logout: layout wires <c>.button-logout</c> to AJAX POST
/// <c>/Account/Logout</c> then client navigation; we wait until we are no
/// longer on any <c>/Admin/*</c> URL.
/// </summary>
public sealed class AdminProfileMenu
{
    private readonly IPage _page;

    public AdminProfileMenu(IPage page, AppUrl app)
    {
        _ = app;
        _page = page;
    }

    private ILocator Wrapper    => _page.Locator("div.dropdown.user_profile:visible").First;
    private ILocator Toggle     => Wrapper.Locator("button#dropdownMenuButton1");
    private ILocator Menu       => Wrapper.Locator("ul.dropdown-menu[aria-labelledby='dropdownMenuButton1']");
    private ILocator LogoutLink => Menu.Locator("a.button-logout");

    public Task<bool> IsToggleVisibleAsync() => Toggle.IsVisibleAsync();

    public async Task<string> GetDisplayNameAsync()
    {
        var span = Toggle.Locator("#AdminUserName").First;
        return await span.CountAsync() == 0 ? "" : (await span.InnerTextAsync()).Trim();
    }

    public async Task OpenAsync()
    {
        await Toggle.ClickAsync();
        await Menu.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public async Task LogoutAsync()
    {
        await OpenAsync();

        var logoutPostTask = _page.WaitForResponseAsync(
            r => r.Url.Contains("/Account/Logout", StringComparison.OrdinalIgnoreCase)
                 && r.Status == 200,
            new PageWaitForResponseOptions
            {
                Timeout = ConfigFactory.Settings.Timeouts.DefaultMs
            });

        await LogoutLink.ClickAsync();

        try { await logoutPostTask; }
        catch (TimeoutException) { /* navigation may win */ }

        await _page.WaitForURLAsync(
            url => !url.Contains("/Admin/", StringComparison.OrdinalIgnoreCase),
            new PageWaitForURLOptions
            {
                Timeout = ConfigFactory.Settings.Timeouts.NavigationMs
            });
    }
}
