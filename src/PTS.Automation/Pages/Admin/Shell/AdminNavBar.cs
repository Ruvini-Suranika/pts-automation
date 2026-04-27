namespace PTS.Automation.Pages.Admin.Shell;

/// <summary>
/// Primary admin horizontal nav from <c>Views/Shared/AdminMenu.cshtml</c>, embedded in
/// <c>_AdminLayout.cshtml</c> desktop header (<c>header.top_header.admin-menu.desktop-menu</c>).
/// Targets the visible desktop copy at the default Playwright viewport (lg+).
/// </summary>
public sealed class AdminNavBar
{
    private readonly IPage _page;

    public AdminNavBar(IPage page, AppUrl app)
    {
        _ = app;
        _page = page;
    }

    private ILocator DesktopHeader => _page.Locator("header.top_header.admin-menu.desktop-menu").First;

    private async Task OpenBankMenuAsync()
    {
        await DesktopHeader.Locator("a.nav-link.dropdown-toggle.bankActive").ClickAsync();
    }

    private async Task OpenAdminMenuAsync()
    {
        await DesktopHeader.Locator("a.nav-link.dropdown-toggle.adminActive").ClickAsync();
    }

    /// <summary>Bank → Trust Accounts.</summary>
    public async Task GoToTrustAccountsAsync()
    {
        await OpenBankMenuAsync();
        await DesktopHeader.GetByRole(AriaRole.Link, new() { Name = "Trust Accounts", Exact = true }).ClickAsync();
    }

    /// <summary>Bank → Reconciliation.</summary>
    public async Task GoToReconciliationAsync()
    {
        await OpenBankMenuAsync();
        await DesktopHeader.GetByRole(AriaRole.Link, new() { Name = "Reconciliation", Exact = true }).ClickAsync();
    }

    private async Task OpenAdminDebitsSubmenuAsync()
    {
        await OpenAdminMenuAsync();
        await DesktopHeader.Locator("a.dropdown-item.submenu-toggle")
            .Filter(new() { HasText = "Debits" })
            .First
            .ClickAsync();
    }

    public async Task GoToDebitsUnauthorisedAsync()
    {
        await OpenAdminDebitsSubmenuAsync();
        await DesktopHeader.GetByRole(AriaRole.Link, new() { Name = "Debits unauthorised", Exact = true }).ClickAsync();
    }

    public async Task GoToDebitsAuthorisedAsync()
    {
        await OpenAdminDebitsSubmenuAsync();
        await DesktopHeader.GetByRole(AriaRole.Link, new() { Name = "Debits authorised", Exact = true }).ClickAsync();
    }

    public async Task GoToDebitsGroupingAsync()
    {
        await OpenAdminDebitsSubmenuAsync();
        await DesktopHeader.GetByRole(AriaRole.Link, new() { Name = "Debits grouping", Exact = true }).ClickAsync();
    }

    public async Task GoToTrusteesAuthorisationAsync()
    {
        await OpenAdminDebitsSubmenuAsync();
        await DesktopHeader.GetByRole(AriaRole.Link, new() { Name = "Trustee's authorisation", Exact = true }).ClickAsync();
    }

    private async Task OpenAdminCreditsSubmenuAsync()
    {
        await OpenAdminMenuAsync();
        await DesktopHeader.Locator("a.dropdown-item.submenu-toggle")
            .Filter(new() { HasText = "Credits" })
            .First
            .ClickAsync();
    }

    public async Task GoToUnclaimedOverviewAsync()
    {
        await OpenAdminCreditsSubmenuAsync();
        await DesktopHeader.GetByRole(AriaRole.Link, new() { Name = "Unclaimed overview", Exact = true }).ClickAsync();
    }

    public async Task GoToUnassignedOverviewAsync()
    {
        await OpenAdminCreditsSubmenuAsync();
        await DesktopHeader.GetByRole(AriaRole.Link, new() { Name = "Unassigned overview", Exact = true }).ClickAsync();
    }
}
