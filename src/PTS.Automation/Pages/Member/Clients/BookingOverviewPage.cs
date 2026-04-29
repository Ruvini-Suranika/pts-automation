using Microsoft.Playwright;
using PTS.Automation.Pages.Member.Shell;

namespace PTS.Automation.Pages.Member.Clients;

/// <summary>
/// Member → Booking overview / summary (<c>Client/BookingDetails</c>).
/// View: <c>Views/Client/BookingDetails.cshtml</c>. Sub-nav: <c>_clientSubMenu.cshtml</c>.
/// </summary>
public sealed class BookingOverviewPage : MemberPage
{
    public BookingOverviewPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => MemberRoutes.BookingDetailsOverview;

    /// <summary>Use <see cref="GotoWithBookingAsync"/> — query parameters are required.</summary>
    public override Task GotoAsync() =>
        throw new InvalidOperationException("Use GotoWithBookingAsync(clientNewId, bookingRefId).");

    public async Task GotoWithBookingAsync(string clientNewId, string bookingRefId)
    {
        var path = $"{MemberRoutes.BookingDetailsOverview}?Id={Uri.EscapeDataString(clientNewId)}&BookingRefId={Uri.EscapeDataString(bookingRefId)}";
        await Page.GotoAsync(new Uri(App.BaseUri, path).ToString(),
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await WaitForReadyAsync();
    }

    /// <summary>Sub-menu is always rendered on this view; hidden inputs are not Playwright-visible.</summary>
    protected override ILocator ReadinessIndicator =>
        Page.Locator("#headerOverview");

    private ILocator BookingSummarySection => Page.Locator("section.booking_summary_section");

    public Task<bool> IsQuickNoteSectionVisibleAsync() =>
        Page.GetByRole(AriaRole.Heading, new() { Name = "Quick Note" }).IsVisibleAsync();

    public Task OpenEditQuickNoteModalAsync() =>
        Page.Locator("button[data-bs-target=\"#EditQuickNote\"]").First.ClickAsync();

    public Task<bool> IsEditQuickNoteModalVisibleAsync() =>
        Page.Locator("#EditQuickNote.show").IsVisibleAsync();

    public Task<string> QuickNoteDisplayedTextAsync() =>
        Page.Locator("#spanQuickNote").InnerTextAsync();

    public ILocator EditQuickNoteTextArea => Page.Locator("#txtEditQuickNote");

    public Task CloseEditQuickNoteModalAsync() =>
        Page.Locator("#EditQuickNote .btn-close").First.ClickAsync();

    /// <summary>Confirmation toggle in booking summary table (per supplier row).</summary>
    public ILocator FirstBookingSummaryConfirmationControl() =>
        BookingSummarySection.Locator("tbody#tblBookingSummary a[onclick*='ChangeStatusBookingSummary']").First;

    public Task<int> BookingSummaryDataRowCountAsync() =>
        Page.Locator("tbody#tblBookingSummary tr").CountAsync();

    public async Task WaitForBookingSummaryRowsAsync(int minimumRows = 1)
    {
        await Page.WaitForFunctionAsync(
            "min => document.querySelectorAll(\"tbody#tblBookingSummary tr\").length >= min",
            minimumRows,
            new PageWaitForFunctionOptions { Timeout = Settings.Timeouts.NavigationMs });
    }

    public Task<string?> BookingReferenceHiddenValueAsync() =>
        Page.Locator("#BookingRefId").GetAttributeAsync("value");

    public Task<bool> IsBookingSummarySectionVisibleAsync() =>
        Page.GetByRole(AriaRole.Heading, new() { Name = "Booking summary" }).IsVisibleAsync();

    public Task<bool> IsSubNavOverviewActiveOrPresentAsync() =>
        Page.Locator("#headerOverview").IsVisibleAsync();

    public Task<bool> IsSubNavBookingTabVisibleAsync() =>
        Page.Locator("#headerBooking").IsVisibleAsync();

    public Task ClickBookingManagementTabAsync() =>
        Page.Locator("#headerBooking").ClickAsync();

    public async Task<string?> ItineraryHrefAsync() =>
        await Page.Locator("#headerItinerary").GetAttributeAsync("href");

    /// <summary>Payment-out summary rows built by <c>BindPaymentOutSummaryData</c> in <c>client-bookingdetails.js</c>.</summary>
    public ILocator PaymentOutSummaryDataRows =>
        Page.Locator("#tbl_paymentOutSummary tr[id^='payment-out-']");

    public Task<int> PaymentOutSummaryDataRowCountAsync() => PaymentOutSummaryDataRows.CountAsync();

    public async Task WaitForPaymentOutSummaryRowsAsync(int minimumRows = 1)
    {
        await Page.WaitForFunctionAsync(
            "min => document.querySelectorAll(\"#tbl_paymentOutSummary tr[id^='payment-out-']\").length >= min",
            minimumRows,
            new PageWaitForFunctionOptions { Timeout = Settings.Timeouts.NavigationMs });
    }

    /// <summary>FX column is the 11th cell (0-based index 10): <c>1:{FixedRate}</c>.</summary>
    public Task<string> FirstPaymentOutFxRateCellTextAsync() =>
        PaymentOutSummaryDataRows.First.Locator("td").Nth(10).InnerTextAsync();
}
