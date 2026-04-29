using Microsoft.Playwright;
using PTS.Automation.Pages.Member.Shell;

namespace PTS.Automation.Pages.Member.Clients;

/// <summary>
/// Member → Booking management / itinerary (<c>Client/ClientBookingDetails</c>).
/// View: <c>Views/Client/ClientBookingDetails.cshtml</c>.
/// </summary>
public sealed class ClientBookingDetailsManagementPage : MemberPage
{
    public ClientBookingDetailsManagementPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => MemberRoutes.ClientBookingDetailsManagement;

    public override Task GotoAsync() =>
        throw new InvalidOperationException("Use GotoWithBookingAsync(clientNewId, bookingRefId).");

    public async Task GotoWithBookingAsync(string clientNewId, string bookingRefId)
    {
        var path =
            $"{MemberRoutes.ClientBookingDetailsManagement}?id={Uri.EscapeDataString(clientNewId)}&BookingRefId={Uri.EscapeDataString(bookingRefId)}";
        await Page.GotoAsync(new Uri(App.BaseUri, path).ToString(),
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await WaitForReadyAsync();
    }

    protected override ILocator ReadinessIndicator =>
        Page.Locator("#headerBooking");

    public Task<bool> IsItineraryNavLinkVisibleAsync() =>
        Page.Locator("#headerItinerary").IsVisibleAsync();

    public async Task<string?> ItineraryHrefAsync() =>
        await Page.Locator("#headerItinerary").GetAttributeAsync("href");

    public Task<int> ItineraryPreviewLinkCountAsync() =>
        Page.Locator("a.dropdown-item[href*='ClientBookingsItinerary']").CountAsync();

    /// <summary>Activity rows with an external supplier pending internal conversion.</summary>
    public Task<int> ActivityConvertToInternalBadgeCountAsync() =>
        Page.Locator("div.accomTitleText").Filter(new LocatorFilterOptions
        {
            Has = Page.GetByText("Activities", new() { Exact = true })
        }).GetByText("CONVERT TO INTERNAL SUPPLIER", new() { Exact = true }).CountAsync();

    public Task<int> TransportConvertToInternalBadgeCountAsync() =>
        Page.Locator("div.transport_div").GetByText("CONVERT TO INTERNAL SUPPLIER", new() { Exact = true }).CountAsync();

    public Task<int> PackageConvertToInternalBadgeCountAsync() =>
        Page.Locator("div.package_div").GetByText("CONVERT TO INTERNAL SUPPLIER", new() { Exact = true }).CountAsync();
}
