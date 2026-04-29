namespace PTS.Automation.Pages.Admin.Shell;

/// <summary>
/// Primary admin horizontal nav from <c>Views/Shared/AdminMenu.cshtml</c>, embedded in
/// <c>_AdminLayout.cshtml</c> desktop header (<c>header.top_header.admin-menu.desktop-menu</c>).
/// Targets the visible desktop copy at the default Playwright viewport (lg+).
/// </summary>
public sealed class AdminNavBar
{
    private readonly IPage _page;

    /// <summary>
    /// Full page navigations can exceed the default action timeout; link clicks wait for navigation to finish.
    /// </summary>
    private static LocatorClickOptions NavClick =>
        new() { Timeout = ConfigFactory.Settings.Timeouts.NavigationMs };

    public AdminNavBar(IPage page, AppUrl app)
    {
        _ = app;
        _page = page;
    }

    private ILocator DesktopHeader => _page.Locator("header.top_header.admin-menu.desktop-menu").First;

    /// <summary>
    /// Follows a desktop menu link via <see cref="IPage.GotoAsync"/> instead of <see cref="ILocator.ClickAsync"/>.
    /// Some admin MVC pages never satisfy Playwright's post-click navigation commit, which can hang the test for the full timeout.
    /// </summary>
    private async Task NavigateViaDesktopNavLinkAsync(string linkAccessibleName)
    {
        var link = DesktopHeader.GetByRole(AriaRole.Link, new() { Name = linkAccessibleName, Exact = true });
        var navTimeout = ConfigFactory.Settings.Timeouts.NavigationMs;
        var href = await link.GetAttributeAsync("href");
        if (string.IsNullOrWhiteSpace(href))
            throw new InvalidOperationException($"Desktop nav link '{linkAccessibleName}' has no href attribute.");

        var target = href.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? href
            : new Uri(new Uri(_page.Url), href).ToString();

        await _page.GotoAsync(target, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded,
            Timeout = navTimeout
        });
    }

    private async Task OpenBankMenuAsync()
    {
        await DesktopHeader.Locator("a.nav-link.dropdown-toggle.bankActive").ClickAsync(NavClick);
    }

    private async Task OpenAdminMenuAsync()
    {
        await DesktopHeader.Locator("a.nav-link.dropdown-toggle.adminActive").ClickAsync(NavClick);
    }

    private async Task OpenMembersMenuAsync()
    {
        await DesktopHeader.Locator("a.nav-link.dropdown-toggle.memberActive.restricted_button").ClickAsync(NavClick);
    }

    /// <summary>Members → Members list (<c>Admin/Members</c>).</summary>
    public async Task GoToMembersListAsync()
    {
        await OpenMembersMenuAsync();
        await NavigateViaDesktopNavLinkAsync("Members");
    }

    /// <summary>Members → Applying (<c>Admin/Applying</c>).</summary>
    public async Task GoToApplyingAsync()
    {
        await OpenMembersMenuAsync();
        await NavigateViaDesktopNavLinkAsync("Applying");
    }

    /// <summary>Bank → Trust Accounts.</summary>
    public async Task GoToTrustAccountsAsync()
    {
        await OpenBankMenuAsync();
        await NavigateViaDesktopNavLinkAsync("Trust Accounts");
    }

    /// <summary>Bank → Reconciliation.</summary>
    public async Task GoToReconciliationAsync()
    {
        await OpenBankMenuAsync();
        await NavigateViaDesktopNavLinkAsync("Reconciliation");
    }

    private async Task OpenAdminDebitsSubmenuAsync()
    {
        await OpenAdminMenuAsync();
        await DesktopHeader.Locator("a.dropdown-item.submenu-toggle")
            .Filter(new() { HasText = "Debits" })
            .First
            .ClickAsync(NavClick);
    }

    public async Task GoToDebitsUnauthorisedAsync()
    {
        await OpenAdminDebitsSubmenuAsync();
        await NavigateViaDesktopNavLinkAsync("Debits unauthorised");
    }

    public async Task GoToDebitsAuthorisedAsync()
    {
        await OpenAdminDebitsSubmenuAsync();
        await NavigateViaDesktopNavLinkAsync("Debits authorised");
    }

    public async Task GoToDebitsGroupingAsync()
    {
        await OpenAdminDebitsSubmenuAsync();
        await NavigateViaDesktopNavLinkAsync("Debits grouping");
    }

    public async Task GoToTrusteesAuthorisationAsync()
    {
        await OpenAdminDebitsSubmenuAsync();
        await NavigateViaDesktopNavLinkAsync("Trustee's authorisation");
    }

    private async Task OpenAdminCreditsSubmenuAsync()
    {
        await OpenAdminMenuAsync();
        await DesktopHeader.Locator("a.dropdown-item.submenu-toggle")
            .Filter(new() { HasText = "Credits" })
            .First
            .ClickAsync(NavClick);
    }

    public async Task GoToUnclaimedOverviewAsync()
    {
        await OpenAdminCreditsSubmenuAsync();
        await NavigateViaDesktopNavLinkAsync("Unclaimed overview");
    }

    public async Task GoToUnassignedOverviewAsync()
    {
        await OpenAdminCreditsSubmenuAsync();
        await NavigateViaDesktopNavLinkAsync("Unassigned overview");
    }

    /// <summary>Admin → Bookings (opens <c>Admin/BookingSearch</c>).</summary>
    public async Task GoToBookingSearchAsync()
    {
        await OpenAdminMenuAsync();
        var link = DesktopHeader.Locator("a.dropdown-item[href*='BookingSearch']").First;
        var href = await link.GetAttributeAsync("href");
        if (string.IsNullOrWhiteSpace(href))
            throw new InvalidOperationException("Admin Bookings nav link has no href.");

        var target = href.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? href
            : new Uri(new Uri(_page.Url), href).ToString();

        await _page.GotoAsync(target, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded,
            Timeout = ConfigFactory.Settings.Timeouts.NavigationMs
        });
    }

    private async Task OpenAdminBookingsSubmenuAsync()
    {
        await OpenAdminMenuAsync();
        await DesktopHeader.Locator("a.dropdown-item.submenu-toggle")
            .Filter(new() { HasText = "Bookings" })
            .First
            .ClickAsync(NavClick);
    }

    /// <summary>Admin → Bookings → Clients (<c>Admin/SearchClient</c>).</summary>
    public async Task GoToClientSearchAsync()
    {
        await OpenAdminBookingsSubmenuAsync();
        await NavigateViaDesktopNavLinkAsync("Clients");
    }

    /// <summary>Header settings (gear) mega menu → Users → <c>Admin/MemberUser</c>.</summary>
    public async Task GoToSettingsMemberUsersAsync()
    {
        await DesktopHeader.Locator("#settingDropdown").ClickAsync(NavClick);
        var link = _page.Locator(".MegaContainer a[href*='MemberUser']").First;
        await link.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = ConfigFactory.Settings.Timeouts.DefaultMs
        });

        var href = await link.GetAttributeAsync("href");
        if (string.IsNullOrWhiteSpace(href))
            throw new InvalidOperationException("Settings mega menu Users link has no href.");

        var target = href.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? href
            : new Uri(new Uri(_page.Url), href).ToString();

        await _page.GotoAsync(target, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded,
            Timeout = ConfigFactory.Settings.Timeouts.NavigationMs
        });
    }
}
