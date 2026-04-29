using Allure.NUnit.Attributes;
using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Member.Accounts;

namespace PTS.Automation.Features.Member.Accounts;

/// <summary>
/// Coverage for Member → Accounts → Payment Due (the page hidden in nav,
/// reachable via direct URL or via the "Outstanding" → "Due" toggle).
///
/// Maps to the QA backlog items:
///   1. User is able to select the tick boxes
///   2. User is able to send reminder
///   3. Branch Store column visible in Payments Due screen
///   4. Refine Search function
///   5. Refine search by Payment Type
///   6. Booking link in row
///
/// Data-dependent tests skip with a clear reason if the QA test member has
/// no payments due rather than failing.
/// </summary>
[TestFixture]
[AllureSuite(Categories.Member)]
[AllureFeature("Payment due")]
[AllureTag(Categories.Regression)]
[AllureTag(Categories.Member)]
[AllureTag("Accounts.PaymentDue")]
[Category(Categories.Regression)]
[Category(Categories.Member)]
public class PaymentDueTests : MemberTest
{
    private const string FeatureCategory = "Accounts.PaymentDue";

    [Test, Category(FeatureCategory), Category(Categories.UI)]
    [Description("BRANCH STORE column is present in the Payment Due grid header.")]
    public async Task Branch_store_column_is_present_in_grid_header()
    {
        var page = new PaymentDuePage(Page, Settings.Applications.Member);
        await StepAsync("Open Payment Due", () => page.GotoAsync());

        var headers = await StepAsync("Read column headers",
            () => page.GetColumnHeadersAsync());

        await StepAsync("Log column headers", async () =>
        {
            Logger.Information("Headers: {Headers}", string.Join(" | ", headers));
        });

        await StepAsync("Verify BRANCH STORE column is present", async () =>
        {
            Assert.That(await page.ColumnIndexOfAsync("BRANCH STORE"), Is.GreaterThan(0),
                $"Expected a BRANCH STORE column. Got: {string.Join(" | ", headers)}");
        });
    }

    [Test, Category(FeatureCategory), Category(Categories.UI)]
    [Description("Tick boxes: clicking the header check-all selects every row checkbox; " +
                 "clicking it again clears them.")]
    public async Task Tick_boxes_can_be_selected_and_cleared()
    {
        var page = new PaymentDuePage(Page, Settings.Applications.Member);
        await StepAsync("Open Payment Due", () => page.GotoAsync());
        await StepAsync("Wait for initial grid load", () => page.WaitForGridLoadedAsync());

        var rowBoxes = await page.RowCheckboxCountAsync();
        if (rowBoxes == 0)
            Assert.Ignore("No payment-due rows for this test member — no tickboxes to select.");

        // We don't assert IsVisible on the native #checkAll input — the dev's
        // custom checkbox styling masks it (opacity:0 / height:0 inside a
        // .checkboxContainer label with a .checkmark span). The functional
        // check is whether clicking it toggles the row checkboxes, which is
        // what the rest of the test asserts.

        await StepAsync("Click 'select all' header checkbox", () => page.ClickCheckAllAsync());
        await StepAsync("Brief settle wait after select-all", () => Page.WaitForTimeoutAsync(200));

        var checkedAfterSelect = await StepAsync("Count checked row checkboxes after select-all",
            () => page.CheckedRowCountAsync());

        await StepAsync("Log checkbox selection state", async () =>
        {
            Logger.Information("After select-all: {Checked}/{Total} row checkboxes ticked",
                checkedAfterSelect, rowBoxes);
        });

        await StepAsync("Verify all row checkboxes are ticked", async () =>
        {
            Assert.That(checkedAfterSelect, Is.EqualTo(rowBoxes),
                "After clicking select-all, every visible row checkbox should be ticked.");
        });

        await StepAsync("Click 'select all' again to clear", () => page.ClickCheckAllAsync());
        await StepAsync("Brief settle wait after clear", () => Page.WaitForTimeoutAsync(200));

        await StepAsync("Verify all row checkboxes are cleared", async () =>
        {
            Assert.That(await page.CheckedRowCountAsync(), Is.EqualTo(0),
                "After clicking select-all a second time, all row checkboxes should be cleared.");
        });
    }

    [Test, Category(FeatureCategory), Category(Categories.UI)]
    [Description("Send Reminder is hidden until the initial AJAX completes; clicking it " +
                 "with no row selected does NOT open the email compose modal.")]
    public async Task Send_reminder_with_no_selection_does_not_open_email_modal()
    {
        var page = new PaymentDuePage(Page, Settings.Applications.Member);
        await StepAsync("Open Payment Due", () => page.GotoAsync());

        // Initial load: Send Reminder is hidden by JS until loadData() returns.
        await StepAsync("Wait for grid load", () => page.WaitForGridLoadedAsync());

        await StepAsync("Verify Send Reminder button is visible", async () =>
        {
            Assert.That(await page.IsSendReminderVisibleAsync(), Is.True,
                "Send Reminder button should become visible after the initial AJAX completes " +
                "(per PaymentDue.js: $('#OpenEmailModal').show() in the success handler).");
        });

        await StepAsync("Click Send Reminder without selecting any row",
            () => page.ClickSendReminderAsync());

        await StepAsync("Brief wait after Send Reminder click", () => Page.WaitForTimeoutAsync(400));

        await StepAsync("Verify email compose modal does not open", async () =>
        {
            Assert.That(await page.IsEmailComposeModalVisibleAsync(), Is.False,
                "Email compose modal (#addemailtemplate) must NOT open without a row selection. " +
                "Per the dev JS, an inline error is shown instead.");
        });
    }

    [Test, Category(FeatureCategory), Category(Categories.UI)]
    [Description("Refine search: typing a non-matching string in #searchDue hides every row " +
                 "(client-side filter, no AJAX).")]
    public async Task Refine_search_filters_rows_client_side()
    {
        var page = new PaymentDuePage(Page, Settings.Applications.Member);
        await StepAsync("Open Payment Due", () => page.GotoAsync());
        await StepAsync("Wait for grid load", () => page.WaitForGridLoadedAsync());

        var rowsBefore = await page.Grid.RowCountAsync();
        if (rowsBefore == 0)
            Assert.Ignore("No payment-due rows — refine-search has nothing to filter.");

        const string sentinel = "ZZZ_PTS_AUTOMATION_NO_MATCH_ZZZ";
        await StepAsync($"Type sentinel '{sentinel}' into #searchDue",
            () => page.SetGridSearchAsync(sentinel));

        await StepAsync("Brief wait after refine search", () => Page.WaitForTimeoutAsync(300));

        var visibleAfter = await StepAsync("Count visible Payment Due rows after refine",
            () => Page.Locator("#PaymentDue tr:visible").CountAsync());

        await StepAsync("Verify refine hides all rows", async () =>
        {
            Assert.That(visibleAfter, Is.EqualTo(0),
                $"After applying a non-matching filter all rows should be hidden. " +
                $"Rows before: {rowsBefore}, visible after: {visibleAfter}");
        });
    }

    [Test, Category(FeatureCategory), Category(Categories.Hybrid)]
    [Description("Payment Type filter: selecting Deposit / Balance / Part-Payment then " +
                 "clicking Search fires a POST to /TravelMemberAccount/GetSearchPaymentDue " +
                 "with the matching PaymentTypeId in the body.")]
    [TestCase("Deposit",      "7")]
    [TestCase("Balance",      "8")]
    [TestCase("Part-Payment", "9")]
    public async Task Payment_type_filter_round_trips_into_search_request(string label, string expectedTypeId)
    {
        var page = new PaymentDuePage(Page, Settings.Applications.Member);
        await StepAsync("Open Payment Due", () => page.GotoAsync());
        await StepAsync("Wait for initial grid load", () => page.WaitForGridLoadedAsync());

        await StepAsync($"Select Payment Type = '{label}'",
            () => page.SelectPaymentTypeAsync(label));

        var (status, postData) = await StepAsync("Click Search and capture request",
            () => page.SearchAndCaptureAsync());

        await StepAsync("Verify search response status", async () =>
        {
            Assert.That(status, Is.EqualTo(200),
                $"Search endpoint should return 200 OK. Got {status}.");
        });

        await StepAsync("Verify PaymentTypeId in search POST body", async () =>
        {
            Assert.That(postData, Does.Contain($"PaymentTypeId={expectedTypeId}").IgnoreCase
                                  .Or.Contain($"\"PaymentTypeId\":\"{expectedTypeId}\"").IgnoreCase,
                $"POST body should carry PaymentTypeId={expectedTypeId} for label '{label}'.\n" +
                $"Actual body: {postData}");
        });
    }

    [Test, Category(FeatureCategory), Category(Categories.UI)]
    [Description("Each rendered row's BOOKING column links to /Client/BookingDetails with the " +
                 "client + booking reference query params.")]
    public async Task Booking_link_in_row_points_to_BookingDetails()
    {
        var page = new PaymentDuePage(Page, Settings.Applications.Member);
        await StepAsync("Open Payment Due", () => page.GotoAsync());
        await StepAsync("Wait for grid load", () => page.WaitForGridLoadedAsync());

        var rows = await page.Grid.RowCountAsync();
        if (rows == 0)
            Assert.Ignore("No payment-due rows — no booking link to verify.");

        var link = page.FirstRowBookingLink();
        var href = await StepAsync("Read first-row booking link href",
            () => link.GetAttributeAsync("href"));

        await StepAsync("Log booking link href", async () =>
        {
            Logger.Information("First-row booking link: href={Href}", href);
        });

        await StepAsync("Verify booking link href shape", async () =>
        {
            Assert.That(href, Is.Not.Null.And.Not.Empty, "Booking link must have an href.");
            Assert.That(href, Does.Contain("/Client/BookingDetails").IgnoreCase,
                $"Booking link href should point to /Client/BookingDetails. Actual: {href}");
            Assert.That(href, Does.Contain("Id=").IgnoreCase,
                "Booking link href should carry the client reference (Id=...).");
            Assert.That(href, Does.Contain("BookingRefId=").IgnoreCase,
                "Booking link href should carry the BookingRefId query param.");
        });
    }
}
