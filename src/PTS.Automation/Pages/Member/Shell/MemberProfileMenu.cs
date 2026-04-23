namespace PTS.Automation.Pages.Member.Shell;

/// <summary>
/// The user profile dropdown in the top-right corner — "View Profile" and
/// "Logout" items. Source: <c>_TravelMemberLayout.cshtml</c> lines 115–126
/// (desktop) and 152–163 (mobile). Both variants use the same selectors.
///
/// Logout is handled by a JavaScript click handler in the layout that POSTs
/// to <c>/Account/LogOut</c>; the page is expected to redirect back to
/// <c>/Account/Login</c>.
/// </summary>
public sealed class MemberProfileMenu
{
    private readonly IPage _page;
    private readonly AppUrl _app;

    public MemberProfileMenu(IPage page, AppUrl app)
    {
        _page = page;
        _app = app;
    }

    // ── Locators ───────────────────────────────────────────────────────
    // The layout renders TWO copies of the profile dropdown (desktop + mobile
    // header), each with the same id="dropdownMenuButton1" — invalid HTML but
    // typical in this codebase. We first find the currently-visible wrapper
    // (the outer .dropdown.user_profile whose header is rendered at the active
    // viewport), then scope Toggle and Menu to it. The Menu itself is NOT
    // `:visible` initially (it opens on click), so scoping to a visible
    // ancestor is the only way to unambiguously pick the right one.
    private ILocator Wrapper    => _page.Locator("div.dropdown.user_profile:visible").First;
    private ILocator Toggle     => Wrapper.Locator("button#dropdownMenuButton1");
    private ILocator Menu       => Wrapper.Locator("ul.dropdown-menu[aria-labelledby='dropdownMenuButton1']");
    private ILocator ViewProfile => Menu.Locator("a.dropdown-item", new() { HasText = "View Profile" });
    private ILocator LogoutLink  => Menu.Locator("a#logout");

    // ── Queries ────────────────────────────────────────────────────────
    public Task<bool> IsToggleVisibleAsync() => Toggle.IsVisibleAsync();

    /// <summary>The display name rendered inside the profile button (desktop only).</summary>
    public async Task<string> GetDisplayNameAsync()
    {
        var span = Toggle.Locator("span").First;
        return await span.CountAsync() == 0 ? "" : (await span.InnerTextAsync()).Trim();
    }

    // ── Actions ────────────────────────────────────────────────────────
    public async Task OpenAsync()
    {
        await Toggle.ClickAsync();
        await Menu.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public async Task ClickViewProfileAsync()
    {
        await OpenAsync();
        await ViewProfile.ClickAsync();
    }

    /// <summary>
    /// Logs the current user out. The mechanics are subtle because the dev's
    /// <c>#logout</c> anchor triggers <b>two</b> things when clicked:
    /// <list type="number">
    ///   <item>A JS AJAX <c>POST /Account/Logout</c> (destroys the server session)</item>
    ///   <item>Browser navigation to the anchor's <c>href</c>
    ///         (<c>/Admin/AdminProfile</c>) which for a Member-role user
    ///         redirects via 302 to <c>/Account/AccessDenied</c> and then to
    ///         the site root.</item>
    /// </list>
    /// The two race each other; the final resting URL is not guaranteed to be
    /// <c>/Account/Login</c>. What IS guaranteed is that the user is no longer
    /// on any <c>/Member/*</c> page — that's the invariant we wait for here.
    /// </summary>
    public async Task LogoutAsync()
    {
        await OpenAsync();

        // Start listening for the AJAX logout response BEFORE clicking, so we
        // don't miss it if the server is fast.
        var logoutPostTask = _page.WaitForResponseAsync(
            r => r.Url.Contains("/Account/Logout", StringComparison.OrdinalIgnoreCase)
                 && r.Status == 200,
            new PageWaitForResponseOptions
            {
                Timeout = ConfigFactory.Settings.Timeouts.DefaultMs
            });

        await LogoutLink.ClickAsync();

        // Best-effort wait for the AJAX round-trip — if it was cancelled by the
        // racing navigation, that's fine: the navigation will still log us out.
        try { await logoutPostTask; }
        catch (TimeoutException) { /* ignored — navigation path also logs out */ }

        // The real post-logout invariant: we are NOT on /Member/ any more.
        await _page.WaitForURLAsync(
            url => !url.Contains("/Member/", StringComparison.OrdinalIgnoreCase),
            new PageWaitForURLOptions
            {
                Timeout = ConfigFactory.Settings.Timeouts.NavigationMs
            });
    }
}
