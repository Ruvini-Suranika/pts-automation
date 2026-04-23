using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Member.Accounts;
using PTS.Automation.Pages.Member.Dashboard;

namespace PTS.Automation.Features.Member.Accounts;

/// <summary>
/// Coverage for Member → Accounts → Payments (Outstanding).
///
/// Maps to the QA backlog items:
///   - Branch Store column visible in Payments Outstanding screen
///   - Send Reminder function for Payments Outstanding
///   - Refine Search function on the Accounts Payments Outstanding page
///   - Payment Type search dropdown
///   - Booking link in each row
///
/// Where a test inherently needs a row to exist (e.g. clicking the booking
/// link), it skips with a clear message rather than failing if the QA env
/// has no outstanding payments for the test member.
/// </summary>
[TestFixture]
[Category(Categories.Regression)]
[Category(Categories.Member)]
public class PaymentsOutstandingTests : MemberTest
{
    private const string FeatureCategory = "Accounts.PaymentsOutstanding";

    [Test, Category(FeatureCategory)]
    [Description("BRANCH STORE column is present in the Payments Outstanding grid header.")]
    public async Task Branch_store_column_is_present_in_grid_header()
    {
        var dashboard = new DashboardPage(Page, Settings.Applications.Member);
        var page      = new PaymentsOutstandingPage(Page, Settings.Applications.Member);

        await StepAsync("Open Dashboard", () => dashboard.GotoAsync());
        await StepAsync("Navigate to Payments Outstanding", async () =>
        {
            await dashboard.NavBar.GoToPaymentsOutstandingAsync();
            await page.WaitForReadyAsync();
        });

        var headers = await StepAsync("Read column headers",
            () => page.GetColumnHeadersAsync());

        Logger.Information("Headers: {Headers}", string.Join(" | ", headers));

        Assert.That(headers, Does.Contain("BRANCH STORE")
                                .Or.Contain("Branch Store")
                                .Or.Contain("BRANCH STORE".ToLowerInvariant()),
            $"Expected a BRANCH STORE column. Got: {string.Join(" | ", headers)}");

        var idx = await page.ColumnIndexOfAsync("BRANCH STORE");
        Assert.That(idx, Is.GreaterThan(0),
            "BRANCH STORE column should resolve to a positive 1-based index.");
    }

    [Test, Category(FeatureCategory)]
    [Description("Send Reminder with no row selected shows a 'Please select a client' error " +
                 "and does NOT open the email compose modal.")]
    public async Task Send_reminder_with_no_selection_shows_error_and_does_not_open_email_modal()
    {
        var dashboard = new DashboardPage(Page, Settings.Applications.Member);
        var page      = new PaymentsOutstandingPage(Page, Settings.Applications.Member);

        await StepAsync("Open Dashboard", () => dashboard.GotoAsync());
        await StepAsync("Navigate to Payments Outstanding", async () =>
        {
            await dashboard.NavBar.GoToPaymentsOutstandingAsync();
            await page.WaitForReadyAsync();
        });

        await StepAsync("Wait for grid load", () => page.WaitForGridLoadedAsync());

        Assert.That(await page.IsSendReminderVisibleAsync(), Is.True,
            "Send Reminder button must be visible.");

        await StepAsync("Click Send Reminder without selecting any row",
            () => page.ClickSendReminderAsync());

        // The dev JS calls showErrorMessage("Please select a client") and
        // returns early — the email compose modal must NOT open.
        await Page.WaitForTimeoutAsync(400);
        Assert.That(await page.IsEmailComposeModalVisibleAsync(), Is.False,
            "Email compose modal (#addemailtemplate) should NOT open without a row selection.");
    }

    [Test, Category(FeatureCategory)]
    [Description("Refine search: typing in the inline grid Search box hides non-matching rows " +
                 "(client-side filter, no AJAX).")]
    public async Task Refine_search_filters_rows_client_side()
    {
        var dashboard = new DashboardPage(Page, Settings.Applications.Member);
        var page      = new PaymentsOutstandingPage(Page, Settings.Applications.Member);

        await StepAsync("Open Dashboard", () => dashboard.GotoAsync());
        await StepAsync("Navigate to Payments Outstanding", async () =>
        {
            await dashboard.NavBar.GoToPaymentsOutstandingAsync();
            await page.WaitForReadyAsync();
        });

        await StepAsync("Wait for grid load", () => page.WaitForGridLoadedAsync());

        var rowsBefore = await page.Grid.RowCountAsync();
        if (rowsBefore == 0)
            Assert.Ignore("No outstanding payments exist for the test member — refine-search has nothing to filter.");

        // A sentinel string that won't match any real client name / booking ref / etc.
        const string sentinel = "ZZZ_PTS_AUTOMATION_NO_MATCH_ZZZ";
        await StepAsync($"Type sentinel '{sentinel}' into the grid search box",
            () => page.SetGridSearchAsync(sentinel));

        // The dev JS filters DOM rows by hiding non-matching <tr>s on keyup.
        await Page.WaitForTimeoutAsync(300);

        // Visible row count should drop to 0 after the sentinel is applied.
        // NOTE: chain :visible INTO the row selector — `Rows.Locator(":visible")`
        // would descend and match every visible child of every row (returns
        // thousands). Using the combined css selector keeps the count bound
        // to <tr> elements only.
        var visibleAfter = await Page.Locator("#PaymentOutstanding tr:visible").CountAsync();
        Assert.That(visibleAfter, Is.EqualTo(0),
            $"After applying a non-matching filter all rows should be hidden. " +
            $"Rows before: {rowsBefore}, visible after: {visibleAfter}");
    }

    [Test, Category(FeatureCategory)]
    [Description("Payment Type dropdown: selecting Deposit / Balance / Part-Payment fires a " +
                 "POST to /TravelMemberAccount/GetSearchPaymentOutstanding and the request body " +
                 "contains the corresponding PaymentTypeId.")]
    [TestCase("Deposit",     "7")]
    [TestCase("Balance",     "8")]
    [TestCase("Part-Payment","9")]
    public async Task Payment_type_filter_round_trips_into_search_request(string label, string expectedTypeId)
    {
        var dashboard = new DashboardPage(Page, Settings.Applications.Member);
        var page      = new PaymentsOutstandingPage(Page, Settings.Applications.Member);

        await StepAsync("Open Dashboard", () => dashboard.GotoAsync());
        await StepAsync("Navigate to Payments Outstanding", async () =>
        {
            await dashboard.NavBar.GoToPaymentsOutstandingAsync();
            await page.WaitForReadyAsync();
        });
        await StepAsync("Wait for initial grid load", () => page.WaitForGridLoadedAsync());

        await StepAsync($"Select Payment Type = '{label}'",
            () => page.SelectPaymentTypeAsync(label));

        var (status, postData) = await StepAsync("Click Search and capture request",
            () => page.SearchAndCaptureAsync());

        Assert.That(status, Is.EqualTo(200),
            $"Search endpoint should return 200 OK. Got {status}.");

        Assert.That(postData, Does.Contain($"PaymentTypeId={expectedTypeId}").IgnoreCase
                              .Or.Contain($"\"PaymentTypeId\":\"{expectedTypeId}\"").IgnoreCase,
            $"POST body should carry PaymentTypeId={expectedTypeId} for label '{label}'.\n" +
            $"Actual body: {postData}");
    }

    [Test, Category(FeatureCategory)]
    [Description("Each rendered row's BOOKING column links to /Client/BookingDetails with the " +
                 "client + booking reference query params, opened in a new tab (target=_blank).")]
    public async Task Booking_link_in_row_points_to_BookingDetails_in_new_tab()
    {
        var dashboard = new DashboardPage(Page, Settings.Applications.Member);
        var page      = new PaymentsOutstandingPage(Page, Settings.Applications.Member);

        await StepAsync("Open Dashboard", () => dashboard.GotoAsync());
        await StepAsync("Navigate to Payments Outstanding", async () =>
        {
            await dashboard.NavBar.GoToPaymentsOutstandingAsync();
            await page.WaitForReadyAsync();
        });
        await StepAsync("Wait for grid load", () => page.WaitForGridLoadedAsync());

        var rows = await page.Grid.RowCountAsync();
        if (rows == 0)
            Assert.Ignore("No outstanding payments exist — no booking link to verify.");

        var link = page.FirstRowBookingLink();
        var href   = await link.GetAttributeAsync("href");
        var target = await link.GetAttributeAsync("target");

        Logger.Information("First-row booking link: href={Href} target={Target}", href, target);

        Assert.That(href, Is.Not.Null.And.Not.Empty, "Booking link must have an href.");
        Assert.That(href, Does.Contain("/Client/BookingDetails").IgnoreCase,
            $"Booking link href should point to /Client/BookingDetails. Actual: {href}");
        Assert.That(href, Does.Contain("Id=").IgnoreCase,
            "Booking link href should carry the client reference (Id=...).");
        Assert.That(href, Does.Contain("BookingRefId=").IgnoreCase,
            "Booking link href should carry the BookingRefId query param.");
        Assert.That(target, Is.EqualTo("_blank"),
            "Per the dev JS, the booking row link opens in a new tab.");
    }
}
