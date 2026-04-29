using PTS.Automation.Pages.Admin.Shell;

namespace PTS.Automation.Pages.Admin.Itinerary;

/// <summary>Published client itinerary (<c>Itinerary/ClientBookingsItinerary</c>).</summary>
public sealed class AdminClientItineraryPage : AdminPage
{
    public AdminClientItineraryPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath =>
        throw new InvalidOperationException("Use GotoWithClientAndBookingAsync.");

    protected override ILocator ReadinessIndicator =>
        Page.GetByText("Payment Schedule", new() { Exact = false }).First;

    public async Task GotoWithClientAndBookingAsync(string clientNewId, string bookingRefId)
    {
        var path =
            $"{AdminRoutes.ClientBookingsItineraryPathPrefix}/{Uri.EscapeDataString(clientNewId)}?BookingRefId={Uri.EscapeDataString(bookingRefId)}";
        await Page.GotoAsync(new Uri(App.BaseUri, path).ToString(),
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await WaitForReadyAsync();
    }

    public async Task WaitForPaymentScheduleRowsAsync(int minimumRows = 1)
    {
        await Page.WaitForFunctionAsync(
            "min => document.querySelectorAll(\"#tblMoneyDue tr\").length >= min",
            minimumRows,
            new PageWaitForFunctionOptions { Timeout = Settings.Timeouts.NavigationMs });
    }

    public Task<int> PaymentScheduleRowCountAsync() => Page.Locator("#tblMoneyDue tr").CountAsync();

    public async Task<int> PaymentScheduleStatusColumnIndexAsync()
    {
        var headers = await Page.Locator("table:has(#tblMoneyDue) thead th").AllInnerTextsAsync();
        for (var i = 0; i < headers.Count; i++)
        {
            if (headers[i].Contains("status", StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return -1;
    }

    /// <summary>First payment row due date cell (column index 2 per <c>QuoteItinerary.js</c>).</summary>
    public ILocator FirstPaymentDueDateCell =>
        Page.Locator("#tblMoneyDue tr").First.Locator("td").Nth(2);

    public ILocator TotalPaidValue => Page.Locator("#spmgrossmoney");

    public ILocator StillToPayValue => Page.Locator("#spmTotalClientStillPay");

    public Task<bool> IsTotalPaidSectionVisibleAsync() =>
        Page.Locator(".profile_detail.paidTotal").GetByText("total paid", new() { Exact = false }).IsVisibleAsync();

    public Task<bool> IsStillToPaySectionVisibleAsync() =>
        Page.Locator(".profile_detail.paidTotal").GetByText("still to pay", new() { Exact = false }).IsVisibleAsync();
}
