using System.Text.RegularExpressions;

namespace PTS.Automation.Pages.Member.Shell;

/// <summary>
/// The primary horizontal navigation in the Member portal (Clients / Calendar /
/// Suppliers / Accounts / Reporting). Present on every authenticated page via
/// <c>MemberMenu.cshtml</c> partial in <c>_TravelMemberLayout.cshtml</c>.
///
/// <para>
/// Nav items are dropdowns, not direct links, so every <c>GoTo*Async</c>
/// method expands the parent then clicks the inner item by its exact visible
/// text. We match <i>exactly</i> (anchored regex) because some sibling labels
/// overlap — e.g. "Payments" would otherwise also match "GPS Payments" and
/// "Payments due".
/// </para>
///
/// <para>
/// The layout renders two copies of <c>MemberMenu</c> (desktop + mobile) for
/// responsive display. At the framework's default viewport (lg+) only the
/// desktop copy is visible; we target it via the visible pseudo-locator.
/// </para>
/// </summary>
public sealed class MemberNavBar
{
    private readonly IPage _page;
    private readonly AppUrl _app;

    public MemberNavBar(IPage page, AppUrl app)
    {
        _page = page;
        _app = app;
    }

    // Only consider the currently-visible nav copy. Prevents accidentally
    // clicking the off-screen mobile copy on a desktop viewport.
    private ILocator Root => _page.Locator("nav.navbar.navbar-expand-lg:visible").First;

    // ── Top-level dropdown toggles — matched by the exact label text on
    //    the inner <a.dropdown-toggle>, so "Suppliers" doesn't collide with
    //    Reporting > "Supplier Debits" etc.
    private ILocator ClientsDropdown   => DropdownByLabel("Clients");
    private ILocator SuppliersDropdown => DropdownByLabel("Suppliers");
    private ILocator AccountsDropdown  => DropdownByLabel("Accounts");
    private ILocator ReportingDropdown => DropdownByLabel("Reporting");
    private ILocator CalendarLink      => Root.Locator("#calenderLink");

    private ILocator DropdownByLabel(string label) =>
        Root.Locator("li.nav-item.dropdown")
            .Filter(new LocatorFilterOptions
            {
                Has = _page.Locator("a.nav-link.dropdown-toggle")
                    .Filter(new LocatorFilterOptions { HasTextRegex = ExactText(label) })
            })
            .First;

    /// <summary>True if the nav bar is rendered and visible (i.e. we're on an authenticated page).</summary>
    public Task<bool> IsVisibleAsync() => Root.IsVisibleAsync();

    // ── Clients dropdown ───────────────────────────────────────────────
    public Task GoToClientsAsync()      => ClickDropdownItemAsync(ClientsDropdown,   "Clients");
    public Task GoToQuotesAsync()       => ClickDropdownItemAsync(ClientsDropdown,   "Quotes");
    public Task GoToBookingsAsync()     => ClickDropdownItemAsync(ClientsDropdown,   "Bookings");
    public Task GoToIssueTicketsAsync() => ClickDropdownItemAsync(ClientsDropdown,   "Issue tickets");

    // ── Calendar (top-level, not a dropdown) ───────────────────────────
    public Task GoToCalendarAsync() => CalendarLink.ClickAsync();

    // ── Suppliers dropdown ─────────────────────────────────────────────
    public Task GoToAccommodationAsync() => ClickDropdownItemAsync(SuppliersDropdown, "Accommodation");
    public Task GoToActivitiesAsync()    => ClickDropdownItemAsync(SuppliersDropdown, "Activities");
    public Task GoToFlightsAsync()       => ClickDropdownItemAsync(SuppliersDropdown, "Flights");
    public Task GoToPackagesAsync()      => ClickDropdownItemAsync(SuppliersDropdown, "Packages");
    public Task GoToTransportAsync()     => ClickDropdownItemAsync(SuppliersDropdown, "Transport");
    public Task GoToCruisesAsync()       => ClickDropdownItemAsync(SuppliersDropdown, "Cruises");

    // ── Accounts dropdown ──────────────────────────────────────────────
    public Task GoToAccountOverviewAsync()     => ClickDropdownItemAsync(AccountsDropdown, "Overview");
    public Task GoToTransactionsAsync()        => ClickDropdownItemAsync(AccountsDropdown, "Transactions");
    public Task GoToGpsPaymentsAsync()         => ClickDropdownItemAsync(AccountsDropdown, "GPS Payments");
    public Task GoToPaymentsOutstandingAsync() => ClickDropdownItemAsync(AccountsDropdown, "Payments");
    public Task GoToUnclaimedAsync()           => ClickDropdownItemAsync(AccountsDropdown, "Unclaimed");
    public Task GoToUnassignedAsync()          => ClickDropdownItemAsync(AccountsDropdown, "Unassigned");
    public Task GoToProfitClaimsAsync()        => ClickDropdownItemAsync(AccountsDropdown, "Profit claims");

    // ── Reporting dropdown ─────────────────────────────────────────────
    public Task GoToBusinessReportingAsync()        => ClickDropdownItemAsync(ReportingDropdown, "Business");
    public Task GoToSupplierReportingAsync()        => ClickDropdownItemAsync(ReportingDropdown, "Suppliers");
    public Task GoToBookingsReportingAsync()        => ClickDropdownItemAsync(ReportingDropdown, "Bookings");
    public Task GoToAtolReportingAsync()            => ClickDropdownItemAsync(ReportingDropdown, "ATOL");
    public Task GoToSupplierDebitsReportingAsync()  => ClickDropdownItemAsync(ReportingDropdown, "Supplier Debits");
    public Task GoToUserCommissionReportingAsync()  => ClickDropdownItemAsync(ReportingDropdown, "User Commission Breakdown");

    // ── Helpers ────────────────────────────────────────────────────────
    private async Task ClickDropdownItemAsync(ILocator parentDropdown, string itemText)
    {
        await parentDropdown.Locator("a.nav-link.dropdown-toggle").ClickAsync();

        // Exact-text match: "Payments" must NOT also match "GPS Payments" or
        // "Payments due" in the Accounts dropdown.
        await parentDropdown
            .Locator("ul.dropdown-menu a.dropdown-item")
            .Filter(new LocatorFilterOptions { HasTextRegex = ExactText(itemText) })
            .First
            .ClickAsync();
    }

    /// <summary>
    /// Builds a regex that matches the given text exactly (anchored, with
    /// optional surrounding whitespace). Used instead of Playwright's default
    /// substring <c>HasText</c> wherever sibling labels overlap.
    /// </summary>
    private static Regex ExactText(string value) =>
        new($@"^\s*{Regex.Escape(value)}\s*$", RegexOptions.IgnoreCase);
}
