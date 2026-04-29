using PTS.Automation.Pages.Admin.Shell;

namespace PTS.Automation.Pages.Admin.Bookings;

/// <summary>Admin → Total booking fees (<c>Admin/BookingFee</c>). View: <c>Views/Admin/BookingFee.cshtml</c>.</summary>
public sealed class AdminBookingFeePage : AdminPage
{
    public AdminBookingFeePage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => AdminRoutes.BookingFee;

    public override Task GotoAsync() =>
        throw new InvalidOperationException("Use GotoWithClientNewIdAsync or GotoWithClientNewIdAndReturnResponseAsync.");

    protected override ILocator ReadinessIndicator =>
        Page.GetByText("Total booking fees", new() { Exact = false });

    public async Task GotoWithClientNewIdAsync(string clientNewId)
    {
        _ = await GotoWithClientNewIdAndReturnResponseAsync(clientNewId);
        await WaitForReadyAsync();
    }

    public async Task<IResponse?> GotoWithClientNewIdAndReturnResponseAsync(string clientNewId)
    {
        var path = $"{AdminRoutes.BookingFee}?Id={Uri.EscapeDataString(clientNewId)}";
        var url  = new Uri(App.BaseUri, path).ToString();
        var response = await Page.GotoAsync(url,
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await WaitForReadyAsync();
        return response;
    }

    public Task<bool> IsIncomingFeesSectionVisibleAsync() =>
        Page.GetByRole(AriaRole.Heading, new() { Name = "Incoming fees" }).IsVisibleAsync();

    /// <summary>Incoming/outgoing fee tables live under <c>section.totalBookingFees</c>.</summary>
    public ILocator IncomingFeesSection => Page.Locator("section.totalBookingFees").First;

    public Task<bool> IncomingFeesContainsTextAsync(string fragment) =>
        IncomingFeesSection.GetByText(fragment, new() { Exact = false }).First.IsVisibleAsync();

    /// <summary>Incoming fee total cell (4th column) for the first row whose type contains <paramref name="feeTypeFragment"/>.</summary>
    public async Task<string?> IncomingFeeTotalCellForRowContainingAsync(string feeTypeFragment)
    {
        var row = IncomingFeesSection.Locator("tbody tr").Filter(new() { HasText = feeTypeFragment }).First;
        if (!await row.IsVisibleAsync()) return null;
        return (await row.Locator("td").Nth(3).InnerTextAsync()).Trim();
    }

    /// <summary>First incoming row where type contains <paramref name="contains"/> and optionally excludes <paramref name="mustNotContain"/>.</summary>
    public async Task<string?> IncomingFeeTotalCellForRowMatchingAsync(string contains, string? mustNotContain = null)
    {
        var rows = IncomingFeesSection.Locator("tbody tr");
        var n = await rows.CountAsync();
        for (var i = 0; i < n; i++)
        {
            var row  = rows.Nth(i);
            var text = await row.InnerTextAsync();
            if (!text.Contains(contains, StringComparison.OrdinalIgnoreCase)) continue;
            if (!string.IsNullOrEmpty(mustNotContain)
                && text.Contains(mustNotContain, StringComparison.OrdinalIgnoreCase)) continue;
            return (await row.Locator("td").Nth(3).InnerTextAsync()).Trim();
        }

        return null;
    }
}
