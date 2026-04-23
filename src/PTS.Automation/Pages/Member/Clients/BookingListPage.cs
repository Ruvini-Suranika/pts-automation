using PTS.Automation.Pages.Member.Shell;
using PTS.Automation.Pages.Shared.UiComponents;

namespace PTS.Automation.Pages.Member.Clients;

/// <summary>
/// Member → Bookings list / search page.
/// Source view: <c>Views/Client/BookingSearchView.cshtml</c>.
/// Controller action: <c>ClientController.BookingSearchView</c>.
/// JS: <c>Scripts/client/Client-booking.js</c> — on <c>#filterButton</c> click,
/// POSTs to <c>/client/GetSearchClientBookingDetailinclientsearch</c> and
/// populates <c>#clientsTableBody</c>.
///
/// Initial state: the data section (<c>#bookingMainSection</c>) is
/// <c>display:none</c> and is shown only after a search fires. The filter
/// panel is visible from the start — we anchor readiness on the Search button.
/// </summary>
public sealed class BookingListPage : MemberPage
{
    public const string SearchEndpointFragment = "/Client/GetSearchClientBookingDetailinclientsearch";

    public BookingListPage(IPage page, AppUrl app) : base(page, app)
    {
        Grid = new PtsTable(page, "#clientsTableBody");
    }

    public override string RelativePath => MemberRoutes.Bookings;

    // ── Nested components ───────────────────────────────────────────────────
    /// <summary>The bookings grid (body only). Columns defined in the view.</summary>
    public PtsTable Grid { get; }

    // ── Filter inputs ───────────────────────────────────────────────────────
    private ILocator FirstNameFilter    => Page.Locator("#txtFirstName");
    private ILocator LastNameFilter     => Page.Locator("#txtLastName");
    private ILocator BookingRefFilter   => Page.Locator("#txtBookingReference");
    private ILocator SupplierRefFilter  => Page.Locator("#txtSupplierReference");
    private ILocator EmailFilter        => Page.Locator("#txtEmail");
    private ILocator PhoneFilter        => Page.Locator("#txtTelephone");
    private ILocator DestinationFilter  => Page.Locator("#txtDestination");
    private ILocator EnquiryTypeFilter  => Page.Locator("#dropDownEnquiry");
    // Note: the dev source has a typo (`dropDOwnClientStatus`) — preserved here
    // because this IS the id the app actually renders.
    private ILocator BookingStatusFilter => Page.Locator("#dropDOwnClientStatus");
    private ILocator BookingTypeFilter  => Page.Locator("#dropDownClientType");
    private ILocator AssignedUserFilter => Page.Locator("#dropDownAssignUser");
    private ILocator CountryFilter      => Page.Locator("#dropdownSector");
    private ILocator ProfitStatusFilter => Page.Locator("#profitStatusSelect");
    private ILocator DateFromInput      => Page.Locator("#dt_SpecificRangeDateFrom");
    private ILocator DateToInput        => Page.Locator("#dt_SpecificRangeDateTo");

    // ── Controls ────────────────────────────────────────────────────────────
    private ILocator SearchButton       => Page.Locator("#filterButton");
    private ILocator ClearFiltersButton => Page.Locator("#btnreset");
    private ILocator GridSection        => Page.Locator("#bookingMainSection");
    private ILocator GridSearchInput    => Page.Locator("#search");

    // Sub-nav tabs shared across Clients/Quotes/Bookings/Issue tickets
    private ILocator SubNavBookingsTab  => Page.Locator("section.table_header a.booking, article section.table_header a.booking").First;

    /// <summary>
    /// Readiness: the filter Search button is always rendered and visible on page load
    /// (the grid itself is <c>display:none</c> until a search runs).
    /// </summary>
    protected override ILocator ReadinessIndicator => SearchButton;

    // ── Queries ─────────────────────────────────────────────────────────────
    public Task<bool> IsSearchButtonVisibleAsync()     => SearchButton.IsVisibleAsync();
    public Task<bool> IsClearFiltersButtonVisibleAsync() => ClearFiltersButton.IsVisibleAsync();
    public Task<bool> IsGridSectionVisibleAsync()      => GridSection.IsVisibleAsync();

    /// <summary>Are all the main filter inputs on the page rendered and visible?</summary>
    public async Task<bool> AreMainFilterInputsVisibleAsync() => (await Task.WhenAll(
        FirstNameFilter.IsVisibleAsync(),
        LastNameFilter.IsVisibleAsync(),
        BookingRefFilter.IsVisibleAsync(),
        SupplierRefFilter.IsVisibleAsync(),
        EmailFilter.IsVisibleAsync(),
        PhoneFilter.IsVisibleAsync(),
        DestinationFilter.IsVisibleAsync())).All(v => v);

    /// <summary>Are all the main filter dropdowns rendered and visible?</summary>
    public async Task<bool> AreMainFilterDropdownsVisibleAsync() => (await Task.WhenAll(
        BookingTypeFilter.IsVisibleAsync(),
        EnquiryTypeFilter.IsVisibleAsync(),
        BookingStatusFilter.IsVisibleAsync(),
        AssignedUserFilter.IsVisibleAsync(),
        CountryFilter.IsVisibleAsync(),
        ProfitStatusFilter.IsVisibleAsync())).All(v => v);

    // ── Actions ─────────────────────────────────────────────────────────────
    /// <summary>Fills the Email filter input. Does not click Search.</summary>
    public Task FilterByEmailAsync(string email)        => EmailFilter.FillAsync(email);

    /// <summary>Fills the Booking Reference filter input. Does not click Search.</summary>
    public Task FilterByBookingRefAsync(string bookingRef) => BookingRefFilter.FillAsync(bookingRef);

    /// <summary>
    /// Clicks Search and waits for the AJAX POST to
    /// <c>/Client/GetSearchClientBookingDetailinclientsearch</c> to complete,
    /// then waits for the loading overlay to clear. Returns the HTTP status of
    /// the search response (200 = server accepted).
    ///
    /// Uses <c>WaitForRequestAsync</c> rather than <c>WaitForResponseAsync</c>
    /// because the Response predicate occasionally misses the event on the
    /// very first search click after a fresh navigation (Playwright .NET 1.49).
    /// </summary>
    public async Task<int> SearchAsync()
    {
        var requestTask = Page.WaitForRequestAsync(
            r => r.Url.Contains(SearchEndpointFragment, StringComparison.OrdinalIgnoreCase)
                 && r.Method == "POST",
            new PageWaitForRequestOptions { Timeout = Settings.Timeouts.NavigationMs });

        await SearchButton.ClickAsync();
        var request = await requestTask;
        var response = await request.ResponseAsync();

        await Spinner.WaitUntilHiddenAsync();
        return response?.Status ?? 0;
    }

    /// <summary>
    /// Clicks Search AND captures the request payload (so tests can assert the
    /// right filter was sent). Returns (status, post-data-as-string).
    /// </summary>
    public async Task<(int Status, string PostData)> SearchAndCaptureRequestAsync()
    {
        var requestTask = Page.WaitForRequestAsync(
            r => r.Url.Contains(SearchEndpointFragment, StringComparison.OrdinalIgnoreCase)
                 && r.Method == "POST",
            new PageWaitForRequestOptions { Timeout = Settings.Timeouts.NavigationMs });

        await SearchButton.ClickAsync();
        var request = await requestTask;

        var response = await request.ResponseAsync();
        await Spinner.WaitUntilHiddenAsync();

        return (response?.Status ?? 0, request.PostData ?? "");
    }

    public Task ClearFiltersAsync() => ClearFiltersButton.ClickAsync();
}
