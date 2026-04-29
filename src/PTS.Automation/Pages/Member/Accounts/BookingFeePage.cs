using Microsoft.Playwright;
using PTS.Automation.Pages.Member.Shell;

namespace PTS.Automation.Pages.Member.Accounts;

/// <summary>
/// Member → Client → Total booking fees (<c>Client/BookingFee</c>).
/// View: <c>Views/Client/BookingFee.cshtml</c> — APC and other fees are server-rendered.
/// </summary>
public sealed class BookingFeePage : MemberPage
{
    public BookingFeePage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => MemberRoutes.BookingFee;

    public override Task GotoAsync() =>
        throw new InvalidOperationException("Use GotoWithClientNewIdAsync or GotoWithClientNewIdAndReturnResponseAsync.");

    protected override ILocator ReadinessIndicator =>
        Page.GetByText("Total booking fees", new() { Exact = false });

    public async Task GotoWithClientNewIdAsync(string clientNewId)
    {
        _ = await GotoWithClientNewIdAndReturnResponseAsync(clientNewId);
        await WaitForReadyAsync();
    }

    /// <summary>Navigates to fees for a client; returns the document navigation response (Hybrid assertions).</summary>
    public async Task<IResponse?> GotoWithClientNewIdAndReturnResponseAsync(string clientNewId)
    {
        var path = $"{MemberRoutes.BookingFee}?Id={Uri.EscapeDataString(clientNewId)}";
        var url  = new Uri(App.BaseUri, path).ToString();
        var response = await Page.GotoAsync(url,
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await WaitForReadyAsync();
        return response;
    }

    public Task<bool> IsOutgoingFeesSectionVisibleAsync() =>
        Page.GetByText("Outgoing fees", new() { Exact = true }).IsVisibleAsync();

    public Task<int> ApcFeeRowCountAsync() =>
        Page.Locator("section.totalBookingFees").GetByText("APC Fee", new() { Exact = true }).CountAsync();
}
