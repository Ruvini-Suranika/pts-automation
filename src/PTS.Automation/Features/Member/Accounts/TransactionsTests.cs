using PTS.Automation.Infrastructure;
using PTS.Automation.Pages.Member.Accounts;

namespace PTS.Automation.Features.Member.Accounts;

/// <summary>
/// Coverage for Member → Accounts → Transactions (the <c>/Client/Transactions</c>
/// page — note this one lives under <c>ClientController</c>, not
/// <c>TravelMemberAccountController</c>).
///
/// Maps to the QA backlog items:
///   1. Validate Transactions record            (page loads + grid chrome)
///   2. Validate the three dots options         (copy / print / pdf / excel)
///   3. Refine functionality                    (inline client-side filter)
///   4. Booking Reference link function         (row link points at booking)
///   5. Customer Reference link function        (row link points at client)
///
/// Data-dependent tests (3, 4, 5) call Assert.Ignore with a clear reason
/// if the QA test member has no transactions to render, rather than failing.
/// </summary>
[TestFixture]
[Category(Categories.Regression)]
[Category(Categories.Member)]
public class TransactionsTests : MemberTest
{
    private const string FeatureCategory = "Accounts.Transactions";

    // QA backlog item 1 ------------------------------------------------------
    [Test, Category(FeatureCategory)]
    [Description("Transactions page loads with filter panel, inline grid search, "
                 + "three-dots menu toggle, and the documented column headers.")]
    public async Task Page_loads_with_filter_panel_and_grid_chrome()
    {
        var page = new TransactionsPage(Page, Settings.Applications.Member);
        await StepAsync("Open Transactions", () => page.GotoAsync());

        Assert.That(await page.IsFilterPanelVisibleAsync(), Is.True,
            "Filter panel (#advanceFilter) must be visible.");
        Assert.That(await page.IsSearchButtonVisibleAsync(), Is.True,
            "Search (pts-button[type='search']) must be visible in the filter panel.");
        Assert.That(await page.IsGridSearchVisibleAsync(), Is.True,
            "Inline grid Search (#search) must be visible.");
        Assert.That(await page.IsThreeDotsToggleVisibleAsync(), Is.True,
            "Three-dots menu toggle (#filterCorner) must be visible.");

        var headers = await page.GetColumnHeadersAsync();
        Logger.Information("Headers: {Headers}", string.Join(" | ", headers));

        // Assert the columns the QA backlog cares about specifically.
        foreach (var expected in new[] { "Booking Ref", "Customer Ref", "Type",
                                          "Currency", "Gross Credit", "Debit Amount" })
        {
            Assert.That(await page.ColumnIndexOfAsync(expected), Is.GreaterThan(0),
                $"Expected a '{expected}' column. Got: {string.Join(" | ", headers)}");
        }
    }

    // QA backlog item 2 ------------------------------------------------------
    [Test, Category(FeatureCategory)]
    [Description("The three-dots (#filterCorner) dropdown exposes the four "
                 + "documented actions: Copy to clipboard, Print, Download PDF, Download Excel. "
                 + "PDF and Excel links point at the expected export endpoints.")]
    public async Task Three_dots_menu_exposes_copy_print_pdf_excel()
    {
        var page = new TransactionsPage(Page, Settings.Applications.Member);
        await StepAsync("Open Transactions", () => page.GotoAsync());

        await StepAsync("Open three-dots menu", () => page.OpenThreeDotsMenuAsync());

        // Bootstrap toggles the 'show' class on click — give it a frame to paint.
        await Page.WaitForTimeoutAsync(200);

        Assert.That(await page.IsCopyToClipboardVisibleAsync(), Is.True,
            "'Copy to clipboard' option should be visible in the three-dots menu.");
        Assert.That(await page.IsPrintVisibleAsync(), Is.True,
            "'Print' option should be visible in the three-dots menu.");
        Assert.That(await page.IsDownloadPdfVisibleAsync(), Is.True,
            "'Download PDF' option (#TransactionToPdf) should be visible.");
        Assert.That(await page.IsDownloadExcelVisibleAsync(), Is.True,
            "'Download Excel' option (#TransactionToExcel) should be visible.");

        // Before Search runs, the hrefs may be empty or pointing at default params;
        // after Search runs they get rewritten by loadData() to include all filter values.
        await StepAsync("Fire a broad search so the PDF / Excel hrefs get populated", async () =>
        {
            await page.SelectBroadPeriodAsync();
            await page.SearchAndCaptureAsync();
        });

        var pdfHref   = await page.GetDownloadPdfHrefAsync();
        var excelHref = await page.GetDownloadExcelHrefAsync();
        Logger.Information("Export links -- PDF: {Pdf}  |  Excel: {Excel}", pdfHref, excelHref);

        Assert.That(pdfHref,   Does.Contain("/Common/ExportTransactionToPdf").IgnoreCase,
            $"Download PDF href should point to /Common/ExportTransactionToPdf. Actual: {pdfHref}");
        Assert.That(excelHref, Does.Contain("/Common/ExportTransactionToExcel").IgnoreCase,
            $"Download Excel href should point to /Common/ExportTransactionToExcel. Actual: {excelHref}");
    }

    // QA backlog item 3 ------------------------------------------------------
    [Test, Category(FeatureCategory)]
    [Description("Refine search: the inline #search input hides non-matching rows "
                 + "client-side on keyup (no AJAX).")]
    public async Task Refine_search_filters_rows_client_side()
    {
        var page = new TransactionsPage(Page, Settings.Applications.Member);
        await StepAsync("Open Transactions", () => page.GotoAsync());
        await StepAsync("Fire a broad search to populate rows", async () =>
        {
            await page.SelectBroadPeriodAsync();
            await page.SearchAndCaptureAsync();
        });

        var rowsBefore = await page.Grid.RowCountAsync();
        if (rowsBefore == 0)
            Assert.Ignore("No transactions returned for this member / period — refine-search has nothing to filter.");

        const string sentinel = "ZZZ_PTS_AUTOMATION_NO_MATCH_ZZZ";
        await StepAsync($"Type sentinel '{sentinel}' into the grid #search box",
            () => page.SetGridSearchAsync(sentinel));

        // The dev JS loops over every non-header <tr> and hides non-matches on keyup.
        await Page.WaitForTimeoutAsync(300);

        // NOTE on selector: the dev JS uses tr:gt(0) so it skips the header row,
        // but we scope :visible into the body <tbody#transactionsresult> to avoid
        // counting the header — same descendant-vs-filter trap as A1/A2.
        var visibleAfter = await Page.Locator("#transactionsresult tr:visible").CountAsync();
        Assert.That(visibleAfter, Is.EqualTo(0),
            $"After applying a non-matching sentinel filter all rows should be hidden. "
            + $"Rows before: {rowsBefore}, visible after: {visibleAfter}");
    }

    // QA backlog item 4 ------------------------------------------------------
    [Test, Category(FeatureCategory)]
    [Description("The first row's Booking Ref column is a link that points at the "
                 + "booking-details / booking-overview page.")]
    public async Task Booking_reference_link_points_to_booking_details()
    {
        var page = new TransactionsPage(Page, Settings.Applications.Member);
        await StepAsync("Open Transactions", () => page.GotoAsync());
        await StepAsync("Fire a broad search to populate rows", async () =>
        {
            await page.SelectBroadPeriodAsync();
            await page.SearchAndCaptureAsync();
        });

        var rows = await page.Grid.RowCountAsync();
        if (rows == 0)
            Assert.Ignore("No transactions — no booking-ref link to verify.");

        var link = await page.FirstRowBookingLinkAsync();

        // Dev template renders either a direct <a href='/Client/BookingDetails?...'>
        // or a <a onclick='navigateToBooking(...)'> — guard for both.
        var href    = await link.GetAttributeAsync("href");
        var onclick = await link.GetAttributeAsync("onclick");
        Logger.Information("First-row BookingRef -- href={Href} onclick={Onclick}", href, onclick);

        var combined = $"{href ?? ""} {onclick ?? ""}";
        Assert.That(combined.Length, Is.GreaterThan(0),
            "Booking Ref cell's anchor must declare a navigation (href or onclick).");
        Assert.That(combined,
            Does.Contain("BookingDetails").IgnoreCase
               .Or.Contain("ClientBookingDetails").IgnoreCase
               .Or.Contain("BookingOverview").IgnoreCase,
            $"Booking Ref link should navigate to a booking-detail / booking-overview page. "
            + $"Actual: href={href}, onclick={onclick}");
    }

    // QA backlog item 5 ------------------------------------------------------
    [Test, Category(FeatureCategory)]
    [Description("The first row's Customer Ref column is a link that points at the "
                 + "client-details / client-overview page.")]
    public async Task Customer_reference_link_points_to_client_details()
    {
        var page = new TransactionsPage(Page, Settings.Applications.Member);
        await StepAsync("Open Transactions", () => page.GotoAsync());
        await StepAsync("Fire a broad search to populate rows", async () =>
        {
            await page.SelectBroadPeriodAsync();
            await page.SearchAndCaptureAsync();
        });

        var rows = await page.Grid.RowCountAsync();
        if (rows == 0)
            Assert.Ignore("No transactions — no customer-ref link to verify.");

        var link = await page.FirstRowCustomerLinkAsync();

        var href    = await link.GetAttributeAsync("href");
        var onclick = await link.GetAttributeAsync("onclick");
        Logger.Information("First-row CustomerRef -- href={Href} onclick={Onclick}", href, onclick);

        var combined = $"{href ?? ""} {onclick ?? ""}";
        Assert.That(combined.Length, Is.GreaterThan(0),
            "Customer Ref cell's anchor must declare a navigation (href or onclick).");
        Assert.That(combined,
            Does.Contain("ClientDetails").IgnoreCase
               .Or.Contain("ClientOverview").IgnoreCase
               .Or.Contain("ClientBookingDetails").IgnoreCase,
            $"Customer Ref link should navigate to a client-detail / client-overview page. "
            + $"Actual: href={href}, onclick={onclick}");
    }

    // Bonus: validates the filter contract without needing any data --------
    [Test, Category(FeatureCategory)]
    [Description("Search GET to /Client/TransactionsResults carries the Additional Narrative "
                 + "value in the query string when the user enters it (round-trip filter check).")]
    public async Task Additional_narrative_filter_round_trips_into_search_request()
    {
        var page = new TransactionsPage(Page, Settings.Applications.Member);
        await StepAsync("Open Transactions", () => page.GotoAsync());

        // Sentinel — unlikely to match any real AdditionalNarrative, so the response will
        // be an empty grid but the request itself still fires (and that's what we assert).
        const string sentinel = "PTS_AUTOMATION_SENTINEL_NARRATIVE_ZZZ";
        await StepAsync($"Set Additional Narrative to '{sentinel}'",
            () => Page.Locator("#AdditionalNarrative").FillAsync(sentinel));

        var (status, requestUrl) = await StepAsync("Click Search and capture request",
            () => page.SearchAndCaptureAsync());

        Assert.That(status, Is.EqualTo(200),
            $"Search endpoint should return 200 OK. Got {status}.");

        // GET query string is URL-encoded.
        Assert.That(requestUrl, Does.Contain(Uri.EscapeDataString(sentinel)).IgnoreCase
                                   .Or.Contain(sentinel).IgnoreCase,
            $"Request URL should carry the AdditionalNarrative value.\nActual URL: {requestUrl}");
    }
}
