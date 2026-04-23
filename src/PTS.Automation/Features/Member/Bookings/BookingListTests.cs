using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Member.Clients;
using PTS.Automation.Pages.Member.Dashboard;

namespace PTS.Automation.Features.Member.Bookings;

/// <summary>
/// Read-only coverage for the Member → Bookings list page. These tests do NOT
/// depend on any specific booking existing in QA — they assert on page layout
/// and the search action's contract with the server, which is invariant.
///
/// Covered:
///   1. Page loads with all expected filter inputs / dropdowns / controls.
///   2. Clicking Search fires a POST to the documented AJAX endpoint and the
///      grid section becomes visible.
///   3. Applying the Email filter round-trips the value into the POST body.
/// </summary>
[TestFixture]
[Category(Categories.Regression)]
[Category(Categories.Member)]
[Category(Categories.Bookings)]
public class BookingListTests : MemberTest
{
    [Test]
    [Description("Booking list page loads with the expected filter form, dropdowns, and Search / Clear controls.")]
    public async Task Booking_list_page_loads_with_expected_controls()
    {
        var dashboard = new DashboardPage(Page, Settings.Applications.Member);
        var bookings  = new BookingListPage(Page, Settings.Applications.Member);

        await StepAsync("Open Dashboard", () => dashboard.GotoAsync());

        await StepAsync("Navigate to Bookings via nav bar", async () =>
        {
            await dashboard.NavBar.GoToBookingsAsync();
            await bookings.WaitForReadyAsync();
        });

        await StepAsync("Assert filter + controls are rendered", async () =>
        {
            Assert.That(await bookings.IsSearchButtonVisibleAsync(),       Is.True, "Search button must be visible.");
            Assert.That(await bookings.IsClearFiltersButtonVisibleAsync(), Is.True, "Clear Filters button must be visible.");
            Assert.That(await bookings.AreMainFilterInputsVisibleAsync(),  Is.True, "All main text filter inputs must be visible.");
            Assert.That(await bookings.AreMainFilterDropdownsVisibleAsync(), Is.True, "All main dropdown filters must be visible.");
        });

        await StepAsync("Assert grid section is hidden until a search runs (by design)", async () =>
        {
            Assert.That(await bookings.IsGridSectionVisibleAsync(), Is.False,
                "Per BookingSearchView.cshtml, #bookingMainSection starts with display:none until a search fires.");
        });
    }

    [Test]
    [Description("Clicking Search fires a POST to /Client/GetSearchClientBookingDetailinclientsearch, " +
                 "returns 200, and the grid section becomes visible.")]
    public async Task Clicking_search_fires_booking_search_endpoint_and_reveals_grid()
    {
        var dashboard = new DashboardPage(Page, Settings.Applications.Member);
        var bookings  = new BookingListPage(Page, Settings.Applications.Member);

        await StepAsync("Open Dashboard", () => dashboard.GotoAsync());

        await StepAsync("Navigate to Bookings", async () =>
        {
            await dashboard.NavBar.GoToBookingsAsync();
            await bookings.WaitForReadyAsync();
        });

        // Apply a sentinel booking-ref filter that will not match any real
        // booking. This keeps the test FAST (server returns empty set in
        // hundreds of ms) and deterministic (not dependent on how many
        // bookings this test user has accumulated in QA over time).
        const string sentinelRef = "AUTO-SEARCH-SENTINEL-0000";
        await StepAsync($"Set Booking Reference filter to '{sentinelRef}'",
            () => bookings.FilterByBookingRefAsync(sentinelRef));

        var status = await StepAsync("Click Search and wait for AJAX",
            () => bookings.SearchAsync());

        Assert.That(status, Is.EqualTo(200),
            $"Search endpoint should return 200 OK. Got {status}.");

        await StepAsync("Grid section should be visible after search", async () =>
        {
            // The app toggles #bookingMainSection visible in the JS after the
            // AJAX response. Give it a short moment to repaint if necessary.
            await Page.WaitForTimeoutAsync(200);
            Assert.That(await bookings.IsGridSectionVisibleAsync(), Is.True,
                "The grid section (#bookingMainSection) should be visible after the first Search click.");

            var rowCount = await bookings.Grid.RowCountAsync();
            Logger.Information("Rows returned for sentinel-ref search: {Count}", rowCount);
            // Zero rows is the expected (and desirable) state here.
            Assert.That(rowCount, Is.GreaterThanOrEqualTo(0));
        });
    }

    [Test]
    [Description("Filter-value round-trip: the Email filter value is included in the AJAX request body " +
                 "when Search is clicked.")]
    public async Task Email_filter_value_is_sent_in_search_request_body()
    {
        var dashboard = new DashboardPage(Page, Settings.Applications.Member);
        var bookings  = new BookingListPage(Page, Settings.Applications.Member);

        await StepAsync("Open Dashboard", () => dashboard.GotoAsync());

        await StepAsync("Navigate to Bookings", async () =>
        {
            await dashboard.NavBar.GoToBookingsAsync();
            await bookings.WaitForReadyAsync();
        });

        // Use a sentinel unlikely to match real data so the grid returns empty;
        // that's fine — we're asserting on the request, not the response.
        const string sentinel = "nobody.matches.this@pts-automation.test";
        await StepAsync($"Set Email filter to '{sentinel}'",
            () => bookings.FilterByEmailAsync(sentinel));

        var (status, postData) = await StepAsync("Click Search and capture request",
            () => bookings.SearchAndCaptureRequestAsync());

        Assert.That(status, Is.EqualTo(200));
        Assert.That(postData, Does.Contain(Uri.EscapeDataString(sentinel)).IgnoreCase
                             .Or.Contain(sentinel).IgnoreCase,
            $"Search request body should carry the email filter value.\nActual POST body: {postData}");

        Logger.Information("Round-trip confirmed — POST body length: {Len}", postData.Length);
    }
}
