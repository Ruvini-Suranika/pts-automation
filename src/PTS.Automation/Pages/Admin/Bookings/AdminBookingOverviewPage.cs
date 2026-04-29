using Microsoft.Playwright;
using PTS.Automation.Pages.Admin.Shell;

namespace PTS.Automation.Pages.Admin.Bookings;

/// <summary>Admin → Booking overview (<c>Admin/BookingDetails</c>). View: <c>Views/Admin/BookingDetails.cshtml</c>.</summary>
public sealed class AdminBookingOverviewPage : AdminPage
{
    public AdminBookingOverviewPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => AdminRoutes.BookingDetails;

    public override Task GotoAsync() =>
        throw new InvalidOperationException("Use GotoWithBookingAsync(clientNewId, bookingRefId).");

    protected override ILocator ReadinessIndicator => Page.Locator("#headerOverview");

    public async Task GotoWithBookingAsync(string clientNewId, string bookingRefId)
    {
        var path =
            $"{AdminRoutes.BookingDetails}?Id={Uri.EscapeDataString(clientNewId)}&bookingReferenceId={Uri.EscapeDataString(bookingRefId)}";
        await Page.GotoAsync(new Uri(App.BaseUri, path).ToString(),
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await WaitForReadyAsync();
    }

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

    public Task<bool> IsPaymentOutSummarySectionVisibleAsync() =>
        Page.GetByText("Payment out summary", new() { Exact = true }).IsVisibleAsync();

    /// <summary>Controls → Booking access (itinerary lock). Switch <c>#ChkBookingaccess</c>.</summary>
    public ILocator BookingAccessSwitch =>
        Page.GetByRole(AriaRole.Switch, new() { Name = "Booking access" });

    public Task<bool> IsControlsSectionVisibleAsync() =>
        Page.Locator("section.controlsForBooking").GetByText("Controls", new() { Exact = true }).IsVisibleAsync();

    /// <summary>Eye icon on payment-out rows opens log modal (<c>ViewSummarLog</c> in admin client-bookingdetails.js).</summary>
    public ILocator PaymentOutLogTriggers =>
        Page.Locator("#tbl_paymentOutSummary span.trust_account[onclick*='ViewSummarLog']");

    public Task ClickFirstPaymentOutLogAsync() =>
        PaymentOutLogTriggers.First.ClickAsync();

    public Task WaitForLogNotesModalAsync() =>
        Page.Locator("#pts-view-log-modal.show").WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = Settings.Timeouts.DefaultMs
        });

    public Task<bool> IsLogNotesModalVisibleAsync() =>
        Page.Locator("#pts-view-log-modal.show").IsVisibleAsync();

    public Task<string> TrusteeAuthorisationLogNoteTextAsync() =>
        Page.Locator("#pts-view-log-modal.show #TrusteeAuthorisationLogNote").InnerTextAsync();

    public Task<string> CurrencyPurchasingLogNoteTextAsync() =>
        Page.Locator("#pts-view-log-modal.show #CurrencyPurchasingLogNote").InnerTextAsync();

    public Task PressEscapeAsync() => Page.Keyboard.PressAsync("Escape");

    public Task<bool> IsFinancialsSectionVisibleAsync() =>
        Page.Locator("div.col-lg-4").GetByText("Financials", new() { Exact = true }).First.IsVisibleAsync();

    public ILocator TotalBookingFeesLink => Page.Locator("#spmtotalbookingfee");

    /// <summary>Edit payment-out row (pen) when enabled — excludes <c>opacity: 0.5</c> disabled controls.</summary>
    public ILocator EnabledPaymentOutEditLinks =>
        Page.Locator("#tbl_paymentOutSummary a[onclick*='BindEditPopUpForPaySupplier']:not([style*='0.5'])");

    public Task ClickFirstEnabledPaymentOutEditAsync() => EnabledPaymentOutEditLinks.First.ClickAsync();

    public Task<bool> IsEditPaySupplierModalVisibleAsync() =>
        Page.Locator("#editpaySupplierModal.show").IsVisibleAsync();

    public Task CloseEditPaySupplierModalAsync() =>
        Page.Locator("#editpaySupplierModal .btn-close").First.ClickAsync();
}

